// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class CarNameInfo : MonoBehaviour
    {
        public TMPro.TMP_Text carNameText;
        public Animator animator;

        public void SetUpCarNameText(string carName)
        {
            carNameText.text = carName;
            animator.Play("CarNameFadeIn");

            Destroy(this.gameObject, 4.1f);
        }
    }
}
