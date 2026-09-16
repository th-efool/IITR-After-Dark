// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class InventoryUI : MonoBehaviour
    {

        public Image[] slotIcons;
        public Image itemSelectedIndicator;
        public Sprite emptyHandIcon;

        private CharacterManager _characterManagerToObserve;

        [Header("Health Canvas")]
        [SerializeField] TMPro.TMP_Text _healthText;
        [SerializeField] Slider _sliderHealth;

        //I made spectator UI functionality here in order not to make to many UI scripts,
        //but it can be moved into new component easily
        [Header("SpectatorUI")]
        public Text _spectatorText;
        public GameObject _spectatorUI;

        #region interactable objects
        [Header("InteractionBar")]
        [SerializeField] GameObject _interactionHUD;
        [SerializeField] Image _interactionBar;
        InteractableObject _interactableObject;
        float timeToActivateObject;
        #endregion

        private void Start()
        {
            GameManager.GameEvent_PlayerSpawned += PlayerSpawned;
            Spectator.SpectatorEvent_NewPlayerSpectated += OnNewPlayerSpectated;

            //hide on start of the game interaction bar if it is not disabled in the hierarchy
            EndedInteraction();
        }
        void PlayerSpawned(GameObject _player, bool _observe)
        {
            if (!_observe) return;

            _spectatorText.text = "";
            _spectatorUI.SetActive(false);

            _characterManagerToObserve = _player.GetComponent<CharacterManager>();
            _characterManagerToObserve.CharacterEvent_EquipmentStateChanged += PlayerEquipmentStateChanged;
            _characterManagerToObserve.CharacterEvent_PlayerChangedItem += PlayerChangedItem;
            _characterManagerToObserve.CharacterEvent_Resurrection += OnObservedPlayerResurrected;

            CharacterInteraction charInteraction = _characterManagerToObserve.GetComponent<CharacterInteraction>();
            charInteraction.CharacterEvent_StartedInteraction += StartedInteraction;
            charInteraction.CharacterEvent_EndedInteraction += EndedInteraction;


            PlayerEquipmentStateChanged();
        }

        void PlayerEquipmentStateChanged()
        {
            for (int i = 0; i < slotIcons.Length; i++)
            {
                if (_characterManagerToObserve.items[i])
                    slotIcons[i].sprite = _characterManagerToObserve.items[i].icon;
                else
                    slotIcons[i].sprite = emptyHandIcon;
            }
        }
        void PlayerChangedItem(int _slotID)
        {
            itemSelectedIndicator.gameObject.transform.position = slotIcons[_slotID].gameObject.transform.position;
        }

        private void Update()
        {
            if (_characterManagerToObserve)
            {
                _sliderHealth.maxValue = _characterManagerToObserve.maxHealth;
                _sliderHealth.value = _characterManagerToObserve.health;
                _healthText.text = "Health: 100/" + _characterManagerToObserve.health.ToString("0");

                if (_interactableObject)
                {
                    UpdateInteractionBar(1f - ((timeToActivateObject - Time.time) / _interactableObject.TimeNeededToActivate));
                }
            }
        }

        void OnNewPlayerSpectated(int _playerID)
        {
            List<CharacterManager> players = RoomManager._instance.players;
            CharacterManager foundPlayer = players[_playerID];
            if (foundPlayer != null)
            {
                _spectatorText.text = "<color=#A4A4A4>Now spectating: </color><color=#FF0900>" + foundPlayer.GetComponent<Player>().username + "</color>";
                _spectatorUI.SetActive(true);
            }
        }
        void OnObservedPlayerResurrected()
        {
            _spectatorText.text = "";
            _spectatorUI.SetActive(false);
        }


        #region InteractionBar
        private void StartedInteraction(InteractableObject interactableObject)
        {
            _interactableObject = interactableObject;
            timeToActivateObject = Time.time + interactableObject.TimeNeededToActivate;
            _interactionHUD.gameObject.SetActive(true);
        }
        private void EndedInteraction()
        {
            _interactableObject = null;
            _interactionHUD.gameObject.SetActive(false);
        }

        public void UpdateInteractionBar(float percentage)
        {

            _interactionBar.fillAmount = percentage;

        }
        #endregion

        private void OnDestroy()
        {
            GameManager.GameEvent_PlayerSpawned -= PlayerSpawned;
            Spectator.SpectatorEvent_NewPlayerSpectated -= OnNewPlayerSpectated;
        }
    }
}