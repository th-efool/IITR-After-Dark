// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio.InventorySystem{

    [CreateAssetMenu(fileName = "New Outfit Item SO", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Outfit")]
    public class OutfitItemSO : ItemSO{

        [Header("Outfit")]
        public string outfitStyle;
        public int outfitID = 1;
        public GameObject previewModelPrefab;
    }
}
