using System.Reflection;
using EFT;
using EFT.HealthSystem;
using SkillsExtended.Helpers;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SkillsExtended.Skills.FieldMedicine.Patches;

internal class StimulatorApplyBuffPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        var healthController = typeof(ActiveHealthController);
        var nestedTypes = healthController.GetNestedType("Stimulator", BindingFlags.NonPublic);
        return nestedTypes.GetMethod("smethod_0", BindingFlags.Static | BindingFlags.Public);
    }

    // Resolvido na review 01 do item 002 (PA-01-02): "SkillManager_0" não é declarado diretamente em
    // ActiveHealthController — provavelmente herdado de sua classe base. Sobe BaseType até achar em
    // vez de arriscar um GetField que só busca na classe exata (retornaria null silenciosamente).
    private static readonly FieldInfo SkillManagerAccessor = ResolveSkillManagerField();

    private static FieldInfo ResolveSkillManagerField()
    {
        for (var t = typeof(ActiveHealthController); t != null; t = t.BaseType)
        {
            var field = t.GetField("SkillManager_0", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) return field;
        }
        return null;
    }

    [PatchPrefix]
    public static bool Prefix(InjectorBuff buffSettings, float refValue, Vector2? limits, ref float __result)
    {
        if (!SkillsExtendedPlugin.SkillData.FieldMedicine.Enabled || limits is null)
        {
            return true;
        }

        var value = buffSettings.AbsoluteValue ? buffSettings.Value : (buffSettings.Value + 1f) * refValue;

        // ref: AUD-01-03 — resolve o SkillManager do DONO REAL do efeito (capturado por
        // StimulatorHealthControllerTrackerPatch a partir de method_6), em vez de sempre o MainPlayer.
        var ownerHealthController = StimulatorHealthControllerTrackerPatch.CurrentEffectOwner;
        var skillManager = ownerHealthController != null && SkillManagerAccessor != null
            ? SkillManagerAccessor.GetValue(ownerHealthController) as SkillManager
            : GameUtils.GetSkillManager(); // fallback — não deveria ser atingido no fluxo normal

        var newSkillCap = 60 * (1 + skillManager?.SkillManagerExtended.FieldMedicineSkillCap);

        __result = Mathf.CeilToInt(Mathf.Clamp(value, limits.Value.x, newSkillCap));

#if DEBUG
        Logger.LogDebug("==================================================================");
        Logger.LogDebug($"Skill Name:                   `{buffSettings.SkillName}`");
        Logger.LogDebug($"IsAbsolute:                   `{buffSettings.AbsoluteValue}`");
        Logger.LogDebug($"Buff Value:                   `{value}`");
        Logger.LogDebug($"Adjusted max skill cap:       `{__result}`");
        Logger.LogDebug("==================================================================");
#endif
        
        return false;
    }
}