// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio.InventorySystem
{
    public class ItemShop : NetworkBehaviour
    {
        public ItemSO[] items;
        public GameObject shopItemUIPrefab;
        public Transform shopItemsContent;

        private void Start()
        {
            foreach (ItemSO _item in items)
            {
                GameObject _instantiatedItem;

                _instantiatedItem = Instantiate(shopItemUIPrefab);

                _instantiatedItem.transform.SetParent(shopItemsContent);
                _instantiatedItem.GetComponent<ShopItem>().itemSO = _item;
                _instantiatedItem.GetComponent<ShopItem>().itemName = _item.uniqueName;
                _instantiatedItem.GetComponent<ShopItem>().amount = _item.stackSize;
                _instantiatedItem.GetComponent<ShopItem>().itemRarity = _item.rarity.ToString();
                _instantiatedItem.GetComponent<ShopItem>().itemSprite = _item.sprite;
                _instantiatedItem.GetComponent<ShopItem>().itemPrice = _item.price;

                _instantiatedItem.GetComponent<ShopItem>().SetUpItem();
            }
        }
    }
}
