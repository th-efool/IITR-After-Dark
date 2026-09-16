// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class PlayerMiniMapCircle : MonoBehaviour
    {
        public Image playerImage;
        public GameObject[] miniMapContent;

        public RedicionStudio.InventorySystem.Player player;

        private void Update()
        {
            if (player.isLocalPlayer && player.playerImage.sprite != playerImage.sprite)
            {
                SetCircleColor(player.isLocalPlayer);
            }
        }

        public void SetCircleColor(bool isLocalPlayer)
        {
            foreach (GameObject miniMapElement in miniMapContent)
            {
                miniMapElement.SetActive(isLocalPlayer);
            }

            if (isLocalPlayer)
            {
                playerImage.sprite = player.playerImage.sprite;
            }
        }
    }
}