using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager;
using Kingmaker.Blueprints;
using static RTTransmog.Extensions;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Blueprints.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem;
using Kingmaker.View.Animation;

namespace RTTransmog;

#if DEBUG
[EnableReloading]
#endif
public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;
    internal static UnityModManager.ModEntry mod;
    private static Browser<string, string> ShoulderBrowser = new(true, true, false, true);
    private static Browser<string, string> Ring1Browser = new(true, true, false, true);
    private static Browser<string, string> Ring2Browser = new(true, true, false, true);
    private static Browser<string, string> NeckBrowser = new(true, true, false, true);
    private static Browser<string, string> HeadBrowser = new(true, true, false, true);
    private static Browser<string, string> GlovesBrowser = new(true, true, false, true);
    private static Browser<string, string> FeetBrowser = new(true, true, false, true);
    private static Browser<string, string> ArmorBrowser = new(true, true, false, true);
    //weapon browsers
    private static Dictionary<Slot, Browser<string, string>> WeaponBrowsers = new() {
        { Slot.Knife, new(true, true, false, true) },
        { Slot.Fencing, new(true, true, false, true) },
        { Slot.AxeTwoHanded, new(true, true, false, true) },
        { Slot.Assault, new(true, true, false, true) },
        { Slot.BrutalOneHanded, new(true, true, false, true) },
        { Slot.BrutalTwoHanded, new(true, true, false, true) },
        { Slot.HeavyOnHip, new(true, true, false, true) },
        { Slot.Pistol, new(true, true, false, true) },
        { Slot.Rifle, new(true, true, false, true) },
        { Slot.Fist, new(true, true, false, true) },
        { Slot.Staff, new(true, true, false, true) },
        { Slot.EldarRifle, new(true, true, false, true) },
        { Slot.EldarAssault, new(true, true, false, true) },
        { Slot.EldarHeavyOnHip, new(true, true, false, true) },
        { Slot.EldarHeavyOnShoulder, new(true, true, false, true) },
        { Slot.OneHandedHammer, new(true, true, false, true) },
        { Slot.TwoHandedHammer, new(true, true, false, true) },
    };
    //
    private static Dictionary<string, string> KeyCache = new();

    private static bool showShoulderBrowser = false;
    private static bool showRing1Browser = false;
    private static bool showRing2Browser = false;
    private static bool showNeckBrowser = false;
    private static bool showHeadBrowser = false;
    private static bool showGlovesBrowser = false;
    private static bool showFeetBrowser = false;
    private static bool showArmorBrowser = false;
    //
    private static bool showWeaponBrowsers = false;
    private static bool showAllWeaponBrowsers = false;
    private static bool mainHand = true;
    private static int weaponSet {
        get { return (m_weaponSet == false ? 0 : 1); }
    }
    private static bool m_weaponSet = false; //false is set 0, true is set 1
    private static bool showAnimRelevantBrowser = false;
    private static bool showKnifeBrowser = false;
    private static bool showFencingBrowser = false;
    private static bool showAxeTwoHandedBrowser = false;
    private static bool showAssaultBrowser = false;
    private static bool showBrutalOneHandedBrowser = false;
    private static bool showBrutalTwoHandedBrowser = false;
    private static bool showHeavyOnHipBrowser = false;
    private static bool showPistolBrowser = false;
    private static bool showRifleBrowser = false;
    private static bool showFistBrowser = false;
    private static bool showStaffBrowser = false;
    private static bool showEldarRifleBrowser = false;
    private static bool showEldarAssaultBrowser = false;
    private static bool showEldarHeavyOnHipBrowser = false;
    private static bool showEldarHeavyOnShoulderBrowser = false;
    private static bool showOneHandedHammerBrowser = false;
    private static bool showTwoHandedHammerBrowser = false;
    //

    private static Slot currentBrowserSlot;
    private static BaseUnitEntity pickedUnit = null;
    public static Settings settings;
    public enum Slot {
        Mainhand,
        Offhand,
        Shoulder,
        Ring1,
        Ring2,
        Neck,
        Head,
        Gloves,
        Feet,
        Armor,

        // one per anim style, except empty/unused ones.
        Knife,
        Fencing,
        AxeTwoHanded,
        Assault,
        BrutalOneHanded,
        BrutalTwoHanded,
        HeavyOnHip,
        Pistol,
        Rifle,
        Fist,
        Staff,
        EldarRifle,
        EldarAssault,
        EldarHeavyOnHip,
        EldarHeavyOnShoulder,
        OneHandedHammer,
        TwoHandedHammer,
    }
    private static Dictionary<Slot, string> SlotName = new Dictionary<Slot, string>() {
        { Slot.Mainhand, "Mainhand" },
        { Slot.Offhand, "Offhand" },
        { Slot.Shoulder, "Shoulders" },
        { Slot.Ring1, "Ring 1" },
        { Slot.Ring2, "Ring 2" },
        { Slot.Neck, "Neck" },
        { Slot.Head, "Head" },
        { Slot.Gloves, "Gloves" },
        { Slot.Feet, "Feet" },
        { Slot.Armor, "Armor" },
        { Slot.Knife, "Knives" },
        { Slot.Fencing, "Fencing" },
        { Slot.AxeTwoHanded, "Two-handed Axes" },
        { Slot.Assault, "Assault (Rifles/Shotguns)" },
        { Slot.BrutalOneHanded, "Brutal One-handed" },
        { Slot.BrutalTwoHanded, "Brutal Two-handed" },
        { Slot.HeavyOnHip, "Heavy Weapons" },
        { Slot.Pistol, "Pistols" },
        { Slot.Rifle, "Rifles" },
        { Slot.Fist, "Fist Weapons" },
        { Slot.Staff, "Staves" },
        { Slot.EldarRifle, "Eldar Rifles" },
        { Slot.EldarAssault, "Eldar Assault (Rifles/Shotguns)" },
        { Slot.EldarHeavyOnHip, "Eldar Heavy Weapons (Hip)" },
        { Slot.EldarHeavyOnShoulder, "Eldar Heavy Weapons (Shoulder)" },
        { Slot.OneHandedHammer, "One-handed Hammers" },
        { Slot.TwoHandedHammer, "Two-handed Hammers" },
    };
    public static WeaponAnimationStyle getAnimStyleFromSlot(Slot slot) // Needed, otherwise the indices are misaligned.
    {
        if (WeaponAnimationStyle.TryParse<WeaponAnimationStyle>(slot.ToString(), out var animStyle)) {
            return animStyle;
        }

        throw new Exception("Failed to parse weapon animation style.");
    }
    public static Slot getSlotFromAnimStyle(WeaponAnimationStyle animStyle) // Needed, otherwise the indices are misaligned.
    {
        if (Slot.TryParse<Slot>(animStyle.ToString(), out var slot)) {
            return slot;
        }

        throw new Exception("Failed to parse weapon animation style.");
    }
    public static bool Load(UnityModManager.ModEntry modEntry) {
        log = modEntry.Logger;
        mod = modEntry;
#if DEBUG
        modEntry.OnUnload = OnUnload;
#endif
        modEntry.OnGUI = OnGUI;
        settings = Settings.Load<Settings>(modEntry);
        modEntry.OnSaveGUI = OnSaveGUI;
        HarmonyInstance = new Harmony(modEntry.Info.Id);
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }
    public static void OnSaveGUI(ModEntry modEntry) {
        settings.Save(modEntry);
    }
    public static void FirstInit() {
        EntityPartStorage.perSave.didFirstInit = true;
        foreach (var item in Game.Instance.Player.Inventory) {
            EventHandler.Instance.HandleItemsAdded(Game.Instance.Player.Inventory, item, 1);
        }
        foreach (var item in Game.Instance.Player.PartyAndPets.SelectMany(pap => pap.Inventory.Items)) {
            EventHandler.Instance.HandleItemsAdded(Game.Instance.Player.Inventory, item, 1);
        }
    }
    public static Dictionary<string, (string, string)> getDictForSlot(Slot slot) {
        if (((int)slot) >= (int)Slot.Knife) // cant do comparisons in a switch statement, and this saves a few lines.
        {
            return EntityPartStorage.perSave.Weapons[weaponSet][mainHand][getAnimStyleFromSlot(slot)];
        }
        switch (slot) {
            case Slot.Shoulder: return EntityPartStorage.perSave.Shoulders;
            case Slot.Ring1: return EntityPartStorage.perSave.Ring1;
            case Slot.Ring2: return EntityPartStorage.perSave.Ring2;
            case Slot.Neck: return EntityPartStorage.perSave.Neck;
            case Slot.Head: return EntityPartStorage.perSave.Head;
            case Slot.Gloves: return EntityPartStorage.perSave.Gloves;
            case Slot.Feet: return EntityPartStorage.perSave.Feet;
            case Slot.Armor: return EntityPartStorage.perSave.Armor;
            default: return null;
        }
    }
    public static Dictionary<WeaponAnimationStyle, Dictionary<string, (string, string)>> getDictForSlot(
        WeaponAnimationStyle style, bool mainHand2, int weaponSet) {
        return EntityPartStorage.perSave.Weapons[weaponSet][mainHand2];
    }
    public static void UpdateEquippedItems(Slot slot, bool removeOld, string oldId, bool addNew = false, string newId = "") {
        if (removeOld) {
            var oldBp = ResourcesLibrary.BlueprintsCache.Load(oldId) as BlueprintItemEquipment;
            var EEs = ExtractEEs(oldBp);
            pickedUnit.View.CharacterAvatar.RemoveEquipmentEntities(EEs);
        }
        PartUnitBody body = pickedUnit.OwnerEntity.Body;
        ItemSlot itemSlot = null;
        switch (slot) {
            case Slot.Mainhand: {
                    itemSlot = body.m_HandsEquipmentSets[body.m_CurrentHandsEquipmentSetIndex].PrimaryHand;
                }; break;
            case Slot.Offhand: {
                    itemSlot = body.m_HandsEquipmentSets[body.m_CurrentHandsEquipmentSetIndex].SecondaryHand;
                }; break;
            case Slot.Shoulder: {
                    itemSlot = body.Shoulders;
                }; break;
            case Slot.Ring1: {
                    itemSlot = body.Ring1;
                }; break;
            case Slot.Ring2: {
                    itemSlot = body.Ring2;
                }; break;
            case Slot.Neck: {
                    itemSlot = body.Neck;
                }; break;
            case Slot.Head: {
                    itemSlot = body.Head;
                }; break;
            case Slot.Gloves: {
                    itemSlot = body.Gloves;
                }; break;
            case Slot.Feet: {
                    itemSlot = body.Feet;
                }; break;
            case Slot.Armor: {
                    itemSlot = body.Armor;
                }; break;
        }
        if (addNew) {
            getDictForSlot(slot)[pickedUnit.UniqueId] = (newId, GetKey(newId));
            EntityPartStorage.SavePerSaveSettings();
        }
        pickedUnit.View.HandsEquipment.HandleEquipmentSetChanged();
        if (itemSlot == null) return;
        EventBus.RaiseEvent<IUnitEquipmentHandler>(pickedUnit, delegate (IUnitEquipmentHandler h)
        {
            h.HandleEquipmentSlotUpdated(itemSlot, itemSlot.MaybeItem ?? null);
        }, true);
        pickedUnit.View.HandsEquipment.ChangeEquipmentWithoutAnimation();
    }
    public static IEnumerable<EquipmentEntity> ExtractEEs(BlueprintItemEquipment blueprintItemEquipment, BaseUnitEntity unit = null) {
        unit ??= pickedUnit;
        IEnumerable<EquipmentEntity> enumerable;
        Gender gender = unit.Gender;
        BlueprintRace race = unit.Progression.Race;
        Race race2 = ((race != null) ? race.RaceId : Race.Human);
        enumerable = ((blueprintItemEquipment != null && blueprintItemEquipment.EquipmentEntity != null) ? blueprintItemEquipment.EquipmentEntity.Load(gender, race2) : Enumerable.Empty<EquipmentEntity>());
        return enumerable;
    }
    public static void OverrideGUI() {
        Div();
        Label($"Overrides for {SlotName[currentBrowserSlot]}".color(RGBA.lime).Bold());
        using (HorizontalScope()) {
            bool hasOverride = false;
            string name = "None".color(RGBA.aqua);
            if (getDictForSlot(currentBrowserSlot).TryGetValue(pickedUnit.UniqueId, out var Override)) {
                name = Override.Item2.color(RGBA.aqua);
                hasOverride = true;
            }
            ActionButton("Reset Override", () => {
                if (hasOverride) {
                    getDictForSlot(currentBrowserSlot).Remove(pickedUnit.UniqueId);
                    UpdateEquippedItems(currentBrowserSlot, true, Override.Item1);
                    EntityPartStorage.SavePerSaveSettings();
                }
            }, AutoWidth());
            Label($"Current Override: {name}");
        }
    }
    public static void ResetBrowsers() {
        ShoulderBrowser.ResetSearch();
        Ring1Browser.ResetSearch();
        Ring2Browser.ResetSearch();
        NeckBrowser.ResetSearch();
        HeadBrowser.ResetSearch();
        GlovesBrowser.ResetSearch();
        FeetBrowser.ResetSearch();
        ArmorBrowser.ResetSearch();
        // weapon browsers reset
        foreach(var browser in WeaponBrowsers)
            browser.Value.ResetSearch();
    }
    public static void OnGUI(UnityModManager.ModEntry modEntry) {
        var units = new List<BaseUnitEntity>() { Game.Instance?.Player?.MainCharacterEntity };
        units.AddRange(Game.Instance.Player.ActiveCompanions ?? new());
        units = units.Where(u => u != null).ToList();
        if (units.Count > 0) {
            GUILayout.Label("Character to change:");

            int selectedIndex = pickedUnit != null ? Array.IndexOf(units.ToArray(), pickedUnit) : 0;
            if (selectedIndex < 0) {
                selectedIndex = 0;
                pickedUnit = null;
            }
            int newIndex = GUILayout.SelectionGrid(selectedIndex, units.Select(m => m.CharacterName).ToArray(), 6);
            if (selectedIndex != newIndex || pickedUnit == null) {
                pickedUnit = units[newIndex];
                ResetBrowsers();
            }
            Div();
            if (TogglePrivate("Show Items without Equipment Entity (those are by default hidden because I assume they don't change visuals). This could be helpful if you don't want equipment in a slot to show because you can just override the slot with an item with no visuals.", ref settings.shouldShowItemsWithoutEE, false, false, 0, AutoWidth())) {
                ResetBrowsers();
            }

            currentBrowserSlot = Slot.Shoulder;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Shoulder]} Browser", ref showShoulderBrowser);
            if (showShoulderBrowser) {
                BrowserGUI<BlueprintItemEquipmentShoulders>(ShoulderBrowser, EntityPartStorage.perSave.KnownShoulders);
            }

            currentBrowserSlot = Slot.Ring1;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Ring1]} Browser", ref showRing1Browser);
            if (showRing1Browser) {
                BrowserGUI<BlueprintItemEquipmentRing>(Ring1Browser, EntityPartStorage.perSave.KnownRing);
            }

            currentBrowserSlot = Slot.Ring2;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Ring2]} Browser", ref showRing2Browser);
            if (showRing2Browser) {
                BrowserGUI<BlueprintItemEquipmentRing>(Ring2Browser, EntityPartStorage.perSave.KnownRing);
            }

            currentBrowserSlot = Slot.Neck;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Neck]} Browser", ref showNeckBrowser);
            if (showNeckBrowser) {
                BrowserGUI<BlueprintItemEquipmentNeck>(NeckBrowser, EntityPartStorage.perSave.KnownNeck);
            }

            currentBrowserSlot = Slot.Head;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Head]} Browser", ref showHeadBrowser);
            if (showHeadBrowser) {
                BrowserGUI<BlueprintItemEquipmentHead>(HeadBrowser, EntityPartStorage.perSave.KnownHead);
            }

            currentBrowserSlot = Slot.Gloves;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Gloves]} Browser", ref showGlovesBrowser);
            if (showGlovesBrowser) {
                BrowserGUI<BlueprintItemEquipmentGloves>(GlovesBrowser, EntityPartStorage.perSave.KnownGloves);
            }

            currentBrowserSlot = Slot.Feet;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Feet]} Browser", ref showFeetBrowser);
            if (showFeetBrowser) {
                BrowserGUI<BlueprintItemEquipmentFeet>(FeetBrowser, EntityPartStorage.perSave.KnownFeet);
            }

            currentBrowserSlot = Slot.Armor;
            OverrideGUI();
            DisclosureToggle($"Show {SlotName[Slot.Armor]} Browser", ref showArmorBrowser);
            if (showArmorBrowser) {
                BrowserGUI<BlueprintItemArmor>(ArmorBrowser, EntityPartStorage.perSave.KnownArmor);
            }
            // Weapon browsers.
            Div();
            using (VerticalScope()) {
                Space(10);
                DisclosureToggle($"Show Weapon Browsers", ref showWeaponBrowsers);
            }
            if (showWeaponBrowsers) {
                WeaponSlotSelector();
                if (!showAllWeaponBrowsers) {
                    var currentAnimStyle = mainHand == true
                        ? pickedUnit.View.HandsEquipment.Sets.First(a => a.Value.MainHand.m_SlotIdx == weaponSet).Value
                            .MainHand.VisibleItemBlueprint?.VisualParameters?.AnimStyle
                        : pickedUnit.View.HandsEquipment.Sets.First(a => a.Value.OffHand.m_SlotIdx == weaponSet).Value
                            .OffHand.VisibleItemBlueprint?.VisualParameters?.AnimStyle;
                    if (currentAnimStyle != null) {
                        var currentSlot = getSlotFromAnimStyle((WeaponAnimationStyle)currentAnimStyle);
                        currentBrowserSlot = currentSlot;
                        OverrideGUI();
                        DisclosureToggle($"Show {SlotName[currentSlot]} Browser", ref showAnimRelevantBrowser);
                        if (showAnimRelevantBrowser) {
                            BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentSlot],
                                EntityPartStorage.perSave.KnownWeapons[(WeaponAnimationStyle)currentAnimStyle],
                                animStyle: (WeaponAnimationStyle)currentAnimStyle);
                        }
                    }
                }
                else {
                    currentBrowserSlot = Slot.Knife;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Knife]} Browser", ref showKnifeBrowser);
                    if (showKnifeBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Knife)],
                            animStyle: WeaponAnimationStyle.Knife);
                    }

                    currentBrowserSlot = Slot.Fencing;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Fencing]} Browser", ref showFencingBrowser);
                    if (showFencingBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Fencing)],
                            animStyle: WeaponAnimationStyle.Fencing);
                    }

                    currentBrowserSlot = Slot.AxeTwoHanded;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.AxeTwoHanded]} Browser", ref showAxeTwoHandedBrowser);
                    if (showAxeTwoHandedBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.AxeTwoHanded)],
                            animStyle: WeaponAnimationStyle.AxeTwoHanded);
                    }

                    currentBrowserSlot = Slot.Assault;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Assault]} Browser", ref showAssaultBrowser);
                    if (showAssaultBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Assault)],
                            animStyle: WeaponAnimationStyle.Assault);
                    }

                    currentBrowserSlot = Slot.BrutalOneHanded;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.BrutalOneHanded]} Browser",
                        ref showBrutalOneHandedBrowser);
                    if (showBrutalOneHandedBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.BrutalOneHanded)],
                            animStyle: WeaponAnimationStyle.BrutalOneHanded);
                    }

                    currentBrowserSlot = Slot.BrutalTwoHanded;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.BrutalTwoHanded]} Browser",
                        ref showBrutalTwoHandedBrowser);
                    if (showBrutalTwoHandedBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.BrutalTwoHanded)],
                            animStyle: WeaponAnimationStyle.BrutalTwoHanded);
                    }

                    currentBrowserSlot = Slot.HeavyOnHip;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.HeavyOnHip]} Browser", ref showHeavyOnHipBrowser);
                    if (showHeavyOnHipBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.HeavyOnHip)],
                            animStyle: WeaponAnimationStyle.HeavyOnHip);
                    }

                    currentBrowserSlot = Slot.Pistol;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Pistol]} Browser", ref showPistolBrowser);
                    if (showPistolBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Pistol)],
                            animStyle: WeaponAnimationStyle.Pistol);
                    }

                    currentBrowserSlot = Slot.Rifle;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Rifle]} Browser", ref showRifleBrowser);
                    if (showRifleBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Rifle)],
                            animStyle: WeaponAnimationStyle.Rifle);
                    }

                    currentBrowserSlot = Slot.Fist;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Fist]} Browser", ref showFistBrowser);
                    if (showFistBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Fist)],
                            animStyle: WeaponAnimationStyle.Fist);
                    }

                    currentBrowserSlot = Slot.Staff;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.Staff]} Browser", ref showStaffBrowser);
                    if (showStaffBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Staff)],
                            animStyle: WeaponAnimationStyle.Staff);
                    }

                    currentBrowserSlot = Slot.EldarRifle;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.EldarRifle]} Browser", ref showEldarRifleBrowser);
                    if (showEldarRifleBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarRifle)],
                            animStyle: WeaponAnimationStyle.EldarRifle);
                    }

                    currentBrowserSlot = Slot.EldarAssault;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.EldarAssault]} Browser", ref showEldarAssaultBrowser);
                    if (showEldarAssaultBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarAssault)],
                            animStyle: WeaponAnimationStyle.EldarAssault);
                    }

                    currentBrowserSlot = Slot.EldarHeavyOnHip;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.EldarHeavyOnHip]} Browser", ref showEldarHeavyOnHipBrowser);
                    if (showEldarHeavyOnHipBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarHeavyOnHip)],
                            animStyle: WeaponAnimationStyle.EldarHeavyOnHip);
                    }

                    currentBrowserSlot = Slot.EldarHeavyOnShoulder;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.EldarHeavyOnShoulder]} Browser",
                        ref showEldarHeavyOnShoulderBrowser);
                    if (showEldarHeavyOnShoulderBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarHeavyOnShoulder)],
                            animStyle: WeaponAnimationStyle.EldarHeavyOnShoulder);
                    }

                    currentBrowserSlot = Slot.OneHandedHammer;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.OneHandedHammer]} Browser", ref showOneHandedHammerBrowser);
                    if (showOneHandedHammerBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.OneHandedHammer)],
                            animStyle: WeaponAnimationStyle.OneHandedHammer);
                    }

                    currentBrowserSlot = Slot.TwoHandedHammer;
                    OverrideGUI();
                    DisclosureToggle($"Show {SlotName[Slot.TwoHandedHammer]} Browser", ref showTwoHandedHammerBrowser);
                    if (showTwoHandedHammerBrowser) {
                        BrowserGUI<BlueprintItemEquipmentHand>(WeaponBrowsers[currentBrowserSlot],
                            EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.TwoHandedHammer)],
                            animStyle: WeaponAnimationStyle.TwoHandedHammer);
                    }
                }
            }

            Space(5);
        }
        else {
            Label("No Units to select. Load a safe first!".color(RGBA.medred).Bold());
        }
    }
    public static string GetKey(string id) {
        if (KeyCache.TryGetValue(id, out var ret)) {
            return ret;
        }
        var blueprint = ResourcesLibrary.BlueprintsCache.Load(id);
        try {
            if (blueprint is IUIDataProvider uiDataProvider) {
                string name;
                bool isEmpty = true;
                try {
                    isEmpty = string.IsNullOrEmpty(uiDataProvider.Name);
                } catch (NullReferenceException) {
                }
                if (isEmpty) {
                    name = blueprint.name;
                } else {
                    name = uiDataProvider.Name;
                    if (name == "<null>" || name.StartsWith("[unknown key: ")) {
                        name = blueprint.name;
                    } else {
                        name += $" : {blueprint.name}";
                    }
                }
                KeyCache[id] = name + $" ({id})";
                return KeyCache[id];
            }
            KeyCache[id] = blueprint.name + $" ({id})";
            return KeyCache[id];
        } catch (Exception ex) {
            log.Log(ex.ToString());
            log.Log($"-------{blueprint}-----{id}");
            return $"Error ({id})";
        }
    }
    public static void SpriteGUI(string id, float scaling = 0.5f, int targetWidth = 0) {
        var blueprint = ResourcesLibrary.BlueprintsCache.Load(id) as BlueprintItemEquipment;
        var sprite = blueprint.Icon;
        float w, h;
        if (sprite == null) {
            if (targetWidth == 0) {
                w = sprite.rect.width * scaling;
                h = w;
            } else {
                w = targetWidth;
                h = w;
            }
        } else {
            if (targetWidth == 0) {
                w = sprite.rect.width * scaling;
                h = sprite.rect.height * scaling;
            } else {
                w = targetWidth;
                h = targetWidth * (sprite.rect.height / sprite.rect.width);
            }
        }
        using (VerticalScope(GUILayout.Width(w + 10))) {
            bool hasOverride = false;
            if (getDictForSlot(currentBrowserSlot).TryGetValue(pickedUnit.UniqueId, out var Override)) {
                hasOverride = true;
            }
            if (sprite == null) {
                if (GUILayout.Button("No Icon", rarityStyle, GUILayout.Width(w), GUILayout.Height(h))) {
                    UpdateEquippedItems(currentBrowserSlot, hasOverride, Override.Item1, true, id);
                }
            } else {
                if (GUILayout.Button(sprite.texture, rarityStyle, GUILayout.Width(w), GUILayout.Height(h))) {
                    UpdateEquippedItems(currentBrowserSlot, hasOverride, Override.Item1, true, id);
                }
            }
            var key = GetKey(id);
            if (HasEE.TryGetValue(id, out var b) && !b) key = "Has no EquipmentEntity for current Unit! ".color(RGBA.red) + key;
            Label(key);
        }
    }
    private static void WeaponSlotSelector() {
        if (mainHand) {
            Space(5);
            ActionButton("Current Hand: Main", () => {
                mainHand = !mainHand;
            }, AutoWidth());
            Space(5);
        }
        else {
            Space(5);
            ActionButton("Current Hand: Off", () => {
                mainHand = !mainHand;
            }, AutoWidth());
            Space(5);
        }
        GUILayout.Label("Weapon set");
        using (HorizontalScope()) {
            using (VerticalScope(Width(10))) {
                if (GUILayout.Toggle(!m_weaponSet, "1", AutoWidth())) {
                    m_weaponSet = false;
                }
            }
            using (VerticalScope(Width(10))) {
                if (GUILayout.Toggle(m_weaponSet, "2", AutoWidth())) {
                    m_weaponSet = true;
                }
            }
        }
        var currentAnimStyle = mainHand == true
            ? pickedUnit.View.HandsEquipment.Sets.First(a => a.Value.MainHand.m_SlotIdx == weaponSet).Value
                .MainHand.VisibleItemBlueprint?.VisualParameters?.AnimStyle
            : pickedUnit.View.HandsEquipment.Sets.First(a => a.Value.OffHand.m_SlotIdx == weaponSet).Value
                .OffHand.VisibleItemBlueprint?.VisualParameters?.AnimStyle;
        var animStyleString = currentAnimStyle == null
            ? "None"
            : SlotName[getSlotFromAnimStyle((WeaponAnimationStyle)currentAnimStyle)];
        GUILayout.Label($"Current Animation Style: {animStyleString}");
        showAllWeaponBrowsers = GUILayout.Toggle(showAllWeaponBrowsers, "Show all weapon browsers");
        Space(5);
    }
    public static void BrowserGUI<T>(Browser<string, string> browser, HashSet<string> knownIds, WeaponAnimationStyle animStyle = WeaponAnimationStyle.None) where T : BlueprintItemEquipment {
        using (HorizontalScope()) {
            Space(25);
            using (VerticalScope()) {
                Func<IEnumerable<string>> available;
                IEnumerable<string> current;
                if (animStyle == WeaponAnimationStyle.None) {
                    available = () => Kingmaker.Cheats.Utilities.GetBlueprintGuids<T>().Where(HasEEsForCurrentUnit);
                    current = knownIds.Where(HasEEsForCurrentUnit);
                }
                else {
                    available = () =>
                        Kingmaker.Cheats.Utilities.GetBlueprintGuids<T>()
                            .Where(item => MatchesAnimStyle(item, animStyle));
                    current = knownIds.Where(item => MatchesAnimStyle(item, animStyle));
                }
                browser.OnGUI(current, available, id => id, GetKey, id => [GetKey(id)], null, null, null, 50, true, true, "", false, null,
                            (definitions, _currentDict) => {
                                var count = definitions.Count;
                                int itemsPerRow = 12;
                                using (VerticalScope()) {
                                    for (var ii = 0; ii < count;) {
                                        var tmp = ii;
                                        using (HorizontalScope()) {
                                            for (; ii < Math.Min(tmp + itemsPerRow, count); ii++) {
                                                var customID = definitions[ii];
                                                // 6 Portraits per row; 692px per image + buffer
                                                SpriteGUI(customID, 0.5f, (int)(Params.WindowWidth - itemsPerRow * 200.0f/6.0f) / itemsPerRow);
                                            }
                                        }
                                    }
                                }
                            });
            }
        }
    }
    public static bool MatchesAnimStyle(string id, WeaponAnimationStyle animStyle) {
        var bp = ResourcesLibrary.BlueprintsCache.Load(id);
        if ((bp as BlueprintItemEquipmentHand).VisualParameters.AnimStyle == animStyle)
            return true;
        return false;
    }
    private static Dictionary<string, bool> HasEE = new(); 
    public static bool HasEEsForCurrentUnit(string id) {
        bool hasEEs = (ExtractEEs(ResourcesLibrary.BlueprintsCache.Load(id) as BlueprintItemEquipment, pickedUnit)?.Count() ?? 0) > 0;
        HasEE[id] = hasEEs;
        return hasEEs || settings.shouldShowItemsWithoutEE;
    }

#if DEBUG
    public static bool OnUnload(UnityModManager.ModEntry modEntry) {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif
}