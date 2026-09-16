// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class ExperienceManager : MonoBehaviour
    {
        private float currentXP;
        private float targetXP = 100;
        [HideInInspector] public int currentLevel;
        [SerializeField] private GameObject playerLevelElement;

        [HideInInspector] public float lerpTimer;
        [HideInInspector] public float delayTimer;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI currentXPText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI currentLevelPlayerText;
        [SerializeField] private TextMeshProUGUI nextLevelText;
        [SerializeField] private Image backXPBar;
        [SerializeField] private Image frontXPBar;
        [Header("Multipliers")]
        [Range(1f, 300f)]
        public float additionMultiplier = 300;
        [Range(2f, 4f)]
        public float powerMultiplier = 2;
        [Range(7f, 14f)]
        public float divisionMultiplier = 7;
        [SerializeField] public GameObject ExperienceUI;

        private void Start()
        {
            frontXPBar.fillAmount = currentXP / targetXP;
            backXPBar.fillAmount = currentXP / targetXP;
            targetXP = CalculateRequiredXp();
            currentLevelText.text = currentLevel.ToString();
            currentLevelPlayerText.text = currentLevel.ToString();
            nextLevelText.text = "";
            int nextLevel = currentLevel += 1;
            nextLevelText.text = nextLevel.ToString();
        }

        private void Update()
        {
            UpdateXpUI();
            if (Input.GetKeyDown(KeyCode.M))
                GainExperienceFlatRate(20);
            if (currentXP > targetXP)
                LevelUp();
        }

        public void UpdateXpUI()
        {
            currentXP = GetComponent<Player>().experiencePoints;

            float xpfraction = currentXP / targetXP;
            float FXP = frontXPBar.fillAmount;
            if (FXP < xpfraction)
            {
                delayTimer += Time.deltaTime;
                backXPBar.fillAmount = xpfraction;
                if (delayTimer > 3)
                {
                    lerpTimer += Time.deltaTime;
                    float percentComplete = lerpTimer / 4;
                    frontXPBar.fillAmount = Mathf.Lerp(FXP, backXPBar.fillAmount, percentComplete);

                }
            }
            currentXPText.text = currentXP + " / " + targetXP;
        }

        public void GainExperienceFlatRate(float xpGained)
        {
            currentXP += xpGained;
            lerpTimer = 0f;
            delayTimer = 0f;
        }

        public void GainExperienceScalable(float xpGained, int passedLevel)
        {
            if (passedLevel < currentLevel)
            {
                float multiplier = 1 + (currentLevel - passedLevel) * 0.1f;
                currentXP += xpGained * multiplier;
            }
            else
            {
                currentXP += xpGained;
            }
            lerpTimer = 0f;
            delayTimer = 0f;
        }

        public void LevelUp()
        {
            currentLevel++;
            frontXPBar.fillAmount = 0f;
            backXPBar.fillAmount = 0f;
            currentXP = Mathf.RoundToInt(currentXP - targetXP);
            targetXP = CalculateRequiredXp();
            currentLevelText.text = currentLevel.ToString();
            currentLevelPlayerText.text = currentLevel.ToString();
            int nextLevel = currentLevel += 1;
            nextLevelText.text = nextLevel.ToString();
        }

        private int CalculateRequiredXp()
        {
            int solveForRequiredXp = 0;
            for (int levelCycle = 1; levelCycle <= currentLevel; levelCycle++)
            {
                solveForRequiredXp += (int)Mathf.Floor(levelCycle + additionMultiplier * Mathf.Pow(powerMultiplier, levelCycle / divisionMultiplier));
            }
            return solveForRequiredXp / 4;
        }
    }
}