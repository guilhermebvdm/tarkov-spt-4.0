// Decompiled with JetBrains decompiler
// Type: RealismMod.SurgeryEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Communications;
using EFT.HealthSystem;
using System;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class SurgeryEffect : ICustomHealthEffect
{
  private bool triedtoRemoveTrnqt = false;
  private bool ranOnce = false;
  private float _painFactor = 0.0f;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public float HpPerTick { get; }

  public Player _Player { get; }

  public float HpRegened { get; set; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public float HpRegenLimitFactor { get; }

  public SurgeryEffect(
    float hpTick,
    int? dur,
    EBodyPart part,
    Player player,
    int delay,
    float limitFactor,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.HpRegened = 0.0f;
    this.HpPerTick = hpTick;
    this.Duration = dur;
    this.BodyPart = part;
    this._Player = player;
    this.Delay = delay;
    this.HpRegenLimitFactor = limitFactor;
    this.EffectType = EHealthEffectType.Surgery;
    this.RealHealthController = realHealthController;
    this._painFactor = this.RealHealthController.SurgeryPainFactor;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    ++this.TimeExisted;
    if (!this.ranOnce)
    {
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayMessageNotification($"Surgery Kit Applied On {this.BodyPart.ToString()}, Restoring HP.", (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
      this.RealHealthController.PainSurgeryStrength += this._painFactor;
      this.ranOnce = true;
    }
    if (!this.triedtoRemoveTrnqt)
    {
      this.triedtoRemoveTrnqt = true;
      if (this.RealHealthController.RemoveCustomEffectOfType(typeof (TourniquetEffect), this.BodyPart) && PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayMessageNotification("Surgical Kit Used, Removing Tourniquet Effect Present On: " + this.BodyPart.ToString(), (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
    }
    float current = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(this.BodyPart, false).Current;
    float maximum = ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(this.BodyPart, false).Maximum;
    float num = maximum * this.HpRegenLimitFactor;
    if ((double) this.HpRegened < (double) num && this.TimeExisted % 3 == 0)
    {
      this._Player.ActiveHealthController.AddEffect<HealthChange>(this.BodyPart, new float?(0.0f), new float?(3f), new float?(1f), new float?(this.HpPerTick), (Action<HealthChange>) null);
      this.HpRegened += this.HpPerTick;
    }
    if ((double) this.HpRegened < (double) num && (double) current != (double) maximum)
      return;
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayMessageNotification($"Surgical Kit Health Regeneration On {this.BodyPart.ToString()} Has Expired", (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
    this.Duration = new int?(0);
    this.RealHealthController.PainSurgeryStrength -= this._painFactor;
  }
}
