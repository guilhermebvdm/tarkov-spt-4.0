// Decompiled with JetBrains decompiler
// Type: RealismMod.DayTimeSpawnPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

internal class DayTimeSpawnPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ZoneLeaveControllerClass).GetMethod("IsDayByHour");
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(ref bool __result)
  {
    GameWorldController.RunEarlyGameCheck();
    if (!Plugin.ModInfo.DoGasEvent)
      return true;
    __result = false;
    return false;
  }
}
