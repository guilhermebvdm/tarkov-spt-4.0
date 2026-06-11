// Decompiled with JetBrains decompiler
// Type: RealismMod.WeaponMod
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class WeaponMod : RealismItem
{
  public string ModType { get; set; } = string.Empty;

  public float VerticalRecoil { get; set; } = 0.0f;

  public float HorizontalRecoil { get; set; } = 0.0f;

  public float Dispersion { get; set; } = 0.0f;

  public float CameraRecoil { get; set; } = 0.0f;

  public float AutoROF { get; set; } = 0.0f;

  public float SemiROF { get; set; } = 0.0f;

  public float ModMalfunctionChance { get; set; } = 0.0f;

  public float ReloadSpeed { get; set; } = 0.0f;

  public float AimSpeed { get; set; } = 0.0f;

  public float ChamberSpeed { get; set; } = 0.0f;

  public float Convergence { get; set; } = 0.0f;

  public bool CanCycleSubs { get; set; } = false;

  public float RecoilAngle { get; set; } = 0.0f;

  public bool StockAllowADS { get; set; } = false;

  public float FixSpeed { get; set; } = 0.0f;

  public float ModShotDispersion { get; set; } = 0.0f;

  public float MeleeDamage { get; set; } = 0.0f;

  public float MeleePen { get; set; } = 0.0f;

  public float Flash { get; set; } = 0.0f;

  public float AimStability { get; set; } = 0.0f;

  public float Handling { get; set; } = 0.0f;
}
