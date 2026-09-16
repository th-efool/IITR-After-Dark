// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class WeaponWheelSystem : MonoBehaviour
    {
        public List<WeaponWheelItem> weapons;

        public GameObject HandgunsButton;

        [Space]
        public GameObject HeavyGunsButton;

        [Space]
        public GameObject AssaultRiflesButton;

        [Space]
        public TMPro.TMP_Text WeaponNameText;
        public TMPro.TMP_Text WeaponInfoText;
        public TMPro.TMP_Text MousePositionText;

        public void CheckWeapons()
        {

        }
    }

    [System.Serializable]
    public class WeaponWheelItem
    {
        public string WeaponName;
        [TextArea]
        public string InfoText;
        public RedicionStudio.InventorySystem.ItemSO.ItemType type;
    }
}
