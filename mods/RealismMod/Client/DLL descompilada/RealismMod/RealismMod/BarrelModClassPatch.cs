// Decompiled with JetBrains decompiler
// Type: RealismMod.BarrelModClassPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class BarrelModClassPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BarrelItemClass).GetConstructor(new Type[2]
    {
      typeof (string),
      typeof (BarrelTemplateClass)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(BarrelItemClass __instance, BarrelTemplateClass template)
  {
    float shotDisp = (float) (((double) template.ShotgunDispersion - 1.0) * 100.0);
    Utils.SafelyAddAttributeToList(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.ShotDispersion)
    {
      Name = Attributes.ENewItemAttributeId.ShotDispersion.GetName(),
      Base = (Func<float>) (() => shotDisp),
      StringValue = (Func<string>) (() => $"{shotDisp}%"),
      LessIsGood = true,
      DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
      LabelVariations = (EItemAttributeLabelVariations) 1
    }, (Item) __instance);
  }
}
