// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class PlayerAiSensor : MonoBehaviour
    {
        public Animator playerAnimator;
        public PlayerAI playerAi;
        public Transform currentPlayer;

        private void OnTriggerStay(Collider other)
        {
            if (!playerAi.HasHandsUp)
            {
                if (playerAi.isSetAsAi)
                {
                    if (other.tag == "Player")
                    {
                        if (other.GetComponent<PlayerAI>() != null)
                        {
                            if (!other.GetComponent<PlayerAI>().isSetAsAi)
                            {
                                if (!other.GetComponent<RedicionStudio.InventorySystem.PlayerInventoryModule>().inCar)
                                {
                                    currentPlayer = other.transform;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}