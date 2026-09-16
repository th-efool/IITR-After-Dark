// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class CustomNetworkManager : NetworkManager
    {
        public delegate void OnPlayerConnected(NetworkConnection conn);
        public OnPlayerConnected NetworkManagerEvent_OnPlayerConnected;

        public static CustomNetworkManager Instance;

        public override void Awake()
        {
            base.Awake();
            Instance = this;

            GameManager.GameBooted = true;
        }

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            base.OnServerAddPlayer(conn);
            NetworkManagerEvent_OnPlayerConnected?.Invoke(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            conn.identity.gameObject.GetComponent<PlayerInteraction>().Disconnect();

            //clears connection and destroys client owned objects
            base.OnServerDisconnect(conn);
        }

    }
}