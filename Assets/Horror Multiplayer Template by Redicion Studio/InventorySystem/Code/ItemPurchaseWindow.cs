// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio.InventorySystem
{
    public class ItemPurchaseWindow : MonoBehaviour
    {
        public ItemSO itemSO;
        public Item item;
        public int amount;

        [Space]
        public string itemName;

        [Space]
        public ConfigurationSO rarityConfiguration;
        public string itemRarity;
        public Image rarityImage;

        [Space]
        public Sprite itemSprite;

        [Space]
        public int itemPrice;

        [Space]
        [Header("UI")]
        public TMPro.TMP_Text itemNameText;
        public TMPro.TMP_Text itemRarityText;
        public TMPro.TMP_Text itemPriceText;
        public TMPro.TMP_Text purchaseDisabledText;
        public Image itemImage;

        public Animator windowAnimator;

        public Button purchaseButton;
        public Image purchaseButtonImage;
        public Color purchaseButtonDisabledColor;
        public Color purchaseEnabledDisabledColor;

        bool hasPurchasedItem = false;
        bool hasCanceled = false;

        bool isHunterItem;
        bool isOutfitItem;

        private void Update()
        {
            if(RoomManager._instance != null)
            {
                if(RoomManager._instance.MatchRunning && !hasCanceled && !hasPurchasedItem)
                    CancelPurchase();
            }
            else
            {
                CancelPurchase();
            }
        }

        public void OpenItemPurchaseWindow(ItemSO _itemSO, int _amount, bool _isHunterItem, bool _isOutfitItem)
        {
            isHunterItem = _isHunterItem;
            isOutfitItem = _isOutfitItem;
            itemSO = _itemSO;
            amount = _amount;
            itemName = itemSO.uniqueName;
            itemNameText.text = itemSO.uniqueName;
            itemPrice = itemSO.price;
            itemPriceText.text = itemPrice + "$";
            itemRarity = itemSO.rarity.ToString();
            itemRarityText.text = itemRarity;
            if (itemRarity == "None")
                rarityImage.color = Color.black;
            else if (itemRarity == "Common")
                rarityImage.color = rarityConfiguration.commonColor;
            else if (itemRarity == "Rare")
                rarityImage.color = rarityConfiguration.rareColor;
            else if (itemRarity == "Unique")
                rarityImage.color = rarityConfiguration.uniqueColor;
            itemImage.sprite = itemSO.sprite;
            if (NetworkClient.localPlayer.GetComponent<Player>().funds < itemPrice)
            {
                purchaseButton.interactable = false;
                purchaseButtonImage.color = purchaseButtonDisabledColor;
                purchaseDisabledText.gameObject.SetActive(true);
            }
            else
            {
                purchaseButton.interactable = true;
                purchaseButtonImage.color = purchaseEnabledDisabledColor;
                purchaseDisabledText.gameObject.SetActive(false);
            }
            windowAnimator.Play("ItemPurchaseWindowOpenAnimation");
        }

        public void BuyItem()
        {
            if(!hasPurchasedItem && !hasCanceled)
            {
                hasPurchasedItem = true;
                GameObject _localPlayer;

                _localPlayer = NetworkClient.localPlayer.gameObject;

                item = new Item(itemSO.uniqueName, itemSO is ConsumableItemSO consumableItemSO ? consumableItemSO.shelfLifeInSeconds : 0f);

                _localPlayer.GetComponent<PlayerInteractionModule>().AddItem(_localPlayer.GetComponent<PlayerInventoryModule>(), itemPrice, item, amount);

                windowAnimator.Play("ItemPurchaseWindowCloseAnimation");

                StartCoroutine(CloseWindow());

                if (isHunterItem)
                {
                    if(itemSO is RedicionStudio.InventorySystem.KillerSO killerSO)
                    {
                        foreach (RedicionStudio.InventorySystem.KillerSO _killerItem in _localPlayer.GetComponent<KillerSelectorManager>().killerItemSOs)
                        {
                            if (_killerItem.killerID == killerSO.killerID)
                                _localPlayer.GetComponent<Player>().SetKillerID(_killerItem.killerID);
                            break;
                        }

                        foreach (KillerUIItemButton killerUIItem in _localPlayer.GetComponent<KillerSelectorManager>().killerSelectionButtons)
                        {
                            if (killerUIItem.killerID == killerSO.killerID)
                                killerUIItem.isItemInInventory = true;
                            killerUIItem.CheckItem();
                        }

                        _localPlayer.GetComponent<KillerSelectorManager>().CheckItems();
                    }
                }
                else if(isOutfitItem)
                {
                    if (itemSO is RedicionStudio.InventorySystem.OutfitItemSO outfitSO)
                    {
                        foreach (RedicionStudio.InventorySystem.OutfitItemSO _outfitItem in _localPlayer.GetComponent<OutfitManager>().outfitItemSOs)
                        {
                            if (_outfitItem.outfitID == outfitSO.outfitID)
                                _localPlayer.GetComponent<Player>().SetOutfitID(_outfitItem.outfitID);
                            break;
                        }

                        foreach (OutfitUIItemButton outfitUIItem in _localPlayer.GetComponent<OutfitManager>().outfitSelectionButtons)
                        {
                            if (outfitUIItem.outfitID == outfitSO.outfitID)
                                outfitUIItem.isItemInInventory = true;
                            outfitUIItem.CheckItem();
                        }
                    }

                    _localPlayer.GetComponent<OutfitManager>().CheckItems();
                }
            }
        }

        public void CancelPurchase()
        {
            if(!hasPurchasedItem && !hasCanceled)
            {
                hasCanceled = true;
                windowAnimator.Play("ItemPurchaseWindowCloseAnimation");

                StartCoroutine(CloseWindow());
            }
        }

        private IEnumerator CloseWindow()
        {
            yield return new WaitForSeconds(0.20f);

            Destroy(gameObject);
        }
    }
}
