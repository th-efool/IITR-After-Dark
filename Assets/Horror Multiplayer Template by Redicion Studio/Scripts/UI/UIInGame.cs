// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using UnityEngine;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class UIInGame : MonoBehaviour
    {

        [SerializeField] private GameObject _content;

        private void Update()
        {
            _content.SetActive(Player.localPlayer != null);
        }
    }
}