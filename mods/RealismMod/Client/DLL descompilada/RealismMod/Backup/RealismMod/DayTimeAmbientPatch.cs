// Decompiled with JetBrains decompiler
// Type: RealismMod.DayTimeAmbientPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Audio.AmbientSubsystem;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

internal class DayTimeAmbientPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (DayTimeAmbientBlender).GetMethod("SetSeasonStatus");
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(DayTimeAmbientBlender __instance)
  {
    GameWorldController.RunEarlyGameCheck();
    return !GameWorldController.MuteAmbientAudio;
  }
}
