// Decompiled with JetBrains decompiler
// Type: RealismMod.OnItemAddedOrRemovedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class OnItemAddedOrRemovedPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("OnItemAddedOrRemoved", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player __instance)
  {
    if (!__instance.IsYourPlayer)
      return;
    StatCalc.CalcPlayerWeightStats(__instance);
    GearController.SetGearParamaters(__instance);
    GearController.CheckGear(__instance);
    GearController.CheckForDevices(__instance.Inventory);
    if (Plugin.ServerConfig.enable_hazard_zones)
      Plugin.RealHealthController.CheckInventoryForHazardousMaterials(__instance.Inventory);
  }
}
