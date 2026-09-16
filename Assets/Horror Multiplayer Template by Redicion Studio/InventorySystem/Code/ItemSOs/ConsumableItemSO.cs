// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio.InventorySystem {

    //[CreateAssetMenu(fileName = "New Consumable Item SO", menuName = "Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Consumable")]
    public class ConsumableItemSO : UseableItemSO {

		[Header("Consumable")]
		public int nutrition;

		[Space]
		public float shelfLifeInSeconds;

		private void ApplyEffects(PlayerNutritionModule playerNutrition) {
			playerNutrition.value += nutrition;
			if (playerNutrition.value > 100) {
				playerNutrition.value = 100;
			}
		}

		public override void Use(PlayerInventoryModule playerInventory, int slotIndex) {
			ItemSlot slot = playerInventory.slots[slotIndex];
			_ = slot.DecreaseBy(1);
			playerInventory.slots[slotIndex] = slot;

			ApplyEffects(playerInventory.playerNutrition);

			base.Use(playerInventory, slotIndex);
		}

		public override void OnUsed(PlayerInventoryModule playerInventory) { }

		public override string GetTooltipText() {
			return base.GetTooltipText().Replace("{NUTRITION}", nutrition > 0 ? "+" + nutrition.ToString() : nutrition.ToString());
		}
	}
}
