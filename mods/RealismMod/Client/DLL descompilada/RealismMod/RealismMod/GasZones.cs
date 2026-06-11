// Decompiled with JetBrains decompiler
// Type: RealismMod.GasZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class GasZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.Gas;

  [JsonProperty("FactoryGasZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsGasZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZGasZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineGasZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsGasZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsGasZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeGas")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseGasZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsGasZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveGasZones")]
  public List<HazardGroup> Reserve { get; set; }
}
