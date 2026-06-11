// Decompiled with JetBrains decompiler
// Type: RealismMod.ArmorLevelUIPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine.UI;

#nullable disable
namespace RealismMod;

public class ArmorLevelUIPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ItemViewStats).GetMethod("method_1", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void PatchPrefix(ItemViewStats __instance, ArmorPlateItemClass armorPlate)
  {
    Image image = (Image) AccessTools.Field(typeof (ItemViewStats), "_armorClassIcon").GetValue((object) __instance);
    if (((ArmoredEquipmentItemClass) armorPlate).Armor.Template.ArmorClass <= 6)
      return;
    string key = $"{((ArmoredEquipmentItemClass) armorPlate).Armor.Template.ArmorClass}.png";
    image.sprite = Plugin.LoadedSprites[key];
  }
}
