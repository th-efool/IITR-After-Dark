// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using Mirror;

namespace RedicionStudio.InventorySystem {

	public abstract class UseableItemSO : ItemSO {

		[Space]
		public float cooldownInSeconds;
		public string cooldownTag;

		public virtual bool CanBeUsed(PlayerInventoryModule playerInventory, int slotIndex) {
			return playerInventory.GetCooldown(cooldownTag.GetStableHashCode()) <= 0f;
		}

		/// <summary>
		/// (Server)
		/// </summary>
		public virtual void Use(PlayerInventoryModule playerInventory, int slotIndex) {
			if (cooldownInSeconds > 0f) {
				playerInventory.SetCooldown(cooldownTag.GetStableHashCode(), cooldownInSeconds);
			}
		}

		/// <summary>
		/// (Client)
		/// </summary>
		public abstract void OnUsed(PlayerInventoryModule playerInventory);

		protected override void OnValidate() {
			base.OnValidate();

			if (stackSize > 1) {
				stackSize = 1;
			}
		}
	}
}
