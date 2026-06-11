// Decompiled with JetBrains decompiler
// Type: RealismMod.GameWorldController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class GameWorldController
{
  public const float GAS_OPACITY_GLOBAL_MODI = 0.85f;
  public const float RAD_RAIN_MODI = 0.15f;
  public const float BASE_MAP_RAD_STRENGTH = 0.05f;
  public const float FOG_GAS_STRENGTH_MODI = 2.2f;

  public static bool DidExplosionClientSide { get; set; } = false;

  public static bool GameStarted { get; set; } = false;

  public static bool RanEarliestGameCheck { get; set; } = false;

  public static bool IsMapThatCanDoGasEvent { get; set; } = false;

  public static bool IsMapThatCanDoRadEvent { get; set; } = false;

  public static string CurrentMap { get; set; } = "";

  public static bool MapWithDynamicWeather { get; set; } = false;

  public static float CurrentMapRadStrength { get; private set; } = 0.0f;

  public static float CurrentGasEventStrength { get; private set; } = 0.0f;

  public static float CurrentGasEventStrengthBot { get; private set; } = 0.0f;

  public static List<LampController> Lights { get; set; } = new List<LampController>();

  public static List<ExfiltrationPoint> ExfilsInLocation { get; set; } = new List<ExfiltrationPoint>();

  public static bool IsRightDateForExp { get; private set; }

  public static float TimeInRaid { get; set; }

  public static GamePlayerOwner GamePlayerOwner { get; set; }

  public static bool DoMapGasEvent
  {
    get
    {
      return GameWorldController.IsMapThatCanDoGasEvent && Plugin.ModInfo.DoGasEvent && !Plugin.ModInfo.HasExploded && !GameWorldController.DidExplosionClientSide && (!Plugin.ModInfo.IsPreExplosion || !GameWorldController.IsRightDateForExp);
    }
  }

  public static bool DoMapRads
  {
    get => GameWorldController.IsMapThatCanDoRadEvent && Plugin.ModInfo.HasExploded;
  }

  public static bool MuteAmbientAudio
  {
    get
    {
      return Plugin.ModInfo.DoGasEvent || Plugin.ModInfo.IsPreExplosion && GameWorldController.IsRightDateForExp || GameWorldController.DidExplosionClientSide || GameWorldController.DoMapRads || Plugin.ModInfo.HasExploded;
    }
  }

  public static bool IsInRaid() => GClass2107.InRaid;

  public static void Reset()
  {
    GameWorldController.Lights.Clear();
    GameWorldController.ExfilsInLocation.Clear();
    GameWorldController.GameStarted = false;
    GameWorldController.RanEarliestGameCheck = false;
    GameWorldController.TimeInRaid = 0.0f;
  }

  public static void CalculateGasEventStrength()
  {
    if (!GameWorldController.DoMapGasEvent)
    {
      GameWorldController.CurrentGasEventStrengthBot = 0.0f;
      GameWorldController.CurrentGasEventStrength = 0.0f;
    }
    else
    {
      float num1 = PlayerState.EnviroType == 1 ? 0.4f : 1f;
      float num2 = Mathf.Clamp(Plugin.RealismWeatherComponent.TargetFog * 2.2f, 0.06f, 0.2f);
      float num3 = num2 * num1;
      GameWorldController.CurrentGasEventStrengthBot = num2;
      GameWorldController.CurrentGasEventStrength = Mathf.Lerp(GameWorldController.CurrentGasEventStrength, num3, 0.025f);
    }
  }

  public static void CalculateMapRadStrength()
  {
    if (!GameWorldController.DoMapRads)
    {
      GameWorldController.CurrentMapRadStrength = 0.0f;
    }
    else
    {
      bool flag = PlayerState.EnviroType == 1;
      GameWorldController.CurrentMapRadStrength = Mathf.Lerp(GameWorldController.CurrentMapRadStrength, Mathf.Clamp(0.05f * (flag ? 0.4f : 1f) + (flag ? 0.0f : Plugin.RealismWeatherComponent.TargetRain * 0.15f), 0.01f, 0.16f), 0.025f);
    }
  }

  public static void CheckDate()
  {
    DateTime utcNow = DateTime.UtcNow;
    GameWorldController.IsRightDateForExp = PluginConfig.ZoneDebug.Value || utcNow.Month == 10 && utcNow.Day >= 31 /*0x1F*/ || utcNow.Month == 11 && utcNow.Day <= 5;
  }

  public static void GameWorldUpdate()
  {
    GameWorldController.CalculateGasEventStrength();
    GameWorldController.CalculateMapRadStrength();
  }

  public static void RunEarlyGameCheck()
  {
    if (GameWorldController.RanEarliestGameCheck)
      return;
    GameWorldController.CheckDate();
    Plugin.RequestRealismDataFromServer(EUpdateType.ModInfo);
    GameWorldController.RanEarliestGameCheck = true;
  }

  public static float GetHeadsetVolume() => (float) PluginConfig.HeadsetGain.Value * 0.02f;

  public static float GetGameVolumeAsFactor()
  {
    SharedGameSettingsClass instance = Singleton<SharedGameSettingsClass>.Instance;
    if (((SharedGameSettingsClass.GClass2153<GClass1050>) instance?.Sound)?.Settings == null)
      return 1f;
    int? nullable = ((SharedGameSettingsClass.GClass2153<GClass1050>) instance.Sound).Settings.OverallVolume?.Value;
    return (nullable.HasValue ? new float?((float) nullable.GetValueOrDefault() * 0.1f) : new float?()) ?? 1f;
  }

  public static void RandomizeLootResources(Item item)
  {
    if (!Plugin.ServerConfig.bot_loot_changes)
      return;
    ResourceComponent resourceComponent;
    if (item.TryGetItemComponent<ResourceComponent>(ref resourceComponent) && (double) resourceComponent.MaxResource > 1.0)
    {
      resourceComponent.Value = Mathf.Round(resourceComponent.MaxResource * Random.Range(0.35f, 1f));
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) $"item {GClass2112.LocalizedName(item)}, value: {resourceComponent.Value}, max: {resourceComponent.MaxResource}");
    }
    FoodDrinkComponent foodDrinkComponent;
    if (item.TryGetItemComponent<FoodDrinkComponent>(ref foodDrinkComponent) && (double) foodDrinkComponent.MaxResource > 1.0)
    {
      foodDrinkComponent.HpPercent = Mathf.Round(foodDrinkComponent.MaxResource * Random.Range(0.35f, 1f));
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) $"item {GClass2112.LocalizedName(item)}, value: {foodDrinkComponent.HpPercent}, max: {foodDrinkComponent.MaxResource}");
    }
    MedKitComponent medKitComponent;
    if (item.TryGetItemComponent<MedKitComponent>(ref medKitComponent) && medKitComponent.MaxHpResource > 1)
    {
      medKitComponent.HpResource = Mathf.Round((float) medKitComponent.MaxHpResource * Random.Range(0.35f, 1f));
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) $"item {GClass2112.LocalizedName(item)}, value: {medKitComponent.HpResource}, max: {medKitComponent.MaxHpResource}");
    }
  }
}
