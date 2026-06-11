// Decompiled with JetBrains decompiler
// Type: RealismMod.RealismEventInfo
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class RealismEventInfo : IRealismInfo
{
  public bool IsHalloween { get; set; }

  public bool DoGasEvent { get; set; }

  public bool DoExtraCultists { get; set; }

  public bool DoExtraRaiders { get; set; }

  public bool IsChristmas { get; set; }

  public bool IsPreExplosion { get; set; }

  public bool HasExploded { get; set; }

  public bool IsNightTime { get; set; }
}
