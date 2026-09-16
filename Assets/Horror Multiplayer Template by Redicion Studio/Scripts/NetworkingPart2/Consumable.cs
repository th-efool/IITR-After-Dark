// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Consumable : GameplayItem
    {
        [Header("Consumable")]
        [SyncVar] public int amountOfHealthToRegenerate = 50;
        private int healthToRegenerate;
        public string stopConsumingAnimatorTrigger = "StopMedikitUse";
        public string useConsumableAnimatorTrigger = "MedikitUse";

        private Coroutine healingCoroutine;

        private void Start()
        {
            healthToRegenerate = amountOfHealthToRegenerate;
        }

        protected override void Server_Use()
        {
            if (_myOwner.health < _myOwner.maxHealth)
            {
                // Notify the client to start the healing process
                TargetStartHealing(_myOwner.GetComponent<NetworkIdentity>().connectionToClient);
            }
        }

        [TargetRpc]
        void TargetStartHealing(NetworkConnection target)
        {
            if (!_myOwner.GetComponent<CharacterManager>().isHealing)
            {
                healingCoroutine = StartCoroutine(HealOverTime(_myOwner.GetComponent<NetworkIdentity>()));
            }
        }

        private IEnumerator HealOverTime(NetworkIdentity playerNetId)
        {
            CmdSetHealingStatus(true); // Update healing status
                                       // Calculate total health to regenerate
            float healthMultiplier = _myOwner.GetComponent<CharacterManager>().consumableHealthMultiplier;
            int totalHealthToRegenerate = (int)(amountOfHealthToRegenerate * healthMultiplier);

            CmdPlayHealingAnimation(true); // Start healing animation

            int healedHealth = 0;
            while (healedHealth < totalHealthToRegenerate && _myOwner.health < _myOwner.maxHealth)
            {
                if (_myOwner.GetComponent<HunterAbilities>()._inFight)
                {
                    CmdSetHealingStatus(false); // Update healing status
                    playerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, playerNetId.GetComponent<Player>().username, healthToRegenerate, false, "");
                    healingCoroutine = null;
                    yield break;
                }
                // Check if left mouse button is not held down
                if (!_input.use)
                {
                    CmdSetHealingStatus(false); // Update healing status
                    CmdPlayHealingAnimation(false); // Stop healing animation
                    playerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, playerNetId.GetComponent<Player>().username, healthToRegenerate, false, "");
                    healingCoroutine = null;
                    yield break;
                }

                int healthToHeal = Mathf.Min((int)(healthMultiplier), totalHealthToRegenerate - healedHealth);
                CmdHealPlayer(healthToHeal);
                healedHealth += healthToHeal;
                playerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(true, amountOfHealthToRegenerate, playerNetId.GetComponent<Player>().username, healthToRegenerate, false, "");
                yield return new WaitForSeconds(0.5f); // Heal over time interval
            }

            if (amountOfHealthToRegenerate <= 0)
            {
                RpcCompleteHealing(_myOwner.GetComponent<NetworkIdentity>());
                CmdCompleteHealing(_myOwner.GetComponent<NetworkIdentity>());
            }

            CmdPlayHealingAnimation(false); // Stop healing animation

            CmdSetHealingStatus(false); // Update healing status
            playerNetId.GetComponent<PlayerInteractionModule>().UpdateHealingUI(false, amountOfHealthToRegenerate, playerNetId.GetComponent<Player>().username, healthToRegenerate, false, "");
            healingCoroutine = null;
        }

        [Command]
        void CmdHealPlayer(int healthToHeal)
        {
            if (healthToHeal > 0)
            {
                amountOfHealthToRegenerate -= healthToHeal;
                _myOwner.GetComponent<CharacterManager>().Server_TakeHealth(healthToHeal);
                _myOwner.GetComponent<Player>().healedHealth += healthToHeal;
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
                _myOwner.GetComponent<CharacterManager>().bandage.SetActive(true);
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = false;
                _myOwner.GetComponent<Animator>().SetTrigger(useConsumableAnimatorTrigger);
            }
            else
            {
                _myOwner.GetComponent<Animator>().SetTrigger(stopConsumingAnimatorTrigger);
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
                _myOwner.GetComponent<CharacterManager>().bandage.SetActive(true);
                foreach (MeshRenderer meshRenderer in _itemMesh)
                    meshRenderer.enabled = false;
                _myOwner.GetComponent<Animator>().SetTrigger(useConsumableAnimatorTrigger);
            }
            else
            {
                _myOwner.GetComponent<Animator>().SetTrigger(stopConsumingAnimatorTrigger);
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
        void CmdCompleteHealing(NetworkIdentity playerNetId)
        {
            CharacterManager characterManager = playerNetId.GetComponent<CharacterManager>();

            Animator animator = playerNetId.GetComponent<Animator>();

            if (characterManager.health == 100)
            {
                animator.SetBool("Injured", false);
                animator.SetTrigger("CancelInjury");
            }

            playerNetId.GetComponent<Animator>().SetTrigger(stopConsumingAnimatorTrigger);

            Rpc_Consumed();
            _myOwner.Server_DetachCurrentItem();
        }

        [ClientRpc]
        void RpcCompleteHealing(NetworkIdentity playerNetId)
        {
            CharacterManager characterManager = playerNetId.GetComponent<CharacterManager>();

            Animator animator = playerNetId.GetComponent<Animator>();

            if (characterManager.health == 100)
            {
                animator.SetBool("Injured", false);
                animator.SetTrigger("CancelInjury");
            }

            playerNetId.GetComponent<Animator>().SetTrigger(stopConsumingAnimatorTrigger);
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