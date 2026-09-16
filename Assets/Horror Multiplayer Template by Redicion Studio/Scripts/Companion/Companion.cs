// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

namespace RedicionStudio
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    public class Companion : NetworkBehaviour
    {
        public GameObject owner;
        public string ownerName;
        public TMPro.TMP_Text companionText;
        public Transform companionTextCanvas;
        [Space]
        public UnityEngine.AI.NavMeshAgent agent;

        public Animator animator;
        [SyncVar] public bool isStopped = true;

        private static Transform _camera;

        [Space]
        [Header("Animations")]
        public string idleAnimationName = "Idle";
        public string walkAnimationName = "Walk";
        public string runAnimationName = "Run";

        public void Start()
        {
            agent = GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();

            agent.updateRotation = true;
            agent.updatePosition = true;

            Walk();

            GetComponent<NetworkTransform>().clientAuthority = false;
        }

        private void Update()
        {
            if (owner == null)
                return;

            if (companionText != null)
                companionText.text = ownerName + "'s" + " companion";

            if (owner.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().slots[3].item.itemSO == null)
            {
                if (isServer)
                {
                    RpcDestroyCompanion(netIdentity);
                }
            }

            if ((owner.transform.position - transform.position).sqrMagnitude < 2 * 2)
            {
                Stop();
            }
            else
            {
                Walk();
            }

            if (!isStopped)
                agent.SetDestination(owner.transform.position);

            if (_camera == null)
                _camera = GameObject.Find("MainCamera").transform;

            if (_camera != null & companionTextCanvas != null)
                companionTextCanvas.LookAt(companionTextCanvas.position + _camera.rotation * Vector3.forward, _camera.rotation * Vector3.up);
        }

        [ClientRpc]
        void RpcDestroyCompanion(NetworkIdentity companionNetID)
        {
            NetworkServer.Destroy(companionNetID.gameObject);
        }

        public void Stop()
        {
            isStopped = true;
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(idleAnimationName))
            {
                animator.Play(idleAnimationName);
            }
            agent.speed = 0;
        }

        public void Run()
        {
            isStopped = false;
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(runAnimationName))
            {
                animator.Play(runAnimationName);
            }

            agent.speed = 6;
        }

        public void Walk()
        {
            isStopped = false;
            if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(walkAnimationName))
            {
                animator.Play(walkAnimationName);
            }

            agent.speed = 4f;
        }
    }
}