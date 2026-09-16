// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using RedicionStudio.InventorySystem;

namespace RedicionStudio.BSystem {

	public class BSystemDebug : MonoBehaviour {

		public static Keyboard _keyboard;

		[SerializeField]
		PlaceableSO _digit1PlaceableSO,
			_digit2PlaceableSO,
			_digit3PlaceableSO,
			_digit4PlaceableSO,
			_digit5PlaceableSO,
			_digit6PlaceableSO,
			_digit7PlaceableSO,
			_digit8PlaceableSO,
			_digit9PlaceableSO;

		private static void Choose(PlaceableSO placeableSO) {
			BSystem.currentPlaceableSO = placeableSO;
			BSystem.OnCurrentPlaceableSOChangedAction?.Invoke(); // ?
		}

		private void Update() {
			_keyboard = Keyboard.current;

			if (_keyboard == null) {
				return;
			}

			if (Player.localPlayer == null || BSystem.editMode) {
				return;
			}

			if (_keyboard.digit1Key.wasPressedThisFrame) {
				Choose(_digit1PlaceableSO);
			}

			if (_keyboard.digit2Key.wasPressedThisFrame) {
				Choose(_digit2PlaceableSO);
			}

			if (_keyboard.digit3Key.wasPressedThisFrame) {
				Choose(_digit3PlaceableSO);
			}

			if (_keyboard.digit4Key.wasPressedThisFrame) {
				Choose(_digit4PlaceableSO);
			}

			if (_keyboard.digit5Key.wasPressedThisFrame) {
				Choose(_digit5PlaceableSO);
			}

			if (_keyboard.digit6Key.wasPressedThisFrame) {
				Choose(_digit6PlaceableSO);
			}

			if (_keyboard.digit7Key.wasPressedThisFrame) {
				Choose(_digit7PlaceableSO);
			}

			if (_keyboard.digit8Key.wasPressedThisFrame) {
				Choose(_digit8PlaceableSO);
			}

			if (_keyboard.digit9Key.wasPressedThisFrame) {
				Choose(_digit9PlaceableSO);
			}
		}
	}
}
