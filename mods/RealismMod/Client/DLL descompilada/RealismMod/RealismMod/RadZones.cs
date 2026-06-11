// Decompiled with JetBrains decompiler
// Type: RealismMod.RadZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class RadZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.Radiation;

  [JsonProperty("FactoryRadZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsRadZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZRadZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineRadZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsRadZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsRadZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeRadZones")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseRadZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsRadZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveRadZones")]
  public List<HazardGroup> Reserve { get; set; }
}
