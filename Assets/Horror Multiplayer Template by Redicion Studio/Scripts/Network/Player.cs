// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using System;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.UI;

namespace RedicionStudio.InventorySystem
{
    public class Player : NetworkBehaviour
    {

        [Header("Player Modules")]
        public RedicionStudio.InventorySystem.PlayerInventoryModule playerInventory;
        public PlayerNutritionModule playerNutrition;

        ChallengesManager challengesManager;

        [Space]
        [SyncVar] public int id;
        [SyncVar] public string username;
        [SyncVar] public byte status;
        [SyncVar] public int funds;
        [SyncVar] public int experiencePoints;
        [SyncVar] public int killerId;
        [SyncVar] public int outfitId;
        [SyncVar] public int escaped;
        [SyncVar] public int killedPlayers;
        [SyncVar] public int capturedPlayers;
        [SyncVar] public int abilitiesUsed;
        [SyncVar] public int healedHealth;
        [SyncVar] public int damageDealt;
        [SyncVar] public int completedTasks;
        [SyncVar] public int timeSurvived;
        [SyncVar] public int helpedPlayers;
        [SyncVar] public int instrumentsUsed;
        [HideInInspector] public Instance instance;
        [HideInInspector] public PropertyArea propertyArea;

        public static Dictionary<int, Player> onlinePlayers = new Dictionary<int, Player>();

        public static Player localPlayer;

        public List<GameObject> placedObjects = new List<GameObject>();

        [Space]
        [Header("Player Profile UI")]
        public GameObject playerProfileUI;
        public TMPro.TMP_Text usernameText;
        public Image playerImage;

        public static event Action<Player, string> OnMessage;

        [Command]
        private void CmdPlace(string placeableSOUniqueName, Vector3 position, Quaternion rotation)
        {
            BSystem.PlaceableSO placeableSO = BSystem.PlaceableSO.GetPlaceableSO(placeableSOUniqueName);
            if (placeableSO == null)
            {
                return;
            }
            if (funds >= placeableSO.price)
            {
                if (propertyArea == null || !propertyArea.Contains(new Bounds(position, Vector3.one * .1f)))
                {
                    return;
                }
                // -
                GameObject gO = BSystem.PlaceableObject.Place(id, placeableSOUniqueName, position, rotation); // ?
                placedObjects.Add(gO);
                funds -= placeableSO.price;
            }
        }

        [Command]
        public void CmdEdit(uint id, Vector3 newPosition, Quaternion newRotation)
        { // TODO: refactor
            if (NetworkServer.spawned.TryGetValue(id, out NetworkIdentity identity))
            {
                if (propertyArea == null || !identity.TryGetComponent(out BSystem.PlaceableObject placeableObject) || placeableObject.ownerId != this.id ||
                    !propertyArea.Contains(new Bounds(newPosition, Vector3.one * .1f)))
                {
                    return;
                }
                identity.transform.position = newPosition;
                identity.transform.rotation = newRotation;
                RpcEditUpdate(id, newPosition, newRotation);
            }
        }

        [ClientRpc]
        public void RpcEditUpdate(uint id, Vector3 newPosition, Quaternion newRotation)
        {
            if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity identity))
            {
                identity.transform.position = newPosition;
                identity.transform.rotation = newRotation;
            }
        }

        [Command]
        public void CmdEditDelete(uint id)
        {
            if (NetworkServer.spawned.TryGetValue(id, out NetworkIdentity identity))
            {
                if (propertyArea == null || !identity.TryGetComponent(out BSystem.PlaceableObject placeableObject) || placeableObject.ownerId != this.id)
                {
                    return;
                }

                placedObjects.Remove(placeableObject.gameObject);
                NetworkServer.Destroy(identity.gameObject);
                funds += placeableObject.placeableSO.sellPrice;
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer(); // ?

            localPlayer = this;

            MasterServer.MSClient.State = MasterServer.MSClient.NetworkState.InGame;
            TPCameraController.LockCursor(true);
            //_camera = FindObjectOfType<Camera>().transform;
            _camera = GameObject.Find("MainCamera").transform;
            usernameText.text = username;

            BSystem.BSystem.OnPlaceRequestAction = () => {
                CmdPlace(BSystem.BSystem.currentPlaceableSO.uniqueName, BSystem.BSystem.position, BSystem.BSystem.rotation);
            };
        }

        private void Start()
        {

            if (isLocalPlayer)
            {
                StartCoroutine(InitializeChallengesManager());
            }

            if (GetComponent<PlayerAI>().isSetAsAi == true)
            {
                GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().enabled = false;
                return;
            }
            onlinePlayers[id] = this;

            if (!isLocalPlayer)
            {
                Destroy(GetComponent<TPCharacterController>());
            }
            else
            {
                Destroy(_nameplateCanvas.gameObject);
                if (username != "")
                {
                    GetComponent<PlayerAI>().enabled = false;
                    GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
                }
            }

            if (!isLocalPlayer && !isServerOnly)
            {
                _nameplateText.text = username + (status == 100 ? "\n<color=#6ab04c><developer></color>" : string.Empty);
            }

#if UNITY_SERVER// || UNITY_EDITOR // (Server)
		int propertyAreaId = PropertyArea.Assign(instance.uniqueName, id);
		TargetAreaId(propertyAreaId);
		propertyArea = PropertyArea.GetPropertyArea(instance.uniqueName, id); // ?????
#endif

#if UNITY_SERVER || UNITY_EDITOR
            if (isServer)
            {
                MasterServer.MSClient.GetPlacedObjects(id, (placedObjectsData) => {
                    for (int i = 0; i < placedObjectsData.Length; i++)
                    {
                        Vector3 position = propertyArea.transform.position + new Vector3(
                            placedObjectsData[i].x,
                            placedObjectsData[i].y,
                            placedObjectsData[i].z);
                        GameObject gO = BSystem.PlaceableObject.Place(id, placedObjectsData[i].placeableSOUniqueName, position,
                            new Quaternion(placedObjectsData[i].rotX,
                            placedObjectsData[i].rotY,
                            placedObjectsData[i].rotZ,
                            placedObjectsData[i].rotW)); // ?
                        placedObjects.Add(gO);
                    }
                });
            }
#endif
        }

        [TargetRpc]
        private void TargetAreaId(int id)
        {
            Debug.Log(id);
            PropertyArea.myIndex = id;
        }

        private void OnDestroy()
        {
            _ = onlinePlayers.Remove(id);

            if (localPlayer == this)
            {
                localPlayer = null;
            }

            if (isServer)
            {
                instance.RemovePlayer(id);
            }

            if (isLocalPlayer)
            {
                TPCameraController.LockCursor(false);
                PropertyArea.myIndex = -1;
            }

#if UNITY_SERVER || UNITY_EDITOR
            if (isServer)
            {
                if (placedObjects.Count > 0)
                {
                    MasterServer.MServer.PlacedObjectJSONData[] placedObjectsData = new MasterServer.MServer.PlacedObjectJSONData[placedObjects.Count];
                    for (int i = 0; i < placedObjects.Count; i++)
                    {
                        Vector3 position = placedObjects[i].transform.position - propertyArea.transform.position;
                        placedObjectsData[i] = new MasterServer.MServer.PlacedObjectJSONData
                        {
                            placeableSOUniqueName = placedObjects[i].GetComponent<BSystem.PlaceableObject>().placeableSOUniqueName,
                            x = position.x,
                            y = position.y,
                            z = position.z,
                            rotX = placedObjects[i].transform.rotation.x,
                            rotY = placedObjects[i].transform.rotation.y,
                            rotZ = placedObjects[i].transform.rotation.z,
                            rotW = placedObjects[i].transform.rotation.w,
                        };
                        NetworkServer.Destroy(placedObjects[i].gameObject);
                    }
                    MasterServer.MSClient.SavePlacedObjects(id, placedObjectsData);
                }
            }

            if (isServer)
            {
                MasterServer.MSManager.SendPacket(new MasterServer.AccountDataResponsePacket { Id = id, Funds = funds, OwnsProperty = true, Nutrition = playerNutrition.value, ExperiencePoints = experiencePoints, KillerId = killerId, OutfitId = outfitId, Escaped = escaped, KilledPlayers = killedPlayers, CapturedPlayers = capturedPlayers, AbilitiesUsed = abilitiesUsed, HealedHealth = healedHealth, DamageDealt = damageDealt, CompletedTasks = completedTasks, TimeSurvived = timeSurvived, HelpedPlayers = helpedPlayers, InstrumentsUsed = instrumentsUsed });

                MasterServer.MServer.InventoryJSONData[] inventoryJSONData = new MasterServer.MServer.InventoryJSONData[playerInventory.slots.Count];
                for (int i = 0; i < playerInventory.slots.Count; i++)
                {
                    inventoryJSONData[i] = new MasterServer.MServer.InventoryJSONData
                    {
                        hash = playerInventory.slots[i].item.hash,
                        amount = playerInventory.slots[i].amount,
                        shelfLife = playerInventory.slots[i].item.currentShelfLifeInSeconds
                    };
                }
                MasterServer.MSClient.SaveInventory(id, inventoryJSONData);
            }
#endif
            // Refactor

#if UNITY_SERVER// || UNITY_EDITOR // (Server)
		propertyArea?.AssignTo(0);
		propertyArea = null;
#endif
        }

        [SerializeField] private Transform _nameplateCanvas;
        [SerializeField] private TextMeshProUGUI _nameplateText;
        private static Transform _camera;

        private void Update()
        {
            if (challengesManager == null && GameObject.FindGameObjectWithTag("ChallengesManager").GetComponent<ChallengesManager>() != null)
            {
                challengesManager = GameObject.FindGameObjectWithTag("ChallengesManager").GetComponent<ChallengesManager>();
            }
            if (localPlayer != null && !isLocalPlayer)
            {
                if (this.GetComponent<CharacterManager>().alive == true)
                {
                    this.GetComponent<Health>()._HealthCanvas.gameObject.SetActive(true);
                    _nameplateCanvas.gameObject.SetActive(true);
                    _nameplateCanvas.LookAt(_nameplateCanvas.position + _camera.rotation * Vector3.forward,
                        _camera.rotation * Vector3.up);
                }
                else
                {
                    this.GetComponent<Health>()._HealthCanvas.gameObject.SetActive(false);
                    _nameplateCanvas.gameObject.SetActive(false);
                }
                this.GetComponent<StarterAssets.ThirdPersonController>().staminaSlider.gameObject.SetActive(false);
            }
            else if (localPlayer != null && isLocalPlayer)
            {
                this.GetComponent<ExperienceManager>().ExperienceUI.SetActive(true);
                this.GetComponent<Player>().playerProfileUI.SetActive(true);
            }
        }

        public void SetPlayerPosition(Vector3 position, Quaternion rotation)
        {
            CmdSetPlayerPosition(position, rotation);
        }

        [Command]
        public void CmdSetPlayerPosition(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;

            RpcSetPlayerPosition(GetComponent<NetworkIdentity>(), position, rotation);
        }

        [ClientRpc]
        public void RpcSetPlayerPosition(NetworkIdentity playerNetId, Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }

        [Command]
        public void CmdSend(string message)
        {
            if (message.Trim() != "")
                RpcReceive(message.Trim());
        }

        [ClientRpc]
        public void RpcReceive(string message)
        {
            OnMessage?.Invoke(this, message);
        }

        public void SetExperience(int _xp)
        {
            if (isServer)
            {
                experiencePoints += _xp;
            }
        }

        public void SetKillerID(int id)
        {
            CmdSetKillerID(id);
        }

        [Command]
        private void CmdSetKillerID(int id)
        {
            killerId = id;
            RpcSetKillerID(id);
        }

        [ClientRpc]
        private void RpcSetKillerID(int id)
        {
            killerId = id;
        }

        public void SetOutfitID(int id)
        {
            CmdSetOutfitID(id);
        }

        [Command]
        private void CmdSetOutfitID(int id)
        {
            outfitId = id;
            RpcSetOutfitID(id);
        }

        [ClientRpc]
        private void RpcSetOutfitID(int id)
        {
            outfitId = id;
        }

        public void SetDamageDealtValue(int value)
        {
            if (localPlayer)
                OnDamageDealtValueChanged(value);
        }

        public void SetKilledPlayersValue(int value)
        {
            if (localPlayer)
                OnKilledPlayersValueChanged(value);
        }

        public void SetEscapedValue(int value)
        {
            if (localPlayer)
                OnEscapedValueChanged(value);
        }

        public void SetTimeSurvivedValue(int value)
        {
            if (localPlayer)
                OnTimeSurvivedValueChanged(value);
        }

        public void SetHealedHealthValue(int value)
        {
            if (localPlayer)
                OnHealedHealthValueChanged(value);
        }

        public void SetInstrumentsUsedValue(int value)
        {
            if (localPlayer)
                OnInstrumentsUsedValueChanged(value);
        }

        public void SetCapturedPlayersValue(int value)
        {
            if (localPlayer)
                OnCapturedPlayersValueChanged(value);
        }

        public void SetAbilitiesUsedValue(int value)
        {
            if (localPlayer)
                OnAbilitiesUsedValueChanged(value);
        }

        IEnumerator InitializeChallengesManager()
        {
            while (challengesManager == null)
            {
                yield return null;
                challengesManager = GameObject.FindGameObjectWithTag("ChallengesManager")?.GetComponent<ChallengesManager>();
            }

            // If ChallengesManager is found, update the challenge values
            OnEscapedValueChanged(escaped);
            OnKilledPlayersValueChanged(killedPlayers);
            OnCapturedPlayersValueChanged(capturedPlayers);
            OnAbilitiesUsedValueChanged(abilitiesUsed);
            OnHealedHealthValueChanged(healedHealth);
            OnDamageDealtValueChanged(damageDealt);
            OnCompletedTasksValueChanged(completedTasks);
            OnTimeSurvivedValueChanged(timeSurvived);
            OnHelpedPlayersValueChanged(helpedPlayers);
            OnInstrumentsUsedValueChanged(instrumentsUsed);
        }

        void OnEscapedValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("Escaped", newValue);
        }

        void OnKilledPlayersValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("KilledPlayers", newValue);
        }

        void OnCapturedPlayersValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("CapturedPlayers", newValue);
        }

        void OnAbilitiesUsedValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("AbilitiesUsed", newValue);
        }

        void OnHealedHealthValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("HealedHealth", newValue);
        }

        void OnDamageDealtValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("DamageDealt", newValue);
        }

        void OnCompletedTasksValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("CompletedTasks", newValue);
        }

        void OnTimeSurvivedValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("TimeSurvived", newValue);
        }

        void OnHelpedPlayersValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("HelpedPlayers", newValue);
        }

        void OnInstrumentsUsedValueChanged(int newValue)
        {
            challengesManager.UpdateChallengeProgress("InstrumentsUsed", newValue);
        }
    }
}