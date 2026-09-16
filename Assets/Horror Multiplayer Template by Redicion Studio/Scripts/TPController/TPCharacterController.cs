// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Mirror;

namespace RedicionStudio {

	[RequireComponent(typeof(CharacterController))]
	public class TPCharacterController : NetworkBehaviour {

		private static CharacterController _characterController;

		private static Camera _camera;
		private static CinemachineFreeLook _cinemachineFreeLook;

		[SerializeField] private Transform _lookAt;

		private void Init() {
			_characterController = GetComponent<CharacterController>();

			_camera = FindObjectOfType<Camera>();
			_cinemachineFreeLook = FindObjectOfType<CinemachineFreeLook>();
			_cinemachineFreeLook.Follow = transform;
			_cinemachineFreeLook.LookAt = _lookAt;
		}

		private static Keyboard _keyboard;

		private static float _verticalInput, _horizontalInput;

		private static Vector3 _inputDirection;

		private void Update() {
			if (!isLocalPlayer) {
				return;
			}

			_keyboard = Keyboard.current;

			if (_keyboard == null) {
				return;
			}

			if (_characterController == null) {
				Init();
			}

			_verticalInput = 0f;
			if (_keyboard.wKey.isPressed) {
				_verticalInput += 1f;
			}
			if (_keyboard.sKey.isPressed) {
				_verticalInput -= 1f;
			}
			_horizontalInput = 0f;
			if (_keyboard.aKey.isPressed) {
				_horizontalInput -= 1f;
			}
			if (_keyboard.dKey.isPressed) {
				_horizontalInput += 1f;
			}

			_inputDirection = new Vector3(_horizontalInput, 0f, _verticalInput);
			_inputDirection.Normalize();
			if (_inputDirection.magnitude >= .1f) {
				transform.rotation = Quaternion.Euler(0f,
					Mathf.Atan2(_inputDirection.x, _inputDirection.z) * Mathf.Rad2Deg + _camera.transform.rotation.eulerAngles.y,
					0f);
				_characterController.Move(transform.rotation * Vector3.forward * 6f * Time.deltaTime); // TODO: Speed
			}
		}
	}
}
