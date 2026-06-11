// Decompiled with JetBrains decompiler
// Type: RealismMod.LampPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Interactive;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class LampPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (LampController).GetMethod("ManualUpdate");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(LampController __instance)
  {
    GameWorldController.RunEarlyGameCheck();
    if (!Plugin.ServerConfig.enable_hazard_zones || !Plugin.ModInfo.IsHalloween || !GameWorldController.IsMapThatCanDoGasEvent || !GameWorldController.DidExplosionClientSide && !Plugin.ModInfo.HasExploded)
      return;
    ((Turnable) __instance).Switch((Turnable.EState) 3);
    ((Behaviour) __instance).enabled = false;
  }
}
