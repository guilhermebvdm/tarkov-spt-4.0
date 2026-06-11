// Decompiled with JetBrains decompiler
// Type: RealismMod.HeadsetConstructorPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class HeadsetConstructorPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (HeadphonesItemClass).GetConstructor(new Type[2]
    {
      typeof (string),
      typeof (HeadphonesTemplateClass)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(HeadphonesItemClass __instance)
  {
    Item obj = (Item) __instance;
    float dB = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(obj.TemplateId)).dB;
    if ((double) dB <= 0.0)
      return;
    obj.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.NoiseReduction)
    {
      Name = Attributes.ENewItemAttributeId.NoiseReduction.GetName(),
      Base = (Func<float>) (() => dB),
      StringValue = (Func<string>) (() => dB.ToString() + " NRR"),
      DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
    });
  }
}
