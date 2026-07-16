using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_SetHeadRotation : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _fcField;
  private static FieldInfo _headRotVecField;
  private static ConditionalWeakTable<ProceduralWeaponAnimation, Player> _playerCache = new ConditionalWeakTable<ProceduralWeaponAnimation, Player>();
  private static Vector3 _dzLerp = Vector3.zero;
  private static Vector3 _dzLerpTarget = Vector3.zero;

  protected override MethodBase GetTargetMethod()
  {
    Patch_SetHeadRotation._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    Patch_SetHeadRotation._fcField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_firearmController");
    Patch_SetHeadRotation._headRotVecField = AccessTools.Field(typeof (ProceduralWeaponAnimation), "_headRotationVec");
    return (MethodBase) typeof (ProceduralWeaponAnimation).GetMethod("SetHeadRotation", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
  {
    if ((__instance == null))
      return true;
    Player player;
    if (!Patch_SetHeadRotation._playerCache.TryGetValue(__instance, out player))
    {
      Player.FirearmController firearmController = (Player.FirearmController) Patch_SetHeadRotation._fcField.GetValue((object) __instance);
      if ((firearmController != null))
      {
        player = (Player) Patch_SetHeadRotation._playerField.GetValue((object) firearmController);
        if ((player != null))
          Patch_SetHeadRotation._playerCache.Add(__instance, player);
      }
    }
    if ((player == null))
    {
      Patch_SetHeadRotation._dzLerpTarget = headRot;
      return true;
    }
    if (!player.IsYourPlayer || (int)player.MovementContext.CurrentState.Name == 21 || !PrimeMover.EnableMod.Value)
      return true;
    Vector3 headRotInitial = headRot;
    if (PrimeMover.IsWeaponDeadzone.Value)
      headRotInitial = NewDeadzoneController.GetHeadRotWithDeadzone(headRotInitial);
    headRotInitial.y *= 1.5f;
    player.HeadRotation = headRotInitial;
    if (Patch_SetHeadRotation._headRotVecField != (FieldInfo) null)
      Patch_SetHeadRotation._headRotVecField.SetValue((object) __instance, (object) headRotInitial);
    return false;
  }
}




