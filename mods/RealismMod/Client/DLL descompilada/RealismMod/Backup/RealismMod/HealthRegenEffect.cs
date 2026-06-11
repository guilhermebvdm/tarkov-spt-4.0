// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthRegenEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using System;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class HealthRegenEffect : ICustomHealthEffect
{
  private bool deductedRecordedDamage = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public float HpPerTick { get; }

  public Player _Player { get; }

  public float HpRegened { get; set; }

  public float HpRegenLimit { get; }

  public int Delay { get; set; }

  public EDamageType DamageType { get; }

  public EHealthEffectType EffectType { get; }

  public HealthRegenEffect(
    float hpTick,
    int? dur,
    EBodyPart part,
    Player player,
    int delay,
    float limit,
    EDamageType damageType,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.HpRegened = 0.0f;
    this.HpRegenLimit = limit;
    this.HpPerTick = hpTick;
    this.Duration = dur;
    this.BodyPart = part;
    this._Player = player;
    this.Delay = delay;
    this.DamageType = damageType;
    this.EffectType = EHealthEffectType.HealthRegen;
    this.RealHealthController = realHealthController;
  }

  public void Tick()
  {
    ++this.TimeExisted;
    float current = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(this.BodyPart, false).Current;
    float maximum = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(this.BodyPart, false).Maximum;
    if (!this.deductedRecordedDamage)
    {
      if (this.DamageType == 32768 /*0x8000*/)
        this.RealHealthController.DmgeTracker.TotalHeavyBleedDamage = Mathf.Max(this.RealHealthController.DmgeTracker.TotalHeavyBleedDamage - this.HpRegenLimit, 0.0f);
      if (this.DamageType == 16384 /*0x4000*/)
        this.RealHealthController.DmgeTracker.TotalLightBleedDamage = Mathf.Max(this.RealHealthController.DmgeTracker.TotalLightBleedDamage - this.HpRegenLimit, 0.0f);
      this.deductedRecordedDamage = true;
    }
    if ((double) this.HpRegened >= (double) this.HpRegenLimit || (double) current >= (double) maximum || (double) current <= 0.0)
      this.Duration = new int?(0);
    if ((double) this.HpRegened >= (double) this.HpRegenLimit || this.Delay > 0 || this.TimeExisted % 3 != 0)
      return;
    this._Player.ActiveHealthController.AddEffect<HealthChange>(this.BodyPart, new float?(0.0f), new float?(3f), new float?(1f), new float?(this.HpPerTick), (Action<HealthChange>) null);
    this.HpRegened += this.HpPerTick;
  }
}
