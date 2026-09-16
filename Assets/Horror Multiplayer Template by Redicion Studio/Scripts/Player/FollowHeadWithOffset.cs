// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using UnityEngine;

namespace RedicionStudio
{
    public class FollowHeadWithOffset : MonoBehaviour
    {
        public CinemachineCamera virtualCamera;
        public Transform headTransform;

        void Start()
        {
            if (virtualCamera != null && headTransform != null)
            {
                virtualCamera.transform.position = headTransform.position;
                virtualCamera.transform.rotation = headTransform.rotation;

                virtualCamera.Follow = headTransform;
                virtualCamera.LookAt = headTransform;
            }
        }

        void LateUpdate()
        {
            if (virtualCamera != null && headTransform != null)
            {
                virtualCamera.transform.position = headTransform.position;
                virtualCamera.transform.rotation = headTransform.rotation;
            }
        }
    }
}
