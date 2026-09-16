// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Cinemachine;
using UnityEngine.UI;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class MainMenuManager : NetworkBehaviour
    {
        public NetworkManager networkManager;

        public RoomManager roomManager;

        // menu camera
        public Transform menuCamera;

        // menu ui
        public Transform menuUI;

        // offline menu
        public OfflineMainMenuManager offlineMainMenuManager;
        [HideInInspector] public bool isOffline = true;

        // main menu buttons
        public MainMenuUIButton[] mainMenuUIButtons;

        public GameObject[] uIElementsToHide;

        public Canvas characterCanvas;

        public GameObject challengesWindow;

        public GameObject roomManagerMessagesContent;
        public TMPro.TMP_Text roomManagerMessagesText;

        public GameObject currencyUI;

        public ChatSystem chatSystem;

        public GameObject countDownText;
        public GameObject CountdownTextTitel;

        [Header("Loading Screen")]
        public GameObject loadingScreenPrefab;

        [Space]
        [Header("Player Customization (Online)")]
        [HideInInspector] public bool isSurvivorCustomizationSelected = false;
        public GameObject SurvivorCustomizationButtonSelectedGameObject;
        public GameObject SurvivorCustomizationPlayerFollowCamera;

        [Space]
        [Header("Item Shop (Online)")]
        [HideInInspector] public bool isItemShopSelected = false;
        public GameObject ItemShopButtonSelectedGameObject;
        public GameObject ItemShopUI;
        public Transform ItemShopContent;
        public Toggle PerkToggle;
        public Toggle OutfitToggle;
        public Toggle KillerToggle;
        public TMPro.TMP_Text itemsFoundText;
        public GameObject ItemShopCamera;
        public Transform ItemShopCameraTarget;

        [Space]
        [Header("Killer Customization (Online)")]
        [HideInInspector] public bool isKillerCustomizationSelected = false;
        public GameObject KillerCustomizationButtonSelectedGameObject;
        public GameObject KillerCustomizationPlayerFollowCamera;
        public Transform KillerCustomizationPlayerFollowCameraTarget;
        public Transform KillerSelectionPreviewModelPosition;

        [Space]
        [Header("Survivor Outfit Customization (Online)")]
        [HideInInspector] public bool isSurvivorOutfitCustomizationSelected = false;
        public GameObject SurvivorOutfitCustomizationButtonSelectedGameObject;
        public GameObject SurvivorOutfitCustomizationPlayerFollowCamera;

        [Space]
        [Header("Settings Menu")]
        public GameSettingsManager gameSettings;
        [HideInInspector] public bool isSettingsMenuSelected = false;
        public GameObject SettingsMenuButtonSelectedGameObject;
        public GameObject SettingsMenuPlayerFollowCamera;

        [Header("Pause Menu")]
        public KeyCode pauseMenuKey = KeyCode.Escape;
        [HideInInspector] public bool pauseMenuActive = false;
        public GameObject pauseMenuUI;
        public MainMenuUIButton[] pauseMenuUIButtons;

        [Space]
        [Header("Pause Menu Settings Menu")]
        [HideInInspector] public bool isPauseMenuSettingsMenuSelected = false;
        public GameObject PauseMenuSettingsMenuButtonSelectedGameObject;

        [Space]
        [Header("Ready Button (Online)")]
        public GameObject readyCheckMark;

        [Space]
        [Header("Music")]
        public bool isMainMenuMusicPlaying = true;
        public AudioSource mainMenuMusic;
        public float fadeDuration = 4f;
        private Coroutine fadeCoroutine;

        private RedicionStudio.PlayerInputs _input;

        private bool lastTogglePauseMenuInput = false;

        private void Start()
        {
            if (!isServer)
            {
                foreach (MainMenuUIButton uIButton in mainMenuUIButtons)
                {
                    uIButton.buttonSelectedGameObject.SetActive(false);
                    uIButton.buttonHoverGameObject.SetActive(false);
                }

                PerkToggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });
                OutfitToggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });
                KillerToggle.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });

                OnToggleValueChanged();
            }
            countDownText.SetActive(false);
            CountdownTextTitel.SetActive(false);
        }

        private void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            bool currentPauseMenuToggleInput = _input.pauseMenu;

            if (!isServer)
            {
                if (!roomManager.MatchRunning)
                {
                    if (!isMainMenuMusicPlaying)
                    {
                        ToggleMainMenuMusic(true);
                    }
                    ToggleMenu(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    if (isSurvivorCustomizationSelected)
                    {
                        SurvivorCustomizationButtonSelectedGameObject.SetActive(true);
                        SurvivorCustomizationPlayerFollowCamera.SetActive(true);
                        if (SurvivorCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow == null)
                        {
                            SurvivorCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow = NetworkClient.localPlayer.GetComponent<CharacterManager>().menuLookAtPoint;
                        }
                    }
                    else
                    {
                        SurvivorCustomizationButtonSelectedGameObject.SetActive(false);
                        SurvivorCustomizationPlayerFollowCamera.SetActive(false);
                    }
                    if (isItemShopSelected)
                    {
                        ItemShopButtonSelectedGameObject.SetActive(true);
                        ItemShopUI.SetActive(true);
                        ItemShopCamera.SetActive(true);
                        if (ItemShopCamera.GetComponent<CinemachineCamera>().Follow == null)
                        {
                            ItemShopCamera.GetComponent<CinemachineCamera>().Follow = ItemShopCameraTarget;
                        }
                    }
                    else
                    {
                        ItemShopButtonSelectedGameObject.SetActive(false);
                        ItemShopUI.SetActive(false);
                        ItemShopCamera.SetActive(false);
                    }
                    if (isKillerCustomizationSelected)
                    {
                        KillerCustomizationButtonSelectedGameObject.SetActive(true);
                        KillerCustomizationPlayerFollowCamera.SetActive(true);
                        if (KillerCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow == null)
                        {
                            KillerCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow = KillerCustomizationPlayerFollowCameraTarget;
                        }
                        NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().killerSelectionUI.SetActive(true);
                    }
                    else
                    {
                        KillerCustomizationButtonSelectedGameObject.SetActive(false);
                        KillerCustomizationPlayerFollowCamera.SetActive(false);
                        NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().killerSelectionUI.SetActive(false);
                    }
                    if (isSurvivorOutfitCustomizationSelected)
                    {
                        SurvivorOutfitCustomizationButtonSelectedGameObject.SetActive(true);
                        SurvivorOutfitCustomizationPlayerFollowCamera.SetActive(true);
                        if (SurvivorOutfitCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow == null)
                        {
                            SurvivorOutfitCustomizationPlayerFollowCamera.GetComponent<CinemachineCamera>().Follow = NetworkClient.localPlayer.GetComponent<CharacterManager>().menuLookAtPoint;
                        }
                        NetworkClient.localPlayer.GetComponent<OutfitManager>().outfitSelectionUI.SetActive(true);
                    }
                    else
                    {
                        SurvivorOutfitCustomizationButtonSelectedGameObject.SetActive(false);
                        SurvivorOutfitCustomizationPlayerFollowCamera.SetActive(false);
                        NetworkClient.localPlayer.GetComponent<OutfitManager>().outfitSelectionUI.SetActive(false);
                    }
                    if (isSettingsMenuSelected)
                    {
                        SettingsMenuButtonSelectedGameObject.SetActive(true);
                        SettingsMenuPlayerFollowCamera.SetActive(true);
                        gameSettings.gameSettingsUI.SetActive(true);
                    }
                    else
                    {
                        SettingsMenuButtonSelectedGameObject.SetActive(false);
                        SettingsMenuPlayerFollowCamera.SetActive(false);
                        gameSettings.gameSettingsUI.SetActive(false);
                    }
                    if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.GetComponent<CharacterManager>().isReady)
                    {
                        readyCheckMark.SetActive(true);
                    }
                    else
                    {
                        readyCheckMark.SetActive(false);
                    }
                    if (NetworkClient.localPlayer != null)
                    {
                        NetworkClient.localPlayer.GetComponent<CharacterManager>().miniMap.SetActive(false);
                        NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().staminaSlider.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (!roomManager.MatchEnding)
                    {
                        ToggleMenu(false);
                        if (NetworkClient.localPlayer != null && !pauseMenuActive)
                        {
                            NetworkClient.localPlayer.GetComponent<CharacterManager>().miniMap.SetActive(roomManager.MatchStarted);
                            NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().staminaSlider.gameObject.SetActive(roomManager.MatchStarted);
                            ToggleUIElements(!roomManager.MatchStarted);
                            if (NetworkClient.localPlayer.GetComponent<HunterAbilities>()._isHunter)
                            {
                                characterCanvas.enabled = roomManager.MatchStarted;
                            }
                        }
                        if (currentPauseMenuToggleInput && !lastTogglePauseMenuInput && roomManager.MatchStarted && !NetworkClient.localPlayer.GetComponent<HunterAbilities>()._inFight && !NetworkClient.localPlayer.GetComponent<EmoteWheel>().inEmoteWheel)
                        {
                            if (pauseMenuActive)
                                TogglePauseMenu(false, false);
                            else
                                TogglePauseMenu(true, false);
                        }
                        if (pauseMenuActive)
                        {
                            if (isPauseMenuSettingsMenuSelected)
                            {
                                PauseMenuSettingsMenuButtonSelectedGameObject.SetActive(true);
                                gameSettings.gameSettingsUI.SetActive(true);
                            }
                            else
                            {
                                PauseMenuSettingsMenuButtonSelectedGameObject.SetActive(false);
                                gameSettings.gameSettingsUI.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        if (NetworkClient.localPlayer != null)
                        {
                            NetworkClient.localPlayer.GetComponent<CharacterManager>().miniMap.SetActive(false);
                            NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().staminaSlider.gameObject.SetActive(false);
                        }
                        ToggleUIElements(true);
                        characterCanvas.enabled = false;
                        if (isPauseMenuSettingsMenuSelected)
                            TogglePauseMenuSettingsMenuWindow(true);
                        if (pauseMenuActive)
                            TogglePauseMenu(false, false);
                    }
                    if (SurvivorCustomizationPlayerFollowCamera.activeInHierarchy)
                        SurvivorCustomizationPlayerFollowCamera.SetActive(false);
                    if (KillerCustomizationPlayerFollowCamera.activeInHierarchy)
                        KillerCustomizationPlayerFollowCamera.SetActive(false);
                    if (ItemShopCamera.activeInHierarchy)
                        ItemShopCamera.SetActive(false);
                    if (ItemShopUI.activeInHierarchy)
                        ItemShopUI.SetActive(false);
                    if (isSurvivorCustomizationSelected)
                        ToggleSurvivorCustomizationWindow(true);
                    if (isKillerCustomizationSelected)
                        ToggleKillerCustomizationWindow(true);
                    if (isSurvivorOutfitCustomizationSelected)
                        ToggleSurvivorOutfitCustomizationWindow(true);
                    if (SettingsMenuPlayerFollowCamera.activeInHierarchy)
                        SettingsMenuPlayerFollowCamera.SetActive(false);
                }
            }

            if (isOffline)
            {
                offlineMainMenuManager.ToggleOfflineMenu(true);
            }
            else
            {
                offlineMainMenuManager.ToggleOfflineMenu(false);
            }

            if (roomManagerMessagesText.text == "")
                roomManagerMessagesContent.SetActive(false);
            else
                roomManagerMessagesContent.SetActive(true);

            lastTogglePauseMenuInput = currentPauseMenuToggleInput;
        }

        public void OnToggleValueChanged()
        {
            int itemsFoundCount = 0;

            foreach (Transform child in ItemShopContent)
            {
                var shopItem = child.GetComponent<ShopItem>();
                if (shopItem != null)
                {
                    bool showItem = false;

                    if (PerkToggle.isOn && shopItem.itemSO.itemType == ItemSO.ItemType.Perk)
                        showItem = true;
                    if (OutfitToggle.isOn && shopItem.itemSO.itemType == ItemSO.ItemType.Outfit)
                        showItem = true;
                    if (KillerToggle.isOn && shopItem.itemSO.itemType == ItemSO.ItemType.Killer)
                        showItem = true;

                    child.gameObject.SetActive(showItem);

                    if (showItem)
                        itemsFoundCount++;
                }
            }

            itemsFoundText.text = itemsFoundCount + " items found";
        }

        private void ToggleMenu(bool state)
        {
            if (state)
            {
                menuCamera.gameObject.SetActive(true);
                menuUI.gameObject.SetActive(true);
                ToggleUIElements(true);
            }
            else
            {
                menuCamera.gameObject.SetActive(false);
                menuUI.gameObject.SetActive(false);
                ToggleUIElements(false);
            }
        }

        public void ToggleSurvivorCustomizationWindow(bool forceClose)
        {
            if (forceClose)
            {
                if (isSurvivorCustomizationSelected)
                {
                    GameObject localPlayer = NetworkClient.localPlayer.gameObject;
                    localPlayer.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().ToggleInventory();
                }
                isSurvivorCustomizationSelected = false;
            }
            else
            {
                if (!isSurvivorCustomizationSelected)
                {
                    ToggleItemShopWindow(true);
                    ToggleKillerCustomizationWindow(true);
                    ToggleSurvivorOutfitCustomizationWindow(true);
                    ToggleSettingsMenuWindow(true);
                    isSurvivorCustomizationSelected = true;
                    challengesWindow.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    chatSystem.ToggleChatSystem(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                }
                else
                {
                    isSurvivorCustomizationSelected = false;
                    challengesWindow.SetActive(true);
                    roomManagerMessagesContent.SetActive(true);
                    chatSystem.ToggleChatSystem(true);
                    countDownText.SetActive(true);
                    CountdownTextTitel.SetActive(true);
                }
                GameObject localPlayer = NetworkClient.localPlayer.gameObject;
                localPlayer.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().ToggleInventory();
            }
        }

        public void ToggleItemShopWindow(bool forceClose)
        {
            if (forceClose)
            {
                isItemShopSelected = false;
            }
            else
            {
                if (!isItemShopSelected)
                {
                    ToggleSurvivorCustomizationWindow(true);
                    ToggleKillerCustomizationWindow(true);
                    ToggleSurvivorOutfitCustomizationWindow(true);
                    ToggleSettingsMenuWindow(true);
                    ItemShopShowAllItems();
                    isItemShopSelected = true;
                    challengesWindow.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    chatSystem.ToggleChatSystem(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                }
                else
                {
                    isItemShopSelected = false;
                    challengesWindow.SetActive(true);
                    roomManagerMessagesContent.SetActive(true);
                    chatSystem.ToggleChatSystem(true);
                    countDownText.SetActive(true);
                    CountdownTextTitel.SetActive(true);
                }
            }
        }

        public void ItemShopShowAllItems()
        {
            foreach (Transform child in ItemShopContent)
            {
                if (child.GetComponent<RedicionStudio.InventorySystem.ShopItem>().itemSO.itemType == RedicionStudio.InventorySystem.ItemSO.ItemType.Killer || child.GetComponent<RedicionStudio.InventorySystem.ShopItem>().itemSO.itemType == RedicionStudio.InventorySystem.ItemSO.ItemType.Outfit)
                {
                    child.gameObject.SetActive(false);
                }
                else
                {
                    child.gameObject.SetActive(true);
                }
            }
        }

        public void ItemShopShowPerkItems()
        {
            foreach (Transform child in ItemShopContent)
            {
                if (child.GetComponent<RedicionStudio.InventorySystem.ShopItem>().itemSO.itemType == RedicionStudio.InventorySystem.ItemSO.ItemType.Perk)
                {
                    child.gameObject.SetActive(true);
                }
                else
                    child.gameObject.SetActive(false);
            }
        }

        public void ToggleKillerCustomizationWindow(bool forceClose)
        {
            if (forceClose)
            {
                isKillerCustomizationSelected = false;
                NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().killerSelectionUI.SetActive(false);
            }
            else
            {
                if (!isKillerCustomizationSelected)
                {
                    ToggleItemShopWindow(true);
                    ToggleSurvivorCustomizationWindow(true);
                    ToggleSurvivorOutfitCustomizationWindow(true);
                    ToggleSettingsMenuWindow(true);
                    isKillerCustomizationSelected = true;
                    challengesWindow.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    chatSystem.ToggleChatSystem(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                    foreach (KillerUIItemButton killerUIButton in NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().killerSelectionButtons)
                    {
                        killerUIButton.CheckItem();
                    }
                }
                else
                {
                    isKillerCustomizationSelected = false;
                    NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().killerSelectionUI.SetActive(false);
                    challengesWindow.SetActive(true);
                    roomManagerMessagesContent.SetActive(true);
                    chatSystem.ToggleChatSystem(true);
                    countDownText.SetActive(true);
                    CountdownTextTitel.SetActive(true);
                    foreach (GameObject model in NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().instantiatedKillerPreviewModels)
                    {
                        if (model != null)
                            Destroy(model);
                    }
                    NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().instantiatedKillerPreviewModels.Clear();
                }
            }
        }

        public void ToggleSurvivorOutfitCustomizationWindow(bool forceClose)
        {
            if (forceClose)
            {
                isSurvivorOutfitCustomizationSelected = false;
                NetworkClient.localPlayer.GetComponent<OutfitManager>().outfitSelectionUI.SetActive(false);
            }
            else
            {
                if (!isSurvivorOutfitCustomizationSelected)
                {
                    ToggleItemShopWindow(true);
                    ToggleSurvivorCustomizationWindow(true);
                    ToggleKillerCustomizationWindow(true);
                    ToggleSettingsMenuWindow(true);
                    isSurvivorOutfitCustomizationSelected = true;
                    challengesWindow.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    chatSystem.ToggleChatSystem(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                    NetworkClient.localPlayer.GetComponent<OutfitManager>().TogglePlayerModel(true);
                }
                else
                {
                    isSurvivorOutfitCustomizationSelected = false;
                    NetworkClient.localPlayer.GetComponent<OutfitManager>().outfitSelectionUI.SetActive(false);
                    challengesWindow.SetActive(true);
                    roomManagerMessagesContent.SetActive(true);
                    chatSystem.ToggleChatSystem(true);
                    countDownText.SetActive(true);
                    CountdownTextTitel.SetActive(true);
                    foreach (GameObject model in NetworkClient.localPlayer.GetComponent<OutfitManager>().instantiatedOutfitPreviewModels)
                    {
                        if (model != null)
                            Destroy(model);
                    }
                    NetworkClient.localPlayer.GetComponent<OutfitManager>().instantiatedOutfitPreviewModels.Clear();
                    NetworkClient.localPlayer.GetComponent<OutfitManager>().TogglePlayerModel(true);
                }
            }
        }

        public void ToggleSettingsMenuWindow(bool forceClose)
        {
            if (forceClose)
            {
                isSettingsMenuSelected = false;
            }
            else
            {
                if (!isSettingsMenuSelected)
                {
                    ToggleItemShopWindow(true);
                    ToggleSurvivorCustomizationWindow(true);
                    ToggleSurvivorOutfitCustomizationWindow(true);
                    ToggleKillerCustomizationWindow(true);
                    isSettingsMenuSelected = true;
                    challengesWindow.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    chatSystem.ToggleChatSystem(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                }
                else
                {
                    isSettingsMenuSelected = false;
                    challengesWindow.SetActive(true);
                    roomManagerMessagesContent.SetActive(true);
                    chatSystem.ToggleChatSystem(true);
                    countDownText.SetActive(true);
                    CountdownTextTitel.SetActive(true);
                }
            }
        }

        public void TogglePauseMenuSettingsMenuWindow(bool forceClose)
        {
            if (forceClose)
            {
                isPauseMenuSettingsMenuSelected = false;
            }
            else
            {
                if (!isPauseMenuSettingsMenuSelected)
                {
                    ToggleItemShopWindow(true);
                    ToggleSurvivorCustomizationWindow(true);
                    ToggleSurvivorOutfitCustomizationWindow(true);
                    ToggleKillerCustomizationWindow(true);
                    ToggleSettingsMenuWindow(true);
                    isPauseMenuSettingsMenuSelected = true;
                    chatSystem.ToggleChatSystem(false);
                }
                else
                {
                    isPauseMenuSettingsMenuSelected = false;
                    chatSystem.ToggleChatSystem(true);
                }
            }
        }

        public void ToggleUIElements(bool _hide)
        {
            foreach (GameObject element in uIElementsToHide)
            {
                if (_hide)
                    element.SetActive(false);
                else
                    element.SetActive(true);
            }
        }

        public void ToggleReadiness()
        {
            NetworkClient.localPlayer.GetComponent<CharacterManager>().ToggleReadiness(false);
        }

        public void LeaveLobby()
        {
            if (networkManager != null)
            {
                if (networkManager.isNetworkActive)
                {
                    foreach (GameObject entry in roomManager.playerListItems)
                    {
                        Destroy(entry);
                    }
                    roomManager.playerListItems.Clear();
                    roomManager.playerList.SetActive(false);

                    if (NetworkClient.localPlayer.GetComponent<OutfitManager>().instantiatedOutfitPreviewModels.Count > 0)
                    {
                        foreach (GameObject model in NetworkClient.localPlayer.GetComponent<OutfitManager>().instantiatedOutfitPreviewModels)
                        {
                            Destroy(model);
                        }

                        NetworkClient.localPlayer.GetComponent<OutfitManager>().instantiatedOutfitPreviewModels.Clear();
                    }
                    if (NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().instantiatedKillerPreviewModels.Count > 0)
                    {
                        foreach (GameObject model in NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().instantiatedKillerPreviewModels)
                        {
                            Destroy(model);
                        }

                        NetworkClient.localPlayer.GetComponent<KillerSelectorManager>().instantiatedKillerPreviewModels.Clear();
                    }
                    characterCanvas.enabled = false;
                    networkManager.StopClient();
                    ToggleOfflineStatus(true);
                    ToggleMenu(false);
                    TogglePauseMenu(false, true);
#if !UNITY_SERVER || UNITY_EDITOR // (Client)
                    foreach (MatchMap matchMap in RoomManager._instance.matchMaps)
                    {
                        matchMap.mapGameObject.SetActive(false);
                    }

                    foreach (MatchMap matchMap in RoomManager._instance.matchMaps)
                    {
                        if (matchMap.mapId == offlineMainMenuManager.defaultMatchMapId)
                        {
                            matchMap.mapGameObject.SetActive(true);
                        }
                    }
                    offlineMainMenuManager.Reconnect();
#endif
                    offlineMainMenuManager.OpenOfflineMenu();
                    Instantiate(loadingScreenPrefab);
                }
            }
        }

        public void ToggleOfflineStatus(bool offline)
        {
            isOffline = offline;
        }

        public void ToggleMainMenuMusic(bool value)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (value)
            {
                isMainMenuMusicPlaying = true;
                fadeCoroutine = StartCoroutine(FadeIn(mainMenuMusic, fadeDuration));
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeOut(mainMenuMusic, fadeDuration));
            }
        }

        public void UIButtonTogglePauseMenu(bool show)
        {
            TogglePauseMenu(show, false);
        }

        public void TogglePauseMenu(bool show, bool offline)
        {
            if (show)
            {
                if (!offline)
                {
                    NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, true);
                    NetworkClient.localPlayer.GetComponent<CharacterManager>().miniMap.SetActive(false);
                    NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().staminaSlider.gameObject.SetActive(false);
                }
                pauseMenuActive = true;
                foreach (MainMenuUIButton uIButton in mainMenuUIButtons)
                {
                    uIButton.buttonSelectedGameObject.SetActive(false);
                    uIButton.buttonHoverGameObject.SetActive(false);
                }
                pauseMenuUI.SetActive(true);
                TPCameraController.LockCursor(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                pauseMenuUI.SetActive(false);
                TogglePauseMenuSettingsMenuWindow(true);
                pauseMenuActive = false;
                if (!offline)
                {
                    if (NetworkClient.localPlayer != null)
                        NetworkClient.localPlayer.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
                }
                TPCameraController.LockCursor(true);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        IEnumerator FadeIn(AudioSource audioSource, float duration)
        {
            float currentTime = 0;
            float startVolume = audioSource.volume;

            audioSource.Play();

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(0, startVolume, currentTime / duration);
                yield return null;
            }

            audioSource.volume = startVolume;
        }

        IEnumerator FadeOut(AudioSource audioSource, float duration)
        {
            float currentTime = 0;
            float startVolume = audioSource.volume;

            while (currentTime < duration)
            {
                currentTime += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0, currentTime / duration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
            isMainMenuMusicPlaying = false;
        }

        public void CloseApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}