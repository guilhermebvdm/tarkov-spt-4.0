// Decompiled with JetBrains decompiler
// Type: RealismMod.UpdateWeaponVariablesPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class UpdateWeaponVariablesPatch : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;

  protected virtual MethodBase GetTargetMethod()
  {
    UpdateWeaponVariablesPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    UpdateWeaponVariablesPatch.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("UpdateWeaponVariables", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance)
  {
    Player.FirearmController firearmController = (Player.FirearmController) UpdateWeaponVariablesPatch.fcField.GetValue((object) __instance);
    if (Object.op_Equality((Object) firearmController, (Object) null))
      return;
    Player player = (Player) UpdateWeaponVariablesPatch.playerField.GetValue((object) firearmController);
    if (!Object.op_Inequality((Object) player, (Object) null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    StatCalc.UpdateAimParameters(firearmController, __instance);
    __instance.Shootingg.CalculateRecoilParameters();
  }
}
