// Decompiled with JetBrains decompiler
// Type: RealismMod.TransmitterHalloweenEvent
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using System.Collections;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class TransmitterHalloweenEvent : Transmitter
{
  public bool TriggeredExplosion { get; private set; }

  protected override IEnumerator DoLogic()
  {
    this.CanTurnOn = false;
    float time = 0.0f;
    float clipLength = 0.0f;
    Plugin.RequestRealismDataFromServer(EUpdateType.TimeOfDay);
    while ((double) time < 0.5)
    {
      time += Time.deltaTime;
      yield return (object) null;
    }
    if (PluginConfig.ZoneDebug.Value)
    {
      ManualLogSource logger1 = Utils.Logger;
      bool flag = GameWorldController.IsRightDateForExp;
      string str1 = "GameWorldController.IsRightDateForExp " + flag.ToString();
      logger1.LogWarning((object) str1);
      ManualLogSource logger2 = Utils.Logger;
      flag = this.IsInRightLocation();
      string str2 = "IsInRightLocation() " + flag.ToString();
      logger2.LogWarning((object) str2);
      ManualLogSource logger3 = Utils.Logger;
      flag = Plugin.ModInfo.IsHalloween;
      string str3 = "Plugin.ModInfo.IsHalloween " + flag.ToString();
      logger3.LogWarning((object) str3);
      ManualLogSource logger4 = Utils.Logger;
      flag = Plugin.ModInfo.HasExploded;
      string str4 = "Plugin.ModInfo.HasExploded " + flag.ToString();
      logger4.LogWarning((object) str4);
      ManualLogSource logger5 = Utils.Logger;
      flag = GameWorldController.DidExplosionClientSide;
      string str5 = "GameWorldController.DidExplosionClientSide " + flag.ToString();
      logger5.LogWarning((object) str5);
      ManualLogSource logger6 = Utils.Logger;
      flag = Plugin.ModInfo.IsNightTime;
      string str6 = " Plugin.ModInfo.IsNightTime " + flag.ToString();
      logger6.LogWarning((object) str6);
    }
    bool isRightTime = PluginConfig.ZoneDebug.Value || GameWorldController.IsRightDateForExp && Plugin.ModInfo.IsNightTime;
    if (((!this.IsInRightLocation() || !Plugin.ModInfo.IsHalloween || Plugin.ModInfo.HasExploded ? 0 : (!GameWorldController.DidExplosionClientSide ? 1 : 0)) & (isRightTime ? 1 : 0)) != 0)
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["transmitter_success.wav"];
      this._audioSource.Play();
      this.SpawnQuestTrigger();
      this.TriggeredExplosion = true;
      this.AddSelfToDevicesList();
      this.DeactivateZone();
      this.PlaySoundForAI();
      clipLength = this._audioSource.clip.length;
      while ((double) time <= (double) clipLength)
      {
        time += Time.deltaTime;
        yield return (object) null;
      }
      Object.Instantiate<GameObject>(Plugin.ExplosionGO, new Vector3(-350f, 3f, -500f), new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
    }
    else
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["transmitter_fail.wav"];
      clipLength = this._audioSource.clip.length;
      this._audioSource.Play();
      this.PlaySoundForAI();
      while ((double) time <= (double) clipLength)
      {
        time += Time.deltaTime;
        yield return (object) null;
      }
      this.CanTurnOn = true;
    }
  }
}
