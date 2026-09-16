// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RedicionStudio.BSystem {

	public class BSystemPreview : MonoBehaviour {

		public static BSystemPreview Instance { get; private set; } // ?

		private static Transform _transform;

		private void Awake() {
			if (Instance == null) {
				Instance = this;
			}
			else {
				throw new UnityException("Instance");
			}

			_transform = transform;

			BSystem.OnCurrentPlaceableSOChangedAction += UpdateModel; // ?
		}

		private static Transform _model;

		[SerializeField] private Material _material;
		[SerializeField] private Material _materialEdit;

		public static void UpdateModel() {
			if (_model != null) {
				Destroy(_model.gameObject);
			}

			if (BSystem.currentPlaceableSO == null) {
				return;
			}

			_model = Instantiate(BSystem.currentPlaceableSO.modelPrefab, _transform).transform;

			Collider[] colliders = _model.GetComponentsInChildren<Collider>();
			for (int i = 0; i < colliders.Length; i++) {
				colliders[i].enabled = false;
			}

			MeshRenderer[] meshRenderers = _model.GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < meshRenderers.Length; i++) {
				meshRenderers[i].material = BSystem.editMode ? Instance._materialEdit : Instance._material;
			}
		}

		private void LateUpdate() {
			if (_model == null) {
				return;
			}

			_model.gameObject.SetActive(BSystem.canPlace); // ?
			_model.position = BSystem.position;
			_model.rotation = BSystem.rotation;
		}
	}
}
