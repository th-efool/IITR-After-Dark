// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class LoadingScreen : MonoBehaviour
    {
        [Header("Loading Screen")]
        public GameObject loadingScreen;
        public LoadingScreenItem[] loadingScreenItems;
        public Image loadingScreenPicture;
        [Header("Loading Text")]
        public string[] loadingTexts;
        public TMPro.TMP_Text loadingText;
        [Header("Tips")]
        public TMPro.TMP_Text tipText;
        [Space]
        public GameObject loadingScreenContent;
        [Space]
        [Header("Music")]
        private MainMenuManager menuManager;

        void Start()
        {
            loadingScreenContent.SetActive(true);
            LoadingScreenItem currentLoadingScreen = loadingScreenItems[Random.Range(0, loadingScreenItems.Length)];
            loadingScreenPicture.sprite = currentLoadingScreen.loadingScreenSprite;
            tipText.text = currentLoadingScreen.tip;

            StartCoroutine(Loading());
        }

        private void Update()
        {
            if (menuManager == null)
            {
                if (GameObject.FindGameObjectWithTag("MainMenuManager") != null)
                    menuManager = GameObject.FindGameObjectWithTag("MainMenuManager").GetComponent<MainMenuManager>();
            }
        }

        IEnumerator Loading()
        {
            yield return new WaitForSeconds(2);
            loadingText.text = loadingTexts[0];
            yield return new WaitForSeconds(2);
            loadingText.text = loadingTexts[1];
            yield return new WaitForSeconds(2);
            loadingText.text = loadingTexts[2];
            yield return new WaitForSeconds(2);
            loadingText.text = loadingTexts[3];
            yield return new WaitForSeconds(5f);
            if (menuManager != null)
            {
                menuManager.ToggleMainMenuMusic(false);
            }
            Destroy(loadingScreen);
        }
    }

    [System.Serializable]
    public class LoadingScreenItem
    {
        public string Name;
        public Sprite loadingScreenSprite;
        [TextArea]
        public string tip;
    }
}