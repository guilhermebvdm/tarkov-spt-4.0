using System.Reflection;
using HarmonyLib;
using SkillsExtended.Skills.Core.Patches;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

public class AbstractSkillClassSummaryLevelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(AbstractSkillClass), nameof(AbstractSkillClass.SummaryLevel));
    }

    [PatchPrefix]
    public static bool Prefix(AbstractSkillClass __instance, ref int __result)
    {
        if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled)
        {
            return true;
        }

        // ref: AUD-01-10 — dono real via SkillManagerConstructorPatch.SkillOwners, não GameUtils.GetSkillManager()
        // (que sempre resolve o MainPlayer, independentemente de quem é o __instance de fato).
        if (!SkillManagerConstructorPatch.SkillOwners.TryGetValue(__instance, out var skillManager) || skillManager == null)
        {
            return true;
        }

        var newSkillCap = 60 * (1 + skillManager.SkillManagerExtended.FieldMedicineSkillCap);

        var level = __instance.Level;
        var buff = __instance.Buff;
        __result = Mathf.CeilToInt(Mathf.Min(buff > 0 ? newSkillCap : 51, level + buff));

        return false;
    }
}
