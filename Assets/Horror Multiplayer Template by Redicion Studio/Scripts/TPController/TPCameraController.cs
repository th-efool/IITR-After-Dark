// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Mirror;

namespace RedicionStudio {

	[RequireComponent(typeof(CinemachineFreeLook))]
	public class TPCameraController : MonoBehaviour {

		private static CinemachineFreeLook _cinemachineFreeLook;

        private GameObject _localPlayer;

		[SerializeField] private float _mouseSensitivity = .9f;

		public static void LockCursor(bool value) {
			if (value) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				return;
			}
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

		public static void ResetAxis() {
			_cinemachineFreeLook.m_YAxis.Value = .5f;
			_cinemachineFreeLook.m_XAxis.Value = 0f;
		}

		private void Awake() {
			_cinemachineFreeLook = GetComponent<CinemachineFreeLook>();
        }

		private static Mouse _mouse;

		private void Update() {
			_mouse = Mouse.current;

            if(_localPlayer == null)
                _localPlayer = NetworkClient.localPlayer.gameObject;

            if (_mouse == null || _cinemachineFreeLook.LookAt == null || BSystem.BSystemUI.Instance.Active || RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu || RedicionStudio.InventorySystem.PlayerInventoryModule.inWeaponWheel || _localPlayer.GetComponent<EmoteWheel>().inEmoteWheel) {
				return;
			}

			Vector2 mouseDelta = _mouse.delta.ReadUnprocessedValue();
			_cinemachineFreeLook.m_YAxis.Value -= mouseDelta.y * _mouseSensitivity / 100f; // TODO: ?
			_cinemachineFreeLook.m_XAxis.Value += mouseDelta.x * _mouseSensitivity;
		}
	}
}
