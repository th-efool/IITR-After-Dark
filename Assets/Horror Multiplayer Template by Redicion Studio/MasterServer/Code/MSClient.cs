// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using RedicionStudio;

namespace RedicionStudio.MasterServer
{

    public class MSClient : MSManager
    {

        [SerializeField] private string _address = "127.0.0.1";

        public enum NetworkState
        {
            Idle,
            Pending,
            Lobby,
            InGame
        }

        private static NetworkState _state;
        public static Action OnStateChanged;
        public static NetworkState State
        {
            get => _state;
            set
            {
                _state = value;
                OnStateChanged?.Invoke();
            }
        }

        public void Connect()
        {
            NetDataWriter netDataWriter = new NetDataWriter();
#if UNITY_SERVER// || UNITY_EDITOR // (Server)
			netDataWriter.Put(_serverConnectionKey);
#else
            netDataWriter.Put(_connectionKey);
#endif
            _netManager.Connect(_address, _port, netDataWriter);
            State = NetworkState.Pending;
        }

        public void Disconnect()
        {
            if (_netManager != null)
            {
                _netManager.DisconnectAll();
                State = NetworkState.Idle;
            }
        }

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
        public static AuthRequestType localAuthRequestType;
        public static string localUsername;
        public static string localEmail;
        public static string localPassword;
#endif

        private void OnPeerConnected(NetPeer peer)
        {
            Debug.Log("[MS] Connected");

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
            SendPacket(peer, new AuthRequestPacket
            {
                ClientVersion = Convert.ToInt32(Application.version.Replace(".", string.Empty)),
                Type = localAuthRequestType,
                Username = localUsername,
                Email = localEmail,
                EncryptedPassword = Tools.PBKDF2Hash(localPassword),
            });
#else
			Mirror.NetworkManager.singleton.StartServer();
#endif
        }

        public static Action OnConnectionFailed;
        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
#if !UNITY_SERVER || UNITY_EDITOR // (Client)
            isAuthenticated = false;
            State = NetworkState.Idle;
            if (disconnectInfo.Reason == DisconnectReason.ConnectionFailed)
            {
                OnConnectionFailed?.Invoke();
            }
#else
			Application.Quit();
			// Mirror.NetworkManager.singleton.StopServer();
#endif
            Debug.Log("[MS] Disconnected: " + disconnectInfo.Reason.ToString());
        }

        #region Packets;

        public static bool isAuthenticated;

        public static Action<byte, string> OnAuthResponse;
        private void OnAuthResponsePacket(AuthResponsePacket packet)
        {
            Debug.Log("OnAuthResponsePacket: " + packet.Code);

            OnAuthResponse?.Invoke(packet.Code, packet.Token);

            if (packet.Code != 100 && packet.Code != 101)
            {
                _netManager.FirstPeer.Disconnect();
                return;
            }

#if !UNITY_SERVER || UNITY_EDITOR // (Client)
            CustomNetAuthenticator.local_token = packet.Token;
#endif

            isAuthenticated = true;
            State = NetworkState.Lobby;
        }

#if UNITY_SERVER || UNITY_EDITOR // (Server)
        private static Dictionary<string, Action<AccountData>> _tempTokens = new Dictionary<string, Action<AccountData>>();
        public static void CheckToken(string token, Action<AccountData> onResult)
        {
            _tempTokens.Add(token, onResult);
            SendPacket(_netManager.FirstPeer, new AccountDataRequestPacket { Token = token });
        }
        private static void OnAccountDataResponse(AccountDataResponsePacket packet)
        {
            if (_tempTokens.TryGetValue(packet.Token, out Action<AccountData> onResult))
            {
                onResult.Invoke(new AccountData
                {
                    Id = packet.Id,
                    Username = packet.Username,
                    Status = packet.Status,
                    Funds = packet.Funds,
                    OwnsProperty = packet.OwnsProperty,
                    Nutrition = packet.Nutrition,
                    ExperiencePoints = packet.ExperiencePoints,
                    KillerId = packet.KillerId,
                    OutfitId = packet.OutfitId,
                    Escaped = packet.Escaped,
                    KilledPlayers = packet.KilledPlayers,
                    CapturedPlayers = packet.CapturedPlayers,
                    AbilitiesUsed = packet.AbilitiesUsed,
                    HealedHealth = packet.HealedHealth,
                    DamageDealt = packet.DamageDealt,
                    CompletedTasks = packet.CompletedTasks,
                    TimeSurvived = packet.TimeSurvived,
                    HelpedPlayers = packet.HelpedPlayers,
                    InstrumentsUsed = packet.InstrumentsUsed
                });
            }
            _tempTokens.Remove(packet.Token);
        }
#endif

        // Instances
        public static void SendInstances(InstanceInfo[] instances)
        {
            SendPacket(new InstancesPacket
            {
                JSON = JSON.ToJson(instances)
            });
        }

        public static InstanceInfo[] last_instances;
        public static Action OnInstancesAction;
        private static void OnInstances(InstancesPacket packet)
        {
            last_instances = JSON.FromJson<InstanceInfo>(packet.JSON);
            OnInstancesAction?.Invoke();
        }

        public static ConnectionInfoPacket lastConnectionInfoPacket;
        public static Action OnConnectionInfoAction;
        private static void OnConnectionInfo(ConnectionInfoPacket packet)
        {
            lastConnectionInfoPacket = packet;
            OnConnectionInfoAction?.Invoke();
        }

        // Save Load
#if UNITY_SERVER || UNITY_EDITOR
        private static Dictionary<int, Action<MServer.PlacedObjectJSONData[]>> _tempPlacedObjectsPackets = new Dictionary<int, Action<MServer.PlacedObjectJSONData[]>>();
        public static void SavePlacedObjects(int ownerId, MServer.PlacedObjectJSONData[] placedObjects)
        {
            SendPacket(_netManager.FirstPeer, new SavePlacedObjectsPacket
            {
                OwnerId = ownerId,
                JSON = JSON.ToJson(placedObjects)
            });
        }
        public static void GetPlacedObjects(int ownerId, Action<MServer.PlacedObjectJSONData[]> onResult)
        {
            if (_tempPlacedObjectsPackets.ContainsKey(ownerId))
            {
                _tempPlacedObjectsPackets.Remove(ownerId);
            }
            _tempPlacedObjectsPackets.Add(ownerId, onResult);
            SendPacket(_netManager.FirstPeer, new GetPlacedObjectsPacket
            {
                OwnerId = ownerId
            });
        }
        private static void OnPlacedObjects(PlacedObjectsPacket packet)
        {
            if (_tempPlacedObjectsPackets.TryGetValue(packet.OwnerId, out Action<MServer.PlacedObjectJSONData[]> onResult))
            {
                onResult.Invoke(JSON.FromJson<MServer.PlacedObjectJSONData>(packet.JSON));
            }
            _tempPlacedObjectsPackets.Remove(packet.OwnerId);
        }

        private static Dictionary<int, Action<MServer.InventoryJSONData[]>> _tempInventoryPackets = new Dictionary<int, Action<MServer.InventoryJSONData[]>>();
        public static void SaveInventory(int ownerId, MServer.InventoryJSONData[] inventory)
        {
            SendPacket(_netManager.FirstPeer, new SaveInventoryPacket
            {
                OwnerId = ownerId,
                JSON = JSON.ToJson(inventory)
            });
        }
        public static void GetInventory(int ownerId, Action<MServer.InventoryJSONData[]> onResult)
        {
            if (_tempInventoryPackets.ContainsKey(ownerId))
            {
                _tempInventoryPackets.Remove(ownerId);
            }
            _tempInventoryPackets.Add(ownerId, onResult);
            SendPacket(_netManager.FirstPeer, new GetInventoryPacket
            {
                OwnerId = ownerId
            });
        }
        private static void OnInventory(InventoryPacket packet)
        {
            if (_tempInventoryPackets.TryGetValue(packet.OwnerId, out Action<MServer.InventoryJSONData[]> onResult))
            {
                onResult.Invoke(JSON.FromJson<MServer.InventoryJSONData>(packet.JSON));
            }
            _tempInventoryPackets.Remove(packet.OwnerId);
        }
#endif

        protected override void Subscribe()
        {
#if !UNITY_SERVER || UNITY_EDITOR // (Client)
            _netPacketProcessor.SubscribeReusable<AuthResponsePacket>(OnAuthResponsePacket);

            // Instances
            _netPacketProcessor.SubscribeReusable<InstancesPacket>(OnInstances);

            // Connection Info
            _netPacketProcessor.SubscribeReusable<ConnectionInfoPacket>(OnConnectionInfo);
#else
			_netPacketProcessor.SubscribeReusable<AccountDataResponsePacket>(OnAccountDataResponse);

			// Save Load
			_netPacketProcessor.SubscribeReusable<PlacedObjectsPacket>(OnPlacedObjects);

			_netPacketProcessor.SubscribeReusable<InventoryPacket>(OnInventory);
#endif
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            OnPeerConnectedAction += OnPeerConnected;
            OnPeerDisconnectedAction += OnPeerDisconnected;

            if (!_netManager.Start())
            { // ?
                Application.Quit();
                return;
            }
        }

        private void Start()
        {
#if UNITY_SERVER
			Connect();
#endif
        }

        public static int Ping => _netManager.FirstPeer.Ping;
    }
}
