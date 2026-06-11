// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.ADSAudioPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod.Audio;

public class ADSAudioPatch : ModulePatch
{
  private static FieldInfo _weaponSoundPlayerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ADSAudioPatch._weaponSoundPlayerField = AccessTools.Field(typeof (Player.FirearmController), "weaponSoundPlayer_0");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_60", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(Player.FirearmController __instance)
  {
    if (!StanceController.IsIdle() && StanceController.IsAiming)
      return false;
    float num = __instance.CalculateAimingSoundVolume() * PluginConfig.ADSVolume.Value;
    ((WeaponSoundPlayer) ADSAudioPatch._weaponSoundPlayerField.GetValue((object) __instance)).PlayAimingSound(num);
    return false;
  }
}
