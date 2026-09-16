using StarterAssets;
using System;
using UnityEngine;
using Mirror;
using RedicionStudio;

namespace UnityStandardAssets.Vehicles.Aeroplane
{
    [RequireComponent(typeof (AeroplaneController))]
    public class AeroplaneUserControl4Axis : Vehicle
    {
        // these max angles are only used on mobile, due to the way pitch and roll input are handled
        public float maxRollAngle = 80;
        public float maxPitchAngle = 80;

        // reference to the aeroplane that we're controlling
        private AeroplaneController m_Aeroplane;
        private float m_Throttle;
        private bool m_AirBrakes;
        private float m_Yaw;
        private RedicionStudio.PlayerInputs _input;
        [SerializeField, Range(0f, 100f)] float inputSensitivity = 0.002f;

        //public bool canFly = false;


        private void Awake()
        {
            // Set up the reference to the aeroplane controller.
            m_Aeroplane = GetComponent<AeroplaneController>();
        }


        private void FixedUpdate()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();
            //if(canFly == true)
            if (canDrive == true)
            {
                // Read input for the pitch, yaw, roll and throttle of the aeroplane.
                float roll = _input.look.x * inputSensitivity;
                float pitch = _input.look.y * inputSensitivity;
                m_AirBrakes = _input.aim;
                m_Yaw = _input.move.x;
                m_Throttle = _input.move.y;
#if MOBILE_INPUT
        AdjustInputForMobileControls(ref roll, ref pitch, ref m_Throttle);
#endif
                // Pass the input to the aeroplane
                m_Aeroplane.Move(roll, pitch, m_Yaw, m_Throttle, m_AirBrakes);

                CmdMoveAeroplane(m_Aeroplane, roll, pitch, m_AirBrakes, m_Yaw, m_Throttle);
            }
        }

        [Command]
        void CmdMoveAeroplane(AeroplaneController _aeroplane, float _roll, float _pitch, bool _m_AirBrakes, float _m_Yaw, float _m_Throttle)
        {
            //m_Aeroplane.Move(_roll, _pitch, _m_Yaw, _m_Throttle, _m_AirBrakes);

            _aeroplane.RollInput = _roll;
            _aeroplane.PitchInput = _pitch;
            _aeroplane.AirBrakes = _m_AirBrakes;
            _aeroplane.YawInput = _m_Yaw;
            _aeroplane.ThrottleInput = _m_Throttle;

            RpcMoveAeroplane(_aeroplane, _roll, _pitch, _m_AirBrakes, _m_Yaw, _m_Throttle);
        }

        [ClientRpc]
        void RpcMoveAeroplane(AeroplaneController _aeroplane, float _roll, float _pitch, bool _m_AirBrakes, float _m_Yaw, float _m_Throttle)
        {
            //m_Aeroplane.Move(_roll, _pitch, _m_Yaw, _m_Throttle, _m_AirBrakes);

            _aeroplane.RollInput = _roll;
            _aeroplane.PitchInput = _pitch;
            _aeroplane.AirBrakes = _m_AirBrakes;
            _aeroplane.YawInput = _m_Yaw;
            _aeroplane.ThrottleInput = _m_Throttle;
        }

        private void AdjustInputForMobileControls(ref float roll, ref float pitch, ref float throttle)
        {
            // because mobile tilt is used for roll and pitch, we help out by
            // assuming that a centered level device means the user
            // wants to fly straight and level!

            // this means on mobile, the input represents the *desired* roll angle of the aeroplane,
            // and the roll input is calculated to achieve that.
            // whereas on non-mobile, the input directly controls the roll of the aeroplane.

            float intendedRollAngle = roll*maxRollAngle*Mathf.Deg2Rad;
            float intendedPitchAngle = pitch*maxPitchAngle*Mathf.Deg2Rad;
            roll = Mathf.Clamp((intendedRollAngle - m_Aeroplane.RollAngle), -1, 1);
            pitch = Mathf.Clamp((intendedPitchAngle - m_Aeroplane.PitchAngle), -1, 1);
        }
    }
}
