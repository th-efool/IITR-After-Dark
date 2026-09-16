// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform car;
        public float distance = 6.4f;
        public float height = 1.4f;
        public float rotationDamping = 3.0f;
        public float heightDamping = 10.0f;
        public float zoomRatio = 0.5f;
        public float defaultFOV = 60f;
        private Vector3 rotationVector;

        void LateUpdate()
        {
            if (car != null)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in car.GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player != null)
                        {
                            if (seat.Player.GetComponent<Player>().username == NetworkClient.localPlayer.gameObject.GetComponent<Player>().username)
                                return;
                        }
                    }
                }

                if (car.GetComponent<CarController>() != null)
                {
                    distance = car.GetComponent<CarController>().Cameradistance;
                    height = car.GetComponent<CarController>().Cameraheight;
                    rotationDamping = car.GetComponent<CarController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<CarController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<CarController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<CarController>().CameradefaultFOV;
                }
                else if (car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>() != null)
                {
                    distance = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameradistance;
                    height = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameraheight;
                    rotationDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameradefaultFOV;
                }

                float wantedAngle = rotationVector.y;
                float wantedHeight = car.position.y + height;
                float myAngle = transform.eulerAngles.y;
                float myHeight = transform.position.y;

                myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.fixedDeltaTime);
                myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.fixedDeltaTime);

                Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);
                transform.position = car.position;
                transform.position -= currentRotation * Vector3.forward * distance;
                Vector3 temp = transform.position;
                temp.y = myHeight;
                transform.position = temp;
                transform.LookAt(car);

                /*if (car.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername != NetworkClient.localPlayer.gameObject.GetComponent<Player>().username)
                    return;*/

                Vector3 localVelocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().linearVelocity);
                if (localVelocity.z < -0.5f)
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y + 180;
                    rotationVector = temp;
                }
                else
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y;
                    rotationVector = temp;
                }
                float acc = car.GetComponent<Rigidbody>().linearVelocity.magnitude;
                this.GetComponent<Camera>().fieldOfView = defaultFOV + acc * zoomRatio * Time.fixedDeltaTime;
            }
        }

        void FixedUpdate()
        {
            if (car != null)
            {
                if (car.GetComponent<CarController>() != null)
                {
                    distance = car.GetComponent<CarController>().Cameradistance;
                    height = car.GetComponent<CarController>().Cameraheight;
                    rotationDamping = car.GetComponent<CarController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<CarController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<CarController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<CarController>().CameradefaultFOV;
                }
                else if (car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>() != null)
                {
                    distance = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameradistance;
                    height = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameraheight;
                    rotationDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameradefaultFOV;
                }

                float wantedAngle = rotationVector.y;
                float wantedHeight = car.position.y + height;
                float myAngle = transform.eulerAngles.y;
                float myHeight = transform.position.y;

                myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.fixedDeltaTime);
                myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.fixedDeltaTime);

                Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);
                transform.position = car.position;
                transform.position -= currentRotation * Vector3.forward * distance;
                Vector3 temp = transform.position;
                temp.y = myHeight;
                transform.position = temp;
                transform.LookAt(car);

                /*if (car.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername != NetworkClient.localPlayer.gameObject.GetComponent<Player>().username)
                    return;*/

                Vector3 localVelocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().linearVelocity);
                if (localVelocity.z < -0.5f)
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y + 180;
                    rotationVector = temp;
                }
                else
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y;
                    rotationVector = temp;
                }
                float acc = car.GetComponent<Rigidbody>().linearVelocity.magnitude;
                this.GetComponent<Camera>().fieldOfView = defaultFOV + acc * zoomRatio * Time.fixedDeltaTime;
            }
        }

        void Update()
        {
            if (car != null)
            {
                foreach (VehicleEnterExit.VehicleSync.Seat seat in car.GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                {
                    if (seat.DriverSeat == true)
                    {
                        if (seat.Player != null)
                        {
                            if (seat.Player.GetComponent<Player>().username == NetworkClient.localPlayer.gameObject.GetComponent<Player>().username)
                                return;
                        }
                    }
                }

                if (car.GetComponent<CarController>() != null)
                {
                    distance = car.GetComponent<CarController>().Cameradistance;
                    height = car.GetComponent<CarController>().Cameraheight;
                    rotationDamping = car.GetComponent<CarController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<CarController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<CarController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<CarController>().CameradefaultFOV;
                }
                else if (car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>() != null)
                {
                    distance = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameradistance;
                    height = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().Cameraheight;
                    rotationDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerarotationDamping;
                    heightDamping = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameraheightDamping;
                    zoomRatio = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CamerazoomRatio;
                    defaultFOV = car.GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>().CameradefaultFOV;
                }

                float wantedAngle = rotationVector.y;
                float wantedHeight = car.position.y + height;
                float myAngle = transform.eulerAngles.y;
                float myHeight = transform.position.y;

                myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.fixedDeltaTime);
                myHeight = Mathf.Lerp(myHeight, wantedHeight, heightDamping * Time.fixedDeltaTime);

                Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);
                transform.position = car.position;
                transform.position -= currentRotation * Vector3.forward * distance;
                Vector3 temp = transform.position;
                temp.y = myHeight;
                transform.position = temp;
                transform.LookAt(car);

                /*if (car.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername != NetworkClient.localPlayer.gameObject.GetComponent<Player>().username)
                    return;*/

                Vector3 localVelocity = car.InverseTransformDirection(car.GetComponent<Rigidbody>().linearVelocity);
                if (localVelocity.z < -0.5f)
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y + 180;
                    rotationVector = temp;
                }
                else
                {
                    Vector3 _temp = rotationVector;
                    temp.y = car.eulerAngles.y;
                    rotationVector = temp;
                }
                float acc = car.GetComponent<Rigidbody>().linearVelocity.magnitude;
                this.GetComponent<Camera>().fieldOfView = defaultFOV + acc * zoomRatio * Time.fixedDeltaTime;
            }
        }
    }
}