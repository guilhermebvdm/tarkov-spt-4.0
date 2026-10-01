using System.Reflection;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

internal class PersonalBuffFullStringPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(InjectorBuff), nameof(InjectorBuff.GetStringValue));
    }

    [PatchPrefix]
    public static void Prefix(InjectorBuff __instance)
    {
        if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled)
        {
            return;
        }

        // ref: AUD-01-20 — mesma correção de PersonalBuffPatch: mutava um clone descartado antes de
        // GetStringValue() rodar, então o tooltip nunca refletia o ajuste.
        var skillManager = GameUtils.GetSkillManager()?.SkillManagerExtended;
        skillManager?.AdjustStimulatorBuff(__instance);
    }
}