// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class OutfitManager : NetworkBehaviour
    {
        public RedicionStudio.InventorySystem.ItemSO[] outfitItemSOs;
        public RedicionStudio.InventorySystem.ItemSO defaultOutfitSO;
        public int defaultOutfitId = 1;
        public OutfitItem[] outfits;
        public GameObject outfitItemUIPrefab;
        public Transform outfitItemsContent;
        public TMPro.TMP_Text descriptionText;
        private Item item;
        public GameObject outfitSelectionUI;
        public GameObject showPlayerCameraPrefab;

        public GameObject purchaseWindowPrefab;

        public List<GameObject> instantiatedOutfitPreviewModels = new List<GameObject>();

        [Space]
        public List<OutfitUIItemButton> outfitSelectionButtons = new List<OutfitUIItemButton>();

        public GameObject[] sheriffOutfitMesh;
        GameObject previousOutfit;

        private void Start()
        {
            if (isLocalPlayer)
            {
                StartCoroutine(C_Check());
            }
        }

        private void Update()
        {
            foreach (OutfitItem outfit in outfits)
            {
                if (instantiatedOutfitPreviewModels.Count == 0 && !GetComponent<CharacterManager>().isSheriff)
                {
                    if (outfit.outfitID == GetComponent<Player>().outfitId)
                    {
                        foreach (OutfitItem outfititem in outfits)
                        {
                            outfititem.outfitModel.SetActive(false);
                        }

                        outfit.outfitModel.SetActive(true);
                    }
                }

                if (outfit.outfitID == GetComponent<Player>().outfitId)
                {
                    if (GetComponent<CharacterManager>().health < 100)
                    {
                        foreach (SkinnedMeshRenderer renderer in outfit.outfitPartsToBeDyedWithBlood)
                        {
                            if (renderer.material != outfit.bloodMaterial)
                                renderer.material = outfit.bloodMaterial;
                        }
                    }
                    else
                    {
                        foreach (SkinnedMeshRenderer renderer in outfit.outfitPartsToBeDyedWithBlood)
                        {
                            if (renderer.material != outfit.defaultMaterial)
                                renderer.material = outfit.defaultMaterial;
                        }
                    }
                }
            }
        }

        IEnumerator C_Check()
        {
            yield return new WaitForSeconds(3f);

            if (NetworkClient.localPlayer.gameObject.GetComponent<Player>().outfitId == defaultOutfitId)
                CmdCheckItemInInventory(defaultOutfitId, true);

            outfitSelectionUI.SetActive(true);
            foreach (RedicionStudio.InventorySystem.OutfitItemSO _outfitItem in outfitItemSOs)
            {
                GameObject _instantiatedItem;

                _instantiatedItem = Instantiate(outfitItemUIPrefab);

                _instantiatedItem.transform.SetParent(outfitItemsContent);
                outfitSelectionButtons.Add(_instantiatedItem.GetComponent<OutfitUIItemButton>());
                _instantiatedItem.GetComponent<OutfitUIItemButton>().localPlayer = NetworkClient.localPlayer.gameObject;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().previewModelPrefab = _outfitItem.previewModelPrefab;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().itemSO = _outfitItem;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().outfitID = _outfitItem.outfitID;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().outfitManager = this;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().descriptionText = descriptionText;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().uniqueName = _outfitItem.uniqueName;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().price = _outfitItem.price;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().outfitSprite = _outfitItem.sprite;
                _instantiatedItem.GetComponent<OutfitUIItemButton>().description = _outfitItem.tooltipText;

                _instantiatedItem.GetComponent<OutfitUIItemButton>().SetUpItem();
            }
            outfitSelectionUI.SetActive(false);
        }

        public void CheckItems()
        {
            foreach (OutfitUIItemButton outfitUIItem in outfitSelectionButtons)
            {
                outfitUIItem.CheckItem();
            }
        }

        [Server]
        private bool IsItemInInventoryOnServer(int outfitId)
        {
            foreach (var slot in GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().slots)
            {
                RedicionStudio.InventorySystem.ItemSO _itemSO = slot.item.itemSO;

                if (_itemSO != null && _itemSO is RedicionStudio.InventorySystem.OutfitItemSO outfitSO && outfitSO.outfitID == outfitId)
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckItemInInventory(int outfitId, bool purchase)
        {
            CmdCheckItemInInventory(outfitId, purchase);
        }

        [Command]
        private void CmdCheckItemInInventory(int outfitId, bool purchase)
        {
            bool isItemInInventory = IsItemInInventoryOnServer(outfitId);
            RpcHandleItemCheckResult(isItemInInventory, outfitId, purchase);
        }


        [ClientRpc]
        private void RpcHandleItemCheckResult(bool isItemInInventory, int outfitId, bool purchase)
        {
            if (isLocalPlayer)
            {
                if (isItemInInventory)
                {
                    //Item is already in the inventory
                    if (purchase)
                    {
                        SetOutfitID(outfitId);
                    }
                    foreach (OutfitUIItemButton outfitUIItem in outfitSelectionButtons)
                    {
                        if (outfitUIItem.outfitID == outfitId)
                        {
                            outfitUIItem.isItemInInventory = true;
                            outfitUIItem.CheckItem();
                        }
                    }
                }
                else
                {
                    //Item is not in the inventory. Initiating purchase
                    if (purchase)
                    {
                        foreach (RedicionStudio.InventorySystem.OutfitItemSO _outfitItem in outfitItemSOs)
                        {
                            if (_outfitItem.outfitID == outfitId)
                            {
                                BuyOutfitItem(_outfitItem, 0, outfitId);
                            }
                        }
                    }
                }
            }
        }

        public void BuyOutfitItem(RedicionStudio.InventorySystem.ItemSO itemSO, int itemPrice, int outfitId)
        {
            GameObject _localPlayer;

            _localPlayer = NetworkClient.localPlayer.gameObject;

            if (outfitId == defaultOutfitId)
            {
                item = new Item(itemSO.uniqueName, itemSO is ConsumableItemSO consumableItemSO ? consumableItemSO.shelfLifeInSeconds : 0f);

                _localPlayer.GetComponent<PlayerInteractionModule>().AddItem(_localPlayer.GetComponent<PlayerInventoryModule>(), itemPrice, item, 1);

                foreach (RedicionStudio.InventorySystem.OutfitItemSO _outfitItem in outfitItemSOs)
                {
                    if (_outfitItem.outfitID == outfitId)
                        _localPlayer.GetComponent<Player>().SetOutfitID(_outfitItem.outfitID);
                    break;
                }

                foreach (OutfitUIItemButton outfitUIItem in outfitSelectionButtons)
                {
                    if (outfitUIItem.outfitID == outfitId)
                        outfitUIItem.isItemInInventory = true;
                    outfitUIItem.CheckItem();
                }
            }
            else
            {
                if (isLocalPlayer)
                {
                    ItemPurchaseWindow _purchaseWindow = Instantiate(purchaseWindowPrefab).GetComponent<ItemPurchaseWindow>();
                    _purchaseWindow.OpenItemPurchaseWindow(itemSO, 1, false, true);
                }
            }
        }

        private void SetOutfitID(int outfitId)
        {
            NetworkClient.localPlayer.gameObject.GetComponent<Player>().SetOutfitID(outfitId);

            foreach (OutfitItem outfit in outfits)
            {
                if (outfit.outfitID == outfitId)
                {
                    foreach (OutfitItem outfititem in outfits)
                    {
                        outfititem.outfitModel.SetActive(false);
                    }

                    outfit.outfitModel.SetActive(true);
                    NetworkClient.localPlayer.gameObject.GetComponent<Player>().playerImage.sprite = outfit.outfitImage;
                }
            }

            foreach (OutfitUIItemButton outfitUIItem in outfitSelectionButtons)
            {
                if (outfitUIItem.outfitID == outfitId)
                {
                    outfitUIItem.isItemInInventory = true;
                    outfitUIItem.CheckItem();
                    outfitUIItem.SelectOutfit();
                }
            }
        }

        public void TogglePlayerModel(bool _show)
        {
            GameObject _localPlayer;

            _localPlayer = NetworkClient.localPlayer.gameObject;

            foreach (OutfitItem outfit in _localPlayer.GetComponent<OutfitManager>().outfits)
            {
                outfit.outfitModel.SetActive(false);
                if (outfit.outfitID == _localPlayer.GetComponent<Player>().outfitId)
                {
                    outfit.outfitModel.SetActive(_show);
                }
            }
        }

        public GameObject GetLocalPlayer()
        {
            return NetworkClient.localPlayer.gameObject;
        }

        public void SetSheriffOutfit(NetworkIdentity playerNetId)
        {
            foreach (OutfitItem outfit in outfits)
            {
                if (outfit.outfitModel.activeInHierarchy)
                {
                    previousOutfit = outfit.outfitModel;
                    break;
                }
            }

            foreach (OutfitItem outfit in outfits)
            {
                outfit.outfitModel.SetActive(false);
            }

            foreach (GameObject _sheriffOutfitMesh in sheriffOutfitMesh)
            {
                _sheriffOutfitMesh.SetActive(true);
            }

            if (playerNetId != null && NetworkClient.localPlayer != null)
            {
                if (NetworkClient.localPlayer.GetComponent<NetworkIdentity>().netId == playerNetId.netId)
                {
                    GameObject instantiatedShowPlayerCamera;
                    instantiatedShowPlayerCamera = Instantiate(showPlayerCameraPrefab);
                    instantiatedShowPlayerCamera.transform.position = NetworkClient.localPlayer.transform.position;
                    instantiatedShowPlayerCamera.transform.rotation = NetworkClient.localPlayer.transform.rotation;
                }
            }
        }

        public void SetPreviousOutfit()
        {
            if (previousOutfit != null)
            {
                previousOutfit.SetActive(true);
            }

            foreach (GameObject _sheriffOutfitMesh in sheriffOutfitMesh)
            {
                _sheriffOutfitMesh.SetActive(false);
            }
        }

        public void ShowOutfit(int outfitId)
        {
            CmdShowOutfit(GetComponent<NetworkIdentity>(), outfitId);
        }

        [Command]
        void CmdShowOutfit(NetworkIdentity playerNetId, int _outfitId)
        {
            foreach (OutfitItem outfit in outfits)
            {
                if (outfit.outfitID == _outfitId)
                {
                    foreach (OutfitItem outfititem in outfits)
                    {
                        outfititem.outfitModel.SetActive(false);
                    }

                    outfit.outfitModel.SetActive(true);
                }
            }

            RpcShowOutfit(playerNetId, _outfitId);
        }

        [ClientRpc]
        void RpcShowOutfit(NetworkIdentity playerNetId, int _outfitId)
        {
            foreach (OutfitItem outfit in outfits)
            {
                if (outfit.outfitID == _outfitId)
                {
                    foreach (OutfitItem outfititem in outfits)
                    {
                        outfititem.outfitModel.SetActive(false);
                    }

                    outfit.outfitModel.SetActive(true);
                }
            }
        }
    }

    [System.Serializable]
    public class OutfitItem
    {
        public string name;
        public int outfitID;
        public GameObject outfitModel;
        public Sprite outfitImage;
        public SkinnedMeshRenderer[] outfitPartsToBeDyedWithBlood;
        public Material defaultMaterial;
        public Material bloodMaterial;
    }
}