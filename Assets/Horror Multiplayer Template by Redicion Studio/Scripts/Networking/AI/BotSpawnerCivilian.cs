// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedicionStudio.VehicleEnterExit;

namespace RedicionStudio
{
    public class BotSpawnerCivilian : NetworkBehaviour
    {
        [SerializeField] GameObject _playerPrefab;
        PlayerAI _spawnedBot;

        [SerializeField] public VehicleSync _targetedVehicle;
        [SerializeField] public Transform _targetedWaypoint;
        [HideInInspector] public Instance instance;

        private void Update()
        {
            if (!isServer)
                return;

            if (_spawnedBot != null)
                return;

            _spawnedBot = Instantiate(_playerPrefab, transform.position, transform.rotation).GetComponent<PlayerAI>();
            NetworkServer.Spawn(_spawnedBot.gameObject);

            instance.instancedServerWorldObjects.Add(_spawnedBot.gameObject);

            if (_targetedVehicle)
            {
                _spawnedBot.SetAsBot();
                _spawnedBot.GetInTheVehicle(_targetedVehicle);
            }
            else
            {
                _spawnedBot.SetAsBot();
                _spawnedBot.SetWaypoint(_targetedWaypoint, "Walking");
            }
            Debug.Log(gameObject.name + " has instantiated " + "'" + _spawnedBot.gameObject.name + "'");
        }
    }
}