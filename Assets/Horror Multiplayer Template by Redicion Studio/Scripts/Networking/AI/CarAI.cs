// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using UnityEngine;
using System.Collections;
using Mirror;
using Random = UnityEngine.Random;

namespace RedicionStudio
{
    [RequireComponent(typeof(CarController))]
    public class CarAI : NetworkBehaviour
    {
        [Header("Car Behavior Settings")]
        public bool canDrive = false;
        [SyncVar] public float desiredSpeed = 12;
        public float steeringSensitivity = 1f;
        [SyncVar] public bool isSpecialTagObjectInFront = false;
        [SyncVar] public bool ignoreObstacles = false;

        [Space]
        [Header("Waypoint")]
        public Transform currentWaypoint = null;

        [Space]
        [Header("Dependencies")]
        public CarController carController;

        [Space]
        [Header("Raycast")]
        private float currentDistanceToObjectInFront;
        public float Raycastheight = 0.5f;
        public string[] ObstaclesTags;
        public float DisatanceUntilStop = 3.5f;

        [Header("Turn Lights")]
        public GameObject LeftTurnLights;
        public GameObject RightTurnLights;

        private void Awake()
        {
            if (carController == null & GetComponent<CarController>() != null)
                carController = GetComponent<CarController>();
        }

        private void FixedUpdate()
        {
            if (currentWaypoint == null || !canDrive)
            {
                //Brake(true);
            }
            else
            {
                Vector3 offsetTargetPos = currentWaypoint.position;

                offsetTargetPos += currentWaypoint.right *
                                   (Mathf.PerlinNoise(Time.time * 0.1f, 0.1f) * 2 - 1) *
                                   0.1f;

                Vector3 localTarget = transform.InverseTransformPoint(offsetTargetPos);

                float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

                float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(carController.currentSpeed);
                if (carController.isControlledByCarAi)
                    carController.horizontalInput = steer;
            }
            if (carController.isControlledByCarAi)
            {
                float accel = Mathf.Clamp((desiredSpeed - GetComponent<Rigidbody>().linearVelocity.magnitude * 3.6f) * 0.4f, -1, 1);

                accel *= (1 - 0.1f) +
                         (Mathf.PerlinNoise(Time.time * 0.1f, 0.1f) * 0.1f);

                carController.verticalInput = accel;

                /*RaycastHit hit;

                Vector3[] directions = { new Vector3(transform.position.x, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x + 0.3f, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x + 0.6f, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x + 1f, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x - 0.3f, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x - 0.6f, transform.position.y + Raycastheight, transform.position.z), new Vector3(transform.position.x - 1f, transform.position.y + Raycastheight, transform.position.z) };

                foreach (Vector3 direction in directions)
                {
                    if (Physics.Raycast(direction, transform.TransformDirection(Vector3.forward), out hit))
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(direction, transform.TransformDirection(Vector3.forward));

                        currentDistanceToObjectInFront = hit.distance;

                        foreach (string tag in ObstaclesTags)
                        {
                            if (hit.transform.tag == tag)
                            {
                                if (isSpecialTagObjectInFront == false & currentDistanceToObjectInFront < DisatanceUntilStop)
                                {
                                    isSpecialTagObjectInFront = true;
                                    Brake(true);
                                }
                                continue;
                            }
                            if (isSpecialTagObjectInFront == true)
                            {
                                isSpecialTagObjectInFront = false;
                                Brake(false);
                            }
                        }
                    }
                }*/
                if (carController.currentSteerAngle >= -1 && carController.currentSteerAngle <= 1)
                {
                    LeftTurnLights.SetActive(false);
                    RightTurnLights.SetActive(false);
                }
                else if (carController.currentSteerAngle >= 5 && carController.currentSteerAngle <= 30)
                {
                    LeftTurnLights.SetActive(false);
                    RightTurnLights.SetActive(true);
                }
                else if (carController.currentSteerAngle >= -5 && carController.currentSteerAngle <= -30)
                {
                    RightTurnLights.SetActive(false);
                    LeftTurnLights.SetActive(true);
                }
            }
        }

        public void Brake(bool brake)
        {
            if (brake)
            {
                carController.verticalInput = 0;
                carController.isBreaking = brake;
            }
            else
            {
                carController.isBreaking = brake;
            }
        }

        public void SetWaypoint(Transform waypoint)
        {
            currentWaypoint = waypoint;
        }
    }
}