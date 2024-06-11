using HarmonyLib;
using Kingmaker.Blueprints.Area;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.View;
using Kingmaker.Items.Slots;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Items;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using System.Reflection;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.View.Animation;

namespace RTTransmog {
    [HarmonyPatch]
    internal static class Patches {
        public static bool CheckForOverride(ItemSlot slot, BaseUnitEntity unit, out (string, string) Override) {
            Main.Slot slotCategory = Main.Slot.Armor;
            if (slot is ArmorSlot) {
                slotCategory = Main.Slot.Armor;
            } else if (slot is HandSlot hs) {
                if (hs.IsPrimaryHand) {
                    slotCategory = Main.Slot.Mainhand;
                } else {
                    slotCategory = Main.Slot.Offhand;
                } 
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentRing> ring) {
                var body = slot.Owner.GetBodyOptional();
                if (ring == body.Ring1) {
                    slotCategory = Main.Slot.Ring1;
                } else {
                    slotCategory = Main.Slot.Ring2;
                }
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentFeet>) {
                slotCategory = Main.Slot.Feet;
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentGloves>) {
                slotCategory = Main.Slot.Gloves;
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentHead>) {
                slotCategory = Main.Slot.Head;
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentNeck>) {
                slotCategory = Main.Slot.Neck;
            } else if (slot is EquipmentSlot<BlueprintItemEquipmentShoulders>) {
                slotCategory = Main.Slot.Shoulder;
            }
            Main.log.Log($"Checking for slot: {slotCategory}");
            if (Main.getDictForSlot(slotCategory).TryGetValue(unit.UniqueId, out var ret)) {
                Override = ret;
                return true;
            }
            Override = ("", "");
            return false;
        }
        
        public static bool CheckForOverride(WeaponAnimationStyle animStyle, BaseUnitEntity unit, out (string, string) Override, bool mainHand_2, int weaponSet) {
            Main.log.Log($"Checking for anim style: {animStyle}");
            if (Main.getDictForSlot(animStyle,mainHand_2, weaponSet).TryGetValue(animStyle, out var styleDictionary) && styleDictionary.TryGetValue(unit.UniqueId, out var ret))
            {
                Override = ret;
                return true;
            }
            Override = ("", "");
            return false;
        }
        
        
        internal static class Weapon_Patches {
            /*internal static class UnitViewHandsEquipment_Patches {
                [HarmonyTargetMethods]
                public static IEnumerable<MethodBase> TargetMethods() {
                    var targetType = typeof(BlueprintItemEquipmentHand);
                    var propertyGetter = targetType.GetProperty(nameof(BlueprintItemEquipmentHand.VisualParameters)).GetGetMethod();
                    var type = typeof(UnitViewHandsEquipment);
                    foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
                        method.GetMethodBody().
                        var instructions = MethodBodyReader.ReadInstructions(method).ToList();
                        if (instructions.Any(instr => instr.operand as MethodInfo == propertyGetter)) {
                            yield return method;
                        }
                    }
                }
            }*/
            internal static class ItemEntityWeapon_Patches {
                
            }
            [HarmonyPatch(typeof(UnitViewHandSlotData))]
            internal static class UnitViewHandSlotData_Patches {
                [HarmonyPatch(nameof(UnitViewHandSlotData.VisibleItemBlueprint), MethodType.Getter)]
                [HarmonyPostfix]
                public static void ReplaceVisibleItemBlueprintInHand(UnitViewHandSlotData __instance, ref BlueprintItemEquipmentHand __result)
                {
                    if (__result == null) return;
                    if (CheckForOverride(__result.VisualParameters.AnimStyle, __instance.Owner, out var Override, __instance.m_IsMainHand, __instance.m_SlotIdx)) {
                        __result = (BlueprintItemEquipmentHand)ResourcesLibrary.BlueprintsCache.Load(Override.Item1);
                    }
                }
            }
            internal static class UnitAnimationCallbackReceiver_Patches {

            }
        }
        
        [HarmonyPatch(typeof(UnitEntityView))]
        internal static class UnitEntityView_Patch {
            [HarmonyPatch(nameof(UnitEntityView.TryForceRampIndices))]
            [HarmonyPrefix]
            public static bool TryForceRampIndices(UnitEntityView __instance, ItemSlot slot, IEnumerable<EquipmentEntity> ees, Character character) {
                if (CheckForOverride(slot, __instance.EntityData, out var Override)) {
                    Character character2 = ((character == null) ? __instance.CharacterAvatar : character);
                    if (character2 == null) {
                        return true;
                    }
                    BlueprintItemEquipment blueprintItemEquipment = ResourcesLibrary.BlueprintsCache.Load(Override.Item1) as BlueprintItemEquipment;
                    if (blueprintItemEquipment == null || blueprintItemEquipment.ForcedRampColorPresetIndex < 0) {
                        return true;
                    }
                    foreach (EquipmentEntity equipmentEntity in ees) {
                        if (!(equipmentEntity.ColorPresets == null) && equipmentEntity.ColorPresets.IndexPairs.Count > 0) {
                            int primaryIndex = equipmentEntity.ColorPresets.IndexPairs[blueprintItemEquipment.ForcedRampColorPresetIndex].PrimaryIndex;
                            int secondaryIndex = equipmentEntity.ColorPresets.IndexPairs[blueprintItemEquipment.ForcedRampColorPresetIndex].SecondaryIndex;
                            character2.SetRampIndices(equipmentEntity, new int?(primaryIndex), new int?(secondaryIndex), false);
                        }
                    }
                    return false;
                }
                return true;
            }
            [HarmonyPatch(nameof(UnitEntityView.ExtractEquipmentEntities), [typeof(ItemSlot)])]
            [HarmonyPrefix]
            public static bool ExtractEquipmentEntities(UnitEntityView __instance, ItemSlot slot, ref IEnumerable<EquipmentEntity> __result) {
                if (CheckForOverride(slot, __instance.EntityData, out var Override)) {
                    __result = Main.ExtractEEs(ResourcesLibrary.BlueprintsCache.Load(Override.Item1) as BlueprintItemEquipment, __instance.EntityData);
                    Main.log.Log($"Found Override with {Override.Item2}, returning array with {__result?.Count() ?? 0} items");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Game))]
        internal static class Game_Patch {
            internal static bool isLoadGame = false;
            [HarmonyPatch(nameof(Game.LoadGameForce))]
            [HarmonyPrefix]
            private static void LoadGameForce() {
                isLoadGame = true;
            }
            [HarmonyPatch(nameof(Game.LoadArea), [typeof(BlueprintArea), typeof(BlueprintAreaEnterPoint), typeof(AutoSaveMode), typeof(SaveInfo), typeof(Action)])]
            [HarmonyPrefix]
            private static void LoadArea() {
                EntityPartStorage.ClearCachedPerSave();
                if (!EventBus.GlobalSubscribers.Contains(EventHandler.Instance)) {
                    EventBus.Subscribe(EventHandler.Instance);
                }
                if (isLoadGame) {
                    isLoadGame = false;
                }
            }
        }
    }
}