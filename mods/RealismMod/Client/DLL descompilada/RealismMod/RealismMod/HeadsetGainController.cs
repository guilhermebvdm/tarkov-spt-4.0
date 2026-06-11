// Decompiled with JetBrains decompiler
// Type: RealismMod.HeadsetGainController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using Comfort.Common;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
namespace RealismMod;

public static class HeadsetGainController
{
  public static void IncreaseGain()
  {
    KeyboardShortcut keyboardShortcut = PluginConfig.IncGain.Value;
    if (!Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey) || !DeafenController.HasHeadSet || PluginConfig.HeadsetGain.Value >= 15)
      return;
    ++PluginConfig.HeadsetGain.Value;
    Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(0.0f, 0.0f, 0.0f), Plugin.RealismAudioController.DeviceAudioClips["beep.wav"], 0.0f, (BetterAudio.AudioSourceGroupType) 9, 100, 1f, (EOcclusionTest) 0, (AudioMixerGroup) null, false);
  }

  public static void DecreaseGain()
  {
    KeyboardShortcut keyboardShortcut = PluginConfig.DecGain.Value;
    if (!Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey) || !DeafenController.HasHeadSet || PluginConfig.HeadsetGain.Value <= -10)
      return;
    --PluginConfig.HeadsetGain.Value;
    Singleton<BetterAudio>.Instance.PlayAtPoint(new Vector3(0.0f, 0.0f, 0.0f), Plugin.RealismAudioController.DeviceAudioClips["beep.wav"], 0.0f, (BetterAudio.AudioSourceGroupType) 9, 100, 1f, (EOcclusionTest) 0, (AudioMixerGroup) null, false);
  }

  public static void AdjustHeadsetVolume()
  {
    HeadsetGainController.IncreaseGain();
    HeadsetGainController.DecreaseGain();
  }
}
