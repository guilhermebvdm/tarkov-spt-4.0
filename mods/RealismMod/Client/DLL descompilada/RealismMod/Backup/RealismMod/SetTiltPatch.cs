// Decompiled with JetBrains decompiler
// Type: RealismMod.SetTiltPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class SetTiltPatch : ModulePatch
{
  private static FieldInfo movementContextField;
  private static FieldInfo playerField;
  public static float tiltBeforeMount;

  protected virtual MethodBase GetTargetMethod()
  {
    SetTiltPatch.movementContextField = AccessTools.Field(typeof (MovementState), "MovementContext");
    SetTiltPatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementState).GetMethod("SetTilt", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static void Prefix(MovementState __instance, float tilt)
  {
    MovementContext movementContext = (MovementContext) SetTiltPatch.movementContextField.GetValue((object) __instance);
    if (!((Player) SetTiltPatch.playerField.GetValue((object) movementContext)).IsYourPlayer)
      return;
    float num = WeaponStats.BipodIsDeployed ? 0.5f : 2.5f;
    if (!StanceController.IsMounting)
      SetTiltPatch.tiltBeforeMount = tilt;
    else if ((double) Math.Abs(SetTiltPatch.tiltBeforeMount - tilt) > (double) num)
    {
      StanceController.IsMounting = false;
      SetTiltPatch.tiltBeforeMount = 0.0f;
    }
  }
}
