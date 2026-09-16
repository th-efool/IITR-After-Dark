// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Projectile : NetworkBehaviour
    {
        public string shooterUsername;

        [Space]
        public int Damage = 20;

        bool _hitted = false;
        public AttackType AttackType;

        public string animatorTriggerName = "KillerAttacked";
        public string animationName = "KillerAttacked";

        public float expirationTime = 5f;
        private float currentExpirationTime;

        [SyncVar] NetworkIdentity hunter;

        [SerializeField] GameObject _projectileModel;

        public void SetupProjectile(string ownerUsername, bool hasAuthority)
        {
            shooterUsername = ownerUsername;
            _hitted = false;
        }
        public void SetupProjectile_ServerSide()
        {
            StartCoroutine(DestroyProjectile());
        }
        IEnumerator DestroyProjectile()
        {
            yield return new WaitForSeconds(10);

            NetworkServer.Destroy(gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            //if (!hasAuthority) return;

            if (_hitted) return;

            if (collision.gameObject.GetComponent<VehicleEnterExit.VehicleSync>() != null)
            {
                if (collision.gameObject.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername == shooterUsername)
                {
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                    // To prevent the fired projectile from causing damage upon contact with the vehicle of the player who fired the projectile.
                    return;
                }
            }
            else if (collision.transform.root.GetComponent<VehicleEnterExit.VehicleSync>() != null)
            {
                if (collision.transform.root.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername == shooterUsername)
                {
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                    // To prevent the fired projectile from causing damage upon contact with the vehicle of the player who fired the projectile.
                    return;
                }
            }
            if (collision.gameObject.GetComponent<CharacterManager>() != null)
            {
                if (collision.gameObject.GetComponent<Player>().username == shooterUsername)
                {
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                    // To prevent the fired projectile from causing damage upon contact with the player who fired the projectile.
                    return;
                }
            }
            else if (collision.transform.root.GetComponent<CharacterManager>() != null)
            {
                if (collision.transform.root.gameObject.GetComponent<Player>().username == shooterUsername)
                {
                    Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
                    // To prevent the fired projectile from causing damage upon contact with the player who fired the projectile.
                    return;
                }
            }

            // print("I HITTED: " + collision.gameObject.name);

            GameObject hit = collision.gameObject;
            VehicleHealth health = hit.GetComponent<VehicleHealth>();
            CharacterManager playerHealth = hit.GetComponent<CharacterManager>();
            if (hit.GetComponent<VehicleHealth>() != null)
                health = hit.GetComponent<VehicleHealth>();
            else if (hit.transform.root.GetComponent<VehicleHealth>() != null)
                health = hit.transform.root.GetComponent<VehicleHealth>();
            if (hit.GetComponent<Health>() != null)
                playerHealth = hit.GetComponent<CharacterManager>();
            else if (hit.transform.root.GetComponent<Health>() != null)
                playerHealth = hit.transform.root.GetComponent<CharacterManager>();
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            if (health != null)
            {
                if (health.gameObject.GetComponent<VehicleEnterExit.VehicleSync>().DriverUsername != shooterUsername)
                {
                    if (hasAuthority)
                        CmdTakeDamageVehicle(health, Damage);
                }
            }

            if (playerHealth != null)
            {
                if (playerHealth.gameObject.GetComponent<HunterAbilities>()._isHunter)
                {
                    if (playerHealth.gameObject.GetComponent<Player>().username != shooterUsername)
                    {
                        if (hasAuthority)
                            Cmd_BlockHunter(playerHealth.gameObject.GetComponent<NetworkIdentity>());
                    }
                }
                else
                {
                    if (playerHealth.gameObject.GetComponent<Player>().username != shooterUsername)
                    {
                        if (hasAuthority)
                            CmdTakeDamage(playerHealth, Damage);
                    }
                }
            }


            StopProjectile();

            GetComponent<CapsuleCollider>().enabled = false;

            OnCollided(collision.gameObject);
            if (hasAuthority)
                CmdCollided();
            _hitted = true;
        }

        protected virtual void OnCollided(GameObject objectCollidedWith)
        {
            print("COLLIED");
        }

        [Command]
        void CmdCollided()
        {
            RpcOnCollided();
        }
        [ClientRpc]
        void RpcOnCollided()
        {
            OnCollidedRPC();
        }
        protected virtual void OnCollidedRPC()
        {
            StopProjectile();
            _projectileModel.SetActive(false);
        }


        void StopProjectile()
        {
            Rigidbody rg = GetComponent<Rigidbody>();
            rg.isKinematic = true;
            rg.linearVelocity = Vector3.zero;
        }

        [Command]
        public void CmdTakeDamageVehicle(VehicleHealth health, int damage)
        {
            health.TakeDamage(damage);
        }
        [Command]
        public void CmdTakeDamage(CharacterManager health, int damage)
        {
            health.Server_TakeDamage(damage);
        }

        private IEnumerator HandleExpirationCoroutine()
        {
            while (currentExpirationTime > 0f)
            {
                currentExpirationTime -= Time.deltaTime;
                yield return null;
            }

            if (hunter != null)
            {
                hunter.GetComponent<HunterAbilities>().isBlocked = false;
            }

            Expired();
        }

        void Expired()
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
        }

        [Command]
        void Cmd_BlockHunter(NetworkIdentity _hunter)
        {
            if (_hunter != null)
            {
                _hunter.GetComponent<HunterAbilities>().isBlocked = true;
                currentExpirationTime = expirationTime;
                StartCoroutine(ServerHandleExpirationCoroutine(_hunter));
                Rpc_BlockHunter(_hunter);
            }
        }

        private IEnumerator ServerHandleExpirationCoroutine(NetworkIdentity _hunter)
        {
            while (currentExpirationTime > 0f)
            {
                currentExpirationTime -= Time.deltaTime;
                yield return null;
            }

            if (_hunter != null)
            {
                _hunter.GetComponent<HunterAbilities>().isBlocked = false;
            }
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
                currentExpirationTime = expirationTime;
                StartCoroutine(HandleExpirationCoroutine());
            }
        }
    }
}