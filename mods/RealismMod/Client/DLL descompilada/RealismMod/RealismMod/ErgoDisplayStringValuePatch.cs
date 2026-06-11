// Decompiled with JetBrains decompiler
// Type: RealismMod.ErgoDisplayStringValuePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ErgoDisplayStringValuePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("method_15", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref string __result)
  {
    Gun dataObj = TemplateStats.GetDataObj<Gun>(TemplateStats.GunStats, MongoID.op_Implicit(((Item) __instance).TemplateId));
    StatDeltaDisplay.DisplayDelta(__instance, dataObj);
    float num = __instance.Template.Ergonomics * (1f + UIWeaponStats.ErgoDelta);
    __result = Mathf.Clamp(num, 0.0f, 100f).ToString("0.##");
    return false;
  }
}
