// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedicionStudio
{
    public abstract class UIClickable : MonoBehaviour, IPointerDownHandler
    {

        protected abstract void OnPressed();

        public void OnPointerDown(PointerEventData eventData) => OnPressed();
    }
}
