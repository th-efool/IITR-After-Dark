// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class MatchEndCameraManager : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(DestroyMatchEndCamera());
        }

        IEnumerator DestroyMatchEndCamera()
        {
            yield return new WaitForSeconds(23);

            Destroy(gameObject);
        }
    }
}
