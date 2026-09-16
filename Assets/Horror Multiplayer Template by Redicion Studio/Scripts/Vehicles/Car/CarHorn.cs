// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class CarHorn : NetworkBehaviour
    {
        public float HornVolume = 1;
        public AudioClip Horn0;
        public AudioClip Horn1;
        public AudioClip Horn2;
        public string carServerObjectName;

        public void PlayCarHorn(int _hornID, Vector3 position, string _carServerObjectName)
        {
            if(Horn0 != null && Horn1 != null && Horn2 != null)
            {
                if (_hornID == 0)
                    PlayClipAt(Horn0, position, HornVolume, 1, 20, _carServerObjectName);
                else if (_hornID == 1)
                    PlayClipAt(Horn1, position, HornVolume, 1, 20, _carServerObjectName);
                else if (_hornID == 2)
                    PlayClipAt(Horn2, position, HornVolume, 1, 20, _carServerObjectName);
            }
        }

        private void PlayClipAt(AudioClip _clip, Vector3 _position, float _volume, float _minDistance, float _maxDistance, string _carServerObjectName)
        {
            if (!isServer)
            {
                var tempGO = new GameObject("CarHornAudio");
                tempGO.transform.position = _position;
                GameObject tempCar;
                StartCoroutine(WaitUntilSuitableCarFound());

                IEnumerator WaitUntilSuitableCarFound()
                {
                    tempCar = GameObject.Find(_carServerObjectName);

                    yield return new WaitUntil(() => tempCar != null);

                    tempGO.transform.SetParent(tempCar.transform);

                    var aSource = tempGO.AddComponent<AudioSource>();
                    aSource.clip = _clip;
                    aSource.volume = _volume;
                    aSource.minDistance = _minDistance;
                    aSource.maxDistance = _maxDistance;
                    aSource.reverbZoneMix = 1;
                    aSource.spatialBlend = 1;
                    aSource.Play();
                    Destroy(tempGO, _clip.length);
                    Destroy(this.gameObject, _clip.length);

                    StopCoroutine(WaitUntilSuitableCarFound());
                }
                return;
            }
        }
    }
}