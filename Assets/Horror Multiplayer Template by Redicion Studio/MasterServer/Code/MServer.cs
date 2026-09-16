// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
#if UNITY_SERVER || UNITY_EDITOR // (Server)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Linq;
using RedicionStudio;

namespace RedicionStudio.MasterServer {

	public class MServer : MSManager {

		[Header("Other")]
		[SerializeField] private int _maxConnections = 100;
		[SerializeField] private int _minClientVersionToConnect;

		public class NetPeerInfo {

			public NetPeer peer;

			public string EndPointStr => peer.EndPoint.ToString();

			// Server
			public bool isServer;
			public InstanceInfo[] instances;

			// Client
			public bool isAuthenticated;
			public string token;
			public AccountData accountData;
		}

		private static Dictionary<string, NetPeerInfo> _connectedPeers = new Dictionary<string, NetPeerInfo>();

		public static bool IsOnline(int accountId) {
			try {
				return _connectedPeers.Values.Where(value => value.accountData != null && value.accountData.Id == accountId).Any();
			}
			catch {
				return false;
			}
		}

		public static AccountData GetAccountData(string token) {
			try {
				return _connectedPeers.Values.Where(value => value.token == token).First().accountData;
			}
			catch {
				return null;
			}
		}

		public static NetPeerInfo IsServer(string endPointStr) {
			if (_connectedPeers.ContainsKey(endPointStr) && _connectedPeers[endPointStr].isServer) {
				return _connectedPeers[endPointStr];
			}
			return null;
		}

		public static bool IsAuthenticated(string endPointStr) {
			if (_connectedPeers.TryGetValue(endPointStr, out NetPeerInfo peerInfo)) {
				return peerInfo.isAuthenticated;
			}
			return false;
		}

		#region On Connected

		[SerializeField, Tooltip("Seconds")] private float _authTimeout = 10.0f;
		private IEnumerator AuthTimeout(NetPeerInfo peerInfo) {
			yield return new WaitForSecondsRealtime(_authTimeout);
			if (!peerInfo.isAuthenticated) {
				peerInfo.peer.Disconnect();
			}
		}

		private void OnClientConnected(NetPeerInfo peerInfo) {
			StartCoroutine(AuthTimeout(peerInfo));
		}

		private void OnServerConnected(NetPeerInfo peerInfo) {
		}

		#endregion

		private void OnConnectionRequest(ConnectionRequest request) {
			if (_netManager.ConnectedPeersCount < _maxConnections) {
				string key;
				if (!request.Data.TryGetString(out key) || key == null) {
					request.RejectForce();
					return;
				}

				if (key == _connectionKey) {
					string endPointStr = request.RemoteEndPoint.ToString();
					NetPeerInfo peerInfo = new NetPeerInfo {
						peer = request.Accept()
					};
					_connectedPeers.Add(endPointStr, peerInfo);
					OnClientConnected(peerInfo);
					Debug.Log(endPointStr + " has connected (C)");
					return;
				}

				if (key == _serverConnectionKey) {
					string endPointStr = request.RemoteEndPoint.ToString();
					NetPeerInfo peerInfo = new NetPeerInfo {
						peer = request.Accept(),
						isServer = true
					};
					_connectedPeers.Add(endPointStr, peerInfo);
					OnServerConnected(peerInfo);
					Debug.Log(endPointStr + " has connected (S)");
					return;
				}
			}

			request.RejectForce();
		}

		private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
			string endPointStr = peer.EndPoint.ToString();
			if (_connectedPeers.Remove(endPointStr)) {
				Debug.Log(endPointStr + " has disconnected");
			}
		}

		#region Auth
		#region AuthBase
		private void AuthAccept(NetPeerInfo peerInfo, AccountData accountData, byte code) {
			peerInfo.isAuthenticated = true;

			string token = Tools.GetToken();
			while (GetAccountData(token) != null) {
				token = Tools.GetToken();
			}

			peerInfo.token = token;
			peerInfo.accountData = accountData;

			SendPacket(peerInfo.peer, new AuthResponsePacket { Code = code, Token = token });
		}

		private IEnumerator DelayedDisconnect(NetPeer peer) {
			yield return new WaitForSecondsRealtime(2.0f);
			peer?.Disconnect();
		}

		private void AuthReject(NetPeer peer, byte code) {
			SendPacket(peer, new AuthResponsePacket { Code = code });
			StartCoroutine(DelayedDisconnect(peer));
		}
		#endregion

		private void OnAccountCreation(AuthRequestPacket packet, NetPeerInfo who) {
			if (!Tools.IsValidUsername(packet.Username)) {
				AuthReject(who.peer, 214);
				return;
			}

			if (!Tools.IsValidEmail(packet.Email)) {
				AuthReject(who.peer, 212);
				return;
			}

			if (string.IsNullOrWhiteSpace(packet.EncryptedPassword)) {
				AuthReject(who.peer, 213);
				return;
			}

			AccountData accountData = Database.CreateAccount(packet.Email, packet.EncryptedPassword, packet.Username);
			if (accountData == null) {
				AuthReject(who.peer, 207);
				return;
			}

			AuthAccept(who, accountData, 100);
		}

		private void OnAuthorization(AuthRequestPacket packet, NetPeerInfo who) {
			if (!Tools.IsValidEmail(packet.Email) || string.IsNullOrWhiteSpace(packet.EncryptedPassword)) {
				AuthReject(who.peer, 203);
				return;
			}

			AccountData accountData = Database.GetAccountData(packet.Email, packet.EncryptedPassword);
			if (accountData == null) {
				AuthReject(who.peer, 203);
				return;
			}

			if (IsOnline(accountData.Id)) {
				AuthReject(who.peer, 205);
				return;
			}

			AuthAccept(who, accountData, 101);
		}

		private void OnAuthRequestPacket(AuthRequestPacket packet, NetPeer who) {
			if (!_connectedPeers.TryGetValue(who.EndPoint.ToString(), out NetPeerInfo peerInfo)) {
				who.Disconnect();
				return;
			}

			if (packet.ClientVersion < _minClientVersionToConnect) {
				AuthReject(who, 202);
				return;
			}

			switch (packet.Type) {
				case AuthRequestType.AccountCreation:
					OnAccountCreation(packet, peerInfo);
					return;
				case AuthRequestType.Authorization:
					OnAuthorization(packet, peerInfo);
					return;
			}

			who.Disconnect();
		}

		private static void OnAccountDataRequest(AccountDataRequestPacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			AccountData accountData = GetAccountData(packet.Token);
			if (accountData != null) {
				SendPacket(who, new AccountDataResponsePacket {
					Token = packet.Token,
					Id = accountData.Id,
					Username = accountData.Username,
					Status = accountData.Status,
					Funds = accountData.Funds,
					OwnsProperty = accountData.OwnsProperty,
					Nutrition = accountData.Nutrition,
                    ExperiencePoints = accountData.ExperiencePoints,
                    KillerId = accountData.KillerId,
                    OutfitId = accountData.OutfitId,
                    Escaped = accountData.Escaped,
                    KilledPlayers = accountData.KilledPlayers,
                    CapturedPlayers = accountData.CapturedPlayers,
                    AbilitiesUsed = accountData.AbilitiesUsed,
                    HealedHealth = accountData.HealedHealth,
                    DamageDealt = accountData.DamageDealt,
                    CompletedTasks = accountData.CompletedTasks,
                    TimeSurvived = accountData.TimeSurvived,
                    HelpedPlayers = accountData.HelpedPlayers,
                    InstrumentsUsed = accountData.InstrumentsUsed
                });
			}
			else {
				SendPacket(who, new AccountDataResponsePacket {
					Token = packet.Token,
					Id = 0
				});
			}
		}

		private static void OnAccountDataResponse(AccountDataResponsePacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			Database.UpdateAccountData(packet.Id, packet.Funds, packet.OwnsProperty, packet.Nutrition, packet.ExperiencePoints, packet.KillerId, packet.OutfitId, packet.Escaped, packet.KilledPlayers, packet.CapturedPlayers, packet.AbilitiesUsed, packet.HealedHealth, packet.DamageDealt, packet.CompletedTasks, packet.TimeSurvived, packet.HelpedPlayers, packet.InstrumentsUsed);
		}
		#endregion

		// Instances
		private static void OnInstances(InstancesPacket packet, NetPeer who) { // From Server
			NetPeerInfo peerInfo = IsServer(who.EndPoint.ToString());
			if (peerInfo == null) {
				who.Disconnect(); // Ban?
				return;
			}

			peerInfo.instances = JSON.FromJson<InstanceInfo>(packet.JSON);
			Debug.Log(peerInfo.EndPointStr + ": " + peerInfo.instances?.Length + " instances");
		}

		private static void OnGetInstances(GetInstancesPacket packet, NetPeer who) { // From Client
			if (!IsAuthenticated(who.EndPoint.ToString())) {
				who.Disconnect();
				return;
			}

			if (_connectedPeers.Count < 1) {
				return;
			}

			List<InstanceInfo> instances = new List<InstanceInfo>();
			foreach (NetPeerInfo peerInfo in _connectedPeers.Values) {
				if (peerInfo.instances == null) {
					continue; // ?
				}
				instances.AddRange(peerInfo.instances);
			}

			SendPacket(who, new InstancesPacket {
				JSON = JSON.ToJson(instances.ToArray())
			});
		}

		// Connection Info
		private static void OnGetConnectionInfo(GetConnectionInfoPacket packet, NetPeer who) {
			if (!IsAuthenticated(who.EndPoint.ToString())) {
				who.Disconnect();
				return;
			}

			if (string.IsNullOrWhiteSpace(packet.InstanceUniqueName)) {
				return;
			}
			foreach (NetPeerInfo peerInfo in _connectedPeers.Values.Where(value => value.instances != null)) {
				if (peerInfo.instances.Where(value => value.uniqueName == packet.InstanceUniqueName).Count() > 0) {
					SendPacket(who, new ConnectionInfoPacket {
						Address = peerInfo.peer.EndPoint.Address.ToString()
					});
					return;
				}
			}
			IEnumerable<NetPeerInfo> iEnumerable = _connectedPeers.Values.Where(value => value.instances == null || value.instances.Length < Instance.MaxNumOfInstances);
			SendPacket(who, new ConnectionInfoPacket {
				Address = iEnumerable.Any() ? iEnumerable.First().peer.EndPoint.Address.ToString() : "full"
			});
		}


		// Save Load
		[System.Serializable]
		public class PlacedObjectJSONData {

			public string placeableSOUniqueName;
			public float x;
			public float y;
			public float z;
			public float rotX;
			public float rotY;
			public float rotZ;
			public float rotW;
		}

		private static void OnGetPlacedObjects(GetPlacedObjectsPacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			PlacedObjectData[] placedObjects = Database.GetPlacedObjects(packet.OwnerId);

			PlacedObjectJSONData[] jsonData = new PlacedObjectJSONData[placedObjects.Length];
			for (int i = 0; i < placedObjects.Length; i++) {
				jsonData[i] = new PlacedObjectJSONData {
					placeableSOUniqueName = placedObjects[i].UniqueName,
					x = placedObjects[i].X,
					y = placedObjects[i].Y,
					z = placedObjects[i].Z,
					rotX = placedObjects[i].RotX,
					rotY = placedObjects[i].RotY,
					rotZ = placedObjects[i].RotZ,
					rotW = placedObjects[i].RotW
				};
			}

			SendPacket(who, new PlacedObjectsPacket {
				OwnerId = packet.OwnerId,
				JSON = JSON.ToJson(jsonData)
			});
		}

		[System.Serializable]
		public class InventoryJSONData {

			public int hash;
			public int amount;
			public float shelfLife;
		}

		private static void OnGetInventory(GetInventoryPacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			InventoryData[] inventoryData = Database.GetInventory(packet.OwnerId);

			InventoryJSONData[] jsonData = new InventoryJSONData[inventoryData.Length];
			for (int i = 0; i < inventoryData.Length; i++) {
				jsonData[i] = new InventoryJSONData {
					hash = inventoryData[i].Hash,
					amount = inventoryData[i].Amount,
					shelfLife = inventoryData[i].ShelfLife
				};
			}

			SendPacket(who, new InventoryPacket {
				OwnerId = packet.OwnerId,
				JSON = JSON.ToJson(jsonData)
			});
		}

		private static void OnSavePlacedObjects(SavePlacedObjectsPacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			PlacedObjectJSONData[] placedObjects = JSON.FromJson<PlacedObjectJSONData>(packet.JSON);
			if (placedObjects == null || placedObjects.Length < 1) { // ?
				return;
			}

			PlacedObjectData[] dbData = new PlacedObjectData[placedObjects.Length];
			for (int i = 0; i < placedObjects.Length; i++) {
				dbData[i] = new PlacedObjectData {
					OwnerId = packet.OwnerId,
					UniqueName = placedObjects[i].placeableSOUniqueName,
					X = placedObjects[i].x,
					Y = placedObjects[i].y,
					Z = placedObjects[i].z,
					RotX = placedObjects[i].rotX,
					RotY = placedObjects[i].rotY,
					RotZ = placedObjects[i].rotZ,
					RotW = placedObjects[i].rotW
				};
			}

			Database.DeletePlacedObjects(packet.OwnerId);
			Database.SavePlacedObjects(dbData);
		}

		private static void OnSaveInventory(SaveInventoryPacket packet, NetPeer who) {
			if (IsServer(who.EndPoint.ToString()) == null) {
				who.Disconnect();
				return;
			}

			InventoryJSONData[] inventoryJSONData = JSON.FromJson<InventoryJSONData>(packet.JSON);
			if (inventoryJSONData == null || inventoryJSONData.Length < 1) { // ?
				return;
			}

			InventoryData[] dbData = new InventoryData[inventoryJSONData.Length];
			for (int i = 0; i < inventoryJSONData.Length; i++) {
				dbData[i] = new InventoryData {
					OwnerId = packet.OwnerId,
					Hash = inventoryJSONData[i].hash,
					Amount = inventoryJSONData[i].amount,
					ShelfLife = inventoryJSONData[i].shelfLife
				};
			}

			Database.DeleteInventory(packet.OwnerId);
			Database.SaveInventory(dbData);
		}

		protected override void Subscribe() {
			_netPacketProcessor.SubscribeReusable<AuthRequestPacket, NetPeer>(OnAuthRequestPacket);
			_netPacketProcessor.SubscribeReusable<AccountDataRequestPacket, NetPeer>(OnAccountDataRequest);
			_netPacketProcessor.SubscribeReusable<AccountDataResponsePacket, NetPeer>(OnAccountDataResponse);

			// Instances
			_netPacketProcessor.SubscribeReusable<InstancesPacket, NetPeer>(OnInstances); // From Server
			_netPacketProcessor.SubscribeReusable<GetInstancesPacket, NetPeer>(OnGetInstances); // From Client

			// Connection Info
			_netPacketProcessor.SubscribeReusable<GetConnectionInfoPacket, NetPeer>(OnGetConnectionInfo); // From Client

			// Save Load
			_netPacketProcessor.SubscribeReusable<GetPlacedObjectsPacket, NetPeer>(OnGetPlacedObjects); // From Server
			_netPacketProcessor.SubscribeReusable<SavePlacedObjectsPacket, NetPeer>(OnSavePlacedObjects); // From Server

			// Inventory Save Load
			_netPacketProcessor.SubscribeReusable<GetInventoryPacket, NetPeer>(OnGetInventory); // From Server
			_netPacketProcessor.SubscribeReusable<SaveInventoryPacket, NetPeer>(OnSaveInventory); // From Server
		}

		protected override void Awake() {
			base.Awake();

			OnConnectionRequestAction += OnConnectionRequest;
			OnPeerDisconnectedAction += OnPeerDisconnected;

			if (!_netManager.Start(_port)) { // ?
				Application.Quit();
				return;
			}

			Database.OpenConnection();
		}

		protected override void Stop() {
			base.Stop();

			Database.CloseConnection();
		}
	}
}
#endif
