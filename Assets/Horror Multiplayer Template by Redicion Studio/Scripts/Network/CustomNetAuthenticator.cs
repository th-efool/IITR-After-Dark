// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class CustomNetAuthenticator : NetworkAuthenticator
    {

        #region Messages

        public struct RequestMessage : NetworkMessage
        {

            public string token;
            public string instanceName;
        }

        public struct ResponseMessage : NetworkMessage
        {

            public byte code;
        }

        #endregion

        #region Server
#if UNITY_SERVER || UNITY_EDITOR

        private System.Collections.IEnumerator Timeout(NetworkConnection conn)
        {
            yield return new WaitForSecondsRealtime(12.0f);
            if (conn != null && !conn.isAuthenticated)
            {
                conn.Disconnect();
            }
        }

        public override void OnServerAuthenticate(NetworkConnection conn)
        {
            StartCoroutine(Timeout(conn));
        }

        public class AuthData
        {

            public readonly MasterServer.AccountData accountData;
            public readonly Instance instance;

            public AuthData(MasterServer.AccountData accountData, Instance instance)
            {
                this.accountData = accountData;
                this.instance = instance;
            }
        }

        private static System.Collections.Generic.SortedSet<string> _pendingDisconnect = new System.Collections.Generic.SortedSet<string>(); // ?
        private System.Collections.IEnumerator DelayedDisconnect(NetworkConnection conn)
        {
            _pendingDisconnect.Add(conn.address); // ?
            yield return new WaitForSecondsRealtime(2.0f);
            conn?.Disconnect(); // Reject?
            _ = _pendingDisconnect.Remove(conn.address);
        }

        private void ServerAccept100(NetworkConnection conn)
        {
            conn.Send(new ResponseMessage
            {
                code = 100
            });
            ServerAccept(conn);
        }

        private void ServerReject(NetworkConnection conn, byte code)
        {
            conn.Send(new ResponseMessage
            {
                code = code
            });
            _ = StartCoroutine(DelayedDisconnect(conn));
        }

        private void OnRequestMessage(NetworkConnection conn, RequestMessage msg)
        {
            if (_pendingDisconnect.Contains(conn.address))
            {
                conn?.Disconnect();
                return;
            }

            if (msg.token == null || msg.token.Length != 32)
            { // ?
                ServerReject(conn, 201);
                return;
            }

            MasterServer.MSClient.CheckToken(msg.token, (accountData) => {
                if (accountData.Id == 0)
                {
                    ServerReject(conn, 203);
                    return;
                }

                if (Instance.instances.TryGetValue(msg.instanceName, out Instance instance))
                {
                    conn.authenticationData = new AuthData(accountData, instance);
                    ServerAccept100(conn);
                    return;
                }

                if (Instance.instances.Count >= Instance.MaxNumOfInstances)
                {
                    ServerReject(conn, 202);
                    return;
                }

                instance = Instance.Create(msg.instanceName);
                conn.authenticationData = new AuthData(accountData, instance);
                ServerAccept100(conn);
            });
        }

        #region Start & Stop

        public override void OnStartServer()
        {
            base.OnStartServer(); // ?

            NetworkServer.RegisterHandler<RequestMessage>(OnRequestMessage, false);
        }

        public override void OnStopServer()
        {
            base.OnStopServer(); // ?

            NetworkServer.UnregisterHandler<RequestMessage>();
        }

        #endregion

#endif
        #endregion

        #region Client
#if !UNITY_SERVER || UNITY_EDITOR // (Client)

        public static string local_token;
        public static string local_instanceName;
        public override void OnClientAuthenticate()
        {
            NetworkClient.connection.Send(new RequestMessage
            {
                token = local_token,
                instanceName = local_instanceName
            });
        }

        public static System.Action<ResponseMessage> OnResponseMessageAction; // ?
        private void OnResponseMessage(ResponseMessage msg)
        {
            Debug.Log("CustomNetAuthenticator->OnResponseMessage->msg.code: " + msg.code);

            OnResponseMessageAction?.Invoke(msg); // ?

            if (msg.code == 100)
            {
                ClientAccept();
                return;
            }

            ClientReject();
        }

        #region Start & Stop

        public override void OnStartClient()
        {
            base.OnStartClient(); // ?

            NetworkClient.RegisterHandler<ResponseMessage>(OnResponseMessage, false);
        }

        public override void OnStopClient()
        {
            base.OnStopClient(); // ?

            NetworkClient.UnregisterHandler<ResponseMessage>();
        }

        #endregion

#endif
        #endregion

        #region Override

#if UNITY_SERVER && !UNITY_EDITOR
	public override void OnClientAuthenticate() { }
#endif
#if !UNITY_SERVER && !UNITY_EDITOR
	public override void OnServerAuthenticate(NetworkConnection conn) { }
#endif

        #endregion
    }
}