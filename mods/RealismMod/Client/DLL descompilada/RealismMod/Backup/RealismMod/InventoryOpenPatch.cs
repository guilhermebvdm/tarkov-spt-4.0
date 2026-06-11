// Decompiled with JetBrains decompiler
// Type: RealismMod.InventoryOpenPatch
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

public class InventoryOpenPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("SetInventoryOpened", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(Player __instance, bool opened)
  {
    if (!__instance.IsYourPlayer)
      return;
    PlayerState.IsInInventory = Object.op_Inequality((Object) AccessTools.Field(typeof (Player), "_handsController").GetValue((object) __instance), (Object) null) && opened;
  }
}
