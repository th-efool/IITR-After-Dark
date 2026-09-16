// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio.InventorySystem{

    //[CreateAssetMenu(fileName = "New Companion Item SO", menuName = "Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Companion")]
    public class CompanionItemSO : ItemSO{

        [Header("Companion")]
        public string companionNote;
    }
}
