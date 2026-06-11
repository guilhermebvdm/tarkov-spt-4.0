// Decompiled with JetBrains decompiler
// Type: RealismMod.GetCachedReadonlyQualitiesPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class GetCachedReadonlyQualitiesPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (AmmoTemplate).GetMethod("GetCachedReadonlyQualities", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void PatchPostFix(AmmoTemplate __instance, ref List<ItemAttributeClass> __result)
  {
    if (__result.Any<ItemAttributeClass>((Func<ItemAttributeClass, bool>) (i => (Attributes.ENewItemAttributeId) i.Id == Attributes.ENewItemAttributeId.BallisticCoefficient)) || __result.Any<ItemAttributeClass>((Func<ItemAttributeClass, bool>) (i => (Attributes.ENewItemAttributeId) i.Id == Attributes.ENewItemAttributeId.ProjectileCount)))
      return;
    GetCachedReadonlyQualitiesPatch.AddCustomAttributes(__instance, ref __result);
  }

  private static string GetHeat(AmmoTemplate __instance)
  {
    float heatFactor = __instance.HeatFactor;
    if ((double) heatFactor <= 1.0)
      return "";
    if ((double) heatFactor <= 1.0499999523162842)
      return "Low";
    if ((double) heatFactor <= 1.1000000238418579)
      return "Significant";
    if ((double) heatFactor <= 1.1499999761581421)
      return "High";
    if ((double) heatFactor <= 1.2000000476837158)
      return "Very High";
    return (double) heatFactor > 1.2000000476837158 ? "Extreme" : string.Empty;
  }

  private static string GetMalfChance(AmmoTemplate __instance)
  {
    float malfMisfireChance = __instance.MalfMisfireChance;
    if ((double) malfMisfireChance <= 0.0)
      return "";
    if ((double) malfMisfireChance <= 0.5)
      return "Low Chance";
    if ((double) malfMisfireChance <= 1.0)
      return "Significant Chance";
    if ((double) malfMisfireChance <= 1.5)
      return "High Chance";
    if ((double) malfMisfireChance <= 2.0)
      return "Very High Chance";
    return (double) malfMisfireChance > 2.0 ? "Extremely High Chance" : string.Empty;
  }

  private static string GetDuraBurnString(AmmoTemplate __instance)
  {
    float num = __instance.DurabilityBurnModificator - 1f;
    if ((double) num <= 1.25)
      return "Very Low";
    if ((double) num <= 1.5)
      return "Low";
    if ((double) num <= 2.0)
      return "Significant";
    if ((double) num <= 4.0)
      return "High";
    if ((double) num <= 8.0)
      return "Very High";
    return (double) num > 8.0 ? "Extreme" : string.Empty;
  }

  public static void AddCustomAttributes(
    AmmoTemplate ammoTemplate,
    ref List<ItemAttributeClass> ammoAttributes)
  {
    if (!PluginConfig.EnableAmmoStats.Value)
      return;
    float fireRate = (float) Math.Round(((double) ammoTemplate.casingMass - 1.0) * 100.0, 2);
    if ((double) fireRate != 0.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.Firerate)
      {
        Name = Attributes.ENewItemAttributeId.Firerate.GetName(),
        Base = (Func<float>) (() => fireRate),
        StringValue = (Func<string>) (() => $"{fireRate} %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = true
      });
    float fragChance = ammoTemplate.FragmentationChance * 100f;
    if ((double) fragChance > 0.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.FragmentationChance)
      {
        Name = Attributes.ENewItemAttributeId.FragmentationChance.GetName(),
        Base = (Func<float>) (() => fragChance),
        StringValue = (Func<string>) (() => $"{fragChance} %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    if ((double) ammoTemplate.BallisticCoeficient > 0.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.BallisticCoefficient)
      {
        Name = Attributes.ENewItemAttributeId.BallisticCoefficient.GetName(),
        Base = (Func<float>) (() => ammoTemplate.BallisticCoeficient),
        StringValue = (Func<string>) (() => $"{ammoTemplate.BallisticCoeficient}"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    if ((double) ammoTemplate.MalfMisfireChance > 0.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.MalfunctionChance)
      {
        Name = Attributes.ENewItemAttributeId.MalfunctionChance.GetName(),
        Base = (Func<float>) (() => ammoTemplate.MalfMisfireChance),
        StringValue = (Func<string>) (() => GetCachedReadonlyQualitiesPatch.GetMalfChance(ammoTemplate)),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    if ((double) ammoTemplate.DurabilityBurnModificator > 1.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.DurabilityBurn)
      {
        Name = Attributes.ENewItemAttributeId.DurabilityBurn.GetName(),
        Base = (Func<float>) (() => ammoTemplate.DurabilityBurnModificator),
        StringValue = (Func<string>) (() => GetCachedReadonlyQualitiesPatch.GetDuraBurnString(ammoTemplate)),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    if ((double) ammoTemplate.HeatFactor > 1.0)
      ammoAttributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.Heat)
      {
        Name = Attributes.ENewItemAttributeId.Heat.GetName(),
        Base = (Func<float>) (() => ammoTemplate.HeatFactor),
        StringValue = (Func<string>) (() => GetCachedReadonlyQualitiesPatch.GetHeat(ammoTemplate)),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
  }
}
