// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class RoomManagerUI : MonoBehaviour
    {
        public GameObject playerTileUI_prefab;
        public List<PlayerTileUI> playerTiles = new List<PlayerTileUI>();
        public GameObject Grid;

        public Text CountdownText;
        public Text CountdownTextTitel;

        Coroutine c_msgLiveTimeCounter;

        [Header("Room messages")]
        public TMPro.TMP_Text RoomMessageTextField;
        public Image RoomManagerMessagesBackground;
        private void Awake()
        {
            CountdownTextTitel.text = "";
            CountdownText.text = "";

            RoomManager rm = GetComponent<RoomManager>();

            rm.RoomEvent_NewPlayerSpawned += PlayerNumberChanged;
            rm.RoomEvent_PlayerDespawned += PlayerNumberChanged;
            rm.RoomEvent_MatchCountdown += Countdown;
            rm.RoomEvent_Message += RoomManagerMessage;

            playerTileUI_prefab.gameObject.SetActive(false);
        }

        void RoomManagerMessage(string _msg, float _liveTime)
        {
            if (c_msgLiveTimeCounter != null)
                StopCoroutine(c_msgLiveTimeCounter);

            c_msgLiveTimeCounter = StartCoroutine(MessageLiveTimeCounter());
            IEnumerator MessageLiveTimeCounter()
            {
                RoomManagerMessagesBackground.gameObject.SetActive(true);
                RoomMessageTextField.text = _msg;
                yield return new WaitForSeconds(_liveTime);
                RoomMessageTextField.text = "";
                RoomManagerMessagesBackground.gameObject.SetActive(false);
            }
        }

        private void Countdown(int seconds)
        {
            CountdownTextTitel.text = (seconds == 0 ? "" : "Match starts in:");
            CountdownText.text = (seconds == 0 ? "" : seconds.ToString());
        }

        private void PlayerNumberChanged()
        {
            if (!playerTileUI_prefab) return;

            foreach (PlayerTileUI playerTile in playerTiles)
                if (playerTile)
                    Destroy(playerTile.gameObject);

            playerTiles.Clear();

            for (int i = 0; i < RoomManager._instance.players.Count; i++)
            {
                playerTileUI_prefab.SetActive(true);

                PlayerTileUI ptUI = Instantiate(playerTileUI_prefab, Grid.transform).GetComponent<PlayerTileUI>();

                playerTileUI_prefab.SetActive(false);

                playerTiles.Add(ptUI);

                ptUI.usernameTextField.text = "Player " + i;
            }
        }
    }
}