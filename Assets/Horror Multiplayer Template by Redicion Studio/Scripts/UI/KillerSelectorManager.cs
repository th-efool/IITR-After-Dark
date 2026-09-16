// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class KillerSelectorManager : NetworkBehaviour
    {
        public RedicionStudio.InventorySystem.ItemSO[] killerItemSOs;
        public RedicionStudio.InventorySystem.ItemSO defaultKillerSO;
        public int defaultKillerId = 1;
        public GameObject killerItemUIPrefab;
        public Transform killerItemsContent;
        public TMPro.TMP_Text descriptionText;
        private Item item;
        public GameObject killerSelectionUI;

        public GameObject purchaseWindowPrefab;

        public List<GameObject> instantiatedKillerPreviewModels = new List<GameObject>();

        [Space]
        public List<KillerUIItemButton> killerSelectionButtons = new List<KillerUIItemButton>();

        private void Start()
        {
            if (isLocalPlayer)
            {
                StartCoroutine(C_Check());
            }
        }

        IEnumerator C_Check()
        {
            yield return new WaitForSeconds(3f);

            if (NetworkClient.localPlayer.gameObject.GetComponent<Player>().killerId == defaultKillerId)
                CmdCheckItemInInventory(defaultKillerId, true);

            killerSelectionUI.SetActive(true);
            foreach (RedicionStudio.InventorySystem.KillerSO _killerItem in killerItemSOs)
            {
                GameObject _instantiatedItem;

                _instantiatedItem = Instantiate(killerItemUIPrefab);

                _instantiatedItem.transform.SetParent(killerItemsContent);
                killerSelectionButtons.Add(_instantiatedItem.GetComponent<KillerUIItemButton>());
                _instantiatedItem.GetComponent<KillerUIItemButton>().localPlayer = NetworkClient.localPlayer.gameObject;
                _instantiatedItem.GetComponent<KillerUIItemButton>().previewModelPrefab = _killerItem.previewModelPrefab;
                _instantiatedItem.GetComponent<KillerUIItemButton>().itemSO = _killerItem;
                _instantiatedItem.GetComponent<KillerUIItemButton>().killerID = _killerItem.killerID;
                _instantiatedItem.GetComponent<KillerUIItemButton>().killerSelectorManager = this;
                _instantiatedItem.GetComponent<KillerUIItemButton>().descriptionText = descriptionText;
                _instantiatedItem.GetComponent<KillerUIItemButton>().uniqueName = _killerItem.uniqueName;
                _instantiatedItem.GetComponent<KillerUIItemButton>().price = _killerItem.price;
                _instantiatedItem.GetComponent<KillerUIItemButton>().killerSprite = _killerItem.sprite;
                _instantiatedItem.GetComponent<KillerUIItemButton>().description = _killerItem.tooltipText;

                _instantiatedItem.GetComponent<KillerUIItemButton>().SetUpItem();
            }
            killerSelectionUI.SetActive(false);
        }

        public void CheckItems()
        {
            foreach (KillerUIItemButton killerUIItem in killerSelectionButtons)
            {
                killerUIItem.CheckItem();
            }
        }

        [Server]
        private bool IsItemInInventoryOnServer(int killerId)
        {
            foreach (var slot in GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().slots)
            {
                RedicionStudio.InventorySystem.ItemSO _itemSO = slot.item.itemSO;

                if (_itemSO != null && _itemSO is RedicionStudio.InventorySystem.KillerSO killerSO && killerSO.killerID == killerId)
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckItemInInventory(int killerId, bool purchase)
        {
            CmdCheckItemInInventory(killerId, purchase);
        }

        [Command]
        private void CmdCheckItemInInventory(int killerId, bool purchase)
        {
            bool isItemInInventory = IsItemInInventoryOnServer(killerId);
            RpcHandleItemCheckResult(isItemInInventory, killerId, purchase);
        }


        [ClientRpc]
        private void RpcHandleItemCheckResult(bool isItemInInventory, int killerId, bool purchase)
        {
            if (isLocalPlayer)
            {
                if (isItemInInventory)
                {
                    //Item is already in the inventory
                    if (purchase)
                    {
                        SetKillerID(killerId);
                    }
                    foreach (KillerUIItemButton killerUIItem in killerSelectionButtons)
                    {
                        if (killerUIItem.killerID == killerId)
                        {
                            killerUIItem.isItemInInventory = true;
                            killerUIItem.CheckItem();
                        }
                    }
                }
                else
                {
                    //Item is not in the inventory. Initiating purchase
                    if (purchase)
                    {
                        foreach (RedicionStudio.InventorySystem.KillerSO _killerItem in killerItemSOs)
                        {
                            if (_killerItem.killerID == killerId)
                            {
                                BuyKillerItem(_killerItem, 0, killerId);
                            }
                        }
                    }
                }
            }
        }

        public void BuyKillerItem(RedicionStudio.InventorySystem.ItemSO itemSO, int itemPrice, int killerId)
        {
            GameObject _localPlayer;

            _localPlayer = NetworkClient.localPlayer.gameObject;

            if (killerId == defaultKillerId)
            {
                item = new Item(itemSO.uniqueName, itemSO is ConsumableItemSO consumableItemSO ? consumableItemSO.shelfLifeInSeconds : 0f);

                _localPlayer.GetComponent<PlayerInteractionModule>().AddItem(_localPlayer.GetComponent<PlayerInventoryModule>(), itemPrice, item, 1);

                foreach (RedicionStudio.InventorySystem.KillerSO _killerItem in killerItemSOs)
                {
                    if (_killerItem.killerID == killerId)
                        _localPlayer.GetComponent<Player>().SetKillerID(_killerItem.killerID);
                    break;
                }

                foreach (KillerUIItemButton killerUIItem in killerSelectionButtons)
                {
                    if (killerUIItem.killerID == killerId)
                        killerUIItem.isItemInInventory = true;
                    killerUIItem.CheckItem();
                }
            }
            else
            {
                if (isLocalPlayer)
                {
                    ItemPurchaseWindow _purchaseWindow = Instantiate(purchaseWindowPrefab).GetComponent<ItemPurchaseWindow>();
                    _purchaseWindow.OpenItemPurchaseWindow(itemSO, 1, true, false);
                }
            }
        }

        private void SetKillerID(int killerId)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<Player>().SetKillerID(killerId);

            foreach (KillerUIItemButton killerUIItem in killerSelectionButtons)
            {
                if (killerUIItem.killerID == killerId)
                {
                    killerUIItem.isItemInInventory = true;
                    killerUIItem.CheckItem();
                    killerUIItem.SelectKiller();
                }
            }
        }
    }
}