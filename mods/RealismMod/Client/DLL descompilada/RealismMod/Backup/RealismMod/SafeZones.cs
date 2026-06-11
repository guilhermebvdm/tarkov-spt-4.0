// Decompiled with JetBrains decompiler
// Type: RealismMod.SafeZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class SafeZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.SafeZone;

  [JsonProperty("FactorySafeZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsSafeZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZSafeZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineSafeZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsSafeZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsSafeZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeSafeZone")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseSafeZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsSafeZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveSafeZones")]
  public List<HazardGroup> Reserve { get; set; }
}
