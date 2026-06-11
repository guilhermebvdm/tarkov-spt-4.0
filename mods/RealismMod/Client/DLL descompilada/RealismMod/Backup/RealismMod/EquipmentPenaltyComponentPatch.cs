// Decompiled with JetBrains decompiler
// Type: RealismMod.EquipmentPenaltyComponentPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class EquipmentPenaltyComponentPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (EquipmentPenaltyComponent).GetConstructor(new Type[3]
    {
      typeof (Item),
      typeof (GInterface359),
      typeof (bool)
    });
  }

  private static float GetAverage(Func<CompositeArmorComponent, float> predicate, Item item)
  {
    List<CompositeArmorComponent> list = GClass3176.GetItemComponentsInChildren<CompositeArmorComponent>(item, true).ToList<CompositeArmorComponent>();
    return !list.Any<CompositeArmorComponent>() ? 0.0f : (float) Math.Round((double) list.Average<CompositeArmorComponent>(predicate), 2);
  }

  private static float GetAverageBlunt(Item item)
  {
    return EquipmentPenaltyComponentPatch.GetAverage(new Func<CompositeArmorComponent, float>(EquipmentPenaltyComponentPatch.GetBluntThroughput), item);
  }

  private static float GetBluntThroughput(CompositeArmorComponent subArmor)
  {
    return ((ArmorComponent) subArmor).BluntThroughput;
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(
    EquipmentPenaltyComponent __instance,
    Item item,
    bool anyArmorPlateSlots)
  {
    Gear gearStats = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(item.TemplateId));
    int num;
    if (Plugin.ServerConfig.gear_weight)
    {
      if (!anyArmorPlateSlots)
      {
        MongoID? parentId = item.Template.ParentId;
        MongoID? nullable = MongoID.op_Implicit("5448e53e4bdc2d60728b4567");
        num = parentId.HasValue == nullable.HasValue ? (parentId.HasValue ? (MongoID.op_Equality(parentId.GetValueOrDefault(), nullable.GetValueOrDefault()) ? 1 : 0) : 1) : 0;
      }
      else
        num = 1;
    }
    else
      num = 0;
    if (num != 0)
    {
      float comfort = gearStats.Comfort;
      if ((double) comfort > 0.0 && (double) comfort != 1.0)
      {
        float comfortPercent = -1f * (float) Math.Round(((double) comfort - 1.0) * 100.0);
        item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.Comfort)
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
    float gasProtection = gearStats.GasProtection;
    if ((double) gasProtection > 0.0)
    {
      gasProtection *= 100f;
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.GasProtection)
      {
        Name = Attributes.ENewItemAttributeId.GasProtection.GetName(),
        Base = (Func<float>) (() => gasProtection),
        StringValue = (Func<string>) (() => gasProtection.ToString() + " %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    }
    float radProtection = gearStats.RadProtection;
    if ((double) radProtection > 0.0)
    {
      radProtection *= 100f;
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.RadProtection)
      {
        Name = Attributes.ENewItemAttributeId.RadProtection.GetName(),
        Base = (Func<float>) (() => radProtection),
        StringValue = (Func<string>) (() => radProtection.ToString() + " %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    }
    if (!gearStats.AllowADS)
      ((GClass3175) __instance).Item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.CantADS)
      {
        Name = Attributes.ENewItemAttributeId.CantADS.GetName(),
        StringValue = (Func<string>) (() => ""),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    ArmorComponent armorComponent;
    if (!anyArmorPlateSlots && !item.TryGetItemComponent<ArmorComponent>(ref armorComponent))
      return;
    item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.BluntThroughput)
    {
      Name = Attributes.ENewItemAttributeId.BluntThroughput.GetName(),
      Base = (Func<float>) (() => (float) (1.0 - (double) EquipmentPenaltyComponentPatch.GetAverageBlunt(item) * 100.0)),
      StringValue = (Func<string>) (() => ((float) ((1.0 - (double) EquipmentPenaltyComponentPatch.GetAverageBlunt(item)) * 100.0)).ToString() + " %"),
      DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
    });
    if (Plugin.ServerConfig.realistic_ballistics && gearStats.CanSpall)
    {
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.CanSpall)
      {
        Name = Attributes.ENewItemAttributeId.CanSpall.GetName(),
        StringValue = (Func<string>) (() => ""),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
      item.Attributes.Add(new ItemAttributeClass((Enum) Attributes.ENewItemAttributeId.SpallReduction)
      {
        Name = Attributes.ENewItemAttributeId.SpallReduction.GetName(),
        StringValue = (Func<string>) (() => ((float) ((1.0 - (double) gearStats.SpallReduction) * 100.0)).ToString() + " %"),
        DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1)
      });
    }
  }
}
