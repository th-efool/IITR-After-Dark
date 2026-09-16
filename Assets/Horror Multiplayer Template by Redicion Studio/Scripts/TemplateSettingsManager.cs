// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RedicionStudio.Settings
{
    public class TemplateSettingsManager : MonoBehaviour
    {
        private RedicionStudio.Settings.TemplateSettings templateSettings;

        public GameObject[] worldElementsToDisable;
        public UnityEvent onDisableWorldElements;

        private void Start()
        {
            StartCoroutine(LoadTemplateSettings());
        }

        IEnumerator LoadTemplateSettings()
        {
            while (templateSettings == null)
            {
                templateSettings = Resources.Load<RedicionStudio.Settings.TemplateSettings>("TemplateSettings");

                yield return null;
            }

            if (templateSettings != null)
            {
                if (templateSettings.hideSpecificWorldElements)
                {
                    DisableWorldElements();
                }
            }
        }

        void DisableWorldElements()
        {
            foreach (GameObject worldElement in worldElementsToDisable)
            {
                worldElement.SetActive(false);
            }
            onDisableWorldElements.Invoke();
        }
    }
}