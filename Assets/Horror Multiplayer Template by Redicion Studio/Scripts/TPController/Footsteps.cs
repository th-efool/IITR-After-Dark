// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class Footsteps : NetworkBehaviour
    {
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;
        private CharacterController _controller;
        public Transform leftFootPosition;
        public GameObject leftFootprintPrefab;
        public Transform rightFootPosition;
        public GameObject rightFootprintPrefab;
        public float offset = 0.05f;

        void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void OnLeftFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    //AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                    PlayClipAt(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume, 1, 5);
                }
                RaycastHit hit;

                if (Physics.Raycast(leftFootPosition.position, leftFootPosition.forward, out hit))
                {
                    if (!GetComponent<HunterAbilities>()._isHunter)
                        Instantiate(leftFootprintPrefab, hit.point + hit.normal * offset, Quaternion.LookRotation(hit.normal, leftFootPosition.up));
                }
            }
        }

        private void OnRightFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    //AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                    PlayClipAt(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume, 1, 5);
                }
                RaycastHit hit;

                if (Physics.Raycast(rightFootPosition.position, rightFootPosition.forward, out hit))
                {
                    if (!GetComponent<HunterAbilities>()._isHunter)
                        Instantiate(rightFootprintPrefab, hit.point + hit.normal * offset, Quaternion.LookRotation(hit.normal, rightFootPosition.up));
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                //AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                PlayClipAt(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume, 1, 5);
            }
        }

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance)
        {
            if (isServer)
                return;

            var tempGO = new GameObject("FootstepAudio");
            tempGO.transform.position = _position;
            var aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = _clip;
            aSource.volume = _volume;
            aSource.minDistance = _minDistance;
            aSource.maxDistance = _maxDistance;
            aSource.reverbZoneMix = 1;
            aSource.spatialBlend = 1;
            aSource.Play();
            Destroy(tempGO, _clip.length);
            return;
        }
    }
}