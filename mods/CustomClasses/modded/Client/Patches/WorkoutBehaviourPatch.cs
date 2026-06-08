using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;                  // ESkillId
using EFT.Hideout;          // WorkoutBehaviour
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     (a) Gym: o XP de treino NÃO passa por <c>OnTrigger</c> — vem por <c>WorkoutBehaviour.method_18</c>
///     → <c>SetCurrent</c> direto. Aqui, em vez de reimplementar a lógica do treino (invasivo/GPL),
///     fazemos snapshot do <c>Current</c> das skills com fator ANTES, deixamos o original rodar, e no
///     Postfix escalamos só o DELTA (XP ganho) pelo fator: total = delta×fator (clamp ≥ 0).
///     Membros verificados via mods/SkillDistribution (WorkoutBehaviourPatch) + validados na compilação.
/// </summary>
internal class WorkoutBehaviourPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorkoutBehaviour), nameof(WorkoutBehaviour.method_18));
    }

    [PatchPrefix]
    private static void Prefix(WorkoutBehaviour __instance, out Dictionary<ESkillId, float>? __state)
    {
        __state = null;
        if (!Plugin.Enabled)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();
            var mgr = __instance.HideoutPlayerOwner_0?.HideoutPlayer?.Skills;
            if (mgr is null)
            {
                return;
            }

            var snap = new Dictionary<ESkillId, float>();
            foreach (var e in SkillMultipliers.Entries)
            {
                if (Math.Abs(e.Value - 1f) < 1e-4f)
                {
                    continue;   // fator 1 = vanilla, ignora
                }

                var sk = mgr.GetSkill(e.Key);
                if (sk is not null)
                {
                    snap[e.Key] = sk.Current;
                }
            }

            if (snap.Count > 0)
            {
                __state = snap;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] gym prefix falhou: {ex.Message}");
        }
    }

    [PatchPostfix]
    private static void Postfix(WorkoutBehaviour __instance, Dictionary<ESkillId, float>? __state)
    {
        if (__state is null)
        {
            return;
        }

        try
        {
            var mgr = __instance.HideoutPlayerOwner_0?.HideoutPlayer?.Skills;
            if (mgr is null)
            {
                return;
            }

            foreach (var kv in __state)
            {
                if (!SkillMultipliers.TryGet(kv.Key, out var factor))
                {
                    continue;
                }

                var sk = mgr.GetSkill(kv.Key);
                if (sk is null)
                {
                    continue;
                }

                var delta = sk.Current - kv.Value;   // XP base aplicado pelo treino
                if (delta <= 1e-4f)
                {
                    continue;
                }

                var f = factor < 0f ? 0f : factor;
                sk.SetCurrent(sk.Current + delta * (f - 1f), true);   // extra → total = delta×f (nunca < base, clamp ≥ 0)
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] gym postfix falhou: {ex.Message}");
        }
    }
}
