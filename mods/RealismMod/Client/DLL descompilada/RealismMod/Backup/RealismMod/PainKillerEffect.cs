// Decompiled with JetBrains decompiler
// Type: RealismMod.PainKillerEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;

#nullable disable
namespace RealismMod;

public class PainKillerEffect : ICustomHealthEffect
{
  private bool addedEffect = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public float TunnelVisionStrength { get; }

  public float PainKillerStrength { get; set; }

  public PainKillerEffect(
    int? dur,
    Player player,
    int delay,
    float tunnelStrength,
    float painKillerStrength,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this.BodyPart = (EBodyPart) 0;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.PainKiller;
    this.TunnelVisionStrength = tunnelStrength;
    this.PainKillerStrength = painKillerStrength;
    this.RealHealthController = realHealthController;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    int? duration1 = this.Duration;
    this.Duration = duration1.HasValue ? new int?(duration1.GetValueOrDefault() - 1) : new int?();
    int? duration2 = this.Duration;
    int num1 = 0;
    if (duration2.GetValueOrDefault() <= num1 & duration2.HasValue)
    {
      Plugin.RealHealthController.PainReliefStrength -= this.PainKillerStrength;
      Plugin.RealHealthController.PainTunnelStrength -= this.TunnelVisionStrength;
    }
    else if (!this.addedEffect)
    {
      this.addedEffect = true;
      Plugin.RealHealthController.PainReliefStrength += this.PainKillerStrength;
      Plugin.RealHealthController.PainTunnelStrength += this.TunnelVisionStrength;
      RealismHealthController healthController = Plugin.RealHealthController;
      int reliefDuration = healthController.ReliefDuration;
      duration2 = this.Duration;
      int num2 = duration2.Value;
      healthController.ReliefDuration = reliefDuration + num2;
    }
  }
}
