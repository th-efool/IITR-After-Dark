// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class Instance : NetworkBehaviour
    {

        public const int MaxNumOfPlayersPerInstance = 5;
        public const int MaxNumOfInstances = 1;

        [HideInInspector] public string uniqueName;

        [SerializeField] private VehicleEntry[] vehicles;
        [SerializeField] private WorldObjectEntry[] worldObjects;
        [SerializeField] private NpcEntry[] npcs;
        [SerializeField] private GameObject botSpawnerPrefab;

        [HideInInspector] public List<GameObject> instancedServerWorldObjects;

        // Dict<uniqueName, Instance>
        public static Dictionary<string, Instance> instances = new Dictionary<string, Instance>();

        private static readonly Vector3 _boundsSize = new Vector3(130.0f, 130.0f, 1500.0f);
        [Server]
        public static Instance Create(string uniqueName)
        {
            GameObject gO = Instantiate(((CustomNetManager)NetworkManager.singleton).instancePrefab);
            gO.transform.position = Vector3.forward * _boundsSize.z * instances.Count;
            Instance instance = gO.GetComponent<Instance>();
            instance.uniqueName = uniqueName;
            NetworkServer.Spawn(gO);
            instance.CreateWorldObjects(instance);
            instances[uniqueName] = instance;
            return instance;
        }

        public Dictionary<int, RedicionStudio.InventorySystem.Player> players = new Dictionary<int, RedicionStudio.InventorySystem.Player>();

        [Server]
        public void CreateWorldObjects(Instance _instance)
        {
            foreach (VehicleEntry vehicle in _instance.vehicles)
            {
                GameObject _Vehicle = Instantiate(vehicle.vehiclePrefab, vehicle.vehicleSpawnPoint.position, vehicle.vehicleSpawnPoint.rotation) as GameObject;

                NetworkServer.Spawn(_Vehicle);

                if (vehicle.vehicleWayPoint != null)
                    _Vehicle.GetComponent<CarAI>().currentWaypoint = vehicle.vehicleWayPoint;

                if (vehicle.isNpc)
                {
                    GameObject _Npc = Instantiate(_instance.botSpawnerPrefab, vehicle.vehicleNpcSpawnPoint.position, vehicle.vehicleNpcSpawnPoint.rotation) as GameObject;

                    NetworkServer.Spawn(_Npc);

                    _Npc.GetComponent<BotSpawnerCivilian>()._targetedVehicle = _Vehicle.GetComponent<VehicleEnterExit.VehicleSync>();
                    _Npc.GetComponent<BotSpawnerCivilian>().instance = _instance;
                    instancedServerWorldObjects.Add(_Npc);
                    Debug.Log("World object " + "'" + _Npc.name + "'" + " instantiated");
                }
                instancedServerWorldObjects.Add(_Vehicle);
                Debug.Log("World object " + "'" + _Vehicle.name + "'" + " instantiated");
            }
            foreach (WorldObjectEntry worldObject in _instance.worldObjects)
            {
                GameObject _WorldObject = Instantiate(worldObject.worldObjectPrefab, worldObject.worldObjectSpawnPoint.position, worldObject.worldObjectSpawnPoint.rotation) as GameObject;

                NetworkServer.Spawn(_WorldObject);

                instancedServerWorldObjects.Add(_WorldObject);
                Debug.Log("World object " + "'" + _WorldObject.name + "'" + " instantiated");
            }
            foreach (NpcEntry npc in _instance.npcs)
            {
                GameObject _Npc = Instantiate(npc.npcPrefab, npc.npcSpawnPoint.position, npc.npcSpawnPoint.rotation) as GameObject;

                NetworkServer.Spawn(_Npc);

                _Npc.GetComponent<BotSpawnerCivilian>()._targetedWaypoint = npc.npcWayPoint;
                _Npc.GetComponent<BotSpawnerCivilian>().instance = _instance;
                instancedServerWorldObjects.Add(_Npc);
                Debug.Log("World object " + "'" + _Npc.name + "'" + " instantiated");
            }
            Debug.Log("Instanced world objects: " + instancedServerWorldObjects.Count);
        }

        [Server]
        public void AddPlayer(int id, RedicionStudio.InventorySystem.Player player)
        {
            players[id] = player;
        }

        [Server]
        public void RemovePlayer(int id)
        {
            _ = players.Remove(id);
            if (players.Count < 1)
            {
                RoomManager._instance.isCurrentSelectedMatchMapIdSet = false;
                _ = instances.Remove(uniqueName);
                foreach (GameObject worldObject in instancedServerWorldObjects)
                {
                    if (worldObject != null)
                        NetworkServer.Destroy(worldObject);
                }
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    [System.Serializable]
    public class VehicleEntry
    {
        public string name = "Vehicle0";
        [Space]
        public GameObject vehiclePrefab;
        [Space]
        public Transform vehicleSpawnPoint;
        [Space]
        public Transform vehicleWayPoint;
        [Space]
        public bool isNpc = false;
        [Space]
        public Transform vehicleNpcSpawnPoint;
    }

    [System.Serializable]
    public class WorldObjectEntry
    {
        public string name = "WorldObject0";
        [Space]
        public GameObject worldObjectPrefab;
        [Space]
        public Transform worldObjectSpawnPoint;
    }

    [System.Serializable]
    public class NpcEntry
    {
        public string name = "Npc0";
        [Space]
        public GameObject npcPrefab;
        [Space]
        public Transform npcSpawnPoint;
        [Space]
        public Transform npcWayPoint;
    }
}