// Decompiled with JetBrains decompiler
// Type: RealismMod.PassiveHealthRegenEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using System;

#nullable disable
namespace RealismMod;

public class PassiveHealthRegenEffect : ICustomHealthEffect
{
  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public PassiveHealthRegenEffect(Player player, RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = new int?();
    this.BodyPart = (EBodyPart) 7;
    this._Player = player;
    this.EffectType = EHealthEffectType.PassiveHealthRegen;
    this.RealHealthController = realHealthController;
  }

  public float GetRegenRate(float hydroPerc, float energyPerc)
  {
    return (float) (0.30000001192092896 * (double) hydroPerc * (double) energyPerc * (1.0 + (double) this._Player.Skills.VitalityBuffBleedChanceRed.Value));
  }

  public float GetHpThreshold()
  {
    return (float) (0.85000002384185791 * (1.0 - (double) this._Player.Skills.VitalityBuffBleedChanceRed.Value));
  }

  public float GetResourceThreshold()
  {
    return (float) (0.89999997615814209 * (1.0 - (double) this._Player.Skills.HealthBreakChanceRed.Value));
  }

  public void Tick()
  {
    ++this.TimeExisted;
    if (this.TimeExisted % 3 != 0 || this.RealHealthController.CancelPassiveRegen || this.RealHealthController.BlockPassiveRegen)
      return;
    foreach (int num1 in this.RealHealthController.BodyPartsArr)
    {
      EBodyPart bodyPart = (EBodyPart) num1;
      float current = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(bodyPart, false).Current;
      float maximum = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(bodyPart, false).Maximum;
      float num2 = current / maximum;
      float energyPerc = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).Energy.Current / ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).Energy.Maximum;
      float hydroPerc = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).Hydration.Current / ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).Hydration.Maximum;
      if (!this.RealHealthController.HasCustomEffectOfType(typeof (TourniquetEffect), bodyPart) && (double) current < (double) maximum && (double) num2 > (double) this.GetHpThreshold() && (double) energyPerc > (double) this.GetResourceThreshold() && (double) hydroPerc > (double) this.GetResourceThreshold())
      {
        float num3 = this.GetRegenRate(hydroPerc, energyPerc) * (((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(bodyPart, false).Maximum / 120f);
        this._Player.ActiveHealthController.AddEffect<HealthChange>(bodyPart, new float?(0.0f), new float?(3f), new float?(1f), new float?(num3), (Action<HealthChange>) null);
      }
    }
  }
}
