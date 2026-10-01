using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;

namespace Orbit.Compat;

/// <summary>
/// SPT 4.0 compatibility layer — game members looked up by name at runtime.
///
/// The sources bind some Harmony patches from strings ("_owner", "TryPassCurrentDoor", ...). Those are the
/// real member names of the deobfuscated SPT 4.1 assembly. In the SPT 4.0 assembly the same members carry
/// the older remap names (BotOwner_0, method_2, ...), so the string lookup finds nothing and the mod turns
/// the whole feature off by itself ("body guards unavailable, native sleep disabled"). The compiler cannot
/// see it. Members reached through normal C# syntax are renamed at their use sites instead; this file only
/// holds the ones that exist as strings.
/// </summary>
internal static class Spt40Members
{
    // (type that declares the member on 4.0, 4.1 name) -> 4.0 name.
    // Each row was matched by reading the 4.0 method in references/eft-decompiled (file:line in the comment).
    private static readonly Dictionary<(Type, string), string> Methods = new()
    {
        // BotDoorOpener.cs:455 — picks the interaction point, rolls the breach chance, enters NearDoor.
        { (typeof(BotDoorOpener), "TryPassCurrentDoor"), "method_2" },
        // BotDoorOpener.cs:626 — sets EnteringDoorSequence and runs the look / step-through sequence.
        { (typeof(BotDoorOpener), "RunEnteringDoorSequence"), "method_7" },
        // BotDoorOpener.cs:428 — stops the bot while the door leaf is still swinging.
        { (typeof(BotDoorOpener), "WaitForDoorOpen"), "method_0" },
        // BotDoorOpener.cs:737 — decides Breach / Open / Close and calls Interact.
        { (typeof(BotDoorOpener), "InteractionWithDoor"), "method_8" },
        // BotFirstAidClass.cs:447 — puts the medkit in hands and applies it (called by TryApplyToCurrentPart).
        { (typeof(BotFirstAidClass), "ApplyToSelf"), "method_3" },
    };

    // (type that declares the field on 4.0, 4.1 name) -> 4.0 name, for fields read through reflection.
    private static readonly Dictionary<(Type, string), string> Fields = new()
    {
        { (typeof(PatrolLootPointsData), "_lootingNow"), "LootingNow" },             // PatrolLootPointsData.cs:135
        { (typeof(PatrollingData), "_comeTime"), "ComeTime" },                       // PatrollingData.cs:34
        { (typeof(PatrollingData), "_reservChoosedTime"), "ReservChoosedTime" },     // PatrollingData.cs:37
        { (typeof(PatrolPointChooserBasic), "_nextChangeWay"), "NextChangeWay" },    // PatrolPointChooserBasic.cs:88
        // "_shallStartInteract": private and still obfuscated on 4.0, and not the same field in each script.
        // It is the flag set when the bot reaches the point and cleared by ManualUpdate when the wait ends.
        { (typeof(DropItemReservWay), "_shallStartInteract"), "bool_1" },            // DropItemReservWay.cs:11 (set :65, cleared :23)
        { (typeof(DropItemAndHealReservWay), "_shallStartInteract"), "bool_0" },     // DropItemAndHealReservWay.cs:11 (set :78, cleared :29)
        { (typeof(UseSurgeKitReservWay), "_shallStartInteract"), "bool_0" },         // UseSurgeKitReservWay.cs:14 (set :88, cleared :32)
    };

    /// <summary>The field the sources name with its 4.1 name, looked up under its 4.0 name; null when absent.</summary>
    public static FieldInfo Field(Type type, string name41)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        for (var t = type; t != null; t = t.BaseType)
        {
            var name = Fields.TryGetValue((t, name41), out var name40) ? name40 : name41;
            var field = t.GetField(name, flags);
            if (field != null) return field;
        }
        return null;
    }

    // The field holding the bot that owns a subsystem is "_owner" on 4.1. On 4.0 it is BotOwner_0 on the
    // classes deriving from GClass429 / GClass177<T> and Owner on BotDoorOpener and the patrol point choosers.
    private static readonly string[] OwnerFieldNames = { "BotOwner_0", "Owner", "_owner" };

    /// <summary>4.0 name of a method the sources name with its 4.1 name; the same name when it did not change.</summary>
    public static string Method(Type type, string name41)
    {
        for (var t = type; t != null; t = t.BaseType)
            if (Methods.TryGetValue((t, name41), out var name40)) return name40;
        return name41;
    }

    /// <summary>The instance field of type BotOwner that 4.1 calls "_owner", searched through the base types.</summary>
    public static FieldInfo OwnerField(Type type)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        foreach (var name in OwnerFieldNames)
            for (var t = type; t != null; t = t.BaseType)
            {
                var field = t.GetField(name, flags);
                if (field != null && field.FieldType == typeof(BotOwner)) return field;
            }
        return null;
    }
}
