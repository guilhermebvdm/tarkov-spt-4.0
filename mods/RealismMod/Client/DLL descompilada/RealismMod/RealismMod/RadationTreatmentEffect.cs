// Decompiled with JetBrains decompiler
// Type: RealismMod.RadationTreatmentEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;

#nullable disable
namespace RealismMod;

public class RadationTreatmentEffect : ICustomHealthEffect
{
  private bool _addedRate = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public float DeradRate { get; private set; } = 0.0f;

  public RadationTreatmentEffect(
    Player player,
    int? dur,
    int delay,
    RealismHealthController realismHealthController,
    float rate)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.RealHealthController = realismHealthController;
    this.DeradRate = rate;
    this.EffectType = EHealthEffectType.RadiationTreatment;
    this.BodyPart = (EBodyPart) 1;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    if (!this._addedRate)
    {
      HazardTracker.RadTreatmentRate += this.DeradRate;
      this._addedRate = true;
    }
    int? duration1 = this.Duration;
    this.Duration = duration1.HasValue ? new int?(duration1.GetValueOrDefault() - 1) : new int?();
    int? duration2 = this.Duration;
    int num = 0;
    if (duration2.GetValueOrDefault() <= num & duration2.HasValue)
    {
      HazardTracker.RadTreatmentRate -= this.DeradRate;
      this.Duration = new int?(0);
    }
  }
}
