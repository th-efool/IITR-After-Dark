// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio
{
    public class DeathScreen : MonoBehaviour
    {
        public int timer = 9;
        public TMPro.TMP_Text timerText;

        public GameObject _camera;

        public void SetUpDeathScreen(Vector3 playerPosition, string killerUsername)
        {
            StartCoroutine(RespawnTimer());

            _camera.transform.position = new Vector3(playerPosition.x, playerPosition.y + 2, playerPosition.z - 5);
        }

        void Update()
        {
            if (timer == 0)
                Destroy(this.gameObject);
        }

        IEnumerator RespawnTimer()
        {
            while (true)
            {
                timeCount();
                yield return new WaitForSeconds(1);
            }
        }
        void timeCount()
        {
            timer -= 1;
            timerText.text = "Respawn in: " + "'" + timer + "'";
        }
    }
}
