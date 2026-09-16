// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

namespace RedicionStudio.BSystem {

	public class PlaceableObject : NetworkBehaviour {

		[SyncVar] public int ownerId;
		[SyncVar, HideInInspector] public string placeableSOUniqueName;

		// (Server)
		public static GameObject Place(int ownerId, string placeableSOUniqueName, Vector3 position, Quaternion rotation) {
			GameObject gO = Instantiate(BSystem.Instance.placeableObjectPrefab);
			gO.name = placeableSOUniqueName;
			gO.GetComponent<PlaceableObject>().ownerId = ownerId; // TODO: cache
			gO.GetComponent<PlaceableObject>().placeableSOUniqueName = placeableSOUniqueName;
			gO.transform.position = position;
			gO.transform.rotation = rotation;
			NetworkServer.Spawn(gO);
			return gO;
		}

		[HideInInspector] public PlaceableSO placeableSO;

		private Vector3 _BackwardSideOffset => Vector3.back * placeableSO.GetSize().z / 2f;
		private Vector3 _LeftSideOffset => Vector3.left * placeableSO.GetSize().x / 2f;
		private Vector3 _ForwardSideOffset => Vector3.forward * placeableSO.GetSize().z / 2f;
		private Vector3 _RightSideOffset => Vector3.right * placeableSO.GetSize().x / 2f;
		private Vector3 _UpSideOffset => Vector3.up * placeableSO.GetSize().y / 2f;

		private static GameObject _triggerGO;
		private static BoxCollider _triggerGOBoxCollider;

		private static readonly Vector3 _scalarAdjustment = new Vector3(.04f, 0f, .04f);

		private void CreateTrigger(PlaceableSO.Direction direction, PlaceableSO.Type type, Vector3 localPosition) {
			_triggerGO = new GameObject(direction.ToString() + type.ToString() + "Trigger");
			_triggerGO.transform.parent = transform;
			_triggerGO.transform.localPosition = localPosition;
			_triggerGO.transform.localEulerAngles = PlaceableSO.GetDirectionEulerAngleY(direction);
			_triggerGOBoxCollider = _triggerGO.AddComponent<BoxCollider>();
			_triggerGOBoxCollider.size = PlaceableSO.GetSize(type) + _scalarAdjustment;
			_triggerGOBoxCollider.isTrigger = true;
			_triggerGO.layer = LayerMask.NameToLayer(type.ToString() + "Trigger"); // ?
		}

		private static readonly Vector3 _forwardThickness = new Vector3(0f, 0f, PlaceableSO.Thickness);
		private static readonly Vector3 _rightThickness = new Vector3(PlaceableSO.Thickness, 0f, 0f);
		private static readonly Vector3 _forwardHalfSize = new Vector3(0f, 0f, PlaceableSO.Size / 2f);
		private static readonly Vector3 _rightHalfSize = new Vector3(PlaceableSO.Size / 2f, 0f, 0f);
		private static readonly Vector3 _upHalfSize = new Vector3(0f, PlaceableSO.Size / 2f, 0f);
		private static readonly Vector3 _upHalfThickness = new Vector3(0f, PlaceableSO.Thickness / 2f, 0f);

		private void CreateFloorTriggerColliders() {
			CreateTrigger(PlaceableSO.Direction.Backward, PlaceableSO.Type.Floor, _BackwardSideOffset + _forwardThickness - _forwardHalfSize);
			CreateTrigger(PlaceableSO.Direction.Left, PlaceableSO.Type.Floor, _LeftSideOffset + _rightThickness - _rightHalfSize);
			CreateTrigger(PlaceableSO.Direction.Forward, PlaceableSO.Type.Floor, _ForwardSideOffset - _forwardThickness + _forwardHalfSize);
			CreateTrigger(PlaceableSO.Direction.Right, PlaceableSO.Type.Floor, _RightSideOffset - _rightThickness + _rightHalfSize);

			CreateTrigger(PlaceableSO.Direction.Backward, PlaceableSO.Type.Wall, _BackwardSideOffset + (_forwardThickness / 2f) + _UpSideOffset + _upHalfSize);
			CreateTrigger(PlaceableSO.Direction.Left, PlaceableSO.Type.Wall, _LeftSideOffset + (_rightThickness / 2f) + _UpSideOffset + _upHalfSize);
			CreateTrigger(PlaceableSO.Direction.Forward, PlaceableSO.Type.Wall, _ForwardSideOffset - (_forwardThickness / 2f) + _UpSideOffset + _upHalfSize);
			CreateTrigger(PlaceableSO.Direction.Right, PlaceableSO.Type.Wall, _RightSideOffset - (_rightThickness / 2f) + _UpSideOffset + _upHalfSize);
		}

		private void CreateWallTriggerColliders() {
			CreateTrigger(PlaceableSO.Direction.Backward, PlaceableSO.Type.Floor, _ForwardSideOffset - _forwardHalfSize + _UpSideOffset + _upHalfThickness);
			CreateTrigger(PlaceableSO.Direction.Forward, PlaceableSO.Type.Floor, _BackwardSideOffset + _forwardHalfSize + _UpSideOffset + _upHalfThickness);

			CreateTrigger(PlaceableSO.Direction.Backward, PlaceableSO.Type.Wall, _UpSideOffset + _upHalfSize);
		}

		private void Start() {
			placeableSO = PlaceableSO.GetPlaceableSO(placeableSOUniqueName);

			_ = Instantiate(placeableSO.modelPrefab, transform);

			switch (placeableSO.type) {
				default:
					// ?
					gameObject.layer = LayerMask.NameToLayer("Placeable");
					Transform[] gOs = GetComponentsInChildren<Transform>();
					for (int i = 0; i < gOs.Length; i++) {
						gOs[i].gameObject.layer = LayerMask.NameToLayer("Placeable");
					}
					break;
				case PlaceableSO.Type.Floor:
					// ?
					gameObject.layer = LayerMask.NameToLayer("PlaceableFloor");
					gOs = GetComponentsInChildren<Transform>();
					for (int i = 0; i < gOs.Length; i++) {
						gOs[i].gameObject.layer = LayerMask.NameToLayer("PlaceableFloor");
					}
					break;
			}

			if (placeableSO.type == PlaceableSO.Type.Floor) {
				CreateFloorTriggerColliders();
				return;
			}

			if (placeableSO.type == PlaceableSO.Type.Wall) {
				CreateWallTriggerColliders();
			}
		}
	}
}
