// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class RocketExplosion : MonoBehaviour
    {
        /*float radius;

        private void Start()
        {
            radius = GetComponent<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }

        void OnTriggerStay(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Collider[] colliders = Physics.OverlapSphere(other.gameObject.transform.position, radius);
                foreach (Collider col in colliders)
                {
                    if (col.tag == "Player")
                    {
                        col.GetComponent<Health>().TakeDamage(100);
                    }
                }
            }

            this.GetComponent<SphereCollider>().enabled = false;

            StartCoroutine(DestroyExplosion());
        }

        IEnumerator DestroyExplosion()
        {
            yield return new WaitForSeconds(3);

            Destroy(gameObject);
        }*/
    }
}
