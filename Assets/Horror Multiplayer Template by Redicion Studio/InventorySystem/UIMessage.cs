// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio.InventorySystem
{
    public class UIMessage : MonoBehaviour
    {
        public TMPro.TMP_Text messageText;

        public void ShowMessage(string text)
        {
            messageText.text = text;

            this.GetComponent<Animator>().Play("UIMessageIn");

            StartCoroutine(DisableMessage());
        }

        IEnumerator DisableMessage()
        {
            yield return new WaitForSeconds(3);

            this.GetComponent<Animator>().Play("UIMessageOut");

            Destroy(this.gameObject, 1);
        }
    }
}
