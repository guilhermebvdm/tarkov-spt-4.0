// Decompiled with JetBrains decompiler
// Type: RealismMod.AmbientSoundPlayerGroupPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Audio.AmbientSubsystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

internal class AmbientSoundPlayerGroupPatch : ModulePatch
{
  private static string[] _clipsToDisable = new string[11]
  {
    "lark",
    "crow",
    "nightingale",
    "greenmocking",
    "woodpecker",
    "robin",
    "raven",
    "rook",
    "bullfinch",
    "starling",
    "sparrow"
  };
  private static FieldInfo _playerGroupField;

  protected virtual MethodBase GetTargetMethod()
  {
    AmbientSoundPlayerGroupPatch._playerGroupField = AccessTools.Field(typeof (AmbientSoundPlayerGroup), "_soundPlayers");
    return (MethodBase) typeof (AmbientSoundPlayerGroup).GetMethod("Play");
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(AmbientSoundPlayerGroup __instance)
  {
    GameWorldController.RunEarlyGameCheck();
    if (!GameWorldController.MuteAmbientAudio)
      return true;
    foreach (BaseAmbientSoundPlayer ambientSoundPlayer in (List<BaseAmbientSoundPlayer>) AmbientSoundPlayerGroupPatch._playerGroupField.GetValue((object) __instance))
    {
      if (!((IEnumerable<string>) AmbientSoundPlayerGroupPatch._clipsToDisable).Contains<string>(((Object) ambientSoundPlayer).name.ToLower()))
        return false;
    }
    return false;
  }
}
