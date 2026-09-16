// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using RedicionStudio.UIUtils;

namespace RedicionStudio.InventorySystem {

	public class SyncDictionaryIntDouble : SyncDictionary<int, double> { }

	public class PlayerInventoryModule : Inventory {

		[Header("Player Modules")]
		public Player player;
		public PlayerNutritionModule playerNutrition;

		[Space]
		public AudioSource audioSource;

        [Space]
        public ManageTPController TPControllerManager;

        [Space]
        public GameObject cartridgeEjectPrefab;
        Transform _cartridgeEjectSpawnPointPosition;

        [Space]
        public bool inPropertyArea = false;

        [Space]
        public bool inShop = false;

        [Space]
        public bool inCar = false;

        [Space]
        public bool usesParachute = false;

        [Space]
        public bool isAiming = false;

        private RoomManager _roomManager;

        public ChatSystem chatWindow;

        public void LoadInventory() {
			for (int i = 0; i < 67; i++) {
				slots.Add(new ItemSlot());
			}

#if UNITY_SERVER || UNITY_EDITOR // ?
			MasterServer.MSClient.GetInventory(player.id, (inventoryData) => {
				if (inventoryData == null || inventoryData.Length < 1) { // ?
					return;
				}
				ItemSlot slot;
				for (int i = 0; i < inventoryData.Length; i++) {
					slot = new ItemSlot();
					Debug.Log(inventoryData[i].hash);
					slot.item.hash = inventoryData[i].hash;
					slot.amount = inventoryData[i].amount;
					slot.item.currentShelfLifeInSeconds = inventoryData[i].shelfLife;
					slots[i] = slot;
				}
			});
#endif
		}

		[Command]
		private void CmdInventoryMerge(int from, int to) {
			if (0 <= from && from < slots.Count &&
				0 <= to && to < slots.Count &&
				from != to) {
				ItemSlot fromSlot = slots[from];
				ItemSlot toSlot = slots[to];
				if (fromSlot.amount > 0 && toSlot.amount > 0) {
					if (fromSlot.item.Equals(toSlot.item)) {
						int put = toSlot.IncreaseBy(fromSlot.amount);
						fromSlot.DecreaseBy(put);
						slots[from] = fromSlot;
						slots[to] = toSlot;
					}
				}
			}
		}

		[Command]
		private void CmdInventorySplit(int from, int to) {
			if (0 <= from && from < slots.Count &&
				0 <= to && to < slots.Count &&
				from != to) {
				ItemSlot fromSlot = slots[from];
				ItemSlot toSlot = slots[to];
				if (fromSlot.amount >= 2 && toSlot.amount == 0) {
					toSlot = fromSlot;

					toSlot.amount = fromSlot.amount / 2;
					fromSlot.amount -= toSlot.amount;

					slots[from] = fromSlot;
					slots[to] = toSlot;
				}
			}
		}

		[Command]
		private void CmdSwapInventoryInventory(int from, int to) {
			if (0 <= from && from < slots.Count &&
				0 <= to && to < slots.Count &&
				from != to) {
				ItemSlot fromSlot = slots[from];
				if ((to == 0 && !(fromSlot.item.itemSO is PerkSO)) ||
					(to == 1 && !(fromSlot.item.itemSO is PerkSO)) ||
                    (to == 2 && !(fromSlot.item.itemSO is PerkSO)) ||
                    (to == 3 && !(fromSlot.item.itemSO is PerkSO)) ||
                    (to == 4 && !(fromSlot.item.itemSO is OutfitItemSO))){
                    return;
				}
				slots[from] = slots[to];
				slots[to] = fromSlot;
			}
		}

		private void Start() {
			if (isLocalPlayer) {
				UIPlayerInventory.playerInventory = this;
				slots.Callback += Slots_Callback;
				UIPlayerInventory.InstanceRefresh();

				UIDragAndDrop.OnDragAndClearAction = CmdDropItem;
				UIDragAndDrop.OnDragAndDropAction = (from, to) => {
					if (slots[from].amount > 0 && slots[to].amount > 0 &&
					slots[from].item.Equals(slots[to].item)) {
						CmdInventoryMerge(from, to);
					}
					else if (_keyboard != null && _keyboard.shiftKey.isPressed) {
						CmdInventorySplit(from, to);
					}
					else {
						CmdSwapInventoryInventory(from, to);
					}
				};
			}
            if (chatWindow == null)
                chatWindow = GameObject.FindGameObjectWithTag("ChatWindow").GetComponent<ChatSystem>();
        }

		private void Slots_Callback(SyncList<ItemSlot>.Operation op, int itemIndex, ItemSlot oldItem, ItemSlot newItem) {
			UIPlayerInventory.InstanceRefresh();
		}

		/// <summary>
		/// (Server)
		/// </summary>
		private void DropItem(Item item, int amount) {
			Vector2 randomPoint = Random.insideUnitCircle * 2f;
			Vector3 position = new Vector3(transform.position.x + randomPoint.x, transform.position.y, transform.position.z + randomPoint.y);

			GameObject gO = Instantiate(ConfigurationSO.Instance.itemDropPrefab, position, Quaternion.identity);
			ItemDrop itemDrop = gO.GetComponent<ItemDrop>();
			itemDrop.item = item;
			itemDrop.amount = amount;
			NetworkServer.Spawn(gO);
		}

		/// <summary>
		/// (Server)
		/// </summary>
		private void DropItemAndClearSlot(int slotIndex) {
			ItemSlot slot = slots[slotIndex];
			DropItem(slot.item, slot.amount);
			slot.amount = 0;
			slots[slotIndex] = slot;
		}

		[Command]
		public void CmdDropItem(int index) {
            if(index > 4) // Ensures that no item can be dropped as long as it is equipped.
            {
                if (0 <= index && index < slots.Count && slots[index].amount > 0){
                    DropItemAndClearSlot(index);
                }
            }
		}

		#region Cooldowns

		private Dictionary<int, double> _local_itemCooldowns = new Dictionary<int, double>();
		private readonly SyncDictionaryIntDouble _itemCooldowns = new SyncDictionaryIntDouble();

		public void SetCooldown(int itemSOHash, float cooldownInSeconds) {
			double cooldownEndTime = NetworkTime.time + cooldownInSeconds;

			if (isClient && !isServer) {
				_local_itemCooldowns[itemSOHash] = cooldownEndTime;
			}
			else {
				_itemCooldowns[itemSOHash] = cooldownEndTime;
			}
		}

		public float GetCooldown(int itemSOHash) {
			double cooldownEndTime;

			if (isClient && !isServer) {
				if (_local_itemCooldowns.TryGetValue(itemSOHash, out cooldownEndTime)) {
					return NetworkTime.time >= cooldownEndTime ? 0f : (float)(cooldownEndTime - NetworkTime.time);
				}
			}

			if (_itemCooldowns.TryGetValue(itemSOHash, out cooldownEndTime)) {
				return NetworkTime.time >= cooldownEndTime ? 0f : (float)(cooldownEndTime - NetworkTime.time);
			}

			return 0f;
		}

		#endregion

		[ClientRpc]
		public void RpcOnItemUsed(Item item) {
			if (item.itemSO is UseableItemSO usableItemSO) {
				usableItemSO.OnUsed(this);
			}
		}

		[Command]
		public void CmdUseItem(int slotIndex) {
			if (0 <= slotIndex && slotIndex < slots.Count && slots[slotIndex].amount > 0 && slots[slotIndex].item.itemSO is UseableItemSO usableItemSO && usableItemSO.CanBeUsed(this, slotIndex)) {
				usableItemSO.Use(this, slotIndex);
			}
		}

        [Command]
        public void CmdAim()
        {
            TPControllerManager.aimValue = 1;
        }

        public string currentPlayerUsername()
        {
            return GetComponent<Player>().username;
        }

        private static Keyboard _keyboard;
		private static Mouse _mouse;

		public static bool inMenu;
        public static bool inWeaponWheel;

        private void OnDestroy() {
			if (isLocalPlayer) {
				UIPlayerInventory.playerInventory = null;
				slots.Callback -= Slots_Callback;
			}
		}

		private int _index;
		private double _interval = 60f;
		private double _lastTime;
		private ItemSlot _slot;
		private void Update() {
            if (_roomManager == null)
                _roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();
            if (isServer) {
				if (NetworkTime.time >= _lastTime + _interval) {
					for (_index = 0; _index < slots.Count; _index++) {
						_slot = slots[_index];
						if (_slot.amount > 0 && _slot.item.itemSO != null && _slot.item.itemSO is ConsumableItemSO) {
							if (_slot.item.currentShelfLifeInSeconds > 0f) {
								_slot.item.currentShelfLifeInSeconds -= (float)_interval;
							}
							else {
								_slot.item = new Item();
								_slot.amount = 0;
							}
							slots[_index] = _slot;
						}
					}
					_lastTime = NetworkTime.time;
				}
			}

			if (!isLocalPlayer) {
				return;
			}

			_keyboard = Keyboard.current;
			_mouse = Mouse.current;

			if (_keyboard == null || _mouse == null) {
				return;
			}

            /*if (_keyboard.tabKey.wasPressedThisFrame && !inShop) {
				inMenu = !inMenu;
				if (inMenu) {
					if (BSystem.BSystem.inMenu) {
						BSystem.BSystem.inMenu = false;
						BSystemUI.Instance.SetActive(false);

					}
					UIPlayerInventory.SetActive(true);
                    UIPlayerInventory.InventoryUI.SetActive(true);
                    TPController.TPCameraController.LockCursor(false);
				}
				else {
					UIPlayerInventory.SetActive(false);
                    UIPlayerInventory.InventoryUI.SetActive(false);
                    TPController.TPCameraController.LockCursor(true);
				}
			}*/

            _slot = slots[0];
			/*if (!BSystem.BSystem.inMenu && !inMenu && !inWeaponWheel && !GetComponent<EmoteWheel>().inEmoteWheel && !inPropertyArea && !inShop && !inCar && !usesParachute && !this.GetComponent<EmoteWheel>().isPlayingAnimation && isAiming && !this.GetComponent<Health>().isDeath && _input.shoot && _slot.amount > 0 && _slot.item.itemSO != null && _slot.item.itemSO is WeaponItemSO weaponItemSO) {
				_interval = weaponItemSO.cooldownInSeconds;
				if (NetworkTime.time >= _lastTime + _interval) {
					if (weaponItemSO.automatic) {
						CmdUseItem(0);
					}
					else if (_mouse.leftButton.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame) {
						CmdUseItem(0);
                    }
					_lastTime = NetworkTime.time;
				}
			}
            //Aim
            if (!BSystem.BSystem.inMenu & !inMenu & !inPropertyArea & !inShop & !inCar & !usesParachute & !this.GetComponent<Health>().isDeath & !this.GetComponent<EmoteWheel>().isPlayingAnimation & _input.aim & _slot.amount > 0 & _slot.item.itemSO != null & _slot.item.itemSO is WeaponItemSO)
            {
                CmdAim();
            }*/
        }

		[Space]
		[SerializeField] private Transform _gFX;

		private void LateUpdate() {
            if(chatWindow == null)
                chatWindow = chatWindow = GameObject.FindGameObjectWithTag("ChatWindow").GetComponent<ChatSystem>();
            if (isServer) {
				return;
			}

			for (int i = 0; i < _gFX.childCount; i++) {
				_gFX.GetChild(i).gameObject.SetActive(false);
                _gFX.GetChild(i).GetComponent<WeaponManager>().enabled = false;
            }
			if (!this.GetComponent<Health>().isDeath && !inCar && !usesParachute && !this.GetComponent<EmoteWheel>().isPlayingAnimation && slots[0].amount > 0 && slots[0].item.itemSO != null) {
                this.GetComponent<Animator>().SetLayerWeight(1, 1);
                for (int i = 0; i < _gFX.childCount; i++) {
					if (_gFX.GetChild(i).name == slots[0].item.itemSO.uniqueName) {
						_gFX.GetChild(i).gameObject.SetActive(true);
                        _gFX.GetChild(i).GetComponent<WeaponManager>().enabled = true;
                        this.GetComponent<ManageTPController>().CurrentWeaponBulletSpawnPoint = _gFX.GetChild(i).GetComponent<WeaponManager>().CurrentWeaponBulletSpawnPoint;
                        this.GetComponent<ManageTPController>().CurrentCartridgeEjectSpawnPoint = _gFX.GetChild(i).GetComponent<WeaponManager>().CartridgeEjectEffectSpawnPoint;
                    }
				}
			}
            else
            {
                this.GetComponent<Animator>().SetLayerWeight(1, 0);
                this.GetComponent<ManageTPController>().PlayerRig.weight = 0;
            }
		}

        public void SwapInventoryInventory(int from, int to)
        {
            CmdSwapInventoryInventory(from, to);
        }

        public void ToggleInventory()
        {
            if (!inShop)
            {
                inMenu = !inMenu;
                if (inMenu)
                {
                    if (BSystem.BSystem.inMenu)
                    {
                        BSystem.BSystem.inMenu = false;
                        BSystem.BSystemUI.Instance.SetActive(false);

                    }
                    UIPlayerInventory.SetActive(true);
                    UIPlayerInventory.InventoryUI.SetActive(true);
                    TPCameraController.LockCursor(false);
                    /*foreach (Transform child in UIPlayerInventory._slotsContent)
                    {
                        if (child.GetComponent<RedicionStudio.InventorySystem.UISlot>().item.itemSO.itemType == RedicionStudio.InventorySystem.ItemSO.ItemType.Killer || child.GetComponent<RedicionStudio.InventorySystem.UISlot>().item.itemSO.itemType == RedicionStudio.InventorySystem.ItemSO.ItemType.Outfit)
                        {
                            child.gameObject.SetActive(false);
                        }
                    }*/
                }
                else
                {
                    UIPlayerInventory.SetActive(false);
                    UIPlayerInventory.InventoryUI.SetActive(false);
                    //TPController.TPCameraController.LockCursor(true);
                }
            }
        }
	}
}
