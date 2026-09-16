// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using Unity.Cinemachine;
using UnityEngine;

namespace RedicionStudio
{
    [ExecuteInEditMode]
    [SaveDuringPlay]
    [AddComponentMenu("")]
    public class LockRotation : CinemachineExtension
    {
        private Quaternion initialLocalRotation;
        private Transform followTarget;

        protected override void Awake()
        {
            base.Awake();
            var followComp = GetComponent<CinemachineFollow>();
            if (followComp != null)
            {
                followTarget = followComp.FollowTarget;
                if (followTarget != null)
                {
                    initialLocalRotation = Quaternion.Inverse(followTarget.rotation) * transform.rotation;
                }
            }
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
        {
            if (enabled && stage == CinemachineCore.Stage.Body && followTarget != null)
            {
                state.RawOrientation = followTarget.rotation * initialLocalRotation;
            }
        }
    }
}