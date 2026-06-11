// Decompiled with JetBrains decompiler
// Type: RealismMod.OnGameStartPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using RealismMod.Audio;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class OnGameStartPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GameWorld).GetMethod("OnGameStarted", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(GameWorld __instance)
  {
    ((MonoBehaviour) Plugin.Instance).StartCoroutine(Plugin.RealismAudioController.LoadAudioClipsCoroutine());
    ProfileData.CurrentProfileId = Utils.GetYourPlayer().ProfileId;
    if (Plugin.ServerConfig.enable_hazard_zones)
    {
      GameWorldController.CurrentMap = Singleton<GameWorld>.Instance.MainPlayer.Location.ToLower();
      GameWorldController.MapWithDynamicWeather = !GameWorldController.CurrentMap.Contains("factory") && !(GameWorldController.CurrentMap == "laboratory");
      GameWorldController.IsMapThatCanDoGasEvent = GameWorldController.CurrentMap != "laboratory" && !GameWorldController.CurrentMap.Contains("factory");
      GameWorldController.IsMapThatCanDoRadEvent = GameWorldController.CurrentMap != "laboratory";
      Plugin.RealismAudioController.RunReInitPlayer();
      if (GameWorldController.DoMapGasEvent)
      {
        Player yourPlayer = Utils.GetYourPlayer();
        AmbientAudioInitializer.CreateAmbientAudioPlayer(yourPlayer, ((Component) yourPlayer).gameObject.transform, Plugin.RealismAudioController.GasEventAudioClips, volume: 2f);
        AmbientAudioInitializer.CreateAmbientAudioPlayer(yourPlayer, ((Component) yourPlayer).gameObject.transform, Plugin.RealismAudioController.GasEventLongAudioClips, true, 5f, 30f, 0.4f, 55f, 65f, 0.0f);
      }
      if (GameWorldController.DoMapRads)
      {
        Player yourPlayer = Utils.GetYourPlayer();
        AmbientAudioInitializer.CreateAmbientAudioPlayer(yourPlayer, ((Component) yourPlayer).gameObject.transform, Plugin.RealismAudioController.RadEventAudioClips, volume: 1.5f);
      }
      ZoneSpawner.CreateZones((ZoneCollection) ZoneData.GasZoneLocations);
      ZoneSpawner.CreateZones((ZoneCollection) ZoneData.RadZoneLocations);
      if (ZoneSpawner.ShouldSpawnDynamicZones())
        ZoneSpawner.CreateZones((ZoneCollection) ZoneData.RadAssetZoneLocations);
      ZoneSpawner.CreateZones((ZoneCollection) ZoneData.SafeZoneLocations);
      ZoneSpawner.CreateZones((ZoneCollection) ZoneData.QuestZoneLocations);
      ZoneSpawner.CreateZones((ZoneCollection) ZoneData.InteractionLocations);
      HazardTracker.GetHazardValues(ProfileData.CurrentProfileId);
      HazardTracker.ResetTracker();
    }
    GameWorldController.GameStarted = true;
  }
}
