// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using System.Threading.Tasks;
using UnityEngine.Rendering.PostProcessing;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    /// <summary>
    /// This component is used for hunter special attacks and for survivor behaviour while being attacked
    /// </summary>
    [RequireComponent(typeof(CharacterManager))]
    public class HunterAbilities : NetworkBehaviour
    {
        public delegate void HunterAbilityUsed(int abilityID, float cooldown);
        public HunterAbilityUsed CharacterEvent_HunterAbilityUsed;

        public delegate void VictimInRange(bool inRange);
        public VictimInRange CharacterEvent_VictimInRange;

        public delegate void FightStateChanged(int hunterState, int victimState);
        public FightStateChanged FightEvent_FightStateChanged;

        [Header("Hunters")]
        Hunter currentHunter;
        public Hunter[] hunters;
        public int currentHunterID = 1;

        public Avatar NormalAvatar;
        public GameObject[] HunterMeshParents;
        public GameObject[] SurvivorMeshParents;
        public GameObject[] SurvivorXRayMesh;
        public GameObject showFinisherCameraPrefab;
        private GameObject instantiatedShowFinisherCameraPrefab;

        [Header("Audio")]
        public AudioSource chaseMusicAudioSource;
        bool isFadeOutAudioSource = false;
        float elapsedTime = 0;
        public AudioClip survivorHeartbeat;
        public AudioSource survivorHeartbeatAudioSource;
        [Tooltip("This music is played for the survivor player when they are injured and being chased by the hunter.")]
        public AudioClip survivorInjuredChaseMusic;

        private static Transform mainCamera;
        private static Transform characterCanvas;

        [Header("Inputs")]
        [SerializeField] KeyCode _leftAbilityInput = KeyCode.LeftArrow;
        [SerializeField] KeyCode _upAbilityInput = KeyCode.UpArrow;
        [SerializeField] KeyCode _rightAbilityInput = KeyCode.RightArrow;
        [SerializeField] KeyCode _fightInput = KeyCode.Mouse0;

        CharacterManager _myCharacterManager;
        public CharacterManager _potentialVictim;
        public HunterAbilities _victim;

        [HideInInspector] public HunterAbilities _attacker;

        int _rayCastMask = (1 << 0 | 1 << 9);

        float distanceBeetwenHunterAndVictim = 1f; //1 = one meter


        public List<CharacterManager> potentialVictims = new List<CharacterManager>();

        [HideInInspector] public bool _isHunter;
        [HideInInspector] public bool _inFight;
        [HideInInspector] public bool _isFighting = false;
        [HideInInspector] public bool _canUseItems = true;

        [HideInInspector] public int FightInputCounter;

        public bool bot = false;

        public float HunterLerpPositionSpeed = 5f;

        bool victimInRange = false;

        float chaseMusicDistance = 5f;

        private static List<HunterAbilities> allVictims = new List<HunterAbilities>();

        private PostProcessVolume postProcessVolume;
        private Vignette vignette;
        public float intensityIncrement = 0.1f;
        public float changeInterval = 0.1f;

        [SyncVar] public bool isBlocked;

        private RedicionStudio.PlayerInputs _input;

        private bool lastToggleFightInput = false;

        void Start()
        {
            _myCharacterManager = GetComponent<CharacterManager>();
        }

        float _timer = 0;
        float _botSpacebarCooldown = 0.3f;
        void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (bot)
            {
                if (_inFight)
                {
                    if (_timer <= Time.time)
                    {
                        _timer = Time.time + _botSpacebarCooldown;
                        FightInputCounter++;
                        RPC_FightInput(FightInputCounter);
                    }
                }
            }

            GetComponent<Animator>().SetBool("isHunter", _isHunter);

            if (GetComponent<Player>().killerId != currentHunterID)
                currentHunterID = GetComponent<Player>().killerId;

            if (currentHunter == null)
            {
                foreach (Hunter hunter in hunters)
                {
                    if (hunter.HunterID == currentHunterID)
                    {
                        currentHunter = hunter;
                    }
                }
            }
            else if (currentHunter.HunterID != currentHunterID)
            {
                foreach (Hunter hunter in hunters)
                {
                    if (hunter.HunterID == currentHunterID)
                    {
                        currentHunter = hunter;
                    }
                }
            }

            if (!hasAuthority) return;

            PlayChaseMusic();

            bool currentFightToggleInput = _input.fight;

            if (_inFight)
            {
                if (currentFightToggleInput && !lastToggleFightInput)
                    Cmd_FightInput();
            }

            lastToggleFightInput = currentFightToggleInput;

            if (!_isHunter) return;

            if (!RoomManager._instance.MatchRunning) return;

            if (RoomManager._instance.MatchEnding) return;

            if (_inFight) return;

            if (_input.activateHunterAbility1)
                ActivateAbility(0);
            if (_input.activateHunterAbility2)
                ActivateAbility(1);
            if (_input.activateHunterAbility3)
                ActivateAbility(2);

        }
        [Command]
        void Cmd_FightInput()
        {
            FightInputCounter++;
            RPC_FightInput(FightInputCounter);
        }
        [ClientRpc]
        void RPC_FightInput(int fightInputCounter)
        {
            FightInputCounter = fightInputCounter;
        }
        private void FixedUpdate()
        {
            //hunter instance of script will update the UI, even for victims, because UI watches hunter script instance
            if (_inFight)
            {
                if (_isHunter)
                {
                    FightEvent_FightStateChanged?.Invoke(FightInputCounter, _victim.FightInputCounter);
                }
                else
                {
                    FightEvent_FightStateChanged?.Invoke(_attacker.FightInputCounter, FightInputCounter);
                }
            }

            if (!_isHunter) return;

            #region checking by overlap capsule and raycast if we have survivor in range
            potentialVictims.Clear();
            _potentialVictim = null;

            Collider[] col = Physics.OverlapCapsule(transform.position + transform.forward * 0.5f + transform.up * 1f, transform.position + transform.forward * 1.5f + transform.up * 1f, 0.5f);


            for (int i = 0; i < col.Length; i++)
            {
                if (col[i].gameObject.TryGetComponent(out CharacterManager potentialVictim))
                {
                    if (potentialVictim.transform.root != transform.root)
                        potentialVictims.Add(potentialVictim);
                }
            }

            if (potentialVictims.Count > 0)
            {
                List<CharacterManager> sortedVictims = potentialVictims.OrderBy(e => Vector3.Distance(gameObject.transform.position, e.transform.position)).ToList();

                if (AbleToReachVictim(sortedVictims[0]))
                {
                    _potentialVictim = sortedVictims[0];
                }
            }
            #endregion

            if (!victimInRange && _potentialVictim) //victim in hunter range
            {
                victimInRange = true;
                CharacterEvent_VictimInRange?.Invoke(victimInRange);
            }

            if (victimInRange && !_potentialVictim) //victim went out of hunter range
            {
                victimInRange = false;
                CharacterEvent_VictimInRange?.Invoke(victimInRange);
            }
        }

        void PlayChaseMusic()
        {
            #region Chase Music and Survivor Heartbeat Management
            if (RoomManager._instance.MatchStarted && RoomManager._instance.MatchRunning)
            {
                if (isFadeOutAudioSource)
                {
                    if (chaseMusicAudioSource.volume != 0)
                    {
                        float t = elapsedTime / 2f;
                        chaseMusicAudioSource.volume = Mathf.Lerp(1, 0, t);
                        elapsedTime += Time.deltaTime;
                    }
                    else if (chaseMusicAudioSource.volume == 0)
                    {
                        chaseMusicAudioSource.Stop();
                        chaseMusicAudioSource.volume = 1;
                        elapsedTime = 0;
                        isFadeOutAudioSource = false;
                    }
                }
                else
                {
                    // checks if a victim is near the hunter
                    bool victimFound = new bool();
                    victimFound = false;
                    var colliders = Physics.OverlapSphere(transform.position, chaseMusicDistance);
                    foreach (var collider in colliders)
                    {
                        if (_isHunter && collider.gameObject.GetComponent<CharacterManager>() != null || !_isHunter && collider.gameObject.GetComponent<CharacterManager>() != null && collider.gameObject.GetComponent<HunterAbilities>()._isHunter)
                        {
                            if (collider.gameObject.GetComponent<CharacterManager>().alive & collider.gameObject.GetComponent<Player>().username != GetComponent<Player>().username)
                            {
                                victimFound = true;
                                if (!chaseMusicAudioSource.isPlaying)
                                {
                                    if (collider.gameObject.GetComponent<CharacterManager>().health < 100 || !_isHunter && GetComponent<CharacterManager>().health < 100)
                                    {
                                        chaseMusicAudioSource.clip = survivorInjuredChaseMusic;
                                    }
                                    else
                                    {
                                        chaseMusicAudioSource.clip = currentHunter.ChaseMusic;
                                    }
                                    chaseMusicAudioSource.Play();
                                    chaseMusicDistance = 8f;
                                }
                            }
                        }
                    }
                    if (!victimFound)
                    {
                        if (chaseMusicAudioSource.isPlaying)
                        {
                            chaseMusicDistance = 5f;
                            isFadeOutAudioSource = true;
                        }
                    }
                }
            }
            if (!_isHunter && RoomManager._instance.MatchStarted && RoomManager._instance.MatchRunning)
            {
                if (isFadeOutAudioSource)
                {
                    if (survivorHeartbeatAudioSource.volume != 0)
                    {
                        float t = elapsedTime / 2f;
                        survivorHeartbeatAudioSource.volume = Mathf.Lerp(1, 0, t);
                        elapsedTime += Time.deltaTime;
                    }
                    else if (survivorHeartbeatAudioSource.volume == 0)
                    {
                        survivorHeartbeatAudioSource.Stop();
                        survivorHeartbeatAudioSource.volume = 1;
                        elapsedTime = 0;
                        isFadeOutAudioSource = false;
                    }
                }
                else
                {
                    // checks if hunter is near the player
                    Transform foundHunter = new GameObject().transform;
                    bool hunterFound = new bool();
                    hunterFound = false;
                    var colliders = Physics.OverlapSphere(transform.position, 30f);
                    foreach (var collider in colliders)
                    {
                        if (collider.gameObject.GetComponent<CharacterManager>() != null)
                        {
                            if (GetComponent<CharacterManager>().alive && collider.gameObject.GetComponent<CharacterManager>().alive & collider.gameObject.GetComponent<HunterAbilities>()._isHunter & collider.gameObject.GetComponent<Player>().username != GetComponent<Player>().username)
                            {
                                foundHunter = collider.transform;
                                hunterFound = true;
                                if (!survivorHeartbeatAudioSource.isPlaying)
                                {
                                    survivorHeartbeatAudioSource.pitch = 1;
                                    survivorHeartbeatAudioSource.clip = survivorHeartbeat;
                                    survivorHeartbeatAudioSource.Play();
                                }
                            }
                        }
                    }
                    if (!hunterFound)
                    {
                        if (survivorHeartbeatAudioSource.isPlaying)
                        {
                            isFadeOutAudioSource = true;
                        }
                    }
                    if (hunterFound == true & foundHunter != null & Vector3.Distance(transform.position, foundHunter.position) < 30f)
                    {
                        float distanceToHunter = Vector3.Distance(transform.position, foundHunter.position);
                        if (distanceToHunter < 4)
                            distanceToHunter = 4;
                        survivorHeartbeatAudioSource.pitch = 35 / distanceToHunter / 2;

                        if (survivorHeartbeatAudioSource.pitch < 1f)
                            survivorHeartbeatAudioSource.pitch = 1f;
                        else if (survivorHeartbeatAudioSource.pitch > 2.05f)
                            survivorHeartbeatAudioSource.pitch = 2.05f;
                    }
                }
            }
            #endregion
        }

        bool AbleToReachVictim(CharacterManager victim)
        {
            if (victim != null)
                return true;
            RaycastHit hit;
            Vector3 hunterCheckerPoint = transform.position + transform.up * 1f;
            if (Physics.Raycast(hunterCheckerPoint, (victim.transform.position + victim.transform.up * 1f) - hunterCheckerPoint, out hit, 10f, _rayCastMask))
            {
                return hit.collider.gameObject.transform.root == victim.transform.root;
            }
            return false;
        }

        public void SetHunter()
        {
            foreach (GameObject AllHunterMesh in HunterMeshParents)
            {
                AllHunterMesh.SetActive(true);
            }
            foreach (GameObject AllSurvivorMesh in SurvivorMeshParents)
            {
                AllSurvivorMesh.SetActive(false);
            }
            foreach (Hunter hunter in hunters)
            {
                foreach (GameObject AllHunterMesh in hunter.HunterMesh)
                {
                    AllHunterMesh.SetActive(false);
                }
                if (hunter.HunterID == currentHunterID)
                {
                    currentHunter = hunter;
                    this.GetComponent<CharacterManager>().itemParent = hunter.itemParent;
                    this.GetComponent<Animator>().avatar = hunter.HunterAvatar;
                    if (currentHunter.useAnimatorOverrideController)
                    {
                        this.GetComponent<Animator>().runtimeAnimatorController = hunter.AnimatorController;
                        this.GetComponent<CharacterManager>().defaultAnimController = hunter.AnimatorController;
                    }
                    foreach (GameObject AllHunterMesh in hunter.HunterMesh)
                    {
                        AllHunterMesh.SetActive(true);
                    }
                    GetComponent<StarterAssets.ThirdPersonController>().canSprint = hunter.canSprint;
                    GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = hunter.MoveSpeed;
                    GetComponent<StarterAssets.ThirdPersonController>().SprintSpeed = hunter.SprintSpeed;
                    GetComponent<StarterAssets.ThirdPersonController>().MovementMultiplier = hunter.MovementMultiplier;
                }
            }
            if (isServer)
            {
                _isHunter = true;
                Rpc_SetHunterStatus(true);
            }
            else if (hasAuthority)
            {
                Cmd_SetHunterStatus(true);
            }

            for (int i = 0; i < 3; i++)
            {
                ResetCooldown(i);
            }

            characterCanvas = GameObject.FindGameObjectWithTag("CharacterCanvas").transform;
            var hunterAbilitiesUI = characterCanvas.GetComponent<HunterAbilitiesUI>();
            var specialAttacks = currentHunter._specialAttacks;

            for (int i = 0; i < specialAttacks.Length; i++)
            {
                var iconIndex = i;
                var specialAttackImage = specialAttacks[i].specialAttackImage;

                hunterAbilitiesUI._hunterAbilitiesIcons[iconIndex].hunterAbilityImage.sprite = specialAttackImage;
                hunterAbilitiesUI._hunterAbilitiesIcons[iconIndex].hunterAbilityFilterImage.sprite = specialAttackImage;
                hunterAbilitiesUI._hunterAbilitiesIcons[iconIndex].hunterAbilityFilledImage.sprite = specialAttackImage;
            }
        }

        [Command]
        void Cmd_SetHunterStatus(bool status)
        {
            _isHunter = status;

            Rpc_SetHunterStatus(status);
        }

        [ClientRpc]
        void Rpc_SetHunterStatus(bool status)
        {
            _isHunter = status;
        }

        public void SetSurvivor()
        {
            foreach (Hunter hunter in hunters)
            {
                foreach (GameObject AllHunterMesh in hunter.HunterMesh)
                {
                    AllHunterMesh.SetActive(false);
                }
            }
            foreach (GameObject AllHunterMesh in HunterMeshParents)
            {
                AllHunterMesh.SetActive(false);
            }
            foreach (GameObject AllSurvivorMesh in SurvivorMeshParents)
            {
                AllSurvivorMesh.SetActive(true);
            }
            GetComponent<OutfitManager>().ShowOutfit(GetComponent<Player>().outfitId);
            this.GetComponent<Animator>().avatar = NormalAvatar;
            if (isServer)
            {
                _isHunter = false;
                Rpc_SetHunterStatus(false);
            }
            else if (hasAuthority)
            {
                Cmd_SetHunterStatus(false);
            }
            this.GetComponent<CharacterManager>().defaultAnimController = this.GetComponent<CharacterManager>().survivorAnimController;
            GetComponent<StarterAssets.ThirdPersonController>().canSprint = true;
            GetComponent<StarterAssets.ThirdPersonController>().MoveSpeed = 1f;
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

        void ActivateAbility(int abilityID)
        {

            if (_inFight) return;

            if (currentHunter._specialAttacks[abilityID]._abilityCooldownEnd > Time.time) return;

            if (currentHunter._specialAttacks[abilityID].specialAttackType == HunterSpecialAttack.SpecialAttackType.Finisher)
            {
                if (!_potentialVictim || _potentialVictim != null && _potentialVictim.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar || _potentialVictim != null && _potentialVictim.GetComponent<PlayerInteractionModule>().isClimbing && _potentialVictim.GetComponent<CharacterManager>().escaped) return;

                if (GetComponent<CharacterManager>().itemCurrentlyInUse.GetComponent<Sword>() == null) return;

                ResetCooldown(abilityID);

                CmdSetAbilitiesUsedValue(1);

                _isFighting = true;
                Cmd_StartFinisher(_potentialVictim, abilityID);
            }
            else if (currentHunter._specialAttacks[abilityID].specialAttackType == HunterSpecialAttack.SpecialAttackType.HuntersVision)
            {
                ResetCooldown(abilityID);

                CmdSetAbilitiesUsedValue(1);

                allVictims.Clear();

                NetworkIdentity[] networkIdentities = FindObjectsOfType<NetworkIdentity>();

                foreach (var networkIdentity in networkIdentities)
                {
                    HunterAbilities networkPlayer = networkIdentity.GetComponent<HunterAbilities>();
                    if (networkPlayer != null)
                    {
                        if (!networkPlayer._isHunter)
                        {
                            allVictims.Add(networkPlayer);
                            foreach (GameObject xRayMesh in networkPlayer.SurvivorXRayMesh)
                                xRayMesh.SetActive(true);
                        }
                    }
                }

                StartCoroutine(C_HuntersVision(abilityID));
            }
            else if (currentHunter._specialAttacks[abilityID].specialAttackType == HunterSpecialAttack.SpecialAttackType.HuntersInstinct)
            {
                ResetCooldown(abilityID);

                CmdSetAbilitiesUsedValue(1);

                mainCamera = GameObject.Find("MainCamera").transform;

                if (mainCamera != null)
                {
                    int layerMask = 1 << 31;
                    mainCamera.GetComponent<Camera>().cullingMask |= layerMask;
                }

                StartCoroutine(C_HuntersInstinct(abilityID));
            }
            else if (currentHunter._specialAttacks[abilityID].specialAttackType == HunterSpecialAttack.SpecialAttackType.RapidRush)
            {
                ResetCooldown(abilityID);

                CmdSetAbilitiesUsedValue(1);

                StartCoroutine(C_RapidRush(abilityID));
            }
            else if (currentHunter._specialAttacks[abilityID].specialAttackType == HunterSpecialAttack.SpecialAttackType.BlackoutStrike)
            {
                ResetCooldown(abilityID);

                CmdSetAbilitiesUsedValue(1);

                allVictims.Clear();

                NetworkIdentity[] networkIdentities = FindObjectsOfType<NetworkIdentity>();

                foreach (var networkIdentity in networkIdentities)
                {
                    HunterAbilities networkPlayer = networkIdentity.GetComponent<HunterAbilities>();
                    if (networkPlayer != null)
                    {
                        if (!networkPlayer._isHunter)
                        {
                            allVictims.Add(networkPlayer);
                            foreach (GameObject xRayMesh in networkPlayer.SurvivorXRayMesh)
                                xRayMesh.SetActive(true);
                        }
                    }
                }

                StartCoroutine(C_BlackoutStrike(abilityID));
            }
        }
        [Command]
        void Cmd_StartFinisher(CharacterManager victim, int specialAttackID)
        {
            _myCharacterManager.Rpc_SetMovementPermission(false);
            victim.Rpc_SetMovementPermission(false);

            StartCoroutine(FinisherCoroutine(victim.GetComponent<HunterAbilities>(), specialAttackID));

            GetComponent<Player>().capturedPlayers += 1;
            GetComponent<CharacterManager>().TempCapturedPlayers += 1;
            RPC_StartFinisher(victim, victim.transform.position, specialAttackID);
        }

        [ClientRpc]
        public void RPC_StartFinisher(CharacterManager victim, Vector3 victimPositionOnServer, int specialAttackID)
        {
            _victim = victim.GetComponent<HunterAbilities>();
            _victim._attacker = this;
            _victim._canUseItems = false;
            _victim._inFight = true;
            _victim._isFighting = true;
            _victim.FightInputCounter = 1;

            _inFight = true;
            _canUseItems = false;
            FightInputCounter = 1;
            victim.GetComponent<Animator>().SetBool("Injured", false);

            _victim.PlayVictimFightAnimation(victim.GetComponent<HunterAbilities>(), specialAttackID);
            PlayHunterFightAnimation(specialAttackID);

            FightEvent_FightStateChanged?.Invoke(FightInputCounter, _victim.FightInputCounter);

            Vector3 hunterPozXZ = new Vector3(transform.position.x, victim.transform.position.y, transform.position.z);
            Vector3 victimPozXZ = new Vector3(victim.transform.position.x, transform.position.y, victim.transform.position.z);

            victim.transform.position = victimPositionOnServer;
            victim.transform.LookAt(hunterPozXZ);

            transform.LookAt(victimPozXZ);
            _myCharacterManager.LerpCharacterPositionToDestination(victim.transform.position + (hunterPozXZ - victimPozXZ).normalized * distanceBeetwenHunterAndVictim);

            StartCoroutine(SetVictimPositionCoroutine(victim, specialAttackID));

            if (victim.itemCurrentlyInUse != null)
            {
                foreach (MeshRenderer itemMeshRenderer in victim.itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = false;
            }
            GetComponent<Player>().SetCapturedPlayersValue(1);
            ManageFinisherCamera(victim.GetComponent<NetworkIdentity>(), true);
        }

        IEnumerator SetVictimPositionCoroutine(CharacterManager victim, int specialAttackID)
        {
            float duration = currentHunter._specialAttacks[specialAttackID].FinisherDuration + currentHunter._specialAttacks[specialAttackID].FightDuration;
            float elapsedTime = 0.0f;

            while (elapsedTime < duration)
            {
                Vector3 hunterPozXZ = new Vector3(transform.position.x, victim.transform.position.y, transform.position.z);
                Vector3 victimPozXZ = new Vector3(victim.transform.position.x, transform.position.y, victim.transform.position.z);

                victim.transform.LookAt(hunterPozXZ);

                transform.LookAt(victimPozXZ);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        IEnumerator FinisherCoroutine(HunterAbilities victim, int specialAttackID)
        {
            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].FightDuration);
            _inFight = false;
            //_canUseItems = true;
            victim._inFight = false;
            victim._canUseItems = true;
            victim.GetComponent<Animator>().SetBool("Injured", false);

            RPC_EndFinisher(victim, FightInputCounter >= victim.FightInputCounter, specialAttackID);
            StartCoroutine(EndFinisherCoroutine(victim));

            if (FightInputCounter >= victim.FightInputCounter) //hunter wins
            {
                yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].VictimDeathDuration);
                victim.GetComponent<CharacterManager>().Server_TakeDamage(999);
                GetComponent<Player>().killedPlayers += 1;
                GetComponent<CharacterManager>().TempKilledPlayers += 1;
                GetComponent<Player>().damageDealt += 100;
                GetComponent<CharacterManager>().TempDamageDealt += 100;
                StartCoroutine(HunterRecovered(specialAttackID, victim));
            }
            else   //survivor wins
            {
                StartCoroutine(VictimEscaped(victim, specialAttackID));
                yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].HunterDisabledDuration);
                GetComponent<CharacterManager>().Rpc_SetMovementPermission(true);
            }
        }

        IEnumerator HunterRecovered(int specialAttackID, HunterAbilities victim)
        {
            yield return new WaitForSeconds(0);

            GetComponent<CharacterManager>().Rpc_SetMovementPermission(true);
            RpcSetItemAnimation(victim.gameObject.GetComponent<NetworkIdentity>());
            RpcSetItemAnimation(GetComponent<NetworkIdentity>());
        }

        IEnumerator EndFinisherCoroutine(HunterAbilities victim)
        {
            yield return new WaitForSeconds(5);

            victim._isFighting = false;
            _isFighting = false;
            _canUseItems = true;
        }

        [ClientRpc]
        public void RPC_EndFinisher(HunterAbilities victim, bool hunterWon, int specialAttackID)
        {
            _inFight = false;
            //_canUseItems = true;
            victim._inFight = false;
            victim._canUseItems = true;
            if (victim.GetComponent<CharacterManager>().health < victim.GetComponent<CharacterManager>().maxHealth)
                victim.GetComponent<Animator>().SetBool("Injured", true);
            StartCoroutine(EndFinisherCoroutine(victim));

            //hide fight bars for victim and hunter
            victim.FightEvent_FightStateChanged?.Invoke(0, 0);
            FightEvent_FightStateChanged?.Invoke(0, 0);

            //if hunter won play finisher animation for every client
            if (hunterWon)
            {
                victim.PlayVictimDeathAnimation(victim, specialAttackID);
                PlayHunterKillAnimation(specialAttackID);
                GetComponent<Player>().SetKilledPlayersValue(1);
                GetComponent<Player>().SetDamageDealtValue(100);
            }
            else
            {
                victim.GetComponent<CharacterManager>().PlayAnimationTrigger(currentHunter._specialAttacks[specialAttackID].VictimEscapedAnimatorTriggerName);
                transform.GetComponent<CharacterManager>().PlayAnimationTrigger(currentHunter._specialAttacks[specialAttackID].HunterDisabledAnimatorTriggerName);
            }
            ManageFinisherCamera(victim.GetComponent<NetworkIdentity>(), false);
            if (victim.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (!victim.GetComponent<ManageTPController>().isFirstPerson)
                    victim.GetComponent<Animator>().SetTrigger(victim.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    victim.GetComponent<Animator>().SetTrigger(victim.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                foreach (MeshRenderer itemMeshRenderer in victim.GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = true;
            }
        }

        IEnumerator VictimEscaped(HunterAbilities victim, int specialAttackID)
        {
            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].VictimEscapedDuration);

            victim.gameObject.GetComponent<CharacterManager>().Rpc_SetMovementPermission(true);
            RpcSetItemAnimation(GetComponent<NetworkIdentity>());
            RpcSetItemAnimation(victim.gameObject.GetComponent<NetworkIdentity>());
        }

        [ClientRpc]
        void RpcSetItemAnimation(NetworkIdentity _player)
        {
            if (_player.GetComponent<CharacterManager>().itemCurrentlyInUse != null)
            {
                if (!_player.GetComponent<ManageTPController>().isFirstPerson)
                    _player.GetComponent<Animator>().SetTrigger(_player.GetComponent<CharacterManager>().itemCurrentlyInUse.idleAnimatorTriggerName);
                else
                    _player.GetComponent<Animator>().SetTrigger(_player.GetComponent<CharacterManager>().itemCurrentlyInUse.firstPersonIdleAnimatorTriggerName);
                foreach (MeshRenderer itemMeshRenderer in _player.GetComponent<CharacterManager>().itemCurrentlyInUse._itemMesh)
                    itemMeshRenderer.enabled = true;
            }
        }

        void ResetCooldown(int abilityID)
        {
            currentHunter._specialAttacks[abilityID]._abilityCooldownEnd = Time.time + currentHunter._specialAttacks[abilityID].Cooldown;
            CharacterEvent_HunterAbilityUsed?.Invoke(abilityID, currentHunter._specialAttacks[abilityID].Cooldown);
        }

        #region Animations played while spacebars are being pressed

        public void PlayVictimFightAnimation(HunterAbilities victim, int specialAttackID)
        {
            victim.GetComponent<Animator>().SetTrigger(currentHunter._specialAttacks[specialAttackID].VictimFightAnimatorTriggerName);
        }
        public void PlayHunterFightAnimation(int specialAttackID)
        {
            transform.GetComponent<Animator>().SetTrigger(currentHunter._specialAttacks[specialAttackID].HunterFightAnimatorTriggerName);
        }
        #endregion
        #region Finisher Animations
        public void PlayHunterKillAnimation(int specialAttackID)
        {
            transform.GetComponent<Animator>().SetTrigger(currentHunter._specialAttacks[specialAttackID].HunterKillAnimatorTriggerName);
        }
        public void PlayVictimDeathAnimation(HunterAbilities victim, int specialAttackID)
        {
            victim.GetComponent<Animator>().SetTrigger(currentHunter._specialAttacks[specialAttackID].VictimDeathAnimatorTriggerName);
        }

        IEnumerator C_HuntersVision(int specialAttackID)
        {
            PlayClipAt(currentHunter._specialAttacks[specialAttackID].HuntersVisionAudioClip, transform.position, 1, 1, 500, 0, 1);

            if (postProcessVolume == null)
            {
                postProcessVolume = GameObject.FindGameObjectWithTag("PostProcessVolume").GetComponent<PostProcessVolume>();
                postProcessVolume.profile.TryGetSettings(out vignette);
            }

            float targetIntensity = 0.737f; // The target intensity for the vignette
            float currentIntensity = 0.0f; // The current intensity of the vignette

            while (currentIntensity < targetIntensity)
            {
                currentIntensity += intensityIncrement;
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].HuntersVisionDuration);

            while (currentIntensity > 0.55f)
            {
                currentIntensity -= intensityIncrement; // Decrease intensity gradually
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            // Ensure the vignette intensity reaches exactly 0.55f
            vignette.intensity.value = 0.55f;

            allVictims.Clear();

            NetworkIdentity[] networkIdentities = FindObjectsOfType<NetworkIdentity>();

            foreach (var networkIdentity in networkIdentities)
            {
                HunterAbilities networkPlayer = networkIdentity.GetComponent<HunterAbilities>();
                if (networkPlayer != null)
                {
                    if (!networkPlayer._isHunter)
                    {
                        allVictims.Add(networkPlayer);
                        foreach (GameObject xRayMesh in networkPlayer.SurvivorXRayMesh)
                            xRayMesh.SetActive(false);
                    }
                }
            }

            allVictims.Clear();
        }

        IEnumerator C_HuntersInstinct(int specialAttackID)
        {
            if (postProcessVolume == null)
            {
                postProcessVolume = GameObject.FindGameObjectWithTag("PostProcessVolume").GetComponent<PostProcessVolume>();
                postProcessVolume.profile.TryGetSettings(out vignette);
            }

            float targetIntensity = 0.737f; // The target intensity for the vignette
            float currentIntensity = 0.0f; // The current intensity of the vignette

            while (currentIntensity < targetIntensity)
            {
                currentIntensity += intensityIncrement;
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].HuntersInstinctDuration);

            while (currentIntensity > 0.55f)
            {
                currentIntensity -= intensityIncrement; // Decrease intensity gradually
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            // Ensure the vignette intensity reaches exactly 0.55f
            vignette.intensity.value = 0.55f;

            if (mainCamera != null)
            {
                int layerMask = ~(1 << 31);
                mainCamera.GetComponent<Camera>().cullingMask &= layerMask;
            }
        }

        IEnumerator C_RapidRush(int specialAttackID)
        {
            GetComponent<StarterAssets.ThirdPersonController>().SprintSpeed = currentHunter._specialAttacks[specialAttackID].RapidRushSprintSpeed;

            if (postProcessVolume == null)
            {
                postProcessVolume = GameObject.FindGameObjectWithTag("PostProcessVolume").GetComponent<PostProcessVolume>();
                postProcessVolume.profile.TryGetSettings(out vignette);
            }

            float targetIntensity = 0.737f; // The target intensity for the vignette
            float currentIntensity = 0.0f; // The current intensity of the vignette

            while (currentIntensity < targetIntensity)
            {
                currentIntensity += intensityIncrement;
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].RapidRushDuration);

            while (currentIntensity > 0.55f)
            {
                currentIntensity -= intensityIncrement; // Decrease intensity gradually
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            // Ensure the vignette intensity reaches exactly 0.55f
            vignette.intensity.value = 0.55f;

            GetComponent<StarterAssets.ThirdPersonController>().SprintSpeed = currentHunter.SprintSpeed;
        }

        IEnumerator C_BlackoutStrike(int specialAttackID)
        {
            if (postProcessVolume == null)
            {
                postProcessVolume = GameObject.FindGameObjectWithTag("PostProcessVolume").GetComponent<PostProcessVolume>();
                postProcessVolume.profile.TryGetSettings(out vignette);
            }

            float targetIntensity = 0.737f; // The target intensity for the vignette
            float currentIntensity = 0.0f; // The current intensity of the vignette

            CmdBlackoutStrike(specialAttackID);

            while (currentIntensity < targetIntensity)
            {
                currentIntensity += intensityIncrement;
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            yield return new WaitForSeconds(currentHunter._specialAttacks[specialAttackID].BlackoutStrikeDuration);

            while (currentIntensity > 0.55f)
            {
                currentIntensity -= intensityIncrement; // Decrease intensity gradually
                vignette.intensity.value = Mathf.Clamp01(currentIntensity);
                yield return new WaitForSeconds(changeInterval);
            }

            // Ensure the vignette intensity reaches exactly 0.55f
            vignette.intensity.value = 0.55f;

            allVictims.Clear();

            NetworkIdentity[] networkIdentities = FindObjectsOfType<NetworkIdentity>();

            foreach (var networkIdentity in networkIdentities)
            {
                HunterAbilities networkPlayer = networkIdentity.GetComponent<HunterAbilities>();
                if (networkPlayer != null)
                {
                    if (!networkPlayer._isHunter)
                    {
                        allVictims.Add(networkPlayer);
                        foreach (GameObject xRayMesh in networkPlayer.SurvivorXRayMesh)
                            xRayMesh.SetActive(false);
                    }
                }
            }

            allVictims.Clear();
        }
        [Command]
        void CmdBlackoutStrike(int specialAttackID)
        {
            RpcBlackoutStrike(specialAttackID);
        }

        [ClientRpc]
        void RpcBlackoutStrike(int specialAttackID)
        {
            // Find all radio components in the scene
            RadioManager[] allRadios = FindObjectsOfType<RadioManager>();
            // Find all light components in the scene
            Light[] allLights = FindObjectsOfType<Light>();

            // Filter out the lights that belong to GameObjects named "FlashLight"
            Light[] filteredLights = allLights.Where(light => light.gameObject.name != "FlashLight").ToArray();

            // Play sound
            PlayClipAt(currentHunter._specialAttacks[specialAttackID].BlackoutStrikeAudioClip, transform.position, 1, 500, 500, 0, 1);

            foreach (RadioManager radio in allRadios)
            {
                radio.DisableRadio(true);
            }

            // Start flickering
            StartCoroutine(BlackoutStrikeStartFlickering(filteredLights, allRadios, specialAttackID));
        }

        IEnumerator BlackoutStrikeStartFlickering(Light[] lights, RadioManager[] radios, int specialAttackID)
        {
            // Start flickering
            yield return StartCoroutine(BlackoutStrikeFlickerLights(lights));

            // Turn off all lights
            foreach (Light light in lights)
            {
                light.enabled = false;
            }

            // Wait for blackoutDuration before turning the lights back on
            yield return new WaitForSeconds(20f); // Adjust the duration based on your blackout time

            // Turn on lights again
            TurnOnLights(lights);
            // Turn on radios again
            foreach (RadioManager radio in radios)
            {
                radio.DisableRadio(false);
            }
        }

        IEnumerator BlackoutStrikeFlickerLights(Light[] lights)
        {
            float flickerDuration = 4.65f; // Flicker duration in seconds

            // Flicker each light individually
            List<Coroutine> flickerCoroutines = new List<Coroutine>();
            foreach (Light light in lights)
            {
                // Flicker lights for the duration of flickerDuration with a random delay
                Coroutine coroutine = StartCoroutine(FlickerLight(light, flickerDuration, Random.Range(0f, 0.5f)));
                flickerCoroutines.Add(coroutine);
            }

            // Wait for all flickering to finish
            yield return new WaitForSeconds(flickerDuration + 0.5f);

            // Stop all flicker coroutines to ensure they don't interfere
            foreach (Coroutine coroutine in flickerCoroutines)
            {
                StopCoroutine(coroutine);
            }
        }

        IEnumerator FlickerLight(Light light, float flickerDuration, float delay)
        {
            yield return new WaitForSeconds(delay);

            float startTime = Time.time;

            while (Time.time - startTime < flickerDuration)
            {
                light.enabled = !light.enabled;
                yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            }

            // Ensure the light is off after flickering
            light.enabled = false;
        }

        void TurnOnLights(Light[] lights)
        {
            foreach (Light light in lights)
            {
                light.enabled = true;
            }
        }

        #endregion

        [Command]
        void CmdSetAbilitiesUsedValue(int value)
        {
            GetComponent<Player>().abilitiesUsed += value;
            GetComponent<CharacterManager>().TempAbilitiesUsed += value;

            RpcSetAbilitiesUsedValue(value);
        }

        [ClientRpc]
        void RpcSetAbilitiesUsedValue(int value)
        {
            GetComponent<Player>().SetAbilitiesUsedValue(value);
        }

        void ManageFinisherCamera(NetworkIdentity _victimPlayer, bool _showCamera)
        {
            if (_showCamera)
            {
                if (!isServer)
                    instantiatedShowFinisherCameraPrefab = Instantiate(showFinisherCameraPrefab, _victimPlayer.transform.position, _victimPlayer.transform.rotation);
            }
            else
            {
                if (!isServer)
                {
                    float _timeToDestroy;
                    if (!NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>()._isHunter)
                    {
                        if (NetworkClient.localPlayer.gameObject.GetComponent<ManageTPController>().isFirstPerson)
                            _timeToDestroy = 0;
                        else
                            _timeToDestroy = 2;
                    }
                    else
                        _timeToDestroy = 5;

                    Destroy(instantiatedShowFinisherCameraPrefab, _timeToDestroy);
                }
            }
        }

        [System.Serializable]
        public class HunterSpecialAttack
        {
            [HideInInspector] public float _abilityCooldownEnd;

            public float Cooldown = 30f;
            public SpecialAttackType specialAttackType;
            public Sprite specialAttackImage;

            [Space]
            [Header("Finisher Attack")]
            //here we specify the duration of each finisher, we do that to know when to unblock
            //movement for hunter, since we dont want to release him while he is playing killing animation
            public float FinisherDuration = 2f;

            public float FightDuration = 2f; //how long the survivor and the hunter have to gain superiority

            public string VictimFightAnimatorTriggerName;
            public string HunterFightAnimatorTriggerName;
            public string HunterKillAnimatorTriggerName;
            public string VictimDeathAnimatorTriggerName;
            public string HunterDisabledAnimatorTriggerName;
            public string VictimEscapedAnimatorTriggerName;

            public float HunterDisabledDuration = 4f;
            public float VictimEscapedDuration = 2.02f;
            public float VictimDeathDuration = 2.03f;

            [Space]
            [Header("Hunter's Vision")]
            public float HuntersVisionDuration = 7f;
            public AudioClip HuntersVisionAudioClip;

            [Space]
            [Header("Hunter's Instinct")]
            public float HuntersInstinctDuration = 12f;

            [Space]
            [Header("Rapid Rush")]
            public float RapidRushDuration = 5f;
            public float RapidRushSprintSpeed = 6.1f;

            [Space]
            [Tooltip("The Blackout Strike is a attack, aiming to shroud the surroundings in darkness by disabling all light sources.")]
            [Header("Blackout Strike")]
            public float BlackoutStrikeDuration = 12f;
            public AudioClip BlackoutStrikeAudioClip;

            public enum SpecialAttackType
            {
                HuntersVision,
                Finisher,
                HuntersInstinct,
                RapidRush,
                BlackoutStrike,
            }
        }

        [System.Serializable]
        public class Hunter
        {
            public string name = "Hunter";
            public int HunterID = 1;
            public Avatar HunterAvatar;
            public bool useAnimatorOverrideController = false;
            public AnimatorOverrideController AnimatorController;
            public GameObject[] HunterMesh;
            public float MoveSpeed = 1f;
            public float SprintSpeed = 3.5f;
            public float MovementMultiplier = 1.9f;
            public bool canSprint = false;
            public AudioClip ChaseMusic;
            public Transform itemParent;
            public HunterSpecialAttack[] _specialAttacks;
            public GameObject hunterWeaponItem;
        }
    }
}