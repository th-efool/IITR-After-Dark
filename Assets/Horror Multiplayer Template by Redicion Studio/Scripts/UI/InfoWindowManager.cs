// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class InfoWindowManager : MonoBehaviour
    {
        [Header("UI")]
        public TMPro.TMP_Text infoText;

        [Space]
        public Animator infoWindowAnimator;

        public void OpenInfoWindow(string _infoText)
        {
            infoText.text = _infoText;
            infoWindowAnimator.SetTrigger("FadeIn");
        }

        public void CloseInfoWindow()
        {
            infoWindowAnimator.SetTrigger("FadeOut");
            StartCoroutine(CloseInfoWindowCoroutine());
        }

        IEnumerator CloseInfoWindowCoroutine()
        {
            yield return new WaitForSeconds(1);

            Destroy(gameObject);
        }
    }
}
