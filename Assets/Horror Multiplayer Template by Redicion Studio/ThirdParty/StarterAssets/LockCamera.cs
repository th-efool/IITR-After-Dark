using UnityEngine;
using Unity.Cinemachine;

[SaveDuringPlay]
[AddComponentMenu("")]
public class LockCamera : CinemachineExtension
{
    public float m_MinXRotation = -70;
    public float m_MaxXRotation = 70;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Finalize)
        {
            var minXRot = state.RawOrientation;
            minXRot.x = m_MinXRotation;
            if (state.RawOrientation.x < m_MinXRotation)
            {
                state.RawOrientation = minXRot;
            }

            var maxXRot = state.RawOrientation;
            maxXRot.x = m_MaxXRotation;
            if (state.RawOrientation.x > m_MaxXRotation)
            {
                state.RawOrientation = maxXRot;
            }
        }
    }
}