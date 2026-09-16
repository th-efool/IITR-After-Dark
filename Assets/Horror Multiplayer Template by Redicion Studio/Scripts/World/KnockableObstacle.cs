// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class KnockableObstacle : NetworkBehaviour
    {
        public GameObject[] knockableObstacleMesh;
        public Transform startPositionFront;
        public Transform hunterPositionFront;
        public Transform startPositionBack;
        public Transform hunterPositionBack;
        public string KnockableObstacleKnockingFrontTriggerName = "KnockingFront";
        public string KnockableObstacleKnockingBackTriggerName = "KnockingBack";
        public Animator animator;
        [HideInInspector] public bool isKnocked = false;
        [HideInInspector] bool isDestroyed = false;
        [HideInInspector] public bool isKnocking = false;

        [SyncVar] public bool reinstated = false;

        private string killerBlockedAnimatorTriggerName = "KillerAttacked";
        private string killerBlockedAnimationName = "KillerAttacked";

        private float killerBlockedExpirationTime = 5f;
        private float killerBlockedCurrentExpirationTime;

        [SyncVar] NetworkIdentity hunter;

        private bool canBlockHunter = true;

        public RoomManager roomManager;

        public GameObject destructibleKnockableObstaclePrefab;
        public Transform destructibleKnockableObstaclePosition;
        public GameObject instantiatedDestructibleKnockableObstaclePrefab;

        public ClimbableObstacle[] associatedClimbableObstacles;

        public GameObject[] additionalColliders;

        private void Update()
        {
            if (roomManager == null && GameObject.FindGameObjectWithTag("RoomManager") != null)
                roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();

            /*if (isServer && roomManager != null && reinstated && roomManager.MatchEnding)
            {
                reinstated = false;
            }
            else if (isServer && roomManager != null && !reinstated && roomManager.MatchRunning && !roomManager.MatchEnding)
            {
                reinstated = true;
                ServerRestoreKnockableObstacle();
            }*/
        }

        public void DestroyKnockableObstacle()
        {
            if (isServer)
            {
                isDestroyed = true;

                GameObject _destructibleKnockableObstaclePrefab = Instantiate(destructibleKnockableObstaclePrefab, destructibleKnockableObstaclePosition.position, destructibleKnockableObstaclePosition.rotation);

                NetworkServer.Spawn(_destructibleKnockableObstaclePrefab);

                instantiatedDestructibleKnockableObstaclePrefab = _destructibleKnockableObstaclePrefab;

                foreach (GameObject ObstacleMesh in knockableObstacleMesh)
                {
                    ObstacleMesh.SetActive(false);
                }

                foreach (GameObject additionalCollider in additionalColliders)
                {
                    additionalCollider.SetActive(false);
                }

                RpcDestroyKnockableObstacle();
            }
        }

        [ClientRpc]
        void RpcDestroyKnockableObstacle()
        {
            foreach (GameObject ObstacleMesh in knockableObstacleMesh)
            {
                ObstacleMesh.SetActive(false);
            }

            foreach (GameObject additionalCollider in additionalColliders)
            {
                additionalCollider.SetActive(false);
            }
        }

        [ClientRpc]
        void RpcSetPlayerCurrentKnockableObstacle(NetworkIdentity player, bool remove)
        {
            if (!remove)
                player.GetComponent<PlayerInteractionModule>().currentKnockableObstacle = GetComponent<NetworkIdentity>();
            else
                player.GetComponent<PlayerInteractionModule>().currentKnockableObstacle = null;
        }

        private void OnTriggerStay(Collider other)
        {
            if (isServer && !isDestroyed && !isKnocked && isKnocking && other.GetComponent<PlayerInteractionModule>() != null && other.GetComponent<HunterAbilities>()._isHunter)
            {
                if (canBlockHunter)
                {
                    canBlockHunter = false;
                    float distanceToFront = Vector3.Distance(other.transform.position, hunterPositionFront.position);
                    float distanceToBack = Vector3.Distance(other.transform.position, hunterPositionBack.position);

                    if (distanceToFront < distanceToBack)
                    {
                        other.transform.position = hunterPositionFront.position;
                        other.transform.rotation = hunterPositionFront.rotation;
                    }
                    else
                    {
                        other.transform.position = hunterPositionBack.position;
                        other.transform.rotation = hunterPositionBack.rotation;
                    }
                    other.GetComponent<HunterAbilities>().isBlocked = true;
                    killerBlockedCurrentExpirationTime = killerBlockedExpirationTime;
                    StartCoroutine(ServerHandleExpirationCoroutine(other.GetComponent<NetworkIdentity>()));
                    Rpc_BlockHunter(other.GetComponent<NetworkIdentity>());
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (isServer && !isDestroyed && !isKnocked && !isKnocking && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle == null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentKnockableObstacle(other.GetComponent<NetworkIdentity>(), false);
                }
                else if (other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle.netId != GetComponent<NetworkIdentity>().netId)
                {
                    other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentKnockableObstacle(other.GetComponent<NetworkIdentity>(), false);
                }
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle != null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentKnockableObstacle = null;
                    RpcSetPlayerCurrentKnockableObstacle(other.GetComponent<NetworkIdentity>(), true);
                }
            }
        }

        private IEnumerator HandleExpirationCoroutine()
        {
            while (killerBlockedCurrentExpirationTime > 0f)
            {
                killerBlockedCurrentExpirationTime -= Time.deltaTime;
                yield return null;
            }

            if (hunter != null)
            {
                hunter.GetComponent<HunterAbilities>().isBlocked = false;
            }

            Expired();
        }

        private IEnumerator ServerHandleExpirationCoroutine(NetworkIdentity _hunter)
        {
            while (killerBlockedCurrentExpirationTime > 0f)
            {
                killerBlockedCurrentExpirationTime -= Time.deltaTime;
                yield return null;
            }

            if (_hunter != null)
            {
                _hunter.GetComponent<HunterAbilities>().isBlocked = false;
            }
        }

        void Expired()
        {
            if (hunter != null)
            {
                hunter.GetComponent<HunterAbilities>().isBlocked = false;
                hunter.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
                if (hunter.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(killerBlockedAnimationName))
                    hunter.GetComponent<CharacterManager>().PlayAnimationTrigger("StopAttack");
                if (hunter.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    if (!hunter.GetComponent<ManageTPController>().isFirstPerson)
                        hunter.GetComponent<Animator>().SetTrigger(hunter.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        hunter.GetComponent<Animator>().SetTrigger(hunter.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }
        }

        [ClientRpc]
        void Rpc_BlockHunter(NetworkIdentity _hunter)
        {
            if (_hunter != null)
            {
                canBlockHunter = false;
                float distanceToFront = Vector3.Distance(_hunter.transform.position, hunterPositionFront.position);
                float distanceToBack = Vector3.Distance(_hunter.transform.position, hunterPositionBack.position);

                if (distanceToFront < distanceToBack)
                {
                    _hunter.transform.position = hunterPositionFront.position;
                    _hunter.transform.rotation = hunterPositionFront.rotation;
                }
                else
                {
                    _hunter.transform.position = hunterPositionBack.position;
                    _hunter.transform.rotation = hunterPositionBack.rotation;
                }
                hunter = _hunter;
                _hunter.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, true);
                _hunter.GetComponent<CharacterManager>().PlayAnimationTrigger(killerBlockedAnimatorTriggerName);
                _hunter.GetComponent<HunterAbilities>().isBlocked = true;
                killerBlockedCurrentExpirationTime = killerBlockedExpirationTime;
                StartCoroutine(HandleExpirationCoroutine());
            }
        }

        public void SetKnocked()
        {
            foreach (GameObject additionalCollider in additionalColliders)
            {
                additionalCollider.SetActive(true);
            }

            foreach (ClimbableObstacle climbableObstacle in associatedClimbableObstacles)
            {
                climbableObstacle.canBeClimbedOver = true;
            }
        }

        public void ServerRestoreKnockableObstacle()
        {
            if (isServer)
            {
                isDestroyed = false;
                isKnocked = false;
                isKnocking = false;
                canBlockHunter = true;

                foreach (GameObject additionalCollider in additionalColliders)
                {
                    additionalCollider.SetActive(false);
                }

                if (instantiatedDestructibleKnockableObstaclePrefab != null)
                {
                    NetworkServer.Destroy(instantiatedDestructibleKnockableObstaclePrefab);
                }

                foreach (ClimbableObstacle climbableObstacle in associatedClimbableObstacles)
                {
                    climbableObstacle.canBeClimbedOver = false;
                }

                animator.Rebind();

                RpcRestoreKnockableObstacle();
            }
        }

        [ClientRpc]
        public void RpcRestoreKnockableObstacle()
        {
            isDestroyed = false;
            isKnocked = false;
            isKnocking = false;
            canBlockHunter = true;

            foreach (GameObject additionalCollider in additionalColliders)
            {
                additionalCollider.SetActive(false);
            }

            foreach (ClimbableObstacle climbableObstacle in associatedClimbableObstacles)
            {
                climbableObstacle.canBeClimbedOver = false;
            }

            animator.Rebind();

            foreach (GameObject ObstacleMesh in knockableObstacleMesh)
            {
                ObstacleMesh.SetActive(true);
            }
        }
    }
}