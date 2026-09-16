// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using RedicionStudio.InventorySystem;

namespace RedicionStudio.BSystem {

	public class BSystem : MonoBehaviour {

		public static BSystem Instance { get; private set; }

		private static Transform _camera;

		[SerializeField] private LayerMask _defaultMask;
		[SerializeField] private LayerMask _objectMask;
		[SerializeField] private LayerMask _floorMask;
		[SerializeField] private LayerMask _wallMask;

		private void Awake() {
			if (Instance == null) {
				Instance = this;
			}
			else {
				throw new UnityException("Instance");
			}

			//_camera = FindObjectOfType<Camera>().transform;
            _camera = GameObject.Find("MainCamera").transform;

            OnCurrentPlaceableSOChangedAction += () => {
				if (currentPlaceableSO == null) {
					_raycastForwardMask = _defaultMask;
					return;
				}
				switch (currentPlaceableSO.type) {
					case PlaceableSO.Type.Object:
						_raycastForwardMask = _objectMask;
						return;
					case PlaceableSO.Type.Floor:
						_raycastForwardMask = _floorMask;
						return;
					case PlaceableSO.Type.Wall:
						_raycastForwardMask = _wallMask;
						return;
				}
			};

			currentPlaceableSO = null;
			OnCurrentPlaceableSOChangedAction.Invoke();
		}

		private const float _CameraForwardOffset = 2f;
		private const float _MaxPlacementDistance = 15f;

		private static LayerMask _raycastForwardMask;

		private static RaycastHit _forwardHitInfo;
		private static bool _forwardHit;
		private static void RaycastForward() {
			_forwardHit = Physics.Raycast(_camera.position + _camera.forward * _CameraForwardOffset, _camera.forward, out _forwardHitInfo, _MaxPlacementDistance, _raycastForwardMask); // ?
		}

		public static PlaceableSO currentPlaceableSO;

		public static Vector3 position;
		public static Quaternion rotation;

		private static void Rotate() {
			rotation.eulerAngles += new Vector3(0f,
				(currentPlaceableSO.type == PlaceableSO.Type.Wall) ? 180f : 90f,
				0f);
		}

		private static bool _forwardHitIsTrigger;
		public static bool canPlace;

		private static GameObject _forwardHitGO;

		private static Keyboard _keyboard;
		private static Mouse _mouse;

		public static Action OnPlaceRequestAction;

		[SerializeField] private LayerMask _placeObstacleMask;

		private static PlaceableObject _editTarget;
		private static bool _forwardHitIsPlaceable;
		public static bool editMode;
		public static bool inMenu;

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
		private void Update() {
			if (Player.localPlayer == null) {
				return;
			}

			_keyboard = Keyboard.current;
			_mouse = Mouse.current;

			if (_keyboard == null || _mouse == null) {
				return;
			}
            // Responsible for opening the build menu.
            /*if (_keyboard.xKey.wasPressedThisFrame) {
				if (!editMode || inMenu) {
					inMenu = !inMenu;
					if (inMenu) {
						if (RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu) {
							RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu = false;
							RedicionStudio.InventorySystem.UIPlayerInventory.SetActive(false);
						}
					}
					TPController.TPCameraController.LockCursor(!inMenu);
					BSystemUI.Instance.SetActive(inMenu);
				}
				return;
			}*/

            if (inMenu) {
				return;
			}

			RaycastForward();

			if (!_forwardHit) {
				canPlace = false;
				return;
			}

			position = _forwardHitInfo.point;
			if (currentPlaceableSO != null) {
				position += Vector3.up * (currentPlaceableSO.GetSize().y / 2f);
			}

			if (_forwardHitInfo.transform.gameObject != _forwardHitGO) {
				_forwardHitGO = _forwardHitInfo.transform.gameObject;

				_forwardHitIsTrigger = LayerMask.LayerToName(_forwardHitGO.layer).Contains("Trigger");
				_forwardHitIsPlaceable = LayerMask.LayerToName(_forwardHitGO.layer).Contains("Placeable");
			}

			if (_forwardHitIsTrigger) {
				position = _forwardHitGO.transform.position;
				rotation = _forwardHitGO.transform.rotation;
			}

			if (currentPlaceableSO == null && _forwardHitIsPlaceable) {
				if (_keyboard.eKey.wasPressedThisFrame) {
					_editTarget = _forwardHitGO.GetComponentInParent<PlaceableObject>();
					if (_editTarget.ownerId == Player.localPlayer.id) {
						editMode = true;
						_editTarget.gameObject.SetActive(false); // ?
						currentPlaceableSO = _editTarget.placeableSO;
						OnCurrentPlaceableSOChangedAction.Invoke();
					}
				}
				return;
			}

			canPlace = currentPlaceableSO != null && (currentPlaceableSO.CanBePlacedOnGround || _forwardHitIsTrigger) &&
				!currentPlaceableSO.CheckBox(position, rotation, Instance._placeObstacleMask); // ?

			if (_keyboard.rKey.wasPressedThisFrame) {
				Rotate();
			}

			if (editMode) {
				if (_keyboard.escapeKey.wasPressedThisFrame) {
					Player.localPlayer.CmdEditDelete(_editTarget.netId);
					_editTarget.gameObject.SetActive(true); // ?
					editMode = false;
					currentPlaceableSO = null;
					OnCurrentPlaceableSOChangedAction.Invoke();
				}
				if (_mouse.leftButton.wasPressedThisFrame) {
					Player.localPlayer.CmdEdit(_editTarget.netId, position, rotation);
					_editTarget.gameObject.SetActive(true); // ?
					editMode = false;
					currentPlaceableSO = null;
					OnCurrentPlaceableSOChangedAction.Invoke();
				}
				return;
			}

			if (!canPlace) {
				return;
			}

			if (_mouse.leftButton.wasPressedThisFrame) {
				OnPlaceRequestAction.Invoke();
			}
		}
#endif

		[Space]
		public GameObject placeableObjectPrefab;

		public static Action OnCurrentPlaceableSOChangedAction;
	}
}
