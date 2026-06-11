// Decompiled with JetBrains decompiler
// Type: RealismMod.TryGetPartsToApplyPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class TryGetPartsToApplyPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass2814<>).MakeGenericType(typeof (HealthControllerClass.GClass2819)).GetMethod("TryGetBodyPartToApply");
  }

  [PatchPrefix]
  private static bool Prefix(
    Item item,
    EBodyPart bodyPart,
    out EBodyPart? damagedBodyPart,
    ref bool __result)
  {
    TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    if (!GameWorldController.IsInRaid())
    {
      damagedBodyPart = new EBodyPart?(bodyPart);
      __result = true;
      return false;
    }
    damagedBodyPart = new EBodyPart?(bodyPart);
    return true;
  }
}
