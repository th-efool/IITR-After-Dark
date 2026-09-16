// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CompanionManager : NetworkBehaviour
    {
        [SerializeField] public CompanionItem[] companions;
        [SyncVar] public string currentCompanionName;
        [SyncVar] public bool isCompanionInstantiated = false;

        public RedicionStudio.InventorySystem.PlayerInventoryModule inventoryModule;

        RedicionStudio.InventorySystem.UIPlayerInventory uiPlayerInventory;

        private void Update()
        {
            if (uiPlayerInventory == null)
                uiPlayerInventory = GameObject.Find("UIPlayerInventory").GetComponent<RedicionStudio.InventorySystem.UIPlayerInventory>();

            if (!isLocalPlayer)
            {
                return;
            }

            if (GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().slots[3].item.itemSO != null)
            {
                if (inventoryModule.slots[3].item.itemSO is RedicionStudio.InventorySystem.CompanionItemSO)
                {
                    foreach (CompanionItem companion in companions)
                    {
                        if (currentCompanionName != inventoryModule.slots[3].item.itemSO.uniqueName)
                        {
                            if (companion.name == inventoryModule.slots[3].item.itemSO.uniqueName && !isCompanionInstantiated)
                            {
                                Vector3 companionSpawnPosition = new Vector3(transform.position.x - 2, transform.position.y, transform.position.z);

                                CmdInstantiateCompanion(gameObject, GetComponent<Player>().username, inventoryModule.slots[3].item.itemSO.uniqueName, true, companionSpawnPosition, transform.rotation);
                            }
                        }
                    }
                }
            }
            else
            {
                if (isCompanionInstantiated)
                {
                    CmdClearCompanion("", false);
                }
            }
        }

        [Command]
        void CmdInstantiateCompanion(GameObject owner, string ownerName, string companionName, bool isInstantiated, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            if (isCompanionInstantiated)
                return;

            currentCompanionName = companionName;
            isCompanionInstantiated = isInstantiated;

            GameObject foundCompanion = new GameObject();

            foreach (CompanionItem companion in companions)
            {
                if (companion.name == companionName)
                    foundCompanion = companion.companionPrefab;
            }

            GameObject _Companion = Instantiate(foundCompanion, spawnPosition, spawnRotation) as GameObject;

            NetworkServer.Spawn(_Companion);

            Companion comp = _Companion.GetComponent<Companion>();
            comp.netIdentity.AssignClientAuthority(this.connectionToClient);

            comp.owner = owner;
            comp.ownerName = ownerName;

            Debug.Log("Companion " + "''" + _Companion + "''" + " of the player " + "''" + ownerName + "''" + " has been spawned.");

            RpcInstantiateCompanion(comp, owner, ownerName, companionName, isInstantiated);
        }

        [ClientRpc]
        void RpcInstantiateCompanion(Companion _companion, GameObject _owner, string _ownerName, string companionName, bool isInstantiated)
        {
            _companion.owner = _owner;
            _companion.ownerName = _ownerName;
            currentCompanionName = companionName;
            isCompanionInstantiated = isInstantiated;
        }

        [Command]
        void CmdClearCompanion(string companionName, bool isInstantiated)
        {
            currentCompanionName = companionName;
            isCompanionInstantiated = isInstantiated;

            RpcClearCompanion(companionName, isInstantiated);
        }

        [ClientRpc]
        void RpcClearCompanion(string companionName, bool isInstantiated)
        {
            currentCompanionName = companionName;
            isCompanionInstantiated = isInstantiated;
        }
    }

    [System.Serializable]
    public class CompanionItem
    {
        [Tooltip("The name of the companionItem must match the unique name of the companionSO")]
        public string name = "Companion0";
        [Space]
        public RedicionStudio.InventorySystem.ItemSO itemSO;
        [Space]
        public GameObject companionPrefab;
    }
}