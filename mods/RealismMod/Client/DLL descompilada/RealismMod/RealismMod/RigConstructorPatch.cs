// Decompiled with JetBrains decompiler
// Type: RealismMod.RigConstructorPatch
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

public class RigConstructorPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (VestItemClass).GetConstructor(new Type[2]
    {
      typeof (string),
      typeof (VestTemplateClass)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(VestItemClass __instance, VestTemplateClass template)
  {
    Item obj = (Item) __instance;
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(obj.TemplateId));
    if (Plugin.ServerConfig.reload_changes)
    {
      float reloadSpeedMulti = dataObj.ReloadSpeedMulti;
      if ((double) reloadSpeedMulti > 0.0 && (double) reloadSpeedMulti != 1.0)
      {
        float reloadSpeedPercent = (float) Math.Round(((double) reloadSpeedMulti - 1.0) * 100.0);
        obj.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.GearReloadSpeed)
        {
          Name = Attributes.ENewItemAttributeId.GearReloadSpeed.GetName(),
          Base = (Func<float>) (() => reloadSpeedPercent),
          StringValue = (Func<string>) (() => reloadSpeedPercent.ToString() + " %"),
          DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
          LabelVariations = (EItemAttributeLabelVariations) 1,
          LessIsGood = false
        });
      }
    }
    if (!Plugin.ServerConfig.gear_weight || template.ArmorType != 0)
      return;
    float comfort = dataObj.Comfort;
    if ((double) comfort > 0.0 && (double) comfort != 1.0)
    {
      float comfortPercent = -1f * (float) Math.Round(((double) comfort - 1.0) * 100.0);
      obj.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.Comfort)
      {
        Name = Attributes.ENewItemAttributeId.Comfort.GetName(),
        Base = (Func<float>) (() => comfortPercent),
        StringValue = (Func<string>) (() => comfortPercent.ToString() + " %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = false
      });
    }
  }
}
