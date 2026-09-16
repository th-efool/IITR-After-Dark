using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace RedicionStudio
{
    public class ShowFinisherFightCameraManager : MonoBehaviour
    {
        public GameObject _camera;

        private void Start()
        {
            if (NetworkClient.localPlayer != null && !NetworkClient.localPlayer.gameObject.GetComponent<HunterAbilities>()._inFight)
            {
                _camera.SetActive(false);
            }
            else
            {
                _camera.SetActive(true);
            }
        }
    }
}
