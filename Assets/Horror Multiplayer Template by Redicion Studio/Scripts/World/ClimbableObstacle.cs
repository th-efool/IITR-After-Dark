// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class ClimbableObstacle : NetworkBehaviour
    {
        public Transform targetPosition;
        public Transform startPosition;
        public float teleportDelay = 2.10f;
        public string ClimbOverObstacleTriggerName = "ClimbOver";
        [HideInInspector] public bool canBeClimbedOver = true;

        [ClientRpc]
        void RpcSetPlayerCurrentClimbableObstacle(NetworkIdentity player, bool remove)
        {
            if (!remove)
                player.GetComponent<PlayerInteractionModule>().currentClimbableObstacle = GetComponent<NetworkIdentity>();
            else
                player.GetComponent<PlayerInteractionModule>().currentClimbableObstacle = null;
        }

        void OnTriggerEnter(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle == null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentClimbableObstacle(other.GetComponent<NetworkIdentity>(), false);
                }
                else if (other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle.netId != GetComponent<NetworkIdentity>().netId)
                {
                    other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle = GetComponent<NetworkIdentity>();
                    RpcSetPlayerCurrentClimbableObstacle(other.GetComponent<NetworkIdentity>(), false);
                }
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (isServer && other.GetComponent<PlayerInteractionModule>() != null)
            {
                if (other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle != null)
                {
                    other.GetComponent<PlayerInteractionModule>().currentClimbableObstacle = null;
                    RpcSetPlayerCurrentClimbableObstacle(other.GetComponent<NetworkIdentity>(), true);
                }
            }
        }
    }
}