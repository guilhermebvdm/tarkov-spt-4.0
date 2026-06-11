// Decompiled with JetBrains decompiler
// Type: RealismMod.HazardGroup
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public class HazardGroup
{
  public bool IsTriggered { get; set; }

  public float SpawnChance { get; set; }

  public string QuestToEnable { get; set; }

  public string QuestToBlock { get; set; }

  public InteractableGroup InteractableGroup { get; set; }

  public List<Zone> Zones { get; set; }

  public List<Asset> Assets { get; set; }

  public List<RealismMod.Loot> Loot { get; set; }

  public List<string> AudioFiles { get; set; }
}
