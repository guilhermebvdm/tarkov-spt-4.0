// Decompiled with JetBrains decompiler
// Type: RealismMod.ZoneData
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class ZoneData
{
  public static SafeZones SafeZoneLocations;
  public static GasZones GasZoneLocations;
  public static GasAssetZones GasAssetZoneLocations;
  public static RadZones RadZoneLocations;
  public static RadAssetZones RadAssetZoneLocations;
  public static QuestZones QuestZoneLocations;
  public static InteractionZones InteractionLocations;

  private static T DeserializeHazardZones<T>(string file) where T : class
  {
    return JsonConvert.DeserializeObject<T>(File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\BepInEx\\plugins\\Realism\\data\\zones\\{file}.json"));
  }

  private static void MergeData(ZoneCollection original, ZoneCollection toBeMerged)
  {
    if (original == null || toBeMerged == null)
      return;
    original.Factory.AddRange((IEnumerable<HazardGroup>) toBeMerged.Factory);
    original.Customs.AddRange((IEnumerable<HazardGroup>) toBeMerged.Customs);
    original.GZ.AddRange((IEnumerable<HazardGroup>) toBeMerged.GZ);
    original.Shoreline.AddRange((IEnumerable<HazardGroup>) toBeMerged.Shoreline);
    original.Streets.AddRange((IEnumerable<HazardGroup>) toBeMerged.Streets);
    original.Labs.AddRange((IEnumerable<HazardGroup>) toBeMerged.Labs);
    original.Interchange.AddRange((IEnumerable<HazardGroup>) toBeMerged.Interchange);
    original.Lighthouse.AddRange((IEnumerable<HazardGroup>) toBeMerged.Lighthouse);
    original.Woods.AddRange((IEnumerable<HazardGroup>) toBeMerged.Woods);
    original.Reserve.AddRange((IEnumerable<HazardGroup>) toBeMerged.Reserve);
  }

  public static void DeserializeZoneData()
  {
    ZoneData.SafeZoneLocations = ZoneData.DeserializeHazardZones<SafeZones>("safe_zones");
    UserZones toBeMerged1 = ZoneData.DeserializeHazardZones<UserZones>("user_safe_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.SafeZoneLocations, (ZoneCollection) toBeMerged1);
    ZoneData.InteractionLocations = ZoneData.DeserializeHazardZones<InteractionZones>("interactable_zones");
    ZoneData.QuestZoneLocations = ZoneData.DeserializeHazardZones<QuestZones>("quest_zones");
    UserZones toBeMerged2 = ZoneData.DeserializeHazardZones<UserZones>("user_quest_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.QuestZoneLocations, (ZoneCollection) toBeMerged2);
    ZoneData.GasZoneLocations = ZoneData.DeserializeHazardZones<GasZones>("gas_zones");
    UserZones toBeMerged3 = ZoneData.DeserializeHazardZones<UserZones>("user_gas_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.GasZoneLocations, (ZoneCollection) toBeMerged3);
    ZoneData.GasAssetZoneLocations = ZoneData.DeserializeHazardZones<GasAssetZones>("gas_asset_zones");
    UserZones toBeMerged4 = ZoneData.DeserializeHazardZones<UserZones>("user_gas_asset_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.GasAssetZoneLocations, (ZoneCollection) toBeMerged4);
    ZoneData.RadZoneLocations = ZoneData.DeserializeHazardZones<RadZones>("rad_zones");
    UserZones toBeMerged5 = ZoneData.DeserializeHazardZones<UserZones>("user_rad_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.RadZoneLocations, (ZoneCollection) toBeMerged5);
    ZoneData.RadAssetZoneLocations = ZoneData.DeserializeHazardZones<RadAssetZones>("rad_asset_zones");
    UserZones toBeMerged6 = ZoneData.DeserializeHazardZones<UserZones>("user_rad_asset_zones");
    ZoneData.MergeData((ZoneCollection) ZoneData.RadAssetZoneLocations, (ZoneCollection) toBeMerged6);
  }

  public static List<HazardGroup> GetZones(EZoneType zoneType, string map)
  {
    ZoneCollection zoneCollection1;
    switch (zoneType)
    {
      case EZoneType.Radiation:
        zoneCollection1 = (ZoneCollection) ZoneData.RadZoneLocations;
        break;
      case EZoneType.Gas:
        zoneCollection1 = (ZoneCollection) ZoneData.GasZoneLocations;
        break;
      case EZoneType.RadAssets:
        zoneCollection1 = (ZoneCollection) ZoneData.RadAssetZoneLocations;
        break;
      case EZoneType.GasAssets:
        zoneCollection1 = (ZoneCollection) ZoneData.GasAssetZoneLocations;
        break;
      case EZoneType.Quest:
        zoneCollection1 = (ZoneCollection) ZoneData.QuestZoneLocations;
        break;
      case EZoneType.Interactable:
        zoneCollection1 = (ZoneCollection) ZoneData.InteractionLocations;
        break;
      default:
        zoneCollection1 = (ZoneCollection) ZoneData.SafeZoneLocations;
        break;
    }
    ZoneCollection zoneCollection2 = zoneCollection1;
    switch (map)
    {
      case "bigmap":
        return zoneCollection2.Customs;
      case "factory4_day":
      case "factory4_night":
        return zoneCollection2.Factory;
      case "interchange":
        return zoneCollection2.Interchange;
      case "laboratory":
        return zoneCollection2.Labs;
      case "lighthouse":
        return zoneCollection2.Lighthouse;
      case "rezervbase":
        return zoneCollection2.Reserve;
      case "sandbox":
      case "sandbox_high":
        return zoneCollection2.GZ;
      case "shoreline":
        return zoneCollection2.Shoreline;
      case "tarkovstreets":
        return zoneCollection2.Streets;
      case "woods":
        return zoneCollection2.Woods;
      default:
        return (List<HazardGroup>) null;
    }
  }

  public static IEnumerable<Vector3> GetSafeSpawns()
  {
    switch (Singleton<GameWorld>.Instance.MainPlayer.Location)
    {
      case "bigmap":
        return SafeSpawns.CustomsSpawns;
      case "factory4_day":
      case "factory4_night":
        return SafeSpawns.FactorySpawns;
      case "interchange":
        return SafeSpawns.InterchangeSpawns;
      case "laboratory":
        return SafeSpawns.LabsSpawns;
      case "lighthouse":
        return SafeSpawns.LighthouseSpawns;
      case "rezervbase":
        return SafeSpawns.ReserveSpawns;
      case "sandbox":
      case "sandbox_high":
        return SafeSpawns.GZSpawns;
      case "shoreline":
        return SafeSpawns.ShorelineSpawns;
      case "tarkovstreets":
        return SafeSpawns.SteetsSpawns;
      case "woods":
        return SafeSpawns.WoodsSpawns;
      default:
        return (IEnumerable<Vector3>) null;
    }
  }
}
