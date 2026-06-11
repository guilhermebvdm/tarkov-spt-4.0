// Decompiled with JetBrains decompiler
// Type: RealismMod.SingleFireRatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SingleFireRatePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("get_SingleFireRate", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(ref Weapon __instance, ref int __result)
  {
    if (!Utils.PlayerIsReady || ((Item) __instance)?.Owner == null || ((IContainer) ((Item) __instance)?.Owner)?.ID == null || !(((IContainer) ((Item) __instance).Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
      return true;
    AmmoTemplate currentAmmoTemplate = __instance.CurrentAmmoTemplate;
    float num = Mathf.Clamp(Mathf.Pow(__instance.Repairable.Durability / (float) __instance.Repairable.TemplateDurability, 0.1f), 0.95f, 1f);
    __result = currentAmmoTemplate != null ? (int) ((double) WeaponStats.SemiFireRate * (double) currentAmmoTemplate.casingMass * (double) num) : WeaponStats.SemiFireRate;
    return false;
  }
}
