// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

namespace RedicionStudio.InventorySystem {

	[CreateAssetMenu(fileName = "New Item", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Inventory System")]
	public class ItemSO : ScriptableObject {

		public string uniqueName;

		private static Dictionary<int, ItemSO> _itemSOs;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		private static void Load() {
			ItemSO[] itemSOs = Resources.LoadAll<ItemSO>(string.Empty);

			List<string> duplicates = itemSOs.ToList().GroupBy(item => item.uniqueName)
				.Where(group => group.Count() > 1)
				.Select(group => group.Key).ToList();

			if (duplicates.Count == 0) {
				_itemSOs = itemSOs.ToDictionary(item => item.uniqueName.GetStableHashCode(), item => item);
				Debug.Log(_itemSOs.Count + " item scriptable objects have been loaded.");
				return;
			}

			for (int i = 0; i < duplicates.Count; i++) {
				Debug.LogError("Multiple item scriptable objects with the name \"" + duplicates[i] + "\" have been found.");
			}
		}

		public static ItemSO GetItemSO(int hash) {
			if (!_itemSOs.ContainsKey(hash)) {
				return null;
			}
			return _itemSOs[hash];
		}

		public GameObject modelPrefab;
		public Sprite sprite;

		[Space]
		public int stackSize = 1;

		[Space, TextArea(1, 32)]
		public string tooltipText;

		public enum Rarity {
			None,
			Common,
			Rare,
			Unique
		}

        public enum ItemType
        {
            Outfit,
            Perk,
            Killer,
        }

        [Space]
		public Rarity rarity;

		[Space]
		public int price;
		public int sellPrice;

        [Space]
        public ItemType itemType;

        protected virtual void OnValidate() {
			if (stackSize < 1) {
				stackSize = 1;
			}
		}

		public virtual string GetTooltipText() {
			return tooltipText.Replace("{NAME}", uniqueName).Replace("{PRICE}", price.ToString()).Replace("{SELL_PRICE}", sellPrice.ToString());
		}
	}
}
