// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using RedicionStudio.NetworkUtils;

namespace RedicionStudio.InventorySystem {

	[System.Serializable]
	public struct Item {

		public int hash;

		[System.NonSerialized]
		private ItemSO _itemSO;
		[System.NonSerialized]
		private int oldHash;
		public ItemSO itemSO {
			get {
				if (_itemSO == null || hash != oldHash) {
					_itemSO = ItemSO.GetItemSO(hash);
					oldHash = hash;
				}
				return _itemSO;
			}
		}

		public float currentShelfLifeInSeconds;

		public Item(string itemSOUniqueName, float currentShelfLifeInSeconds) {
			hash = itemSOUniqueName.GetStableHashCode();

			_itemSO = null;
			oldHash = hash;

			this.currentShelfLifeInSeconds = currentShelfLifeInSeconds;
		}

		public string TooltipText => itemSO.GetTooltipText().Replace("{CURRENT_SHELF_LIFE}",
			System.TimeSpan.FromSeconds(currentShelfLifeInSeconds).ToString("hh\\:mm\\:ss"));
	}

	[System.Serializable]
	public struct ItemSlot {

		public Item item;
		public int amount;

		public int IncreaseBy(int value) {
			int c = Mathf.Clamp(value, 0, item.itemSO.stackSize - amount); // ?
			amount += c;
			return c;
		}

		public int DecreaseBy(int value) {
			int c = Mathf.Clamp(value, 0, amount);
			amount -= c;
			return c;
		}

		public string TooltipText => item.TooltipText.Replace("{AMOUNT}", amount.ToString());
	}

	public class SyncListItemSlot : SyncList<ItemSlot> { }

	public abstract class Inventory : ItemContainer {

		public bool PossibleToAdd(Item item, int amount) {
			for (int i = 5; i < slots.Count; i++) {
				if (slots[i].amount == 0) {
					amount -= item.itemSO.stackSize;
				}
				else if (slots[i].item.Equals(item)) { // ?
					amount -= slots[i].item.itemSO.stackSize - slots[i].amount;
				}

				if (amount <= 0) {
					return true;
				}
			}

			return false;
		}

		public bool Add(Item item, int amount) {
			if (!PossibleToAdd(item, amount)) {
				return false;
			}

			int i;

			for (i = 5; i < slots.Count; i++) {
				if (slots[i].amount > 0 && slots[i].item.Equals(item)) { // ?
					ItemSlot tempSlot = slots[i];
					amount -= tempSlot.IncreaseBy(amount);
					slots[i] = tempSlot;
				}

				if (amount <= 0) {
					return true;
				}
			}

			for (i = 5; i < slots.Count; i++) {
				if (slots[i].amount == 0) {
					int tempValue = Mathf.Min(amount, item.itemSO.stackSize);
					slots[i] = new ItemSlot { item = item, amount = tempValue };
					amount -= tempValue;
				}

				if (amount <= 0) {
					return true;
				}
			}

			return false;
		}

		public bool Remove(Item item, int amount) {
			ItemSlot slot;
			for (int i = 0; i < slots.Count; i++) {
				slot = slots[i];
				if (slot.amount > 0 && slot.item.Equals(item)) { // ?
					amount -= slot.DecreaseBy(amount);
					slots[i] = slot;

					if (amount == 0) {
						return true;
					}
				}
			}

			return false;
		}
	}
}
