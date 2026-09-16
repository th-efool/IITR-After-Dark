// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using Mirror;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RedicionStudio.VehicleEnterExit;
using static RedicionStudio.VehicleEnterExit.VehicleSync;

namespace RedicionStudio
{
    public class PlayerInteraction : NetworkBehaviour
    {
        int _interactionColliderLayer = (1 << 9 | 1 << 0); //look only for vehicles and objects on default layer

        /// <summary>
        /// I had to put "EnterColliders" on their own layer because sometimes bullets were hitting them and vehicles avoided getting damage
        /// </summary>
        [SerializeField] Animator _anim;
        public bool inVehicle;
        [HideInInspector] public VehicleSync _myVehicle;
        public GameObject CarNameInfoPrefab;

        public delegate void OnPlayerDisconnects(PlayerInteraction player);
        public OnPlayerDisconnects PlayerEvent_OnPlayerDisconnects;

        private bool hasExit;
        private float currentVehicleVelocity;
        private int currentVehicleType = 0;

        private Transform _transformToStickTo;

        private RedicionStudio.PlayerInputs _input;

        void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (_transformToStickTo)
            {
                transform.position = _transformToStickTo.position;
                transform.rotation = _transformToStickTo.rotation;
            }

            if (!hasAuthority) return;

            if (!_input.interact) return;

            if (!inVehicle)
            {
                RaycastHit cameraHit;
                //we are shooting raycast from origin that is in front of our camera by 0.8meter, instead of directly from camera to avoid
                //occasionally hitting targets that are behind our player model
                if (Physics.Raycast(transform.position + transform.up * 0.8f, transform.forward, out cameraHit, 3f, _interactionColliderLayer))
                {
                    if (cameraHit.collider.gameObject)
                    {
                        VehicleEnter vehicleEnter = cameraHit.collider.gameObject.GetComponent<VehicleEnter>();

                        if (!vehicleEnter) return; //if we interected with nothing then do nothing

                        CmdRequestEntering(vehicleEnter.VehicleSync, vehicleEnter.SeatID, false, false);
                    }
                }
            }
            else
            {
                if (_myVehicle.GetComponent<CarController>() != null)
                {
                    currentVehicleVelocity = _myVehicle.GetComponent<CarController>().currentSpeed;
                    currentVehicleType = 1;
                }
                if (_myVehicle.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>() != null)
                {
                    currentVehicleVelocity = _myVehicle.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().currentSpeed;
                    currentVehicleType = 2;
                }
                Debug.Log("Player's rigidbody speed: " + currentVehicleVelocity);
                if (currentVehicleVelocity > 15)
                    _myVehicle.GetComponent<VehicleEnterExit.VehicleSync>().EnterAnimationDuration = 0.5f;
                if (currentVehicleVelocity < 15)
                    _myVehicle.GetComponent<VehicleEnterExit.VehicleSync>().EnterAnimationDuration = 2.45f;
                CmdRequestExiting();
            }
        }
        [Command]
        void CmdRequestEntering(VehicleSync vehicleSync, int seatID, bool forcedEnter, bool driverKickedOut)
        {
            vehicleSync.RequestEntering(seatID, this, forcedEnter, driverKickedOut);
        }
        [Command]
        void CmdRequestExiting()
        {
            _myVehicle.RequestExiting(this);
        }

        public void ExitVehicle(Seat seat, float animDuration, int animID, bool forcedExit)
        {
            if (!forcedExit)
                _anim.Play("ExitVehicle" + animID);
            else
                _anim.Play("GetKickedOut");

            if (forcedExit)
            {
                transform.position = seat.GetKickedOutPoint().position;
                transform.rotation = seat.GetKickedOutPoint().rotation;
            }

            if (currentVehicleVelocity > 15)
            {
                animDuration = 0.5f;
            }

            StartCoroutine(Teleport());
            IEnumerator Teleport()
            {
                yield return new WaitForSeconds(animDuration);
                if (currentVehicleVelocity > 15)
                {
                    hasExit = true;
                    if (!forcedExit)
                    {
                        transform.position = seat.EnterPoint.position;
                        transform.rotation = seat.EnterPoint.rotation;
                    }

                    //reanable player movement
                    BlockPlayer(false, false);
                    GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

                    inVehicle = false;

                    if (hasAuthority)
                    {
                        GetComponent<ManageTPController>().StickCameraToPlayer();
                    }

                    if (!forcedExit)
                        GetComponent<Health>().FallFromVehicle(currentVehicleType);
                }
                if (hasExit == false & currentVehicleVelocity < 15)
                {
                    if (!forcedExit)
                    {
                        transform.position = seat.EnterPoint.position;
                        transform.rotation = seat.EnterPoint.rotation;
                    }

                    //reanable player movement
                    BlockPlayer(false, false);

                    GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

                    inVehicle = false;

                    if (hasAuthority)
                    {
                        GetComponent<ManageTPController>().StickCameraToPlayer();
                    }

                    _anim.Play("Idle Walk Run Blend");
                }
                hasExit = false;
            }
        }

        public void KickOutOtherPlayer(Transform pointToTeleport, float animDuration, VehicleSync vehicle, bool forcedEnter)
        {
            _transformToStickTo = pointToTeleport;

            _anim.Play("KickOut");

            BlockPlayer(true, false);
            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = true;

            /*if (GetComponent<HunterAbilities>()._isHunter)
            {
                animDuration = 128f; // 2:08 in seconds
            }*/

            StartCoroutine(Teleport());
            IEnumerator Teleport()
            {
                yield return new WaitForSeconds(animDuration);

                //reanable player movement
                //BlockPlayer(false, false);
                _transformToStickTo = null;
                //_anim.Play("Idle Walk Run Blend");
                if (GetComponent<HunterAbilities>()._isHunter)
                {
                    //reanable player movement
                    BlockPlayer(false, false);

                    GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

                    inVehicle = false;

                    _anim.Play("Hunter Idle Walk Run Blend");

                    transform.position = vehicle._seats[0].hunterKickOutPoint.position;
                    //transform.rotation = vehicle._seats[0].hunterKickOutPoint.rotation;

                    RpcBlockKillerToGetIntoVhicle();
                }
                else
                {
                    CmdRequestEntering(vehicle, 0, forcedEnter, true);
                }
            }
        }

        public void ForceExitVehicle()
        {
            //reanable player movement
            BlockPlayer(false, false);

            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

            inVehicle = false;

            if (hasAuthority)
            {
                GetComponent<ManageTPController>().StickCameraToPlayer();
            }

            if (_myVehicle != null)
                transform.position = _myVehicle._seats[0].KickOutPoint().position;

            _anim.Play("Idle Walk Run Blend");
        }

        [ClientRpc]
        void RpcBlockKillerToGetIntoVhicle()
        {
            //reanable player movement
            BlockPlayer(false, false);

            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

            inVehicle = false;

            if (hasAuthority)
            {
                GetComponent<ManageTPController>().StickCameraToPlayer();
            }

            _anim.Play("Hunter Idle Walk Run Blend");
        }

        public void EnterVehicle(VehicleSync vehicleToEnter, int animID)
        {
            if (hasAuthority)
            {
                GetComponent<ManageTPController>().StickCameraToVehicle(vehicleToEnter.transform);
            }
            if (isLocalPlayer)
            {
                GameObject _carNameInfo = Instantiate(CarNameInfoPrefab);
                _carNameInfo.GetComponent<CarNameInfo>().SetUpCarNameText(vehicleToEnter.VehicleName);
                _carNameInfo = null;
            }
            //disable player movement and collision when in car
            BlockPlayer(true, true);
            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = true;

            _myVehicle = vehicleToEnter;
            inVehicle = true;
            _anim.Play("EnterVehicle" + animID);
        }

        void BlockPlayer(bool block, bool blockCamera = true)
        {

            GetComponent<ThirdPersonController>().BlockPlayer(hasAuthority ? block : false, blockCamera);
            GetComponent<CharacterController>().enabled = !block;
            GetComponent<CapsuleCollider>().enabled = !block;
            GetComponent<NetworkTransform>().enabled = !block;
        }

        public void Exited()
        {
            _myVehicle = null;
        }

        public void Disconnect()
        {
            PlayerEvent_OnPlayerDisconnects?.Invoke(this);
        }
    }
}