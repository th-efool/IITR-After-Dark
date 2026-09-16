// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class KillMessage : MonoBehaviour
    {
        public Text killMessageText;

        public void ShowKillMessage(string killerPlayer, string killedPlayer, AttackType attackType)
        {
            killMessageText.text = "<color=red>" + killerPlayer + "</color> has killed " + "<color=grey>" + killedPlayer + "</color>" + "<color=cyan>[" + attackType.ToString() + "]</color>";

            this.GetComponent<Animator>().Play("KillMessageIn");

            StartCoroutine(DisableKillMessage());
        }

        IEnumerator DisableKillMessage()
        {
            yield return new WaitForSeconds(10);

            this.GetComponent<Animator>().Play("KillMessageOut");

            Destroy(this.gameObject, 1);
        }
    }
}