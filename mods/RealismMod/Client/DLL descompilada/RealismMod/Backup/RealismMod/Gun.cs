// Decompiled with JetBrains decompiler
// Type: RealismMod.Gun
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class Gun : RealismItem
{
  public string WeapType { get; set; } = string.Empty;

  public float BaseTorque { get; set; } = 0.0f;

  public bool HasShoulderContact { get; set; } = true;

  public float BaseReloadSpeedMulti { get; set; } = 1f;

  public string OperationType { get; set; } = string.Empty;

  public float WeapAccuracy { get; set; } = 1f;

  public float RecoilDamping { get; set; } = 0.6f;

  public float RecoilHandDamping { get; set; } = 0.7f;

  public bool WeaponAllowADS { get; set; } = true;

  public float BaseChamberSpeedMulti { get; set; } = 1f;

  public float MaxChamberSpeed { get; set; } = 1f;

  public float MinChamberSpeed { get; set; } = 1f;

  public bool IsManuallyOperated { get; set; } = false;

  public float MaxReloadSpeed { get; set; } = 1f;

  public float MinReloadSpeed { get; set; } = 1f;

  public float BaseChamberCheckSpeed { get; set; } = 1f;

  public float BaseFixSpeed { get; set; } = 1f;

  public float VisualMulti { get; set; } = 1f;

  public bool EnableBSGVisRecoil { get; set; } = true;

  public bool ReduceBSGVisRecoil { get; set; } = false;
}
