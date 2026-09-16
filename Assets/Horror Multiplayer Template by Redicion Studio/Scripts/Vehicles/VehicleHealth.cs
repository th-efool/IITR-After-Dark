// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class VehicleHealth : NetworkBehaviour
    {
        public const float maxHealth = 100;

        [SyncVar]
        public float currentHealth = maxHealth;
        public GameObject VehicleExplosionPrefab;
        public GameObject VehicleDestroyedPrefab;
        public UnityEngine.UI.Slider healthbar;
        public TMPro.TMP_Text healthText;
        bool isVehicleDestroyed = false;
        [Header("Engine Demage")]
        public GameObject engineDemageLevel1;
        public GameObject engineDemageLevel2;
        public GameObject engineDemageLevel3;

        [SyncVar] bool repaired = false;

        public GameObject hood;

        [Space]
        public float TimeToExplosionDuringBurning = 10f;

        [SyncVar] public float burningTimer = 0;

        [SyncVar] public NetworkIdentity vehicleRepairManager;
        public GameObject vehicleRepairManagerPrefab;
        public Transform vehicleRepairManagerPosition;

        private void Start()
        {
            if (isServer)
            {
                GameObject _vehicleRepairManager = Instantiate(vehicleRepairManagerPrefab, transform.position, transform.rotation) as GameObject;

                NetworkServer.Spawn(_vehicleRepairManager);

                VehicleHealth vehicleHealth = this;

                vehicleHealth.vehicleRepairManager = _vehicleRepairManager.GetComponent<NetworkIdentity>();

                _vehicleRepairManager.GetComponent<VehicleRepairManager>().vehicle = GetComponent<NetworkIdentity>();

                RpcInstantiateVehicleRepairManager(GetComponent<NetworkIdentity>(), _vehicleRepairManager.GetComponent<NetworkIdentity>());
            }
        }

        public void TakeDamage(int amount)
        {
            if (!isServer) return;

            currentHealth -= amount;
            if (currentHealth < 0)
                currentHealth = 0;
            /*if (currentHealth <= 0)
            {
                if(isVehicleDestroyed == false)
                {
                    isVehicleDestroyed = true;
                    foreach (VehicleEnterExit.VehicleSync.Seat seats in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                    {
                        if (seats.Player != null)
                            seats.Player.GetComponent<Health>().TakeDamage(100, AttackType.Exploded, "Explosion wave");
                    }
                    RpcDie(transform.position, transform.rotation);
                }
            }*/
            if (currentHealth <= 45)
            {
                if (isVehicleDestroyed == false)
                {
                    repaired = false;
                    RpcSetRepairedStatus(false);
                }
            }
        }
        [ClientRpc]
        void RpcDie(Vector3 _position, Quaternion _rotation)
        {
            isVehicleDestroyed = true;
            GameObject VehicleExplosion = Instantiate(VehicleExplosionPrefab, _position, _rotation) as GameObject;
            GameObject VehicleDestroyed = Instantiate(VehicleDestroyedPrefab, _position, _rotation) as GameObject;
            gameObject.SetActive(false);
        }

        [ClientRpc]
        void RpcSetRepairedStatus(bool _status)
        {
            repaired = _status;
        }

        private void Update()
        {
            healthbar.value = currentHealth;
            healthText.text = currentHealth.ToString();

            if (isServer)
            {
                if (currentHealth > 100)
                    currentHealth = 100;
            }

            if (currentHealth == 100)
            {
                if (!repaired && isServer)
                    RpcSetRepairedStatus(true);

                if (hood.transform.rotation.z != 0)
                {
                    Quaternion newHoodRotation = new Quaternion(hood.transform.rotation.x, hood.transform.rotation.y, 0, hood.transform.rotation.w);
                }

                engineDemageLevel1.SetActive(false);
                engineDemageLevel2.SetActive(false);
                engineDemageLevel3.SetActive(false);
            }
            else
            {
                engineDemageLevel1.SetActive(false);
                engineDemageLevel2.SetActive(true);
                engineDemageLevel3.SetActive(false);

                if (hood.transform.rotation.z != -48)
                {
                    Quaternion newHoodRotation = new Quaternion(hood.transform.rotation.x, hood.transform.rotation.y, -48, hood.transform.rotation.w);
                }
            }
            /*if (currentHealth > 46 & currentHealth < 65)
            {
                engineDemageLevel1.SetActive(true);
                engineDemageLevel2.SetActive(false);
                engineDemageLevel3.SetActive(false);
            }*/
            /*if (currentHealth > 0 & currentHealth < 45)
            {
                engineDemageLevel1.SetActive(false);
                engineDemageLevel2.SetActive(true);
                engineDemageLevel3.SetActive(false);
                if(hood.transform.rotation.z != -48)
                {
                    Quaternion newHoodRotation = new Quaternion(hood.transform.rotation.x, hood.transform.rotation.y, -48, hood.transform.rotation.w);
                }
            }*/
            /*if (currentHealth > 0 & currentHealth < 25)
            {
                engineDemageLevel1.SetActive(false);
                engineDemageLevel2.SetActive(false);
                engineDemageLevel3.SetActive(true);

                if (isServer || hasAuthority)
                {
                    if (currentHealth <= 0)
                    {
                        burningTimer += 0.1f;
                        if (burningTimer > TimeToExplosionDuringBurning & isVehicleDestroyed == false)
                        {
                            isVehicleDestroyed = true;
                            foreach (VehicleEnterExit.VehicleSync.Seat seats in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                            {
                                if (seats.Player != null)
                                    seats.Player.GetComponent<Health>().TakeDamage(100, AttackType.Exploded, "Explosion wave");
                            }
                            RpcDie(transform.position, transform.rotation);
                        }
                    }
                }
            }*/
            if (Vector3.Dot(transform.up, Vector3.down) > 0)
            {
                if (GetComponent<UnityStandardAssets.Vehicles.Aeroplane.AeroplaneController>() == null)
                {
                    if (isServer)
                    {
                        if (isVehicleDestroyed == false)
                        {
                            isVehicleDestroyed = true;
                            foreach (VehicleEnterExit.VehicleSync.Seat seats in GetComponent<VehicleEnterExit.VehicleSync>()._seats)
                            {
                                if (seats.Player != null)
                                    seats.Player.GetComponent<Health>().TakeDamage(100, AttackType.Exploded, "Explosion wave");
                            }
                            RpcDie(transform.position, transform.rotation);
                        }
                    }
                }
            }

            if (vehicleRepairManager != null)
            {
                vehicleRepairManager.transform.position = vehicleRepairManagerPosition.position;
                vehicleRepairManager.transform.rotation = vehicleRepairManagerPosition.rotation;
            }
        }

        [ClientRpc]
        void RpcInstantiateVehicleRepairManager(NetworkIdentity _vehicle, NetworkIdentity vehicleRepairManagerNetID)
        {
            _vehicle.GetComponent<VehicleHealth>().vehicleRepairManager = vehicleRepairManagerNetID;
            vehicleRepairManagerNetID.GetComponent<VehicleRepairManager>().vehicle = _vehicle;
        }
    }
}