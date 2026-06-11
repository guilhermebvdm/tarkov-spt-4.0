// Decompiled with JetBrains decompiler
// Type: RealismMod.VisualRecoilPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class VisualRecoilPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (WeaponRecoilProcessBase).GetMethod("CalculateRecoil", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(WeaponRecoilProcessBase __instance, float deltaTime, bool isAiming)
  {
    if (!PluginConfig.EnableBSGVisRecoil.Value || !WeaponStats.EnableBSGVisRecoil)
      return false;
    float num1 = WeaponStats.IsPistol ? 40f : 20f;
    float num2 = WeaponStats.IsPistol ? Mathf.Min(1.75f, (ShootController.FactoredTotalVRecoil + ShootController.FactoredTotalDispersion) / num1) : Mathf.Min(2.5f, (ShootController.FactoredTotalHRecoil + ShootController.FactoredTotalDispersion) / num1);
    float num3 = WeaponStats.ReduceBSGVisRecoil ? 1f : Mathf.Min(2.5f, (ShootController.FactoredTotalHRecoil + ShootController.FactoredTotalDispersion) / num1);
    float num4 = (isAiming ? __instance.CurveAimingValueMultiply : __instance.CurveAimingValueMultiply) * PluginConfig.BSGVisRecoilMulti.Value * num3;
    __instance._current = Vector3.op_Multiply(num4 * __instance.TransformationCurve.Evaluate(__instance._curveTime), __instance.method_0());
    __instance._curveTime += deltaTime * __instance.CurveTimeMultiply;
    return false;
  }
}
