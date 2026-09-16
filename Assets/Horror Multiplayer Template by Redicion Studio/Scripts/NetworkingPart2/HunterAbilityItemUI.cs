// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class HunterAbilityItemUI : MonoBehaviour
    {
        [SerializeField] Image _filler;
        [SerializeField] Image _ableToBeUsedIndicator;
        [SerializeField] GameObject _filled;
        [SerializeField] AudioClip _ableToBeUseAudioClip;
        [SerializeField] TMPro.TMP_Text _cooldownTimerText;
        Coroutine c_fillAbilityIcon;
        float _cooldownTimer;

        public void AbilityUsed(float cooldown)
        {
            _filler.fillAmount = 1;
            _cooldownTimer = 0;
            _cooldownTimer = Time.time + cooldown;
            if (c_fillAbilityIcon != null)
                StopCoroutine(c_fillAbilityIcon);
            c_fillAbilityIcon = StartCoroutine(FillAbilityIcon(cooldown));
        }
        public void AbleToBeUsed(bool able)
        {
            _ableToBeUsedIndicator.enabled = !able;
        }

        IEnumerator FillAbilityIcon(float coolDown)
        {
            while (_cooldownTimer > Time.time)
            {
                _filler.fillAmount = (((_cooldownTimer - Time.time) / coolDown));
                if (Mathf.CeilToInt(Mathf.Max(_cooldownTimer - Time.time, 0)) > 99)
                    _cooldownTimerText.fontSize = 30;
                else
                    _cooldownTimerText.fontSize = 36;
                _cooldownTimerText.text = Mathf.CeilToInt(Mathf.Max(_cooldownTimer - Time.time, 0)).ToString();
                yield return null;
            }

            _filler.fillAmount = 0f;
            PlayClipAt(_ableToBeUseAudioClip, transform.position, 1, 1, 500, 0, 1);
            _filled.GetComponent<Animator>().SetTrigger("PlayFilledAnimation");
            _cooldownTimerText.fontSize = 15;
            _cooldownTimerText.text = "READY";
        }

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance, float spatialBlend, float reverbZoneMix)
        {
            var tempGO = new GameObject();
            tempGO.transform.position = _position;
            var aSource = tempGO.AddComponent<AudioSource>();
            aSource.clip = _clip;
            aSource.volume = _volume;
            aSource.minDistance = _minDistance;
            aSource.maxDistance = _maxDistance;
            aSource.reverbZoneMix = reverbZoneMix;
            aSource.spatialBlend = spatialBlend;
            aSource.Play();
            Destroy(tempGO, _clip.length);
        }
    }
}