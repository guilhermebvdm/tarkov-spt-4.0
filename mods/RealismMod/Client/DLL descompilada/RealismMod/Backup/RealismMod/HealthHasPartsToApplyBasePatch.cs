// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthHasPartsToApplyBasePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class HealthHasPartsToApplyBasePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass2814<>).MakeGenericType(typeof (HealthControllerClass.GClass2819)).GetMethod("HasPartsToApply");
  }

  [PatchPrefix]
  private static bool Prefix(Item item, ref IResult __result)
  {
    if (GameWorldController.IsInRaid())
      return true;
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    if (dataObj == null || !Plugin.RealHealthController.ShouldAlwaysAllowOutOfRaid(item, dataObj))
      return true;
    __result = SuccessfulResult.New;
    return false;
  }
}
