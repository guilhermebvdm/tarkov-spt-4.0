using System;
using System.Collections.Generic;

namespace Orbit.Compat;

/// <summary>
/// SPT 4.0 compatibility layer — runtime type names.
///
/// Ghost Mode decides what a native (non-ORBIT) bot may do while asleep from the name of its active brain
/// layer, read with <c>GetType().Name</c> and compared against 4.1 class names ("FollowerPatrolLayer", ...).
/// On SPT 4.0 the same classes are still called GClassNNNN, so every one of those comparisons would be false
/// and native bots would silently never qualify. The compiler cannot see this: both sides are strings.
///
/// <see cref="Of"/> returns the 4.1 name for the types listed here and the plain runtime name for anything
/// else, which is what the callers compared against on 4.1 (an unlisted type matched nothing there either).
/// Every 4.1 type name the sources compare as a string must have a row; scripts/check-type-name-literals.py
/// fails when one is missing.
/// </summary>
internal static class Spt40TypeNames
{
    // 4.0 type -> 4.1 name. Pairs from the official 4.0 -> 4.1 table; the trailing comment is the literal
    // each layer returns from Name() in references/eft-decompiled, used to confirm the pair.
    private static readonly Dictionary<Type, string> Names = new()
    {
        { typeof(Class99), "AdvAssaultTargetLayer" },                 // "AdvAssaultTarget"
        { typeof(GClass52), "BoarClosePatrolLayer" },                 // "BoarClPatrol"
        { typeof(GClass53), "BoarPatrolLayer" },                      // "BoarPatrol"
        { typeof(GClass54), "BossBoarPatrolLayer" },                  // "BsBoarPatrol"
        { typeof(GClass79), "BirdEyePatrolLayer" },                   // "PtrlBirdEye"
        { typeof(GClass86), "FollowerPatrolLayer" },                  // "PatrolFollower"
        { typeof(GClass91), "HoldNearBossLayer" },                    // "HoldNearBoss"
        { typeof(GClass92), "KolontayHoldNearBossLayer" },            // inherits GClass91
        { typeof(GClass119), "MarksmanTargetLayer" },                 // "MarksmanTarget"
        { typeof(GClass123), "PartisanPlantingTargetLayer" },         // "PartisanMine"
        { typeof(GClass124), "PartisanPlantingTargetManyLayer" },     // "PartMineAll"
        { typeof(GClass133), "PatrolAssaultLayer" },                  // "PatrolAssault"
        { typeof(GClass134), "FullMapPatrolLayer" },                  // "Full map patrol"
        { typeof(GClass135), "PatrolStayAtPositionLayer" },           // "StayAtPos"
        { typeof(GClass138), "PatrolStayAtPositionLayerWithIndoor" }, // inherits GClass135
        { typeof(GClass172), "ZryachiyPatrolLayer" },                 // "LayingPatrol"
        { typeof(GClass174), "StandByLogicLayer" },                   // "StandBy"
    };

    /// <summary>4.1 name of the object's exact runtime type, or its runtime name when the type is not listed.</summary>
    public static string Of(object instance)
    {
        if (instance == null) return null;
        var type = instance.GetType();
        return Names.TryGetValue(type, out var name) ? name : type.Name;
    }
}
