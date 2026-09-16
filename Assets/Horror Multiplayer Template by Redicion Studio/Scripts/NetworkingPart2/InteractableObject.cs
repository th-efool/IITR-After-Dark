// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class InteractableObject : NetworkBehaviour
    {
        [Header("InteractableObject")]
        [Tooltip("How much time do we have to hold interaction key to activate object")]
        public float TimeNeededToActivate = 10f;

        float _timeWhenActivate;

        //we have to know if object is already in use to not let others use it
        bool _beingInteracted;

        [SerializeField] int team = 0; //if player is in different team than this team, then dont let him use this


        public int UseSupply = 1;

        public Transform userPosition;

        //server event to notify observing objects (like player) that this object is activated
        public delegate void Used();
        public Used InteractableObject_ServerEvent_Activated;

        private void Start()
        {
            RoomManager._instance.RoomEvent_NewRound += ResetObject;

            UpdateObjectState(_beingInteracted, UseSupply);
        }

        public virtual void StartInteraction()
        {
            _timeWhenActivate = Time.time + TimeNeededToActivate;

            _beingInteracted = true;

            Rpc_UpdateObjectState(true, UseSupply);
        }
        public virtual void EndInteraction()
        {
            _beingInteracted = false;

            Rpc_UpdateObjectState(false, UseSupply);
        }

        #region activate/reset
        //when interaction bar is filled
        protected virtual void ActivateObject()
        {

        }
        protected virtual void ResetObject()
        {
            UseSupply = 1;
            Rpc_UpdateObjectState(false, 1);
        }
        #endregion

        [ClientRpc]
        protected void Rpc_UpdateObjectState(bool inUse, int supply)
        {
            UseSupply = supply;
            UpdateObjectState(inUse, supply);
        }
        protected virtual void UpdateObjectState(bool inUse, int supply)
        {

        }

        public virtual bool AbleToBeInteracted(int interactorTeam)
        {
            return UseSupply > 0 && !_beingInteracted && interactorTeam == team;
        }

        private void Update()
        {
            if (!isServer) return;

            if (_beingInteracted && _timeWhenActivate <= Time.time)
            {
                ActivateObject();
                InteractableObject_ServerEvent_Activated?.Invoke();
                _beingInteracted = false;
            }
        }
    }
}