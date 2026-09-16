// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class ChatSystem : MonoBehaviour
    {
        public InputField chatMessage;
        public Text chatHistory;
        public Scrollbar scrollbar;
        [HideInInspector]
        public bool isChatOpen = false;
        public GameObject[] chatContent;

        public void Awake()
        {
            RedicionStudio.InventorySystem.Player.OnMessage += OnPlayerMessage;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (isChatOpen == false)
                {
                    isChatOpen = true;
                    GetComponent<Animator>().Play("ChatIn");
                    TPCameraController.LockCursor(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    chatMessage.ActivateInputField();
                }
                else
                {
                    isChatOpen = false;
                    GetComponent<Animator>().Play("ChatOut");
                    TPCameraController.LockCursor(true);
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    chatMessage.DeactivateInputField();
                }
            }
            if (isChatOpen == true)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    if (chatMessage.text != "")
                    {
                        OnSend();
                    }
                }
            }
        }

        void OnPlayerMessage(RedicionStudio.InventorySystem.Player player, string message)
        {
            string prettyMessage = player.isLocalPlayer ?
                $"<color=red>{player.username}: </color> {message}" :
                $"<color=red>{player.username}: </color> {message}";
            AppendMessage(prettyMessage);
        }

        public void OnSend()
        {
            if (chatMessage.text.Trim() == "")
                return;

            // get our player
            RedicionStudio.InventorySystem.Player player = NetworkClient.connection.identity.GetComponent<RedicionStudio.InventorySystem.Player>();

            // send a message
            player.CmdSend(chatMessage.text.Trim());

            chatMessage.text = "";
        }

        internal void AppendMessage(string message)
        {
            StartCoroutine(AppendAndScroll(message));
        }

        IEnumerator AppendAndScroll(string message)
        {
            chatHistory.text += message + "\n";

            yield return null;
            yield return null;

            // slam the scrollbar down
            scrollbar.value = 0;
        }

        public void ToggleChatSystem(bool _enable)
        {
            foreach (GameObject content in chatContent)
            {
                content.SetActive(_enable);
            }
        }
    }

}