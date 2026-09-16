// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedicionStudio
{
    public class UIDragAndDropIndicator : MonoBehaviour
    {
        public Image image;
        public Image rarityImage;
        public TextMeshProUGUI rarityText;
        public GameObject amountContent;
        public TextMeshProUGUI amountText;
        public RedicionStudio.InventorySystem.Item item;
    }
}
