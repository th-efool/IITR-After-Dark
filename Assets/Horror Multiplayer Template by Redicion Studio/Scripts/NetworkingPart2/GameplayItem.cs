// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class GameplayItem : NetworkBehaviour
    {
        [Header("UI")]
        public Sprite icon;
        public GameObject interactionIndicator;

        [Header("Item stats")]
        public string ItemName = "item";
        public int Damage = 10;

        public float useCooldown = 0.1f;
        float _useTimer = 0f;

        public bool canUsedByHunter = false;
        public bool canAim;
        public bool isAiming = false;
        public string aimAnimatorTriggerName;
        public string idleAnimatorTriggerName;
        public string firstPersonAimAnimatorTriggerName;
        public string firstPersonIdleAnimatorTriggerName;

        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;

        public bool useOverrideController = false;
        public AnimatorOverrideController AnimatorController;

        [Tooltip("Set the following variable to true if the aim animation of the item is in layer 4 of the player animator.")]
        public bool useAnimationLayer4WhenAiming = false;

        protected SphereCollider _interactionTrigger;
        protected BoxCollider _itemCollider;
        protected CharacterManager _myOwner { get; private set; }
        protected StarterAssets.ThirdPersonController _TPController;

        [HideInInspector] [SyncVar] public bool followItemContainerPosition = false;
        [HideInInspector] [SyncVar] public NetworkIdentity itemContainer;
        [HideInInspector] [SyncVar] public int itemContainerPositionIndex;

        public bool InUse { protected set; get; } = false;

        Transform _itemParent;

        public MeshRenderer[] _itemMesh;

        public bool IsOwned { private set; get; } = false;

        [HideInInspector] public RedicionStudio.PlayerInputs _input;


        protected virtual void Awake()
        {
            _interactionTrigger = GetComponent<SphereCollider>();
            _itemCollider = GetComponent<BoxCollider>();
            transform.gameObject.layer = 10;
        }
        protected virtual void FixedUpdate()
        {
            UpdateTransform();
        }
        protected virtual void LateUpdate()
        {
            UpdateTransform();
        }
        protected virtual void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            UpdateTransform();
            if (_myOwner)
            {
                if (InUse)
                {
                    if (hasAuthority)
                    {
                        _useTimer += Time.deltaTime;
                        if (_input.use && !_myOwner.gameObject.GetComponent<HunterAbilities>()._inFight && _myOwner.gameObject.GetComponent<HunterAbilities>()._canUseItems && !_myOwner.gameObject.GetComponent<HunterAbilities>().isBlocked && !_myOwner.gameObject.GetComponent<CharacterManager>().isHealing && !_myOwner.gameObject.GetComponent<PlayerInteractionModule>().isClimbing && !_myOwner.gameObject.GetComponent<PlayerInteractionModule>().isUsingRadio && !_myOwner.gameObject.GetComponent<CharacterManager>().usesWeapon && !RoomManager._instance.mainMenuManager.pauseMenuActive && RoomManager._instance.MatchStarted)
                        {
                            if (_useTimer >= useCooldown)
                            {
                                _useTimer = 0f;
                                ClientUse();
                            }
                        }
                    }
                }
            }
        }

        protected virtual void UpdateTransform()
        {
            if (_myOwner)
            {
                followItemContainerPosition = false;
                if (InUse)
                {
                    transform.SetPositionAndRotation(_itemParent.position, _itemParent.rotation);
                }
                else
                {
                    transform.SetPositionAndRotation(_myOwner.transform.position, _myOwner.transform.rotation);
                }
            }
            else
            {
                if (followItemContainerPosition)
                {
                    if (itemContainer != null)
                    {
                        ItemContainerManager itemContainerManager = itemContainer.GetComponent<ItemContainerManager>();

                        if (itemContainerManager != null)
                        {
                            List<Transform> availableSpawnPositions = itemContainerManager.availableSpawnPositions;

                            if (itemContainerPositionIndex >= 0 && itemContainerPositionIndex < availableSpawnPositions.Count)
                            {
                                Transform _spawnPosition = availableSpawnPositions[itemContainerPositionIndex];

                                if (_spawnPosition != null)
                                {
                                    transform.SetPositionAndRotation(_spawnPosition.position, _spawnPosition.rotation);
                                }
                            }
                        }
                    }
                }
            }
        }

        void ClientUse()
        {
            Use();
            ClientRequest_Use();
        }
        [Command]
        void ClientRequest_Use()
        {
            if (!hasAuthority)
                Use();

            Server_Use();


            Rpc_Use();
        }

        protected virtual void Server_Use()
        {

        }

        [ClientRpc]
        void Rpc_Use()
        {
            if (!isServer && !hasAuthority)
                Use();
        }

        ///method launched for every client when item is being used
        public virtual void Use() { }
        public virtual void SecondaryUse() { }

        public virtual void Equip(CharacterManager myNewOwner, Transform itemParent)
        {
            SetAsInteractable(false);
            _myOwner = myNewOwner;
            _TPController = myNewOwner.GetComponent<StarterAssets.ThirdPersonController>();

            interactionIndicator.SetActive(false);

            _itemParent = itemParent;

            IsOwned = true;

            if (!myNewOwner.GetComponent<ManageTPController>().isFirstPerson)
                SetAnimatorTrigger(idleAnimatorTriggerName);
            else
                SetAnimatorTrigger(firstPersonIdleAnimatorTriggerName);
        }
        public virtual void Drop()
        {
            transform.gameObject.layer = 10;
            _itemCollider.isTrigger = false;

            //interactionIndicator.SetActive(true);
            SetAsInteractable(true);
            _myOwner = null;
            foreach (MeshRenderer meshRenderer in _itemMesh)
                meshRenderer.enabled = true;
            GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

            IsOwned = false;
        }

        public virtual void Activate()
        {
            if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                SetAnimatorTrigger(idleAnimatorTriggerName);
            else
                SetAnimatorTrigger(firstPersonIdleAnimatorTriggerName);
            foreach (MeshRenderer meshRenderer in _itemMesh)
                meshRenderer.enabled = true;
            InUse = true;
        }
        public virtual void Putdown()
        {
            foreach (MeshRenderer meshRenderer in _itemMesh)
                meshRenderer.enabled = false;
            InUse = false;
        }

        public void SetAsInteractable(bool _set)
        {
            transform.gameObject.layer = 10;

            //_interactionTrigger.enabled = _set;
            _itemCollider.enabled = _set;
        }

        void SetAnimatorTrigger(string triggerName)
        {
            _myOwner.GetComponent<Animator>().SetTrigger(triggerName);
            if (hasAuthority)
            {
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), triggerName);
            }
            else
            {
                RpcSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), triggerName);
            }
        }

        [Command]
        void CmdSetAnimatorTrigger(NetworkIdentity playerNetId, string triggerName)
        {
            playerNetId.GetComponent<Animator>().SetTrigger(triggerName);

            RpcSetAnimatorTrigger(playerNetId, triggerName);
        }

        [ClientRpc]
        void RpcSetAnimatorTrigger(NetworkIdentity playerNetId, string triggerName)
        {
            playerNetId.GetComponent<Animator>().SetTrigger(triggerName);
        }

    }
}