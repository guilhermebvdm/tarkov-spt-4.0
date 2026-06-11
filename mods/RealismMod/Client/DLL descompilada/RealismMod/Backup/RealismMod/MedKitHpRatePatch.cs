// Decompiled with JetBrains decompiler
// Type: RealismMod.MedKitHpRatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class MedKitHpRatePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (MedKitComponent).GetMethod("get_HpResourceRate", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MedKitComponent __instance, ref float __result)
  {
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(((GClass3175) __instance).Item.TemplateId));
    if (GameWorldController.IsInRaid() || dataObj == null || !Plugin.RealHealthController.ShouldAlwaysAllowOutOfRaid(((GClass3175) __instance).Item, dataObj))
      return true;
    __result = 1f;
    return false;
  }
}
