// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class UIPlayerListItem : MonoBehaviour
    {
        public TMPro.TMP_Text playerNameText;
        public TMPro.TMP_Text healthText;
        public Slider healthSlider;
        public Image playerImage;
        public GameObject[] playerDeadUiElements;
        public GameObject[] playerInjuredUIElements;
        public GameObject escapedUiElement;

        public void UpdateHealth(int health)
        {
            int _health = health;
            if (_health < 0)
                _health = 0;
            healthText.text = "Health: " + _health + "/100";
            healthSlider.value = _health;
            if (_health <= 0)
            {
                foreach (GameObject UiElement in playerDeadUiElements)
                    UiElement.SetActive(true);
                foreach (GameObject UiElement in playerInjuredUIElements)
                    UiElement.SetActive(false);
            }
            else
            {
                foreach (GameObject UiElement in playerDeadUiElements)
                    UiElement.SetActive(false);
                if (_health < 100 && _health != 0)
                {
                    foreach (GameObject UiElement in playerInjuredUIElements)
                        UiElement.SetActive(true);
                }
                else if (_health == 100)
                {
                    foreach (GameObject UiElement in playerInjuredUIElements)
                        UiElement.SetActive(false);
                }
            }
        }
    }
}