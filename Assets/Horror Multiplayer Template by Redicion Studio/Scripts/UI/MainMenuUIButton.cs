// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace RedicionStudio
{
    public class MainMenuUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Main Menu UI Button")]
        [Space]

        [Header("Events")]
        public UnityEvent onPointerEnterEvent;
        public UnityEvent onPointerExitEvent;
        public UnityEvent onPointerClickEvent;
        [Space]

        [Header("Dependencies")]
        public MainMenuManager mainMenuManager;

        public GameObject buttonHoverGameObject;
        public GameObject buttonSelectedGameObject;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (buttonHoverGameObject != null)
                buttonHoverGameObject.SetActive(true);

            onPointerEnterEvent.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (buttonHoverGameObject != null)
                buttonHoverGameObject.SetActive(false);

            onPointerExitEvent.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            foreach (MainMenuUIButton uIButton in mainMenuManager.mainMenuUIButtons)
            {
                if (uIButton.buttonSelectedGameObject != null)
                    uIButton.buttonSelectedGameObject.SetActive(false);
            }

            onPointerClickEvent.Invoke();
        }
    }
}