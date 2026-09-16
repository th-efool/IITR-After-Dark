// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedicionStudio
{
    public class UIDraggableWindow : MonoBehaviour, IInitializePotentialDragHandler, IPointerDownHandler, IDragHandler
    {

        [SerializeField] private Canvas _canvas;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void OnPointerDown(PointerEventData _)
        {
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
    }
}
