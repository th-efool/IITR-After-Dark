// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio;

namespace RedicionStudio
{
    [System.Serializable]
    public class DoorSettings
    {
        public string PlayerOpenDoorTriggerName = "Throw";
        public string PlayerUnlockingDoorTriggerName = "UnlockingDoor";
        public Transform unlockPlayerPosition;
        public Animator anim;
        public string OpenDoorTriggerName = "OpenDoor";
        public string CloseDoorTriggerName = "CloseDoor";
        public float unlockingDuration = 10f;
        public MeshCollider meshCollider;
        public float doorAnimationLength = 1f;
        public AudioSource audioSource;
        public GameObject destructibleDoorPrefab;
        public Transform destructibleDoorPosition;
        public GameObject[] doorMesh;
        public GameObject[] doorDamagedMesh;
    }

    public class OpenableDoorManager : NetworkBehaviour
    {
        public GameObject openableDoorFrontPrefab;
        public GameObject openableDoorBackPrefab;

        public Transform doorFrontSpawnPosition;
        public Transform doorBackSpawnPosition;

        public OpenableDoor openableDoorFront;
        public OpenableDoor openableDoorBack;

        [Header("Front Door Settings")]
        public DoorSettings frontDoorSettings;

        [Header("Back Door Settings")]
        public DoorSettings backDoorSettings;

        [ClientRpc]
        private void RpcApplyDoorSettings(NetworkIdentity _openableDoorBack, NetworkIdentity _openableDoorFront)
        {
            openableDoorFront = _openableDoorFront.GetComponent<OpenableDoor>();
            openableDoorBack = _openableDoorBack.GetComponent<OpenableDoor>();

            openableDoorFront.associatedOpenableDoor = _openableDoorBack.GetComponent<OpenableDoor>();
            openableDoorBack.associatedOpenableDoor = _openableDoorFront.GetComponent<OpenableDoor>();

            openableDoorFront.doorId = 0;
            openableDoorBack.doorId = 1;

            openableDoorFront.openableDoorManager = GetComponent<NetworkIdentity>();
            openableDoorBack.openableDoorManager = GetComponent<NetworkIdentity>();

            openableDoorFront.SetUpDoorClient();
            openableDoorBack.SetUpDoorClient();
        }

        public void SetUpDoors()
        {
            if(isServer)
            {
                GameObject doorFront = Instantiate(openableDoorFrontPrefab, doorFrontSpawnPosition.position, doorFrontSpawnPosition.rotation);
                NetworkServer.Spawn(doorFront);
                openableDoorFront = doorFront.GetComponent<OpenableDoor>();

                GameObject doorBack = Instantiate(openableDoorBackPrefab, doorBackSpawnPosition.position, doorBackSpawnPosition.rotation);
                NetworkServer.Spawn(doorBack);
                openableDoorBack = doorBack.GetComponent<OpenableDoor>();

                openableDoorFront.associatedOpenableDoor = openableDoorBack;
                openableDoorBack.associatedOpenableDoor = openableDoorFront;

                openableDoorFront.doorId = 0;
                openableDoorFront.doorId = 1;

                openableDoorFront.openableDoorManager = GetComponent<NetworkIdentity>();
                openableDoorBack.openableDoorManager = GetComponent<NetworkIdentity>();

                RpcApplyDoorSettings(openableDoorBack.GetComponent<NetworkIdentity>(), openableDoorFront.GetComponent<NetworkIdentity>());

                openableDoorFront.SetUpDoorServer();
                openableDoorBack.SetUpDoorServer();
            }
        }
    }
}
