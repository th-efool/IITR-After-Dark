using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ServerMessageManager : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text serverMessageText;
        [SerializeField] Animator anim;
        [SerializeField] GameObject _root;

        public void ShowServerMessage(string serverMessage)
        {
            ServerMessageManager[] allManagers = FindObjectsOfType<ServerMessageManager>();

            foreach (ServerMessageManager manager in allManagers)
            {
                if (manager.gameObject != this.gameObject)
                {
                    manager.DestroyShowServerMessage();
                }
            }

            serverMessageText.text = serverMessage;
            anim.SetTrigger("ServerMessageFadeInAnimation");

            StartCoroutine(ShowServerMessageCoroutine());
        }

        IEnumerator ShowServerMessageCoroutine()
        {
            yield return new WaitForSeconds(5);

            anim.SetTrigger("ServerMessageFadeOutAnimation");
            Destroy(_root, 2f);
        }

        public void DestroyShowServerMessage()
        {
            Destroy(_root);
        }
    }
}
