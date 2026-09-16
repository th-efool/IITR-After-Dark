// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using StarterAssets;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Health : NetworkBehaviour
    {
        public string username;

        public const int maxHealth = 100;
        [SyncVar(hook = nameof(OnChangeHealth))] public int currentHealth = maxHealth;
        public RectTransform healthbar;

        [SerializeField] public Transform _HealthCanvas;
        private static Transform _camera;
        public static Health localPlayer;

        [SerializeField] private string deathAnimationName;

        private float _respawnTimer;

        //public string attackerUsername; //sync var is to slow to update killer name on time so we will update this by rpc

        [Space]
        [SyncVar] public bool isDeath = false;

        [Space]
        [Header("UI")]
        public GameObject killMessagePrefab;
        public GameObject deathCanvasPrefab;

        public Transform playerRagdoll;
        private bool isFallingFromCar = false;
        [HideInInspector] public bool isFallingFromAircraft = false;
        [SyncVar] public bool waitingForFallDamage = false;

        [Header("Parachute")]
        //public GameObject parachute;
        //public GameObject parachuteReleasedPrefab;

        [Header("Drop Money")]
        public GameObject droppedMoneyPrefab;

        GameObject[] playerSpawnPoints;

        private void Start()
        {
            username = this.GetComponent<Player>().username;

            playerSpawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer(); // ?

            localPlayer = this;

            //_camera = FindObjectOfType<Camera>().transform;
            _camera = GameObject.Find("MainCamera").transform;

            if (isLocalPlayer)
                _HealthCanvas.gameObject.SetActive(false);
        }

        public void TakeDamage(int amount, AttackType attackType, string attackerName)
        {
            if (!isServer)
            {
                return;
            }

            /*if (isDeath)
                return;*/

            /*currentHealth -= amount;
            print(gameObject.name + "'s" + " health = " + currentHealth);
            if (GetComponent<PlayerAI>().isSetAsAi == true)
            {
                GetComponent<PlayerAI>().Run();
            }
            if (currentHealth <= 0)
            {
                print("Player: " + gameObject.name + " is dead");
                isDeath = true;

                currentHealth = maxHealth;
                _respawnTimer = 7.30f;

                //RpcPlayAnimation(deathAnimationName);
                RpcFallingDown();
                if (GetComponent<PlayerAI>().isSetAsAi == true)
                {
                    GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
                    healthbar.parent.gameObject.SetActive(false);
                    GetComponent<CapsuleCollider>().isTrigger = true;
                    RpcNPCDie();
                }
                else
                {
                    RpcShowDeathScreen(attackerName);

                    RpcDeathConfirmation(attackerName, attackType);
                    isDeath = false;
                    StartCoroutine(Respawn());
                }
                Vector3 droppedMoneyPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 2f);
                Quaternion droppedMoneyRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);

                GameObject droppedMoney = Instantiate(droppedMoneyPrefab, droppedMoneyPosition, droppedMoneyRotation) as GameObject;

                NetworkServer.Spawn(droppedMoney);
            }*/
        }

        [ClientRpc]
        void RpcDeathConfirmation(string killerName, AttackType attackType)
        {
            Instantiate(killMessagePrefab).GetComponent<KillMessage>().ShowKillMessage(killerName, username, attackType);
        }

        [ClientRpc]
        void RpcShowDeathScreen(string attackerName)
        {
            isDeath = true;
            if (isLocalPlayer)
            {
                Instantiate(deathCanvasPrefab).GetComponent<DeathScreen>().SetUpDeathScreen(this.transform.position, attackerName);
            }
        }

        IEnumerator Respawn()
        {
            _respawnTimer -= 1;

            yield return new WaitForSeconds(7.30f);


            RpcRespawn();
        }

        void OnChangeHealth(int currenthealth, int health)
        {
            if (healthbar != null)
                healthbar.sizeDelta = new Vector2(health * 5, healthbar.sizeDelta.y);
            currentHealth = health;
        }

        [ClientRpc]
        void RpcRespawn()
        {
            if (isLocalPlayer)
            {
                StartCoroutine(WaitUntilSuitableSpawnPointFound());

                IEnumerator WaitUntilSuitableSpawnPointFound()
                {
                    int index = Random.Range(0, playerSpawnPoints.Length);
                    GameObject currentSpawnPoint = playerSpawnPoints[index];

                    yield return new WaitUntil(() => currentSpawnPoint.GetComponent<PlayerSpawnPoint>().isPlayerInTrigger == false);

                    transform.position = currentSpawnPoint.transform.position;
                    transform.rotation = new Quaternion(0, 0, 0, 0);

                    StopCoroutine(WaitUntilSuitableSpawnPointFound());
                }
            }

            //this.GetComponent<Animator>().Rebind(); this messes up animations
            isDeath = false;
            if (GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar)
                GetComponent<PlayerInteraction>().ForceExitVehicle();
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<Animator>().enabled = true;
            GetComponent<Animator>().Play("Idle Walk Run Blend");
        }

        private void Update()
        {
            if (localPlayer != null && !isLocalPlayer)
            {
                _HealthCanvas.LookAt(_HealthCanvas.position + _camera.rotation * Vector3.forward,
                    _camera.rotation * Vector3.up);
            }

            if (isFallingFromCar)
            {
                playerRagdoll.position = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            }
            else if (isFallingFromAircraft)
            {
                //parachute.SetActive(true);
            }
            if (isFallingFromAircraft == false)
            {
                //parachute.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (localPlayer == this)
            {
                localPlayer = null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (waitingForFallDamage)
            {
                if (collision != null)
                {
                    if (isServer)
                    {
                        waitingForFallDamage = false;
                        TakeDamage(100, AttackType.Plane, "World");
                    }
                }
            }
        }

        [ClientRpc]
        public void RpcPlayAnimation(string Animation)
        {
            this.GetComponent<Animator>().Play(Animation);
        }

        [ClientRpc]
        public void RpcFallingDown()
        {
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<Animator>().enabled = false;
        }

        [ClientRpc]
        public void RpcNPCDie()
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
            healthbar.parent.gameObject.SetActive(false);
            GetComponent<CapsuleCollider>().isTrigger = true;
        }

        public void FallFromVehicle(int vehicleType)
        {
            CmdFallFromVehicle(vehicleType);
        }

        [Command]
        public void CmdFallFromVehicle(int vehicleType)
        {
            GetComponent<CharacterController>().enabled = false;
            GetComponent<ThirdPersonController>().enabled = false;
            if (vehicleType == 1)
            {
                isFallingFromCar = true;
                GetComponent<Animator>().SetTrigger("FallFromVehicle");
            }
            else if (vehicleType == 2)
            {
                isFallingFromCar = true;
                GetComponent<Animator>().SetBool("FreeFall", true);
                GetComponent<Animator>().Play("InAir");
                waitingForFallDamage = true;
            }
            RpcFallFromVehicle(vehicleType);
        }

        [ClientRpc]
        public void RpcFallFromVehicle(int vehicleType)// vehicleType, 1 = Car, 2 = Aircraft
        {
            GetComponent<CharacterController>().enabled = false;
            GetComponent<ThirdPersonController>().enabled = false;
            if (vehicleType == 1)//Car
            {
                isFallingFromCar = true;
                GetComponent<Animator>().SetTrigger("FallFromVehicle");

                StartCoroutine(StandUpCar());
            }
            else if (vehicleType == 2)//Aircraft
            {
                isFallingFromCar = true;
                GetComponent<Animator>().SetBool("FreeFall", true);
                GetComponent<Animator>().Play("InAir");
                waitingForFallDamage = true;

                StartCoroutine(StandUpPlane());
            }
        }

        IEnumerator StandUpCar()
        {
            yield return new WaitForSeconds(5.20f);

            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<Animator>().enabled = true;
            isFallingFromCar = false;
            GetComponent<Animator>().ResetTrigger("FallFromVehicle");
            //reanable player movement
            GetComponent<CharacterController>().enabled = true;
            GetComponent<ThirdPersonController>().enabled = true;
            GetComponent<CapsuleCollider>().enabled = true;

            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

            GetComponent<PlayerInteraction>().inVehicle = false;
        }

        IEnumerator StandUpPlane()
        {
            yield return new WaitForSeconds(0.17f);

            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<Animator>().enabled = true;
            isFallingFromCar = false;
            GetComponent<Animator>().SetBool("FreeFall", false);
            GetComponent<Animator>().Play("Idle Walk Run Blend");
            //reanable player movement
            GetComponent<CharacterController>().enabled = true;
            GetComponent<ThirdPersonController>().enabled = true;
            GetComponent<CapsuleCollider>().enabled = true;

            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar = false;

            GetComponent<PlayerInteraction>().inVehicle = false;
        }

        public void ReleaseParachute()
        {
            isFallingFromAircraft = false;
            GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().usesParachute = false;
            CmdReleaseParachute(transform.position, transform.rotation);
        }

        [Command]
        void CmdReleaseParachute(Vector3 _position, Quaternion _rotation)
        {
            isFallingFromAircraft = false;
            /*GameObject ReleasedParachute = Instantiate(parachuteReleasedPrefab, _position, _rotation) as GameObject;
            NetworkServer.Spawn(ReleasedParachute, connectionToClient);*/

            RpcReleaseParachute();
        }

        [ClientRpc]
        void RpcReleaseParachute()
        {
            isFallingFromAircraft = false;
            //reanable player movement
            //parachute.SetActive(false);
            GetComponent<Animator>().Play("Idle Walk Run Blend");
        }
    }
    public enum AttackType : byte
    {
        Minigun,
        Rockets,
        Exploded,
        Gun,
        Car,
        Plane,
    }
}