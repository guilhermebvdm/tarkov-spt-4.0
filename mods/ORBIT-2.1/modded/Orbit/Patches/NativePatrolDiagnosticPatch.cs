using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using Orbit.Systems;
using SPT.Reflection.Patching;
using UnityEngine;

namespace Orbit.Patches;

// Observe the original results. Never call the chooser again or change its decision.
public class NativePatrolArrivalDiagnosticPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        // SPT 4.0: the parameterless arrival check is still PatrolMoveSimple.method_0 (4.1: IsCome()),
        // and the owner field is BotOwner_0 (4.1: _owner), injected by Harmony through the ___ prefix.
        => AccessTools.Method(typeof(PatrolMoveSimple), nameof(PatrolMoveSimple.method_0), Type.EmptyTypes);

    [PatchPostfix]
    public static void Postfix(BotOwner ___BotOwner_0, bool __result)
        => NativePatrolDiagnostics.ArrivalChecked(___BotOwner_0, __result);
}

public class NativeGlukharChoiceDiagnosticPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(PatrolPointChooserBossGluhar), nameof(PatrolPointChooserBossGluhar.FindNextPoint));

    // SPT 4.0: the point choosers keep their bot in the Owner field (4.1: _owner).
    [PatchPostfix]
    public static void Postfix(BotOwner ___Owner, PatrolPointContainer __result)
        => NativePatrolDiagnostics.PointChosen(___Owner, __result);
}

public class NativeFollowerArrivalDiagnosticPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => AccessTools.Method(typeof(PatrolMoveSimple), nameof(PatrolMoveSimple.IsCome), new[]
            { typeof(BotOwner), typeof(Vector3?), typeof(bool), typeof(float).MakeByRefType() });

    [PatchPostfix]
    public static void Postfix(BotOwner __0, Vector3? __1, bool __2, float __3, bool __result)
        => NativePatrolDiagnostics.FollowerArrivalChecked(__0, __1, __2, __3, __result);
}
