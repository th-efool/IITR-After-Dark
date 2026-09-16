// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class LeverManager : NetworkBehaviour
    {
        public GeneratorManager generator;
        public Animator escapeDoorAnimator;
        [SyncVar] public bool serverUsed = false;

        private RedicionStudio.PlayerInputs _input;

        private void Update()
        {
            if (_input == null)
                _input = GameObject.FindGameObjectWithTag("InputManager").GetComponent<RedicionStudio.PlayerInputs>();

            if (generator == null)
            {
                GameObject generatorObject = GameObject.FindGameObjectWithTag("Generator");
                if (generatorObject != null)
                {
                    generator = generatorObject.GetComponent<GeneratorManager>();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isServer)
                return;

            if (!serverUsed && generator != null && other.CompareTag("Player"))
            {
                Player player = other.GetComponent<Player>();
                if (player != null && player.username == NetworkClient.localPlayer.gameObject.GetComponent<Player>().username && generator.health == 100)
                {
                    if (_input.interact)
                    {
                        NetworkClient.localPlayer.GetComponent<PlayerInteractionModule>().OpenEscapeDoor(NetworkClient.localPlayer.GetComponent<NetworkIdentity>(), generator.GetComponent<NetworkIdentity>(), GetComponent<NetworkIdentity>());
                    }
                }
            }
        }
    }
}