// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedicionStudio;
using RedicionStudio.InventorySystem;

namespace RedicionStudio.VehicleEnterExit
{
    [RequireComponent(typeof(NetworkTransform))]
    public class VehicleSync : NetworkBehaviour
    {
        public string VehicleName;
        [Space]
        public float EnterAnimationDuration = 1f;
        public float EnterAnimationShortenedDuration = 0.5f;
        public float KickOutAnimationDuration = 4.9f;

        private NetworkTransform _networkTransform;

        Rigidbody _rigidBody;

        private PlayerInteraction _vehicleOwner;

        public Seat[] _seats;

        [SyncVar] public string DriverUsername = "default";

        Vehicle _vehicle;
        CarAI _carAI;

        public CustomNetManager NetManager;

        public bool AI = false;

        public AudioClip openDoorSound;
        public AudioClip closeDoorSound;

        void Start()
        {
            _vehicle = GetComponent<Vehicle>();
            _carAI = GetComponent<CarAI>();

            _rigidBody = GetComponent<Rigidbody>();
            _networkTransform = GetComponent<NetworkTransform>();
            _networkTransform.clientAuthority = true;

            if(NetManager == null)
            {
                NetManager = GameObject.FindGameObjectWithTag("NetworkManager").GetComponent<CustomNetManager>();

                if(NetManager != null)
                    NetManager.NetworkManagerEvent_OnPlayerConnected += OnNewClientConnected;
            }
            else
            {
                NetManager.NetworkManagerEvent_OnPlayerConnected += OnNewClientConnected;
            }
        }

        void LateUpdate()
        {
            for (int i = 0; i < _seats.Length; i++)
            {
                if (_seats[i].Player)
                    _seats[i].Sync();
            }
        }

        void Update()
        {
            for (int i = 0; i < _seats.Length; i++)
            {
                if (_seats[i].Player)
                    _seats[i].Sync();
            }
        }

        void FixedUpdate()
        {
            for (int i = 0; i < _seats.Length; i++)
            {
                if (_seats[i].Player)
                    _seats[i].Sync();
            }
        }

        private void OnDestroy()
        {
            if (NetManager != null)
                NetManager.NetworkManagerEvent_OnPlayerConnected -= OnNewClientConnected;
        }

        void OnNewClientConnected(NetworkConnection conn)
        {
            List<PlayerInteraction> playersInVehicle = new List<PlayerInteraction>();

            for (int i = 0; i < _seats.Length; i++)
            {
                playersInVehicle.Add(_seats[i].Player ? _seats[i].Player.GetComponent<PlayerInteraction>() : null);
            }

            TargetRpcUpdateVehicleForLateClient(conn, playersInVehicle.ToArray());
        }

        [TargetRpc]
        void TargetRpcUpdateVehicleForLateClient(NetworkConnection conn, PlayerInteraction[] players)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i])
                {
                    _seats[i].Player = players[i].gameObject;
                    players[i].EnterVehicle(this, _seats[i].AnimID);
                }
            }
        }

        #region server & host only
        public void RequestEntering(int seatID, PlayerInteraction player, bool forcedEnter, bool driverKickedOut)
        {
            //bunch of checks if player requesting entering car can enter it
            if (seatID >= _seats.Length) return;

            Seat seat = _seats[seatID];

            if (seatID != 0 && AI) return; //dont let players acces any other doors than drivers door if AI is controlling the car

            if (player.connectionToClient == null && !player.hasAuthority)
                AI = true;

            if (!seat.Player) //entering car
            {
                if (player.GetComponent<HunterAbilities>()._isHunter)
                    return;

                if (seat.PlayerSitting) return;

                //disable player collider
                player.EnterVehicle(this, seat.AnimID);

                seat.Player = player.gameObject;

                if (seat.DriverSeat)
                {
                    RemoveVehicleOwner();

                    AssingVehicleOwner(player);

                    //assing driver nick for this vehicle
                    //DriverUsername = player.GetComponent<Player>().username;
                }

                StopSeatProcedureIfExist(seat);

                seat.EnteringExitingProcedure = StartCoroutine(EnteringProcedure());

                IEnumerator EnteringProcedure()
                {
                    if(!forcedEnter)
                        yield return new WaitForSeconds(EnterAnimationDuration);
                    else
                        yield return new WaitForSeconds(EnterAnimationShortenedDuration);
                    if (seat.DriverSeat)
                        EnableCarForDriver(true);

                    seat.PlayerSitting = true;

                    //enable car driving for bot if it is bot
                    if (player.connectionToClient == null)
                    {
                        _vehicle.isControlledByCarAi = true;
                        _carAI.canDrive = true;
                        _carAI.Brake(false);
                    }

                    RpcToogleSeatFirstPersonCamera(seatID, player.GetComponent<NetworkIdentity>(), true);

                    seat.EnteringExitingProcedure = null;
                }

                if(!forcedEnter)
                    PlayDoorAnimations(seat, EnterAnimationDuration, forcedEnter, false, driverKickedOut, false);
                else
                    PlayDoorAnimations(seat, EnterAnimationShortenedDuration, forcedEnter, false, driverKickedOut, false);
                RpcPlayerEnters(player, seatID, forcedEnter);
            }
            else if (seat.DriverSeat) //we have to kick out driver
            {

                if (seat.EnteringExitingProcedure != null) return; //dont want to kick out player that is entering/exiting car

                RequestExiting(seat.Player.GetComponent<PlayerInteraction>(), true);
                _carAI.Brake(true);

                RpcPlayerKickOutPlayer(player, seatID);
            }
        }
        public void RequestExiting(PlayerInteraction player, bool forcedExit = false)
        {
            Seat seatToEmpty = _seats[GetSeatIDPlayer(player)];

            float finalAnimaDuration = forcedExit ? KickOutAnimationDuration : EnterAnimationDuration;

            if (seatToEmpty == null) return;

            if (!seatToEmpty.PlayerSitting) return;
            seatToEmpty.PlayerSitting = false;

            if (seatToEmpty.DriverSeat)
            {
                EnableCarForDriver(false);
            }


            if (player.connectionToClient == null)
            {
                AI = false;
                player.GetComponent<PlayerAI>().ServerEvent_GotKickedOutOfCar();
                _carAI.Brake(true);
                _carAI.canDrive = false;
                _vehicle.isControlledByCarAi = false;
            }

            seatToEmpty.Player.GetComponent<PlayerInteraction>().ExitVehicle(seatToEmpty, finalAnimaDuration, seatToEmpty.AnimID, forcedExit);

            StopSeatProcedureIfExist(seatToEmpty);

            seatToEmpty.EnteringExitingProcedure = StartCoroutine(ExitingProcedure());

            RpcToogleSeatFirstPersonCamera(GetSeatIDPlayer(player), player.GetComponent<NetworkIdentity>(), false);

            IEnumerator ExitingProcedure()
            {
                seatToEmpty.ForcedExiting = forcedExit;
                yield return new WaitForSeconds(finalAnimaDuration);

                seatToEmpty.Player.GetComponent<PlayerInteraction>().Exited();
                seatToEmpty.Player = null;
                seatToEmpty.ForcedExiting = false;
                seatToEmpty.EnteringExitingProcedure = null;
            }

            PlayDoorAnimations(seatToEmpty, finalAnimaDuration, false, forcedExit, false, true);

            RpcPlayerExits(player, forcedExit);
            if(GetComponent<CarController>() != null)
                GetComponent<CarController>().currentSpeed = 0;
        }

        void StopSeatProcedureIfExist(Seat seat)
        {
            if (seat.EnteringExitingProcedure != null)
            {
                StopCoroutine(seat.EnteringExitingProcedure);
                seat.EnteringExitingProcedure = null;
            }
        }

        void AssingVehicleOwner(PlayerInteraction player)
        {
            _vehicleOwner = player;

            //if it is null than it means it is bot whot tries to enter vehicle
            if (player.GetComponent<NetworkIdentity>().connectionToClient != null)
            {
                netIdentity.AssignClientAuthority(player.GetComponent<NetworkIdentity>().connectionToClient);

                _vehicleOwner.PlayerEvent_OnPlayerDisconnects += OnPlayerDisconnected;
            }
        }
        void RemoveVehicleOwner()
        {
            if (!_vehicleOwner) return;

            if (_vehicleOwner.GetComponent<NetworkIdentity>().connectionToClient != null)
            {
                netIdentity.RemoveClientAuthority();

                _vehicleOwner.PlayerEvent_OnPlayerDisconnects -= OnPlayerDisconnected;
            }
            _vehicleOwner = null;
        }
        /// <summary>
        /// Driver of vehicle has authority to his vehicle, so if he disconnects, then all objects that are 
        /// possed by him are destroyed, we dont want the whole vehicle to vanish if player that drives it disconnects 
        /// so we remove authority of this car for this player before we destroy all of his possed objects
        /// </summary>
        /// <param name="player"></param>
        public void OnPlayerDisconnected(PlayerInteraction player)
        {
            RemoveVehicleOwner();

            int seatID = GetSeatIDPlayer(player);


            if (seatID != -1)
            {
                Seat seat = _seats[seatID];

                seat.Player = null;
                seat.PlayerSitting = false;

                //if vehicle owner was in vehicle while disconnecting, then send
                //info to all clients that his seat is no longer occupied and
                //can be retaken

                StopSeatProcedureIfExist(seat);

                RpcUpdateSeatState(seatID, null);
            }
        }
        #endregion

        #region client only
        [ClientRpc]
        void RpcUpdateSeatState(int seatIdToUpdate, PlayerInteraction player)
        {
            if (seatIdToUpdate < 0) return;

            Seat seatToUpdate = _seats[seatIdToUpdate];

            seatToUpdate.Player = player != null ? player.gameObject : null;
            seatToUpdate.PlayerSitting = player != null ? true : false;


            StopSeatProcedureIfExist(seatToUpdate); //if player disconnected while entering/exiting vehicle
        }


        [ClientRpc]
        void RpcPlayerEnters(PlayerInteraction player, int seatID, bool forcedEnter)
        {
            if (isServer) return;
            PlayerEnters(player, seatID, forcedEnter);
        }
        [ClientRpc]
        void RpcPlayerKickOutPlayer(PlayerInteraction player, int seatID)
        {
            PlayerKickOut(player, seatID);
        }
        void PlayerEnters(PlayerInteraction player, int seatID, bool forcedEnter)
        {
            Seat seat = _seats[seatID];

            if(!forcedEnter)
                player.EnterVehicle(this, seat.AnimID);
            else
                player.EnterVehicle(this, seat.AnimShortenedID);

            seat.PlayerSitting = false;
            seat.Player = player.gameObject;


            if(!forcedEnter)
                PlayDoorAnimations(seat, EnterAnimationDuration, forcedEnter, false, false, false);
            else
                PlayDoorAnimations(seat, EnterAnimationShortenedDuration, forcedEnter, false, false, false);
        }
        void PlayerKickOut(PlayerInteraction player, int seatID)
        {
            player.KickOutOtherPlayer(_seats[seatID].KickOutPoint(), KickOutAnimationDuration, this, true);
        }

        [ClientRpc]
        void RpcPlayerExits(PlayerInteraction player, bool forcedExit)
        {
            PlayerExits(player, forcedExit);
        }
        void PlayerExits(PlayerInteraction player, bool forcedExit)
        {
            if (isServer) return;

            float finalAnimDuration = forcedExit ? KickOutAnimationDuration : EnterAnimationDuration;

            Seat seatToEmpty = _seats[GetSeatIDPlayer(player)];
            seatToEmpty.Player.GetComponent<PlayerInteraction>().ExitVehicle(seatToEmpty, finalAnimDuration, seatToEmpty.AnimID, forcedExit);

            if (seatToEmpty.EnteringExitingProcedure != null)
            {
                StopCoroutine(seatToEmpty.EnteringExitingProcedure);
                seatToEmpty.EnteringExitingProcedure = null;
            }
            seatToEmpty.EnteringExitingProcedure = StartCoroutine(ExitingProcedure());

            IEnumerator ExitingProcedure()
            {
                seatToEmpty.ForcedExiting = forcedExit;
                yield return new WaitForSeconds(finalAnimDuration);

                seatToEmpty.Player = null;
                seatToEmpty.ForcedExiting = false;
                seatToEmpty.EnteringExitingProcedure = null;
            }

            PlayDoorAnimations(seatToEmpty, finalAnimDuration, false, forcedExit, false, true);
        }
        #endregion

        void EnableCarForDriver(bool canDrive)
        {
            if (!hasAuthority)
                GetComponent<Rigidbody>().isKinematic = canDrive;

            if (_vehicleOwner.connectionToClient == null)
                GetComponent<Rigidbody>().isKinematic = false;

            _networkTransform.clientAuthority = _vehicleOwner.connectionToClient != null;
            _vehicle.canDrive = hasAuthority && canDrive;

            RPCEnableVehicleForDriver(canDrive, _vehicleOwner.connectionToClient == null);
        }
        [ClientRpc]
        void RPCEnableVehicleForDriver(bool canDrive, bool bot)
        {
            if (isServer) return;
            //_networkTransform.clientAuthority = true;
            _networkTransform.clientAuthority = !bot;
            _rigidBody.isKinematic = !hasAuthority;

            if (bot)
                _rigidBody.isKinematic = true;

            _vehicle.canDrive = hasAuthority && canDrive && !bot;

        }

        //we want to open door at the start of animation and close at the end,
        //so we need animation duration, and seat that will be used
        void PlayDoorAnimations(Seat seat, float animationDuration, bool forcedEnter, bool forcedExit, bool driverKickedOut, bool isExiting)
        {
            if (!seat.Door) return;

            float doorAnimationDuration = 0.3f;

            if (!forcedEnter)
                seat.Door.localRotation = seat.DoorClosed;
            else
                seat.Door.localRotation = seat.DoorOpened;

            StartCoroutine(DoorOpenClose());
            if (!forcedEnter)
                PlayClipAt(openDoorSound, transform.position, 0.21f, 1, 10);

            IEnumerator DoorOpenClose()
            {
                if (!forcedExit)
                {
                    if (!forcedEnter)
                    {
                        yield return new WaitForSeconds(0.10f);

                        LerpDoor(seat, seat.DoorOpened);

                        yield return new WaitForSeconds(animationDuration - doorAnimationDuration);
                        LerpDoor(seat, seat.DoorClosed);
                        PlayClipAt(closeDoorSound, transform.position, 0.21f, 1, 10);
                    }
                    else
                    {
                        if(isExiting)
                            yield return new WaitForSeconds(1.17f);
                        else
                            yield return new WaitForSeconds(animationDuration - doorAnimationDuration);

                        LerpDoor(seat, seat.DoorClosed);
                        PlayClipAt(closeDoorSound, transform.position, 0.21f, 1, 10);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(0.11f);

                    LerpDoor(seat, seat.DoorOpened);
                }
            }

            void LerpDoor(Seat seat, Quaternion lerpRotation)
            {
                if (seat.LerpDoorAnimation != null)
                {
                    StopCoroutine(seat.LerpDoorAnimation);
                    seat.LerpDoorAnimation = null;
                }
                seat.LerpDoorAnimation = StartCoroutine(LerpDoor());

                IEnumerator LerpDoor()
                {
                    Transform door = seat.Door;

                    float percentage;
                    float progress = 0;

                    Quaternion startRoot = door.localRotation;

                    while (progress < doorAnimationDuration)
                    {
                        percentage = progress / doorAnimationDuration;

                        door.localRotation = Quaternion.Lerp(startRoot, lerpRotation, percentage);
                        progress += Time.deltaTime;

                        yield return null;
                    }
                    door.localRotation = lerpRotation;
                }
            }
        }

        int GetSeatIDPlayer(PlayerInteraction player)
        {
            int seatID = -1;

            for (int i = 0; i < _seats.Length; i++)
            {
                if (_seats[i].Player == player.gameObject)
                    seatID = i;
            }

            return seatID;
        }

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance)
        {
            if(!isServer)
            {
                var tempGO = new GameObject("CarDoorAudio");
                tempGO.transform.position = _position;
                var aSource = tempGO.AddComponent<AudioSource>();
                aSource.clip = _clip;
                aSource.volume = _volume;
                aSource.minDistance = _minDistance;
                aSource.maxDistance = _maxDistance;
                aSource.reverbZoneMix = 1;
                aSource.spatialBlend = 1;
                aSource.Play();
                Destroy(tempGO, _clip.length);
            }
        }

        [ClientRpc]
        void RpcToogleSeatFirstPersonCamera(int seatId, NetworkIdentity playerNetId, bool _show)
        {
            if(NetworkClient.localPlayer.GetComponent<Player>().username.Equals(playerNetId.GetComponent<Player>().username))
            {
                if(NetworkClient.localPlayer.GetComponent<ManageTPController>().isFirstPerson)
                {
                    _seats[seatId].seatFirstPersonCamera.SetActive(_show);
                }
            }
        }

        [System.Serializable]
        public class Seat
        {
            public bool DriverSeat;
            //this points for different situations can be reduced by making more universal animations
            public Transform SeatPoint;
            public Transform EnterPoint;
            [SerializeField] Transform _getKickedOutPoint;
            [SerializeField] Transform _kickOutPoint;
            public bool ForcedExiting;
            public Transform GetKickedOutPoint() { return _getKickedOutPoint ? _getKickedOutPoint : EnterPoint; }
            public Transform KickOutPoint() { return _kickOutPoint ? _kickOutPoint : EnterPoint; }
            public Transform hunterKickOutPoint;

            public int AnimID; //will be useful when you would like to have more different entering animation, for jets for example
            public int AnimShortenedID; //Enter the ID of the shortened enter animation that plays after the player kicks another player or bot out of the vehicle.

            //we need that flag to know if player is playing entering/exiting animation or he already sits
            //so for example if he is in half of entering animation, than he cannot exit vehicle, he can exit only
            //when he sits
            //[HideInInspector] 
            public bool PlayerSitting;

            public GameObject Player;

            public Transform Door;
            public Quaternion DoorOpened;
            public Quaternion DoorClosed;

            public Coroutine LerpDoorAnimation;
            public Coroutine EnteringExitingProcedure;

            public GameObject seatFirstPersonCamera;

            public void Sync()
            {
                if (!ForcedExiting)
                {
                    Player.transform.position = SeatPoint.position;
                    Player.transform.rotation = SeatPoint.rotation;
                }
                else
                {
                    Player.transform.position = GetKickedOutPoint().position;
                    Player.transform.rotation = GetKickedOutPoint().rotation;
                }
            }
        }
    }
}