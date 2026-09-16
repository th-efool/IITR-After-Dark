// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio.InventorySystem
{
    [CreateAssetMenu(fileName = "New Killer Item SO", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Killer")]
    public class KillerSO : ItemSO
    {
        public int killerID = 1;
        public GameObject previewModelPrefab;
    }
}