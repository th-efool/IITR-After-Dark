// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class SyncAnimationController : MonoBehaviour
    {
        public Animator mainAnimator;
        public Animator targetAnimator;

        private void Update()
        {
            for (int layerIndex = 0; layerIndex < mainAnimator.layerCount; layerIndex++)
            {
                AnimatorStateInfo stateInfo = mainAnimator.GetCurrentAnimatorStateInfo(layerIndex);
                targetAnimator.Play(stateInfo.fullPathHash, layerIndex, stateInfo.normalizedTime);
            }
        }
    }
}
