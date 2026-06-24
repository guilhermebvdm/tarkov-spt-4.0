// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_ProcessUpperbodyRotation
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_ProcessUpperbodyRotation : ModulePatch
{
  private static FieldInfo movementContextField;

  protected virtual MethodBase GetTargetMethod()
  {
    Patch_ProcessUpperbodyRotation.movementContextField = AccessTools.Field(typeof (MovementState), "MovementContext");
    return (MethodBase) typeof (MovementState).GetMethod("ProcessUpperbodyRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MovementState __instance, float deltaTime)
  {
    MovementContext movementContext = (MovementContext) Patch_ProcessUpperbodyRotation.movementContextField.GetValue((object) __instance);
    float num = Mathf.DeltaAngle((float) Math.Sign(movementContext.HandsToBodyAngle) * movementContext.TrunkRotationLimit, movementContext.HandsToBodyAngle);
    movementContext.ApplyRotation(Quaternion.Lerp(movementContext.TransformRotation, Quaternion.op_Multiply(movementContext.TransformRotation, Quaternion.Euler(0.0f, num, 0.0f)), 30f * PrimeMover.Instance.DeltaTime));
    return false;
  }
}
