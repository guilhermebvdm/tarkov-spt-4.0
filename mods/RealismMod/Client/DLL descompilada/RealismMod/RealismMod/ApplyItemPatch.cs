// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyItemPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ApplyItemPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GControl4).GetMethod("ApplyItem", new Type[3]
    {
      typeof (Item),
      typeof (EBodyPart),
      typeof (float)
    });
  }

  [PatchPrefix]
  private static bool Prefix(
    GControl4 __instance,
    Item item,
    EBodyPart bodyPart,
    ref bool __result)
  {
    Player player = (Player) AccessTools.Field(typeof (GControl4), "Player").GetValue((object) __instance);
    if (!player.IsYourPlayer || !((GClass2814<ActiveHealthController.GClass2813>) __instance).CanApplyItem(item, bodyPart))
      return true;
    bool canUse = true;
    if (item is MedsItemClass _)
    {
      if (PluginConfig.EnableMedicalLogging.Value)
        ModulePatch.Logger.LogWarning((object) "ApplyItem Med");
      Plugin.RealHealthController.CanUseMedItem(player, bodyPart, item, ref canUse);
    }
    if (item is FoodDrinkItemClass _)
    {
      if (PluginConfig.EnableMedicalLogging.Value)
        ModulePatch.Logger.LogWarning((object) "ApplyItem Food");
      Plugin.RealHealthController.CanConsume(player, item, ref canUse);
    }
    __result = canUse;
    return canUse;
  }
}
