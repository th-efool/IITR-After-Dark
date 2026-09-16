// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class FirecrackerManager : NetworkBehaviour
    {
        private bool hasBlockedHunter = false;

        public float expirationTime = 5f;
        private float currentExpirationTime;

        public string animatorTriggerName = "KillerAttacked";
        public string animationName = "KillerAttacked";

        [SyncVar] NetworkIdentity hunter;

        void Start()
        {
            currentExpirationTime = expirationTime;
        }

        private void Update()
        {
            if (!isServer)
                return;

            HandleExpiration();

            if (hasBlockedHunter)
                return;

            CheckForHunter();
        }

        void HandleExpiration()
        {
            currentExpirationTime -= Time.deltaTime;

            if (currentExpirationTime <= 0f)
            {
                if (hunter != null)
                    hunter.GetComponent<HunterAbilities>().isBlocked = false;
                Rpc_Expired();
            }
        }

        void CheckForHunter()
        {
            var colliders = Physics.OverlapSphere(transform.position, 5f, 1 << 6);
            foreach (var collider in colliders)
            {
                var hunterAbilities = collider.GetComponent<HunterAbilities>();
                if (hunterAbilities != null && hunterAbilities._isHunter)
                {
                    hasBlockedHunter = true;
                    hunter = collider.GetComponent<NetworkIdentity>();
                    hunter.GetComponent<HunterAbilities>().isBlocked = true;
                    Rpc_BlockHunter(hunter);
                    break;
                }
            }
        }

        [ClientRpc]
        void Rpc_Expired()
        {
            if (hunter != null)
            {
                hunter.GetComponent<HunterAbilities>().isBlocked = false;
                hunter.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
                if (hunter.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(animationName))
                    hunter.GetComponent<CharacterManager>().PlayAnimationTrigger("StopAttack");
                if (hunter.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
                {
                    if (!hunter.GetComponent<ManageTPController>().isFirstPerson)
                        hunter.GetComponent<Animator>().SetTrigger(hunter.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        hunter.GetComponent<Animator>().SetTrigger(hunter.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
            }

            StartCoroutine(DestroyAfterDelay());
        }

        IEnumerator DestroyAfterDelay()
        {
            yield return null;
            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        void Rpc_BlockHunter(NetworkIdentity _hunter)
        {
            if (_hunter != null)
            {
                hunter = _hunter;
                _hunter.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, true);
                _hunter.GetComponent<CharacterManager>().PlayAnimationTrigger(animatorTriggerName);
                _hunter.GetComponent<HunterAbilities>().isBlocked = true;
            }
        }
    }
}