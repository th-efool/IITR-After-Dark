// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedicionStudio.InventorySystem{

    [CreateAssetMenu(fileName = "New Perk Item SO", menuName = "Tools/Horror Multiplayer Template by Redicion Studio/Inventory System/ItemSOs/Perk")]
    public class PerkSO : ItemSO
    {
        [System.Serializable]
        public class PerkEffect
        {
            public string name;
            public PerkEffectType type;
            public PerkStatus status;
            public float value;
        }

        [Header("Perk")]
        public List<PerkEffect> perkEffects = new List<PerkEffect>();
        public string perkNote;
    }

    public enum PerkEffectType
    {
        staminaConsumption,
        staminaRegeneration,
        repairSpeed,
        consumableHealthGeneration,
    }

    public enum PerkStatus
    {
        increase,
        decrease,
    }
}
