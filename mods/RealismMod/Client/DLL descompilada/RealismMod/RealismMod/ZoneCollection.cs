// Decompiled with JetBrains decompiler
// Type: RealismMod.ZoneCollection
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public interface ZoneCollection
{
  EZoneType ZoneType { get; set; }

  List<HazardGroup> Factory { get; set; }

  List<HazardGroup> Customs { get; set; }

  List<HazardGroup> GZ { get; set; }

  List<HazardGroup> Shoreline { get; set; }

  List<HazardGroup> Streets { get; set; }

  List<HazardGroup> Labs { get; set; }

  List<HazardGroup> Interchange { get; set; }

  List<HazardGroup> Lighthouse { get; set; }

  List<HazardGroup> Woods { get; set; }

  List<HazardGroup> Reserve { get; set; }
}
