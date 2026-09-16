// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class ToolboxManager : GameplayItem
    {
        private Coroutine updateCoroutine;

        public bool readyToRepair = false;

        public float repairPerFrameValue = 0.1f;

        public float updateInterval = 0.1f;

        public override void Putdown()
        {
            readyToRepair = false;
            base.Putdown();
        }

        private void Start()
        {
            if (updateCoroutine == null)
            {
                updateCoroutine = StartCoroutine(c_Update());
            }
        }

        private IEnumerator c_Update()
        {
            while (true)
            {
                M_Update();

                yield return new WaitForSeconds(updateInterval);
            }
        }

        private void M_Update()
        {
            if (hasAuthority && !_myOwner.GetComponent<HunterAbilities>()._inFight)
            {
                if (_input.use)
                {
                    if (!readyToRepair)
                    {
                        readyToRepair = true;
                        CmdSetInUse(true);
                    }
                }
                else
                {
                    if (readyToRepair)
                    {
                        readyToRepair = false;
                        CmdSetInUse(false);
                    }
                }
            }
        }

        [Command]
        private void CmdSetInUse(bool value)
        {
            readyToRepair = value;
            RpcUpdateInUse(value);
        }

        [ClientRpc]
        private void RpcUpdateInUse(bool value)
        {
            readyToRepair = value;
        }

        private void OnDestroy()
        {
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }
    }
}