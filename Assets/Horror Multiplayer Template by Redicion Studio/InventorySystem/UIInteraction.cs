// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using TMPro;

namespace RedicionStudio.InventorySystem
{
    public class UIInteraction : MonoBehaviour
    {

        [SerializeField] private GameObject _content;
        [SerializeField] private TextMeshProUGUI _interactionText;

        public static PlayerInteractionModule playerInteraction;

        private void Update()
        {
            if (playerInteraction == null || playerInteraction.currentInteractable == null || RedicionStudio.InventorySystem.PlayerInventoryModule.inMenu)
            {
                _content.SetActive(false);
                return;
            }

            _content.SetActive(true);

            _interactionText.text = playerInteraction.currentInteractable.GetInfoText();
        }
    }
}
