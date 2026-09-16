// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CharacterInteraction : NetworkBehaviour
    {

        public KeyCode useButton = KeyCode.E;

        [SerializeField] float _interactionRange;
        int _interactionLayerMask = (1 << 0 | 1 << 9);
        InteractableObject _clientInteractableObject;
        InteractableObject _serverMyInteractableObject;

        bool _clientCurrentlyInteracting;

        public delegate void StartedInteraction(InteractableObject objectToInteract);
        public StartedInteraction CharacterEvent_StartedInteraction;

        public delegate void EndedInteraction();
        public EndedInteraction CharacterEvent_EndedInteraction;

        private CharacterManager _characterManager;

        Coroutine c_enableItemMesh;

        private RedicionStudio.PlayerInputs _input;

        private void Start()
        {
            _characterManager = GetComponent<CharacterManager>();
        }
        void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (_input.interact && _characterManager.alive)
            {
                RaycastHit hit;
                if (!_clientCurrentlyInteracting && Physics.Raycast(transform.position + transform.up * 1f, transform.forward, out hit, _interactionRange, _interactionLayerMask))
                {
                    if (hit.collider.gameObject.TryGetComponent(out InteractableObject interactableObject))
                    {
                        if (interactableObject.AbleToBeInteracted(GetComponent<CharacterManager>().Team))
                        {
                            //if looking at the interactable object and pushing use button, send server message that we want to use an object
                            _clientCurrentlyInteracting = true;
                            ClientRequestInteraction(interactableObject);
                        }
                    }
                }
            }
            else if (_clientCurrentlyInteracting)
            {
                ClientRequestEndInteraction();
                _clientCurrentlyInteracting = false;
            }
        }
        [Command]
        void ClientRequestInteraction(InteractableObject interactableObject)
        {
            if (_serverMyInteractableObject) return;

            _serverMyInteractableObject = interactableObject;
            _serverMyInteractableObject.StartInteraction();

            _serverMyInteractableObject.InteractableObject_ServerEvent_Activated += ServerEndInteraction;

            RpcPlayerStartedInteraction(interactableObject);

            _characterManager.Rpc_SetMovementPermission(false);
        }
        [Command]
        void ClientRequestEndInteraction()
        {
            ServerEndInteraction();
        }

        //launched when: player finished using object, player releases use button
        void ServerEndInteraction()
        {
            if (_serverMyInteractableObject)
            {
                GetComponent<Player>().instrumentsUsed += 1;
                GetComponent<CharacterManager>().TempInstrumentsUsed += 1;

                RpcPlayerEndedInteraction();

                _characterManager.Rpc_SetMovementPermission(true);

                _serverMyInteractableObject.InteractableObject_ServerEvent_Activated -= ServerEndInteraction;

                _serverMyInteractableObject.EndInteraction();

                _serverMyInteractableObject = null;
            }
        }
        [ClientRpc]
        void RpcPlayerStartedInteraction(InteractableObject interactableObject)
        {
            _clientInteractableObject = interactableObject;

            CharacterEvent_StartedInteraction?.Invoke(interactableObject);
            //lerp character position and rotation to interactable object user position
            if (interactableObject.userPosition)
            {
                _characterManager.LerpCharacterPositionToDestination(interactableObject.userPosition.position, 4.5f);
                _characterManager.SetCharacterRotation(interactableObject.userPosition.rotation);
                if (interactableObject.GetComponent<PhoneReviver>() != null)
                {
                    if (c_enableItemMesh != null)
                    {
                        StopCoroutine(c_enableItemMesh);
                        c_enableItemMesh = null;
                    }
                    if (_characterManager.itemCurrentlyInUse != null)
                    {
                        foreach (MeshRenderer itemMeshRenderer in _characterManager.itemCurrentlyInUse._itemMesh)
                            itemMeshRenderer.enabled = false;
                    }
                    _characterManager.GetComponent<Animator>().SetTrigger("UseTelephone");
                    interactableObject.GetComponent<PhoneReviver>().telephoneReceiverMesh.enabled = false;
                }
            }
        }

        [ClientRpc]
        void RpcPlayerEndedInteraction()
        {
            if (!_clientInteractableObject) return;

            if (_clientInteractableObject.GetComponent<PhoneReviver>() != null)
            {
                _characterManager.GetComponent<Animator>().SetTrigger("TelephoneUseEnd");
                _clientInteractableObject.GetComponent<PhoneReviver>().telephoneReceiverMesh.enabled = true;
                c_enableItemMesh = StartCoroutine(EnableItemMesh());
            }
            CharacterEvent_EndedInteraction?.Invoke();
            _clientCurrentlyInteracting = false;
            _clientInteractableObject = null;
            GetComponent<Player>().SetInstrumentsUsedValue(1);

        }

        IEnumerator EnableItemMesh()
        {
            yield return new WaitForSeconds(1);

            if (_characterManager.itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer itemMeshRenderer in _characterManager.itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = true;
                if (!_characterManager.GetComponent<ManageTPController>().isFirstPerson)
                    _characterManager.GetComponent<Animator>().SetTrigger(_characterManager.itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    _characterManager.GetComponent<Animator>().SetTrigger(_characterManager.itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
            }
        }
    }
}