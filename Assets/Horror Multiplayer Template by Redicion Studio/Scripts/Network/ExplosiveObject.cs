// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

namespace RedicionStudio
{
    public class ExplosiveObject : NetworkBehaviour
    {
        public GameObject hitEffect;
        public GameObject explosionEffect;
        public GameObject defaultMesh;
        public GameObject destroyedMesh;
        public UnityEvent onExplosion;
        bool isExploded = false;
        public const int maxHealth = 100;
        public float explosionRadius = 10.0f;
        public int explosionDemage = 100;
        [SyncVar(hook = nameof(OnChangeHealth))] public int currentHealth = maxHealth;
        [Header("Dependencies")]
        public ExplosionRadius explosionRadiusObject;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<NetworkBullet>() != null)
            {
                GameObject hit = collision.gameObject;

                ContactPoint contact = collision.contacts[0];
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;
                SpawnHitEffect(pos, rot);

                if (maxHealth != 0 && currentHealth != 0)
                {
                    TakeDamage(50);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (!isServer)
            {
                return;
            }

            currentHealth -= amount;
        }

        void Update()
        {
            if (isExploded == false)
            {
                if (currentHealth <= 0)
                {
                    isExploded = true;
                    currentHealth = 0;
                    SetExplosion();
                }
            }
        }

        void OnChangeHealth(int currenthealth, int health)
        {
            currentHealth = health;
        }

        void SpawnHitEffect(Vector3 _position, Quaternion _rotation)
        {
            Instantiate(hitEffect, _position, _rotation);
        }

        void SetExplosion()
        {
            //explosionRadiusObject.checkPlayersInRadius = true;
            destroyedMesh.SetActive(true);
            defaultMesh.SetActive(false);
            Instantiate(explosionEffect, this.transform);
            onExplosion.Invoke();
        }
    }
}