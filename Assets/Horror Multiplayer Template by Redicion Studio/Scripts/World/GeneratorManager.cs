// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class GeneratorManager : NetworkBehaviour
    {
        [SyncVar] public float health = 0;

        [SyncVar(hook = nameof(OnBeingRepairedChanged))]
        public bool beingRepaired = false;

        [SyncVar] public bool canBeRepaired = true;

        [SyncVar] public NetworkIdentity currentRepairerPlayer;

        public Transform generatorRepairPosition;

        private void Start()
        {

        }

        private void Update()
        {
            if (isServer)
            {
                if (health > 100)
                    health = 100;
            }

            if (isServer && currentRepairerPlayer != null && canBeRepaired && beingRepaired)
            {
                RpcUpdatePlayerRepairUI(currentRepairerPlayer, true, health);
            }
        }

        [ClientRpc]
        public void RpcUpdatePlayerRepairUI(NetworkIdentity _player, bool repairs, float value)
        {
            _player.GetComponent<PlayerInteractionModule>().UpdateRepairUI(repairs, value, currentRepairerPlayer.GetComponent<Player>().username, "Generator");
        }

        private void OnTriggerStay(Collider other)
        {
            if (!isServer)
                return;

            if (canBeRepaired)
            {
                if (other.CompareTag("Player") && other.TryGetComponent(out CharacterManager characterManager))
                {
                    if (characterManager.itemCurrentlyInUse != null && characterManager.itemCurrentlyInUse.ItemName == "toolbox")
                    {
                        ToolboxManager toolboxManager = characterManager.itemCurrentlyInUse.GetComponent<ToolboxManager>();
                        if (toolboxManager != null && toolboxManager.readyToRepair && !beingRepaired)
                        {
                            currentRepairerPlayer = characterManager.GetComponent<NetworkIdentity>();
                            beingRepaired = true;
                            RpcStartRepair(characterManager.GetComponent<NetworkIdentity>());
                            //currentRepairerPlayer.transform.LookAt(transform);
                            currentRepairerPlayer.transform.position = generatorRepairPosition.position;
                            currentRepairerPlayer.transform.rotation = generatorRepairPosition.rotation;
                            StartCoroutine(RepairCoroutine());
                        }
                    }
                }
            }
        }

        [ClientRpc]
        private void RpcStartRepair(NetworkIdentity _currentPlayer)
        {
            currentRepairerPlayer = _currentPlayer;
            beingRepaired = true;
            currentRepairerPlayer.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, false);
            currentRepairerPlayer.GetComponent<Animator>().SetTrigger("CarRepairStart");
            if (currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse != null && currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.GetComponent<ToolboxManager>() != null)
            {
                foreach (MeshRenderer meshRenderer in currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.GetComponent<ToolboxManager>()._itemMesh)
                    meshRenderer.enabled = false;
            }
            //currentRepairerPlayer.transform.LookAt(transform);
            currentRepairerPlayer.transform.position = generatorRepairPosition.position;
            currentRepairerPlayer.transform.rotation = generatorRepairPosition.rotation;
        }

        private void OnBeingRepairedChanged(bool oldValue, bool newValue)
        {
            if (!newValue)
            {
                /*repairSlider.gameObject.SetActive(false);
                repairUI.SetActive(false);*/
            }
        }

        void Repair()
        {
            if (health != 100)
            {
                if (!canBeRepaired)
                {
                    canBeRepaired = true;
                    RpcSetCanBeRepairedStatus(true);
                }

                CharacterManager _currentRepairerPlayerManager = currentRepairerPlayer.GetComponent<CharacterManager>();

                if ((_currentRepairerPlayerManager.itemCurrentlyInUse != null &&
                     _currentRepairerPlayerManager.itemCurrentlyInUse.ItemName == "toolbox" &&
                     !_currentRepairerPlayerManager.itemCurrentlyInUse.GetComponent<ToolboxManager>().readyToRepair) ||
                    (_currentRepairerPlayerManager.itemCurrentlyInUse != null &&
                     _currentRepairerPlayerManager.itemCurrentlyInUse.ItemName != "toolbox") ||
                    _currentRepairerPlayerManager.itemCurrentlyInUse == null)
                {
                    beingRepaired = false;
                    currentRepairerPlayer = null;
                    StopCoroutine(RepairCoroutine());
                    RpcCancelRepair(false, false, true, true, true, true, true);
                }
                else
                {
                    health += currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.GetComponent<ToolboxManager>().repairPerFrameValue * currentRepairerPlayer.GetComponent<CharacterManager>().repairSpeedMultiplier;
                }
            }
            else if (health == 100)
            {
                canBeRepaired = false;
                beingRepaired = false;
                currentRepairerPlayer = null;
                StopCoroutine(RepairCoroutine());
                RpcCancelRepair(false, false, true, true, true, true, true);
            }
            else if (currentRepairerPlayer.GetComponent<HunterAbilities>()._inFight)
            {
                canBeRepaired = false;
                beingRepaired = false;
                currentRepairerPlayer = null;
                StopCoroutine(RepairCoroutine());
                RpcCancelRepair(false, false, true, true, true, false, false);
            }
        }

        private IEnumerator RepairCoroutine()
        {
            while (beingRepaired)
            {
                Repair();
                yield return null;
            }
        }

        [ClientRpc]
        void RpcCancelRepair(bool _canBeRepaired, bool _beingRepaired, bool _stopRepairCoroutine, bool _clearCurrentRepairerPlayer, bool setAnimatorTrigger, bool enableItemMesh, bool unBlockPlayer)
        {
            canBeRepaired = _canBeRepaired;
            beingRepaired = _beingRepaired;
            if (unBlockPlayer)
                currentRepairerPlayer.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
            if (enableItemMesh)
            {
                foreach (MeshRenderer meshRenderer in currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.GetComponent<ToolboxManager>()._itemMesh)
                    meshRenderer.enabled = true;
            }
            if (setAnimatorTrigger)
            {
                currentRepairerPlayer.GetComponent<Animator>().SetTrigger("CarRepairEnd");
                if (currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    if (!currentRepairerPlayer.GetComponent<ManageTPController>().isFirstPerson)
                        currentRepairerPlayer.GetComponent<Animator>().SetTrigger(currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        currentRepairerPlayer.GetComponent<Animator>().SetTrigger(currentRepairerPlayer.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }
            currentRepairerPlayer.GetComponent<PlayerInteractionModule>().UpdateRepairUI(false, 0, currentRepairerPlayer.GetComponent<Player>().username, "Generator");
            if (_clearCurrentRepairerPlayer)
                currentRepairerPlayer = null;
            /*if (_stopRepairCoroutine)
                StopCoroutine(RepairCoroutine());*/
        }

        [ClientRpc]
        void RpcSetCanBeRepairedStatus(bool _canBeRepaired)
        {
            canBeRepaired = _canBeRepaired;
        }
    }
}