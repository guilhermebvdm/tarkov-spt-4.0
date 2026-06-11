// Decompiled with JetBrains decompiler
// Type: RealismMod.StartReloadPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class StartReloadPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) ((IEnumerable<MethodInfo>) typeof (Player.FirearmController.GClass1785).GetMethods(BindingFlags.Instance | BindingFlags.Public)).First<MethodInfo>((Func<MethodInfo, bool>) (m => m.GetParameters().Length == 2 && m.GetParameters()[0].Name == "reloadExternalMagResult" && m.GetParameters()[1].Name == "callback"));
  }

  [PatchPrefix]
  private static void Prefix(
    Player.FirearmController.GClass1785 __instance,
    Player.FirearmController.GClass1775 reloadExternalMagResult)
  {
    Player.FirearmController firearmController = (Player.FirearmController) AccessTools.Field(typeof (Player.FirearmController.GClass1785), "firearmController_0").GetValue((object) __instance);
    Player player = (Player) AccessTools.Field(typeof (Player.FirearmController), "_player").GetValue((object) firearmController);
    if (!player.IsYourPlayer || player.MovementContext.CurrentState.Name == 21)
      return;
    Plugin.CanLoadChamber = true;
    Plugin.BlockChambering = false;
  }
}
