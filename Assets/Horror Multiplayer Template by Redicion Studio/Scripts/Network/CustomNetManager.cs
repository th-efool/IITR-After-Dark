// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CustomNetManager : NetworkManager
    {

        public GameObject instancePrefab;

        public delegate void OnPlayerConnected(NetworkConnection conn);
        public OnPlayerConnected NetworkManagerEvent_OnPlayerConnected;

        public override void OnServerAddPlayer(NetworkConnection conn)
        {
#if UNITY_SERVER || UNITY_EDITOR // (Server)
            CustomNetAuthenticator.AuthData authData = (CustomNetAuthenticator.AuthData)conn.authenticationData;
            conn.authenticationData = null;

            GameObject gO = Instantiate(playerPrefab);
            gO.name = authData.accountData.Username;

            Player player = gO.GetComponent<Player>();

            player.id = authData.accountData.Id;
            player.username = authData.accountData.Username;
            player.status = authData.accountData.Status;
            player.instance = authData.instance;
            player.funds = authData.accountData.Funds;
            player.playerNutrition.value = authData.accountData.Nutrition;
            player.experiencePoints = authData.accountData.ExperiencePoints;
            player.killerId = authData.accountData.KillerId;
            player.outfitId = authData.accountData.OutfitId;
            player.escaped = authData.accountData.Escaped;
            player.killedPlayers = authData.accountData.KilledPlayers;
            player.capturedPlayers = authData.accountData.CapturedPlayers;
            player.abilitiesUsed = authData.accountData.AbilitiesUsed;
            player.healedHealth = authData.accountData.HealedHealth;
            player.damageDealt = authData.accountData.DamageDealt;
            player.completedTasks = authData.accountData.CompletedTasks;
            player.timeSurvived = authData.accountData.TimeSurvived;
            player.helpedPlayers = authData.accountData.HelpedPlayers;
            player.instrumentsUsed = authData.accountData.InstrumentsUsed;

            player.playerInventory.LoadInventory();

            gO.transform.position = authData.instance.transform.position;

            player.instance.AddPlayer(player.id, player);
            Debug.Log(authData.accountData.Username + " has authenticated (" + authData.instance.uniqueName + ')');
            NetworkServer.AddPlayerForConnection(conn, gO);
            NetworkManagerEvent_OnPlayerConnected?.Invoke(conn);
#endif
        }

        /*public override void OnServerDisconnect(NetworkConnection conn) {
            base.OnServerDisconnect(conn);
    #if UNITY_SERVER || UNITY_EDITOR // (Server)
            if (conn != null && conn.identity != null) {
                Player player = conn.identity.GetComponent<Player>();
                player.instance.RemovePlayer(player.id);
            }
    #endif
        }*/

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            conn.identity.gameObject.GetComponent<PlayerInteraction>().Disconnect();

            //clears connection and destroys client owned objects
            base.OnServerDisconnect(conn);
        }

        private void SendInstances()
        {
            if (Instance.instances.Count < 1)
            {
                MasterServer.MSClient.SendInstances(new MasterServer.InstanceInfo[0]);
                return;
            }
            MasterServer.InstanceInfo[] instances = new MasterServer.InstanceInfo[Instance.instances.Count];
            int i = 0;
            foreach (KeyValuePair<string, Instance> instance in Instance.instances)
            {
                instances[i] = new MasterServer.InstanceInfo
                {
                    uniqueName = instance.Value.uniqueName,
                    numberOfPlayers = instance.Value.players.Count,
                    ping = MasterServer.MSClient.Ping
                };
                i++;
            }
            MasterServer.MSClient.SendInstances(instances);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

#if UNITY_SERVER // (Server)
		InvokeRepeating("SendInstances", 15f, 15f);
#endif
        }

        public override void OnStopServer()
        {
            base.OnStopServer();

#if UNITY_SERVER || UNITY_EDITOR // (Server)
            CancelInvoke();
#endif
        }

        public override void OnStopClient()
        {
            base.OnStopClient();

            MasterServer.MSClient.State = MasterServer.MSClient.NetworkState.Lobby;
        }

#if UNITY_EDITOR
        public override void OnValidate()
        {
            base.OnValidate(); // ?

            dontDestroyOnLoad = false;
            runInBackground = true;
            autoStartServerBuild = false;
            networkAddress = string.Empty;
            maxConnections = 256;
        }
#endif
    }
}