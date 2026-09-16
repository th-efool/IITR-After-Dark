// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Threading.Tasks;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CharacterManager : NetworkBehaviour
    {
        Animator anim;
        public RuntimeAnimatorController defaultAnimController;
        public RuntimeAnimatorController survivorAnimController;
        public bool useSheriffRuntimeAnimatorController = false;
        public RuntimeAnimatorController sheriffAnimController;

        public KeyCode useButton;
        public KeyCode dropItemButton;

        public KeyCode emptyHand = KeyCode.Alpha1;
        public KeyCode item1 = KeyCode.Alpha2;
        public KeyCode item2 = KeyCode.Alpha3;
        public KeyCode item3 = KeyCode.Alpha4;
        public List<GameplayItem> items = new List<GameplayItem>();
        public GameplayItem itemCurrentlyInUse;
        public int currentSlot = 0;
        [SyncVar] public bool usesWeapon = false;

        public delegate void EquipmentStateChanged();
        public EquipmentStateChanged CharacterEvent_EquipmentStateChanged;

        public delegate void PlayerChangedItem(int _slotID);
        public PlayerChangedItem CharacterEvent_PlayerChangedItem;

        public delegate void TeamSet(byte team);
        public TeamSet CharacterEvent_TeamSet;

        //parent for transforms that items will stick to, apropriate transform for item will 
        //be founded by this script by matching it name and item name
        public Transform itemParent;

        public Transform menuLookAtPoint;

        [Header("Character Health")]
        public int maxHealth = 100;
        public int health = 100;
        public byte Team = 0;
        public bool canRegenerate = false;
        public GameObject bandage;
        [SyncVar] public bool isHealing = false;

        public delegate void Death();
        public Death CharacterEvent_Death;

        public delegate void Resurrection();
        public Death CharacterEvent_Resurrection;
        public bool movementPermission { get; private set; } = true;



        [HideInInspector] public bool alive = true;

        [SyncVar] public bool isSheriff = false;
        public GameObject showingSheriffPrefab;
        public GameObject sheriffWeapon;

        [Space]
        public GameObject miniMap;

        [Space]
        [SyncVar] public bool isReady = false;
        public GameObject readyCheckMark;
        public GameObject notReadyCheckMark;

        [Space]
        [Header("Temporary Match Statistics")]
        [SyncVar] public int TempEscaped;
        [SyncVar] public int TempKilledPlayers;
        [SyncVar] public int TempCapturedPlayers;
        [SyncVar] public int TempAbilitiesUsed;
        [SyncVar] public int TempHealedHealth;
        [SyncVar] public int TempDamageDealt;
        [SyncVar] public int TempCompletedTasks;
        [SyncVar] public int TempTimeSurvived;
        [SyncVar] public int TempHelpedPlayers;
        [SyncVar] public int TempInstrumentsUsed;
        public GameObject MatchResultUIPrefab;
        public GameObject MatchEndCameraPrefab;

        [SyncVar] public bool escaped;

        public float staminaConsumption = 10f;
        public float staminaRegeneration = 10f;
        public float repairSpeedMultiplier = 1f;
        public float consumableHealthMultiplier = 1f;

        List<GameplayItem> previousItems = new List<GameplayItem>();
        GameplayItem currentInteractionItem = null;

        private bool canChangeSlot = true;

        [Header("Audio")]
        public AudioClip[] eventSounds;

        private RedicionStudio.PlayerInputs _input;

        private void Awake()
        {
            //CustomNetworkManager.Instance.NetworkManagerEvent_OnPlayerConnected += UpdateForLatePlayer; // removed, new players cannot join during the match

            anim = GetComponent<Animator>();

            items.Clear();
            for (int i = 0; i < 4; i++)
            {
                items.Add(null);
            }
        }
        void Start()
        {
            RoomManager._instance.RegisterPlayerInGame(gameObject);

            GameManager.SpawnPlayer(gameObject, hasAuthority);

            CmdShareMatchMapId(OfflineMainMenuManager._instance.currentSelectedMapId);

            StartCoroutine(PlayEventSound());
        }
        void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (hasAuthority && movementPermission && alive) //flag "has authority" is true for objects that are yours, for example our player controller
            {
                if (canChangeSlot && !GetComponent<HunterAbilities>()._inFight && !GetComponent<PlayerInteractionModule>().isClimbing && !GetComponent<PlayerInteractionModule>().isUsingRadio && !_input.use && !GetComponent<PlayerInteraction>().inVehicle && !GetComponent<HunterAbilities>().isBlocked && !GetComponent<CharacterManager>().isHealing && !usesWeapon)
                {
                    if (_input.gamepadConnected)
                        HandleGamepadSlotSwitching();

                    HandleKeyboardSlotSwitching();

                    if (_input.dropItem)
                    {
                        Client_DropItem(currentSlot);
                        StartCoroutine(DelaySlotChange());
                    }
                }

                if (_input.interact)
                    Server_PickUpItem(currentSlot, currentInteractionItem);

                // Find the main camera object
                GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");

                if (mainCameraObject == null)
                {
                    return;
                }

                Camera mainCamera = mainCameraObject.GetComponent<Camera>();

                if (mainCamera == null)
                {
                    return;
                }

                // Create a ray from the camera's position
                Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);

                int layerMask = 1 << 10; // Only Layer 10 for interactable items

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 2f, layerMask))
                {
                    HandleItemHit(hit);
                }
                else
                {
                    if (Physics.Raycast(ray, out hit, 4.2f, layerMask))
                    {
                        HandleItemHit(hit);
                    }
                    else
                    {
                        // Deactivate any interaction indicators if nothing is hit
                        DeactivatePreviousItemInteractionIndicators();
                    }
                }

            }

            if (!GetComponent<HunterAbilities>()._inFight)
            {
                if (health < maxHealth && anim != null)
                {
                    if (!GetComponent<HunterAbilities>()._isHunter)
                        anim.SetBool("Injured", true);
                }
                else if (health == maxHealth && anim != null)
                {
                    anim.SetBool("Injured", false);
                }
            }

            if (RoomManager._instance != null && !RoomManager._instance.MatchRunning)
            {
                if (isReady)
                {
                    if (notReadyCheckMark != null)
                        notReadyCheckMark.SetActive(false);
                    if (readyCheckMark != null)
                        readyCheckMark.SetActive(true);
                }
                else
                {
                    if (readyCheckMark != null)
                        readyCheckMark.SetActive(false);
                    if (notReadyCheckMark != null)
                        notReadyCheckMark.SetActive(true);
                }
            }
            else if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
            {
                if (readyCheckMark != null)
                    readyCheckMark.SetActive(false);
                if (notReadyCheckMark != null)
                    notReadyCheckMark.SetActive(false);
            }

            if (itemCurrentlyInUse != null)
            {
                if (itemCurrentlyInUse.useAnimationLayer4WhenAiming && itemCurrentlyInUse.isAiming)
                {
                    bool isItemMeshEnabled = true;
                    foreach (MeshRenderer meshRenderer in itemCurrentlyInUse._itemMesh)
                    {
                        if (!meshRenderer.enabled)
                        {
                            isItemMeshEnabled = false;
                            break;
                        }
                    }

                    bool inFight = GetComponent<HunterAbilities>()._inFight;
                    float layerWeight = isItemMeshEnabled && !inFight ? 1 : 0;
                    GetComponent<Animator>().SetLayerWeight(4, layerWeight);
                }
                else
                {
                    GetComponent<Animator>().SetLayerWeight(4, 0);
                }
            }
            else
            {
                GetComponent<Animator>().SetLayerWeight(4, 0);
            }
        }

        private void HandleGamepadSlotSwitching()
        {
            if (_input.nextSlot)
            {
                int nextSlot = (currentSlot + 1) % items.Count;
                ClientTakeItem(nextSlot);
                StartCoroutine(DelaySlotChange());
            }
            if (_input.previousSlot)
            {
                int previousSlot = (currentSlot - 1 + items.Count) % items.Count;
                ClientTakeItem(previousSlot);
                StartCoroutine(DelaySlotChange());
            }
        }

        private void HandleKeyboardSlotSwitching()
        {
            if (_input.inventorySlot0)
            {
                ClientTakeItem(0);
                StartCoroutine(DelaySlotChange());
            }
            if (_input.inventorySlot1)
            {
                ClientTakeItem(1);
                StartCoroutine(DelaySlotChange());
            }
            if (_input.inventorySlot2)
            {
                ClientTakeItem(2);
                StartCoroutine(DelaySlotChange());
            }
            if (_input.inventorySlot3)
            {
                ClientTakeItem(3);
                StartCoroutine(DelaySlotChange());
            }
        }

        void HandleItemHit(RaycastHit hit)
        {
            GameplayItem newHitItem = hit.collider.GetComponent<GameplayItem>();

            if (newHitItem && !newHitItem.IsOwned)
            {
                if (currentInteractionItem != newHitItem)
                {
                    DeactivatePreviousItemInteractionIndicators();

                    currentInteractionItem = newHitItem;
                    if (!GetComponent<HunterAbilities>()._isHunter)
                        currentInteractionItem.interactionIndicator.SetActive(true);
                    else if (GetComponent<HunterAbilities>()._isHunter && currentInteractionItem.canUsedByHunter)
                        currentInteractionItem.interactionIndicator.SetActive(true);
                }
            }
            else
            {
                DeactivatePreviousItemInteractionIndicators();
            }
        }

        private IEnumerator DelaySlotChange()
        {
            canChangeSlot = false;
            yield return new WaitForSeconds(1f);
            canChangeSlot = true;
        }

        void DeactivatePreviousItemInteractionIndicators()
        {
            foreach (GameplayItem previousItem in previousItems)
            {
                if (previousItem != null)
                {
                    previousItem.interactionIndicator.SetActive(false);
                }
            }

            previousItems.Clear();

            if (currentInteractionItem != null)
            {
                previousItems.Add(currentInteractionItem);
                currentInteractionItem = null;
            }
        }

        void ClientTakeItem(int _requestedSlot)
        {
            if (!movementPermission) return; //if player is blocked do not let him change items

            TakeItem(_requestedSlot);
            Server_TakeItem(_requestedSlot);
        }
        //this method lets us retake slot, even if we already are using it, useful for taking "empty hands" after droping item
        void TakeItem(int _requestedSlot)
        {
            if (currentSlot == _requestedSlot && itemCurrentlyInUse == items[currentSlot]) return; //do not retake item that is currently in use

            //put down old item if it was not empty hands
            //   if (itemCurrentlyInUse)
            //       itemCurrentlyInUse.Putdown();
            if (items[currentSlot])
                items[currentSlot].Putdown();


            itemCurrentlyInUse = null;

            currentSlot = _requestedSlot;
            itemCurrentlyInUse = items[currentSlot];

            if (itemCurrentlyInUse)
            {
                itemCurrentlyInUse.Activate();

                if (itemCurrentlyInUse.useOverrideController)
                {
                    //set proper animator for item if avaible
                    if (itemCurrentlyInUse.AnimatorController)
                        anim.runtimeAnimatorController = itemCurrentlyInUse.AnimatorController;
                    else
                        anim.runtimeAnimatorController = defaultAnimController;
                }
            }
            else //if empty handed set default animator controller
            {
                anim.runtimeAnimatorController = defaultAnimController;
                anim.SetTrigger("Empty");
            }

            CharacterEvent_PlayerChangedItem?.Invoke(_requestedSlot);

        }
        [Command]
        void Server_TakeItem(int _requestedSlot)
        {
            Rpc_TakeItem(_requestedSlot);
        }
        [ClientRpc]
        void Rpc_TakeItem(int _requestedSlot)
        {
            if (hasAuthority) return;

            TakeItem(_requestedSlot);
        }

        [Command]
        void Server_PickUpItem(int slotToInsetItem, GameplayItem itemToInset)
        {
            /*GameplayItem nearestItem = null;
            float lastDistance = 999f;

            Collider[] potentialItems = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, 1f, int.MaxValue);
            foreach (Collider col in potentialItems)
            {
                GameplayItem item = col.GetComponent<GameplayItem>();
                if (item && !item.IsOwned)
                {
                    float distance = Vector3.Distance(transform.position, item.transform.position);
                    if (distance < lastDistance) 
                    {
                        nearestItem = item;
                        lastDistance = distance;
                    }
                }
            }

            if(nearestItem)
                Server_AssignItem(nearestItem, slotToInsetItem);*/

            if (itemToInset && !itemToInset.IsOwned)
            {
                Server_AssignItem(itemToInset, slotToInsetItem);
            }
        }
        void Server_AssignItem(GameplayItem _itemToAssign, int _slotToAssign)
        {
            _itemToAssign.netIdentity.RemoveClientAuthority();
            _itemToAssign.netIdentity.AssignClientAuthority(this.connectionToClient);
            AssignItem(_itemToAssign, _slotToAssign);

            RPC_AssignItem(_itemToAssign.netIdentity, _slotToAssign);
        }
        [ClientRpc]
        void RPC_AssignItem(NetworkIdentity _itemNetID, int _slotToAssign)
        {
            if (isServer) return;
            AssignItem(_itemNetID.GetComponent<GameplayItem>(), _slotToAssign);
        }

        void AssignItem(GameplayItem _itemToAssign, int _slotToAssign)
        {
            bool isHunter = GetComponent<HunterAbilities>()._isHunter;
            bool canBeUsedByHunter = _itemToAssign.canUsedByHunter;

            if ((isHunter && canBeUsedByHunter) || !isHunter)
            {
                if (_slotToAssign == 0) //is selected empty handed slot, check another slots if they are empty, if yes, place item in them
                {
                    for (int i = 1; i < items.Count; i++)
                    {
                        if (items[i] == null)
                        {
                            _slotToAssign = i;
                            break;
                        }
                    }
                }

                if (_slotToAssign == 0) return; //if empty slot was not found, dont do anything

                //if there is already item in slot that we want to place new item in, than drop the old one
                DropItem(_slotToAssign);

                items[_slotToAssign] = _itemToAssign;
                items[_slotToAssign].Equip(this, FindCorrectItemParentFor(_itemToAssign));

                TakeItem(_slotToAssign);

                CharacterEvent_EquipmentStateChanged?.Invoke();
            }
        }

        #region drop items
        [Command] //CLient call to drop item
        void Client_DropItem(int _slotToDrop)
        {
            Server_DropItem(_slotToDrop);
        }
        void Server_DropItem(int _slotToDrop)
        {
            if (!items[_slotToDrop]) return; //if slot is empty than do nothing

            items[_slotToDrop].netIdentity.RemoveClientAuthority();
            DropItem(_slotToDrop);
            Rpc_DropItem(_slotToDrop);
        }
        void Server_DropAndDestroyItem(int _slotToDrop)
        {
            GameplayItem itemToDestroy = items[_slotToDrop];
            if (itemToDestroy == null) return; //if slot is empty than do nothing

            items[_slotToDrop].netIdentity.RemoveClientAuthority();
            DropItem(_slotToDrop);
            Rpc_DropAndDestroyItem(_slotToDrop);

            //we can't destroy item right after we detach if from player, because of synchronization problems,
            //so we have to wait for all network messages to finish themselves
            itemToDestroy.transform.position = new Vector3(0, -999f, 0);

            StartCoroutine(destroyItem());
            IEnumerator destroyItem()
            {
                yield return new WaitForSeconds(0.5f);
                NetworkServer.Destroy(itemToDestroy.gameObject);
            }
        }
        [ClientRpc]
        void Rpc_DropItem(int _slotToDrop)
        {
            if (isServer) return;
            DropItem(_slotToDrop);
        }
        [ClientRpc]
        void Rpc_DropAndDestroyItem(int _slotToDrop)
        {
            if (isServer) return;

            //we can't destroy item right after we detach if from player, because of synchronization problems,
            //so we have to wait for all network messages to finish themselves
            GameplayItem itemToDestroy = items[_slotToDrop];


            DropItem(_slotToDrop);

            if (itemToDestroy)
                itemToDestroy.transform.position = new Vector3(0, -999f, 0);
        }
        void DropItem(int _slotToDrop)
        {
            GameplayItem itemToDrop = items[_slotToDrop];

            if (itemToDrop)
            {
                if (itemToDrop.InUse)
                    itemToDrop.Putdown();

                itemToDrop.Drop();
                itemToDrop.transform.SetPositionAndRotation(transform.position + transform.up * 1f + transform.forward * 0.5f, transform.rotation);

                items[_slotToDrop] = null;

                CharacterEvent_EquipmentStateChanged?.Invoke();

                TakeItem(_slotToDrop);
            }
        }
        [ClientRpc]
        void Rpc_DetachItem(int _slotToDrop)
        {
            if (items[_slotToDrop])
            {
                items[_slotToDrop] = null;

                CharacterEvent_EquipmentStateChanged?.Invoke();

                TakeItem(_slotToDrop);
            }
        }

        public void Give(GameObject _item, int _slotID)
        {
            if (!isServer) return;
            GameObject item = Instantiate(_item);
            NetworkServer.Spawn(item);
            Server_AssignItem(item.GetComponent<GameplayItem>(), _slotID);
        }
        public void ClearInvetory()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    Server_DropAndDestroyItem(i);
                }
            }
        }

        //for now its used only for consumables, if we eat something and want it to vanish, than we want to have it previously be detached from player before destroying it
        public void Server_DropCurrentItem()
        {
            Server_DropItem(currentSlot);
        }
        public void Server_DetachCurrentItem()
        {
            Rpc_DetachItem(currentSlot);
        }
        #endregion
        #region update character for new players
        //if we join the server where players have already certain items in inventory, than we want to see them with these items, and that what this block is about
        void UpdateForLatePlayer(NetworkConnection conn)
        {
            List<NetworkIdentity> itemIdentities = new List<NetworkIdentity>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i])
                {
                    itemIdentities.Add(items[i].netIdentity);
                }
                else
                {
                    itemIdentities.Add(null);
                }
            }
            TargetRpcUpdateForLatePlayer(conn, itemIdentities, currentSlot);
        }
        [TargetRpc]
        void TargetRpcUpdateForLatePlayer(NetworkConnection conn, List<NetworkIdentity> itemsID, int currentlyUsedSlot)
        {

            //filling slots with items
            for (int i = 0; i < itemsID.Count; i++)
            {
                if (itemsID[i])
                {
                    AssignItem(itemsID[i].gameObject.GetComponent<GameplayItem>(), i);
                }
            }
            TakeItem(currentlyUsedSlot);
        }
        #endregion

        //method responsible for finding apropriate transform in hand for item to stick to, to make them fit well in hand
        Transform FindCorrectItemParentFor(GameplayItem _item)
        {
            foreach (Transform trans in itemParent.GetComponentsInChildren<Transform>(true))
            {
                if (trans.name == _item.ItemName)
                    return trans.transform;
            }
            return itemParent;
        }


        public void PlayAnimationBool(string _boolName, bool _active)
        {
            anim.SetBool(_boolName, _active);
        }
        public void PlayAnimationTrigger(string _triggerName)
        {
            anim.SetTrigger(_triggerName);
        }


        #region Health

        public void Server_TakeDamage(int damage)
        {
            if (escaped)
                return;

            health -= damage;

            health = Mathf.Clamp(health, 0, maxHealth); //do not let helth go out of bounds

            Rpc_TakeDamage(health);

            if (health <= 0)
            {
                TempTimeSurvived = RoomManager._instance.playTime;
                RoomManager._instance.NewDeadPlayer(this);
            }
        }
        [ClientRpc]
        void Rpc_TakeDamage(int currentHealth)
        {
            health = currentHealth;
            GetComponent<Animator>().SetTrigger(GetRandomHitTrigger());

            //I moved this code here, so we dont have to check every frame if player is dead, now we check this only when player health state is changed
            if (health <= 0 && alive)
            {
                HealthDepleted();
                CharacterEvent_Death?.Invoke();
                alive = false;
                GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<Animator>().enabled = false;
            }
            UpdatePlayerListUI(GetComponent<NetworkIdentity>());
        }

        public void Server_TakeHealth(int _health)
        {
            health += _health;
            if (health > maxHealth)
                health = maxHealth;

            Rpc_TakeHealth(health);
        }
        [ClientRpc]
        void Rpc_TakeHealth(int currentHealth)
        {
            health = currentHealth;
            UpdatePlayerListUI(GetComponent<NetworkIdentity>());
        }

        void HealthDepleted()
        {
            //Drop all of equipment when dead
            for (int i = 0; i < items.Count; i++)
            {
                DropItem(i);
            }
        }

        [ClientRpc]
        public void Rpc_Ressurect()
        {
            alive = true;
            health = maxHealth;
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<Animator>().enabled = true;
            CharacterEvent_Resurrection?.Invoke();

            if (hasAuthority) //after respawn return to spectate our player controller
                GameManager.GameEvent_SpectatePlayer(gameObject);
            UpdatePlayerListUI(GetComponent<NetworkIdentity>());
        }

        [ClientRpc]
        public void Rpc_SetMovementPermission(bool _perm)
        {
            //GetComponent<NetworkTransform>().enabled = _perm; //prevents the player from moving across the server.
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(!_perm, false);

            movementPermission = _perm;

            /*   if (_perm)
                   GetComponent<Animator>().Play("Locomotion"); //it makes sure that player after release will be playing movement animation,
               //usefull for hunter spiecial attacks, if animation of it will be placed in the same animator controller
            */

        }

        [ClientRpc]
        public void Rpc_AllowOnlyPlayerMovementInput(bool _perm)
        {
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(!_perm, false);

            /*   if (_perm)
                   GetComponent<Animator>().Play("Locomotion"); //it makes sure that player after release will be playing movement animation,
               //usefull for hunter spiecial attacks, if animation of it will be placed in the same animator controller
            */

        }

        [ClientRpc]
        public void Rpc_SetSheriff()
        {
            isSheriff = true;
            if (useSheriffRuntimeAnimatorController)
            {
                GetComponent<Animator>().runtimeAnimatorController = sheriffAnimController;
                defaultAnimController = sheriffAnimController;
            }
            GetComponent<OutfitManager>().SetSheriffOutfit(GetComponent<NetworkIdentity>());
            ShowSheriff();
        }

        void ShowSheriff()
        {
            if (isLocalPlayer)
            {
                foreach (MatchMap matchMap in RoomManager._instance.matchMaps)
                {
                    if (RoomManager._instance.currentSelectedMatchMapId == matchMap.mapId)
                    {
                        Instantiate(showingSheriffPrefab, matchMap.revivedPlayerSpawnpoint.position, matchMap.revivedPlayerSpawnpoint.rotation).GetComponent<ShowSheriffCameraManager>().sheriff = transform;
                    }
                }
            }
        }

        #region CancelAttack
        public void CancelAttack()
        {
            ClientCancelAttack();
        }

        [Command]
        void ClientCancelAttack()
        {
            GetComponent<Animator>().SetTrigger("CancelAttack");

            RpcCancelAttack();
        }

        [ClientRpc]
        void RpcCancelAttack()
        {
            GetComponent<Animator>().SetTrigger("CancelAttack");
        }
        #endregion

        string GetRandomHitTrigger()
        {
            string[] triggers = new string[] { "Hit01", "Hit02" };
            int randomTrigger = Random.Range(0, triggers.Length);
            return triggers[randomTrigger];
        }
        #endregion


        public void SetTeam(byte team)
        {
            Team = team;

            HunterAbilities hunterAbilities = GetComponent<HunterAbilities>();

            CharacterEvent_TeamSet?.Invoke(team);

            if (team == 1)
                hunterAbilities.SetHunter();
            else
                hunterAbilities.SetSurvivor();

        }

        public void SetCharacterRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void LerpCharacterPosition(Vector3 destination, float lerpSpeed = 3f)
        {
            if (hasAuthority)
            {
                LerpCharacterPositionToDestination(destination, lerpSpeed);
            }
        }

        public async void LerpCharacterPositionToDestination(Vector3 destination, float lerpSpeed = 3f)
        {
            float timeNeedToLerp = (Vector3.Distance(destination, transform.position)) / lerpSpeed;
            float endTime = Time.time + timeNeedToLerp;
            Vector3 startPos = transform.position;

            if (!RoomManager._instance.MatchRunning)
            {
                timeNeedToLerp = 0f;
                endTime = 0f;
                startPos = Vector3.zero;
            }
            else while (endTime >= Time.time)
                {
                    if (!RoomManager._instance.MatchRunning)
                    {
                        timeNeedToLerp = 0f;
                        endTime = 0f;
                        startPos = Vector3.zero;
                    }
                    else
                    {
                        float lerpProgress = (1f - (endTime - Time.time) / timeNeedToLerp);
                        transform.position = Vector3.Lerp(startPos, destination, lerpProgress);
                        await Task.Yield();
                    }

                    //print("Lerping " + lerpProgress * 100 + "%");
                }

            transform.position = destination;
            timeNeedToLerp = 0f;
            endTime = 0f;
            startPos = Vector3.zero;
        }

        public void ShowMatchStatistics(float _survivedTime, float _damageDealt, float _completedTasks, float _helpedPlayers, float _instrumentsSuccessfullyUsed, float _playersKilled, bool isHunter, bool survived)
        {
            if (isLocalPlayer)
            {
                GameObject _matchEndCameraPrefab;
                _matchEndCameraPrefab = Instantiate(MatchEndCameraPrefab);
                GameObject _matchResultUIPrefab;
                _matchResultUIPrefab = Instantiate(MatchResultUIPrefab);
                _matchResultUIPrefab.GetComponent<MatchResultManager>().isHunter = isHunter;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().survived = survived;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().survivedTime = _survivedTime;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().survivedTimeXP = _survivedTime / 5 * GetComponent<ExperienceManager>().currentLevel / 5;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().damageDealt = _damageDealt;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().damageDealtXP = _damageDealt / 5 * GetComponent<ExperienceManager>().currentLevel / 5;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().completedTasks = _completedTasks;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().completedTasksXP = _completedTasks * GetComponent<ExperienceManager>().currentLevel / 2 * 7;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().helpedPlayers = _helpedPlayers;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().helpedPlayersXP = _helpedPlayers * GetComponent<ExperienceManager>().currentLevel / 2 * 7;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().instrumentsSuccessfullyUsed = _instrumentsSuccessfullyUsed;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().instrumentsSuccessfullyUsedXP = _instrumentsSuccessfullyUsed * GetComponent<ExperienceManager>().currentLevel / 2 * 7;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().playersKilled = _playersKilled;
                _matchResultUIPrefab.GetComponent<MatchResultManager>().playersKilledXP = _playersKilled * GetComponent<ExperienceManager>().currentLevel / 2 * 7;
            }
        }

        public void ToggleReadiness(bool forceNotReady)
        {
            if (forceNotReady)
            {
                CmdSetReadiness(false);
            }
            else
            {
                if (!isReady)
                {
                    CmdSetReadiness(true);
                }
                else
                {
                    CmdSetReadiness(false);
                }
            }
        }

        [Command]
        public void CmdSetReadiness(bool _isReady)
        {
            isReady = _isReady;

            RpcSetReadiness(_isReady);
        }

        [ClientRpc]
        public void RpcSetReadiness(bool _isReady)
        {
            isReady = _isReady;
        }

        public void SetChaseMusic()
        {
            //GetComponent<HunterAbilities>().chaseMusicAudioSource.clip = chaseMusic;
        }

        IEnumerator PlayEventSound()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(130, 300));

                if (eventSounds.Length > 0)
                {
                    AudioClip randomEventSound = eventSounds[Random.Range(0, eventSounds.Length)];
                    if (RoomManager._instance.MatchRunning && !RoomManager._instance.MatchEnding && !GetComponent<HunterAbilities>()._inFight && !GetComponent<HunterAbilities>().chaseMusicAudioSource.isPlaying && !GetComponent<HunterAbilities>().survivorHeartbeatAudioSource.isPlaying)
                        PlayClipAt(randomEventSound, transform.position, 1, 1, 500, 0, 1);
                }
            }
        }

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance, float spatialBlend, float reverbZoneMix)
        {
            if (!isServer)
            {
                var tempGO = new GameObject();
                tempGO.transform.position = _position;
                var aSource = tempGO.AddComponent<AudioSource>();
                aSource.clip = _clip;
                aSource.volume = _volume;
                aSource.minDistance = _minDistance;
                aSource.maxDistance = _maxDistance;
                aSource.reverbZoneMix = reverbZoneMix;
                aSource.spatialBlend = spatialBlend;
                aSource.Play();
                Destroy(tempGO, _clip.length);
            }
        }

        [Command]
        void CmdUpdatePlayerListUI(NetworkIdentity playerNetId)
        {
            RpcUpdatePlayerListUI(playerNetId);
        }

        [ClientRpc]
        void RpcUpdatePlayerListUI(NetworkIdentity playerNetId)
        {
            UpdatePlayerListUI(playerNetId);
        }

        [Command]
        void CmdShareMatchMapId(int mapId)
        {
            RoomManager._instance.LoadMatchMapServer(mapId);
        }

        void UpdatePlayerListUI(NetworkIdentity playerNetId)
        {
            UIPlayerListItem uiPlayerListItem = FindUIPlayerListItemForPlayer(playerNetId.GetComponent<Player>().username);
            if (uiPlayerListItem != null)
            {
                uiPlayerListItem.UpdateHealth(health);
            }
        }

        UIPlayerListItem FindUIPlayerListItemForPlayer(string username)
        {
            foreach (GameObject entry in RoomManager._instance.playerListItems)
            {
                UIPlayerListItem uiPlayerListItem = entry.GetComponent<UIPlayerListItem>();
                if (uiPlayerListItem.playerNameText.text == username)
                {
                    return uiPlayerListItem;
                }
            }
            return null;
        }

        private void OnDestroy()
        {
            if (RoomManager._instance)
                RoomManager._instance.DeRegisterPlayerFromGame(gameObject);

            //CustomNetworkManager.Instance.NetworkManagerEvent_OnPlayerConnected -= UpdateForLatePlayer; // removed, new players cannot join during the match
        }
    }
}