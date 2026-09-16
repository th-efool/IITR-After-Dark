// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Sword : GameplayItem
    {
        Coroutine c_applyDamage;

        bool swordSwinging = false;

        bool playerHit = false;

        public GameObject bloodHitPrefab;
        public GameObject metalHitPrefab;
        public GameObject woodHitPrefab;
        public GameObject stoneHitPrefab;

        public AudioSource audioSource;
        public AudioClip[] swingSounds;

        [SerializeField] bool blockMovement = false;
        [SerializeField] float blockMovementCooldown = 3.03f;
        Coroutine c_blockMovement;

        [SerializeField] GameObject cooldownUI;

        [Tooltip("If no player was hit with this weapon, the animation will be canceled after the attack with this weapon.")]
        [SerializeField] bool cancelAnimation = false;
        [SerializeField] string cancelAttackAnimatorTriggerName = "CancelSwordAttack";
        [SerializeField] float cancelAnimationCooldown = 1.13f;
        [SerializeField] float requiredStamina = 0.15f;
        Coroutine c_cancelAnimation;

        [Tooltip("Should a recoil animation be played when the weapon collides with colliders of scenery objects?")]
        public bool playRecoilAnimation = false;
        Coroutine c_cancelAttack;
        bool cancelsAttack = false;

        public string attackAnimationTriggerName = "Attack";
        public float attackAnimationLength = 1.13f;

        float _previousSpeed;
        bool _canSprint;

        private string killerBlockedAnimatorTriggerName = "KillerAttacked";
        private string killerBlockedAnimationName = "KillerAttacked";

        private float killerBlockedExpirationTime = 5f;
        private float killerBlockedCurrentExpirationTime;

        [SyncVar] NetworkIdentity hunter;

        private bool canBlockHunter = true;

        public Transform hitParticlePosition;

        public override void Use()
        {
            base.Use();
            if (_myOwner.GetComponent<StarterAssets.ThirdPersonController>()._currentStamina > _myOwner.GetComponent<StarterAssets.ThirdPersonController>()._currentStamina * requiredStamina && !swordSwinging)
            {
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), attackAnimationTriggerName);

                audioSource.clip = swingSounds[Random.Range(0, swingSounds.Length)];
                audioSource.Play();

                damagedCharacters.Clear();

                if (c_applyDamage != null)
                    StopCoroutine(c_applyDamage);
                _previousSpeed = _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed;
                _canSprint = _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint;
                c_applyDamage = StartCoroutine(applyDamageCounter(_myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed, _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint));
                if (blockMovement)
                {
                    c_blockMovement = StartCoroutine(blockMovementCounter());
                    _myOwner.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, false);
                }
                if (cancelAnimation)
                {
                    c_cancelAnimation = StartCoroutine(cancelAnimationCounter());
                }
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint = false;
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = 1;
                Instantiate(cooldownUI).GetComponent<CooldownUIManager>().StartCooldown(useCooldown);
                StarterAssets.ThirdPersonController controller = _myOwner.GetComponent<StarterAssets.ThirdPersonController>();
                float deductionAmount = controller.MaxStamina * requiredStamina;
                controller._currentStamina -= deductionAmount;
                controller._currentStamina = Mathf.Max(0f, controller._currentStamina);
            }

        }

        public override void Activate()
        {
            base.Activate();

            transform.gameObject.layer = 0;
            _itemCollider.isTrigger = true;
            _itemCollider.enabled = true;
        }
        public override void Putdown()
        {
            base.Putdown();
        }
        protected override void Server_Use()
        {
            base.Server_Use();
        }
        List<CharacterManager> damagedCharacters = new List<CharacterManager>();
        private void OnTriggerStay(Collider other)
        {
            if (!hasAuthority) return;
            //Damaging other players
            if (swordSwinging)
            {
                if (other.GetComponent<MaterialIdentifier>() != null)
                {
                    if (other.GetComponent<MaterialIdentifier>().material == MaterialEnum.Metal)
                    {
                        if (!cancelsAttack)
                        {
                            ClientInstantiateMetalHitEffect();
                            if (playRecoilAnimation)
                            {
                                cancelsAttack = true;
                                c_cancelAttack = StartCoroutine(CancelAttack());
                            }
                        }
                    }
                    else if (other.GetComponent<MaterialIdentifier>().material == MaterialEnum.Wood)
                    {
                        bool isDoor;
                        OpenableDoor door = new OpenableDoor();
                        isDoor = false;
                        if(other.GetComponent<OpenableDoorMesh>() != null)
                        {
                            door = other.GetComponent<OpenableDoorMesh>().openableDoorManager.openableDoorFront;
                            isDoor = true;
                        }
                        if (isDoor && door != null && !door.isDoorOpen && !door.isDoorDestroyed)
                        {
                            ClientInstantiateWoodHitEffect();
                            //CmdDestroyDoor(door);
                            CmdDemageDoor(door, 50);
                        }
                        else if (other.GetComponent<KnockableObstacleMesh>() != null)
                        {
                            KnockableObstacle knockableObstacle = other.GetComponent<KnockableObstacleMesh>().knockableObstacle;
                            ClientInstantiateWoodHitEffect();
                            if (knockableObstacle.isKnocked)
                                CmdDemageKnockableObstacle(knockableObstacle.GetComponent<NetworkIdentity>());
                        }
                        else if (!cancelsAttack)
                        {
                            ClientInstantiateWoodHitEffect();
                            if (playRecoilAnimation)
                            {
                                cancelsAttack = true;
                                c_cancelAttack = StartCoroutine(CancelAttack());
                            }
                        }
                    }
                    else if (other.GetComponent<MaterialIdentifier>().material == MaterialEnum.Stone)
                    {
                        if (!cancelsAttack)
                        {
                            ClientInstantiateStoneHitEffect();
                            if (playRecoilAnimation)
                            {
                                cancelsAttack = true;
                                c_cancelAttack = StartCoroutine(CancelAttack());
                            }
                        }
                    }
                    else if (other.GetComponent<MaterialIdentifier>().material == MaterialEnum.Character)
                    {
                        CharacterManager potentialVictim = other.GetComponent<CharacterManager>();

                        if (potentialVictim)
                        {
                            if (potentialVictim.transform.root != _myOwner.transform.root && !damagedCharacters.Contains(potentialVictim))
                            {
                                if (potentialVictim.GetComponent<HunterAbilities>()._isHunter)
                                {
                                    if (canBlockHunter)
                                        Cmd_BlockHunter(potentialVictim.GetComponent<NetworkIdentity>());
                                }
                                else
                                {
                                    if (potentialVictim.GetComponent<CharacterManager>().health - Damage <= 0)
                                    {
                                        _myOwner.GetComponent<Player>().SetKilledPlayersValue(1);
                                    }
                                    ClientSendDamage(potentialVictim.netIdentity);
                                    damagedCharacters.Add(potentialVictim);
                                    ClientTogglePlayerHit(true);
                                    _myOwner.GetComponent<Player>().SetDamageDealtValue(Damage);
                                    //CmdSetChaseMusic(potentialVictim.GetComponent<NetworkIdentity>());
                                }
                            }
                        }
                    }
                    else if (other.GetComponent<VehicleHealth>() != null)
                    {
                        ClientSendVehicleDamage(other.GetComponent<NetworkIdentity>());
                    }
                }
            }
        }
        [Command]
        void ClientSendDamage(NetworkIdentity _characterToDamage)
        {
            if (_characterToDamage.GetComponent<CharacterManager>().health - Damage <= 0)
            {
                _myOwner.GetComponent<Player>().killedPlayers += 1;
                _myOwner.GetComponent<CharacterManager>().TempKilledPlayers += 1;
            }

            _characterToDamage.GetComponent<CharacterManager>().Server_TakeDamage(Damage);

            GameObject hitEffect = Instantiate(bloodHitPrefab, hitParticlePosition.position, hitParticlePosition.rotation) as GameObject;

            NetworkServer.Spawn(hitEffect);

            _myOwner.GetComponent<Player>().damageDealt += Damage;
            _myOwner.GetComponent<CharacterManager>().TempDamageDealt += Damage;
        }

        IEnumerator applyDamageCounter(float previousSpeed, bool canSprint)
        {
            CmdUseWeapon(_myOwner.GetComponent<NetworkIdentity>(), true);
            swordSwinging = true;
            yield return new WaitForSeconds(useCooldown - 0.1f);
            ClientTogglePlayerHit(false);
            CmdUseWeapon(_myOwner.GetComponent<NetworkIdentity>(), false);
            swordSwinging = false;
            _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = previousSpeed;
            _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint = canSprint;
        }

        IEnumerator blockMovementCounter()
        {
            yield return new WaitForSeconds(blockMovementCooldown);
            _myOwner.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
        }

        IEnumerator cancelAnimationCounter()
        {
            yield return new WaitForSeconds(cancelAnimationCooldown - 0.1f);
            if (!playerHit)
            {
                StopCoroutine(applyDamageCounter(_previousSpeed, _canSprint));
                CmdUseWeapon(_myOwner.GetComponent<NetworkIdentity>(), false);
                swordSwinging = false;
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = _previousSpeed;
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint = _canSprint;
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), cancelAttackAnimatorTriggerName);
                if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                    CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), idleAnimatorTriggerName);
                else
                    CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), firstPersonIdleAnimatorTriggerName);
                if (blockMovement)
                {
                    StopCoroutine(c_blockMovement);
                    _myOwner.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(false, false);
                }
            }
        }

        [Command]
        void ClientInstantiateMetalHitEffect()
        {
            GameObject hitEffect = Instantiate(metalHitPrefab, hitParticlePosition.position, hitParticlePosition.rotation) as GameObject;

            NetworkServer.Spawn(hitEffect);
        }

        [Command]
        void ClientInstantiateWoodHitEffect()
        {
            GameObject hitEffect = Instantiate(woodHitPrefab, hitParticlePosition.position, hitParticlePosition.rotation) as GameObject;

            NetworkServer.Spawn(hitEffect);
        }

        [Command]
        void ClientInstantiateStoneHitEffect()
        {
            GameObject hitEffect = Instantiate(stoneHitPrefab, hitParticlePosition.position, hitParticlePosition.rotation) as GameObject;

            NetworkServer.Spawn(hitEffect);
        }

        [Command]
        void ClientTogglePlayerHit(bool status)
        {
            playerHit = status;

            RpcTogglePlayerHit(status);
        }

        [ClientRpc]
        void RpcTogglePlayerHit(bool status)
        {
            playerHit = status;
        }

        IEnumerator CancelAttack()
        {
            yield return new WaitForSeconds(0);

            NetworkClient.localPlayer.gameObject.GetComponent<CharacterManager>().CancelAttack();

            cancelsAttack = false;
        }

        [Command]
        void ClientSendVehicleDamage(NetworkIdentity _vehicleToDamage)
        {
            _vehicleToDamage.GetComponent<VehicleHealth>().currentHealth -= Damage;
        }

        [Command]
        void CmdSetChaseMusic(NetworkIdentity playerNetId)
        {
            RpcSetChaseMusic(playerNetId);
        }

        [ClientRpc]
        void RpcSetChaseMusic(NetworkIdentity playerNetId)
        {
            playerNetId.GetComponent<CharacterManager>().SetChaseMusic();
        }

        [Command]
        void CmdDestroyDoor(OpenableDoor door)
        {
            door.ServerDestroyDoor();
        }

        [Command]
        void CmdDemageDoor(OpenableDoor door, int damage)
        {
            door.ServerDamageDoor(damage, useCooldown);
        }

        [Command]
        void CmdDemageKnockableObstacle(NetworkIdentity knockableObstacle)
        {
            knockableObstacle.GetComponent<KnockableObstacle>().DestroyKnockableObstacle();
        }

        [Command]
        void CmdSetAnimatorTrigger(NetworkIdentity playerNetId, string triggerName)
        {
            playerNetId.GetComponent<Animator>().SetTrigger(triggerName);

            RpcSetAnimatorTrigger(playerNetId, triggerName);
        }

        [ClientRpc]
        void RpcSetAnimatorTrigger(NetworkIdentity playerNetId, string triggerName)
        {
            playerNetId.GetComponent<Animator>().SetTrigger(triggerName);
        }

        [Command]
        void CmdUseWeapon(NetworkIdentity playerNetId, bool _status)
        {
            playerNetId.GetComponent<CharacterManager>().usesWeapon = _status;

            RpcUseWeapon(playerNetId, _status);
        }

        [ClientRpc]
        void RpcUseWeapon(NetworkIdentity playerNetId, bool _status)
        {
            playerNetId.GetComponent<CharacterManager>().usesWeapon = _status;
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

        [Command]
        void Cmd_BlockHunter(NetworkIdentity _hunter)
        {
            if (_hunter != null)
            {
                _hunter.GetComponent<HunterAbilities>().isBlocked = true;
                killerBlockedCurrentExpirationTime = killerBlockedExpirationTime;
                StartCoroutine(ServerHandleExpirationCoroutine(_hunter));
                Rpc_BlockHunter(_hunter);
            }
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

        [ClientRpc]
        void Rpc_BlockHunter(NetworkIdentity _hunter)
        {
            if (_hunter != null)
            {
                canBlockHunter = false;
                hunter = _hunter;
                _hunter.GetComponent<StarterAssets.ThirdPersonController>().BlockPlayer(true, true);
                _hunter.GetComponent<CharacterManager>().PlayAnimationTrigger(killerBlockedAnimatorTriggerName);
                _hunter.GetComponent<HunterAbilities>().isBlocked = true;
                StartCoroutine(HandleExpirationCoroutine());
                StartCoroutine(CanBlockHunterAgainCoroutine());
            }
        }

        private IEnumerator CanBlockHunterAgainCoroutine()
        {
            yield return new WaitForSeconds(15);

            canBlockHunter = true;
        }
    }
}