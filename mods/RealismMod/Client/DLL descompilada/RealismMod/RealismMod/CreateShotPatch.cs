// Decompiled with JetBrains decompiler
// Type: RealismMod.CreateShotPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Ballistics;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class CreateShotPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BallisticsCalculator).GetMethod("CreateShot", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    BallisticsCalculator __instance,
    AmmoItemClass ammo,
    Vector3 origin,
    Vector3 direction,
    int fireIndex,
    string player,
    Item weapon,
    ref EftBulletClass __result,
    float speedFactor,
    int fragmentIndex = 0)
  {
    float num1 = (double) speedFactor > 1.0 ? Mathf.Pow(speedFactor, 4f) : speedFactor;
    float num2 = (double) speedFactor > 1.0 ? Mathf.Pow(speedFactor, 3f) : ((double) speedFactor < 1.0 ? Mathf.Pow(speedFactor, 0.45f) : speedFactor);
    int num3 = Random.Range(0, 512 /*0x0200*/);
    float num4 = ammo.InitialSpeed * speedFactor;
    float num5 = ammo.PenetrationChanceObstacle * num2;
    float num6 = (float) ammo.Damage * num1;
    float num7 = Mathf.Max(ammo.FragmentationChance * num1, 0.0f);
    float num8 = BallisticsCalculator.GetAmmoPenetrationPower(ammo, num3, (GInterface442) __instance.Randoms) * num2;
    float num9 = (float) (1.0 - (1.0 - (double) speedFactor) * 0.25);
    float num10 = Mathf.Max(ammo.BallisticCoeficient * num9, 0.01f);
    if (PluginConfig.EnableBallisticsLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) ("speed factor " + speedFactor.ToString()));
      ModulePatch.Logger.LogWarning((object) ("speed factored " + num4.ToString()));
    }
    __result = EftBulletClass.Create((Item) ammo, fragmentIndex, num3, origin, direction, num4, num4, ammo.BulletMassGram, ammo.BulletDiameterMilimeters, num6, num8, num5, ammo.RicochetChance, num7, 1f, ammo.MinFragmentsCount, ammo.MaxFragmentsCount, BallisticsCalculator.DefaultHitBody, (GInterface442) __instance.Randoms, num10, player, weapon, fireIndex, (EftBulletClass) null, false);
    return false;
  }
}
