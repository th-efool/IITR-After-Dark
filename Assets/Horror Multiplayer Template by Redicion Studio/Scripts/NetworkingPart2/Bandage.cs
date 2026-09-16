// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Bandage : GameplayItem
    {
        [Header("Bandage")]
        [SyncVar] public int amountOfHealthToRegenerate = 50;
        private int healthToRegenerate;
        private NetworkIdentity targetPlayerNetId;

        private Coroutine healingCoroutine;

        public float updateInterval = 0.1f;

        private void Start()
        {
            healthToRegenerate = amountOfHealthToRegenerate;

            StartCoroutine(c_Update());
        }

        private IEnumerator c_Update()
        {
            while (true)
            {
                M_Update();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void M_Update()
        {
            SphereCollider _sphereCollider = GetComponent<SphereCollider>();
            if (_sphereCollider != null)
            {
                _sphereCollider.enabled = IsOwned;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
                CharacterManager otherCharacterManager = other.GetComponent<CharacterManager>();
                if (otherCharacterManager != null && _myOwner != null && !_myOwner.GetComponent<CharacterManager>().isHealing && other.transform != _myOwner.transform)
                {
                    targetPlayerNetId = other.GetComponent<NetworkIdentity>();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.CompareTag("Player"))
            {
                CharacterManager otherCharacterManager = other.GetComponent<CharacterManager>();
                if (otherCharacterManager != null && _myOwner != null && !_myOwner.GetComponent<CharacterManager>().isHealing && other.transform == targetPlayerNetId.transform)
                {
                    targetPlayerNetId = null;
                }
            }
        }

        protected override void Server_Use()
        {
            if (targetPlayerNetId != null)
            {
                CharacterManager targetCharacterManager = targetPlayerNetId.GetComponent<CharacterManager>();
                if (targetCharacterManager.health < targetCharacterManager.maxHealth)
                {
                    TargetStartHealing(_myOwner.connectionToClient, targetPlayerNetId);
                }
            }
        }

        [TargetRpc]
        void TargetStartHealing(NetworkConnection target, NetworkIdentity _targetPlayerNetId)
        {
            if (_myOwner != null && _targetPlayerNetId != null && !_myOwner.GetComponent<CharacterManager>().isHealing)
            {
                healingCoroutine = StartCoroutine(HealOverTime(_myOwner.GetComponent<NetworkIdentity>(), _targetPlayerNetId));
            }
            else
            {
                Debug.Log("Healing coroutine not started due to missing conditions.");
            }
        }

        private IEnumerator HealOverTime(NetworkIdentity ownerPlayerNetId, NetworkIdentity targetPlayerNetId)
        {
            CmdSetHealingStatus(true);

            float healthMultiplier = _myOwner.GetComponent<CharacterManager>().consumableHealthMultiplier;
            int totalHealthToRegenerate = (int)(amountOfHealthToRegenerate * healthMultiplier);

            CmdPlayHealingAnimation(true);

            int healedHealth = 0;
            CharacterManager targetCharacterManager = targetPlayerNetId.GetComponent<CharacterManager>();
            Player targetPlayer = targetPlayerNetId.GetComponent<Player>();

            while (healedHealth < totalHealthToRegenerate && targetCharacterManager.health < targetCharacterManager.maxHealth)
            {
                if (_myOwner.GetComponent<HunterAbilities>()._inFight || targetCharacterManager.GetComponent<HunterAbilities>()._inFight || targetPlayerNetId == null)
                {
                    CmdSetHealingStatus(false);
                    ownerPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayer.username);
                    CmdUpdateHealingUI(false, ownerPlayerNetId, targetPlayerNetId);
                    healingCoroutine = null;
                    yield break;
                }

                if (!_input.use)
                {
                    CmdSetHealingStatus(false);
                    CmdPlayHealingAnimation(false);
                    ownerPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayer.username);
                    CmdUpdateHealingUI(false, ownerPlayerNetId, targetPlayerNetId);
                    healingCoroutine = null;
                    yield break;
                }

                int healthToHeal = Mathf.Min((int)(healthMultiplier), totalHealthToRegenerate - healedHealth);
                CmdHealPlayer(healthToHeal, targetPlayerNetId);
                healedHealth += healthToHeal;
                ownerPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(true, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayer.username);
                CmdUpdateHealingUI(true, ownerPlayerNetId, targetPlayerNetId);
                yield return new WaitForSeconds(0.5f);
            }

            if (amountOfHealthToRegenerate <= 0)
            {
                RpcCompleteHealing(_myOwner.GetComponent<NetworkIdentity>(), targetPlayerNetId);
                CmdCompleteHealing(_myOwner.GetComponent<NetworkIdentity>(), targetPlayerNetId);
            }

            CmdPlayHealingAnimation(false);
            CmdSetHealingStatus(false);
            ownerPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayer.username);
            CmdUpdateHealingUI(false, ownerPlayerNetId, targetPlayerNetId);
            healingCoroutine = null;
        }

        [Command]
        void CmdHealPlayer(int healthToHeal, NetworkIdentity playerToHeal)
        {
            if (healthToHeal > 0)
            {
                amountOfHealthToRegenerate -= healthToHeal;
                CharacterManager targetCharacterManager = playerToHeal.GetComponent<CharacterManager>();
                targetCharacterManager.Server_TakeHealth(healthToHeal);
                Player targetPlayer = playerToHeal.GetComponent<Player>();
                targetPlayer.healedHealth += healthToHeal;
                _myOwner.GetComponent<CharacterManager>().TempHealedHealth += healthToHeal;
                RpcChangeHealedHealthValue(healthToHeal);
            }
            else
            {
                Debug.Log("CmdHealPlayer received non-positive healthToHeal");
            }
        }

        [Command]
        void CmdPlayHealingAnimation(bool play)
        {
            if (play)
            {
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = false;
                _myOwner.GetComponent<Animator>().SetTrigger("BandageUse");
            }
            else
            {
                _myOwner.GetComponent<Animator>().SetTrigger("StopBandageUse");
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = true;
                if (_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }

            RpcPlayHealingAnimation(play);
        }

        [ClientRpc]
        void RpcPlayHealingAnimation(bool play)
        {
            if (play)
            {
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = false;
                _myOwner.GetComponent<Animator>().SetTrigger("BandageUse");
            }
            else
            {
                _myOwner.GetComponent<Animator>().SetTrigger("StopBandageUse");
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = true;
                if (_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }
        }

        [ClientRpc]
        void RpcChangeHealedHealthValue(int value)
        {
            _myOwner.GetComponent<Player>().SetHealedHealthValue(value);
        }

        [Command]
        void CmdSetHealingStatus(bool _status)
        {
            CharacterManager characterManager = _myOwner.GetComponent<CharacterManager>();
            if (characterManager != null)
            {
                characterManager.isHealing = _status;
            }

            RpcSetHealingStatus(_status);
        }

        [ClientRpc]
        void RpcSetHealingStatus(bool _status)
        {
            CharacterManager characterManager = _myOwner.GetComponent<CharacterManager>();
            if (characterManager != null)
            {
                characterManager.isHealing = _status;
            }
        }

        [Command]
        void CmdCompleteHealing(NetworkIdentity ownerPlayerNetId, NetworkIdentity targetPlayerNetId)
        {
            CharacterManager characterManager = targetPlayerNetId.GetComponent<CharacterManager>();

            Animator ownerAnimator = ownerPlayerNetId.GetComponent<Animator>();
            Animator targetAnimator = targetPlayerNetId.GetComponent<Animator>();

            if (characterManager.health == characterManager.maxHealth)
            {
                targetAnimator.SetBool("Injured", false);
                targetAnimator.SetTrigger("CancelInjury");
            }

            ownerAnimator.GetComponent<Animator>().SetTrigger("StopBandageUse");

            Rpc_Consumed();
            _myOwner.Server_DetachCurrentItem();
        }

        [ClientRpc]
        void RpcCompleteHealing(NetworkIdentity ownerPlayerNetId, NetworkIdentity targetPlayerNetId)
        {
            CharacterManager characterManager = targetPlayerNetId.GetComponent<CharacterManager>();

            Animator ownerAnimator = ownerPlayerNetId.GetComponent<Animator>();
            Animator targetAnimator = targetPlayerNetId.GetComponent<Animator>();

            if (characterManager.health == characterManager.maxHealth)
            {
                targetAnimator.SetBool("Injured", false);
                targetAnimator.SetTrigger("CancelInjury");
            }

            ownerAnimator.GetComponent<Animator>().SetTrigger("StopBandageUse");
        }

        [Command]
        void CmdUpdateHealingUI(bool ishealing, NetworkIdentity ownerPlayerNetId, NetworkIdentity targetPlayerNetId)
        {
            targetPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(ishealing, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayerNetId.GetComponent<Player>().username);
            RpcUpdateHealingUI(ishealing, ownerPlayerNetId, targetPlayerNetId);
        }

        [ClientRpc]
        void RpcUpdateHealingUI(bool ishealing, NetworkIdentity ownerPlayerNetId, NetworkIdentity targetPlayerNetId)
        {
            if (NetworkClient.localPlayer.GetComponent<Player>().username == targetPlayerNetId.GetComponent<Player>().username)
                targetPlayerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(ishealing, amountOfHealthToRegenerate, ownerPlayerNetId.GetComponent<Player>().username, healthToRegenerate, true, targetPlayerNetId.GetComponent<Player>().username);
        }

        [ClientRpc]
        void Rpc_Consumed()
        {
            enabled = false;
            foreach (MeshRenderer meshRenderer in _itemMesh)
                meshRenderer.enabled = false;
        }
    }
}