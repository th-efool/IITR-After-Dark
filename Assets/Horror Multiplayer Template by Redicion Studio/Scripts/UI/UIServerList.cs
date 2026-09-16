// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using RedicionStudio.MasterServer;
using TMPro;
using System.Collections;
using UnityEngine.Events;

namespace RedicionStudio
{
    public class UIServerList : MonoBehaviour
    {

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
        [Header("Content")]
        [SerializeField] private GameObject _content;

        [Header("Components")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private UIButton _refreshButton;
        [SerializeField] private UIButton _createServerButton;
        [SerializeField] private UIButton _quickJoinButton;
        [SerializeField] private UIButton _createMatchButton;
        [SerializeField] private UnityEngine.UI.InputField _matchNameInputField;
        [SerializeField] private UICategory _uICategory;
        [SerializeField] private UIButton _connectButton;
        [SerializeField] private TextMeshProUGUI _serverNameText;

        [Space]
        public Transform offlineMenuUI;

        [Space]
        [Header("Loading Screen")]
        public GameObject loadingScreenPrefab;

        [Header("Events")]
        public UnityEvent onConnectEvent;
        public UnityEvent onQuickJoinEvent;
        public UnityEvent onCreateMatchEvent;

        private void Connect(string address)
        {
            Mirror.NetworkManager.singleton.networkAddress = address;
            Mirror.NetworkManager.singleton.StartClient();
        }

        public void ToggleServerList(bool _show)
        {
            _content.SetActive(_show);
        }

        private void Awake()
        {
            _content.SetActive(false);
            offlineMenuUI.gameObject.SetActive(false);
            MSClient.OnStateChanged += () => {
                if (MSClient.State == MSClient.NetworkState.Lobby)
                {
                    //_content.SetActive(true);
                    offlineMenuUI.gameObject.SetActive(true);
                    _refreshButton.onPressed.Invoke();
                }
                else
                {
                    //_content.SetActive(false);
                }
            };

            _connectButton.onPressed = () => {
                onConnectEvent.Invoke();
                Instantiate(loadingScreenPrefab); // Newly added
                MSManager.SendPacket(new GetConnectionInfoPacket { InstanceUniqueName = CustomNetAuthenticator.local_instanceName });
            };

            _uICategory.onOptionSelect = instanceUniqueName => {
                CustomNetAuthenticator.local_instanceName = instanceUniqueName;
                _serverNameText.text = "Connect to: " + instanceUniqueName;
                _connectButton.gameObject.SetActive(true);
            };

            MSClient.OnInstancesAction += () => {
                _headerText.text = "<color=#aaa>Server Count:</color> " + MSClient.last_instances.Length;

                _uICategory.ClearOptions();
                for (int i = 0; i < MSClient.last_instances.Length; i++)
                {
                    _uICategory.AddOption(MSClient.last_instances[i]);
                }
            };

            _createServerButton.onPressed = () => {
                UICreateServer.instance.Show((serverName) => {
                    CustomNetAuthenticator.local_instanceName = serverName;
                    _serverNameText.text = "Connect to: " + serverName;
                    _uICategory.SelectOption(null);
                });
            };

            _quickJoinButton.onPressed = () => {
                bool isServerFound = false;

                Instantiate(loadingScreenPrefab);

                onQuickJoinEvent.Invoke();

                for (int i = 0; i < MSClient.last_instances.Length; i++)
                {
                    if (MSClient.last_instances[i].numberOfPlayers < 5)
                    { // Search if a server with less than 5 players is available
                      // Server with less than 5 players found
                        CustomNetAuthenticator.local_instanceName = MSClient.last_instances[i].uniqueName;
                        _serverNameText.text = "Connect to: " + MSClient.last_instances[i].uniqueName;
                        _uICategory.SelectOption(null);
                        MSManager.SendPacket(new GetConnectionInfoPacket { InstanceUniqueName = CustomNetAuthenticator.local_instanceName }); // Enter server
                        isServerFound = true;
                    }
                }
                if (!isServerFound)
                {
                    // All available servers are full or no server has been created yet, so we need to create a server
                    string _serverName = "";
                    bool isServerNameTaken = false;

                    StartCoroutine(CreateServerProcedure());

                    IEnumerator CreateServerProcedure()
                    {
                        isServerNameTaken = false;

                        _serverName = "Match - " + Random.Range(1, 100); // Give the server a randomly generated name

                        // Check if the randomly generated server name matches the name of an already created server
                        for (int i = 0; i < MSClient.last_instances.Length; i++)
                        {
                            if (MSClient.last_instances[i].uniqueName == _serverName)
                            {
                                // Server name is already used

                                isServerNameTaken = true;
                            }
                        }

                        yield return new WaitUntil(() => !isServerNameTaken); // Waits until a server name is generated that does not matches the name of an already created server
                    }

                    CustomNetAuthenticator.local_instanceName = _serverName;
                    _serverNameText.text = "Connect to: " + _serverName;
                    _uICategory.SelectOption(null);

                    StartCoroutine(EnterServerProcedure());

                    IEnumerator EnterServerProcedure()
                    {
                        yield return new WaitForEndOfFrame();

                        MSManager.SendPacket(new GetConnectionInfoPacket { InstanceUniqueName = CustomNetAuthenticator.local_instanceName }); // Enter the server
                    }
                }
            };

            _createMatchButton.onPressed = () => {
                string _serverName = "";
                bool isServerNameTaken = false;

                Instantiate(loadingScreenPrefab);

                onCreateMatchEvent.Invoke();

                StartCoroutine(CreateMatchProcedure());

                IEnumerator CreateMatchProcedure()
                {
                    isServerNameTaken = false;

                    _serverName = "Match - " + _matchNameInputField.text;

                    // Check if the server name matches the name of an already created server
                    for (int i = 0; i < MSClient.last_instances.Length; i++)
                    {
                        if (MSClient.last_instances[i].uniqueName == _serverName)
                        {
                            // Server name is already used
                            isServerNameTaken = true;
                        }
                    }

                    if (isServerNameTaken)
                    {
                        _serverName = _matchNameInputField.text + " " + Random.Range(1, 100); // Give the server a randomly generated name
                    }

                    yield return new WaitUntil(() => !isServerNameTaken); // Waits until a server name is generated that does not matches the name of an already created server

                    CustomNetAuthenticator.local_instanceName = _serverName;
                    _serverNameText.text = "Connect to: " + _serverName;
                    _uICategory.SelectOption(null);

                    StartCoroutine(EnterMatchProcedure());

                    IEnumerator EnterMatchProcedure()
                    {
                        yield return new WaitForEndOfFrame();

                        MSManager.SendPacket(new GetConnectionInfoPacket { InstanceUniqueName = CustomNetAuthenticator.local_instanceName }); // Enter the server
                    }
                }
            };

            MSClient.OnConnectionInfoAction += () => {
                if (string.IsNullOrEmpty(MSClient.lastConnectionInfoPacket.Address) || MSClient.lastConnectionInfoPacket.Address == "full")
                {
                    UIPopup.instance.Show("No servers available");
                    return;
                }
                Connect(MSClient.lastConnectionInfoPacket.Address);
            };

            _refreshButton.onPressed = () => {
                MSManager.SendPacket(new GetInstancesPacket());
                //UIPopup.instance.Show("The list of servers has been updated, keep in mind that server information may update more than 10 seconds after the change to increase the bandwidth of the master server");
            };
        }
#endif
    }
}