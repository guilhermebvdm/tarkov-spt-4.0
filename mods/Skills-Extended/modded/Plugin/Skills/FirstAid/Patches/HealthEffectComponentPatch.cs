using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.FirstAid.Patches;

// ref: AUD-01-07 — EFT.InventoryLogic/IHealthEffect.cs:6-15 (GInterface392.StimulatorBuffs + membros de IHealthEffect).
// Wrapper per-instance: repassa tudo do template original, exceto DamageEffects (dicionário clonado com o
// custo ajustado). Evita mutar o GClass1443 compartilhado por TemplateId (EFT.InventoryLogic/HealthEffectsComponent.cs:34).
internal sealed class PerInstanceHealthEffect : IHealthEffect
{
    private readonly IHealthEffect _original;
    private readonly Dictionary<EDamageEffectType, GClass1443> _damageEffects;

    public PerInstanceHealthEffect(IHealthEffect original, Dictionary<EDamageEffectType, GClass1443> damageEffects)
    {
        _original = original;
        _damageEffects = damageEffects;
    }

    public float UseTime => _original.UseTime;
    public KeyValuePair<EBodyPart, float>[] BodyPartTimeMults => _original.BodyPartTimeMults;
    public Dictionary<EHealthFactorType, GClass1444> HealthEffects => _original.HealthEffects;
    public Dictionary<EDamageEffectType, GClass1443> DamageEffects => _damageEffects;
    public string StimulatorBuffs => _original.StimulatorBuffs;
}

public class HealthEffectComponentPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Constructor(typeof(HealthEffectsComponent), [typeof(Item), typeof(IHealthEffect)]);
    }

    [PatchPostfix]
    public static void PostFix(HealthEffectsComponent __instance, Item item, IHealthEffect template)
    {
        var skillData = SkillsExtendedPlugin.SkillData.FirstAid;
        if (!skillData.Enabled)
        {
            return;
        }

        var skillManager = GameUtils.GetSkillManager();
        if (skillManager == null)
        {
            return;
        }

        if (template.DamageEffects is null || item is not MedicalItemClass meds)
        {
            return;
        }

        // Why? -- I don't know, but leave it for now because something probably broke
        if (meds.TemplateId.LocalizedName().Contains("Name"))
        {
            return;
        }

        // ref: AUD-01-07 — clona em um dicionário novo em vez de mutar template.DamageEffects[x].Cost
        // (compartilhado por todos os itens do mesmo TemplateId, EFT.InventoryLogic/HealthEffectsComponent.cs:34).
        var adjusted = new Dictionary<EDamageEffectType, GClass1443>(template.DamageEffects);
        var changedTypes = new List<EDamageEffectType>(3);

        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.Fracture, skillManager, changedTypes);
        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.LightBleeding, skillManager, changedTypes);
        TryCloneWithAdjustedCost(adjusted, EDamageEffectType.HeavyBleeding, skillManager, changedTypes);

        if (changedTypes.Count == 0)
        {
            return;
        }

        __instance.IHealthEffect = new PerInstanceHealthEffect(template, adjusted);

        // ref: AUD-01-07 — o tooltip (Item.Attributes) foi montado no construtor base ANTES deste Postfix
        // rodar, com lambdas presas ao GClass1443 original e compartilhado (EFT.InventoryLogic/Item.cs:930-948).
        // Recriamos a attribute de cada tipo ajustado apontando pro clone, via o mesmo mecanismo que o EFT usa
        // (Item.AddOrReplaceAttribute + Item.Replacements, EFT.InventoryLogic/Item.cs:360-390,897-908) —
        // sem isso o tooltip continuaria mostrando o custo pristino, não o já reduzido pela skill.
        foreach (var type in changedTypes)
        {
            if (!Item.Replacements.TryGetValue(type, out var replacement))
            {
                continue;
            }

            var spec = adjusted[type];
            item.AddOrReplaceAttribute(new ItemAttributeClass(type)
            {
                Name = replacement,
                DisplayType = () => EItemAttributeDisplayType.Compact,
                StringValue = () => spec.GetStringValue(),
                FullStringValue = () => spec.GetFullStringValue(replacement)
            });
        }

#if DEBUG
        Logger.LogDebug($"[FirstAid] Per-instance cost applied to {meds.TemplateId.LocalizedName()}\n");
#endif
    }

    private static void TryCloneWithAdjustedCost(
        Dictionary<EDamageEffectType, GClass1443> adjusted,
        EDamageEffectType type,
        SkillManager skillManager,
        List<EDamageEffectType> changedTypes)
    {
        if (!adjusted.TryGetValue(type, out var original) || original is null || original.Cost <= 0)
        {
            return;
        }

        var newCost = original.Cost;
        skillManager.SkillManagerExtended.FirstAidResourceCostBuff.Apply(ref newCost);

        adjusted[type] = new GClass1443
        {
            Delay = original.Delay,
            Duration = original.Duration,
            FadeOut = original.FadeOut,
            Cost = newCost,
            HealthPenaltyMin = original.HealthPenaltyMin,
            HealthPenaltyMax = original.HealthPenaltyMax
        };

        changedTypes.Add(type);
    }
}
