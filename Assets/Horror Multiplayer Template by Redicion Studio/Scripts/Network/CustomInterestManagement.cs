// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using Mirror;
using System.Collections.Generic;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class CustomInterestManagement : InterestManagement
    {

#if UNITY_SERVER || UNITY_EDITOR
        public float visRange = 30f;

        private bool Check(NetworkIdentity identity, NetworkConnection observer)
        {
            Player observerPlayer = observer.identity.GetComponent<Player>();
            return (identity.TryGetComponent(out Instance instance) && instance.uniqueName == observerPlayer.instance.uniqueName) ||
                Vector3.Distance(identity.transform.position, observer.identity.transform.position) <= visRange;
        }

        public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver)
        {
            return Check(identity, newObserver);
        }

        public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize)
        {
            foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
            {
                if (conn != null && conn.isAuthenticated && conn.identity != null && Check(identity, conn))
                {
                    newObservers.Add(conn);
                }
            }
        }

        [SerializeField] private double _rebuildInterval = 4.0;
        private static double _lastRebuildTime;

        private void Update()
        {
            if (NetworkTime.time - _lastRebuildTime >= _rebuildInterval)
            {
                RebuildAll();
                _lastRebuildTime = NetworkTime.time;
            }
        }
#else
	public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnection newObserver) { return false; }

	public override void OnRebuildObservers(NetworkIdentity identity, HashSet<NetworkConnection> newObservers, bool initialize) { }
#endif
    }
}