using System;
using System.Reflection;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CustomClasses.Client;

/// <summary>
///     Prefix em <c>AbstractSkillClass.OnTrigger(SkillActionClass, float val)</c> (AbstractSkillClass.cs:100):
///     escala o XP (`val`) pelo fator da skill da classe (clamp ≥ 0 — debuff nunca remove XP).
///     Cobre raid e fora-de-raid (toda ação de skill passa por OnTrigger). Hot path → O(1), sem alocação.
/// </summary>
internal class OnTriggerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(AbstractSkillClass), nameof(AbstractSkillClass.OnTrigger));
    }

    [PatchPrefix]
    private static void Prefix(AbstractSkillClass __instance, ref float val)
    {
        if (!Plugin.Enabled || val <= 0f)
        {
            return;
        }

        try
        {
            SkillMultipliers.EnsureLoaded();   // lazy: 1ª vez carrega da rota
            if (SkillMultipliers.TryGet(__instance.Id, out var factor))
            {
                val *= factor < 0f ? 0f : factor;   // clamp ≥ 0
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogError($"[CustomClasses] OnTrigger scaling falhou: {ex.Message}");
        }
    }
}
