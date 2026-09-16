// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Audio;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class Chainsaw : GameplayItem
    {
        private Coroutine updateCoroutine;

        public int continuousDamage = 3;
        public float applyContinuousDamageTime = 0.4f;

        public float updateInterval = 0.1f;

        Coroutine c_applyDamage;

        bool chainsawSwinging = false;

        bool playerHit = false;

        public GameObject bloodHitPrefab;
        public GameObject metalHitPrefab;
        public GameObject woodHitPrefab;
        public GameObject stoneHitPrefab;

        public AudioSource audioSource;
        public AudioClip[] swingSounds;
        public AudioClip chainsawIdleSound;
        public AudioClip chainsawAimSound;
        public AudioClip chainsawTurnOnSound;

        [SerializeField] bool blockMovement = false;
        [SerializeField] float blockMovementCooldown = 3.03f;
        Coroutine c_blockMovement;

        [SerializeField] GameObject cooldownUI;

        [Tooltip("If no player was hit with this weapon, the animation will be canceled after the attack with this weapon.")]
        [SerializeField] bool cancelAnimation = false;
        [SerializeField] float cancelAnimationCooldown = 1.13f;
        [SerializeField] float requiredStamina = 0.15f;
        Coroutine c_cancelAnimation;

        Coroutine c_cancelAttack;
        bool cancelsAttack = false;

        public string attackAnimationTriggerName = "Attack";
        public float attackAnimationLength = 1.13f;

        public Animator chainsawItemAnimator;
        public string chainsawItemIdleAnimationName = "ChainsawIdle";
        public string chainsawItemAimAnimationName = "ChainsawAim";

        public GameObject chainsawEffect;

        float _previousSpeed;
        bool _canSprint;

        public AudioMixer audioMixer;
        public string audioMixerGroupName = "Master";

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
        }

        public override void Activate()
        {
            base.Activate();

            transform.gameObject.layer = 0;
            _itemCollider.isTrigger = true;
            _itemCollider.enabled = true;
            PlayClipAt(chainsawTurnOnSound, transform.position, 1, 0.1f, 5, 1, 1);
        }
        public override void Putdown()
        {
            base.Putdown();
        }
        protected override void Server_Use()
        {
            base.Server_Use();
        }

        private void Start()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.loop = false;
            audioSource.pitch = 1.0f;
            AudioMixerGroup[] audioMixerGroups = audioMixer.FindMatchingGroups(audioMixerGroupName);
            if (audioMixerGroups.Length > 0)
            {
                audioSource.outputAudioMixerGroup = audioMixerGroups[0];
            }
            else
            {
                Debug.LogError("AudioMixerGroup not found!");
            }

            if (updateCoroutine == null)
            {
                updateCoroutine = StartCoroutine(c_Update());
            }
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
            if (hasAuthority && IsOwned && _myOwner.GetComponent<NetworkIdentity>().netId == NetworkClient.localPlayer.gameObject.GetComponent<NetworkIdentity>().netId && !_myOwner.GetComponent<HunterAbilities>()._inFight && !chainsawSwinging)
            {
                if (_input != null && _input.aim && RoomManager._instance != null && RoomManager._instance.MatchRunning && RoomManager._instance.MatchStarted && !RoomManager._instance.MatchEnding)
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
            if (IsOwned && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName == "chainsaw")
            {
                chainsawEffect.SetActive(true);
            }
            else
            {
                chainsawEffect.SetActive(false);
            }
            if (isAiming)
            {
                if (IsOwned && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName == "chainsaw")
                {
                    PlaySound(chainsawAimSound);
                    if (chainsawItemAnimator)
                    {
                        AnimatorStateInfo currentState = chainsawItemAnimator.GetCurrentAnimatorStateInfo(0);

                        if (!currentState.IsName(chainsawItemAimAnimationName))
                        {
                            chainsawItemAnimator.Play(chainsawItemAimAnimationName);
                        }
                    }
                }
            }
            else
            {
                if (IsOwned && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse != null && _myOwner.GetComponent<CharacterManager>().itemCurrentlyInUse.ItemName == "chainsaw")
                {
                    PlaySound(chainsawIdleSound);
                    if (chainsawItemAnimator)
                    {
                        AnimatorStateInfo currentState = chainsawItemAnimator.GetCurrentAnimatorStateInfo(0);

                        if (!currentState.IsName(chainsawItemIdleAnimationName))
                        {
                            chainsawItemAnimator.Play(chainsawItemIdleAnimationName);
                        }
                    }
                }
                else
                {
                    if (audioSource.enabled && audioSource.isPlaying)
                    {
                        audioSource.Stop();
                        audioSource.loop = false;
                        chainsawItemAnimator.Play(chainsawItemIdleAnimationName);
                    }
                }
            }

            if (RoomManager._instance != null)
            {
                if (!RoomManager._instance.MatchStarted)
                {
                    audioSource.enabled = false;
                }
                else if (RoomManager._instance.MatchStarted)
                {
                    if (!audioSource.enabled)
                    {
                        audioSource.enabled = true;
                        PlayClipAt(chainsawTurnOnSound, transform.position, 1, 0.1f, 5, 1, 1);
                    }
                }
            }
        }

        void PlaySound(AudioClip clip)
        {
            if (audioSource.clip != clip)
            {
                if (audioSource.enabled)
                {
                    audioSource.clip = clip;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else if (!audioSource.isPlaying)
            {
                if (audioSource.enabled)
                    audioSource.Play();
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

        List<CharacterManager> damagedCharacters = new List<CharacterManager>();
        List<VehicleHealth> damagedVehicles = new List<VehicleHealth>();

        private Coroutine damageCoroutine;

        private void OnTriggerStay(Collider other)
        {
            if (!hasAuthority) return;

            if (chainsawSwinging || isAiming)
            {
                if (other.GetComponent<MaterialIdentifier>() != null)
                {
                    MaterialIdentifier materialIdentifier = other.GetComponent<MaterialIdentifier>();

                    switch (materialIdentifier.material)
                    {
                        case MaterialEnum.Metal:
                            if (!cancelsAttack)
                            {
                                ClientInstantiateMetalHitEffect();
                                if (!isAiming)
                                {
                                    cancelsAttack = true;
                                    c_cancelAttack = StartCoroutine(CancelAttack());
                                }
                            }
                            break;

                        case MaterialEnum.Wood:
                            bool isDoor = false;
                            OpenableDoor door = null;
                            if (other.GetComponent<OpenableDoorMesh>() != null)
                            {
                                door = other.GetComponent<OpenableDoorMesh>().openableDoorManager.openableDoorFront;
                                isDoor = true;
                            }
                            if (isDoor && door != null && !door.isDoorOpen && !door.isDoorDestroyed)
                            {
                                ClientInstantiateWoodHitEffect();
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
                                if (!isAiming)
                                {
                                    cancelsAttack = true;
                                    c_cancelAttack = StartCoroutine(CancelAttack());
                                }
                            }
                            break;

                        case MaterialEnum.Stone:
                            if (!cancelsAttack)
                            {
                                ClientInstantiateStoneHitEffect();
                                if (!isAiming)
                                {
                                    cancelsAttack = true;
                                    c_cancelAttack = StartCoroutine(CancelAttack());
                                }
                            }
                            break;

                        case MaterialEnum.Character:
                            CharacterManager potentialVictim = other.GetComponent<CharacterManager>();
                            if (potentialVictim && potentialVictim.transform.root != _myOwner.transform.root)
                            {
                                if (isAiming && !damagedCharacters.Contains(potentialVictim))
                                {
                                    if (potentialVictim.GetComponent<HunterAbilities>()._isHunter)
                                    {
                                        if (canBlockHunter)
                                            Cmd_BlockHunter(potentialVictim.GetComponent<NetworkIdentity>());
                                    }
                                    else
                                    {
                                        damageCoroutine = StartCoroutine(ApplyContinuousDamage(potentialVictim, continuousDamage));
                                        damagedCharacters.Add(potentialVictim);
                                    }
                                }
                                else if (!isAiming && !damagedCharacters.Contains(potentialVictim))
                                {
                                    if (potentialVictim.GetComponent<HunterAbilities>()._isHunter)
                                    {
                                        if (canBlockHunter)
                                            Cmd_BlockHunter(potentialVictim.GetComponent<NetworkIdentity>());
                                    }
                                    else
                                    {
                                        if (potentialVictim.health - Damage <= 0)
                                        {
                                            _myOwner.GetComponent<Player>().SetKilledPlayersValue(1);
                                        }
                                        ClientSendDamage(potentialVictim.netIdentity, Damage);
                                        damagedCharacters.Add(potentialVictim);
                                        ClientTogglePlayerHit(true);
                                        _myOwner.GetComponent<Player>().SetDamageDealtValue(Damage);
                                    }
                                }
                            }
                            break;

                        default:
                            VehicleHealth vehicle = other.GetComponent<VehicleHealth>();
                            if (vehicle != null && !damagedVehicles.Contains(vehicle))
                            {
                                damageCoroutine = StartCoroutine(ApplyContinuousVehicleDamage(vehicle, continuousDamage));
                                damagedVehicles.Add(vehicle);
                            }
                            break;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            CharacterManager potentialVictim = other.GetComponent<CharacterManager>();
            if (potentialVictim && damagedCharacters.Contains(potentialVictim))
            {
                StopCoroutine(damageCoroutine);
                damagedCharacters.Remove(potentialVictim);
            }

            VehicleHealth vehicle = other.GetComponent<VehicleHealth>();
            if (vehicle && damagedVehicles.Contains(vehicle))
            {
                StopCoroutine(damageCoroutine);
                damagedVehicles.Remove(vehicle);
            }
        }

        private IEnumerator ApplyContinuousDamage(CharacterManager victim, int damage)
        {
            while (isAiming)
            {
                if (victim.health - damage <= 0)
                {
                    _myOwner.GetComponent<Player>().SetKilledPlayersValue(1);
                }

                ClientSendDamage(victim.netIdentity, damage);
                _myOwner.GetComponent<Player>().SetDamageDealtValue(damage);

                yield return new WaitForSeconds(applyContinuousDamageTime);
            }
        }

        private IEnumerator ApplyContinuousVehicleDamage(VehicleHealth vehicle, int damage)
        {
            while (isAiming)
            {
                ClientSendVehicleDamage(vehicle.GetComponent<NetworkIdentity>(), damage);
                yield return new WaitForSeconds(applyContinuousDamageTime);
            }
        }

        [Command]
        void ClientSendDamage(NetworkIdentity characterToDamage, int damage)
        {
            CharacterManager characterManager = characterToDamage.GetComponent<CharacterManager>();
            if (characterManager.health - damage <= 0)
            {
                _myOwner.GetComponent<Player>().killedPlayers += 1;
                _myOwner.GetComponent<CharacterManager>().TempKilledPlayers += 1;
            }

            characterManager.Server_TakeDamage(damage);

            if (isAiming)
            {
                GameObject hitEffect = Instantiate(bloodHitPrefab, hitParticlePosition.position, hitParticlePosition.rotation);
                NetworkServer.Spawn(hitEffect);
            }

            _myOwner.GetComponent<Player>().damageDealt += damage;
            _myOwner.GetComponent<CharacterManager>().TempDamageDealt += damage;
        }

        [Command]
        void ClientSendVehicleDamage(NetworkIdentity vehicleToDamage, int damage)
        {
            VehicleHealth vehicleHealth = vehicleToDamage.GetComponent<VehicleHealth>();
            vehicleHealth.currentHealth -= damage;
        }

        IEnumerator applyDamageCounter(float previousSpeed, bool canSprint)
        {
            chainsawSwinging = true;
            yield return new WaitForSeconds(attackAnimationLength - 0.1f);
            ClientTogglePlayerHit(false);
            chainsawSwinging = false;
            _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = previousSpeed;
            _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint = canSprint;
            CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), "StopAttack");

            if (string.IsNullOrEmpty(idleAnimatorTriggerName))
            {
                idleAnimatorTriggerName = chainsawItemIdleAnimationName;
            }
            if (!_myOwner.GetComponent<ManageTPController>().isFirstPerson)
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), idleAnimatorTriggerName);
            else
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), firstPersonIdleAnimatorTriggerName);
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
                chainsawSwinging = false;
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = _previousSpeed;
                _myOwner.GetComponent<StarterAssets.ThirdPersonController>().canSprint = _canSprint;
                CmdSetAnimatorTrigger(_myOwner.GetComponent<NetworkIdentity>(), "StopAttack");
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

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance, float spatialBlend, float reverbZoneMix)
        {
            if (!isServer)
            {
                var tempGO = new GameObject();
                tempGO.transform.position = _position;
                var aSource = tempGO.AddComponent<AudioSource>();
                aSource.clip = _clip;
                aSource.volume = _volume;
                aSource.minDistance = _minDistance;
                aSource.maxDistance = _maxDistance;
                aSource.reverbZoneMix = reverbZoneMix;
                aSource.spatialBlend = spatialBlend;
                aSource.Play();
                Destroy(tempGO, _clip.length);
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
                killerBlockedCurrentExpirationTime = killerBlockedExpirationTime;
                StartCoroutine(HandleExpirationCoroutine());
                StartCoroutine(CanBlockHunterAgainCoroutine());
            }
        }

        private IEnumerator CanBlockHunterAgainCoroutine()
        {
            yield return new WaitForSeconds(15);

            canBlockHunter = true;
        }

        private void OnDestroy()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }
    }
}