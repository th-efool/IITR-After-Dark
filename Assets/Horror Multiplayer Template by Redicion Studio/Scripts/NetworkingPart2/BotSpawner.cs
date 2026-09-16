// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class BotSpawner : NetworkBehaviour
    {
        [SerializeField] KeyCode _spawnButton = KeyCode.M;
        [SerializeField] GameObject _prefab;
        GameObject _myBot;

        // Update is called once per frame
        void Update()
        {
            if (!isServer) return;

            if (Input.GetKey(_spawnButton))
            {
                if (_myBot)
                    NetworkServer.Destroy(_myBot);

                _myBot = Instantiate(_prefab, transform.position, transform.rotation);
                _myBot.GetComponent<HunterAbilities>().bot = true;
                NetworkServer.Spawn(_myBot);
            }
        }
    }
}
