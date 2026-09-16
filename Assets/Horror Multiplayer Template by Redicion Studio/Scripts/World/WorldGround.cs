// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class WorldGround : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Health>() != null)
            {
                if (other.GetComponent<Health>().isFallingFromAircraft == true)
                {
                    other.GetComponent<Health>().ReleaseParachute();
                }
            }
        }
    }
}
