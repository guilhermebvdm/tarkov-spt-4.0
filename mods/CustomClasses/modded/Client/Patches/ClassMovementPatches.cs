using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Item 050.1 — modificadores de velocidade/inércia de movimento por classe.
///     Postfix nos getters de <c>MovementContext.MaxSpeed</c>/<c>SprintSpeed</c> e em
///     <c>BasePhysicalClass.OnWeightUpdated</c> → multiplicam (compõem com o stances mod, que também faz postfix-mult).
///     Gating: só o player LOCAL (MainPlayer) + classe. Lê o F12 no apply-time.
///     Cobre agora: 🔻 Heavy Frame (Tanque −10% vel) · 🔻 Overladen (Saqueador inércia↑ por peso).
///     (Execution +vel c/ melee na mão e Rooted −vel em ADS entram aqui quando a detecção de melee/mira for confirmada.)
/// </summary>
internal static class ClassMoveSpeed
{
    internal static void Apply(MovementContext ctx, ref float result)
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(ctx, p.MovementContext))
            {
                return;   // só o player local (não bots/remotos)
            }

            // 🔻 Heavy Frame (Tanque): −10% de velocidade (sempre).
            if (PerksConfig.HeavyFrameEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank"))
            {
                result *= PerksConfig.HeavyFrameMoveSpeed?.Value ?? 1f;
            }

            // 🔻 Rooted (Caçador): −15% de velocidade enquanto MIRA (ADS).
            if (PerksConfig.RootedEnabled?.Value == true && SkillMultipliers.IsLocalClass("Hunter")
                && p.HandsController is Player.FirearmController fc && fc.IsAiming)
            {
                result *= PerksConfig.RootedAdsSpeed?.Value ?? 1f;
            }

            // 🔧 Execution (Furtivo): +velocidade com a MELEE na mão.
            if (PerksConfig.ExecutionSpeedEnabled?.Value == true && SkillMultipliers.IsLocalClass("Stealth")
                && p.HandsController is Player.KnifeController)
            {
                result *= PerksConfig.ExecutionMoveSpeed?.Value ?? 1f;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] move speed falhou: {ex.Message}");
        }
    }
}

internal class MaxSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.MaxSpeed));
    }

    [PatchPostfix]
    private static void Postfix(MovementContext __instance, ref float __result)
    {
        ClassMoveSpeed.Apply(__instance, ref __result);
    }
}

internal class SprintSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(MovementContext), nameof(MovementContext.SprintSpeed));
    }

    [PatchPostfix]
    private static void Postfix(MovementContext __instance, ref float __result)
    {
        ClassMoveSpeed.Apply(__instance, ref __result);
    }
}

/// <summary>🔻 Overladen (Saqueador): inércia escala mais com o peso (movimento clunky carregado).</summary>
internal class OverladenInertiaPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BasePhysicalClass), nameof(BasePhysicalClass.OnWeightUpdated));
    }

    [PatchPostfix]
    private static void Postfix(BasePhysicalClass __instance)
    {
        try
        {
            if (PerksConfig.OverladenEnabled?.Value != true)
            {
                return;
            }

            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null || !ReferenceEquals(__instance, p.Physical))
            {
                return;   // só o player local
            }

            if (!SkillMultipliers.IsLocalClass("Scavenger"))
            {
                return;
            }

            // Inertia já escala com o peso no vanilla (OnWeightUpdated) — aqui multiplicamos por cima.
            __instance.Inertia *= PerksConfig.OverladenInertia?.Value ?? 1f;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] overladen falhou: {ex.Message}");
        }
    }
}
