// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class PhoneReviver : InteractableObject
    {
        [Header("Phone Reviver")]
        [SerializeField] Material m_free;
        [SerializeField] Material m_inUse;
        [SerializeField] Material m_depleted;
        [SerializeField] MeshRenderer meshRenderer;
        public MeshRenderer telephoneReceiverMesh;

        protected override void ActivateObject()
        {
            base.ActivateObject();
            if (RoomManager._instance.ReviveRandomDeadPlayer()) //if there was a player to resurrect, then make this phone not avaible be decreasing useSupply from 1 to 0
            {
                UseSupply--;
                Rpc_UpdateObjectState(false, UseSupply); //let all players know that this object is no longer able to revive anyone
            }
        }
        protected override void UpdateObjectState(bool inUse, int supply)
        {
            base.UpdateObjectState(inUse, supply);

            if (inUse)
            {
                meshRenderer.material = m_inUse;
            }
            else
            {
                meshRenderer.material = UseSupply > 0 ? m_free : m_depleted;
            }
        }
    }
}