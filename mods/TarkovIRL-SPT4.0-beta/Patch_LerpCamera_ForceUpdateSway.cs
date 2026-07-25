using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_LerpCamera_ForceUpdateSway : ModulePatch
{
  private static FieldInfo playerField;
  private static FieldInfo fcField;
  private static ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();

  protected override MethodBase GetTargetMethod()
  {
    Patch_LerpCamera_ForceUpdateSway.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_LerpCamera_ForceUpdateSway.fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("LerpCamera", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ProceduralWeaponAnimation __instance, float dt)
  {
    if ((__instance == null))
      return;
    Player player;
    if (!Patch_LerpCamera_ForceUpdateSway._playerCache.TryGetValue(__instance, out player))
    {
      Player.FirearmController firearmController = (Player.FirearmController) Patch_LerpCamera_ForceUpdateSway.fcField.GetValue((object) __instance);
      if ((firearmController != null))
        player = (Player) Patch_LerpCamera_ForceUpdateSway.playerField.GetValue((object) firearmController);
      if ((player != null))
        Patch_LerpCamera_ForceUpdateSway._playerCache.Add(__instance, player);
    }
    if ((player == null) || !player.IsYourPlayer)
      return;
    AnimStateController.SetCurrentBodyAnimState(player.MovementContext.CurrentState.AnimatorStateHash);
    PlayerMotionController.UpdateMovementInformation(player);
    if (player.IsInventoryOpened)
      return;
    __instance.UpdateSwayFactors();
  }
}

