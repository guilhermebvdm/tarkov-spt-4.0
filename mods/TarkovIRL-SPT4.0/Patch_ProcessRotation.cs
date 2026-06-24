// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_ProcessRotation
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_ProcessRotation : ModulePatch
{
  private static FieldInfo movementContextField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_ProcessRotation.movementContextField = AccessTools.Field(typeof (MovementState), "MovementContext");
    return (MethodBase) typeof (MovementState).GetMethod("ProcessRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MovementState __instance, float deltaTime)
  {
    MovementContext movementContext = (MovementContext) Patch_ProcessRotation.movementContextField.GetValue((object) __instance);
    TIRLUtils.LogError($"turn called, HandsToBodyAngle {movementContext.HandsToBodyAngle}, TrunkRotationLimit {movementContext.TrunkRotationLimit}");
    if ((double) Mathf.Abs(movementContext.HandsToBodyAngle) > (double) movementContext.TrunkRotationLimit * 0.20000000298023224)
      __instance.ProcessUpperbodyRotation(deltaTime);
    __instance.UpdateRotationSpeed(deltaTime);
    return false;
  }
}
