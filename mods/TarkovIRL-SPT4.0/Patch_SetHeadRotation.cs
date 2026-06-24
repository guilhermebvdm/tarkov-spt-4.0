// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_SetHeadRotation
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_SetHeadRotation : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _fcField;
  private static Vector3 _dzLerp = Vector3.zero;
  private static Vector3 _dzLerpTarget = Vector3.zero;

  protected override MethodBase GetTargetMethod()
  {
    Patch_SetHeadRotation._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_SetHeadRotation._fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
  {
    if (__instance == null)
      return true;
    Player.FirearmController firearmController = (Player.FirearmController) Patch_SetHeadRotation._fcField.GetValue((object) __instance);
    if (firearmController == null)
    {
      Patch_SetHeadRotation._dzLerpTarget = headRot;
      return true;
    }
    Player player = (Player) Patch_SetHeadRotation._playerField.GetValue((object) firearmController);
    if (player == null || !player.IsYourPlayer || (int)player.MovementContext.CurrentState.Name == 21)
      return true;
    Vector3 headRotInitial = headRot;
    if (PrimeMover.IsWeaponDeadzone.Value)
      headRotInitial = NewDeadzoneController.GetHeadRotWithDeadzone(headRotInitial);
    headRotInitial.y *= 1.5f;
    player.HeadRotation = headRotInitial;
    AccessTools.Field(typeof (ProceduralWeaponAnimation), "_headRotationVec").SetValue((object) __instance, (object) headRotInitial);
    return false;
  }
}
