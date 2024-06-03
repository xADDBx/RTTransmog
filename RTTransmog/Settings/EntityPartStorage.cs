using Kingmaker.EntitySystem.Persistence;
using Kingmaker;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RTTransmog.Main;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Visual.CharacterSystem;

namespace RTTransmog {
    public static class EntityPartStorage {
        public class PerSaveSettings : EntityPart {
            public const string ID = "RTTransmog.PerSaveSettings";
            public bool didFirstInit = false;
            public HashSet<string> KnownWeapons = new ();
            public HashSet<string> KnownShoulders = new();
            public HashSet<string> KnownRing = new();
            public HashSet<string> KnownNeck = new();
            public HashSet<string> KnownHead = new();
            public HashSet<string> KnownGloves = new();
            public HashSet<string> KnownFeet = new();
            public HashSet<string> KnownArmor = new();
            public HashSet<string> KnownMechandrite = new();
            public Dictionary<string, ValueTuple<string, string>> MainhandWeapons = new();
            public Dictionary<string, ValueTuple<string, string>> OffhandWeapons = new();
            public Dictionary<string, ValueTuple<string, string>> Shoulders = new();
            public Dictionary<string, ValueTuple<string, string>> Ring1 = new();
            public Dictionary<string, ValueTuple<string, string>> Ring2 = new();
            public Dictionary<string, ValueTuple<string, string>> Neck = new();
            public Dictionary<string, ValueTuple<string, string>> Head = new();
            public Dictionary<string, ValueTuple<string, string>> Gloves = new();
            public Dictionary<string, ValueTuple<string, string>> Feet = new();
            public Dictionary<string, ValueTuple<string, string>> Armor = new();

        }
        private static PerSaveSettings cachedPerSave = null;
        public static void ClearCachedPerSave() => cachedPerSave = null;
        public static void ReloadPerSaveSettings() {
            var player = Game.Instance?.Player;
            if (player == null || Game.Instance.SaveManager.CurrentState == SaveManager.State.Loading) return;
            if (Game.Instance.State.InGameSettings.List.TryGetValue(PerSaveSettings.ID, out var obj) && obj is string json) {
                try {
                    cachedPerSave = JsonConvert.DeserializeObject<PerSaveSettings>(json);
                } catch (Exception e) {
                    log.Log(e.ToString());
                }
            }
            if (cachedPerSave == null) {
                cachedPerSave = new PerSaveSettings();
                SavePerSaveSettings();
            }
            if (!cachedPerSave.didFirstInit) {
                FirstInit();
                SavePerSaveSettings();
            }
        }
        public static void SavePerSaveSettings() {
            var player = Game.Instance?.Player;
            if (player == null) return;
            if (cachedPerSave == null) ReloadPerSaveSettings();
            var json = JsonConvert.SerializeObject(cachedPerSave);
            Game.Instance.State.InGameSettings.List[PerSaveSettings.ID] = json;
        }
        public static PerSaveSettings perSave {
            get {
                try {
                    if (cachedPerSave != null) return cachedPerSave;
                    ReloadPerSaveSettings();
                } catch (Exception e) {
                    log.Log(e.ToString());
                }
                return cachedPerSave;
            }
        }
    }
}
