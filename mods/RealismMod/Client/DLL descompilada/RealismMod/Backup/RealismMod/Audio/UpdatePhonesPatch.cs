// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.UpdatePhonesPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod.Audio;

public class UpdatePhonesPatch : ModulePatch
{
  private static float GetHelmetProtection(InventoryEquipment equipment)
  {
    EDeafStrength deafStr = (EDeafStrength) 0;
    if (equipment.GetSlot((EquipmentSlot) 11).ContainedItem is CompoundItem)
    {
      int num;
      if (!(equipment.GetSlot((EquipmentSlot) 11).ContainedItem is ArmorItemClass containedItem))
      {
        num = 0;
      }
      else
      {
        ArmorComponent armor = ((ArmoredEquipmentItemClass) containedItem).Armor;
        if (armor == null)
        {
          num = 0;
        }
        else
        {
          EDeafStrength deaf = armor.Deaf;
          num = 1;
        }
      }
      if (num != 0)
        deafStr = ((ArmoredEquipmentItemClass) containedItem).Armor.Deaf;
    }
    return UpdatePhonesPatch.HelmeProtectionFactor(deafStr);
  }

  private static float HelmeProtectionFactor(EDeafStrength deafStr)
  {
    EDeafStrength edeafStrength = deafStr;
    if (edeafStrength == 1)
      return 0.9f;
    return edeafStrength == 2 ? 0.8f : 1f;
  }

  private static float GetHeadsetProtection(InventoryEquipment equipment)
  {
    CompoundItem containedItem = equipment.GetSlot((EquipmentSlot) 11).ContainedItem as CompoundItem;
    if (!(equipment.GetSlot((EquipmentSlot) 12).ContainedItem is HeadphonesItemClass headphonesItemClass1))
      headphonesItemClass1 = containedItem != null ? GClass3176.GetAllItemsFromCollection((GClass3050) containedItem).OfType<HeadphonesItemClass>().FirstOrDefault<HeadphonesItemClass>() : (HeadphonesItemClass) null;
    HeadphonesItemClass headphonesItemClass2 = headphonesItemClass1;
    if (headphonesItemClass2 != null)
    {
      DeafenController.HasHeadSet = true;
      HeadphonesTemplateClass template = headphonesItemClass2.Template;
      return Utils.CalcultateModifierFromRange(TemplateStats.GearStats[MongoID.op_Implicit(((Item) headphonesItemClass2).TemplateId)].dB, 16f, 26f, 0.25f, 0.8f);
    }
    DeafenController.HasHeadSet = false;
    return 1f;
  }

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("UpdatePhones", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    if (!__instance.IsYourPlayer)
      return;
    DeafenController.EarProtectionFactor = UpdatePhonesPatch.GetHeadsetProtection(__instance.Equipment) * UpdatePhonesPatch.GetHelmetProtection(__instance.Equipment);
  }
}
