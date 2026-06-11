// Decompiled with JetBrains decompiler
// Type: RealismMod.FoodPoisoningEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System;

#nullable disable
namespace RealismMod;

public class FoodPoisoningEffect : ICustomHealthEffect
{
  private bool addedEffect = false;
  private float effectStrength = 1f;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public FoodPoisoningEffect(
    int? dur,
    Player player,
    int delay,
    float strength,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.FoodPoisoning;
    this.BodyPart = (EBodyPart) 2;
    this.effectStrength = strength;
    this.RealHealthController = realHealthController;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    if (!this.addedEffect)
    {
      this._Player.ActiveHealthController.AddEffect<ResourceRateDrain>(this.BodyPart, new float?(0.0f), new float?(), new float?(0.0f), new float?(0.0f), (Action<ResourceRateDrain>) null);
      this.RealHealthController.AddBasesEFTEffect(this._Player, "TunnelVision", (EBodyPart) 0, new float?(1f), new float?(20f), new float?(5f), new float?(1f));
      this.RealHealthController.AddToExistingBaseEFTEffect(this._Player, "Contusion", (EBodyPart) 0, 1f, new float?(20f), 5f, 0.5f);
      this.RealHealthController.AddBasesEFTEffect(this._Player, "Tremor", (EBodyPart) 0, new float?(1f), new float?(20f), new float?(5f), new float?(1f));
      Plugin.RealismAudioController.PlayFoodPoisoningSFXInRaid(0.6f);
      this.addedEffect = true;
    }
    ++this.TimeExisted;
    if (this.TimeExisted % 30 == 0)
      Plugin.RealismAudioController.PlayFoodPoisoningSFXInRaid(0.45f);
  }
}
