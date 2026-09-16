// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ChallengesManager : MonoBehaviour
    {
        public Challenge[] challenges;

        // Dictionary to store references to the progress bars by challenge name
        private Dictionary<string, Challenge> challengeDict = new Dictionary<string, Challenge>();

        void Start()
        {
            // Populate the dictionary with challenges
            foreach (var challenge in challenges)
            {
                challengeDict.Add(challenge.Name, challenge);
            }
        }

        // Update progress for a specific challenge
        public void UpdateChallengeProgress(string challengeName, float newValue)
        {
            if (challengeDict.ContainsKey(challengeName))
            {
                Challenge challenge = challengeDict[challengeName];

                // Update the progress bar value
                challenge.ProgressBar.fillAmount = Mathf.Clamp01(newValue / challenge.MaxValue);

                // Update the value text
                challenge.ValueText.text = newValue.ToString() + "/" + challenge.MaxValue;

                // Update the value in the challenge object
                challenge.Value += newValue;

                // Check if the value has reached the max value
                if (newValue >= challenge.MaxValue)
                {
                    // Cross out the description text
                    if (challenge.DescriptionText != null)
                    {
                        challenge.DescriptionText.text = "<s>" + challenge.DescriptionText.text + "</s>";
                    }

                    // Activate the check mark
                    if (challenge.CheckMark != null)
                    {
                        challenge.CheckMark.SetActive(true);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Challenge " + challengeName + " not found.");
            }
        }
    }

    [System.Serializable]
    public class Challenge
    {
        public string Name;
        public float Value;
        public float MaxValue = 2;
        public UnityEngine.UI.Image ProgressBar;
        public TMPro.TMP_Text DescriptionText;
        public TMPro.TMP_Text ValueText;
        public GameObject CheckMark;
    }
}