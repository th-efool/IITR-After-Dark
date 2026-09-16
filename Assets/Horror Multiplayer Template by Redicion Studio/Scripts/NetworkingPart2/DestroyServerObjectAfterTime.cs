// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class DestroyServerObjectAfterTime : NetworkBehaviour
    {
        public float lifetime = 5f; // Lifetime of the object in seconds

        private float elapsedTime = 0f;

        void Start()
        {
            if (isServer)
            {
                // Start the timer
                StartCoroutine(StartTimer());
            }
        }

        private IEnumerator StartTimer()
        {
            while (elapsedTime < lifetime)
            {
                yield return null; // Wait for one frame
                elapsedTime += Time.deltaTime;
            }

            // Destroy the network object after the lifetime has passed
            NetworkServer.Destroy(gameObject);
        }
    }
}
