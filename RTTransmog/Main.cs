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
using Kingmaker.Blueprints.Items;
using Kingmaker.View.Animation;

namespace RTTransmog;

#if DEBUG
[EnableReloading]
#endif
public static class Main
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger log;

    internal static UnityModManager.ModEntry mod;

    //private static Browser<string, string> MainhandWeaponBrowser = new(true, true, false, true);
    //private static Browser<string, string> OffhandWeaponBrowser = new(true, true, false, true);
    private static Browser<string, string> ShoulderBrowser = new(true, true, false, true);
    private static Browser<string, string> Ring1Browser = new(true, true, false, true);
    private static Browser<string, string> Ring2Browser = new(true, true, false, true);
    private static Browser<string, string> NeckBrowser = new(true, true, false, true);
    private static Browser<string, string> HeadBrowser = new(true, true, false, true);
    private static Browser<string, string> GlovesBrowser = new(true, true, false, true);
    private static Browser<string, string> FeetBrowser = new(true, true, false, true);

    private static Browser<string, string> ArmorBrowser = new(true, true, false, true);

    //private static Browser<string, string> WeaponBrowser = new(true, true, false, true);
    //weapon browsers
    private static Browser<string, string> KnifeBrowser = new(true, true, false, true);
    private static Browser<string, string> FencingBrowser = new(true, true, false, true);
    private static Browser<string, string> AxeTwoHandedBrowser = new(true, true, false, true);
    private static Browser<string, string> AssaultBrowser = new(true, true, false, true);
    private static Browser<string, string> BrutalOneHandedBrowser = new(true, true, false, true);
    private static Browser<string, string> BrutalTwoHandedBrowser = new(true, true, false, true);
    private static Browser<string, string> HeavyOnHipBrowser = new(true, true, false, true);
    private static Browser<string, string> PistolBrowser = new(true, true, false, true);
    private static Browser<string, string> RifleBrowser = new(true, true, false, true);
    private static Browser<string, string> FistBrowser = new(true, true, false, true);
    private static Browser<string, string> StaffBrowser = new(true, true, false, true);
    private static Browser<string, string> EldarRifleBrowser = new(true, true, false, true);
    private static Browser<string, string> EldarAssaultBrowser = new(true, true, false, true);
    private static Browser<string, string> EldarHeavyOnHipBrowser = new(true, true, false, true);
    private static Browser<string, string> EldarHeavyOnShoulderBrowser = new(true, true, false, true);
    private static Browser<string, string> OneHandedHammerBrowser = new(true, true, false, true);

    private static Browser<string, string> TwoHandedHammerBrowser = new(true, true, false, true);

    //
    private static Dictionary<string, string> KeyCache = new();
    private static bool showMainhandWeaponBrowser = false;
    private static bool showOffhandWeaponBrowser = false;
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

    public enum Slot
    {
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

        // one per anim style
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

    public static WeaponAnimationStyle getAnimStyleFromSlot(Slot slot) // Needed, otherwise the indices are misaligned.
    {
        if (WeaponAnimationStyle.TryParse<WeaponAnimationStyle>(slot.ToString(), out var animStyle))
        {
            return animStyle;
        }

        throw new Exception("Failed to parse weapon animation style.");
    }

    public static bool Load(UnityModManager.ModEntry modEntry)
    {
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

    public static void OnSaveGUI(ModEntry modEntry)
    {
        settings.Save(modEntry);
    }

    public static void FirstInit()
    {
        EntityPartStorage.perSave.didFirstInit = true;
        foreach (var item in Game.Instance.Player.Inventory)
        {
            EventHandler.Instance.HandleItemsAdded(Game.Instance.Player.Inventory, item, 1);
        }

        foreach (var item in Game.Instance.Player.PartyAndPets.SelectMany(pap => pap.Inventory.Items))
        {
            EventHandler.Instance.HandleItemsAdded(Game.Instance.Player.Inventory, item, 1);
        }
    }

    public static Dictionary<string, (string, string)> getDictForSlot(Slot slot)
    {
        if (((int)slot) >= 10) // cant do comparisons in a switch statement, and this saves like 10 lines.
        {
            return EntityPartStorage.perSave.Weapons[getAnimStyleFromSlot(slot)];
        }

        switch (slot)
        {
            // case Slot.Mainhand: return EntityPartStorage.perSave.MainhandWeapons;
            // case Slot.Offhand: return EntityPartStorage.perSave.OffhandWeapons;
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
        WeaponAnimationStyle style)
    {
        return EntityPartStorage.perSave.Weapons;
    }

    public static void UpdateEquippedItems(Slot slot, bool removeOld, string oldId, bool addNew = false,
        string newId = "")
    {
        if (removeOld)
        {
            var oldBp = ResourcesLibrary.BlueprintsCache.Load(oldId) as BlueprintItemEquipment;
            var EEs = ExtractEEs(oldBp);
            pickedUnit.View.CharacterAvatar.RemoveEquipmentEntities(EEs);
        }

        PartUnitBody body = pickedUnit.OwnerEntity.Body;
        ItemSlot itemSlot = null;
        switch (slot)
        {
            case Slot.Mainhand:
            {
                itemSlot = body.m_HandsEquipmentSets[body.m_CurrentHandsEquipmentSetIndex].PrimaryHand;
            }
                ;
                break;
            case Slot.Offhand:
            {
                itemSlot = body.m_HandsEquipmentSets[body.m_CurrentHandsEquipmentSetIndex].SecondaryHand;
            }
                ;
                break;
            case Slot.Shoulder:
            {
                itemSlot = body.Shoulders;
            }
                ;
                break;
            case Slot.Ring1:
            {
                itemSlot = body.Ring1;
            }
                ;
                break;
            case Slot.Ring2:
            {
                itemSlot = body.Ring2;
            }
                ;
                break;
            case Slot.Neck:
            {
                itemSlot = body.Neck;
            }
                ;
                break;
            case Slot.Head:
            {
                itemSlot = body.Head;
            }
                ;
                break;
            case Slot.Gloves:
            {
                itemSlot = body.Gloves;
            }
                ;
                break;
            case Slot.Feet:
            {
                itemSlot = body.Feet;
            }
                ;
                break;
            case Slot.Armor:
            {
                itemSlot = body.Armor;
            }
                ;
                break;
        }

        if (addNew)
        {
            getDictForSlot(slot)[pickedUnit.UniqueId] = (newId, GetKey(newId));
            EntityPartStorage.SavePerSaveSettings();
        }
        
        pickedUnit.View.HandsEquipment.ChangeEquipmentWithoutAnimation();
        
        if (itemSlot == null) return;
        EventBus.RaiseEvent<IUnitEquipmentHandler>(pickedUnit,
            delegate(IUnitEquipmentHandler h) { h.HandleEquipmentSlotUpdated(itemSlot, itemSlot.MaybeItem ?? null); },
            true);
    }

    public static IEnumerable<EquipmentEntity> ExtractEEs(BlueprintItemEquipment blueprintItemEquipment,
        BaseUnitEntity unit = null)
    {
        unit ??= pickedUnit;
        IEnumerable<EquipmentEntity> enumerable;
        Gender gender = unit.Gender;
        BlueprintRace race = unit.Progression.Race;
        Race race2 = ((race != null) ? race.RaceId : Race.Human);
        enumerable = ((blueprintItemEquipment != null && blueprintItemEquipment.EquipmentEntity != null)
            ? blueprintItemEquipment.EquipmentEntity.Load(gender, race2)
            : Enumerable.Empty<EquipmentEntity>());
        return enumerable;
    }

    public static void OverrideGUI()
    {
        Div();
        Label($"Overrides for {currentBrowserSlot}".color(RGBA.lime).Bold());
        using (HorizontalScope())
        {
            bool hasOverride = false;
            string name = "None".color(RGBA.aqua);
            if (getDictForSlot(currentBrowserSlot).TryGetValue(pickedUnit.UniqueId, out var Override))
            {
                name = Override.Item2.color(RGBA.aqua);
                hasOverride = true;
            }

            ActionButton("Reset Override", () =>
            {
                if (hasOverride)
                {
                    getDictForSlot(currentBrowserSlot).Remove(pickedUnit.UniqueId);
                    UpdateEquippedItems(currentBrowserSlot, true, Override.Item1);
                    EntityPartStorage.SavePerSaveSettings();
                }
            }, AutoWidth());
            Label($"Current Override: {name}");
        }
    }

    public static void ResetBrowsers()
    {
        // MainhandWeaponBrowser.ResetSearch(); deprecated
        // OffhandWeaponBrowser.ResetSearch(); deprecated
        ShoulderBrowser.ResetSearch();
        Ring1Browser.ResetSearch();
        Ring2Browser.ResetSearch();
        NeckBrowser.ResetSearch();
        HeadBrowser.ResetSearch();
        GlovesBrowser.ResetSearch();
        FeetBrowser.ResetSearch();
        ArmorBrowser.ResetSearch();

        // weapon browsers reset
        KnifeBrowser.ResetSearch();
        FencingBrowser.ResetSearch();
        AxeTwoHandedBrowser.ResetSearch();
        AssaultBrowser.ResetSearch();
        BrutalOneHandedBrowser.ResetSearch();
        BrutalTwoHandedBrowser.ResetSearch();
        HeavyOnHipBrowser.ResetSearch();
        PistolBrowser.ResetSearch();
        RifleBrowser.ResetSearch();
        FistBrowser.ResetSearch();
        StaffBrowser.ResetSearch();
        EldarRifleBrowser.ResetSearch();
        EldarAssaultBrowser.ResetSearch();
        EldarHeavyOnHipBrowser.ResetSearch();
        EldarHeavyOnShoulderBrowser.ResetSearch();
        OneHandedHammerBrowser.ResetSearch();
        TwoHandedHammerBrowser.ResetSearch();
    }

    public static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        var units = new List<BaseUnitEntity>() { Game.Instance?.Player?.MainCharacterEntity };
        units.AddRange(Game.Instance.Player.ActiveCompanions ?? new());
        units = units.Where(u => u != null).ToList();
        if (units.Count > 0)
        {
            GUILayout.Label("Character to change:");

            int selectedIndex = pickedUnit != null ? Array.IndexOf(units.ToArray(), pickedUnit) : 0;
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
                pickedUnit = null;
            }

            int newIndex = GUILayout.SelectionGrid(selectedIndex, units.Select(m => m.CharacterName).ToArray(), 6);
            if (selectedIndex != newIndex || pickedUnit == null)
            {
                pickedUnit = units[newIndex];
                ResetBrowsers();
            }

            Div();
            if (TogglePrivate(
                    "Show Items without Equipment Entity (those are by default hidden because I assume they don't change visuals). This could be helpful if you don't want equipment in a slot to show because you can just override the slot with an item with no visuals.",
                    ref settings.shouldShowItemsWithoutEE, false, false, 0, AutoWidth()))
            {
                ResetBrowsers();
            }

            /*
            currentBrowserSlot = Slot.Mainhand;
            OverrideGUI();
            DisclosureToggle("Show Mainhand Weapon Browser", ref showMainhandWeaponBrowser);
            if (showMainhandWeaponBrowser) {
                BrowserGUI<BlueprintItemWeapon>(MainhandWeaponBrowser, EntityPartStorage.perSave.KnownWeapons);
            }

            currentBrowserSlot = Slot.Offhand;
            OverrideGUI();
            DisclosureToggle("Show Offhand Weapon Browser", ref showOffhandWeaponBrowser);
            if (showOffhandWeaponBrowser) {
                BrowserGUI<BlueprintItemWeapon>(OffhandWeaponBrowser, EntityPartStorage.perSave.KnownWeapons);
            }
            */
            currentBrowserSlot = Slot.Shoulder;
            OverrideGUI();
            DisclosureToggle("Show Shoulder Browser", ref showShoulderBrowser);
            if (showShoulderBrowser)
            {
                BrowserGUI<BlueprintItemEquipmentShoulders>(ShoulderBrowser, EntityPartStorage.perSave.KnownShoulders);
            }

            currentBrowserSlot = Slot.Ring1;
            OverrideGUI();
            DisclosureToggle("Show Ring1 Browser", ref showRing1Browser);
            if (showRing1Browser)
            {
                BrowserGUI<BlueprintItemEquipmentRing>(Ring1Browser, EntityPartStorage.perSave.KnownRing);
            }

            currentBrowserSlot = Slot.Ring2;
            OverrideGUI();
            DisclosureToggle("Show Ring2 Browser", ref showRing2Browser);
            if (showRing2Browser)
            {
                BrowserGUI<BlueprintItemEquipmentRing>(Ring2Browser, EntityPartStorage.perSave.KnownRing);
            }

            currentBrowserSlot = Slot.Neck;
            OverrideGUI();
            DisclosureToggle("Show Neck Browser", ref showNeckBrowser);
            if (showNeckBrowser)
            {
                BrowserGUI<BlueprintItemEquipmentNeck>(NeckBrowser, EntityPartStorage.perSave.KnownNeck);
            }

            currentBrowserSlot = Slot.Head;
            OverrideGUI();
            DisclosureToggle("Show Head Browser", ref showHeadBrowser);
            if (showHeadBrowser)
            {
                BrowserGUI<BlueprintItemEquipmentHead>(HeadBrowser, EntityPartStorage.perSave.KnownHead);
            }

            currentBrowserSlot = Slot.Gloves;
            OverrideGUI();
            DisclosureToggle("Show Gloves Browser", ref showGlovesBrowser);
            if (showGlovesBrowser)
            {
                BrowserGUI<BlueprintItemEquipmentGloves>(GlovesBrowser, EntityPartStorage.perSave.KnownGloves);
            }

            currentBrowserSlot = Slot.Feet;
            OverrideGUI();
            DisclosureToggle("Show Feet Browser", ref showFeetBrowser);
            if (showFeetBrowser)
            {
                BrowserGUI<BlueprintItemEquipmentFeet>(FeetBrowser, EntityPartStorage.perSave.KnownFeet);
            }

            currentBrowserSlot = Slot.Armor;
            OverrideGUI();
            DisclosureToggle("Show Armor Browser", ref showArmorBrowser);
            if (showArmorBrowser)
            {
                BrowserGUI<BlueprintItemArmor>(ArmorBrowser, EntityPartStorage.perSave.KnownArmor);
            }

            // Weapon browsers.
            using (VerticalScope())
            {
                Space(10);
                DisclosureToggle("Show Weapon Browsers", ref showWeaponBrowsers);
                Space(10);
            }

            if (showWeaponBrowsers)
            {
                currentBrowserSlot = Slot.Knife;
                OverrideGUI();
                DisclosureToggle("Show Knife Browser", ref showKnifeBrowser);
                if (showKnifeBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(KnifeBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Knife)],
                        animStyle: WeaponAnimationStyle.Knife);
                }

                currentBrowserSlot = Slot.Fencing;
                OverrideGUI();
                DisclosureToggle("Show Fencing Browser", ref showFencingBrowser);
                if (showFencingBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(FencingBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Fencing)],
                        animStyle: WeaponAnimationStyle.Fencing);
                }

                currentBrowserSlot = Slot.AxeTwoHanded;
                OverrideGUI();
                DisclosureToggle("Show AxeTwoHanded Browser", ref showAxeTwoHandedBrowser);
                if (showAxeTwoHandedBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(AxeTwoHandedBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.AxeTwoHanded)],
                        animStyle: WeaponAnimationStyle.AxeTwoHanded);
                }

                currentBrowserSlot = Slot.Assault;
                OverrideGUI();
                DisclosureToggle("Show Assault Browser", ref showAssaultBrowser);
                if (showAssaultBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(AssaultBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Assault)],
                        animStyle: WeaponAnimationStyle.Assault);
                }

                currentBrowserSlot = Slot.BrutalOneHanded;
                OverrideGUI();
                DisclosureToggle("Show BrutalOneHanded Browser", ref showBrutalOneHandedBrowser);
                if (showBrutalOneHandedBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(BrutalOneHandedBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.BrutalOneHanded)],
                        animStyle: WeaponAnimationStyle.BrutalOneHanded);
                }

                currentBrowserSlot = Slot.BrutalTwoHanded;
                OverrideGUI();
                DisclosureToggle("Show BrutalTwoHanded Browser", ref showBrutalTwoHandedBrowser);
                if (showBrutalTwoHandedBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(BrutalTwoHandedBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.BrutalTwoHanded)],
                        animStyle: WeaponAnimationStyle.BrutalTwoHanded);
                }

                currentBrowserSlot = Slot.HeavyOnHip;
                OverrideGUI();
                DisclosureToggle("Show HeavyOnHip Browser", ref showHeavyOnHipBrowser);
                if (showHeavyOnHipBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(HeavyOnHipBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.HeavyOnHip)],
                        animStyle: WeaponAnimationStyle.HeavyOnHip);
                }
                
                currentBrowserSlot = Slot.Pistol;
                OverrideGUI();
                DisclosureToggle("Show Pistol Browser", ref showPistolBrowser);
                if (showPistolBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(PistolBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Pistol)],
                        animStyle: WeaponAnimationStyle.Pistol);
                }

                currentBrowserSlot = Slot.Rifle;
                OverrideGUI();
                DisclosureToggle("Show Rifle Browser", ref showRifleBrowser);
                if (showRifleBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(RifleBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Rifle)],
                        animStyle: WeaponAnimationStyle.Rifle);
                }

                currentBrowserSlot = Slot.Fist;
                OverrideGUI();
                DisclosureToggle("Show Fist Browser", ref showFistBrowser);
                if (showFistBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(FistBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Fist)],
                        animStyle: WeaponAnimationStyle.Fist);
                }

                currentBrowserSlot = Slot.Staff;
                OverrideGUI();
                DisclosureToggle("Show Staff Browser", ref showStaffBrowser);
                if (showStaffBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(StaffBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.Staff)],
                        animStyle: WeaponAnimationStyle.Staff);
                }

                currentBrowserSlot = Slot.EldarRifle;
                OverrideGUI();
                DisclosureToggle("Show EldarRifle Browser", ref showEldarRifleBrowser);
                if (showEldarRifleBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(EldarRifleBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarRifle)],
                        animStyle: WeaponAnimationStyle.EldarRifle);
                }

                currentBrowserSlot = Slot.EldarAssault;
                OverrideGUI();
                DisclosureToggle("Show EldarAssault Browser", ref showEldarAssaultBrowser);
                if (showEldarAssaultBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(EldarAssaultBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarAssault)],
                        animStyle: WeaponAnimationStyle.EldarAssault);
                }

                currentBrowserSlot = Slot.EldarHeavyOnHip;
                OverrideGUI();
                DisclosureToggle("Show EldarHeavyOnHip Browser", ref showEldarHeavyOnHipBrowser);
                if (showEldarHeavyOnHipBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(EldarHeavyOnHipBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarHeavyOnHip)],
                        animStyle: WeaponAnimationStyle.EldarHeavyOnHip);
                }

                currentBrowserSlot = Slot.EldarHeavyOnShoulder;
                OverrideGUI();
                DisclosureToggle("Show EldarHeavyOnShoulder Browser", ref showEldarHeavyOnShoulderBrowser);
                if (showEldarHeavyOnShoulderBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(EldarHeavyOnShoulderBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.EldarHeavyOnShoulder)],
                        animStyle: WeaponAnimationStyle.EldarHeavyOnShoulder);
                }

                currentBrowserSlot = Slot.OneHandedHammer;
                OverrideGUI();
                DisclosureToggle("Show OneHandedHammer Browser", ref showOneHandedHammerBrowser);
                if (showOneHandedHammerBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(OneHandedHammerBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.OneHandedHammer)],
                        animStyle: WeaponAnimationStyle.OneHandedHammer);
                }

                currentBrowserSlot = Slot.TwoHandedHammer;
                OverrideGUI();
                DisclosureToggle("Show TwoHandedHammer Browser", ref showTwoHandedHammerBrowser);
                if (showTwoHandedHammerBrowser)
                {
                    BrowserGUI<BlueprintItemEquipmentHand>(TwoHandedHammerBrowser,
                        EntityPartStorage.perSave.KnownWeapons[getAnimStyleFromSlot(Slot.TwoHandedHammer)],
                        animStyle: WeaponAnimationStyle.TwoHandedHammer);
                }
            }
        }
        else
        {
            Label("No Units to select. Load a safe first!".color(RGBA.medred).Bold());
        }
    }

    public static string GetKey(string id)
    {
        if (KeyCache.TryGetValue(id, out var ret))
        {
            return ret;
        }

        var blueprint = ResourcesLibrary.BlueprintsCache.Load(id);
        try
        {
            if (blueprint is IUIDataProvider uiDataProvider)
            {
                string name;
                bool isEmpty = true;
                try
                {
                    isEmpty = string.IsNullOrEmpty(uiDataProvider.Name);
                }
                catch (NullReferenceException)
                {
                }

                if (isEmpty)
                {
                    name = blueprint.name;
                }
                else
                {
                    name = uiDataProvider.Name;
                    if (name == "<null>" || name.StartsWith("[unknown key: "))
                    {
                        name = blueprint.name;
                    }
                    else
                    {
                        name += $" : {blueprint.name}";
                    }
                }

                KeyCache[id] = name + $" ({id})";
                return KeyCache[id];
            }

            KeyCache[id] = blueprint.name + $" ({id})";
            return KeyCache[id];
        }
        catch (Exception ex)
        {
            log.Log(ex.ToString());
            log.Log($"-------{blueprint}-----{id}");
            return $"Error ({id})";
        }
    }

    public static void SpriteGUI(string id, float scaling = 0.5f, int targetWidth = 0)
    {
        var blueprint = ResourcesLibrary.BlueprintsCache.Load(id) as BlueprintItemEquipment;
        var sprite = blueprint.Icon;
        float w, h;
        if (sprite == null)
        {
            if (targetWidth == 0)
            {
                w = sprite.rect.width * scaling;
                h = w;
            }
            else
            {
                w = targetWidth;
                h = w;
            }
        }
        else
        {
            if (targetWidth == 0)
            {
                w = sprite.rect.width * scaling;
                h = sprite.rect.height * scaling;
            }
            else
            {
                w = targetWidth;
                h = targetWidth * (sprite.rect.height / sprite.rect.width);
            }
        }

        using (VerticalScope(GUILayout.Width(w + 10)))
        {
            bool hasOverride = false;
            if (getDictForSlot(currentBrowserSlot).TryGetValue(pickedUnit.UniqueId, out var Override))
            {
                hasOverride = true;
            }

            if (sprite == null)
            {
                if (GUILayout.Button("No Icon", rarityStyle, GUILayout.Width(w), GUILayout.Height(h)))
                {
                    UpdateEquippedItems(currentBrowserSlot, hasOverride, Override.Item1, true, id);
                }
            }
            else
            {
                if (GUILayout.Button(sprite.texture, rarityStyle, GUILayout.Width(w), GUILayout.Height(h)))
                {
                    UpdateEquippedItems(currentBrowserSlot, hasOverride, Override.Item1, true, id);
                }
            }

            Label(GetKey(id));
        }
    }

    public static void BrowserGUI<T>(Browser<string, string> browser, HashSet<string> knownIds,
        WeaponAnimationStyle animStyle = WeaponAnimationStyle.None)
        where T : BlueprintItemEquipment
    {
        using (HorizontalScope())
        {
            Space(25);
            using (VerticalScope())
            {
                Func<IEnumerable<string>> available;
                IEnumerable<string> current;
                if (animStyle == WeaponAnimationStyle.None)
                {
                    available = () => Kingmaker.Cheats.Utilities.GetBlueprintGuids<T>().Where(HasEEsForCurrentUnit);
                    current = knownIds.Where(HasEEsForCurrentUnit);
                }
                else
                {
                    available = () =>
                        Kingmaker.Cheats.Utilities.GetBlueprintGuids<T>()
                            .Where(item => MatchesAnimStyle(item, animStyle));
                    current = knownIds.Where(item => MatchesAnimStyle(item, animStyle));
                }

                browser.OnGUI(current,
                    available, id => id,
                    GetKey, id => [GetKey(id)], null, null, null, 50, true, true, "", false, null,
                    (definitions, _currentDict) =>
                    {
                        var count = definitions.Count;
                        int itemsPerRow = 12;
                        using (VerticalScope())
                        {
                            for (var ii = 0; ii < count;)
                            {
                                var tmp = ii;
                                using (HorizontalScope())
                                {
                                    for (; ii < Math.Min(tmp + itemsPerRow, count); ii++)
                                    {
                                        var customID = definitions[ii];
                                        // 6 Portraits per row; 692px per image + buffer
                                        SpriteGUI(customID, 0.5f,
                                            (int)(Params.WindowWidth - itemsPerRow * 200.0f / 6.0f) / itemsPerRow);
                                    }
                                }
                            }
                        }
                    });
            }
        }
    }

    public static bool MatchesAnimStyle(string id, WeaponAnimationStyle animStyle)
    {
        var bp = ResourcesLibrary.BlueprintsCache.Load(id);
        if ((bp as BlueprintItemEquipmentHand).VisualParameters.AnimStyle == animStyle)
            return true;
        return false;
    }

    public static bool HasEEsForCurrentUnit(string id)
    {
        var bp = ResourcesLibrary.BlueprintsCache.Load(id);
        //if (bp.GetType().IsSubclassOf(typeof(BlueprintItemEquipmentHand))) return true; // so it actually shows up w/o ticking the box to show things with no EEs // TEMP, REMOVE LATER
        bool hasEEs = (ExtractEEs(bp as BlueprintItemEquipment, pickedUnit)
            ?.Count() ?? 0) > 0;
        return hasEEs || settings.shouldShowItemsWithoutEE;
    }

#if DEBUG
    public static bool OnUnload(UnityModManager.ModEntry modEntry)
    {
        HarmonyInstance.UnpatchAll(modEntry.Info.Id);
        return true;
    }
#endif
}