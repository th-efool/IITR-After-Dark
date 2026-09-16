// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.VehicleEnterExit;
using StarterAssets;
using UnityEngine.AI;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class PlayerAI : NetworkBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent;
        public Transform target;

        private VehicleSync _targetedVehicle;

        public Animator animator;
        [SyncVar] public bool isStopped = true;

        [SyncVar] public bool isSetAsAi = false;

        [SyncVar] public bool HasHandsUp = false;

        [SyncVar] public bool isfearful = false;

        [SyncVar] public bool isfearfulWalking = false;

        bool _travellingToTarget = false;

        private PlayerInteraction _playerInteraction;

        delegate void DestinationReached();
        DestinationReached Event_DestinationReached;


        private void Start()
        {
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
            _playerInteraction = GetComponent<PlayerInteraction>();

            agent.updateRotation = true;
            agent.updatePosition = true;
        }

        public void SetAsBot()
        {
            GetComponent<ThirdPersonController>().enabled = false;
            GetComponent<ManageTPController>().enabled = false;

            animator.SetFloat("MotionSpeed", 1);

            Walk();

            isSetAsAi = true;
            RpcSetAsBot(true);

            GetComponent<NetworkTransform>().clientAuthority = false;
        }

        [ClientRpc]
        void RpcSetAsBot(bool status)
        {
            isSetAsAi = status;
        }

        private void Update()
        {
            if (isSetAsAi)
            {
                var colliders = Physics.OverlapSphere(transform.position, 5f, 1 << 6);
                foreach (var collider in colliders)
                {
                    if (collider != null & collider.tag == "Player" && collider.GetComponent<Player>() != null)
                    {
                        if (!GetComponent<PlayerInteraction>().inVehicle & !isfearfulWalking & !isfearful & !HasHandsUp)
                        {
                            if (collider.GetComponent<ManageTPController>().aimValue == 1 & !isfearfulWalking)
                            {
                                isfearfulWalking = true;
                                StartCoroutine(EndFearfulWalking());
                                FearfulWalk();
                            }
                        }

                        if (collider.GetComponent<ManageTPController>().aimValue == 1 & !isfearful)
                        {
                            float dist = Vector3.Distance(transform.position, collider.transform.position);
                            if (dist < 5)
                            {
                                if (!HasHandsUp)
                                {
                                    HasHandsUp = true;
                                    if (!GetComponent<PlayerInteraction>().inVehicle)
                                        isfearful = true;
                                    StartCoroutine(EndHandsUpCoroutine());
                                    GetComponent<Animator>().SetLayerWeight(2, 1);
                                    HandsUp();
                                }
                                if (!GetComponent<PlayerInteraction>().inVehicle)
                                {
                                    var towardsPlayer = collider.transform.position - transform.position;

                                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(towardsPlayer), Time.deltaTime * 1);

                                    transform.position += transform.forward * 1 * Time.deltaTime;
                                }
                            }
                        }
                    }
                }
                var ProjectileColliders = Physics.OverlapSphere(transform.position, 20f, 1 << 8);
                foreach (var pCollider in ProjectileColliders)
                {
                    if (pCollider.tag == "Bullet")
                    {
                        if (pCollider.GetComponent<NetworkBullet>() != null)
                        {
                            if (!HasHandsUp & !isfearful)
                            {
                                if (!GetComponent<PlayerInteraction>().inVehicle)
                                {
                                    isfearful = true;
                                    StartCoroutine(EndFearfulness());
                                    GetComponent<Animator>().SetLayerWeight(2, 1);
                                    Run();
                                }
                                else
                                {
                                    isfearful = true;
                                    StartCoroutine(EndFearfulness());
                                    Event_DestinationReached += OnReachedDefaultTarget;
                                    _targetedVehicle.RequestExiting(_playerInteraction, false);
                                    SetNavmeshTarget(GameObject.FindGameObjectWithTag("BOTWAYPOINT").transform);
                                    GetComponent<Animator>().SetLayerWeight(2, 0);
                                    RunOutOfVehicle();

                                    /*int mode = Random.Range(134, 523);
                                    if(mode > 250)
                                    {
                                        isfearful = true;
                                        StartCoroutine(EndFastDriving());
                                        _targetedVehicle.GetComponent<CarAI>().desiredSpeed = 100f;
                                        _targetedVehicle.GetComponent<CarAI>().ignoreObstacles = true;
                                    }
                                    else if(mode < 250)
                                    {
                                        isfearful = true;
                                        StartCoroutine(EndFearfulness());
                                        animator.SetTrigger("FearfulRunning");
                                        GetComponent<Animator>().SetLayerWeight(2, 1);
                                        Event_DestinationReached += OnReachedDefaultTarget;
                                        _targetedVehicle.RequestExiting(_playerInteraction, false);
                                        SetNavmeshTarget(GameObject.FindGameObjectWithTag("BOTWAYPOINT").transform);
                                        Run();
                                    }
                                    mode = 0;*/
                                }
                            }
                        }
                    }
                }
            }

            if (!HasHandsUp & !isStopped & target != null)
                agent.SetDestination(target.position);

            if (!HasHandsUp & _travellingToTarget)
            {
                if (Vector3.Distance(transform.position, target.position) < 0.3f)
                {
                    _travellingToTarget = false;
                    Event_DestinationReached?.Invoke();
                }
            }

            if (HasHandsUp && GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("FearfulRunning"))
            {
                GetComponent<Animator>().SetLayerWeight(2, 1);
                if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("HandsUp"))
                    GetComponent<Animator>().Play("HandsUp");
            }
            if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("FearfulRunning") && !GetComponent<Animator>().GetBool("Run"))
            {
                Run();
            }

            if (isSetAsAi && GetComponent<Health>().isDeath)
            {
                animator.ResetTrigger("FearfulRunning");
                animator.ResetTrigger("FearfulWalk");
                animator.ResetTrigger("Walk");
                animator.ResetTrigger("Run");
                animator.SetTrigger("Idle");
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                Stop();
                GetComponent<Animator>().SetLayerWeight(2, 0);
            }
        }

        IEnumerator EndFastDriving()
        {
            yield return new WaitForSeconds(20f);

            _targetedVehicle.GetComponent<CarAI>().ignoreObstacles = false;
            isfearful = false;
            _targetedVehicle.GetComponent<CarAI>().desiredSpeed = 12f;
            StopCoroutine(EndFastDriving());
        }

        IEnumerator EndHandsUpCoroutine()
        {
            yield return new WaitForSeconds(3.14f);

            HasHandsUp = false;
            EndHandsUp();
            StopCoroutine(EndHandsUpCoroutine());
        }

        IEnumerator EndFearfulness()
        {
            yield return new WaitForSeconds(30);

            isfearful = false;
            StopCoroutine(EndFearfulness());
        }

        IEnumerator EndFearfulWalking()
        {
            yield return new WaitForSeconds(5);

            isfearfulWalking = false;
            StopCoroutine(EndFearfulWalking());
        }

        public void HandsUp()
        {
            animator.ResetTrigger("FearfulRunning");
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("Walk");
            animator.ResetTrigger("Run");
            animator.SetTrigger("Idle");
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            Stop();
            GetComponent<Animator>().SetLayerWeight(2, 1);
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("HandsUp"))
                GetComponent<Animator>().Play("HandsUp");
        }

        void EndHandsUp()
        {
            GetComponent<Animator>().ResetTrigger("HandsUp");
            if (!GetComponent<PlayerInteraction>().inVehicle)
            {
                GetComponent<Animator>().Play("FearfulRunning");
                StartCoroutine(EndFearfulness());
                Run();
            }
            else
            {
                isfearful = true;
                StartCoroutine(EndFearfulness());
                Event_DestinationReached += OnReachedDefaultTarget;
                _targetedVehicle.RequestExiting(_playerInteraction, false);
                SetNavmeshTarget(GameObject.FindGameObjectWithTag("BOTWAYPOINT").transform);
                GetComponent<Animator>().SetLayerWeight(2, 0);
                RunOutOfVehicle();

                /*int mode = Random.Range(134, 523);
                if (mode > 250)
                {
                    isfearful = true;
                    StartCoroutine(EndFearfulness());
                    animator.SetTrigger("FearfulRunning");
                    GetComponent<Animator>().SetLayerWeight(2, 1);
                    Event_DestinationReached += OnReachedDefaultTarget;
                    _targetedVehicle.RequestExiting(_playerInteraction, false);
                    SetNavmeshTarget(GameObject.FindGameObjectWithTag("BOTWAYPOINT").transform);
                    Run();
                }
                else if (mode < 250)
                {
                    isfearful = true;
                    StartCoroutine(EndFastDriving());
                    _targetedVehicle.GetComponent<CarAI>().desiredSpeed = 100f;
                    _targetedVehicle.GetComponent<CarAI>().ignoreObstacles = true;
                }
                mode = 0;*/
            }
        }

        public void Stop()
        {
            isStopped = true;
            animator.SetTrigger("Idle");
            animator.ResetTrigger("Walk");
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("Run");

            animator.SetFloat("Speed", 0);
            animator.SetBool("Grounded", true);
        }

        public void Run()
        {
            isStopped = false;
            if (agent.isStopped == true)
                agent.isStopped = false;
            animator.SetTrigger("Idle");
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("Walk");
            animator.SetTrigger("Run");
            GetComponent<Animator>().SetLayerWeight(2, 1);
            GetComponent<Animator>().Play("FearfulRunning");

            animator.SetFloat("Speed", 6);
            animator.SetBool("Grounded", true);

            agent.speed = 6;
        }

        public void RunOutOfVehicle()
        {
            isStopped = false;
            if (agent.isStopped == true)
                agent.isStopped = false;
            animator.SetTrigger("Idle");
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("Walk");
            animator.SetTrigger("Run");
            StartCoroutine(RunOutOfVehicleCoroutine());

            animator.SetFloat("Speed", 6);
            animator.SetBool("Grounded", true);

            agent.speed = 6;
        }

        IEnumerator RunOutOfVehicleCoroutine()
        {
            yield return new WaitForSeconds(3f);

            GetComponent<Animator>().SetLayerWeight(2, 1);
            GetComponent<Animator>().Play("FearfulRunning");
            StopCoroutine(RunOutOfVehicleCoroutine());
        }

        public void RunToCar()
        {
            isStopped = false;
            if (agent.isStopped == true)
                agent.isStopped = false;
            animator.SetTrigger("Idle");
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("Walk");
            animator.SetTrigger("Run");
            GetComponent<Animator>().SetLayerWeight(2, 0);
            GetComponent<Animator>().ResetTrigger("FearfulRunning");

            animator.SetFloat("Speed", 6);
            animator.SetBool("Grounded", true);

            agent.speed = 6;
        }

        public void Walk()
        {
            isStopped = false;
            if (agent.isStopped == true)
                agent.isStopped = false;
            animator.ResetTrigger("FearfulWalk");
            animator.ResetTrigger("FearfulRunning");
            GetComponent<Animator>().SetLayerWeight(2, 0);
            animator.SetTrigger("Idle");
            animator.ResetTrigger("Run");
            animator.SetTrigger("Walk");

            animator.SetFloat("Speed", 2);
            animator.SetBool("Grounded", true);

            agent.speed = 2;
        }

        public void FearfulWalk()
        {
            isStopped = false;
            if (agent.isStopped == true)
                agent.isStopped = false;
            animator.ResetTrigger("FearfulRunning");
            GetComponent<Animator>().SetLayerWeight(2, 1);
            animator.SetTrigger("Idle");
            animator.ResetTrigger("Run");
            animator.SetTrigger("Walk");
            animator.SetTrigger("FearfulWalk");

            animator.SetFloat("Speed", 2);
            animator.SetBool("Grounded", true);

            agent.speed = 2;
        }

        public void SetWaypoint(Transform waypoint, string movement)
        {
            SetNavmeshTarget(waypoint);
            if (movement == "Running")
                Run();
            else if (movement == "Walking" & !isfearful & !isfearfulWalking)
                Walk();
        }

        #region behaviours
        public void GetInTheVehicle(VehicleSync vehicle)
        {
            SetNavmeshTarget(vehicle._seats[0].EnterPoint);

            _targetedVehicle = vehicle;

            Event_DestinationReached += OnReachedDesiredVehicle;

            RunToCar();
        }

        #endregion
        void OnReachedDesiredVehicle()
        {
            Event_DestinationReached -= OnReachedDesiredVehicle;
            _targetedVehicle.RequestEntering(0, _playerInteraction, false, false);
        }


        //what happens when ai got kicked out of car
        public void ServerEvent_GotKickedOutOfCar()
        {
            Event_DestinationReached += OnReachedDefaultTarget;
            SetNavmeshTarget(GameObject.FindGameObjectWithTag("BOTWAYPOINT").transform);
            RunOutOfVehicle();
        }
        void OnReachedDefaultTarget()
        {
            Event_DestinationReached -= OnReachedDefaultTarget;
            Stop();
        }

        void SetNavmeshTarget(Transform destination)
        {
            agent.SetDestination(destination.position);
            target = destination;
            _travellingToTarget = true;
        }
    }
}