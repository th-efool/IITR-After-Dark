// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;

namespace RedicionStudio.InventorySystem {

    //[CreateAssetMenu(fileName = "New Weapon Item SO", menuName = "Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Weapon")]
    public class WeaponItemSO : UseableItemSO {

		[Header("Weapon")]
		[SerializeField] private AmmoItemSO _requiredAmmo;
		public bool automatic;

		[Space]
		[SerializeField] private AudioClip _shotSound;

        public override bool CanBeUsed(PlayerInventoryModule playerInventory, int slotIndex) {
			return slotIndex == 0 &&
				base.CanBeUsed(playerInventory, slotIndex) &&
				(_requiredAmmo == null || (playerInventory.slots[1].amount > 0 && playerInventory.slots[1].item.itemSO.name == _requiredAmmo.name));
		}

		public override void Use(PlayerInventoryModule playerInventory, int slotIndex) {
			if (_requiredAmmo != null) {
				ItemSlot slotB = playerInventory.slots[1];
				slotB.amount--;
				playerInventory.slots[1] = slotB;
			}

			base.Use(playerInventory, slotIndex);

			playerInventory.RpcOnItemUsed(playerInventory.slots[0].item);
		}

		public override void OnUsed(PlayerInventoryModule playerInventory) {
			Debug.Log("WeaponItemSO->OnUsed");
            if(_shotSound != null)
			    playerInventory.audioSource.PlayOneShot(_shotSound);
		}

		public override string GetTooltipText() {
			return base.GetTooltipText().Replace("{REQUIRED_AMMO}", _requiredAmmo != null ? _requiredAmmo.name : "Null");
		}

		protected override void OnValidate() {
			base.OnValidate();

			stackSize = 1;
		}
	}
}
