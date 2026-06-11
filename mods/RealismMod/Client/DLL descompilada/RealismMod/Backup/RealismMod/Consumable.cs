// Decompiled with JetBrains decompiler
// Type: RealismMod.Consumable
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class Consumable : RealismItem
{
  public float Duration { get; set; } = 0.0f;

  public float Delay { get; set; } = 0.0f;

  public float EffectPeriod { get; set; } = 0.0f;

  public float WaitPeriod { get; set; } = 0.0f;

  public float TunnelVisionStrength { get; set; } = 0.0f;

  public float Strength { get; set; } = 0.0f;

  public bool CanBeUsedInRaid { get; set; } = true;

  public EConsumableType ConsumableType { get; set; } = EConsumableType.None;

  public EHeavyBleedHealType HeavyBleedHealType { get; set; } = EHeavyBleedHealType.None;

  public float TrnqtDamage { get; set; } = 0.0f;

  public float HPRestoreAmount { get; set; } = 0.0f;

  public float HPRestoreTick { get; set; } = 0.0f;

  public bool DoesExtraResourceDebuff { get; set; } = false;
}
