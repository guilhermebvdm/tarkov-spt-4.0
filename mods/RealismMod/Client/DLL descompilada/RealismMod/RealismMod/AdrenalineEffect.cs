// Decompiled with JetBrains decompiler
// Type: RealismMod.AdrenalineEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;

#nullable disable
namespace RealismMod;

public class AdrenalineEffect : ICustomHealthEffect
{
  private bool _addedAdrenalineEffect = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int? PositveEffectDuration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public float PositiveEffectDuration { get; }

  public float NegativeEffectDuration { get; }

  public float EffectStrength { get; }

  public AdrenalineEffect(
    Player player,
    int? dur,
    int delay,
    float negativeEffectDur,
    float posEffectDur,
    float strength,
    RealismHealthController realismHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this.PositveEffectDuration = new int?((int) posEffectDur);
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.Adrenaline;
    this.BodyPart = (EBodyPart) 1;
    this.PositiveEffectDuration = posEffectDur;
    this.NegativeEffectDuration = negativeEffectDur;
    this.EffectStrength = strength;
    this.RealHealthController = realismHealthController;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    if (!this._addedAdrenalineEffect)
    {
      this.RealHealthController.HasNegativeAdrenalineEffect = true;
      this.RealHealthController.HasPositiveAdrenalineEffect = true;
      this.RealHealthController.AddBaseEFTEffectIfNoneExisting(this._Player, "PainKiller", (EBodyPart) 0, new float?(0.0f), new float?(this.PositiveEffectDuration), new float?(3f), new float?(1f));
      this.RealHealthController.AddBasesEFTEffect(this._Player, "Tremor", (EBodyPart) 0, new float?(this.PositiveEffectDuration), new float?(this.NegativeEffectDuration), new float?(3f), new float?(this.EffectStrength));
      this._addedAdrenalineEffect = true;
    }
    int? nullable1 = this.PositveEffectDuration;
    this.PositveEffectDuration = nullable1.HasValue ? new int?(nullable1.GetValueOrDefault() - 1) : new int?();
    int? nullable2 = this.PositveEffectDuration;
    int num1 = 0;
    if (nullable2.GetValueOrDefault() <= num1 & nullable2.HasValue)
    {
      this.RealHealthController.HasPositiveAdrenalineEffect = false;
      this.PositveEffectDuration = new int?(0);
    }
    nullable2 = this.Duration;
    nullable1 = nullable2;
    this.Duration = nullable1.HasValue ? new int?(nullable1.GetValueOrDefault() - 1) : new int?();
    nullable2 = this.Duration;
    int num2 = 0;
    if (nullable2.GetValueOrDefault() <= num2 & nullable2.HasValue)
    {
      this.RealHealthController.HasNegativeAdrenalineEffect = false;
      this.Duration = new int?(0);
    }
  }
}
