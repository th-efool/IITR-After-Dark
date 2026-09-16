// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    /// <summary>
    /// This script is not used, its funtionality was moved to NetworkRocket script
    /// </summary>
    public class RocketExplosionRadius : MonoBehaviour
    {

        Projectile _myOwner;
        [SerializeField] float radius = 10f;

        public void SetupExpliosion(Projectile myOwner)
        {
            _myOwner = myOwner;
            gameObject.SetActive(true);
            //radius = GetComponent<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Vehicle") || other.gameObject.CompareTag("Player"))
            {
                Collider[] colliders = Physics.OverlapSphere(other.gameObject.transform.position, radius);
                foreach (Collider col in colliders)
                {
                    if (col.tag == "Vehicle")
                    {
                        _myOwner.CmdTakeDamageVehicle(col.GetComponent<VehicleHealth>(), 50);
                    }

                    if (col.tag == "Player")
                    {
                        _myOwner.CmdTakeDamage(col.GetComponent<CharacterManager>(), 100);
                    }
                }
            }

            GetComponent<SphereCollider>().enabled = false;

            StartCoroutine(DestroyExplosion());
        }


        IEnumerator DestroyExplosion()
        {
            yield return new WaitForSeconds(3);

            Destroy(gameObject);
        }
    }
}