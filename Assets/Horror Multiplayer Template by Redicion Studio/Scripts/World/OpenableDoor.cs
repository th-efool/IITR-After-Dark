// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class OpenableDoor : NetworkBehaviour
    {
        public string PlayerOpenDoorTriggerName = "Throw";
        public string PlayerUnlockingDoorTriggerName = "UnlockingDoor";

        public Transform unlockPlayerPosition;

        public Animator anim;
        public string OpenDoorTriggerName = "OpenDoor";
        public string CloseDoorTriggerName = "CloseDoor";

        [SyncVar] public bool isDoorLocked = false;

        [SyncVar] public bool isDoorOpen = false;

        [SyncVar] public bool isBeingUnlocked = false;

        [SyncVar] public bool isDoorDestroyed = false;

        [SyncVar] public bool canDamageDoor = true;

        public float unlockingDuration = 10f;

        public MeshCollider meshCollider;

        public float doorAnimationLength = 1f;

        public AudioClip openDoorAudio;
        public AudioClip closeDoorAudio;
        public AudioClip doorUnlockedAudio;
        public AudioClip doorUnlockingLoopAudio;
        public AudioSource audioSource;

        public OpenableDoor associatedOpenableDoor;

        public GameObject destructibleDoorPrefab;
        public GameObject instantiatedDestructibleDoorPrefab;
        public Transform destructibleDoorPosition;

        public GameObject[] doorMesh;

        public GameObject[] doorDamagedMesh;

        [SyncVar] public int doorHealth = 100;

        [SyncVar] public bool reinstated = false;

        public RoomManager roomManager;

        public NetworkIdentity openableDoorManager;

        public int doorId;

        bool doorSetUpOnClient = false;

        [ClientRpc]
        public void RpcOpenDoor()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isDoorOpen = true;
            isDoorOpen = true;
            meshCollider.enabled = false;
            anim.SetTrigger(OpenDoorTriggerName);
            StartCoroutine(CoroutineDoorAnimation());
            audioSource.loop = false;
            audioSource.clip = openDoorAudio;
            audioSource.Play();
        }

        [ClientRpc]
        public void RpcCloseDoor()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isDoorOpen = false;
            isDoorOpen = false;
            meshCollider.enabled = false;
            anim.SetTrigger(CloseDoorTriggerName);
            StartCoroutine(CoroutineDoorAnimation());
            audioSource.loop = false;
            audioSource.clip = closeDoorAudio;
            audioSource.Play();
        }

        IEnumerator CoroutineDoorAnimation()
        {
            yield return new WaitForSeconds(doorAnimationLength);

            if (!doorSetUpOnClient)
                SetUpDoorClient();

            meshCollider.enabled = true;
        }

        [ClientRpc]
        public void RpcPlayUnlockingDoorLoopAudio()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            audioSource.loop = true;
            audioSource.clip = doorUnlockingLoopAudio;
            audioSource.Play();
        }

        [ClientRpc]
        public void RpcStopUnlockingDoorLoopAudio()
        {
            audioSource.Stop();
        }

        [ClientRpc]
        public void RpcUnlockDoor()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isDoorLocked = false;
            isDoorLocked = false;
            audioSource.loop = false;
            audioSource.clip = doorUnlockedAudio;
            audioSource.Play();
        }

        [ClientRpc]
        public void RpcLockDoor()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isDoorLocked = true;
            isDoorLocked = true;
        }

        [ClientRpc]
        public void RpcSetBeingUnlocked()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isBeingUnlocked = true;
            isBeingUnlocked = true;
        }

        [ClientRpc]
        public void RpcEndBeingUnlocked()
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (associatedOpenableDoor != null)
                associatedOpenableDoor.isBeingUnlocked = false;
            isBeingUnlocked = false;
        }

        [ClientRpc]
        void RpcSetPlayerCurrentOpenableDoor(NetworkIdentity player, bool remove)
        {
            if (!remove)
                player.GetComponent<PlayerInteractionModule>().currentOpenableDoor = GetComponent<NetworkIdentity>();
            else
            {
                if (player.GetComponent<PlayerInteractionModule>().currentOpenableDoor.netId.Equals(GetComponent<NetworkIdentity>().netId))
                {
                    player.GetComponent<PlayerInteractionModule>().currentOpenableDoor = null;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null && !isDoorDestroyed)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentOpenableDoor == null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentOpenableDoor = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentOpenableDoor(other.GetComponent<NetworkIdentity>(), false);
                }
                else if (other.GetComponent<PlayerInteractionModule>().currentOpenableDoor.netId != GetComponent<NetworkIdentity>().netId)
                {
                    other.GetComponent<PlayerInteractionModule>().currentOpenableDoor = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentOpenableDoor(other.GetComponent<NetworkIdentity>(), false);
                }
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null && !isDoorDestroyed)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentOpenableDoor != null && other.GetComponent<PlayerInteractionModule>().currentOpenableDoor.netId.Equals(GetComponent<NetworkIdentity>().netId))
                {
                    other.GetComponent<PlayerInteractionModule>().currentOpenableDoor = null;
                    RpcSetPlayerCurrentOpenableDoor(other.GetComponent<NetworkIdentity>(), true);
                }
            }
        }

        public void ServerDamageDoor(int demage, float cooldown)
        {
            if (isServer)
            {
                if (doorHealth > 0 && canDamageDoor)
                {
                    canDamageDoor = false;
                    StartCoroutine(SetDoorDamageable(cooldown));
                    doorHealth -= demage;

                    if (doorHealth <= 0)
                    {
                        doorHealth = 0;
                        //RpcDemageDoor(demage);
                        isDoorDestroyed = true;

                        GameObject _destructibleDoorPrefab = Instantiate(destructibleDoorPrefab, destructibleDoorPosition.position, destructibleDoorPosition.rotation);

                        NetworkServer.Spawn(_destructibleDoorPrefab);

                        instantiatedDestructibleDoorPrefab = _destructibleDoorPrefab;

                        RpcDestroyDoor(true);
                        RpcSyncInstantiatedDestructibleDoor(true, instantiatedDestructibleDoorPrefab.GetComponent<NetworkIdentity>());
                    }
                }
            }
        }

        IEnumerator SetDoorDamageable(float cooldown)
        {
            yield return new WaitForSeconds(cooldown);
            canDamageDoor = true;
        }


        [ClientRpc]
        public void RpcDamageDoor(int demage)
        {
            doorHealth -= demage;
        }

        public void ServerDestroyDoor()
        {
            if (isServer)
            {
                isDoorDestroyed = true;

                GameObject _destructibleDoorPrefab = Instantiate(destructibleDoorPrefab, destructibleDoorPosition.position, destructibleDoorPosition.rotation);

                NetworkServer.Spawn(_destructibleDoorPrefab);

                instantiatedDestructibleDoorPrefab = _destructibleDoorPrefab;

                RpcDestroyDoor(true);
                RpcSyncInstantiatedDestructibleDoor(true, instantiatedDestructibleDoorPrefab.GetComponent<NetworkIdentity>());
            }
        }

        [ClientRpc]
        public void RpcDestroyDoor(bool status)
        {
            isDoorDestroyed = status;
        }

        [ClientRpc]
        public void RpcSyncInstantiatedDestructibleDoor(bool status, NetworkIdentity destroyedDoorNetId)
        {
            if(status && destroyedDoorNetId != null)
            {
                instantiatedDestructibleDoorPrefab = destroyedDoorNetId.gameObject;
            }
        }

        private void Update()
        {
            if (roomManager == null && GameObject.FindGameObjectWithTag("RoomManager") != null)
                roomManager = GameObject.FindGameObjectWithTag("RoomManager").GetComponent<RoomManager>();

            if (!doorSetUpOnClient)
            {
                SetUpDoorClient();

                return;
            }

            if (isDoorDestroyed)
            {
                foreach (GameObject _doorMesh in doorMesh)
                {
                    if(_doorMesh != null)
                        _doorMesh.SetActive(false);
                }
            }
            else
            {
                if (doorHealth < 100 && doorHealth != 0)
                {
                    foreach (GameObject _doorMesh in doorMesh)
                    {
                        _doorMesh.SetActive(false);
                    }
                    foreach (GameObject _doorDamagedMesh in doorDamagedMesh)
                    {
                        _doorDamagedMesh.SetActive(true);
                    }
                }
            }

            /*if (isServer && roomManager != null && reinstated && roomManager.MatchEnding)
            {
                reinstated = false;
                RpcRestoreDoor(false);
            }
            else if (isServer && roomManager != null && !reinstated && roomManager.MatchRunning && !roomManager.MatchEnding)
            {
                reinstated = true;
                doorHealth = 100;
                RpcRestoreDoor(true);
                isDoorDestroyed = false;
                RpcDestroyDoor(false);
                isDoorLocked = true;
                RpcLockDoor();
                RpcCloseDoor();
                if (instantiatedDestructibleDoorPrefab != null)
                {
                    NetworkServer.Destroy(instantiatedDestructibleDoorPrefab);
                }

            }*/
        }

        public void SetUpDoorClient()
        {
            if (!isServer)
            {
                if (doorId == 0)
                {
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.PlayerOpenDoorTriggerName;
                    PlayerUnlockingDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.PlayerUnlockingDoorTriggerName;
                    unlockPlayerPosition = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.unlockPlayerPosition;
                    anim = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.anim;
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.OpenDoorTriggerName;
                    CloseDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.CloseDoorTriggerName;
                    unlockingDuration = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.unlockingDuration;
                    meshCollider = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.meshCollider;
                    doorAnimationLength = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorAnimationLength;
                    audioSource = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.audioSource;
                    destructibleDoorPrefab = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.destructibleDoorPrefab;
                    destructibleDoorPosition = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.destructibleDoorPosition;
                    doorMesh = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorMesh;
                    doorDamagedMesh = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorDamagedMesh;
                }
                else
                {
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.PlayerOpenDoorTriggerName;
                    PlayerUnlockingDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.PlayerUnlockingDoorTriggerName;
                    unlockPlayerPosition = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.unlockPlayerPosition;
                    anim = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.anim;
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.OpenDoorTriggerName;
                    CloseDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.CloseDoorTriggerName;
                    unlockingDuration = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.unlockingDuration;
                    meshCollider = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.meshCollider;
                    doorAnimationLength = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorAnimationLength;
                    audioSource = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.audioSource;
                    destructibleDoorPrefab = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.destructibleDoorPrefab;
                    destructibleDoorPosition = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.destructibleDoorPosition;
                    doorMesh = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorMesh;
                    doorDamagedMesh = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorDamagedMesh;
                }

                doorSetUpOnClient = true;
            }
        }

        public void SetUpDoorServer()
        {
            if(isServer)
            {
                if(doorId == 0)
                {
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.PlayerOpenDoorTriggerName;
                    PlayerUnlockingDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.PlayerUnlockingDoorTriggerName;
                    unlockPlayerPosition = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.unlockPlayerPosition;
                    anim = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.anim;
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.OpenDoorTriggerName;
                    CloseDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.CloseDoorTriggerName;
                    unlockingDuration = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.unlockingDuration;
                    meshCollider = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.meshCollider;
                    doorAnimationLength = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorAnimationLength;
                    audioSource = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.audioSource;
                    destructibleDoorPrefab = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.destructibleDoorPrefab;
                    destructibleDoorPosition = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.destructibleDoorPosition;
                    doorMesh = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorMesh;
                    doorDamagedMesh = openableDoorManager.GetComponent<OpenableDoorManager>().frontDoorSettings.doorDamagedMesh;
                }
                else
                {
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.PlayerOpenDoorTriggerName;
                    PlayerUnlockingDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.PlayerUnlockingDoorTriggerName;
                    unlockPlayerPosition = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.unlockPlayerPosition;
                    anim = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.anim;
                    OpenDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.OpenDoorTriggerName;
                    CloseDoorTriggerName = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.CloseDoorTriggerName;
                    unlockingDuration = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.unlockingDuration;
                    meshCollider = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.meshCollider;
                    doorAnimationLength = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorAnimationLength;
                    audioSource = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.audioSource;
                    destructibleDoorPrefab = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.destructibleDoorPrefab;
                    destructibleDoorPosition = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.destructibleDoorPosition;
                    doorMesh = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorMesh;
                    doorDamagedMesh = openableDoorManager.GetComponent<OpenableDoorManager>().backDoorSettings.doorDamagedMesh;
                }

                reinstated = true;
                doorHealth = 100;
                RpcRestoreDoor(true);
                isDoorDestroyed = false;
                RpcDestroyDoor(false);
                isDoorLocked = true;
                RpcLockDoor();
                RpcCloseDoor();
                if (instantiatedDestructibleDoorPrefab != null)
                {
                    NetworkServer.Destroy(instantiatedDestructibleDoorPrefab);
                }
            }
        }

        [ClientRpc]
        public void RpcRestoreDoor(bool status)
        {
            if (!doorSetUpOnClient)
                SetUpDoorClient();

            if (status)
            {
                doorHealth = 100;
                anim.SetTrigger(CloseDoorTriggerName);
            }
            reinstated = status;
            foreach (GameObject _doorMesh in doorMesh)
            {
                _doorMesh.SetActive(true);
            }
            foreach (GameObject _doorDamagedMesh in doorDamagedMesh)
            {
                _doorDamagedMesh.SetActive(false);
            }
            if (instantiatedDestructibleDoorPrefab != null)
            {
                Destroy(instantiatedDestructibleDoorPrefab);
            }
        }
    }
}