// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using System;

namespace RedicionStudio.MasterServer {

	public abstract class MSManager : MonoBehaviour, INetEventListener {

		[Header("Security")]
		[Tooltip("The key to connect to the master server for clients. Can be obtained by intercepting traffic or by reverse-engineering the code structure. Additional protection in the future is required.")]
		[SerializeField] protected string _connectionKey = "SemiPublicKey";
#if UNITY_SERVER || UNITY_EDITOR // (Server)
		[Tooltip("The key to connect to the master server for servers. Excluded from the client build, stored only in the editor/server. Secure. Never give this key to others.")]
		[SerializeField] protected string _serverConnectionKey = "KeepItSecret";
#endif

		[Header("Connection")]
		[SerializeField] protected int _port = 25500;

		protected static NetManager _netManager;

		protected virtual void Subscribe() { }

		protected virtual void Awake() {
			_netManager = new NetManager(this) {
				AutoRecycle = true
			};
			Subscribe();
		}

		private bool IsRunning => _netManager != null && _netManager.IsRunning; // ?

		private void Update() {
			if (!IsRunning) {
				return;
			}

			_netManager.PollEvents();
		}

		protected virtual void Stop() {
			if (IsRunning) {
				_netManager.Stop();
			}
		}

		#region Unity
		private void OnApplicationQuit() { // ?
			Stop();
		}

		private void OnDestroy() { // ?
			Stop();
		}
		#endregion

		private static readonly NetDataWriter _netDataWriter = new NetDataWriter();
		protected static readonly NetPacketProcessor _netPacketProcessor = new NetPacketProcessor();

		public static void SendPacketSerializable<T>(NetPeer peer, T packet) where T : INetSerializable {
			_netDataWriter.Reset();
			_netDataWriter.Put((byte)50);
			packet.Serialize(_netDataWriter);
			peer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
		}

		public static void SendPacket<T>(NetPeer peer, T packet) where T : class, new() {
			_netDataWriter.Reset();
			_netDataWriter.Put((byte)100);
			_netPacketProcessor.Write(_netDataWriter, packet);
			peer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
		}

		public static void SendPacket<T>(T packet) where T : class, new() {
			if(packet == null || _netManager == null || _netManager.FirstPeer == null) {
				return;
			}
			_netDataWriter.Reset();
			_netDataWriter.Put((byte)100);
			_netPacketProcessor.Write(_netDataWriter, packet);
			_netManager.FirstPeer.Send(_netDataWriter, DeliveryMethod.ReliableOrdered);
		}

		#region INetEventListener
		public static Action<ConnectionRequest> OnConnectionRequestAction;
		void INetEventListener.OnConnectionRequest(ConnectionRequest request) {
			OnConnectionRequestAction?.Invoke(request);
		}

		public static Action<System.Net.IPEndPoint, System.Net.Sockets.SocketError> OnNetworkErrorAction;
		void INetEventListener.OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError) {
			OnNetworkErrorAction?.Invoke(endPoint, socketError);
		}

		public static Action<NetPeer, int> OnNetworkLatencyUpdateAction;
		void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency) {
			OnNetworkLatencyUpdateAction?.Invoke(peer, latency);
		}

		//public static Action<NetPeer, NetPacketReader, DeliveryMethod> OnNetworkReceiveAction;
		void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod) { // ?
			if (deliveryMethod != DeliveryMethod.ReliableOrdered) {
				return;
			}

			byte packetType;
			if (!reader.TryGetByte(out packetType)) {
				return;
			}

			if (packetType == 50) {
				return;
			}

			if (packetType == 100) {
				_netPacketProcessor.ReadAllPackets(reader, peer);
			}
		}

		public static Action<System.Net.IPEndPoint, NetPacketReader, UnconnectedMessageType> OnNetworkReceiveUnconnectedAction;
		void INetEventListener.OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) {
			OnNetworkReceiveUnconnectedAction?.Invoke(remoteEndPoint, reader, messageType);
		}

		public static Action<NetPeer> OnPeerConnectedAction;
		void INetEventListener.OnPeerConnected(NetPeer peer) {
			OnPeerConnectedAction?.Invoke(peer);
		}

		public static Action<NetPeer, DisconnectInfo> OnPeerDisconnectedAction;
		void INetEventListener.OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) {
			OnPeerDisconnectedAction?.Invoke(peer, disconnectInfo);
		}
		#endregion
	}
}
