// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ShowPlayerCameraManager : MonoBehaviour
    {
        public CinemachineCamera showPlayerCamera;
        public CinemachineCamera thirdPersonCamera;
        public Transform player;

        private void Update()
        {
            if (thirdPersonCamera == null)
                thirdPersonCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineCamera>();
        }

        void Start()
        {
            StartCoroutine(ShowPlayerCamera());
        }

        IEnumerator ShowPlayerCamera()
        {
            yield return new WaitForSeconds(7f);

            showPlayerCamera.enabled = false;

            yield return new WaitForSeconds(5f);

            Destroy(this.gameObject);
        }
    }
}
