// Decompiled with JetBrains decompiler
// Type: RealismMod.WeaponConstructorPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class WeaponConstructorPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetConstructor(new Type[2]
    {
      typeof (string),
      typeof (WeaponTemplate)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Weapon __instance, string id, WeaponTemplate template)
  {
    if (PluginConfig.ShowBalance.Value)
    {
      List<ItemAttributeClass> attributes = ((Item) __instance).Attributes;
      GClass3174 gclass3174 = new GClass3174((EItemAttributeId) 11);
      ((ItemAttributeClass) gclass3174).Name = Attributes.ENewItemAttributeId.Balance.GetName();
      gclass3174.Range = new Vector2(100f, 200f);
      ((ItemAttributeClass) gclass3174).LessIsGood = false;
      ((ItemAttributeClass) gclass3174).Base = (Func<float>) (() => 150f);
      gclass3174.Delta = (Func<float>) (() => WeaponConstructorPatch.BalanceDelta());
      ((ItemAttributeClass) gclass3174).StringValue = (Func<string>) (() => Math.Round((double) UIWeaponStats.Balance, 1).ToString());
      ((ItemAttributeClass) gclass3174).DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 2);
      attributes.Add((ItemAttributeClass) gclass3174);
    }
    if (PluginConfig.ShowDispersion.Value)
    {
      List<ItemAttributeClass> attributes = ((Item) __instance).Attributes;
      GClass3174 gclass3174 = new GClass3174((EItemAttributeId) 13);
      ((ItemAttributeClass) gclass3174).Name = Attributes.ENewItemAttributeId.Dispersion.GetName();
      gclass3174.Range = new Vector2(0.0f, 50f);
      ((ItemAttributeClass) gclass3174).LessIsGood = true;
      ((ItemAttributeClass) gclass3174).Base = (Func<float>) (() => (float) __instance.Template.RecolDispersion);
      gclass3174.Delta = (Func<float>) (() => WeaponConstructorPatch.DispersionDelta(__instance));
      ((ItemAttributeClass) gclass3174).StringValue = (Func<string>) (() => Math.Round((double) UIWeaponStats.Dispersion, 1).ToString());
      ((ItemAttributeClass) gclass3174).DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 2);
      attributes.Add((ItemAttributeClass) gclass3174);
    }
    if (PluginConfig.ShowCamRecoil.Value)
    {
      List<ItemAttributeClass> attributes = ((Item) __instance).Attributes;
      GClass3174 gclass3174 = new GClass3174((EItemAttributeId) 12);
      ((ItemAttributeClass) gclass3174).Name = Attributes.ENewItemAttributeId.CameraRecoil.GetName();
      gclass3174.Range = new Vector2(0.0f, 50f);
      ((ItemAttributeClass) gclass3174).LessIsGood = true;
      ((ItemAttributeClass) gclass3174).Base = (Func<float>) (() => __instance.Template.RecoilCamera * 100f);
      gclass3174.Delta = (Func<float>) (() => WeaponConstructorPatch.CamRecoilDelta(__instance));
      ((ItemAttributeClass) gclass3174).StringValue = (Func<string>) (() => Math.Round((double) UIWeaponStats.CamRecoil * 100.0, 2).ToString());
      ((ItemAttributeClass) gclass3174).DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 2);
      attributes.Add((ItemAttributeClass) gclass3174);
    }
    if (PluginConfig.ShowRecoilAngle.Value)
      ((Item) __instance).Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.RecoilAngle)
      {
        Name = Attributes.ENewItemAttributeId.RecoilAngle.GetName(),
        Base = (Func<float>) (() => UIWeaponStats.RecoilAngle),
        StringValue = (Func<string>) (() => Math.Round((double) UIWeaponStats.RecoilAngle).ToString()),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    if (!PluginConfig.ShowSemiROF.Value)
      return;
    ((Item) __instance).Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.SemiROF)
    {
      Name = Attributes.ENewItemAttributeId.SemiROF.GetName(),
      Base = (Func<float>) (() => (float) UIWeaponStats.SemiFireRate),
      StringValue = (Func<string>) (() => $"{UIWeaponStats.SemiFireRate.ToString()} {GClass2112.Localized("RPM", (string) null)}"),
      DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
    });
  }

  private static float BalanceDelta()
  {
    return (float) ((150.0 - (150.0 - (double) UIWeaponStats.Balance * -1.0)) / -150.0);
  }

  private static float DispersionDelta(Weapon __instance)
  {
    return (float) (((double) __instance.Template.RecolDispersion - (double) UIWeaponStats.Dispersion) / ((double) __instance.Template.RecolDispersion * -1.0));
  }

  private static float CamRecoilDelta(Weapon __instance)
  {
    float num = __instance.Template.RecoilCamera * 100f;
    return (float) (((double) num - (double) UIWeaponStats.CamRecoil * 100.0) / ((double) num * -1.0));
  }
}
