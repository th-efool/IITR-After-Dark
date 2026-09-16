// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace RedicionStudio.UIUtils {

	public class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

		[SerializeField] private GameObject _tooltipPrefab;
		[Space, SerializeField] private float _delay;
		[Space, TextArea(1, 32)] public string text = string.Empty;

		private GameObject _currentTooltipGO;

		public bool Active => _currentTooltipGO != null && _currentTooltipGO.activeSelf; // ?

		private void CreateTooltip() {
			_currentTooltipGO = Instantiate(_tooltipPrefab, transform.position, Quaternion.identity);
			_currentTooltipGO.transform.SetParent(transform.root, true);
			_currentTooltipGO.transform.SetAsLastSibling();
			_currentTooltipGO.GetComponentInChildren<TextMeshProUGUI>().text = text;
		}

		public void OnPointerEnter(PointerEventData eventData) {
			Invoke(nameof(CreateTooltip), _delay);
		}

		private void Destroy() {
			CancelInvoke(nameof(CreateTooltip));
			if (_currentTooltipGO != null) { // ?
				Destroy(_currentTooltipGO);
			}
		}

		public void OnPointerExit(PointerEventData eventData) {
			Destroy();
		}

		private void OnEnable() {
			Destroy();
		}

		private void OnDisable() {
			Destroy();
		}
	}
}
