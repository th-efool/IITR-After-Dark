// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using UnityEngine.UI;
using RedicionStudio.InventorySystem;
using Unity.VisualScripting;

namespace RedicionStudio
{
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager _instance;

        //keeping track of players
        [HideInInspector] public List<CharacterManager> players = new List<CharacterManager>();
        [HideInInspector] public List<CharacterManager> teamA = new List<CharacterManager>();
        [HideInInspector] public List<CharacterManager> teamB = new List<CharacterManager>();
        public Transform playerListContent;
        public GameObject playerListItemPrefab;
        public GameObject playerList;
        [HideInInspector] public List<GameObject> playerListItems = new List<GameObject>();

        public bool FreezePlayersWhenGameNotReady = true;

        //countdown
        private int secondsToStartTheGame = 0;
        public int playersNeededToStart = 5;
        public int maxPlayers = 5;

        //playtime
        [SyncVar] public int playTime;
        Coroutine c_recordPlayTime;
        public int maxPlayTime = 1800;

        //countdown when enough players are present
        public int enoughPlayersCountdown = 60;

        //countdown when server full
        public int allPlayersCountdown = 10;

        //number of seconds added to countdown when player joins or disconnect
        public int addToCountdown = 10;

        //weapon that is given to hunter
        //public GameObject HunterItem;

        Coroutine c_endMatchSchedule;
        Coroutine c_endMatch;
        Coroutine c_countdownToStartMatch;

        [SyncVar] public bool MatchRunning = false;
        [SyncVar] public bool MatchEnding = false;
        [SyncVar] public bool MatchStarted = false;

        [SyncVar] [HideInInspector] public bool activatePoliceCar = false;

        public delegate void NewPlayerSpawned();
        public NewPlayerSpawned RoomEvent_NewPlayerSpawned;

        public delegate void PlayerDespawned();
        public PlayerDespawned RoomEvent_PlayerDespawned;

        public delegate void NewRound();
        public NewRound RoomEvent_NewRound;

        public delegate void MatchCountdown(int seconds);
        public MatchCountdown RoomEvent_MatchCountdown;

        [Header("Loading Screen")]
        public GameObject loadingScreenPrefab;
        [Header("Fade Screen")]
        public GameObject fadeScreenPrefab;
        [Header("Camera")]
        public GameObject showPlayerCameraPrefab;
        [Header("ClimbableObstacle")]
        public GameObject climbableObstacleFrontPrefab;
        public GameObject climbableObstacleBackPrefab;

        [Space]
        [Header("Audio")]
        public AudioClip matchStartedSound;
        public AudioClip matchEndedSound;

        [Space]
        //[SyncVar(hook = nameof(OnCurrentSelectedMatchMapIdChanged))]
        [SyncVar] public int currentSelectedMatchMapId;
        public MatchMap[] matchMaps;
        [SyncVar] public bool isCurrentSelectedMatchMapIdSet = false;

        [Space]
        public MainMenuManager mainMenuManager;
        public Transform mainLobbyMenuCamera;

        [Space]
        [Header("Spectator UI")]
        public Text spectatorText;

        [Space]
        [Header("Server Message")]
        public GameObject serverMessagePrefab;

        [Space]
        public EscapeManager escapeManager;

        private string[] hunterWonMessages = new string[]
        {
        "The Hunter Prevails! Blood stains the ground...",
        "Killer's Victory! Darkness claims its victims...",
        "The Hunt Ends in Death... The Killer Reigns Supreme.",
        "Death's Grasp Tightens! The Hunter Emerges Victorious.",
        "The Hunter's Fury Unleashed! None Shall Escape.",
        "Survivors Fall Before the Killer's Blade... Darkness Consumes All.",
        "Slaughtered Souls! The Killer's Reign of Terror Knows No Mercy.",
        "Survivors Meet Their Demise! The Hunter's Hunger is Sated.",
        "In the Shadows, Death Lurks... The Killer's Victory is Absolute.",
        "The Hunt Ends in Carnage! The Killer Claims Their Prize."
        };

        private string[] survivorWonMessages = new string[]
        {
        "In the shadows, they elude. But for how long?",
        "Survival: a fleeting victory amidst the encroaching darkness.",
        "Breathing, yet not truly alive. The Survivors persist.",
        "Escaped, but the nightmare lingers.",
        "A reprieve, not redemption. The Survivors endure.",
        "For now, the abyss remains unsated. Survivors persist.",
        "Escape achieved, but at what cost? The darkness waits.",
        "They flee, but the specter of their ordeal remains.",
        "Among the living, yet haunted. The Survivors persevere.",
        "Evaded, but not forgotten. The shadows hunger still."
        };

        protected void Awake()
        {
            /*if (!GameManager.GameBooted)
                SceneManager.LoadScene(0);
            else
                _instance = this;*/

            _instance = this;

            players.Clear();
        }
        private void Start()
        {

            if (GameManager.GameBooted)
            {
                CustomNetworkManager.Instance.NetworkManagerEvent_OnPlayerConnected += NewConnection;
                NetworkManager.singleton.maxConnections = maxPlayers;
            }
        }

        //disconnect player who wants to join during match
        void NewConnection(NetworkConnection conn)
        {
            if (MatchRunning || players.Count >= maxPlayers)
                conn.Disconnect();
        }
        void LaunchMatch()
        {
            RpcPlayMatchSound(true, false);

            NetworkManager.singleton.maxConnections = players.Count; //limiting space after match starded to prevent new players from joining during game

            teamA.Clear();
            teamB.Clear();

            //if which player is dead resurrect him, if not, replenish his health
            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<CharacterManager>().isReady = false;
                players[i].GetComponent<CharacterManager>().RpcSetReadiness(false);

                if (players[i].health <= 0)
                    players[i].Rpc_Ressurect();
                else
                    players[i].health = players[i].maxHealth;

                players[i].TempEscaped = 0;
                players[i].TempKilledPlayers = 0;
                players[i].TempCapturedPlayers = 0;
                players[i].TempAbilitiesUsed = 0;
                players[i].TempHealedHealth = 0;
                players[i].TempDamageDealt = 0;
                players[i].TempCompletedTasks = 0;
                players[i].TempTimeSurvived = 0;
                players[i].TempHelpedPlayers = 0;
                players[i].TempInstrumentsUsed = 0;
            }

            //selecting and setting up hunter
            int hunterID = Random.Range(0, players.Count);

            PlayerSetTeam(players[hunterID], 1);

            Rpc_InitializePerks(players[hunterID].GetComponent<NetworkIdentity>(), true);

            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    Rpc_SetPlayerPosition(players[hunterID].GetComponent<NetworkIdentity>(), matchMap.hunterSpawnpoint.position, matchMap.hunterSpawnpoint.rotation);
                }
            }
            //players[hunterID].transform.position = hunterSpawnpoint.position;
            //players[hunterID].transform.rotation = hunterSpawnpoint.rotation;
            //players[hunterID].GetComponent<NetworkTransform>().RpcTeleportAndRotate(hunterSpawnpoint.position, hunterSpawnpoint.rotation);
            //StartCoroutine(TeleportToNextHunterSpawnpoint(players[hunterID].GetComponent<NetworkTransform>(), 9));
            //players[hunterID].GetComponent<CharacterManager>().ClearInvetory();
            foreach (HunterAbilities.Hunter hunter in players[hunterID].GetComponent<HunterAbilities>().hunters)
            {
                if (hunter.HunterID.Equals(players[hunterID].GetComponent<Player>().killerId))
                {
                    players[hunterID].GetComponent<CharacterManager>().Give(hunter.hunterWeaponItem, 1);
                }
            }

            // setting rest of the players
            for (int i = 0; i < players.Count; i++)
            {
                if (i != hunterID) // hunter is already set up
                {
                    PlayerSetTeam(players[i], 0);
                    //players[i].GetComponent<CharacterManager>().ClearInvetory();

                    Transform nexSpawnpoint = NextMatchSpawnpoint();

                    Rpc_SetPlayerPosition(players[i].GetComponent<NetworkIdentity>(), nexSpawnpoint.position, nexSpawnpoint.rotation);
                    //players[i].transform.position = nexSpawnpoint.position;
                    //players[i].transform.rotation = nexSpawnpoint.rotation;
                    //players[i].GetComponent<NetworkTransform>().RpcTeleportAndRotate(nexSpawnpoint.position, nexSpawnpoint.rotation);
                    //StartCoroutine(TeleportToNextSpawnpoint(players[i].GetComponent<NetworkTransform>(), 9));

                    Rpc_InitializePerks(players[i].GetComponent<NetworkIdentity>(), false);
                }
            }
            MatchRunning = true;
            c_recordPlayTime = StartCoroutine(RecordPlayTime());

            RoomEvent_NewRound?.Invoke();

            RpcUpdatePlayerList(true);
            RpcUpdateItemSelectedIndicator(players[hunterID].GetComponent<NetworkIdentity>(), 1);

            StartCoroutine(SetMovement());

            IEnumerator SetMovement()
            {
                yield return new WaitForSeconds(10);

                Rpc_ShowMapName();

                yield return new WaitForSeconds(8);

                MatchStarted = true;

                for (int i = 0; i < players.Count; i++)
                    players[i].Rpc_SetMovementPermission(true);
            }

            foreach(MatchMap matchMap in matchMaps)
            {
                if(currentSelectedMatchMapId == matchMap.mapId)
                {
                    foreach (MatchObject matchObject in matchMap.matchObjects)
                    {
                        if (matchObject.isItemContainer && matchObject.instantiatedObject != null)
                        {
                            foreach (GameObject instantiatedItem in matchObject.instantiatedObject.GetComponent<ItemContainerManager>().instantiatedItems)
                            {
                                if (instantiatedItem != null)
                                    NetworkServer.Destroy(instantiatedItem);
                            }
                        }

                        if (matchObject.isKnockableObstacle)
                        {
                            if (matchObject.instantiatedObject != null && matchObject.instantiatedObject.GetComponent<KnockableObstacle>().associatedClimbableObstacles != null)
                            {
                                foreach(ClimbableObstacle climbableObstacle in matchObject.instantiatedObject.GetComponent<KnockableObstacle>().associatedClimbableObstacles)
                                {
                                    NetworkServer.Destroy(climbableObstacle.gameObject);
                                }

                                matchObject.instantiatedObject.GetComponent<KnockableObstacle>().associatedClimbableObstacles = null;
                            }
                        }

                        if (matchObject.instantiatedObject != null)
                        {
                            NetworkServer.Destroy(matchObject.instantiatedObject);
                        }

                        matchObject.instantiatedObject = Instantiate(matchObject.objectPrefab, matchObject.spawnPoint.position, matchObject.spawnPoint.rotation);
                        if(matchObject.setScale)
                        {
                            matchObject.instantiatedObject.transform.localScale = matchObject.spawnPoint.localScale;
                        }
                        NetworkServer.Spawn(matchObject.instantiatedObject);
                        if (matchObject.isKnockableObstacle && matchObject.instantiatedObject != null)
                        {
                            GameObject[] spawnedObstacles = new GameObject[2];

                            GameObject instantiatedClimbableObstacleFront = Instantiate(climbableObstacleFrontPrefab, matchObject.climbableObstacleFrontPosition.position, matchObject.climbableObstacleFrontPosition.rotation);
                            GameObject instantiatedClimbableObstacleBack = Instantiate(climbableObstacleBackPrefab, matchObject.climbableObstacleBackPosition.position, matchObject.climbableObstacleBackPosition.rotation);

                            spawnedObstacles[0] = instantiatedClimbableObstacleFront;
                            spawnedObstacles[1] = instantiatedClimbableObstacleBack;

                            NetworkServer.Spawn(instantiatedClimbableObstacleFront);
                            NetworkServer.Spawn(instantiatedClimbableObstacleBack);

                            RpcUpdateClimbableObstacles(matchObject.instantiatedObject.GetComponent<KnockableObstacle>(), spawnedObstacles);
                            matchObject.instantiatedObject.GetComponent<KnockableObstacle>().reinstated = true;
                            matchObject.instantiatedObject.GetComponent<KnockableObstacle>().ServerRestoreKnockableObstacle();
                        }
                        if (matchObject.instantiatedObject != null && matchObject.instantiatedObject.GetComponent<OpenableDoorManager>() != null)
                        {
                            matchObject.instantiatedObject.GetComponent<OpenableDoorManager>().SetUpDoors();
                        }
                    }
                }
            }
        }

        public IEnumerator RecordPlayTime()
        {
            while (playTime < maxPlayTime)
            {
                yield return new WaitForSeconds(1);
                if (isServer)
                    playTime++;
            }
            if (!MatchEnding && MatchRunning && MatchStarted)
                EndMatch(0); //the maximum playing time has been reached, the game is over and the survivors have survived
        }

        int lastMatchSpawnPointId = 0; //remember last used spawnpoint id to not spawn two players at one point
        Transform NextMatchSpawnpoint()
        {
            MatchMap currnetMatchMap = null;

            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    currnetMatchMap = matchMap;
                }
            }

            if (lastMatchSpawnPointId >= currnetMatchMap.survivorMatchSpawnpoints.Length)
                lastMatchSpawnPointId = 0;

            Transform nextSpawnPoint = currnetMatchMap.survivorMatchSpawnpoints[lastMatchSpawnPointId];

            lastMatchSpawnPointId++;

            return nextSpawnPoint;
        }
        int spawnPointId = 0; //remember last used spawnpoint id to not spawn two players at one point
        Transform NextSpawnpoint()
        {
            MatchMap currnetMatchMap = null;

            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    currnetMatchMap = matchMap;
                }
            }

            if (spawnPointId >= currnetMatchMap.survivorSpawnpoints.Length)
                spawnPointId = 0;

            Transform nextSpawnPoint = currnetMatchMap.survivorSpawnpoints[spawnPointId];

            spawnPointId++;

            return nextSpawnPoint;
        }

        IEnumerator TeleportToNextMatchSpawnpoint(NetworkTransform networkTransform, float time)
        {
            MatchMap currnetMatchMap = null;

            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    currnetMatchMap = matchMap;
                }
            }

            if (networkTransform != null)
            {
                int spawnPointId = (lastMatchSpawnPointId + 1) % currnetMatchMap.survivorMatchSpawnpoints.Length;

                // Get the next spawn point
                Transform nextSpawnPoint = currnetMatchMap.survivorMatchSpawnpoints[spawnPointId];

                // Teleport the player to the next spawn point
                networkTransform.RpcTeleportAndRotate(nextSpawnPoint.position, nextSpawnPoint.rotation);

                yield return new WaitForSeconds(time);

                /*// Wait until the player has reached the target position
                while (Vector3.Distance(networkTransform.transform.position, nextSpawnPoint.position) > 0.1f)
                {
                    yield return null;
                }*/

                // Update the last spawn point id
                lastMatchSpawnPointId = spawnPointId;
            }
        }

        private int lastSpawnPointId = 0;
        private List<int> usedSpawnPointIds = new List<int>();

        IEnumerator TeleportToNextSpawnpoint(NetworkTransform networkTransform, float time)
        {
            MatchMap currnetMatchMap = null;

            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    currnetMatchMap = matchMap;
                }
            }

            if (networkTransform != null)
            {
                float timer = 0f;
                int spawnPointId = GetNextAvailableSpawnPointId(currnetMatchMap);
                while (timer < time)
                {
                    Transform nextSpawnPoint = currnetMatchMap.survivorSpawnpoints[spawnPointId];

                    networkTransform.RpcTeleportAndRotate(nextSpawnPoint.position, nextSpawnPoint.rotation);
                    timer += Time.deltaTime;
                    yield return null;
                }

                /*while (Vector3.Distance(networkTransform.transform.position, nextSpawnPoint.position) > 0.1f)
                {
                    yield return null;
                }*/

                usedSpawnPointIds.Add(spawnPointId);
            }
        }

        private int GetNextAvailableSpawnPointId(MatchMap currentMatchMap)
        {
            if (usedSpawnPointIds.Count == currentMatchMap.survivorSpawnpoints.Length)
            {
                ShuffleSpawnPointIds(currentMatchMap);
            }

            int spawnPointId = lastSpawnPointId;
            while (usedSpawnPointIds.Contains(spawnPointId))
            {
                spawnPointId = (spawnPointId + 1) % currentMatchMap.survivorSpawnpoints.Length;
            }

            lastSpawnPointId = spawnPointId;

            return spawnPointId;
        }

        private void ShuffleSpawnPointIds(MatchMap currentMatchMap)
        {
            for (int i = 0; i < currentMatchMap.survivorSpawnpoints.Length; i++)
            {
                int temp = Random.Range(i, currentMatchMap.survivorSpawnpoints.Length);
                int tempId = usedSpawnPointIds[temp];
                usedSpawnPointIds[temp] = usedSpawnPointIds[i];
                usedSpawnPointIds[i] = tempId;
            }
        }

        private void ResetUsedSpawnPointIds()
        {
            usedSpawnPointIds.Clear();
        }

        IEnumerator TeleportToNextHunterSpawnpoint(NetworkTransform networkTransform, float time)
        {
            if (networkTransform != null)
            {
                float timer = 0f;
                while (timer < time)
                {
                    foreach (MatchMap matchMap in matchMaps)
                    {
                        if (currentSelectedMatchMapId == matchMap.mapId)
                        {
                            networkTransform.RpcTeleportAndRotate(matchMap.hunterSpawnpoint.position, matchMap.hunterSpawnpoint.rotation);
                        }
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }

                /*// Wait until the hunter has reached the target position
                while (Vector3.Distance(networkTransform.transform.position, hunterSpawnpoint.position) > 0.1f)
                {
                    yield return null;
                }*/
            }
        }

        //launched when new player connected to the game
        public void RegisterPlayerInGame(GameObject _player)
        {
            CharacterManager newPlayer = _player.GetComponent<CharacterManager>();
            players.Add(newPlayer);

            if (isServer)
            {
                if (MatchRunning || MatchStarted)
                {
                    RpcKickPlayerFromMatch(_player.GetComponent<NetworkIdentity>());
                }
                else
                {
                    RpcLoadMatchMap(currentSelectedMatchMapId);

                    if (FreezePlayersWhenGameNotReady)
                        newPlayer.Rpc_SetMovementPermission(false);

                    Transform nexSpawnpoint = NextSpawnpoint();

                    newPlayer.transform.position = nexSpawnpoint.position;
                    newPlayer.transform.rotation = nexSpawnpoint.rotation;
                    newPlayer.GetComponent<NetworkTransform>().RpcTeleportAndRotate(nexSpawnpoint.position, nexSpawnpoint.rotation);

                    //StartCoroutine(TeleportToNextSpawnpoint(newPlayer.GetComponent<NetworkTransform>()));

                    //if game is counting to start
                    if (c_countdownToStartMatch != null)
                    {
                        if (players.Count < maxPlayers)
                        {
                            secondsToStartTheGame += addToCountdown;
                            Rpc_RoomMessage("Another player joined, 10 seconds have been added.", 5f);
                        }
                        else
                        {
                            Rpc_RoomMessage("Lobby is full, starting in 10 seconds!", 5f);
                        }
                    }

                    //when players are joining set them in default team
                    Rpc_SetTeamForPlayer(_player.GetComponent<NetworkIdentity>(), 0);
                    RpcShowServerMessage("<color=#FF0900>" + _player.GetComponent<Player>().username + "</color><color=#A4A4A4> has joined the match</color>");

                    CheckIfMatchCanBeStarted();
                }

            }
            if (MatchRunning || MatchStarted)
            {

            }
            else
            {
                RoomEvent_NewPlayerSpawned?.Invoke();
            }

        }
        void CheckIfMatchCanBeStarted()
        {
            //checking if game can start
            if (players.Count >= playersNeededToStart)
            {
                //if enough players and countdown is not running then run it.
                if (c_countdownToStartMatch == null)
                {
                    secondsToStartTheGame = enoughPlayersCountdown;
                    c_countdownToStartMatch = StartCoroutine(CountdownToStartMatch());
                }

                if (players.Count == maxPlayers)
                {
                    secondsToStartTheGame = allPlayersCountdown;
                }
            }
            else
            {
                //if there is not enough players to start, but countdown to start was running, then shut it down
                if (c_countdownToStartMatch != null)
                {
                    StopCoroutine(c_countdownToStartMatch);
                    c_countdownToStartMatch = null;
                }

                RoomEvent_MatchCountdown?.Invoke(0);

                Rpc_RoomMessage(playersNeededToStart - players.Count + " player(s) needed to start the match", 999f);
            }
        }
        //launched when player disconnects from the game
        public void DeRegisterPlayerFromGame(GameObject _player)
        {
            players.Remove(_player.GetComponent<CharacterManager>());

            if (isServer)
            {
                //if player was in team remove him from team list
                CharacterManager _registeredPlayer = _player.GetComponent<CharacterManager>();
                if (teamA.Contains(_registeredPlayer))
                {
                    teamA.Remove(_registeredPlayer);
                }
                else if (teamB.Contains(_registeredPlayer))
                {
                    teamB.Remove(_registeredPlayer);
                }


                if (MatchRunning)
                {
                    NetworkManager.singleton.maxConnections = players.Count; //limiting space after someone disconnected to prevent someone else to replece him during match
                    CheckTeamsState();
                    RpcUpdatePlayerList(true);
                }
                else
                    CheckIfMatchCanBeStarted();

                secondsToStartTheGame += addToCountdown;

                if (c_countdownToStartMatch != null)
                    Rpc_RoomMessage("Player left, added " + addToCountdown + " seconds!", 5f);
                RpcShowServerMessage("<color=#FF0900>" + _registeredPlayer.GetComponent<Player>().username + "</color><color=#A4A4A4> has left the match</color>");
            }


            RoomEvent_PlayerDespawned?.Invoke();
        }

        #region team managament
        //method used to assign giver player to given team
        void PlayerSetTeam(CharacterManager _player, byte _team)
        {
            _player.Team = _team;
            if (_team == 0)
            {
                teamA.Add(_player);
            }
            else
            {
                teamB.Add(_player);
            }

            Rpc_SetTeamForPlayer(_player.netIdentity, _team);
        }

        [ClientRpc]
        void Rpc_SetTeamForPlayer(NetworkIdentity _player, byte _team)
        {
            _player.GetComponent<CharacterManager>().SetTeam(_team);
        }
        #endregion

        public void NewDeadPlayer(CharacterManager _deadPlayer)
        {
            CheckTeamsState();
        }

        //method launched when some player dies or disconnect
        //check if some team is completely dead or absent
        void CheckTeamsState()
        {

            if (!MatchRunning) return;

            if (teamA.Count > 0)
            {
                if (!teamAlive(teamA))
                {
                    if (!MatchEnding)
                        EndMatch(1);
                }
            }
            else
            {
                //win by opposite team A absence //A=0
                if (!MatchEnding)
                    EndMatch(1);
                return;
            }
            if (teamB.Count > 0)
            {
                if (!teamAlive(teamB))
                {
                    if (!MatchEnding)
                        EndMatch(0);
                }
            }
            else
            {
                //win by opposite team B absence //B=1
                if (!MatchEnding)
                    EndMatch(0);
                return;
            }

            bool teamAlive(List<CharacterManager> _team)
            {
                for (int i = 0; i < _team.Count; i++)
                {
                    if (_team[i].health > 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        IEnumerator CountdownToStartMatch()
        {

            Rpc_RoomMessage("Enough players have joined, countdown started!", 4f);

            while (!MatchRunning)
            {
                yield return new WaitForSecondsRealtime(1f);

                bool allPlayersReady = true;

                for (int i = 0; i < players.Count; i++)
                {
                    if (!players[i].isReady)
                    {
                        allPlayersReady = false;
                        break;
                    }
                }

                if (allPlayersReady && secondsToStartTheGame > 10)
                {
                    secondsToStartTheGame = 10;
                    Rpc_RoomMessage("All players are ready, countdown is set to 10!", 5f);
                }

                secondsToStartTheGame--;

                Rpc_MatchCountdown(secondsToStartTheGame);

                if (secondsToStartTheGame == 0)
                {
                    Rpc_RoomMessage("Match Started!", 3f);
                    Rpc_ShowLoadingScreen();
                    Rpc_ShowPlayer();
                    LaunchMatch();
                }
            }
            Rpc_MatchCountdown(0);
            c_countdownToStartMatch = null;
        }

        [ClientRpc]
        void Rpc_MatchCountdown(int _seconds)
        {
            RoomEvent_MatchCountdown?.Invoke(_seconds);
        }

        public void EndMatch(int _winnerTeam)
        {
            if (!MatchRunning) return;

            RpcPlayMatchSound(false, true);

            MatchEnding = true;

            StartCoroutine(WaitForEndMatchCoroutine());

            IEnumerator WaitForEndMatchCoroutine()
            {
                yield return new WaitForSeconds(5f);

                Rpc_ShowFadeScreen();

                yield return new WaitForSeconds(3.5f);

                RpcResetSpectatorText();

                for (int i = 0; i < players.Count; i++)
                {
                    if (players[i].alive)
                        players[i].TempTimeSurvived = playTime;
                    SetFirstPersonMode(players[i].GetComponent<NetworkIdentity>(), false);
                    if (FreezePlayersWhenGameNotReady)
                        players[i].Rpc_SetMovementPermission(false);
                    players[i].GetComponent<CharacterManager>().ClearInvetory();
                    if (players[i].GetComponent<HunterAbilities>()._isHunter)
                    {
                        foreach (MatchMap matchMap in matchMaps)
                        {
                            if (currentSelectedMatchMapId == matchMap.mapId)
                            {
                                players[i].transform.position = matchMap.hunterMatchEndedSpawnpoint.position;
                                players[i].transform.rotation = matchMap.hunterMatchEndedSpawnpoint.rotation;
                                players[i].GetComponent<NetworkTransform>().RpcTeleportAndRotate(matchMap.hunterMatchEndedSpawnpoint.position, matchMap.hunterMatchEndedSpawnpoint.rotation);
                            }
                        }
                    }
                    RpcSetBandage(players[i].GetComponent<NetworkIdentity>(), false);
                    players[i].GetComponent<CharacterManager>().isHealing = false;
                }

                //Rpc_RoomMessage(_winnerTeam == 0 ? "Survivors won!" : "Hunter won!", 5f);
                string message;

                if (_winnerTeam == 0) // Survivors won
                {
                    message = survivorWonMessages[Random.Range(0, survivorWonMessages.Length)];
                }
                else // Hunter/Killer won
                {
                    message = hunterWonMessages[Random.Range(0, hunterWonMessages.Length)];
                }

                Rpc_RoomMessage(message, 20f);

                for (int i = 0; i < players.Count; i++)
                {
                    GivePlayerRewards(players[i].GetComponent<NetworkIdentity>(), players[i].GetComponent<HunterAbilities>()._isHunter, players[i].GetComponent<CharacterManager>().alive);
                    RpcShowPlayerMatchStatistics(players[i].GetComponent<NetworkIdentity>(), players[i].GetComponent<HunterAbilities>()._isHunter, players[i].GetComponent<CharacterManager>().alive);
                }

                yield return new WaitForSeconds(20f);

                Rpc_ShowLoadingScreen();

                yield return new WaitForSeconds(2f);

                RpcUpdatePlayerList(false);

                NetworkManager.singleton.maxConnections = maxPlayers;

                MatchRunning = false;

                MatchStarted = false;

                if (c_endMatchSchedule != null)
                {
                    StopCoroutine(c_endMatchSchedule);
                    c_endMatchSchedule = null;
                }

                StopCoroutine(RecordPlayTime());
                playTime = 0;

                c_endMatch = StartCoroutine(EndMatchCoroutine(_winnerTeam));
            }
        }

        IEnumerator EndMatchCoroutine(int _winnerTeam)
        {
            yield return new WaitForSeconds(2);

            MatchEnding = false;

            //reset all players
            for (int i = 0; i < players.Count; i++)
            {
                players[i].GetComponent<Animator>().Rebind();
                players[i].GetComponent<Animator>().enabled = true;
                players[i].GetComponent<CapsuleCollider>().enabled = true;
                players[i].alive = true;
                players[i].health = players[i].maxHealth;
                Rpc_RebindPlayerAnimator(players[i].GetComponent<NetworkIdentity>());

                players[i].GetComponent<CharacterManager>().escaped = false;
                Rpc_SetEscapedStatus(players[i].GetComponent<NetworkIdentity>(), false);

                PlayerSetTeam(players[i], 0);

                //players[i].GetComponent<CharacterManager>().ClearInvetory();

                Transform nexSpawnpoint = NextSpawnpoint();

                players[i].transform.position = nexSpawnpoint.position;
                players[i].transform.rotation = nexSpawnpoint.rotation;
                players[i].GetComponent<NetworkTransform>().RpcTeleportAndRotate(nexSpawnpoint.position, nexSpawnpoint.rotation);

                players[i].GetComponent<HunterAbilities>().SetSurvivor();
                RpcDisableHunterMesh(players[i].GetComponent<NetworkIdentity>());
            }

            Rpc_TogglePoliceCar(false);

            c_endMatchSchedule = StartCoroutine(EndMatchSchedule());

            IEnumerator EndMatchSchedule()
            {
                yield return new WaitForSeconds(3f);
                CheckIfMatchCanBeStarted();
            }
        }

        #region gamemode events
        public bool ReviveRandomDeadPlayer()
        {
            List<CharacterManager> deadPlayers = new List<CharacterManager>();
            for (int i = 0; i < teamA.Count; i++)
            {
                if (teamA[i].health <= 0)
                    deadPlayers.Add(teamA[i]);
            }

            if (deadPlayers.Count == 0)
            {
                //no characters to revive
                return false;
            }
            else
            {
                int deadPlayerToReviveID = Random.Range(0, deadPlayers.Count);
                deadPlayers[deadPlayerToReviveID].isSheriff = true;
                deadPlayers[deadPlayerToReviveID].Rpc_SetSheriff();
                foreach (MatchMap matchMap in matchMaps)
                {
                    if (currentSelectedMatchMapId == matchMap.mapId)
                    {
                        deadPlayers[deadPlayerToReviveID].transform.position = matchMap.revivedPlayerSpawnpoint.position;
                        deadPlayers[deadPlayerToReviveID].transform.rotation = matchMap.revivedPlayerSpawnpoint.rotation;
                        deadPlayers[deadPlayerToReviveID].GetComponent<NetworkTransform>().RpcTeleportAndRotate(matchMap.revivedPlayerSpawnpoint.position, matchMap.revivedPlayerSpawnpoint.rotation);
                    }
                }
                deadPlayers[deadPlayerToReviveID].health = deadPlayers[deadPlayerToReviveID].maxHealth;
                deadPlayers[deadPlayerToReviveID].Rpc_Ressurect();
                deadPlayers[deadPlayerToReviveID].Give(deadPlayers[deadPlayerToReviveID].sheriffWeapon, 1);
                //deadPlayers[deadPlayerToReviveID].Rpc_SetMovementPermission(true);
                StartCoroutine(ReviveRandomDeadPlayerCoroutine(deadPlayers[deadPlayerToReviveID]));
                Rpc_TogglePoliceCar(true);
                return true;
            }
        }

        IEnumerator ReviveRandomDeadPlayerCoroutine(CharacterManager character)
        {
            yield return new WaitForSeconds(11);

            character.Rpc_SetMovementPermission(true);
        }
        #endregion

        #region rooom messages

        public delegate void RoomManagerMessage(string _msg, float _liveTime);
        public RoomManagerMessage RoomEvent_Message;

        [ClientRpc]
        void Rpc_RoomMessage(string _msg, float _liveTime)
        {
            RoomEvent_Message?.Invoke(_msg, _liveTime);
        }
        #endregion

        [ClientRpc]
        public void RpcUpdatePlayerList(bool showPlayerList)
        {
            if (showPlayerList)
            {
                if (NetworkClient.localPlayer.GetComponent<CharacterManager>().Team == 1)
                    playerList.SetActive(false);
                else
                    playerList.SetActive(true);
                // Destroy existing player list items
                foreach (GameObject entry in playerListItems)
                {
                    Destroy(entry);
                }
                playerListItems.Clear();

                // Instantiate new player list items for each player
                for (int i = 0; i < players.Count; i++)
                {
                    NetworkIdentity playerNetId = players[i].GetComponent<NetworkIdentity>();
                    if (playerNetId != null && playerNetId.GetComponent<CharacterManager>().Team == 0)
                    {
                        if (MatchStarted && playerNetId.GetComponent<HunterAbilities>()._isHunter)
                            continue;

                        NetworkIdentity _player = playerNetId;
                        GameObject newPlayerListItem = Instantiate(playerListItemPrefab, playerListContent);
                        newPlayerListItem.GetComponent<UIPlayerListItem>().playerNameText.text = _player.GetComponent<Player>().username;
                        newPlayerListItem.GetComponent<UIPlayerListItem>().healthText.text = "Health: " + _player.GetComponent<CharacterManager>().health + "/" + _player.GetComponent<CharacterManager>().maxHealth;
                        OutfitManager _playerOutfitManager = _player.GetComponent<OutfitManager>();
                        foreach (OutfitItem outfit in _playerOutfitManager.outfits)
                        {
                            if (outfit.outfitID == _player.GetComponent<Player>().outfitId)
                            {
                                newPlayerListItem.GetComponent<UIPlayerListItem>().playerImage.sprite = outfit.outfitImage;
                            }
                        }
                        playerListItems.Add(newPlayerListItem);
                    }
                }
            }
            else
            {
                playerList.SetActive(false);
            }
        }

        [ClientRpc]
        void Rpc_SetEscapedStatus(NetworkIdentity networkID, bool status)
        {
            networkID.GetComponent<CharacterManager>().escaped = status;
        }

        [ClientRpc]
        void Rpc_RebindPlayerAnimator(NetworkIdentity networkID)
        {
            networkID.GetComponent<Animator>().Rebind();
            networkID.GetComponent<Animator>().enabled = true;
            networkID.GetComponent<CapsuleCollider>().enabled = true;
            networkID.GetComponent<CharacterManager>().alive = true;
            networkID.GetComponent<CharacterManager>().isSheriff = false;
            networkID.GetComponent<OutfitManager>().SetPreviousOutfit();
            networkID.GetComponent<CharacterManager>().health = networkID.GetComponent<CharacterManager>().maxHealth;
        }

        [ClientRpc]
        void Rpc_TogglePoliceCar(bool status)
        {
            activatePoliceCar = status;
        }

        [ClientRpc]
        void Rpc_ShowLoadingScreen()
        {
            Instantiate(loadingScreenPrefab);
        }

        [ClientRpc]
        void Rpc_ShowFadeScreen()
        {
            Instantiate(fadeScreenPrefab);
        }

        [ClientRpc]
        void Rpc_ShowMapName()
        {
            foreach (MatchMap matchMap in matchMaps)
            {
                if (currentSelectedMatchMapId == matchMap.mapId)
                {
                    Instantiate(matchMap.showMapNamePrefab).GetComponent<ShowMapNameManager>().ShowMapName(matchMap.name, matchMap.description);
                }
            }
        }

        [ClientRpc]
        void Rpc_InitializePerks(NetworkIdentity playerNetId, bool isHunter)
        {
            playerNetId.GetComponent<PerkManager>().Initialize(isHunter);
        }

        [ClientRpc]
        void Rpc_SetPlayerPosition(NetworkIdentity playerNetId, Vector3 position, Quaternion rotation)
        {
            playerNetId.GetComponent<Player>().SetPlayerPosition(position, rotation);
        }

        private void OnDestroy()
        {
            if (GameManager.GameBooted)
                CustomNetworkManager.Instance.NetworkManagerEvent_OnPlayerConnected -= NewConnection;
        }

        [ClientRpc]
        void RpcShowPlayerMatchStatistics(NetworkIdentity playerNetId, bool isHunter, bool survived)
        {
            playerNetId.GetComponent<CharacterManager>().ShowMatchStatistics(playerNetId.GetComponent<CharacterManager>().TempTimeSurvived, playerNetId.GetComponent<CharacterManager>().TempDamageDealt, playerNetId.GetComponent<CharacterManager>().TempCompletedTasks, playerNetId.GetComponent<CharacterManager>().TempHelpedPlayers, playerNetId.GetComponent<CharacterManager>().TempInstrumentsUsed, playerNetId.GetComponent<CharacterManager>().TempKilledPlayers, isHunter, survived);
        }

        void GivePlayerRewards(NetworkIdentity playerNetId, bool isHunter, bool survived)
        {
            if (isServer)
            {
                ExperienceManager _experienceManager = playerNetId.GetComponent<ExperienceManager>();
                Player _player = playerNetId.GetComponent<Player>();

                int calculatedXPAmount = _player.timeSurvived / 5 * _experienceManager.currentLevel / 5 +
                                          _player.damageDealt / 5 * _experienceManager.currentLevel / 5 +
                                          _player.completedTasks * _experienceManager.currentLevel / 2 * 7 +
                                          _player.helpedPlayers * _experienceManager.currentLevel / 2 * 7 +
                                          _player.instrumentsUsed * _experienceManager.currentLevel / 2 * 7 +
                                          _player.killedPlayers * _experienceManager.currentLevel / 2 * 7;

                _player.SetExperience(calculatedXPAmount);

                int earnedMoney = 0;

                if (survived)
                {
                    earnedMoney += Mathf.RoundToInt(_player.timeSurvived / 5) * _experienceManager.currentLevel;
                }

                earnedMoney += Mathf.RoundToInt(_player.damageDealt / 5) * _experienceManager.currentLevel;

                earnedMoney += _player.completedTasks * _experienceManager.currentLevel * 7;

                earnedMoney += _player.helpedPlayers * _experienceManager.currentLevel * 7;

                earnedMoney += _player.instrumentsUsed * _experienceManager.currentLevel * 7;

                earnedMoney += _player.killedPlayers * _experienceManager.currentLevel * 7;

                playerNetId.GetComponent<PlayerInteractionModule>().AddMoney(playerNetId.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>(), earnedMoney);
            }
        }

        [ClientRpc]
        void Rpc_ShowPlayer()
        {
            StartCoroutine(ShowPlayerCamera());

            IEnumerator ShowPlayerCamera()
            {
                yield return new WaitForSeconds(11);

                GameObject instantiatedShowPlayerCamera;
                instantiatedShowPlayerCamera = Instantiate(showPlayerCameraPrefab);
                if (NetworkClient.localPlayer != null)
                {
                    instantiatedShowPlayerCamera.transform.position = NetworkClient.localPlayer.transform.position;
                    instantiatedShowPlayerCamera.transform.rotation = NetworkClient.localPlayer.transform.rotation;
                    instantiatedShowPlayerCamera.GetComponent<ShowPlayerCameraManager>().player = NetworkClient.localPlayer.transform;
                }
            }
        }

        [ClientRpc]
        void RpcSetBandage(NetworkIdentity playerNetId, bool _show)
        {
            playerNetId.GetComponent<CharacterManager>().bandage.SetActive(_show);
        }

        [ClientRpc]
        void RpcKickPlayerFromMatch(NetworkIdentity playerNetId)
        {
            if (NetworkClient.localPlayer.GetComponent<Player>().username == playerNetId.GetComponent<Player>().username)
            {
                GameObject mainMenuManagerObj = GameObject.FindGameObjectWithTag("MainMenuManager");
                if (mainMenuManagerObj != null)
                {
                    MainMenuManager mainMenuManager = mainMenuManagerObj.GetComponent<MainMenuManager>();
                    mainMenuManager.LeaveLobby();
                }
            }
        }

        [ClientRpc]
        void RpcResetSpectatorText()
        {
            spectatorText.text = "";
        }

        [ClientRpc]
        void RpcPlayMatchSound(bool matchStarted, bool matchEnded)
        {
            var tempGO = new GameObject();
            tempGO.transform.position = transform.position;

            var aSource = tempGO.AddComponent<AudioSource>();

            AudioClip _clip = null;
            if (matchStarted)
            {
                _clip = matchStartedSound;
            }
            else if (matchEnded)
            {
                _clip = matchEndedSound;
            }

            if (_clip != null)
            {
                aSource.clip = _clip;
                aSource.volume = 1;
                aSource.minDistance = 1;
                aSource.maxDistance = 500;
                aSource.reverbZoneMix = 1;
                aSource.spatialBlend = 0;

                aSource.Play();

                Destroy(tempGO, _clip.length);
            }
            else
            {
                Destroy(tempGO);
            }
        }

        void SetFirstPersonMode(NetworkIdentity playerNetId, bool status)
        {
            playerNetId.GetComponent<ManageTPController>().isFirstPerson = status;

            RpcSetFirstPersonMode(playerNetId, status);
        }

        [ClientRpc]
        void RpcSetFirstPersonMode(NetworkIdentity playerNetId, bool status)
        {
            playerNetId.GetComponent<ManageTPController>().isFirstPerson = status;
        }

        [ClientRpc]
        void RpcDisableHunterMesh(NetworkIdentity playerNetId)
        {
            HunterAbilities _hunterAbilities = playerNetId.GetComponent<HunterAbilities>();

            foreach (HunterAbilities.Hunter hunter in _hunterAbilities.hunters)
            {
                foreach (GameObject AllHunterMesh in hunter.HunterMesh)
                {
                    AllHunterMesh.SetActive(false);
                }
            }
            foreach (GameObject AllHunterMesh in _hunterAbilities.HunterMeshParents)
            {
                AllHunterMesh.SetActive(false);
            }
        }

        [ClientRpc]
        void RpcUpdateItemSelectedIndicator(NetworkIdentity playerNetId, int _requestedSlot)
        {
            playerNetId.GetComponent<CharacterManager>().CharacterEvent_PlayerChangedItem?.Invoke(_requestedSlot);
        }

        [ClientRpc]
        void RpcShowServerMessage(string serverMessage)
        {
            Instantiate(serverMessagePrefab).GetComponent<ServerMessageManager>().ShowServerMessage(serverMessage);
        }

        /*private void OnCurrentSelectedMatchMapIdChanged(int oldValue, int newValue)
        {
            if (isServer)
            {
                RpcLoadMatchMap(newValue);
            }
        }*/

        public void LoadMatchMapServer(int mapId)
        {
            if(isServer && !isCurrentSelectedMatchMapIdSet)
            {
                isCurrentSelectedMatchMapIdSet = true;
                currentSelectedMatchMapId = mapId;

                foreach (MatchMap matchMap in matchMaps)
                {
                    matchMap.mapGameObject.SetActive(false);

                    if (currentSelectedMatchMapId == matchMap.mapId)
                    {
                        matchMap.mapGameObject.SetActive(true);
                        escapeManager.spectatorCameraTarget.transform.position = matchMap.spectatorCameraTargetPosition;
                        escapeManager.spectatorCameraTarget.transform.rotation = matchMap.spectatorCameraTargetRotation;
                        escapeManager.escapeDestination.transform.position = matchMap.escapeDestinationPosition;
                        escapeManager.escapeDestination.transform.rotation = matchMap.escapeDestinationRotation;
                        mainMenuManager.ItemShopCameraTarget.transform.position = matchMap.itemShopCameraTargetPosition;
                        mainMenuManager.ItemShopCameraTarget.transform.rotation = matchMap.itemShopCameraTargetRotation;
                        mainMenuManager.KillerCustomizationPlayerFollowCameraTarget.transform.position = matchMap.killerCustomizationPlayerFollowCameraTargetPosition;
                        mainMenuManager.KillerCustomizationPlayerFollowCameraTarget.transform.rotation = matchMap.killerCustomizationPlayerFollowCameraTargetRotation;
                        mainMenuManager.KillerSelectionPreviewModelPosition.position = matchMap.killerSelectionPreviewModelPositionPosition;
                        mainMenuManager.KillerSelectionPreviewModelPosition.rotation = matchMap.killerSelectionPreviewModelPositionRotation;
                    }
                }

                for (int i = 0; i < players.Count; i++)
                {
                    Transform nexSpawnpoint = NextSpawnpoint();

                    players[i].transform.position = nexSpawnpoint.position;
                    players[i].transform.rotation = nexSpawnpoint.rotation;
                    players[i].GetComponent<NetworkTransform>().RpcTeleportAndRotate(nexSpawnpoint.position, nexSpawnpoint.rotation);
                }

                RpcLoadMatchMap(mapId);
            }
        }

        [ClientRpc]
        void RpcLoadMatchMap(int mapId)
        {
            LoadMatchMapLocal(mapId);
        }

        public void LoadMatchMapLocal(int mapId)
        {
            foreach (MatchMap matchMap in matchMaps)
            {
                matchMap.mapGameObject.SetActive(false);

                if (mapId == matchMap.mapId)
                {
                    matchMap.mapGameObject.SetActive(true);
                    mainLobbyMenuCamera.position = matchMap.mainLobbyMenuCameraPosition;
                    mainLobbyMenuCamera.rotation = matchMap.mainLobbyMenuCameraRotation;
                    escapeManager.spectatorCameraTarget.transform.position = matchMap.spectatorCameraTargetPosition;
                    escapeManager.spectatorCameraTarget.transform.rotation = matchMap.spectatorCameraTargetRotation;
                    escapeManager.escapeDestination.transform.position = matchMap.escapeDestinationPosition;
                    escapeManager.escapeDestination.transform.rotation = matchMap.escapeDestinationRotation;
                    mainMenuManager.ItemShopCameraTarget.transform.position = matchMap.itemShopCameraTargetPosition;
                    mainMenuManager.ItemShopCameraTarget.transform.rotation = matchMap.itemShopCameraTargetRotation;
                    mainMenuManager.KillerCustomizationPlayerFollowCameraTarget.transform.position = matchMap.killerCustomizationPlayerFollowCameraTargetPosition;
                    mainMenuManager.KillerCustomizationPlayerFollowCameraTarget.transform.rotation = matchMap.killerCustomizationPlayerFollowCameraTargetRotation;
                    mainMenuManager.KillerSelectionPreviewModelPosition.position = matchMap.killerSelectionPreviewModelPositionPosition;
                    mainMenuManager.KillerSelectionPreviewModelPosition.rotation = matchMap.killerSelectionPreviewModelPositionRotation;
                }
            }
        }

        [ClientRpc]
        private void RpcUpdateClimbableObstacles(KnockableObstacle knockableObstacle, GameObject[] obstacles)
        {
            knockableObstacle.associatedClimbableObstacles = new ClimbableObstacle[obstacles.Length];
            for (int i = 0; i < obstacles.Length; i++)
            {
                knockableObstacle.associatedClimbableObstacles[i] = obstacles[i].GetComponent<ClimbableObstacle>();
            }
        }
    }

    [System.Serializable]
    public class MatchObject
    {
        public string name;
        public GameObject objectPrefab;
        public Transform spawnPoint;
        [Tooltip("If setScale is true, the scale of the spawn point will also be applied to the match object.")]
        public bool setScale = false;
        public GameObject instantiatedObject;
        public bool isItemContainer = false;
        public bool isKnockableObstacle = false;
        [Tooltip("If isKnockableObstacle is true, specify the positions of the ClimbableObstacles that needs to be associated with this KnockableObstacle.")]
        public Transform climbableObstacleFrontPosition;
        public Transform climbableObstacleBackPosition;
    }

    [System.Serializable]
    public class MatchMap
    {
        public string name;
        public string description;
        public GameObject showMapNamePrefab;
        public int mapId;
        public GameObject mapGameObject;
        [Tooltip("These objects will be removed and instantiated every time a match is launched.")]
        public MatchObject[] matchObjects;
        public Transform[] survivorSpawnpoints;
        public Transform[] survivorMatchSpawnpoints;
        public Transform hunterSpawnpoint;
        public Transform hunterMatchEndedSpawnpoint;
        public Transform revivedPlayerSpawnpoint;
        public Vector3 policeCarPosition = new Vector3(19.09f, 0f, 35.16f);
        public Quaternion policeCarRotation = new Quaternion(0f, 229.7999f, 0f, 0f);
        public Vector3 mainLobbyMenuCameraPosition = new Vector3(-22.04f, 7.4f, -13.227f);
        public Quaternion mainLobbyMenuCameraRotation = new Quaternion(4.808903f, 180f, 0f, 0f);
        [Header("Escape System")]
        public Vector3 spectatorCameraTargetPosition = new Vector3(0f, 2.4f, 8.24f);
        public Quaternion spectatorCameraTargetRotation = new Quaternion(0f, 180f, 0f, 0f);
        public Vector3 escapeDestinationPosition = new Vector3(0f, 0f, -17.76f);
        public Quaternion escapeDestinationRotation = new Quaternion(0f, 0f, 0f, 0f);
        [Header("Killer Selection")]
        public Vector3 killerCustomizationPlayerFollowCameraTargetPosition = new Vector3(31.65373f, 0.7f, -11.19842f);
        public Quaternion killerCustomizationPlayerFollowCameraTargetRotation = new Quaternion(0f, 246.1361f, 0f, 0f);
        public Vector3 killerSelectionPreviewModelPositionPosition = new Vector3(28.14f, 0f, -11.9f);
        public Quaternion killerSelectionPreviewModelPositionRotation = new Quaternion(0f, 67.11614f, 0f, 0f);
        [Header("Item Shop")]
        public Vector3 itemShopCameraTargetPosition = new Vector3(-19.105f, 6.7f, -16.532f);
        public Quaternion itemShopCameraTargetRotation = new Quaternion(24.10001f, 39.39999f, 0f, 0f);
    }
}