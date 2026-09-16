// Developed by Florian Lauka from Redicion Studio
// https://redicionstudio.com/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using RedicionStudio.InventorySystem;

namespace RedicionStudio
{
    public class PerkManager : NetworkBehaviour
    {
        public PlayerInventoryModule inventoryModule;

        UIPlayerInventory uiPlayerInventory;

        private void Update()
        {
            if (uiPlayerInventory == null)
                uiPlayerInventory = GameObject.Find("UIPlayerInventory").GetComponent<UIPlayerInventory>();
        }

        private void Start()
        {

        }

        public void Initialize(bool isHunter)
        {
            SetUpPerks(isHunter);
        }

        public void SetUpPerks(bool isHunter)
        {
            CharacterManager characterManager = GetComponent<CharacterManager>();

            // Array of perk slots
            UISlot[] perkSlots = { uiPlayerInventory.slotA, uiPlayerInventory.slotB, uiPlayerInventory.slotC, uiPlayerInventory.slotD };

            // Iterate over each perk slot
            foreach (var slot in perkSlots)
            {
                // Ensure the slot and its item are not null
                if (slot.item.itemSO != null)
                {
                    // Get the item in the slot
                    ItemSO itemSO = slot.item.itemSO;

                    // Check if the item in the slot is a PerkSO
                    if (itemSO is PerkSO perkSO)
                    {
                        // Apply perk effects if the perk is in the inventory
                        ApplyPerkEffects(characterManager, perkSO.perkEffects, isHunter);
                    }
                }
            }
        }

        void ApplyPerkEffects(CharacterManager characterManager, List<PerkSO.PerkEffect> perkEffects, bool isHunter)
        {
            // Iterate through all perk effects and apply them to the character
            foreach (var effect in perkEffects)
            {
                switch (effect.type)
                {
                    case PerkEffectType.staminaConsumption:
                        // Apply effect
                        if (!isHunter)
                            characterManager.staminaConsumption += (effect.status == PerkStatus.increase) ? effect.value : -effect.value;
                        else
                            characterManager.staminaConsumption = 10f;
                        break;

                    case PerkEffectType.staminaRegeneration:
                        // Apply effect
                        if (!isHunter)
                            characterManager.staminaRegeneration += (effect.status == PerkStatus.increase) ? effect.value : -effect.value;
                        else
                            characterManager.staminaRegeneration = 10f;
                        break;

                    case PerkEffectType.repairSpeed:
                        // Apply effect
                        if (!isHunter)
                            characterManager.repairSpeedMultiplier += (effect.status == PerkStatus.increase) ? effect.value : -effect.value;
                        else
                            characterManager.repairSpeedMultiplier = 1f;
                        break;

                    case PerkEffectType.consumableHealthGeneration:
                        // Apply effect
                        if (!isHunter)
                            characterManager.consumableHealthMultiplier += (effect.status == PerkStatus.increase) ? effect.value : -effect.value;
                        else
                            characterManager.consumableHealthMultiplier = 1f;
                        break;
                }
            }
        }

        [Server]
        private bool IsPerkInInventoryOnServer(string perkUniqueName)
        {
            foreach (var slot in inventoryModule.slots)
            {
                ItemSO _itemSO = slot.item.itemSO;

                if (_itemSO != null && _itemSO is PerkSO perkSO && perkSO.uniqueName == perkUniqueName)
                {
                    return true;
                }
            }

            return false;
        }

        public void CheckPerkInInventory(string perkUniqueName)
        {
            CmdCheckPerkInInventory(perkUniqueName);
        }

        [Command]
        private void CmdCheckPerkInInventory(string perkUniqueName)
        {
            bool isPerkInInventory = IsPerkInInventoryOnServer(perkUniqueName);
            RpcHandlePerkCheckResult(isPerkInInventory, perkUniqueName);
        }


        [ClientRpc]
        private void RpcHandlePerkCheckResult(bool isPerkInInventory, string perkUniqueName)
        {
            if (isPerkInInventory)
            {
                // Perk is already in the inventory
            }
            else
            {
                // Perk is not in the inventory
            }
        }
    }
}