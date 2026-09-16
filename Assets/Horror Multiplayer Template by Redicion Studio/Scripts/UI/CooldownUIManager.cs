// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class CooldownUIManager : MonoBehaviour
    {
        public Image cooldownImage;

        public void StartCooldown(float cooldownValue)
        {
            StartCoroutine(Cooldown(cooldownValue));
        }

        IEnumerator Cooldown(float cooldownValue)
        {
            float cooldownTime = cooldownValue;
            while (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;
                cooldownImage.fillAmount = cooldownTime / cooldownValue;

            }

            Destroy(gameObject);

            yield return null;
        }
    }
}
