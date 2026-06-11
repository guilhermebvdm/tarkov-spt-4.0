// Decompiled with JetBrains decompiler
// Type: RealismMod.Gear
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class Gear : RealismItem
{
  public bool AllowADS { get; set; } = true;

  public string ArmorClass { get; set; } = "unclassified";

  public bool CanSpall { get; set; } = false;

  public float SpallReduction { get; set; } = 0.0f;

  public float ReloadSpeedMulti { get; set; } = 1f;

  public bool BlocksMouth { get; set; } = false;

  public bool IsGasMask { get; set; } = false;

  public float RadProtection { get; set; } = 0.0f;

  public float GasProtection { get; set; } = 0.0f;

  public string MaskToUse { get; set; } = string.Empty;

  public float dB { get; set; } = 0.0f;

  public float Comfort { get; set; } = 1f;
}
