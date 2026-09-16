// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio.Casino
{
    public class SlotMachineManager : MonoBehaviour
    {
        [Header("Slot Machine")]
        [SerializeField] private int defaultBet = 50;
        private int currentBet = 50;
        [SerializeField] private TMPro.TMP_Text currentBetValueText;
        [SerializeField] private TMPro.TMP_Text lastWinValueText;
        private int multiplier = 1;
        [SerializeField] private int maxMultiplierValue = 3;
        public List<Sprite> symbols;
        public int reelsCount = 3;
        public int symbolsPerReel = 3;
        public List<Image> reelImages;

        private List<List<int>> reelPositions;
        private bool isSpinning = false;

        [Header("Interaction")]
        public KeyCode interactKey = KeyCode.F;
        public GameObject interactUI;
        public GameObject interactTextHolder;
        public GameObject slotMachineCanvas;
        public TMPro.TMP_Text interactionText;
        bool inTrigger = false;
        bool usingSlotMachine = false;
        private Transform _camera;
        GameObject player;
        [SerializeField] private GameObject UIMessagePrefab;
        GameObject instantiatedUIMessage;
        public GameObject slotMachineCamera;

        [Header("Prizes")]
        [SerializeField] private PrizeItem[] prizes;

        [Header("Texts")]
        [SerializeField] private TMPro.TMP_Text textsText;
        [SerializeField] private string[] WinnerTexts = new string[] { "Very good!", "Keep it up!", "Super!", "Great!", "Unique!", "Perfect!" };
        [SerializeField] private string[] LostTexts = new string[] { "Too bad", "What a pity", "Maybe next time", "Almost!" };

        [Header("Sounds")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip[] interactSounds;
        [SerializeField] private AudioClip[] betSounds;
        [SerializeField] private AudioClip[] spinSounds;
        [SerializeField] private AudioClip[] winSounds;
        [SerializeField] private AudioClip[] loseSounds;

        void Start()
        {
            _camera = GameObject.Find("MainCamera").transform;

            reelPositions = new List<List<int>>();
            for (int i = 0; i < reelsCount; i++)
            {
                reelPositions.Add(new List<int>());
                for (int j = 0; j < symbolsPerReel; j++)
                {
                    reelPositions[i].Add(0);
                }
            }

            currentBet = defaultBet;
            currentBetValueText.text = "$" + currentBet.ToString();
        }

        void Update()
        {
            interactUI.transform.LookAt(interactUI.transform.position + _camera.rotation * Vector3.forward,
            _camera.rotation * Vector3.up);

            if (inTrigger)
            {
                if (Input.GetKeyDown(interactKey))
                {
                    if (usingSlotMachine && !isSpinning)
                    {
                        player.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop = false;
                        usingSlotMachine = false;
                        LockCursor(false);
                        interactionText.text = "Press 'F'";
                        interactTextHolder.SetActive(true);
                        slotMachineCamera.SetActive(false);
                        slotMachineCanvas.SetActive(false);
                        textsText.text = "";
                    }
                    else if (!usingSlotMachine)
                    {
                        player.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop = true;
                        ResetBetValue();
                        usingSlotMachine = true;
                        LockCursor(true);
                        interactTextHolder.SetActive(false);
                        slotMachineCamera.SetActive(true);
                        slotMachineCanvas.SetActive(true);
                        if (interactSounds.Length > 0)
                        {
                            AudioClip sound = interactSounds[Random.Range(0, interactSounds.Length)];
                            source.clip = sound;
                            source.Play();
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (usingSlotMachine && !isSpinning)
                    {
                        player.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop = false;
                        usingSlotMachine = false;
                        LockCursor(false);
                        interactionText.text = "Press 'F'";
                        interactTextHolder.SetActive(true);
                        slotMachineCamera.SetActive(false);
                        slotMachineCanvas.SetActive(false);
                        textsText.text = "";
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Return) && usingSlotMachine && !isSpinning)
            {
                RemoveMoney(currentBet);
                if (spinSounds.Length > 0)
                {
                    AudioClip sound = spinSounds[Random.Range(0, spinSounds.Length)];
                    source.clip = sound;
                    source.Play();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Space) && usingSlotMachine && !isSpinning)
            {
                // Bet One
                multiplier += 1;
                if (multiplier > maxMultiplierValue)
                    multiplier = 1;

                currentBet = defaultBet * multiplier;

                currentBetValueText.text = "$" + currentBet.ToString();
                if (betSounds.Length > 0)
                {
                    AudioClip sound = betSounds[Random.Range(0, betSounds.Length)];
                    source.clip = sound;
                    source.Play();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Tab) && usingSlotMachine && !isSpinning)
            {
                // Bet Max
                multiplier = maxMultiplierValue;

                currentBet = defaultBet * multiplier;

                currentBetValueText.text = "$" + currentBet.ToString();
                if (betSounds.Length > 0)
                {
                    AudioClip sound = betSounds[Random.Range(0, betSounds.Length)];
                    source.clip = sound;
                    source.Play();
                }
            }
        }

        void ResetBetValue()
        {
            multiplier = 1;

            currentBet = defaultBet * multiplier;

            currentBetValueText.text = "$" + currentBet.ToString();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                if (other.GetComponent<StarterAssets.ThirdPersonController>().enabled == true)
                {
                    player = other.gameObject;
                    inTrigger = true;
                    interactUI.SetActive(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                if (other.GetComponent<StarterAssets.ThirdPersonController>().enabled == true)
                {
                    player = null;
                    inTrigger = false;
                    interactUI.SetActive(false);
                    if (usingSlotMachine)
                    {
                        other.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inShop = false;
                        usingSlotMachine = false;
                        LockCursor(false);
                        interactionText.text = "Press 'F'";
                        interactTextHolder.SetActive(true);
                        slotMachineCamera.SetActive(false);
                        slotMachineCanvas.SetActive(false);
                        textsText.text = "";
                    }
                }
            }
        }

        public static void LockCursor(bool value)
        {
            if (value)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        IEnumerator SpinReels()
        {
            for (int i = 0; i < reelsCount; i++)
            {
                int targetSymbolIndex = Random.Range(0, symbols.Count);
                int initialSymbolIndex = reelPositions[i][0];
                int steps = symbols.Count * 2 + targetSymbolIndex - initialSymbolIndex;

                for (int j = 0; j < steps; j++)
                {
                    reelPositions[i][0] = (reelPositions[i][0] + 1) % symbols.Count;
                    UpdateReelImages(); // Update all reel images
                    yield return new WaitForSeconds(0.05f);
                }

                yield return new WaitForSeconds(0.2f); // Delay between spinning reels
            }

            isSpinning = false;
            slotMachineCanvas.SetActive(true);
            CheckWinningCombinations();
        }

        void CheckWinningCombinations()
        {
            if (reelImages[0].sprite.name == reelImages[1].sprite.name & reelImages[0].sprite.name == reelImages[2].sprite.name)
            {
                // All three symbols are equal
                foreach (PrizeItem prize in prizes)
                {
                    if (reelImages[0].sprite.name == prize.PrizeSprite.name)
                    {
                        AddMoney(prize.TriplePrizeValue);
                        lastWinValueText.text = prize.TriplePrizeValue.ToString();
                        DisplayWinningText();
                        if (winSounds.Length > 0)
                        {
                            AudioClip sound = winSounds[Random.Range(0, winSounds.Length)];
                            source.clip = sound;
                            source.Play();
                        }
                    }
                }
            }
            else if (reelImages[0].sprite.name == reelImages[1].sprite.name || (reelImages[1].sprite.name == reelImages[2].sprite.name))
            {
                // Two symbols are equal
                foreach (PrizeItem prize in prizes)
                {
                    if (reelImages[0].sprite.name == prize.PrizeSprite.name || reelImages[1].sprite.name == prize.PrizeSprite.name)
                    {
                        AddMoney(prize.DoublePrizeValue);
                        lastWinValueText.text = prize.DoublePrizeValue.ToString();
                        DisplayWinningText();
                        if (winSounds.Length > 0)
                        {
                            AudioClip sound = winSounds[Random.Range(0, winSounds.Length)];
                            source.clip = sound;
                            source.Play();
                        }
                    }
                }
            }
            else
            {
                instantiatedUIMessage = Instantiate(UIMessagePrefab);

                instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Nothing won");
                DisplayLostText();
                if (loseSounds.Length > 0)
                {
                    AudioClip sound = loseSounds[Random.Range(0, loseSounds.Length)];
                    source.clip = sound;
                    source.Play();
                }
            }
        }

        void DisplayWinningText()
        {
            System.Random random = new System.Random();
            int textInt = random.Next(WinnerTexts.Length);
            string pickText = WinnerTexts[textInt];
            textsText.text = pickText;
        }

        void DisplayLostText()
        {
            System.Random random = new System.Random();
            int textInt = random.Next(LostTexts.Length);
            string pickText = LostTexts[textInt];
            textsText.text = pickText;
        }

        void UpdateReelImages()
        {
            for (int i = 0; i < reelsCount; i++)
            {
                int symbolIndex = reelPositions[i][0];
                Sprite symbol = symbols[symbolIndex];
                reelImages[i].sprite = symbol;
            }
        }

        private void AddMoney(int amount)
        {
            GameObject _localPlayer;

            _localPlayer = NetworkClient.localPlayer.gameObject;

            _localPlayer.GetComponent<PlayerInteractionModule>().AddMoney(_localPlayer.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>(), amount);
        }

        private void RemoveMoney(int amount)
        {
            GameObject _localPlayer;

            _localPlayer = NetworkClient.localPlayer.gameObject;


            if (_localPlayer.GetComponent<Player>().funds < amount)
            {
                instantiatedUIMessage = Instantiate(UIMessagePrefab);

                instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Not enough funds");

                return;
            }
            _localPlayer.GetComponent<PlayerInteractionModule>().RemoveMoney(_localPlayer.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>(), amount);
            isSpinning = true;
            StartCoroutine(SpinReels());
            slotMachineCanvas.SetActive(false);
        }
    }

    [System.Serializable]
    public class PrizeItem
    {
        public string PrizeName;
        public Sprite PrizeSprite;
        [Space]
        public int TriplePrizeValue;
        public int DoublePrizeValue;
    }
}
