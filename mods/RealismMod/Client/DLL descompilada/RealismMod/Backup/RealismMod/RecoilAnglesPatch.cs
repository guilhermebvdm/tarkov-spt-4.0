// Decompiled with JetBrains decompiler
// Type: RealismMod.RecoilAnglesPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RecoilAnglesPatch : ModulePatch
{
  private static FieldInfo fcField;
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    RecoilAnglesPatch.fcField = AccessTools.Field(typeof (NewRecoilShotEffect), "_firearmController");
    RecoilAnglesPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (NewRecoilShotEffect).GetMethod("CalculateBaseRecoilParameters");
  }

  [PatchPrefix]
  public static bool Prefix(
    NewRecoilShotEffect __instance,
    float recoilSuspensionY,
    List<ShotsGroupSettings> shotsGroupSettingsList)
  {
    Player.FirearmController firearmController = (Player.FirearmController) RecoilAnglesPatch.fcField.GetValue((object) __instance);
    Player player = (Player) RecoilAnglesPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer)
      return true;
    if (shotsGroupSettingsList != null)
      __instance.ShotsGroupsSettings = shotsGroupSettingsList;
    float num1 = Mathf.LerpAngle(WeaponStats.TotalRecoilAngle * PluginConfig.RecoilAngleMulti.Value, (float) ((RotationRecoilProcessBase) __instance.HandRotationRecoil).MaxRecoilRotationAngle, recoilSuspensionY);
    float num2 = WeaponStats.TotalDispersion / (1f + recoilSuspensionY);
    __instance.BasicPlayerRecoilDegreeRange = new Vector2(num1 - num2, num1 + num2);
    __instance.BasicRecoilRadian = Vector2.op_Multiply(__instance.BasicPlayerRecoilDegreeRange, (float) Math.PI / 180f);
    ShootController.BaseTotalRecoilAngle = (float) Math.Round((double) num1, 2);
    ShootController.BaseTotalDispersion = (float) Math.Round((double) num2, 2);
    return false;
  }
}
