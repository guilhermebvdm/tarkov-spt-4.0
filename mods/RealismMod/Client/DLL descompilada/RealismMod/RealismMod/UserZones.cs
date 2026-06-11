// Decompiled with JetBrains decompiler
// Type: RealismMod.UserZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class UserZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.SafeZone;

  [JsonProperty("FactoryUserZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsUserZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZUserZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineUserZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsUserZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsUserZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeUserZone")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseUserZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsUserZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveUserZones")]
  public List<HazardGroup> Reserve { get; set; }
}
