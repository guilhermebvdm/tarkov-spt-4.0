using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.WeaponSkills.Patches;

// ref: AUD-01-08 — aplica o bônus de ergonomia por-instância em vez de mutar Weapon.Template.Ergonomics
// (compartilhado por todos os itens do mesmo TemplateId). Weapon.ErgonomicsTotal (EFT.InventoryLogic/Weapon.cs:773)
// é a property por-instância confirmadamente lida pela ergonomia real de gameplay (EFT/Player.cs:12845).
internal class ErgonomicsTotalPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(Weapon), nameof(Weapon.ErgonomicsTotal));
    }

    [PatchPostfix]
    private static void Postfix(Weapon __instance, ref float __result)
    {
        if (SkillsExtendedPlugin.SkillData.NatoWeapons.Enabled &&
            UpdateWeaponsPatch.UsecWeaponInstanceIds.ContainsKey(__instance.Id))
        {
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager != null)
            {
                __result *= 1f + skillManager.SkillManagerExtended.UsecArSystemsErgoBuff;
            }
            return;
        }

        if (SkillsExtendedPlugin.SkillData.EasternWeapons.Enabled &&
            UpdateWeaponsPatch.EasternWeaponInstanceIds.ContainsKey(__instance.Id))
        {
            var skillManager = GameUtils.GetSkillManager();
            if (skillManager != null)
            {
                __result *= 1f + skillManager.SkillManagerExtended.BearAkSystemsErgoBuff;
            }
        }
    }
}
