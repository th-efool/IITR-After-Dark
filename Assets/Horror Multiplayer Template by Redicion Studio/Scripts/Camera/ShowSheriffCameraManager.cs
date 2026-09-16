// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class ShowSheriffCameraManager : MonoBehaviour
    {
        public Transform sheriff;

        public CinemachineCamera showSheriffCamera;
        public CinemachineCamera thirdPersonCamera;

        void Start()
        {
            StartCoroutine(ShowSheriffCamera());
        }

        private void Update()
        {
            if (thirdPersonCamera == null)
                thirdPersonCamera = GameObject.Find("PlayerFollowCamera").GetComponent<CinemachineCamera>();
        }

        IEnumerator ShowSheriffCamera()
        {
            yield return new WaitForSeconds(3.21f);

            showSheriffCamera.enabled = false;

            yield return new WaitForSeconds(5f);

            Destroy(this.gameObject);
        }
    }
}