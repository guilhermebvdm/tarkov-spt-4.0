using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public class Patch_PlayerRotate : ModulePatch
{
  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) AccessTools.Method(typeof (Player), "Rotate");
  }

  [PatchPrefix]
  private static bool Prefix(Player __instance, ref Vector2 deltaRotation, bool ignoreClamp)
  {
    FreeAimController.ApplyInput(ref deltaRotation);
    return true;
  }
}

