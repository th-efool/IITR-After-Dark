using TMPro;
using UnityEngine;

namespace RedicionStudio
{
    public class DisplayCredits : MonoBehaviour
    {
        public CreditsManager credits;
        public TextMeshProUGUI creditsText;
        public float scrollSpeed = 50f;
        private float startYPosition;
        public float startYPositionAddition;
        public float startYPositionDeduction;
        public float endYPositionAddition;
        public float endYPositionDeduction;
        [Space]
        public bool returnToMainMenuAfterCreditsEnded = false;

        public KeyCode returnToMainMenuKey = KeyCode.Escape;

        void Start()
        {
            creditsText.text = FormatCredits();

            float totalHeight = creditsText.preferredHeight - startYPositionDeduction + startYPositionAddition;

            creditsText.rectTransform.anchoredPosition = new Vector2(0, -totalHeight);
            startYPosition = creditsText.rectTransform.anchoredPosition.y;
        }

        void Update()
        {
            creditsText.rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

            if (creditsText.rectTransform.anchoredPosition.y > startYPosition + creditsText.preferredHeight + endYPositionAddition - endYPositionDeduction)
            {
                if(returnToMainMenuAfterCreditsEnded)
                {
                    OfflineMainMenuManager menuManager = FindObjectOfType<OfflineMainMenuManager>();

                    if (menuManager != null)
                    {
                        menuManager.ShowCredits(false);

                        Destroy(gameObject);
                    }
                }

                float totalHeight = creditsText.preferredHeight - startYPositionDeduction + startYPositionAddition;

                creditsText.rectTransform.anchoredPosition = new Vector2(0, -totalHeight);
            }

            if (Input.GetKeyDown(returnToMainMenuKey))
            {
                OfflineMainMenuManager menuManager = FindObjectOfType<OfflineMainMenuManager>();

                if (menuManager != null)
                {
                    menuManager.ShowCredits(false);

                    Destroy(gameObject);
                }
            }
        }

        private string FormatCredits()
        {
            string formattedText = "";
            foreach (var entry in credits.creditEntries)
            {
                if (!string.IsNullOrEmpty(entry.title))
                {
                    formattedText += "<color=red><b>" + entry.title + "</b></color>\n";
                    foreach (var name in entry.names)
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            formattedText += "<color=white>" + name + "</color>\n";
                        }
                    }
                    formattedText += "\n";
                }
            }
            return formattedText;
        }
    }
}
