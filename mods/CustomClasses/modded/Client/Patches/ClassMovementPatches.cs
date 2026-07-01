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

            // Overladen (fix review 2026-06-24): OnWeightUpdated já DERIVOU os campos de inércia reais a partir de
            // Inertia — multiplicar só o Inertia cru (pós-derivação) quase não muda o "clunky". Multiplicamos também
            // os derivados que de fato movem (lateral/diagonal).
            var m = PerksConfig.OverladenInertia?.Value ?? 1f;
            __instance.Inertia *= m;
            __instance.MoveSideInertia *= m;
            __instance.MoveDiagonalInertia *= m;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] overladen falhou: {ex.Message}");
        }
    }
}

/// <summary>
///     Item 050.1 (fix 2026-06-24) — DRIVER REAL da velocidade de movimento.
///     Os getters <c>MaxSpeed</c>/<c>SprintSpeed</c> (patches acima) são só TETO de clamp / denominador de razão —
///     NÃO movem o boneco (a locomoção do EFT é root-motion + cap físico). O valor real passa por
///     <c>MovementContext.SetCharacterMovementSpeed(speed)</c>. Aqui aplicamos os MESMOS multiplicadores de classe
///     ao <c>speed</c> real (via <c>ClassMoveSpeed.Apply</c>); os getters acima ficam como TETO — necessário pro
///     buff do Execution não ser clampado. <c>__0</c> = 1º parâmetro (o speed), robusto ao nome.
///     ⚠️ Coop: só o player LOCAL; peers veem via MovementInfoPacket — gap de coop-sync a validar.
/// </summary>
internal class SetCharacterMovementSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), "SetCharacterMovementSpeed");
    }

    [PatchPrefix]
    private static void Prefix(MovementContext __instance, ref float __0)
    {
        ClassMoveSpeed.Apply(__instance, ref __0);
    }
}

/// <summary>
///     Item 050.1 (fix 2026-06-24) — velocidade de CORRIDA (sprint). Sprint é caminho separado do walk:
///     o driver é <c>MovementContext.SprintAcceleration</c> → campo <c>SprintSpeed</c> (root-motion). Postfix
///     reaplica os mesmos multiplicadores de classe ao <c>SprintSpeed</c> pós-clamp (Rooted é no-op — não se
///     mira correndo). ⚠️ validar in-game: o buff (+10%) pode ser re-clampado no frame seguinte.
/// </summary>
internal class SprintAccelerationPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(MovementContext), "SprintAcceleration");
    }

    [PatchPostfix]
    private static void Postfix(MovementContext __instance)
    {
        try
        {
            var s = __instance.SprintSpeed;
            var s0 = s;
            ClassMoveSpeed.Apply(__instance, ref s);   // gateia internamente (só MainPlayer local + classe)
            if (s != s0)
            {
                __instance.SprintSpeed = s;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] sprint falhou: {ex.Message}");
        }
    }
}
