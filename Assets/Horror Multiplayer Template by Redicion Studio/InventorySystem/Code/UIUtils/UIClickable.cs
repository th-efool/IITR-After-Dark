// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedicionStudio.UIUtils {

	public abstract class UIClickable : MonoBehaviour, IPointerClickHandler {

		public bool interactable = true;

		protected abstract void OnPointerDownOverride();

		public void OnPointerClick(PointerEventData eventData) {
			if (!interactable) {
				return;
			}

			OnPointerDownOverride();
		}
	}
}
