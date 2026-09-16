// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio.BSystem
{
    public class DoorManager : NetworkBehaviour
    {
        [Header("Door Manager")]
        [SyncVar] bool isDoorOpen = false;
        public Transform door;
        public Animator animator;
        public string doorOpenTriggerName = "OpenDoor";
        public string doorCloseTriggerName = "CloseDoor";

        private void OnTriggerStay(Collider other)
        {
            if (other != null)
            {
                if (other.tag == "Player")
                {
                    if (!isDoorOpen)
                    {
                        isDoorOpen = true;
                        animator.ResetTrigger(doorCloseTriggerName);
                        animator.SetTrigger(doorOpenTriggerName);
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null)
            {
                if (other.tag == "Player")
                {
                    if (isDoorOpen)
                    {
                        isDoorOpen = false;
                        animator.ResetTrigger(doorOpenTriggerName);
                        animator.SetTrigger(doorCloseTriggerName);
                    }
                }
            }
        }
    }
}
