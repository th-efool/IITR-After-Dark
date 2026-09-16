// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class DroppedMoney : NetworkBehaviour
    {
        [SyncVar] public int moneyAmount = 10;
        [SyncVar] public bool collected = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!collected & other.tag == "Player" && other.GetComponent<Player>())
            {
                if (isServer)
                {
                    collected = true;
                    other.GetComponent<PlayerInteractionModule>().AddMoney(other.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>(), moneyAmount);

                    NetworkServer.Destroy(gameObject);
                }
            }
        }
    }
}