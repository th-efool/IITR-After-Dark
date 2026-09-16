// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class Throwable : GameplayItem
    {
        [Header("Throwable")]
        bool thrown = false;
        public GameObject throwableObjectPrefab;
        public float throwForce = 5f;
        Coroutine c_throw;
        public string throwAnimatorTriggerName = "Throw";
        public float throwAnimationLength = 1f;
        public float updateInterval = 0.1f;

        protected override void Server_Use()
        {
            if (!thrown)
            {
                thrown = true;

                //_myOwner.PlayAnimationTrigger(throwAnimatorTriggerName);
                Rpc_PlayThrowAnimation(_myOwner);
                c_throw = StartCoroutine(thrownCoroutine());
            }

            base.Server_Use();
        }

        public override void Putdown()
        {
            base.Putdown();
            isAiming = false;
        }

        IEnumerator thrownCoroutine()
        {
            yield return new WaitForSeconds(throwAnimationLength - 0.1f);

            GameObject throwableObject = Instantiate(throwableObjectPrefab, transform.position, Quaternion.identity);

            NetworkServer.Spawn(throwableObject);

            throwableObject.transform.position = _myOwner.itemCurrentlyInUse.transform.position;
            //throwableObject.transform.rotation = _myOwner.itemCurrentlyInUse.transform.rotation;

            Rigidbody grenadeRigidbody = throwableObject.GetComponent<Rigidbody>();

            grenadeRigidbody.AddForce(-transform.forward * throwForce, ForceMode.Impulse);

            _myOwner.Server_DetachCurrentItem();

            Rpc_Thrown();
        }

        [ClientRpc]
        void Rpc_Thrown()
        {
            enabled = false;
            foreach (MeshRenderer meshRenderer in _itemMesh)
                meshRenderer.enabled = false;
        }

        [ClientRpc]
        void Rpc_PlayThrowAnimation(CharacterManager characterManager)
        {
            characterManager.GetComponent<Animator>().SetTrigger(throwAnimatorTriggerName);
        }

        private void Start()
        {
            StartCoroutine(c_Update());
        }

        private IEnumerator c_Update()
        {
            while (true)
            {
                M_Update();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void M_Update()
        {
            if (hasAuthority && IsOwned && _myOwner.GetComponent<NetworkIdentity>().netId == NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId)
            {
                if (_input.aim)
                {
                    if (!isAiming)
                    {
                        isAiming = true;
                        CmdSetAimStatus(true);
                    }
                }
                else
                {
                    if (isAiming)
                    {
                        isAiming = false;
                        CmdSetAimStatus(false);
                    }
                }
            }
        }

        [Command]
        private void CmdSetAimStatus(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                isAiming = value;
                if (value)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
                RpcUpdateAimStatus(value);
            }
        }

        [ClientRpc]
        private void RpcUpdateAimStatus(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (value)
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.aimAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonAimAnimatorTriggerName);
                }
                else
                {
                    if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                    else
                        _myOwner.GetComponent<Animator>().SetTrigger(_myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                }
                isAiming = value;
            }
        }
    }
}