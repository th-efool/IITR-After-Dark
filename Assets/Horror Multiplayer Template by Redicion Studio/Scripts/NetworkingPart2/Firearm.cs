// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class Firearm : GameplayItem
    {
        public string reloadAnimatorTriggerName;

        public float updateInterval = 0.1f;

        public GameObject bulletPrefab;

        public float bulletSpeed;

        public Transform _bulletSpawnPointPosition;

        public GameObject cartridgeEjectPrefab;

        public Transform _cartridgeEjectSpawnPointPosition;

        public int availableAmmunition = 2;

        Coroutine c_reload;
        public float reloadDuration = 1f;
        bool isReloading = false;

        public override void Putdown()
        {
            isAiming = false;
            base.Putdown();
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
            if (hasAuthority && IsOwned && _myOwner.GetComponent<NetworkIdentity>().netId == NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId && !_myOwner.GetComponent<HunterAbilities>()._inFight)
            {
                if (_input != null && _input.aim)
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
                if (_input != null && !isReloading && isAiming && _input.use && availableAmmunition != 0)
                {
                    CmdReload(true);
                    ShootBullet();
                    c_reload = StartCoroutine(Reload());
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

        private void ShootBullet()
        {
            if (!base.hasAuthority) return;

            //Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)); //removed
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            int layerToIgnore1 = 4;
            int layerToIgnore2 = 1;
            LayerMask layerMask = ~(1 << layerToIgnore1 | 1 << layerToIgnore2);
            RaycastHit hit;
            Vector3 collisionPoint;
            if (Physics.Raycast(ray, out hit, 50f, layerMask))
            {
                collisionPoint = hit.point;
            }
            else
            {
                collisionPoint = ray.GetPoint(50f);
            }

            Vector3 bulletVector = (collisionPoint - _bulletSpawnPointPosition.transform.position).normalized;

            CmdShootBullet(_bulletSpawnPointPosition.position, _bulletSpawnPointPosition.rotation, _cartridgeEjectSpawnPointPosition.position, _cartridgeEjectSpawnPointPosition.rotation, bulletVector, bulletSpeed);
        }

        [Command]
        private void CmdShootBullet(Vector3 _position, Quaternion _rotation, Vector3 _cartridgeEjectPosition, Quaternion _cartridgeEjectRotation, Vector3 _bulletVector, float _bulletSpeed)
        {
            GameObject Bullet = Instantiate(bulletPrefab, _position, _rotation) as GameObject;

            Bullet.GetComponent<Rigidbody>().linearVelocity = _bulletVector * _bulletSpeed;

            NetworkServer.Spawn(Bullet);

            NetworkBullet bullet = Bullet.GetComponent<NetworkBullet>();
            bullet.netIdentity.AssignClientAuthority(this.connectionToClient);

            bullet.SetupProjectile_ServerSide();

            RpcBulletFired(bullet, _bulletVector, _bulletSpeed);

            GameObject _cartridgeEject = Instantiate(cartridgeEjectPrefab, _cartridgeEjectPosition, _cartridgeEjectRotation) as GameObject;

            NetworkServer.Spawn(_cartridgeEject, connectionToClient);
        }

        [ClientRpc]
        private void RpcBulletFired(NetworkBullet Bullet, Vector3 _bulletVector, float _bulletSpeed)
        {
            Bullet.GetComponent<Rigidbody>().linearVelocity = _bulletVector * _bulletSpeed;
            Bullet.GetComponent<NetworkBullet>().SetupProjectile(_myOwner.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().currentPlayerUsername(), hasAuthority);
            availableAmmunition -= 1;
        }

        IEnumerator Reload()
        {
            yield return new WaitForSeconds(reloadDuration - 0.1f);
            CmdReload(false);
        }

        [Command]
        private void CmdReload(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                isReloading = value;
                if (value)
                    _myOwner.GetComponent<Animator>().SetTrigger(reloadAnimatorTriggerName);
                else
                {
                    if (isAiming)
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
                }
                RpcReload(value);
            }
        }

        [ClientRpc]
        private void RpcReload(bool value)
        {
            if (IsOwned && _myOwner != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (value)
                    _myOwner.GetComponent<Animator>().SetTrigger(reloadAnimatorTriggerName);
                else
                {
                    if (isAiming)
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
                }
                isReloading = value;
            }
        }
    }
}