// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using Mirror;
using RedicionStudio.NetworkUtils;

namespace RedicionStudio.InventorySystem {

	[RequireComponent(typeof(Collider))]
	public class ItemDrop : NetworkBehaviour, INetInteractable<PlayerInventoryModule> {

		[SyncVar] public Item item;
		[SyncVar] public int amount;

		private void Start() {
			Instantiate(item.itemSO.modelPrefab).transform.SetParent(transform, false);
		}

		// (Server)
		public void OnServerInteract(PlayerInventoryModule player) {
			if (amount < 1) {
				NetworkServer.Destroy(gameObject);
			}

			if (player.Add(item, amount)) {
				amount = 0; // ?
				NetworkServer.Destroy(gameObject);
			}
		}

		// (Client)
		public void OnClientInteract(PlayerInventoryModule player) { }

		// (Client)
		public string GetInfoText() {
			if (amount > 0 && item.itemSO != null) { // ?
				return amount > 1 ? item.itemSO.uniqueName + " (" + amount + ')' : item.itemSO.uniqueName; // ?
			}
			return "???";
		}
	}
}
