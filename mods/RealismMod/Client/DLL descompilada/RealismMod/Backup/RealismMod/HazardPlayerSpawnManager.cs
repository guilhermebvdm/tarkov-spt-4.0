// Decompiled with JetBrains decompiler
// Type: RealismMod.HazardPlayerSpawnManager
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class HazardPlayerSpawnManager
{
  private static List<Collider> HazardZones = new List<Collider>();

  public static void Register(Collider col)
  {
    if (HazardPlayerSpawnManager.HazardZones.Contains(col))
      return;
    HazardPlayerSpawnManager.HazardZones.Add(col);
  }

  public static void Unregister(Collider col) => HazardPlayerSpawnManager.HazardZones.Remove(col);

  public static void RestOnRaidEnd() => HazardPlayerSpawnManager.HazardZones.Clear();

  public static BoxCollider GetZoneContaining(Vector3 position)
  {
    foreach (Collider hazardZone in HazardPlayerSpawnManager.HazardZones)
    {
      if (!Object.op_Equality((Object) hazardZone, (Object) null) && !Object.op_Equality((Object) (hazardZone as BoxCollider), (Object) null))
      {
        Bounds bounds = hazardZone.bounds;
        if (((Bounds) ref bounds).Contains(position))
          return hazardZone as BoxCollider;
      }
    }
    return (BoxCollider) null;
  }
}
