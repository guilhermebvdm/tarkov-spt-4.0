// Decompiled with JetBrains decompiler
// Type: RealismMod.ResourceRateEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System;

#nullable disable
namespace RealismMod;

public class ResourceRateEffect : ICustomHealthEffect
{
  private bool addedEffect = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public ResourceRateEffect(
    int? dur,
    Player player,
    int delay,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.ResourceRate;
    this.BodyPart = (EBodyPart) 1;
    this.RealHealthController = realHealthController;
  }

  public void Tick()
  {
    if (this.addedEffect)
      return;
    this._Player.ActiveHealthController.AddEffect<ResourceRateDrain>(this.BodyPart, new float?(0.0f), new float?(), new float?(0.0f), new float?(0.0f), (Action<ResourceRateDrain>) null);
    this.addedEffect = true;
  }
}
