// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine.Animations.Rigging;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class ManageTPController : NetworkBehaviour
    {
        public Transform CurrentWeaponBulletSpawnPoint;
        public Transform CurrentCartridgeEjectSpawnPoint;
        [Header("Player")]
        public Rig PlayerRig;
        public Transform SecondHandRig_target;
        public ThirdPersonController thirdPersonController;
        private RedicionStudio.PlayerInputs _input;
        public Animator PlayerAnimator;
        private bool newMatch = false;
        [Header("Camera")]
        public GameObject IdleCamera;
        public GameObject WeaponIdleCamera;
        public GameObject WeaponAimCamera;
        public GameObject FirstPersonIdleCamera;
        public Transform target;
        public Vector3 thirdPersonTargetPosition;
        public Quaternion thirdPersonTargetRotation;
        public Transform thirdPersonTargetParent;
        public Vector3 firstPersonTargetPosition;
        public Quaternion firstPersonTargetRotation;
        public Transform firstPersonTargetParent;
        [Header("Camera Modes")]
        [SyncVar] public bool isFirstPerson = false;
        bool isSwitchingPerspective = false;
        [Header("Head Mesh")]
        private List<GameObject> headMeshes = new List<GameObject>();
        private int currentHunterId = 0;
        private int currentSurvivorId = 0;
        [Header("Loading Screen")]
        public GameObject loadingScreenPrefab;
        [Header("Car Theft")]
        public GameObject carTheftCamera;

        [SyncVar] public int aimValue;

        public Transform aimTarget;

        [SerializeField] private CharacterManager _characterManager;

        Coroutine c_HideMesh;

        private Animator animator;

        public GameplayItemList itemList;

        private Dictionary<string, AnimationClip> originalClips = new Dictionary<string, AnimationClip>();
        private List<AnimatorOverrideController> overrideControllers = new List<AnimatorOverrideController>();

        private bool lastToggleCameraInput = false;


        void Start()
        {
            if (isLocalPlayer)
            {
                animator = GetComponent<Animator>();

                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();
                CharacterController cc = GetComponent<CharacterController>();
                cc.enabled = true;
                ThirdPersonController tpc = GetComponent<ThirdPersonController>();
                tpc.enabled = true;
                StickCameraToPlayer();

                WeaponIdleCamera = GameObject.Find("PlayerFollowCameraWeapon");
                if (WeaponIdleCamera != null)
                    WeaponIdleCamera.GetComponent<CinemachineCamera>().Follow = target;
                WeaponAimCamera = GameObject.Find("PlayerFollowCameraWeaponAim");
                if (WeaponAimCamera != null)
                    WeaponAimCamera.GetComponent<CinemachineCamera>().Follow = target;
                IdleCamera = GameObject.Find("PlayerFollowCamera");
                if (IdleCamera != null)
                    IdleCamera.GetComponent<CinemachineCamera>().Follow = target;
                if (WeaponIdleCamera != null)
                    WeaponIdleCamera.SetActive(false);
                if (WeaponAimCamera != null)
                    WeaponAimCamera.SetActive(false);
                gameObject.AddComponent<AudioListener>();
            }
        }

        public void StickCameraToPlayer()
        {
            Camera vehicleCam = GetVehicleCamera();
            vehicleCam.enabled = false;

            CinemachineCamera playerCam = GetPlayerCamera();
            playerCam.enabled = true;
            playerCam.Follow = target;
        }

        public void StickCameraToVehicle(Transform followTransform)
        {
            if (!isFirstPerson)
            {
                Camera vehicleCam = GetVehicleCamera();
                vehicleCam.GetComponent<CameraFollow>().car = followTransform;
                vehicleCam.enabled = true;

                CinemachineCamera playerCam = GetPlayerCamera();
                playerCam.enabled = false;
            }
        }

        Camera GetVehicleCamera()
        {
            GameObject pfc = GameObject.Find("Camera_Vehicle");
            Camera cvc = pfc.GetComponent<Camera>();
            return cvc;
        }
        CinemachineCamera GetPlayerCamera()
        {
            GameObject pfc = GameObject.Find("PlayerFollowCamera");
            CinemachineCamera cvc = pfc.GetComponent<CinemachineCamera>();
            return cvc;
        }

        void Update()
        {
            if (isLocalPlayer)
            {
                if (RoomManager._instance != null && RoomManager._instance.MatchStarted)
                {
                    if (newMatch == false)
                    {
                        newMatch = true;
                        if (GetComponent<HunterAbilities>()._isHunter || !GetComponent<HunterAbilities>()._isHunter && PlayerPrefs.HasKey("EnableFirstPersonMode") && PlayerPrefs.GetInt("EnableFirstPersonMode") == 1)
                        {
                            isSwitchingPerspective = true;
                            CmdSetFirstPersonMode(GetComponent<NetworkIdentity>(), true);
                            target.SetParent(firstPersonTargetParent);
                            target.localPosition = firstPersonTargetPosition;
                            target.localRotation = firstPersonTargetRotation;
                            if (FirstPersonIdleCamera != null)
                                FirstPersonIdleCamera.SetActive(true);
                            c_HideMesh = StartCoroutine(HideHeadMesh(0.44f));
                        }
                    }
                }
                else
                {
                    newMatch = false;
                }

                if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                {
                    if (GetComponent<HunterAbilities>()._isHunter && !currentHunterId.Equals(GetComponent<HunterAbilities>().currentHunterID))
                    {
                        if (headMeshes.Count != 0)
                        {
                            headMeshes.Clear();
                        }

                        HunterAbilities hunterAbilities = GetComponent<HunterAbilities>();

                        if (hunterAbilities != null)
                        {
                            foreach (HunterAbilities.Hunter hunter in hunterAbilities.hunters)
                            {
                                if (hunter.HunterID.Equals(hunterAbilities.currentHunterID))
                                {
                                    foreach (GameObject hunterMesh in hunter.HunterMesh)
                                    {
                                        if (hunterMesh.name.Contains("headMesh"))
                                        {
                                            headMeshes.Add(hunterMesh);
                                        }

                                        AddChildHeadMeshes(hunterMesh.transform);
                                    }
                                }
                            }
                            currentHunterId = hunterAbilities.currentHunterID;
                            foreach (GameObject meshes in headMeshes)
                            {
                                meshes.SetActive(true);
                            }
                        }
                        else
                        {
                            Debug.LogError("HunterAbilities component not found!");
                        }
                    }

                    if (!GetComponent<HunterAbilities>()._isHunter && !currentSurvivorId.Equals(GetComponent<Player>().outfitId))
                    {
                        if (headMeshes.Count != 0)
                        {
                            headMeshes.Clear();
                        }

                        OutfitManager outfitManager = GetComponent<OutfitManager>();

                        if (outfitManager != null)
                        {
                            foreach (OutfitItem outfitItem in outfitManager.outfits)
                            {
                                if (outfitItem.outfitID.Equals(GetComponent<Player>().outfitId))
                                {
                                    AddChildHeadMeshes(outfitItem.outfitModel.transform);
                                }
                            }
                            currentSurvivorId = GetComponent<Player>().outfitId;
                            foreach (GameObject meshes in headMeshes)
                            {
                                meshes.SetActive(true);
                            }
                        }
                        else
                        {
                            Debug.LogError("OutfitManager component not found!");
                        }
                    }
                }
                else
                {
                    currentHunterId = 0;
                    currentSurvivorId = 0;
                    target.SetParent(thirdPersonTargetParent);
                    target.localPosition = thirdPersonTargetPosition;
                    target.localRotation = thirdPersonTargetRotation;
                }

                if (GetComponent<CharacterManager>().alive || !RoomManager._instance.MatchStarted || !RoomManager._instance.MatchRunning)
                {
                    bool currentToggleInput = _input.toggleCamera;

                    //Toggle camera mode
                    if (currentToggleInput && !lastToggleCameraInput)
                    {
                        if (!isSwitchingPerspective && RoomManager._instance.MatchRunning && RoomManager._instance.MatchStarted && !GetComponent<HunterAbilities>()._inFight && !GetComponent<HunterAbilities>()._isFighting && !GetComponent<HunterAbilities>().isBlocked && !GetComponent<CharacterManager>().isHealing && !GetComponent<PlayerInteractionModule>().isClimbing)
                        {
                            if (isFirstPerson)
                            {
                                isSwitchingPerspective = true;
                                CmdSetFirstPersonMode(GetComponent<NetworkIdentity>(), false);
                                target.SetParent(thirdPersonTargetParent);
                                target.localPosition = thirdPersonTargetPosition;
                                target.localRotation = thirdPersonTargetRotation;
                                if (FirstPersonIdleCamera != null)
                                    FirstPersonIdleCamera.SetActive(false);
                                StopCoroutine(c_HideMesh);
                                foreach (GameObject meshes in headMeshes)
                                {
                                    meshes.SetActive(true);
                                }
                                animator.SetBool("isFirstPerson", false);

                                isSwitchingPerspective = false;
                            }
                            else
                            {
                                isSwitchingPerspective = true;
                                CmdSetFirstPersonMode(GetComponent<NetworkIdentity>(), true);
                                target.SetParent(firstPersonTargetParent);
                                target.localPosition = firstPersonTargetPosition;
                                target.localRotation = firstPersonTargetRotation;
                                if (FirstPersonIdleCamera != null)
                                    FirstPersonIdleCamera.SetActive(true);
                                c_HideMesh = StartCoroutine(HideHeadMesh(0.44f));
                            }
                        }
                    }

                    lastToggleCameraInput = currentToggleInput;

                    if (GetComponent<HunterAbilities>()._inFight && isFirstPerson || !GetComponent<HunterAbilities>()._canUseItems && isFirstPerson)
                    {
                        foreach (GameObject meshes in headMeshes)
                        {
                            if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                                meshes.SetActive(true);
                        }
                    }
                    else if (!GetComponent<HunterAbilities>()._inFight && GetComponent<HunterAbilities>()._canUseItems && isFirstPerson && !isSwitchingPerspective)
                    {
                        foreach (GameObject meshes in headMeshes)
                        {
                            if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                                meshes.SetActive(false);
                        }
                    }
                    else if (!isFirstPerson && !isSwitchingPerspective)
                    {
                        foreach (GameObject meshes in headMeshes)
                        {
                            if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                                meshes.SetActive(true);
                        }
                    }

                    if (RoomManager._instance != null && RoomManager._instance.mainMenuManager.pauseMenuActive && isFirstPerson)
                    {
                        isSwitchingPerspective = true;
                        CmdSetFirstPersonMode(GetComponent<NetworkIdentity>(), false);
                        target.SetParent(thirdPersonTargetParent);
                        target.localPosition = thirdPersonTargetPosition;
                        target.localRotation = thirdPersonTargetRotation;
                        if (FirstPersonIdleCamera != null)
                            FirstPersonIdleCamera.SetActive(false);
                        StopCoroutine(c_HideMesh);
                        foreach (GameObject meshes in headMeshes)
                        {
                            if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                                meshes.SetActive(true);
                        }
                        animator.SetBool("isFirstPerson", false);

                        isSwitchingPerspective = false;
                    }
                }
                else
                {
                    if (isFirstPerson)
                    {
                        CmdSetFirstPersonMode(GetComponent<NetworkIdentity>(), false);
                        isSwitchingPerspective = false;
                        target.position = thirdPersonTargetPosition;
                        target.rotation = thirdPersonTargetRotation;
                        target.SetParent(thirdPersonTargetParent);
                        if (FirstPersonIdleCamera != null)
                            FirstPersonIdleCamera.SetActive(false);
                        StopCoroutine(HideHeadMesh(0.44f));
                        foreach (GameObject meshes in headMeshes)
                        {
                            if (RoomManager._instance != null && RoomManager._instance.MatchRunning)
                                meshes.SetActive(true);
                        }
                        animator.SetBool("isFirstPerson", false);
                    }
                }

                if (_characterManager != null)
                {
                    Vector3 mouseWorldPosition = Vector3.zero;
                    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);

                    aimTarget.position = ray.GetPoint(20f);
                    mouseWorldPosition = ray.GetPoint(20f);

                    Vector3 worldAimTarget = mouseWorldPosition;
                    worldAimTarget.y = transform.position.y;
                    Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                    if (_input.aim && !GetComponent<HunterAbilities>()._isFighting || isFirstPerson && !GetComponent<HunterAbilities>()._isFighting && !RoomManager._instance.mainMenuManager.pauseMenuActive)//Aim
                    {
                        if (_characterManager.itemCurrentlyInUse != null && _characterManager.itemCurrentlyInUse.canAim && !GetComponent<HunterAbilities>()._inFight && GetComponent<HunterAbilities>()._canUseItems && !GetComponent<HunterAbilities>().isBlocked && !GetComponent<CharacterManager>().isHealing && !GetComponent<PlayerInteractionModule>().isClimbing && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar && GetComponent<CharacterManager>().alive || isFirstPerson && !GetComponent<HunterAbilities>()._inFight && !GetComponent<HunterAbilities>().isBlocked && !GetComponent<CharacterManager>().isHealing && !GetComponent<PlayerInteractionModule>().isClimbing && !GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar && GetComponent<CharacterManager>().alive)
                        {
                            if (aimValue != 1)
                                CmdSetAimValue(1);

                            if (aimValue == 1)
                            {
                                this.GetComponent<PlayerInteractionModule>().playerInventory.isAiming = true;
                                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
                                if (PlayerRig != null)
                                    PlayerRig.weight = 1;
                                if (_characterManager.itemCurrentlyInUse != null && SecondHandRig_target != null)
                                {
                                    SecondHandRig_target.localPosition = _characterManager.itemCurrentlyInUse.leftHandPosition;
                                    SecondHandRig_target.localRotation = _characterManager.itemCurrentlyInUse.leftHandRotation;
                                }
                                if (thirdPersonController != null)
                                {
                                    thirdPersonController.SetSensitivity(0.5f);
                                    thirdPersonController.SetRotateOnMove(false);
                                }
                                if (isFirstPerson)
                                {
                                    if (WeaponAimCamera != null)
                                        WeaponAimCamera.SetActive(false);
                                    if (FirstPersonIdleCamera != null)
                                        FirstPersonIdleCamera.SetActive(true);
                                }
                                else
                                {
                                    if (FirstPersonIdleCamera != null)
                                        FirstPersonIdleCamera.SetActive(false);
                                    if (WeaponAimCamera != null)
                                        WeaponAimCamera.SetActive(true);
                                }
                            }
                        }
                    }
                    else if (!_input.aim && !isFirstPerson || !GetComponent<CharacterManager>().alive || isFirstPerson && GetComponent<HunterAbilities>()._isFighting)
                    {
                        this.GetComponent<PlayerInteractionModule>().playerInventory.isAiming = false;
                        if (isFirstPerson)
                            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
                        if (aimValue != 0)
                            CmdSetAimValue(0);
                        if (PlayerRig != null)
                            PlayerRig.weight = 0;
                        if (isFirstPerson)
                        {
                            if (FirstPersonIdleCamera != null)
                                FirstPersonIdleCamera.SetActive(true);
                            if (WeaponAimCamera != null)
                                WeaponAimCamera.SetActive(false);
                        }
                        else
                        {
                            if (FirstPersonIdleCamera != null)
                                FirstPersonIdleCamera.SetActive(false);
                            if (WeaponAimCamera != null)
                                WeaponAimCamera.SetActive(false);
                        }
                        if (thirdPersonController != null)
                        {
                            if (isFirstPerson)
                            {
                                thirdPersonController.SetSensitivity(0.5f);
                                thirdPersonController.SetRotateOnMove(false);
                            }
                            else
                            {
                                thirdPersonController.SetSensitivity(0.5f);
                                thirdPersonController.SetRotateOnMove(true);
                            }
                        }
                    }
                }

                if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("KickOut") && !isFirstPerson)
                {
                    if (!carTheftCamera.activeInHierarchy)
                        carTheftCamera.SetActive(true);
                }
                else
                {
                    if (carTheftCamera.activeInHierarchy)
                        carTheftCamera.SetActive(false);
                }
            }
            if (!isLocalPlayer)
            {
                if (aimValue == 1)//Aim
                {
                    if (PlayerRig != null)
                        PlayerRig.weight = 1;
                    if (_characterManager.itemCurrentlyInUse != null && SecondHandRig_target != null)
                    {
                        SecondHandRig_target.localPosition = _characterManager.itemCurrentlyInUse.leftHandPosition;
                        SecondHandRig_target.localRotation = _characterManager.itemCurrentlyInUse.leftHandRotation;
                    }
                    if (thirdPersonController != null)
                    {
                        thirdPersonController.SetSensitivity(0.5f);
                        thirdPersonController.SetRotateOnMove(false);
                    }
                }
                else if (aimValue == 0)
                {
                    if (PlayerRig != null)
                        PlayerRig.weight = 0;
                    if (thirdPersonController != null)
                    {
                        thirdPersonController.SetSensitivity(0.5f);
                        thirdPersonController.SetRotateOnMove(true);
                    }
                }
            }
        }

        [Command]
        void CmdSetAimValue(int value)
        {
            aimValue = value;

            RpcSetAimValue(value);
        }

        [ClientRpc]
        void RpcSetAimValue(int value)
        {
            aimValue = value;
        }

        private IEnumerator HideHeadMesh(float time)
        {
            yield return new WaitForSeconds(time);

            foreach (GameObject meshes in headMeshes)
            {
                meshes.SetActive(false);
            }

            animator.SetBool("isFirstPerson", true);

            yield return new WaitForEndOfFrame();

            if (GetComponent<CharacterManager>().itemCurrentlyInUse != null && GetComponent<CharacterManager>().currentSlot != 0)
            {
                if (GetComponent<CharacterManager>().itemCurrentlyInUse.canAim && GetComponent<CharacterManager>().itemCurrentlyInUse.isAiming)
                {
                    if (!isFirstPerson)
                        animator.SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        animator.SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!isFirstPerson)
                        animator.SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        animator.SetTrigger(GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }

            isSwitchingPerspective = false;
        }

        private void AddChildHeadMeshes(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (child.gameObject.name.Contains("headMesh"))
                {
                    headMeshes.Add(child.gameObject);
                }

                AddChildHeadMeshes(child);
            }
        }

        [Command]
        void CmdSetFirstPersonMode(NetworkIdentity playerNetId, bool firstPersonMode)
        {
            playerNetId.GetComponent<ManageTPController>().isFirstPerson = firstPersonMode;

            RpcSetFirstPersonMode(playerNetId, firstPersonMode);
        }

        [ClientRpc]
        void RpcSetFirstPersonMode(NetworkIdentity playerNetId, bool firstPersonMode)
        {
            playerNetId.GetComponent<ManageTPController>().isFirstPerson = firstPersonMode;

            if (playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse != null && playerNetId.GetComponent<CharacterManager>().currentSlot != 0)
            {
                if (playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.canAim && playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.isAiming)
                {
                    if (!isFirstPerson)
                        playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!isFirstPerson)
                        playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        playerNetId.GetComponent<Animator>().SetTrigger(playerNetId.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }
        }
    }
}