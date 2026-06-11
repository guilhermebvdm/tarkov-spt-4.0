// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.ExplosionPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class ExplosionPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Grenade).GetMethod("Explosion", BindingFlags.Public | BindingFlags.Static);
  }

  [PatchPrefix]
  private static void PreFix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
  {
    Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
    float num = Vector3.Distance(grenadePosition, mainPlayer.Transform.position);
    if ((double) num > 15.0)
      return;
    DeafenController.GrenadeExploded = true;
    DeafenController.GrenadeTimer = 0.0f;
    DeafenController.ExplosionDeafFactor = grenadeItem.Contusion.z * (float) (-(double) num / 100.0 + 1.0);
    DeafenController.IncreaseDeafeningExplosion();
  }
}
