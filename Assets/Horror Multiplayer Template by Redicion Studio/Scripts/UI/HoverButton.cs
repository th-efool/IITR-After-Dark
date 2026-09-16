using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RedicionStudio
{
    public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject hoverElement;

        private void Start()
        {
            if (hoverElement != null)
            {
                hoverElement.SetActive(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (hoverElement != null)
            {
                hoverElement.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (hoverElement != null)
            {
                hoverElement.SetActive(false);
            }
        }
    }
}
