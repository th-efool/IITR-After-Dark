// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class NetworkBullet : Projectile
    {
        /*public bool isRocket = false;
        public GameObject rocketExplosion;

        [Space]
        public int bulletDemage = 10;

        [Space]
        public GameObject MuzzleFlashEffect;

        public GameObject metalHitEffect;
        public GameObject sandHitEffect;
        public GameObject stoneHitEffect;
        public GameObject[] fleshHitEffects;
        public GameObject woodHitEffect;

        [Space]
        public GameObject killMessagePrefab;

        void Start()
        {
            GameObject _MuzzleFlashEffect = Instantiate(MuzzleFlashEffect, this.transform.position, this.transform.rotation) as GameObject;

            StartCoroutine(DestroyBullet());
        }

        void OnCollisionEnter(Collision collision)
        {
            GameObject hit = collision.gameObject;
            Health health = hit.GetComponent<Health>();

            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;

            if (health != null)
            {
                if(hit.GetComponent<Health>().currentHealth < bulletDemage || hit.GetComponent<Health>().currentHealth == bulletDemage)
                {
                    Instantiate(killMessagePrefab).GetComponent<KillMessage>().ShowKillMessage(hit.GetComponent<Health>().attackerUsername, hit.GetComponent<Player>().username);
                }

                health.TakeDamage(bulletDemage);
            }

            if(isRocket == true)
            {
                rocketExplosion.SetActive(true);
                rocketExplosion.transform.parent = null;
            }

            HandleHit(hit, pos, rot);
        }

        void HandleHit(GameObject hit, Vector3 _position, Quaternion _rotation)
        {
            if (hit.GetComponent<MaterialIdentifier>() != null)
            {
                string materialName = hit.GetComponent<MaterialIdentifier>().material.ToString();
                Debug.Log(materialName);

                switch (materialName)
                {
                    case "Metal":
                        SpawnDecal(metalHitEffect, _position, _rotation);
                        break;
                    case "Sand":
                        SpawnDecal(sandHitEffect, _position, _rotation);
                        break;
                    case "Stone":
                        SpawnDecal(stoneHitEffect, _position, _rotation);
                        break;
                    case "Wood":
                        SpawnDecal(woodHitEffect, _position, _rotation);
                        break;
                    case "Meat":
                        SpawnDecal(fleshHitEffects[Random.Range(0, fleshHitEffects.Length)], _position, _rotation);
                        break;
                    case "Character":
                        SpawnDecal(fleshHitEffects[Random.Range(0, fleshHitEffects.Length)], _position, _rotation);
                        break;
                }
            }
        }

        void SpawnDecal(GameObject prefab, Vector3 _position, Quaternion _rotation)
        {
            Instantiate(prefab, _position, _rotation);

            StopCoroutine(DestroyBullet());

            Destroy(gameObject);
        }

        IEnumerator DestroyBullet()
        {
            yield return new WaitForSeconds(10);

            Destroy(gameObject);
        }*/
    }
}