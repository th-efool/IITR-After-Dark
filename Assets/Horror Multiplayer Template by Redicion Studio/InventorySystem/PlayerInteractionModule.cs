// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using RedicionStudio.NetworkUtils;
using System.Collections;

namespace RedicionStudio.InventorySystem
{
    public class PlayerInteractionModule : NetworkBehaviour
    {

        [Header("Player Modules")]
        public PlayerInventoryModule playerInventory;

        [HideInInspector] public INetInteractable<PlayerInventoryModule> currentInteractable;

        [SerializeField] private float _maxDistance;

        [Space]
        [SerializeField] private GameObject UIMessagePrefab;
        GameObject instantiatedUIMessage;

        private static Transform _camera;

        [SyncVar] public NetworkIdentity currentItemContainer;

        [SyncVar] public NetworkIdentity currentClimbableObstacle;
        [SyncVar] public bool isClimbing = false;
        public bool canClimb = true;

        [SyncVar] public NetworkIdentity currentKnockableObstacle;
        [SyncVar] public bool isKnocking = false;
        public bool canKnock = true;

        [SyncVar] public NetworkIdentity currentOpenableDoor;

        [SyncVar] public NetworkIdentity currentRadio;
        [SyncVar] public bool isUsingRadio = false;
        public GameObject radioUIPrefab;
        private GameObject instantiatedRadioUIPrefab;

        public UnityEngine.UI.Slider unlockDoorSlider;
        public GameObject UnlockDoorUI;

        public UnityEngine.UI.Slider repairSlider;
        public GameObject repairUI;
        public TMPro.TMP_Text repairText;

        public UnityEngine.UI.Slider healingSlider;
        public GameObject healingUI;
        public TMPro.TMP_Text healingText;

        private RedicionStudio.PlayerInputs _input;

        private bool lastToggleInteractInput = false;
        private bool lastToggleUseInput = false;
        private bool lastNextSlotInput = false;
        private bool lastPreviousSlotInput = false;
        private bool lastToggleAimInput = false;
        private bool lastToggleDropItemInput = false;
        private bool lastToggleClimbInput = false;

        private void Start()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            //_camera = FindObjectOfType<Camera>().transform;
            _camera = GameObject.Find("MainCamera").transform;
            UIInteraction.playerInteraction = this;
        }

        private void OnDestroy()
        {
            if (isLocalPlayer)
            {
                UIInteraction.playerInteraction = null;
            }
        }

        private void Raycast(Vector3 position, Vector3 forward)
        {
            if (Physics.Raycast(position, forward, out RaycastHit hitInfo, _maxDistance, 1 << LayerMask.NameToLayer("Ground")) && hitInfo.transform.TryGetComponent(out INetInteractable<PlayerInventoryModule> interactable))
            {
                currentInteractable = interactable;
            }
            else
            {
                currentInteractable = null;
            }
        }

        private static Keyboard _keyboard;
        private static Mouse _mouse;

        [Command]
        public void CmdInteract(Vector3 position, Vector3 forward)
        {
            Raycast(position, forward);
            if (currentInteractable != null)
            {
                currentInteractable.OnServerInteract(playerInventory);
            }
        }

        public void AddItem(PlayerInventoryModule player, int itemPrice, Item item, int amount)
        {
            /*if (instantiatedUIMessage != null)
                Destroy(instantiatedUIMessage);*/

            //instantiatedUIMessage = Instantiate(UIMessagePrefab);

            if (player.GetComponent<Player>().funds < itemPrice)
            {
                //instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Not enough funds");

                return;
            }

            //instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Item: " + item.itemSO.uniqueName + " " + amount + "x"  + " purchased");
            CmdAddItem(player, itemPrice, item, amount);
        }

        [Command]
        public void CmdAddItem(PlayerInventoryModule player, int itemPrice, Item item, int amount)
        {
            if (player.GetComponent<Player>().funds < itemPrice)
            {
                //Not enough funds
            }
            else if (player.GetComponent<Player>().funds == itemPrice || player.GetComponent<Player>().funds > itemPrice)
            {
                player.GetComponent<Player>().funds -= itemPrice;
                player.Add(item, amount);
            }
        }

        public void AddMoney(PlayerInventoryModule player, int amount)
        {
            if (isServer)
            {
                player.GetComponent<Player>().funds += amount;
                RpcAddMoney(player, amount);
            }
            else if (hasAuthority)
            {
                /*if (instantiatedUIMessage != null)
                    Destroy(instantiatedUIMessage);*/

                //instantiatedUIMessage = Instantiate(UIMessagePrefab);

                //instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Amount: " + "$" + amount + " added");
                CmdAddMoney(player, amount);
            }
        }

        [Command]
        public void CmdAddMoney(PlayerInventoryModule player, int amount)
        {
            player.GetComponent<Player>().funds += amount;
        }

        [ClientRpc]
        public void RpcAddMoney(PlayerInventoryModule player, int amount)
        {
            player.GetComponent<Player>().funds += amount;
        }

        public void RemoveMoney(PlayerInventoryModule player, int amount)
        {
            /*if (instantiatedUIMessage != null)
                Destroy(instantiatedUIMessage);

            instantiatedUIMessage = Instantiate(UIMessagePrefab);*/

            if (player.GetComponent<Player>().funds < amount)
            {
                //instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Not enough funds");

                return;
            }

            //instantiatedUIMessage.GetComponent<UIMessage>().ShowMessage("Amount: " + "$" + amount + " removed");
            CmdRemoveMoney(player, amount);
        }

        [Command]
        public void CmdRemoveMoney(PlayerInventoryModule player, int amount)
        {
            if (player.GetComponent<Player>().funds < amount)
            {
                // Amount cannot be removed because the player does not have sufficient funds.
            }
            else if (player.GetComponent<Player>().funds == amount || player.GetComponent<Player>().funds > amount)
            {
                player.GetComponent<Player>().funds -= amount;
            }
        }

        public void UpdateRepairUI(bool repairs, float value, string playerName, string text)
        {
            if (!isServer)
            {
                if (NetworkClient.localPlayer.gameObject.GetComponent<Player>().username == playerName)
                {
                    repairText.text = "Repairs " + text + " ...";
                    repairUI.SetActive(repairs);
                }
                repairSlider.value = value / 100;
            }
        }

        public void UpdateHealingUI(bool healing, float value, string playerName, float maxValue, bool anotherPlayerIsBeingHealed, string anotherHealedPlayerName)
        {
            if (!isServer)
            {
                if (NetworkClient.localPlayer.gameObject.GetComponent<Player>().username == playerName)
                {
                    if (!anotherPlayerIsBeingHealed)
                        healingText.text = "Healing " + " ...";
                    else
                        healingText.text = "You are healing " + anotherHealedPlayerName + " ...";
                    healingUI.SetActive(healing);
                }
                else if (anotherPlayerIsBeingHealed && NetworkClient.localPlayer.gameObject.GetComponent<Player>().username == anotherHealedPlayerName)
                {
                    healingText.text = "You are being healed by " + playerName + " ...";
                    healingUI.SetActive(healing);
                }
                healingSlider.value = (maxValue - value) / maxValue;
            }
        }

        [Command]
        void CmdOpenItemContainer()
        {
            if (currentItemContainer != null)
            {
                currentItemContainer.GetComponent<ItemContainerManager>().SpawnItems();
            }
        }

        [Command]
        void CmdClimbOverObstacle(NetworkIdentity obstacleNetId)
        {
            if (obstacleNetId != null)
            {
                isClimbing = true;
                GetComponent<Transform>().position = obstacleNetId.GetComponent<ClimbableObstacle>().startPosition.position;
                GetComponent<Transform>().rotation = obstacleNetId.GetComponent<ClimbableObstacle>().startPosition.rotation;
                GetComponent<Animator>().SetTrigger(obstacleNetId.GetComponent<ClimbableObstacle>().ClimbOverObstacleTriggerName);
                //StartCoroutine(CoroutineClimbOverObstacle(obstacleNetId.GetComponent<NetworkIdentity>()));
                RpcClimbOverObstacle(obstacleNetId);
            }
        }

        [ClientRpc]
        void RpcClimbOverObstacle(NetworkIdentity obstacleNetId)
        {
            if (obstacleNetId != null)
            {
                isClimbing = true;
                GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, false);
                GetComponent<Transform>().position = obstacleNetId.GetComponent<ClimbableObstacle>().startPosition.position;
                GetComponent<Transform>().rotation = obstacleNetId.GetComponent<ClimbableObstacle>().startPosition.rotation;
                GetComponent<Animator>().SetTrigger(obstacleNetId.GetComponent<ClimbableObstacle>().ClimbOverObstacleTriggerName);
                if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    foreach (MeshRenderer meshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                        meshRenderer.enabled = false;
                }
                StartCoroutine(CoroutineClimbOverObstacle(obstacleNetId.GetComponent<NetworkIdentity>()));
            }
        }

        IEnumerator CoroutineClimbOverObstacle(NetworkIdentity obstacleNetId)
        {
            ClimbableObstacle obstacle = obstacleNetId.GetComponent<ClimbableObstacle>();
            Transform playerTransform = GetComponent<Transform>();

            yield return new WaitForSeconds(0.2f);

            Vector3 startPosition = playerTransform.position;
            Quaternion startRotation = playerTransform.rotation;
            Vector3 targetPosition = obstacle.targetPosition.position;
            Quaternion targetRotation = obstacle.targetPosition.rotation;

            float duration = 2.0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                playerTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
                playerTransform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

                yield return null;
            }

            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer meshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    meshRenderer.enabled = true;
                if (!GetComponent<ManageTPController>().isFirstPerson)
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
            isClimbing = false;
            StartCoroutine(CoroutineAllowClimbing());
        }

        IEnumerator CoroutineAllowClimbing()
        {
            yield return new WaitForSeconds(1f);

            canClimb = true;
        }

        [Command]
        void CmdKnockObstacle(NetworkIdentity obstacleNetId)
        {
            if (obstacleNetId != null)
            {
                KnockableObstacle obstacle = obstacleNetId.GetComponent<KnockableObstacle>();
                Transform playerTransform = GetComponent<Transform>();

                float distanceToFront = Vector3.Distance(playerTransform.position, obstacle.startPositionFront.position);
                float distanceToBack = Vector3.Distance(playerTransform.position, obstacle.startPositionBack.position);
                isKnocking = true;
                if (distanceToFront < distanceToBack)
                {
                    playerTransform.position = obstacle.startPositionFront.position;
                    playerTransform.rotation = obstacle.startPositionFront.rotation;
                    GetComponent<Animator>().SetTrigger(obstacle.KnockableObstacleKnockingFrontTriggerName);
                }
                else
                {
                    playerTransform.position = obstacle.startPositionBack.position;
                    playerTransform.rotation = obstacle.startPositionBack.rotation;
                    GetComponent<Animator>().SetTrigger(obstacle.KnockableObstacleKnockingBackTriggerName);
                }
                RpcKnockObstacle(obstacleNetId);
                obstacleNetId.GetComponent<KnockableObstacle>().isKnocking = true;
                StartCoroutine(CoroutineIsKnocking(obstacleNetId));
            }
        }

        IEnumerator CoroutineIsKnocking(NetworkIdentity obstacleNetId)
        {
            yield return new WaitForSeconds(1f);

            obstacleNetId.GetComponent<KnockableObstacle>().isKnocking = false;
            obstacleNetId.GetComponent<KnockableObstacle>().isKnocked = true;
            obstacleNetId.GetComponent<KnockableObstacle>().SetKnocked();
        }

        [ClientRpc]
        void RpcKnockObstacle(NetworkIdentity obstacleNetId)
        {
            if (obstacleNetId != null)
            {
                KnockableObstacle obstacle = obstacleNetId.GetComponent<KnockableObstacle>();
                Transform playerTransform = GetComponent<Transform>();

                obstacle.isKnocking = true;

                float distanceToFront = Vector3.Distance(playerTransform.position, obstacle.startPositionFront.position);
                float distanceToBack = Vector3.Distance(playerTransform.position, obstacle.startPositionBack.position);
                isKnocking = true;
                if (distanceToFront < distanceToBack)
                {
                    playerTransform.position = obstacle.startPositionFront.position;
                    playerTransform.rotation = obstacle.startPositionFront.rotation;
                    GetComponent<Animator>().SetTrigger(obstacle.KnockableObstacleKnockingFrontTriggerName);
                }
                else
                {
                    playerTransform.position = obstacle.startPositionBack.position;
                    playerTransform.rotation = obstacle.startPositionBack.rotation;
                    GetComponent<Animator>().SetTrigger(obstacle.KnockableObstacleKnockingBackTriggerName);
                }

                GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, false);

                if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    foreach (MeshRenderer meshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                        meshRenderer.enabled = false;
                }

                obstacle.animator.SetTrigger("KnockObstacle");

                StartCoroutine(CoroutineKnockObstacle(obstacleNetId));
            }
        }

        IEnumerator CoroutineKnockObstacle(NetworkIdentity obstacleNetId)
        {
            yield return new WaitForSeconds(1f);

            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);

            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer meshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    meshRenderer.enabled = true;

                if (!GetComponent<ManageTPController>().isFirstPerson)
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
            obstacleNetId.GetComponent<KnockableObstacle>().isKnocking = false;
            obstacleNetId.GetComponent<KnockableObstacle>().isKnocked = true;
            obstacleNetId.GetComponent<KnockableObstacle>().SetKnocked();
            isKnocking = false;
            StartCoroutine(CoroutineAllowKnocking());
        }

        IEnumerator CoroutineAllowKnocking()
        {
            yield return new WaitForSeconds(1f);
            canKnock = true;
        }

        IEnumerator CoroutineUnlockOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            float unlockProgress = 0f;

            while (_input.interact && GetComponent<CharacterManager>().itemCurrentlyInUse != null && GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName.Equals("lockpick"))
            {
                unlockProgress += Time.deltaTime / openableDoorNetId.GetComponent<OpenableDoor>().unlockingDuration;
                unlockProgress = Mathf.Clamp01(unlockProgress);

                UnlockDoorUI.SetActive(true);
                unlockDoorSlider.value = unlockProgress;

                if (unlockProgress >= 0.99f)
                {
                    CmdUnlockOpenableDoor(openableDoorNetId);
                    UnlockDoorUI.SetActive(false);
                    unlockDoorSlider.value = 0;
                    yield break;
                }

                yield return null;
            }

            if (!_input.interact || GetComponent<CharacterManager>().itemCurrentlyInUse == null || !GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName.Equals("lockpick"))
            {
                CmdUnlockOpenableDoorEnd(openableDoorNetId);
                UnlockDoorUI.SetActive(false);
                unlockDoorSlider.value = 0;
                yield break;
            }
        }

        [Command]
        void CmdUnlockOpenableDoorEnd(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger("UnlockingDoorEnd");
            openableDoorNetId.GetComponent<OpenableDoor>().RpcEndBeingUnlocked();
            openableDoorNetId.GetComponent<OpenableDoor>().RpcStopUnlockingDoorLoopAudio();

            RpcUnlockOpenableDoorEnd(openableDoorNetId);
        }

        [ClientRpc]
        void RpcUnlockOpenableDoorEnd(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger("UnlockingDoorEnd");
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (!GetComponent<ManageTPController>().isFirstPerson)
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
        }

        [Command]
        void CmdUnlockOpenableDoorStart(NetworkIdentity openableDoorNetId)
        {
            transform.position = openableDoorNetId.GetComponent<OpenableDoor>().unlockPlayerPosition.position;
            transform.rotation = openableDoorNetId.GetComponent<OpenableDoor>().unlockPlayerPosition.rotation;
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerUnlockingDoorTriggerName);
            openableDoorNetId.GetComponent<OpenableDoor>().RpcSetBeingUnlocked();
            openableDoorNetId.GetComponent<OpenableDoor>().RpcPlayUnlockingDoorLoopAudio();

            RpcUnlockOpenableDoorStart(openableDoorNetId);
        }

        [ClientRpc]
        void RpcUnlockOpenableDoorStart(NetworkIdentity openableDoorNetId)
        {
            transform.position = openableDoorNetId.GetComponent<OpenableDoor>().unlockPlayerPosition.position;
            transform.rotation = openableDoorNetId.GetComponent<OpenableDoor>().unlockPlayerPosition.rotation;
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerUnlockingDoorTriggerName);
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, false);
        }

        [Command]
        void CmdUnlockOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger("UnlockingDoorEnd");
            openableDoorNetId.GetComponent<OpenableDoor>().RpcEndBeingUnlocked();
            openableDoorNetId.GetComponent<OpenableDoor>().RpcUnlockDoor();

            RpcUnlockOpenableDoor(openableDoorNetId);
        }

        [ClientRpc]
        void RpcUnlockOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger("UnlockingDoorEnd");
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (!GetComponent<ManageTPController>().isFirstPerson)
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
        }

        [Command]
        void CmdOpenOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerOpenDoorTriggerName);
            openableDoorNetId.GetComponent<OpenableDoor>().RpcOpenDoor();

            RpcOpenOpenableDoor(openableDoorNetId);
        }

        [ClientRpc]
        void RpcOpenOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer itemMeshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = false;
            }
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerOpenDoorTriggerName);
            StartCoroutine(EndUseOpenableDoor());
        }

        [Command]
        void CmdCloseOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerOpenDoorTriggerName);
            openableDoorNetId.GetComponent<OpenableDoor>().RpcCloseDoor();

            RpcCloseOpenableDoor(openableDoorNetId);
        }

        [ClientRpc]
        void RpcCloseOpenableDoor(NetworkIdentity openableDoorNetId)
        {
            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer itemMeshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = false;
            }
            GetComponent<Animator>().SetTrigger(openableDoorNetId.GetComponent<OpenableDoor>().PlayerOpenDoorTriggerName);
            StartCoroutine(EndUseOpenableDoor());
        }

        IEnumerator EndUseOpenableDoor()
        {
            yield return new WaitForSeconds(1);

            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null && !GetComponent<HunterAbilities>()._inFight)
            {
                foreach (MeshRenderer itemMeshRenderer in GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = true;
                if (!GetComponent<ManageTPController>().isFirstPerson)
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    GetComponent<Animator>().SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
        }

        public void OpenEscapeDoor(NetworkIdentity sender, NetworkIdentity generator, NetworkIdentity lever)
        {
            CmdOpenEscapeDoor(sender, generator, lever);
        }

        [Command]
        void CmdOpenEscapeDoor(NetworkIdentity sender, NetworkIdentity generator, NetworkIdentity lever)
        {
            if (lever != null && generator != null && generator.GetComponent<GeneratorManager>().health == 100)
            {
                Player player = sender.GetComponent<Player>();
                if (player != null)
                {
                    CharacterManager characterManager = player.GetComponent<CharacterManager>();
                    if (characterManager != null)
                    {
                        lever.GetComponent<LeverManager>().serverUsed = true;
                        characterManager.PlayAnimationTrigger("Throw");
                        lever.GetComponent<LeverManager>().escapeDoorAnimator.SetTrigger("OpenDoor");
                        RpcOpenEscapeDoor(sender, generator, lever);
                    }
                }
            }
        }

        [ClientRpc]
        void RpcOpenEscapeDoor(NetworkIdentity sender, NetworkIdentity generator, NetworkIdentity lever)
        {
            if (lever != null && generator != null && generator.GetComponent<GeneratorManager>().health == 100)
            {
                Player player = sender.GetComponent<Player>();
                if (player != null)
                {
                    CharacterManager characterManager = player.GetComponent<CharacterManager>();
                    if (characterManager != null)
                    {
                        lever.GetComponent<LeverManager>().serverUsed = true;
                        characterManager.PlayAnimationTrigger("Throw");
                        lever.GetComponent<LeverManager>().escapeDoorAnimator.SetTrigger("OpenDoor");
                        StartCoroutine(OpenEscapeDoorCoroutine(sender));
                    }
                }
            }
        }

        private IEnumerator OpenEscapeDoorCoroutine(NetworkIdentity playerNetId)
        {
            yield return new WaitForSeconds(1f);

            if (playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (!playerNetId.GetComponent<ManageTPController>().isFirstPerson)
                    playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
        }

        [Command]
        void CmdRadioInteraction(bool status)
        {
            isUsingRadio = status;
            if (currentRadio != null)
                currentRadio.GetComponent<RadioManager>().isUsed = status;

            RpcRadioInteraction(status);
        }

        [ClientRpc]
        void RpcRadioInteraction(bool status)
        {
            isUsingRadio = status;
            if (currentRadio != null)
                currentRadio.GetComponent<RadioManager>().isUsed = status;
            GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(status, status);
        }

        [Command]
        void CmdChangeRadioStation(int value)
        {
            if (currentRadio != null)
            {
                currentRadio.GetComponent<RadioManager>().ChangeStation(value);
            }
        }

        [Command]
        void CmdToggleRadioPower()
        {
            if (currentRadio != null)
            {
                currentRadio.GetComponent<RadioManager>().ToggleRadio();
            }
        }

        private static Vector3 _position;
        private static Vector3 _forward;

        private void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (!isLocalPlayer)
            {
                return;
            }

            bool currentInteractToggleInput = _input.interact;
            bool currentUseToggleInput = _input.use;
            bool currentNextSlotInput = _input.nextSlot;
            bool currentPreviousSlotInput = _input.previousSlot;
            bool currentAimToggleInput = _input.aim;
            bool currentDropItemToggleInput = _input.dropItem;
            bool currentClimbToggleInput = _input.climb;

            _position = _camera.position;
            _forward = _camera.forward;

            Raycast(_position, _forward);

            if (currentUseToggleInput && !lastToggleUseInput)
            {
                if (currentItemContainer != null)
                {
                    if (!GetComponent<HunterAbilities>()._isHunter)
                        CmdOpenItemContainer();
                }
            }

            if (currentClimbToggleInput && !lastToggleClimbInput)
            {
                if (currentClimbableObstacle != null)
                {
                    if (!GetComponent<HunterAbilities>()._isHunter && !isClimbing && canClimb && !GetComponent<CharacterManager>().isHealing)
                    {
                        if (currentClimbableObstacle.GetComponent<ClimbableObstacle>().canBeClimbedOver)
                        {
                            canClimb = false;
                            CmdClimbOverObstacle(currentClimbableObstacle.GetComponent<NetworkIdentity>());
                        }
                    }
                }
            }

            if (currentInteractToggleInput && !lastToggleInteractInput)
            {
                if (currentKnockableObstacle != null)
                {
                    if (!GetComponent<HunterAbilities>()._isHunter && !isKnocking && canKnock && !currentKnockableObstacle.GetComponent<KnockableObstacle>().isKnocking && !currentKnockableObstacle.GetComponent<KnockableObstacle>().isKnocked && !GetComponent<CharacterManager>().isHealing)
                    {
                        canKnock = false;
                        CmdKnockObstacle(currentKnockableObstacle.GetComponent<NetworkIdentity>());
                    }
                }
            }

            if (currentInteractToggleInput && !lastToggleInteractInput)
            {
                if (currentOpenableDoor != null && !GetComponent<HunterAbilities>()._isHunter && !GetComponent<CharacterManager>().isHealing)
                {
                    if (currentOpenableDoor.GetComponent<OpenableDoor>().isDoorLocked)
                    {
                        //Door is locked, unlock it if the player has a lockpick in his inventory
                        if (!currentOpenableDoor.GetComponent<OpenableDoor>().isBeingUnlocked && GetComponent<CharacterManager>().itemCurrentlyInUse != null && GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName.Equals("lockpick"))
                        {
                            //Door is not being unlocked, unlock it
                            CmdUnlockOpenableDoorStart(currentOpenableDoor);
                            StartCoroutine(CoroutineUnlockOpenableDoor(currentOpenableDoor));
                        }
                    }
                    else
                    {
                        if (currentOpenableDoor.GetComponent<OpenableDoor>().isDoorOpen)
                        {
                            //Door is open, close it
                            CmdCloseOpenableDoor(currentOpenableDoor);
                        }
                        else
                        {
                            //Door is closed, open it
                            CmdOpenOpenableDoor(currentOpenableDoor);
                        }
                    }
                }

                if (currentRadio != null && !GetComponent<CharacterManager>().isHealing)
                {
                    if (!currentRadio.GetComponent<RadioManager>().isUsed && !currentRadio.GetComponent<RadioManager>().isRadioDisabled)
                    {
                        instantiatedRadioUIPrefab = Instantiate(radioUIPrefab);
                        CmdRadioInteraction(true);
                        currentRadio.GetComponent<RadioManager>().radioCamera.SetActive(true);
                    }
                }
            }

            if (currentRadio != null && isUsingRadio)
            {
                if (currentUseToggleInput && !lastToggleUseInput)
                {
                    CmdChangeRadioStation(1);
                }
                if (_input.gamepadConnected)
                {
                    if (currentNextSlotInput && !lastNextSlotInput)
                    {
                        CmdChangeRadioStation(-1);
                    }
                    if (currentPreviousSlotInput && !lastPreviousSlotInput)
                    {
                        CmdChangeRadioStation(1);
                    }
                }
                else
                {
                    if (currentAimToggleInput && !lastToggleAimInput)
                    {
                        CmdChangeRadioStation(-1);
                    }
                    if (currentUseToggleInput && !lastToggleUseInput)
                    {
                        CmdChangeRadioStation(1);
                    }
                }
                if (currentDropItemToggleInput && !lastToggleDropItemInput)
                {
                    CmdToggleRadioPower();
                }
                if (currentInteractToggleInput && !lastToggleInteractInput || !GetComponent<CharacterManager>().alive || RoomManager._instance.MatchEnding || !RoomManager._instance.MatchRunning || GetComponent<HunterAbilities>()._inFight)
                {
                    if (instantiatedRadioUIPrefab != null)
                        Destroy(instantiatedRadioUIPrefab);
                    currentRadio.GetComponent<RadioManager>().radioCamera.SetActive(false);
                    CmdRadioInteraction(false);
                }
            }

            lastToggleInteractInput = currentInteractToggleInput;
            lastToggleUseInput = currentUseToggleInput;
            lastNextSlotInput = currentNextSlotInput;
            lastPreviousSlotInput = currentPreviousSlotInput;
            lastToggleAimInput = currentAimToggleInput;
            lastToggleDropItemInput = currentDropItemToggleInput;
            lastToggleClimbInput = currentClimbToggleInput;

            if (currentInteractable == null || _keyboard == null)
            {
                return;
            }

            if (_keyboard.fKey.wasPressedThisFrame)
            {
                currentInteractable.OnClientInteract(playerInventory);
                CmdInteract(_position, _forward);
            }
        }
    }
}