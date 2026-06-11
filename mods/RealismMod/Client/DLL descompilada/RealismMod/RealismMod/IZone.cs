// Decompiled with JetBrains decompiler
// Type: RealismMod.IZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public interface IZone
{
  EZoneType ZoneType { get; }

  float ZoneStrength { get; set; }

  bool BlocksNav { get; set; }

  bool UsesDistanceFalloff { get; set; }

  bool IsAnalysable { get; set; }

  bool HasBeenAnalysed { get; set; }

  string Name { get; set; }

  List<GameObject> ActiveDevices { get; set; }

  InteractableSubZone InteractableData { get; set; }
}
