using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal class Patch_UpdateSwayFactors : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;
  private static ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();

  protected override MethodBase GetTargetMethod()
  {
    Patch_UpdateSwayFactors.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_UpdateSwayFactors.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("UpdateSwayFactors", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(ProceduralWeaponAnimation __instance)
  {
    if ((__instance == null) || !PrimeMover.IsWeaponSway.Value || SwayController.IsSwayUpdatedThisFrame || !PrimeMover.EnableMod.Value)
      return;
    Player player;
    if (!Patch_UpdateSwayFactors._playerCache.TryGetValue(__instance, out player))
    {
      Player.FirearmController firearmController = (Player.FirearmController) Patch_UpdateSwayFactors.fcField.GetValue((object) __instance);
      if ((firearmController != null))
        player = (Player) Patch_UpdateSwayFactors.playerField.GetValue((object) firearmController);
      if ((player != null))
        Patch_UpdateSwayFactors._playerCache.Add(__instance, player);
    }
    if ((player == null) || !player.IsYourPlayer || player.MovementContext.CurrentState.Name == EPlayerState.Stationary)
      return;
    Vector3 newSway = SwayController.GetNewSway(__instance.MotionReact.SwayFactors, __instance.IsAiming);
    __instance.MotionReact.SwayFactors = newSway;
    SwayController.IsSwayUpdatedThisFrame = true;
  }
}




