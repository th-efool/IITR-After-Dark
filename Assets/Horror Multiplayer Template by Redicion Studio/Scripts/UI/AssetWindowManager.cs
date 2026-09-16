using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class AssetWindowManager : MonoBehaviour
    {
        public Animator infoWindowAnimator;

        public void OpenInfoWindow()
        {
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
