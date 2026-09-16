// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using StarterAssets;

namespace RedicionStudio
{
    public class CarController : Vehicle
    {
        private const string HORIZONTAL = "Horizontal";
        private const string VERTICAL = "Vertical";
        private RedicionStudio.PlayerInputs _input;
        Vehicle _vehicle;

        [SyncVar] public float horizontalInput;
        [SyncVar] public float verticalInput;
        public float currentSteerAngle;
        private float currentbreakForce;
        public bool isBreaking;
        public GameObject brakeLights;
        public GameObject driveBackwardsLights;
        public GameObject HeadLightsOn;
        public GameObject HeadLightsOff;
        [SyncVar] private bool areHeadLightsOn = false;
        [SyncVar] private bool areBrakeLightsOn = false;
        [SyncVar] private bool areReverseLightsOn = false;

        [SerializeField] public float motorForce;
        [SerializeField] public float breakForce;
        [SerializeField] public float maxSteerAngle;

        [SerializeField] public WheelCollider frontLeftWheelCollider;
        [SerializeField] public WheelCollider frontRightWheelCollider;
        [SerializeField] public WheelCollider rearLeftWheelCollider;
        [SerializeField] public WheelCollider rearRightWheelCollider;

        [SerializeField] public Transform frontLeftWheelTransform;
        [SerializeField] public Transform frontRightWheeTransform;
        [SerializeField] public Transform rearLeftWheelTransform;
        [SerializeField] public Transform rearRightWheelTransform;

        [Header("Speed")]
        public float topSpeed = 100;
        [SyncVar] public float currentSpeed = 0;
        private Vector3 lastPos;

        [Header("UI")]
        public GameObject VehicleUI;

        [Header("SteeringWheel")]
        [SerializeField] private Transform SteeringWheel;
        [SerializeField] private float rotateBackSpeed = 3f;
        [SerializeField] private float rotateSpeed = 10f;
        [SerializeField] private float angle = 0f;
        [SerializeField] private float minAngle = -120f;
        [SerializeField] private float maxAngle = 120f;
        [SerializeField] private float neutralAngle = 0f;

        [Header("Camera")]
        public float Cameradistance = 6.4f;
        public float Cameraheight = 1.4f;
        public float CamerarotationDamping = 3.0f;
        public float CameraheightDamping = 10.0f;
        public float CamerazoomRatio = 0.5f;
        public float CameradefaultFOV = 60f;
        public GameObject firstPersonCamera;
        [Header("Collision")]
        public GameObject hitEffect;
        public GameObject bloodEffect;
        private float dotP;
        public int demageCount;

        private bool increaseCarDrag = true;

        #region networking properties
        [Header("Networking")]
        ///time between synchronization of player input, for example 0.1 seconds mean that input will be sent
        ///to server 10 times per second
        public float SyncInputTime = 0.1f;
        private float _tickTimer;
        #endregion

        public enum EngineAudioOptions
        {
            Simple,
            FourChannel
        }
        [Header("Sound")]
        public EngineAudioOptions engineSoundStyle = EngineAudioOptions.FourChannel;
        public AudioClip lowAccelClip;
        public AudioClip lowDecelClip;
        public AudioClip highAccelClip;
        public AudioClip highDecelClip;
        public float pitchMultiplier = 1f;
        public float lowPitchMin = 1f;
        public float lowPitchMax = 6f;
        public float highPitchMultiplier = 0.25f;
        public float maxRolloffDistance = 500;
        public float dopplerLevel = 1;
        public bool useDoppler = true;

        private AudioSource LowAccel;
        private AudioSource LowDecel;
        private AudioSource HighAccel;
        private AudioSource HighDecel;
        private bool StartedSound;

        [Header("Gear")]
        [SerializeField] private static int NoOfGears = 5;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;
        public float Revs { get; private set; }
        [SerializeField] private float m_RevRangeBoundary = 1f;

        [Header("Police Car")]
        public bool isPoliceCar = false;
        public GameObject LightbarOn;
        public GameObject PoliceSirenSound;
        [SyncVar] private bool isLightbarOn = false;

        [Header("Horn")]
        [Tooltip("0 = Default, 1 = Default02, 2 = Sport")]
        public int CarHornID = 0;
        public GameObject CarHornSoundPrefab;

        [Header("Turn on sound")]
        private bool canPlayFailureTurnOnSound = true;
        public AudioClip turnOnSoundClip;

        private void Start()
        {
            _vehicle = GetComponent<Vehicle>();
            StartSound();
        }

        private void FixedUpdate()
        {
            if (hasAuthority)
            {
                if (GetComponent<VehicleHealth>().currentHealth == 100)
                    GetInput();

            }

            HandleMotor();
            HandleSteering();
            UpdateWheels();
            CalculateRevs();
            GearChanging();
        }

        private void Update()
        {
            if (_vehicle.isControlledByCarAi == false)
            {
                if (increaseCarDrag == true)
                    GetComponent<Rigidbody>().linearDamping = 0.5f;
                else if (increaseCarDrag == false)
                    GetComponent<Rigidbody>().linearDamping = 0;

                foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player == null)
                        {
                            rearLeftWheelCollider.motorTorque = 0;
                            rearRightWheelCollider.motorTorque = 0;
                            verticalInput = 0;
                            increaseCarDrag = true;
                            currentSpeed = 0;
                            foreach (var source in GetComponents<AudioSource>())
                            {
                                source.enabled = false;
                            }
                            GetComponent<CarAI>().LeftTurnLights.SetActive(false);
                            GetComponent<CarAI>().RightTurnLights.SetActive(false);
                        }
                        else if (seat.Player != null)
                        {
                            foreach (var source in GetComponents<AudioSource>())
                            {
                                if (!isServer && GetComponent<VehicleHealth>().currentHealth == 100)
                                {
                                    source.enabled = true;
                                }
                                if (!isServer && GetComponent<VehicleHealth>().currentHealth != 100)
                                {
                                    if (Input.GetKeyDown(KeyCode.W) && canPlayFailureTurnOnSound)
                                    {
                                        AudioSource.PlayClipAtPoint(turnOnSoundClip, transform.position);
                                        StartCoroutine(WaitForTurnOnSoundToEnd());
                                    }
                                }
                                if (!isServer)
                                {
                                    if (hasAuthority)
                                    {
                                        if (GetComponent<VehicleHealth>().currentHealth == 100)
                                        {
                                            if (!areHeadLightsOn)
                                            {
                                                CmdSetHeadLights(true);
                                            }
                                        }
                                        else
                                        {
                                            if (areHeadLightsOn)
                                            {
                                                CmdSetHeadLights(false);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                currentSpeed = transform.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f;
                foreach (var source in GetComponents<AudioSource>())
                {
                    if (!isServer)
                        source.enabled = true;
                }
            }

            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (canDrive)
            {
                currentSpeed = transform.GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f;
                VehicleUI.SetActive(true);

                /*if (Input.GetKeyDown(KeyCode.L))
                {
                    if (areHeadLightsOn)
                    {
                        if (hasAuthority)
                            CmdSetHeadLights(false);
                    }
                    else
                    {
                        if (hasAuthority)
                            CmdSetHeadLights(true);
                    }
                }*/

                if (isPoliceCar & Input.GetKeyDown(KeyCode.Space))
                {
                    if (hasAuthority)
                    {
                        if (isLightbarOn)
                            CmdSetLightbar(false);
                        else
                            CmdSetLightbar(true);
                    }
                }

                if (Input.GetKeyDown(KeyCode.H))
                {
                    if (hasAuthority)
                        CmdHorn(CarHornID, transform.position, transform.rotation, gameObject.name);
                }

                angle = Mathf.Clamp(angle + Input.GetAxis("Horizontal") * rotateSpeed
                        * Time.deltaTime, minAngle, maxAngle);

                if (Mathf.Approximately(0f, Input.GetAxis("Horizontal")))
                {
                    angle = Mathf.MoveTowardsAngle(angle, neutralAngle,
                            rotateBackSpeed * Time.deltaTime);
                }

                SteeringWheel.eulerAngles = angle * Vector3.forward;
            }
            else
            {
                VehicleUI.SetActive(false);
            }

            if (_tickTimer <= Time.time)
            {
                _tickTimer = Time.time + SyncInputTime;
                Tick();
            }

            //Lights
            dotP = Vector3.Dot(transform.forward.normalized, this.GetComponent<Rigidbody>().linearVelocity.normalized);
            if (dotP > 0.5f)
            {
                //forward
                if (hasAuthority & areReverseLightsOn)
                    CmdSetReverseLights(false);
            }
            else if (dotP < -0.5f)
            {
                //reverse
                if (hasAuthority & !areReverseLightsOn)
                    CmdSetReverseLights(true);
                if (hasAuthority & areReverseLightsOn & areBrakeLightsOn)
                    CmdSetBrakeLights(false);
            }
            else
            {
                //sliding sideways
            }

            //Sound
            float camDist = (Camera.main.transform.position - transform.position).sqrMagnitude;

            /*if (StartedSound && camDist > maxRolloffDistance * maxRolloffDistance)
            {
                StopSound();
            }

            if (!StartedSound && camDist < maxRolloffDistance * maxRolloffDistance)
            {
                StartSound();
            }*/

            if (StartedSound)
            {
                float pitch = ULerp(lowPitchMin, lowPitchMax, Revs);

                pitch = Mathf.Min(lowPitchMax, pitch);

                if (engineSoundStyle == EngineAudioOptions.Simple)
                {
                    HighAccel.pitch = pitch * pitchMultiplier * highPitchMultiplier;
                    HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    HighAccel.volume = 1;
                }
                else
                {
                    LowAccel.pitch = pitch * pitchMultiplier;
                    LowDecel.pitch = pitch * pitchMultiplier;
                    HighAccel.pitch = pitch * highPitchMultiplier * pitchMultiplier;
                    HighDecel.pitch = pitch * highPitchMultiplier * pitchMultiplier;

                    float accFade = Mathf.Abs(verticalInput);
                    float decFade = 1 - accFade;

                    float highFade = Mathf.InverseLerp(0.2f, 0.8f, Revs);
                    float lowFade = 1 - highFade;

                    highFade = 1 - ((1 - highFade) * (1 - highFade));
                    lowFade = 1 - ((1 - lowFade) * (1 - lowFade));
                    accFade = 1 - ((1 - accFade) * (1 - accFade));
                    decFade = 1 - ((1 - decFade) * (1 - decFade));

                    LowAccel.volume = lowFade * accFade;
                    LowDecel.volume = lowFade * decFade;
                    HighAccel.volume = highFade * accFade;
                    HighDecel.volume = highFade * decFade;

                    HighAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    LowAccel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    HighDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                    LowDecel.dopplerLevel = useDoppler ? dopplerLevel : 0;
                }
            }

            if (_vehicle.isControlledByCarAi == false & !canDrive)
            {
                HeadLightsOff.SetActive(true);
                HeadLightsOn.SetActive(false);
            }

            if (areHeadLightsOn)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player == null)
                        {
                            HeadLightsOff.SetActive(true);
                            HeadLightsOn.SetActive(false);
                        }
                        else
                        {
                            HeadLightsOn.SetActive(true);
                            HeadLightsOff.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                HeadLightsOff.SetActive(true);
                HeadLightsOn.SetActive(false);
            }

            if (areBrakeLightsOn)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player == null)
                        {
                            brakeLights.SetActive(false);
                        }
                        else
                        {
                            brakeLights.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                brakeLights.SetActive(false);
            }

            if (areReverseLightsOn)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player == null)
                        {
                            driveBackwardsLights.SetActive(false);
                        }
                        else
                        {
                            driveBackwardsLights.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                driveBackwardsLights.SetActive(false);
            }

            if (isPoliceCar)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player == null)
                        {
                            if (isLightbarOn)
                            {
                                PoliceSirenSound.SetActive(false);
                            }
                        }
                        else
                        {
                            if (isLightbarOn)
                            {
                                if (!isServer)
                                    PoliceSirenSound.SetActive(true);
                            }
                        }
                    }
                }
                if (isLightbarOn == true)
                    LightbarOn.SetActive(true);
                else
                    LightbarOn.SetActive(false);
            }

            if (GetComponent<VehicleHealth>().currentHealth != 100)
            {
                rearLeftWheelCollider.motorTorque = 0;
                rearRightWheelCollider.motorTorque = 0;
                verticalInput = 0;
                increaseCarDrag = true;
                currentSpeed = 0;
            }
        }

        #region networking
        void Tick()
        {
            if (isServer)
            {
                RpcUpdateInput(horizontalInput, verticalInput, currentSteerAngle, currentbreakForce, isBreaking);
            }
            else if (hasAuthority)
            {
                CmdSendInputToServer(horizontalInput, verticalInput, currentSteerAngle, currentbreakForce, isBreaking, currentSpeed);
            }
        }
        [Command]
        void CmdSendInputToServer(float _horizontalInput, float _verticalInput, float _currentSteerAngle, float _currentbreakForce, bool _isBreaking, float _currentSpeed)
        {
            ApplyInput(_horizontalInput, _verticalInput, _currentSteerAngle, _currentbreakForce, _isBreaking);
            currentSpeed = _currentSpeed;
            RpcSyncSpeed(_currentSpeed);
        }
        [ClientRpc]
        void RpcSyncSpeed(float _currentSpeed)
        {
            currentSpeed = _currentSpeed;
        }

        [ClientRpc(includeOwner = false)]
        void RpcUpdateInput(float _horizontalInput, float _verticalInput, float _currentSteerAngle, float _currentbreakForce, bool _isBreaking)
        {
            ApplyInput(_horizontalInput, _verticalInput, _currentSteerAngle, _currentbreakForce, _isBreaking);
        }
        void ApplyInput(float _horizontalInput, float _verticalInput, float _currentSteerAngle, float _currentbreakForce, bool _isBreaking)
        {
            horizontalInput = _horizontalInput;
            verticalInput = _verticalInput;
            currentSteerAngle = _currentSteerAngle;
            currentbreakForce = _currentbreakForce;
            isBreaking = _isBreaking;
        }
        #endregion

        private void GetInput()
        {
            if (canDrive == true)
            {
                //horizontalInput = Input.GetAxis(HORIZONTAL);
                horizontalInput = _input.move.x;
                if (_input.move.y != 0)
                    increaseCarDrag = false;
                else if (_input.move.y == 0)
                    increaseCarDrag = true;
                verticalInput = _input.move.y;
                isBreaking = _input.aim;
                if (_input.aim || Input.GetKey(KeyCode.S) & !areReverseLightsOn)
                {
                    if (!areBrakeLightsOn)
                    {
                        if (hasAuthority)
                            CmdSetBrakeLights(true);
                    }
                }
                else
                {
                    if (hasAuthority & areBrakeLightsOn)
                        CmdSetBrakeLights(false);
                }
            }
        }

        private void HandleMotor()
        {
            rearLeftWheelCollider.motorTorque = verticalInput * motorForce;
            rearRightWheelCollider.motorTorque = verticalInput * motorForce;
            currentbreakForce = isBreaking ? breakForce : 0f;
            ApplyBreaking();
        }

        private void ApplyBreaking()
        {
            frontRightWheelCollider.brakeTorque = currentbreakForce;
            frontLeftWheelCollider.brakeTorque = currentbreakForce;
            rearLeftWheelCollider.brakeTorque = currentbreakForce;
            rearRightWheelCollider.brakeTorque = currentbreakForce;
        }

        private void HandleSteering()
        {
            currentSteerAngle = maxSteerAngle * horizontalInput;
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle;
        }

        private void UpdateWheels()
        {
            UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform, "front", "left");
            UpdateSingleWheel(frontRightWheelCollider, frontRightWheeTransform, "front", "right");
            UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform, "rear", "right");
            UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform, "rear", "left");
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform, string _position, string _site)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            if (isControlledByCarAi)
            {
                if (_position == "rear")
                {
                    wheelTransform.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);
                }
                else if (_position == "front")
                {
                    wheelTransform.localEulerAngles = new Vector3((transform.position - lastPos).magnitude / Time.deltaTime * 360.0f, currentSteerAngle, 0);
                }

                wheelTransform.position = pos;

                lastPos = transform.position;
            }
            else
            {
                if (canDrive)
                {
                    wheelTransform.rotation = rot;
                    wheelTransform.position = pos;
                }
                else
                {
                    if (_position == "rear")
                    {
                        wheelTransform.localEulerAngles = new Vector3(currentSpeed * 360.0f, 0, 0);
                    }
                    else if (_position == "front")
                    {
                        wheelTransform.localEulerAngles = new Vector3(currentSpeed * 360.0f, currentSteerAngle, 0);
                    }

                    wheelTransform.position = pos;
                }
            }
        }

        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor) * (1 - factor);
        }

        private void CalculateGearFactor()
        {
            float f = (1 / (float)NoOfGears);
            var targetGearFactor = Mathf.InverseLerp(f * m_GearNum, f * (m_GearNum + 1), Mathf.Abs(currentSpeed / topSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime * 5f);
        }


        private void CalculateRevs()
        {
            CalculateGearFactor();
            var gearNumFactor = m_GearNum / (float)NoOfGears;
            var revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            var revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }

        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value) * from + value * to;
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(currentSpeed / topSpeed);
            float upgearlimit = (1 / (float)NoOfGears) * (m_GearNum + 1);
            float downgearlimit = (1 / (float)NoOfGears) * m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == 1)
            {
                Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
            }
            if (collision.gameObject.tag != "Player" & collision.gameObject.layer != 6 & collision.gameObject.layer != 1)
            {
                GameObject hit = collision.gameObject;

                ContactPoint contact = collision.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;
                SpawnHitEffect(pos, rot);
                if (collision.relativeVelocity.magnitude > 5)
                {
                    float demage = 1 * collision.relativeVelocity.magnitude;
                    demageCount = (int)demage;
                    //demageCount = (int)currentSpeed;
                }

                if (collision.gameObject.tag != "Bullet")
                {
                    if (isServer)
                    {
                        RpcSetCrashDemage(demageCount);
                    }
                    else if (hasAuthority)
                    {
                        CmdSetCrashDemage(demageCount);
                    }
                }
            }
            if (collision.gameObject.tag == "Player" & collision.gameObject.layer == 6 & collision.gameObject.layer != 1)
            {
                GameObject hit = collision.gameObject;

                ContactPoint contact = collision.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;
                demageCount = (int)currentSpeed;
                if (demageCount > 5)
                {
                    SpawnBloodEffect(pos, rot);

                    if (isServer)
                    {
                        RpcSetPlayerDemage(100, collision.gameObject.GetComponent<Health>());

                        if (collision.gameObject.GetComponent<Health>().currentHealth <= 0)
                        {

                        }
                        else
                        {
                            foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                            {
                                if (seat.DriverSeat == true)
                                {
                                    if (seat.Player != null)
                                    {
                                        RpcSetPlayerXP(20, seat.Player.GetComponent<RedicionStudio.InventorySystem.Player>());
                                    }
                                }
                            }
                        }

                        //RpcSetCrashDemage(5);
                    }
                    else if (hasAuthority)
                    {
                        CmdSetPlayerDemage(100, collision.gameObject.GetComponent<Health>());

                        if (collision.gameObject.GetComponent<Health>().currentHealth <= 0)
                        {

                        }
                        else
                        {
                            foreach (VehicleEnterExit.VehicleSync.Seat seat in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                            {
                                if (seat.DriverSeat == true)
                                {
                                    if (seat.Player != null)
                                    {
                                        CmdSetPlayerXP(20, seat.Player.GetComponent<RedicionStudio.InventorySystem.Player>());
                                    }
                                }
                            }
                        }

                        //CmdSetCrashDemage(5);
                    }
                }
            }
        }

        [Command]
        void CmdSetCrashDemage(int _demage)
        {
            GetComponent<VehicleHealth>().TakeDamage(_demage);
        }

        [ClientRpc]
        void RpcSetCrashDemage(int _demage)
        {
            GetComponent<VehicleHealth>().TakeDamage(_demage);
        }

        void SpawnHitEffect(Vector3 _position, Quaternion _rotation)
        {
            Instantiate(hitEffect, _position, _rotation);
        }

        [Command]
        void CmdSetPlayerDemage(int _demage, Health _health)
        {
            _health.TakeDamage(_demage, AttackType.Car, currentPlayerUsername());
        }

        [ClientRpc]
        void RpcSetPlayerDemage(int _demage, Health _health)
        {
            _health.TakeDamage(_demage, AttackType.Car, currentPlayerUsername());
        }

        void SpawnBloodEffect(Vector3 _position, Quaternion _rotation)
        {
            Instantiate(bloodEffect, _position, _rotation);
        }

        [Command]
        void CmdSetHeadLights(bool _status)
        {
            areHeadLightsOn = _status;

            RpcSetHeadLights(_status);
        }

        [ClientRpc]
        void RpcSetHeadLights(bool _status)
        {
            areHeadLightsOn = _status;
        }

        [Command]
        void CmdSetBrakeLights(bool _status)
        {
            areBrakeLightsOn = _status;

            RpcSetBrakeLights(_status);
        }

        [ClientRpc]
        void RpcSetBrakeLights(bool _status)
        {
            areBrakeLightsOn = _status;
        }

        [Command]
        void CmdSetReverseLights(bool _status)
        {
            areReverseLightsOn = _status;

            RpcSetReverseLights(_status);
        }

        [ClientRpc]
        void RpcSetReverseLights(bool _status)
        {
            areReverseLightsOn = _status;
        }

        public string currentPlayerUsername()
        {
            return GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername;
        }

        [Command]
        void CmdSetLightbar(bool _status)
        {
            isLightbarOn = _status;

            RpcSetLightbar(_status);
        }

        [ClientRpc]
        void RpcSetLightbar(bool _status)
        {
            isLightbarOn = _status;
        }

        [Command]
        void CmdSetPlayerXP(int _xp, RedicionStudio.InventorySystem.Player _player)
        {
            _player.SetExperience(_xp);
        }

        [ClientRpc]
        void RpcSetPlayerXP(int _xp, RedicionStudio.InventorySystem.Player _player)
        {
            _player.SetExperience(_xp);
        }

        [Command]
        void CmdHorn(int _HornID, Vector3 _position, Quaternion _rotation, string carServerObjectName)
        {
            GameObject _CarHornSoundPrefab = Instantiate(CarHornSoundPrefab, _position, _rotation) as GameObject;

            NetworkServer.Spawn(_CarHornSoundPrefab, connectionToClient);

            CarHorn _CarHornSoundPrefabScript = _CarHornSoundPrefab.GetComponent<CarHorn>();
            _CarHornSoundPrefabScript.netIdentity.AssignClientAuthority(this.connectionToClient);

            _CarHornSoundPrefabScript.PlayCarHorn(_HornID, _position, carServerObjectName);

            RpcHorn(_CarHornSoundPrefabScript, _HornID, _position, carServerObjectName);
        }

        [ClientRpc]
        void RpcHorn(CarHorn _CarHornSoundPrefabScript, int _HornID, Vector3 _position, string carServerObjectName)
        {
            _CarHornSoundPrefabScript.PlayCarHorn(_HornID, _position, carServerObjectName);
        }

        //Sound
        void StartSound()
        {
            HighAccel = SetUpEngineAudioSource(highAccelClip);

            if (engineSoundStyle == EngineAudioOptions.FourChannel)
            {
                LowAccel = SetUpEngineAudioSource(lowAccelClip);
                LowDecel = SetUpEngineAudioSource(lowDecelClip);
                HighDecel = SetUpEngineAudioSource(highDecelClip);
            }

            StartedSound = true;
        }

        private void StopSound()
        {
            foreach (var source in GetComponents<AudioSource>())
            {
                Destroy(source);
            }

            StartedSound = false;
        }

        IEnumerator WaitForTurnOnSoundToEnd()
        {
            canPlayFailureTurnOnSound = false;

            yield return new WaitForSeconds(turnOnSoundClip.length);

            canPlayFailureTurnOnSound = true;
        }

        private AudioSource SetUpEngineAudioSource(AudioClip clip)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.spatialBlend = 1;
            source.volume = 0;
            source.loop = true;

            source.time = UnityEngine.Random.Range(0f, clip.length);
            source.Play();
            source.minDistance = 5;
            source.maxDistance = 10;
            source.dopplerLevel = 0;
            source.enabled = false;
            return source;
        }
    }
}