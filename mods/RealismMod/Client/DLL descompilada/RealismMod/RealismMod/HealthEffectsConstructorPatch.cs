// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthEffectsConstructorPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class HealthEffectsConstructorPatch : ModulePatch
{
  private static List<string> modifiedMeds = new List<string>();

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (HealthEffectsComponent).GetConstructor(new Type[2]
    {
      typeof (Item),
      typeof (IHealthEffect)
    });
  }

  private static string GetHBTypeString(EHeavyBleedHealType type)
  {
    switch (type)
    {
      case EHeavyBleedHealType.Clot:
        return "CLOTTING AGENT";
      case EHeavyBleedHealType.Tourniquet:
        return "TOURNIQUET";
      case EHeavyBleedHealType.Combo:
        return "TOURNIQUET + CHEST SEAL";
      case EHeavyBleedHealType.Surgical:
        return "SURGICAL";
      default:
        return "NONE";
    }
  }

  private static string GetPKStrengthString(float str)
  {
    float num = str;
    if ((double) num == 0.0)
      return "NONE";
    if ((double) num <= 5.0)
      return "WEAK";
    if ((double) num <= 10.0)
      return "MILD";
    if ((double) num <= 15.0)
      return "STRONG";
    return (double) num > 15.0 ? "VERY STRONG" : "NONE";
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(HealthEffectsComponent __instance, Item item)
  {
    Consumable medStats = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    bool flag = medStats.ConsumableType == EConsumableType.PainPills || medStats.ConsumableType == EConsumableType.PainDrug;
    MongoID? parentId = item.Template.ParentId;
    MongoID? nullable = MongoID.op_Implicit("5448f3a64bdc2d60728b456a");
    if (parentId.HasValue == nullable.HasValue && (!parentId.HasValue || MongoID.op_Equality(parentId.GetValueOrDefault(), nullable.GetValueOrDefault())))
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.StimType)
      {
        Name = Attributes.ENewItemAttributeId.StimType.GetName(),
        StringValue = (Func<string>) (() => Plugin.RealHealthController.GetStimType(MongoID.op_Implicit(item.TemplateId)).ToString()),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = false
      });
    if (flag || medStats.ConsumableType == EConsumableType.Alcohol)
    {
      float strength = medStats.Strength;
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.PainKillerStrength)
      {
        Name = Attributes.ENewItemAttributeId.PainKillerStrength.GetName(),
        StringValue = (Func<string>) (() => HealthEffectsConstructorPatch.GetPKStrengthString(strength)),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = false
      });
    }
    if (medStats.ConsumableType != EConsumableType.Tourniquet && medStats.ConsumableType != EConsumableType.Medkit && medStats.ConsumableType != EConsumableType.Surgical)
      return;
    float hpPerTick = medStats.ConsumableType != EConsumableType.Surgical ? -medStats.TrnqtDamage : medStats.HPRestoreTick;
    if (medStats.ConsumableType == EConsumableType.Medkit || medStats.ConsumableType == EConsumableType.Surgical)
    {
      float hp = medStats.HPRestoreAmount;
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.OutOfRaidHP)
      {
        Name = Attributes.ENewItemAttributeId.OutOfRaidHP.GetName(),
        Base = (Func<float>) (() => hp),
        StringValue = (Func<string>) (() => hp.ToString()),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = false
      });
    }
    if (medStats.HeavyBleedHealType != 0)
    {
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.HBleedType)
      {
        Name = Attributes.ENewItemAttributeId.HBleedType.GetName(),
        StringValue = (Func<string>) (() => HealthEffectsConstructorPatch.GetHBTypeString(medStats.HeavyBleedHealType)),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
        LabelVariations = (EItemAttributeLabelVariations) 1,
        LessIsGood = false
      });
      if (medStats.ConsumableType == EConsumableType.Surgical)
      {
        item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.HpPerTick)
        {
          Name = Attributes.ENewItemAttributeId.HpPerTick.GetName(),
          Base = (Func<float>) (() => hpPerTick),
          StringValue = (Func<string>) (() => hpPerTick.ToString()),
          DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
          LabelVariations = (EItemAttributeLabelVariations) 1,
          LessIsGood = false
        });
        item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.RemoveTrnqt)
        {
          Name = Attributes.ENewItemAttributeId.RemoveTrnqt.GetName(),
          DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
        });
      }
      else if ((double) hpPerTick != 0.0)
        item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.LimbHpPerTick)
        {
          Name = Attributes.ENewItemAttributeId.LimbHpPerTick.GetName(),
          Base = (Func<float>) (() => hpPerTick),
          StringValue = (Func<string>) (() => hpPerTick.ToString()),
          DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1),
          LabelVariations = (EItemAttributeLabelVariations) 1,
          LessIsGood = false
        });
    }
  }
}
