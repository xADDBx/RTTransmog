using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Items.Shields;

namespace RTTransmog {
    public class EventHandler : IItemsCollectionHandler {
        private static EventHandler _Instance;
        public static EventHandler Instance {
            get {
                if (_Instance == null) {
                    _Instance = new();
                }
                return _Instance;
            }
        }
        public static HashSet<string> TypeToSet(BlueprintItem item) {
            if (item is BlueprintItemWeapon weapon) {
                if (EntityPartStorage.perSave.KnownWeapons.TryGetValue(weapon.VisualParameters.AnimStyle, out var dict)) {
                    return dict;
                } else {
                    Main.log.Log($"Unhandled anim style: {weapon.VisualParameters.AnimStyle}");
                    return null;
                }
            }
            if (item is BlueprintItemEquipmentShoulders) return EntityPartStorage.perSave.KnownShoulders;
            if (item is BlueprintItemShield shield) {
                if (EntityPartStorage.perSave.KnownWeapons.TryGetValue(shield.VisualParameters.AnimStyle, out var dict)) {
                    return dict;
                } else {
                    Main.log.Log($"Unhandled anim style: {shield.VisualParameters.AnimStyle}");
                    return null;
                }
            }
            if (item is BlueprintItemEquipmentRing) return EntityPartStorage.perSave.KnownRing;
            if (item is BlueprintItemEquipmentNeck) return EntityPartStorage.perSave.KnownNeck;
            if (item is BlueprintItemEquipmentHead) return EntityPartStorage.perSave.KnownHead;
            if (item is BlueprintItemEquipmentGloves) return EntityPartStorage.perSave.KnownGloves;
            if (item is BlueprintItemEquipmentFeet) return EntityPartStorage.perSave.KnownFeet;
            if (item is BlueprintItemArmor) return EntityPartStorage.perSave.KnownArmor;
            return null;
        }
        internal static bool BatchAdd = false;
        public void HandleItemsAdded(ItemsCollection collection, ItemEntity item, int count) {
            if (collection.IsPlayerInventory) {
                var set = TypeToSet(item.Blueprint);
                if (set != null) {
                    set.Add(item.Blueprint.AssetGuid);
                    if (!BatchAdd) {
                        EntityPartStorage.SavePerSaveSettings();
                    }
                }
            }
        }

        public void HandleItemsRemoved(ItemsCollection collection, ItemEntity item, int count) { }
    }
}
