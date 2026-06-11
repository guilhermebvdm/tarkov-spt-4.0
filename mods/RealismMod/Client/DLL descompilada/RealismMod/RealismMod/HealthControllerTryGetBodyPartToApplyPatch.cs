// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthControllerTryGetBodyPartToApplyPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class HealthControllerTryGetBodyPartToApplyPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (HealthControllerClass).GetMethod("TryGetBodyPartToApply");
  }

  [PatchPrefix]
  private static bool Prefix(
    HealthControllerClass __instance,
    Item item,
    EBodyPart bodyPart,
    out EBodyPart? damagedBodyPart,
    ref bool __result)
  {
    ModulePatch.Logger.LogWarning((object) "HealthControllerClass TryGetBodyPartToApply");
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    if (!GameWorldController.IsInRaid() && Plugin.RealHealthController.ShouldAlwaysAllowOutOfRaid(item, dataObj))
    {
      damagedBodyPart = new EBodyPart?(bodyPart);
      __result = true;
      return false;
    }
    damagedBodyPart = new EBodyPart?(bodyPart);
    return true;
  }
}
