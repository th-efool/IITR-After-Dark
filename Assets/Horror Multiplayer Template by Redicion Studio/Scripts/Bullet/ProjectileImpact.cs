// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ProjectileImpact : MonoBehaviour
    {
        public AudioSource audioSource;

        public AudioClip[] impactSounds;

        public bool detachFromParent = false;

        void Start()
        {
            if (detachFromParent)
                transform.parent = null;

            audioSource.clip = impactSounds[Random.Range(0, impactSounds.Length)];
            audioSource.Play();

            Destroy(this.gameObject, 10);
        }
    }
}
