// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class Spectator : MonoBehaviour
    {
        private CharacterManager ourPlayer;

        public RoomManager roomManager;

        public bool spectateMode = false;
        int currentlySpectatedPlayerID;

        public delegate void PlayerSpectated(int _playerID);
        public static PlayerSpectated SpectatorEvent_NewPlayerSpectated;

        private RedicionStudio.PlayerInputs _input;

        private bool lastToggleUseInput = false;
        private bool lastNextSlotInput = false;
        private bool lastPreviousSlotInput = false;
        private bool lastToggleAimInput = false;

        private void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (roomManager.MatchRunning && !roomManager.MatchEnding)
            {
                if (spectateMode)
                {
                    GetComponent<CameraHandler>().camTrans.GetComponent<Camera>().enabled = true;

                    bool currentUseToggleInput = _input.use;
                    bool currentNextSlotInput = _input.nextSlot;
                    bool currentPreviousSlotInput = _input.previousSlot;
                    bool currentAimToggleInput = _input.aim;

                    if (_input.gamepadConnected)
                    {
                        if (currentNextSlotInput && !lastNextSlotInput)
                        {
                            FindPlayerToSpectate(ourPlayer.Team, 1);
                        }
                        if (currentPreviousSlotInput && !lastPreviousSlotInput)
                        {
                            FindPlayerToSpectate(ourPlayer.Team, -1);
                        }
                    }
                    else
                    {
                        if (currentAimToggleInput && !lastToggleAimInput)
                        {
                            FindPlayerToSpectate(ourPlayer.Team, 1);
                        }
                        if (currentUseToggleInput && !lastToggleUseInput)
                        {
                            FindPlayerToSpectate(ourPlayer.Team, -1);
                        }
                    }

                    lastToggleUseInput = currentUseToggleInput;
                    lastNextSlotInput = currentNextSlotInput;
                    lastPreviousSlotInput = currentPreviousSlotInput;
                    lastToggleAimInput = currentAimToggleInput;
                }
                else
                {
                    GetComponent<CameraHandler>().camTrans.GetComponent<Camera>().enabled = false;
                }
            }
            else
            {
                if (spectateMode)
                {
                    spectateMode = false;
                    GetComponent<CameraHandler>().camTrans.GetComponent<Camera>().enabled = false;
                }
            }
        }
        void FindPlayerToSpectate(int _team, int _arrow)
        {

            List<CharacterManager> players = RoomManager._instance.players;

            int startPlayerIndex = currentlySpectatedPlayerID;

            while (true)
            {
                currentlySpectatedPlayerID += _arrow;

                capPlayerIndex();

                //if we checked all players and none of them is able to spectated, we do nothing
                if (startPlayerIndex == currentlySpectatedPlayerID)
                {
                    //print("Spectator: We looped through every player and didn't found anyone who we can spectate");
                    return;
                }


                /*here we are checking if player that we requested to spectate is:
                -alive
                -in our team
                -is not someone that we already spectate right now
                if which of this condition is not true, then we go to check next player
                */
                if (players[currentlySpectatedPlayerID].health > 0 && players[currentlySpectatedPlayerID].Team == _team && startPlayerIndex != currentlySpectatedPlayerID)
                {
                    Spectate(currentlySpectatedPlayerID);
                    Debug.Log("Succesfully founded someone to spectate in our team");
                    return;
                }

            }

            void capPlayerIndex()
            {
                if (currentlySpectatedPlayerID >= players.Count)
                {
                    currentlySpectatedPlayerID = 0;
                }
                else if (currentlySpectatedPlayerID < 0)
                {
                    currentlySpectatedPlayerID = players.Count - 1;
                }
            }
        }

        void Spectate(int _playerID)
        {
            if (!RoomManager._instance) return;
            GameManager.GameEvent_SpectatePlayer?.Invoke(RoomManager._instance.players[_playerID].gameObject);

            SpectatorEvent_NewPlayerSpectated(_playerID);
        }

        private void Awake()
        {
            GameManager.GameEvent_PlayerSpawned += PlayerSpawned;
            GameManager.GameEvent_PlayerDespawned += PlayerDespawned;
        }
        #region listeners
        void PlayerSpawned(GameObject _player, bool _observe)
        {
            if (!_observe) return;

            ourPlayer = _player.GetComponent<CharacterManager>();

            currentlySpectatedPlayerID = RoomManager._instance.players.IndexOf(ourPlayer);

            _player.GetComponent<CharacterManager>().CharacterEvent_Death += OnOurPlayerDied;
            _player.GetComponent<CharacterManager>().CharacterEvent_Resurrection += OnOurPlayerResurrected;
        }
        void PlayerDespawned(GameObject _player)
        {
            if (_player == ourPlayer)
            {
                _player.GetComponent<CharacterManager>().CharacterEvent_Death -= OnOurPlayerDied;
                _player.GetComponent<CharacterManager>().CharacterEvent_Resurrection -= OnOurPlayerResurrected;
            }
        }

        //let player spectate others only when he is dead
        void OnOurPlayerDied()
        {
            spectateMode = true;
            FindPlayerToSpectate(ourPlayer.Team, 1); // New
        }
        void OnOurPlayerResurrected()
        {
            spectateMode = false;
        }
        #endregion
    }
}