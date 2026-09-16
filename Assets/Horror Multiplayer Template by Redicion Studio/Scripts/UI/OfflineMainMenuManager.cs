// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using RedicionStudio.MasterServer;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RedicionStudio
{
    public class OfflineMainMenuManager : MonoBehaviour
    {
        public static OfflineMainMenuManager _instance;

        public MainMenuManager mainMenuManager;

        // offline menu
        public Transform offlineMenuUI;
        public GameObject[] offlineMenuObjects;
        public Transform startScreenUI;
        public GameObject[] startScreenObjects;

        // main camera
        public Transform offlineMenuCamera;
        public Transform startScreenCamera;
        bool isStartScreen = true;

        public GameObject loginWindow;

        public MainMenuUIButton[] offlineMainMenuUIButtons;

        public GameObject roomManagerMessagesContent;
        public TMPro.TMP_Text roomManagerMessagesText;

        public GameObject currencyUI;

        public GameObject inventory;

        public GameObject countDownText;
        public GameObject CountdownTextTitel;

        [SerializeField] private UnityEngine.UI.InputField _matchNameInputField;

        [Space]
        [Header("Server List (Offline)")]
        [HideInInspector] public bool isServerListSelected = false;
        public GameObject ServerListButtonSelectedGameObject;
        public GameObject ServerListCamera;
        public Transform ServerListCameraTarget;
        public GameObject ServerListUI;
        public GameObject ServerListConnectButton;
        public Transform ServerListContent;

        [Space]
        [Header("Create Server (Offline)")]
        [HideInInspector] public bool isCreateServerSelected = false;
        public GameObject CreateServerButtonSelectedGameObject;
        public GameObject CreateServerCamera;
        public Transform CreateServerCameraTarget;
        public GameObject CreateServerUI;
        public GameObject CreateServerButton;
        public GameObject CreateServerButtonDisabled;
        public int currentSelectedMapId;
        public ServerCreationMatchMap[] matchMaps;
        public Transform matchMapItems;
        public GameObject matchMapItemPrefab;
        [HideInInspector] public List<MatchMapItem> matchMapItemList = new List<MatchMapItem>();
        public int defaultMatchMapId = 0;

        [Space]
        [Header("Settings Menu")]
        public GameSettingsManager gameSettings;
        [HideInInspector] public bool isSettingsMenuSelected = false;
        public GameObject SettingsMenuButtonSelectedGameObject;
        public GameObject SettingsMenuPlayerFollowCamera;

        public GameObject debugSun;

        [SerializeField] private TMP_InputField _lEmailIF;
        [SerializeField] private TMP_InputField _lPasswordIF;

        public GameObject infoWindowPrefab;

        [Space]
        [Header("Credits")]
        public GameObject creditsPrefab;
        public GameObject creditsCamera;

        [Space]
        [Header("Horror Multiplayer Game Template Information")]
        public GameObject assetInfoPrefab;
        private const string ASSET_INFO_SHOWN_KEY = "AssetInfoShown";
        bool assetInfoShown = false;

        protected void Awake()
        {
            _instance = this;
        }

        private void Start()
        {
            OpenStartScreen();
        }

        private void Update()
        {
            if (isServerListSelected)
            {
                ServerListButtonSelectedGameObject.SetActive(true);
                ServerListCamera.SetActive(true);
                if (ServerListCamera.GetComponent<CinemachineCamera>().Follow == null)
                {
                    ServerListCamera.GetComponent<CinemachineCamera>().Follow = ServerListCameraTarget;
                }
                ServerListUI.SetActive(true);
            }
            else
            {
                ServerListButtonSelectedGameObject.SetActive(false);
                ServerListCamera.SetActive(false);
                ServerListUI.SetActive(false);
            }

            if (isCreateServerSelected)
            {
                CreateServerButtonSelectedGameObject.SetActive(true);
                CreateServerCamera.SetActive(true);
                if (CreateServerCamera.GetComponent<CinemachineCamera>().Follow == null)
                {
                    CreateServerCamera.GetComponent<CinemachineCamera>().Follow = CreateServerCameraTarget;
                }
                CreateServerUI.SetActive(true);
                if (_matchNameInputField.text.Length > 6)
                {
                    CreateServerButton.SetActive(true);
                    CreateServerButtonDisabled.SetActive(false);
                }
                else
                {
                    CreateServerButtonDisabled.SetActive(true);
                    CreateServerButton.SetActive(false);
                }
            }
            else
            {
                CreateServerButtonSelectedGameObject.SetActive(false);
                CreateServerCamera.SetActive(false);
                CreateServerUI.SetActive(false);
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

            if (isStartScreen)
            {
                loginWindow.SetActive(false);
                if (Input.anyKeyDown)
                {
                    isStartScreen = false;
                    startScreenCamera.gameObject.SetActive(false);
                    startScreenUI.gameObject.SetActive(false);
                    loginWindow.SetActive(true);
                    foreach (GameObject gameObject in startScreenObjects)
                        gameObject.SetActive(false);
                    foreach (MainMenuUIButton uIButton in offlineMainMenuUIButtons)
                    {
                        uIButton.buttonSelectedGameObject.SetActive(false);
                        uIButton.buttonHoverGameObject.SetActive(false);
                    }
                    if (isServerListSelected)
                        ToggleServerList(true);
                    if (isCreateServerSelected)
                        ToggleCreateServer(true);
                    if (isSettingsMenuSelected)
                        ToggleSettingsMenuWindow(true);
                    offlineMenuCamera.gameObject.SetActive(true);
                    startScreenCamera.gameObject.SetActive(false);
                    roomManagerMessagesContent.SetActive(false);
                    currencyUI.SetActive(false);
                    inventory.SetActive(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                    foreach (GameObject gameObject in offlineMenuObjects)
                        gameObject.SetActive(true);
                }
            }

            if (!assetInfoShown && offlineMenuUI.gameObject.activeInHierarchy)
            {
                assetInfoShown = true;
                ShowAssetInfo();
            }
        }

        public void OpenStartScreen()
        {
            isStartScreen = true;
            offlineMenuUI.gameObject.SetActive(false);
            startScreenUI.gameObject.SetActive(true);
            foreach (GameObject gameObject in startScreenObjects)
                gameObject.SetActive(true);
            foreach (MainMenuUIButton uIButton in offlineMainMenuUIButtons)
            {
                uIButton.buttonSelectedGameObject.SetActive(false);
                uIButton.buttonHoverGameObject.SetActive(false);
            }
            if (isServerListSelected)
                ToggleServerList(true);
            if (isCreateServerSelected)
                ToggleCreateServer(true);
            if (isSettingsMenuSelected)
                ToggleSettingsMenuWindow(true);
            offlineMenuCamera.gameObject.SetActive(false);
            startScreenCamera.gameObject.SetActive(true);
            roomManagerMessagesContent.SetActive(false);
            currencyUI.SetActive(false);
            inventory.SetActive(false);
            countDownText.SetActive(false);
            CountdownTextTitel.SetActive(false);
            foreach (GameObject gameObject in offlineMenuObjects)
                gameObject.SetActive(false);
        }

        public void OpenOfflineMenu()
        {
            isStartScreen = false;
            startScreenUI.gameObject.SetActive(false);
            offlineMenuUI.gameObject.SetActive(true);
            foreach (GameObject gameObject in startScreenObjects)
                gameObject.SetActive(false);
            foreach (MainMenuUIButton uIButton in offlineMainMenuUIButtons)
            {
                uIButton.buttonSelectedGameObject.SetActive(false);
                uIButton.buttonHoverGameObject.SetActive(false);
            }
            if (isServerListSelected)
                ToggleServerList(true);
            if (isCreateServerSelected)
                ToggleCreateServer(true);
            if (isSettingsMenuSelected)
                ToggleSettingsMenuWindow(true);
            offlineMenuCamera.gameObject.SetActive(true);
            startScreenCamera.gameObject.SetActive(false);
            roomManagerMessagesContent.SetActive(false);
            currencyUI.SetActive(false);
            inventory.SetActive(false);
            countDownText.SetActive(false);
            CountdownTextTitel.SetActive(false);
            foreach (GameObject gameObject in offlineMenuObjects)
                gameObject.SetActive(true);
        }

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
        public void Reconnect()
        {
            StartCoroutine(C_Reconnect());
        }

        IEnumerator C_Reconnect()
        {
            yield return new WaitForSeconds(3f);

            FindObjectOfType<MSClient>().Disconnect();

            yield return new WaitForSeconds(6f);

            MSClient.localAuthRequestType = AuthRequestType.Authorization;
            MSClient.localEmail = _lEmailIF.text;
            MSClient.localPassword = _lPasswordIF.text;
            FindObjectOfType<MSClient>().Connect();
        }
#endif

        public void ToggleOfflineMenu(bool _show)
        {
            if (_show)
            {
                foreach (MainMenuUIButton uIButton in offlineMainMenuUIButtons)
                {
                    uIButton.buttonSelectedGameObject.SetActive(false);
                    uIButton.buttonHoverGameObject.SetActive(false);
                }
                offlineMenuCamera.gameObject.SetActive(true);
                offlineMenuUI.gameObject.SetActive(true);
                roomManagerMessagesContent.SetActive(false);
                currencyUI.SetActive(false);
                inventory.SetActive(false);
                countDownText.SetActive(false);
                CountdownTextTitel.SetActive(false);
                foreach (GameObject gameObject in offlineMenuObjects)
                    gameObject.SetActive(true);
                if (isServerListSelected)
                    ToggleServerList(true);
                if (isCreateServerSelected)
                    ToggleCreateServer(true);
                if (isSettingsMenuSelected)
                    ToggleSettingsMenuWindow(true);
            }
            else
            {
                offlineMenuCamera.gameObject.SetActive(false);
                offlineMenuUI.gameObject.SetActive(false);
                currencyUI.SetActive(true);
                foreach (GameObject gameObject in offlineMenuObjects)
                    gameObject.SetActive(false);
                if (roomManagerMessagesText.text != null)
                    roomManagerMessagesContent.SetActive(true);
                countDownText.SetActive(true);
                CountdownTextTitel.SetActive(true);
            }
        }

        public void ToggleServerList(bool forceClose)
        {
            if (forceClose)
            {
                isServerListSelected = false;
            }
            else
            {
                if (!isServerListSelected)
                {
                    isServerListSelected = true;
                    ToggleCreateServer(true);
                    if (isSettingsMenuSelected)
                        ToggleSettingsMenuWindow(true);
                    ServerListConnectButton.SetActive(false);
                    foreach (Transform child in ServerListContent)
                    {
                        ServerInfoUIButton entry = child.GetComponent<ServerInfoUIButton>();
                        if (entry != null && entry != this)
                        {
                            entry.Deselect();
                        }
                    }
                }
                else
                {
                    isServerListSelected = false;
                }
            }
        }

        public void ToggleCreateServer(bool forceClose)
        {
            if (forceClose)
            {
                isCreateServerSelected = false;
            }
            else
            {
                if (!isCreateServerSelected)
                {
                    isCreateServerSelected = true;
                    ToggleServerList(true);
                    if (isSettingsMenuSelected)
                        ToggleSettingsMenuWindow(true);
                    _matchNameInputField.text = "My match";

                    foreach (Transform child in matchMapItems)
                    {
                        Destroy(child.gameObject);
                    }
                    matchMapItemList.Clear();

                    foreach (ServerCreationMatchMap matchMap in matchMaps)
                    {
                        MatchMapItem matchMapItem = Instantiate(matchMapItemPrefab, matchMapItems).GetComponent<MatchMapItem>();
                        matchMapItem.SetUpMatchMapItem(matchMap.name, matchMap.mapImage, matchMap.mapId);

                        matchMapItemList.Add(matchMapItem);
                    }
                    foreach (MatchMapItem mapItem in matchMapItemList)
                    {
                        if (mapItem.mapId == defaultMatchMapId)
                        {
                            mapItem.selectedUIElement.SetActive(true);
                        }
                    }
                }
                else
                {
                    isCreateServerSelected = false;
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
                    isSettingsMenuSelected = true;
                    roomManagerMessagesContent.SetActive(false);
                    countDownText.SetActive(false);
                    CountdownTextTitel.SetActive(false);
                    if (isServerListSelected)
                        ToggleServerList(true);
                    if (isCreateServerSelected)
                        ToggleCreateServer(true);
                }
                else
                {
                    isSettingsMenuSelected = false;
                }
            }
        }

        public void ShowInfoWindow(string _infoText)
        {
            Instantiate(infoWindowPrefab).GetComponent<InfoWindowManager>().OpenInfoWindow(_infoText);
        }

        public void ShowCredits(bool _show)
        {
            if(_show)
            {
                creditsCamera.SetActive(true);
                offlineMenuCamera.gameObject.SetActive(false);
                offlineMenuUI.gameObject.SetActive(false);
                Instantiate(creditsPrefab);
            }
            else
            {
                offlineMenuCamera.gameObject.SetActive(true);
                offlineMenuUI.gameObject.SetActive(true);
                creditsCamera.SetActive(false);
            }
        }

        public void ShowAssetInfo()
        {
            if (PlayerPrefs.GetInt(ASSET_INFO_SHOWN_KEY, 0) == 0)
            {
                Instantiate(assetInfoPrefab).GetComponent<AssetWindowManager>().OpenInfoWindow();

                PlayerPrefs.SetInt(ASSET_INFO_SHOWN_KEY, 1);
                PlayerPrefs.Save();
            }
        }
    }

    [System.Serializable]
    public class ServerCreationMatchMap
    {
        public string name;
        public int mapId;
        public Sprite mapImage;
    }
}