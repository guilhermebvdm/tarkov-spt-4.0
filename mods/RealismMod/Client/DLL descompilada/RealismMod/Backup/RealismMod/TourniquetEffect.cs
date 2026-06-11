// Decompiled with JetBrains decompiler
// Type: RealismMod.TourniquetEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Communications;
using EFT.HealthSystem;
using System;

#nullable disable
namespace RealismMod;

public class TourniquetEffect : ICustomHealthEffect
{
  private bool haveNotified = false;
  private bool haveRemovedSurgery = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; }

  public int TimeExisted { get; set; }

  public float HpPerTick { get; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public TourniquetEffect(
    float hpTick,
    int? dur,
    EBodyPart part,
    Player player,
    int delay,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.HpPerTick = -hpTick;
    this.Duration = dur;
    this.BodyPart = part;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.Tourniquet;
    this.RealHealthController = realHealthController;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    ++this.TimeExisted;
    if (!this.haveNotified)
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayWarningNotification($"Tourniquet Applied On {this.BodyPart.ToString()}, You Are Losing Health On This Limb. Use A Surgery Kit To Remove It", (ENotificationDurationType) 1);
      this.haveNotified = true;
    }
    if (!this.haveRemovedSurgery)
    {
      this.haveRemovedSurgery = true;
      this.RealHealthController.RemoveCustomEffectOfType(typeof (SurgeryEffect), this.BodyPart);
    }
    float current = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(this.BodyPart, false).Current;
    if (this.TimeExisted > 10 && this.TimeExisted % 10 == 0)
    {
      this.RealHealthController.RemoveBaseEFTEffect(this._Player, this.BodyPart, "HeavyBleeding");
      this.RealHealthController.RemoveBaseEFTEffect(this._Player, this.BodyPart, "LightBleeding");
    }
    if ((double) current > 25.0 && this.TimeExisted % 3 == 0)
      this._Player.ActiveHealthController.AddEffect<HealthChange>(this.BodyPart, new float?(0.0f), new float?(3f), new float?(1f), new float?(this.HpPerTick), (Action<HealthChange>) null);
  }
}
