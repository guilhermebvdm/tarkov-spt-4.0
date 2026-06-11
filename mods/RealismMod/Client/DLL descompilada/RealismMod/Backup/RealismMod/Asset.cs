// Decompiled with JetBrains decompiler
// Type: RealismMod.Asset
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class Asset
{
  public string AssetName { get; set; }

  public string Type { get; set; }

  public int Odds { get; set; }

  public bool RandomizeRotation { get; set; }

  public Position Position { get; set; }

  public Rotation Rotation { get; set; }
}
