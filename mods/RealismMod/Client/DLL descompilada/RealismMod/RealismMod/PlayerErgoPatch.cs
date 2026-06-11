// Decompiled with JetBrains decompiler
// Type: RealismMod.PlayerErgoPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class PlayerErgoPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _skillField;

  protected virtual MethodBase GetTargetMethod()
  {
    PlayerErgoPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    PlayerErgoPatch._skillField = AccessTools.Field(typeof (Player.FirearmController), "gclass2017_0");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_12", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Player.FirearmController __instance, ref float __result)
  {
    Player player = (Player) PlayerErgoPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return true;
    SkillManager.GClass2017 gclass2017 = (SkillManager.GClass2017) PlayerErgoPatch._skillField.GetValue((object) __instance);
    float deltaErgonomics = gclass2017.DeltaErgonomics;
    if (!PluginConfig.OverrideMounting.Value && Plugin.ServerConfig.enable_stances && player.MovementContext.IsInMountedState)
      deltaErgonomics += player.MovementContext.PlayerMountingPointData.MountPointData.MountSideDirection != null || !__instance.BipodState ? gclass2017.MountingBonusErgo : gclass2017.BipodBonusErgo;
    bool flag = StanceController.IsBracing && StanceController.BracingDirection == EBracingDirection.Top;
    if (Plugin.ServerConfig.enable_stances && PluginConfig.OverrideMounting.Value && WeaponStats.BipodIsDeployed && !StanceController.IsMounting && !flag)
      deltaErgonomics -= 0.15f;
    __result = Mathf.Max(0.0f, __instance.Item.ErgonomicsTotal * (1f + deltaErgonomics + player.ErgonomicsPenalty));
    return false;
  }
}
