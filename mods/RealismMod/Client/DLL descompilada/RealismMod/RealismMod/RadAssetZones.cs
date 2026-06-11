// Decompiled with JetBrains decompiler
// Type: RealismMod.RadAssetZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class RadAssetZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.RadAssets;

  [JsonProperty("FactoryAssetZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsAssetZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZAssetZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineAssetZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsAssetZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsAssetZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeAssetZone")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseAssetZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsAssetZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveAssetZones")]
  public List<HazardGroup> Reserve { get; set; }
}
