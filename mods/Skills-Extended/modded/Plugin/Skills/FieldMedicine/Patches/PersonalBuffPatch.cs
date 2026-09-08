using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;



namespace SkillsExtended.Skills.FieldMedicine.Patches;

internal class PersonalBuffPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BuffSettings), nameof(BuffSettings.GetPersonalBuffSettings));
    }

    [PatchPostfix]
    public static void PostFix(SkillManager skills, InjectorBuff __result)
    {
        if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled || !__result.IsBuff)
        {
            return;
        }

        // ref: AUD-01-20 — antes clonava e descartava o clone; o ajuste nunca chegava a valer pro
        // __result de fato retornado (bug funcional, não só alocação evitável — ver
        // 004-...-02-spec-tech.md §0/§1.5).
        skills.SkillManagerExtended.AdjustStimulatorBuff(__result);
    }
}