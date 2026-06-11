// Decompiled with JetBrains decompiler
// Type: RealismMod.QuestZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class QuestZones : ZoneCollection
{
  public EZoneType ZoneType { get; set; } = EZoneType.Quest;

  [JsonProperty("FactoryQuestZones")]
  public List<HazardGroup> Factory { get; set; }

  [JsonProperty("CustomsQuestZones")]
  public List<HazardGroup> Customs { get; set; }

  [JsonProperty("GZQuestZones")]
  public List<HazardGroup> GZ { get; set; }

  [JsonProperty("ShorelineQuestZones")]
  public List<HazardGroup> Shoreline { get; set; }

  [JsonProperty("StreetsQuestZones")]
  public List<HazardGroup> Streets { get; set; }

  [JsonProperty("LabsQuestZones")]
  public List<HazardGroup> Labs { get; set; }

  [JsonProperty("InterchangeQuestZones")]
  public List<HazardGroup> Interchange { get; set; }

  [JsonProperty("LighthouseQuestZones")]
  public List<HazardGroup> Lighthouse { get; set; }

  [JsonProperty("WoodsQuestZones")]
  public List<HazardGroup> Woods { get; set; }

  [JsonProperty("ReserveQuestZones")]
  public List<HazardGroup> Reserve { get; set; }
}
