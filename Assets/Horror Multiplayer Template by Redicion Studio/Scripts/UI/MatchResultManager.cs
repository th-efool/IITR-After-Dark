// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedicionStudio
{
    public class MatchResultManager : MonoBehaviour
    {
        [Header("Match Statistics")]
        public float survivedTime; //how long the player has survived
        public CountValueManager survivedTimeCounter;
        public float survivedTimeXP;
        public CountValueManager survivedTimeXPCounter;
        public TMPro.TMP_Text survivedTimeText;
        [Space]
        public float damageDealt; //how much damage the player has dealt
        public CountValueManager damageDealtCounter;
        public float damageDealtXP;
        public CountValueManager damageDealtXPCounter;
        [Space]
        public float completedTasks; //how many tasks the player has completed
        public CountValueManager completedTasksCounter;
        public float completedTasksXP;
        public CountValueManager completedTasksXPCounter;
        [Space]
        public float helpedPlayers; //how often the player has helped other players
        public CountValueManager helpedPlayersCounter;
        public float helpedPlayersXP;
        public CountValueManager helpedPlayersXPCounter;
        [Space]
        public float instrumentsSuccessfullyUsed; //how often the player has successfully used instruments
        public CountValueManager instrumentsSuccessfullyUsedCounter;
        public float instrumentsSuccessfullyUsedXP;
        public CountValueManager instrumentsSuccessfullyUsedXPCounter;
        [Space]
        public float playersKilled; //how many players the player has killed
        public CountValueManager playersKilledCounter;
        public float playersKilledXP;
        public CountValueManager playersKilledXPCounter;
        [Space]
        public CountValueManager totalXPCounter;
        public CountValueManager totalMoneyCounter;
        [Space]
        public TMPro.TMP_Text statusText;
        public GameObject statusTextBackground;

        Coroutine c_displayMatchStatistics;
        Coroutine c_countTotalXP;
        Coroutine c_countTotalMoney;
        Coroutine c_removeWindow;

        public bool isHunter = false;
        public bool survived = false;

        private void Start()
        {
            if (!isHunter)
            {
                // is survivor
                if (survived)
                    statusText.text = "YOU SURVIVED";
                else
                    statusText.text = "YOU WERE KILLED";
                damageDealtCounter.gameObject.SetActive(false);
                playersKilledCounter.gameObject.SetActive(false);
                survivedTimeText.text = "Survived Time";
            }
            else
            {
                // is hunter
                statusText.gameObject.SetActive(false);
                statusTextBackground.SetActive(false);
                helpedPlayersCounter.gameObject.SetActive(false);
                instrumentsSuccessfullyUsedCounter.gameObject.SetActive(false);
                survivedTimeText.text = "Time played";
            }
            c_displayMatchStatistics = StartCoroutine(DisplayMatchStatistics());
            c_removeWindow = StartCoroutine(RemoveWindow());
        }

        IEnumerator DisplayMatchStatistics()
        {
            yield return new WaitForSeconds(2f);
            survivedTimeCounter.AddValue(survivedTime);
            survivedTimeXPCounter.AddValue(survivedTimeXP);
            damageDealtCounter.AddValue(damageDealt);
            damageDealtXPCounter.AddValue(damageDealtXP);
            completedTasksCounter.AddValue(completedTasks);
            completedTasksXPCounter.AddValue(completedTasksXP);
            helpedPlayersCounter.AddValue(helpedPlayers);
            helpedPlayersXPCounter.AddValue(helpedPlayersXP);
            instrumentsSuccessfullyUsedCounter.AddValue(instrumentsSuccessfullyUsed);
            instrumentsSuccessfullyUsedXPCounter.AddValue(instrumentsSuccessfullyUsedXP);
            playersKilledCounter.AddValue(playersKilled);
            playersKilledXPCounter.AddValue(playersKilledXP);

            c_countTotalXP = StartCoroutine(CountTotalXP());
            c_countTotalMoney = StartCoroutine(CountTotalMoney());
        }

        IEnumerator CountTotalXP()
        {
            yield return new WaitForSeconds(1f);
            totalXPCounter.AddValue(survivedTimeXP + damageDealtXP + completedTasksXP + helpedPlayersXP + instrumentsSuccessfullyUsedXP + playersKilledXP);
        }

        IEnumerator CountTotalMoney()
        {
            yield return new WaitForSeconds(2f);
            int earnedMoney = 0;

            if (survived)
            {
                earnedMoney += Mathf.RoundToInt(survivedTimeXP);
            }

            earnedMoney += Mathf.RoundToInt(damageDealtXP);

            earnedMoney += Mathf.RoundToInt(completedTasksXP);

            earnedMoney += Mathf.RoundToInt(helpedPlayersXP);

            earnedMoney += Mathf.RoundToInt(instrumentsSuccessfullyUsedXP);

            earnedMoney += Mathf.RoundToInt(playersKilledXP);

            totalMoneyCounter.AddValue(earnedMoney);
        }

        IEnumerator RemoveWindow()
        {
            yield return new WaitForSeconds(20f);
            Destroy(gameObject);
        }
    }
}