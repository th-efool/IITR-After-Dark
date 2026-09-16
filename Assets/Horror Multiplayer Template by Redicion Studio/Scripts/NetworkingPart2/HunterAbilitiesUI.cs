// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace RedicionStudio
{
    public class HunterAbilitiesUI : MonoBehaviour
    {
        public List<HunterAbilitiesIconsEntry> _hunterAbilitiesIcons = new List<HunterAbilitiesIconsEntry>();
        [SerializeField] GameObject _hunterAbilitiesOverlay;
        [SerializeField] GameObject _fightBarOverlay;
        [SerializeField] Image _fightBar;

        private void Awake()
        {
            ShowHunterOverlay(false);
            ShowFightBar(false);
        }

        void ShowFightBar(bool show)
        {
            _fightBarOverlay.SetActive(show);
        }
        void ShowHunterOverlay(bool show)
        {
            _hunterAbilitiesOverlay.SetActive(show);

            if (NetworkClient.localPlayer != null)
            {
                for (int i = 0; i < _hunterAbilitiesIcons.Count; i++)
                {
                    foreach (HunterAbilities.Hunter hunter in NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>().hunters)
                    {
                        if (NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>().currentHunterID.Equals(hunter.HunterID))
                        {
                            if (hunter._specialAttacks[i].specialAttackType == HunterAbilities.HunterSpecialAttack.SpecialAttackType.HuntersVision || hunter._specialAttacks[i].specialAttackType == HunterAbilities.HunterSpecialAttack.SpecialAttackType.HuntersInstinct || hunter._specialAttacks[i].specialAttackType == HunterAbilities.HunterSpecialAttack.SpecialAttackType.RapidRush || hunter._specialAttacks[i].specialAttackType == HunterAbilities.HunterSpecialAttack.SpecialAttackType.BlackoutStrike)
                            {
                                _hunterAbilitiesIcons[i].hunterAbilityIcon.AbleToBeUsed(true);
                            }
                        }
                    }
                }
            }
        }


        void OnVictimInRangeStateChanged(bool inRange)
        {
            for (int i = 0; i < _hunterAbilitiesIcons.Count; i++)
            {
                foreach (HunterAbilities.Hunter hunter in NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>().hunters)
                {
                    if (NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>().currentHunterID.Equals(hunter.HunterID))
                    {
                        if (hunter._specialAttacks[i].specialAttackType == HunterAbilities.HunterSpecialAttack.SpecialAttackType.Finisher)
                            _hunterAbilitiesIcons[i].hunterAbilityIcon.AbleToBeUsed(inRange);
                    }
                }
            }
        }
        void OnObservedCharacterTeamChanged(byte team)
        {
            ShowHunterOverlay(team == 1);
        }

        void AbilityUsed(int abilityID, float abilityCooldown)
        {
            _hunterAbilitiesIcons[abilityID].hunterAbilityIcon.AbilityUsed(abilityCooldown);
        }

        void FightStateChanged(int hunterState, int victimState)
        {
            if (hunterState == 0)
            {
                ShowFightBar(false);
                return;
            }
            else
            {
                ShowFightBar(true);
            }

            _fightBar.fillAmount = (float)victimState / (victimState + hunterState);
        }

        private void OnEnable()
        {
            GameManager.GameEvent_PlayerSpawned += OnNewPlayerSpawned;
        }
        private void OnDisable()
        {
            GameManager.GameEvent_PlayerSpawned -= OnNewPlayerSpawned;
        }
        void OnNewPlayerSpawned(GameObject _player, bool _observe)
        {
            if (!_observe) return;

            //since player object will never be destroyed, without destroying hud, we dont need to desubscribe this method
            _player.GetComponent<CharacterManager>().CharacterEvent_TeamSet += OnObservedCharacterTeamChanged;

            HunterAbilities hunterToObserve = _player.GetComponent<HunterAbilities>();

            hunterToObserve.CharacterEvent_VictimInRange += OnVictimInRangeStateChanged;
            hunterToObserve.CharacterEvent_HunterAbilityUsed += AbilityUsed;
            hunterToObserve.FightEvent_FightStateChanged += FightStateChanged;

        }
    }

    [System.Serializable]
    public class HunterAbilitiesIconsEntry
    {
        public HunterAbilityItemUI hunterAbilityIcon;
        public Image hunterAbilityImage;
        public Image hunterAbilityFilterImage;
        public Image hunterAbilityFilledImage;
    }
}