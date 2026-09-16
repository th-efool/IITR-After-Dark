// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class NetworkRocket : Projectile
    {

        public GameObject RocketExplosion;
        public RocketExplosionRadius ExplosionDamage;
        public float Radius;
        //execute only on client who launched this
        protected override void OnCollided(GameObject objectCollidedWith)
        {
            base.OnCollided(objectCollidedWith);
            ExplosionDamage.SetupExpliosion(this);
            Detonate();
        }

        //execute for everyone
        protected override void OnCollidedRPC()
        {
            base.OnCollidedRPC();
            GameObject explosionVisual = Instantiate(RocketExplosion, transform);
            explosionVisual.GetComponent<ParticleSystem>().Play();
            Destroy(explosionVisual, 10f);
        }

        void Detonate()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);
            foreach (Collider col in colliders)
            {
                if (col.tag == "Vehicle")
                {
                    CmdTakeDamageVehicle(col.GetComponent<VehicleHealth>(), 50);
                }

                if (col.tag == "Player")
                {
                    CmdTakeDamage(col.GetComponent<CharacterManager>(), 100);
                    //col.GetComponent<Health>().attackerUsername = _myOwner.shooterUsername;
                }
            }
        }
        //public int rocketDemage = 10;

        /* void Start()
         {
             StartCoroutine(DestroyRocket());
         }*/

        /* void OnCollisionEnter(Collision collision)
         {
             GameObject hit = collision.gameObject;
             VehicleHealth health = hit.GetComponent<VehicleHealth>();
             Health playerHealth = hit.GetComponent<Health>();

             ContactPoint contact = collision.contacts[0];
             Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
             Vector3 pos = contact.point;

             if (health != null)
             {
                 health.TakeDamage(rocketDemage);

                 Instantiate(rocketExplosion, transform).GetComponent<RocketExplosionRadius>().shooterUsername = shooterUsername;
             }

             if (playerHealth != null)
             {
                 playerHealth.TakeDamage(rocketDemage);
                 playerHealth.attackerUsername = shooterUsername;
             }
         }*/

        /*  IEnumerator DestroyRocket()
          {
              yield return new WaitForSeconds(10);

              Destroy(gameObject);
          }*/
    }
}