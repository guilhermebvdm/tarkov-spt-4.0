// Decompiled with JetBrains decompiler
// Type: RealismMod.StimEffectShell
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Communications;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class StimEffectShell : ICustomHealthEffect
{
  private bool _hasRemovedTrnqt = false;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; set; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public EStimType StimType { get; }

  public StimEffectShell(
    Player player,
    int? dur,
    int delay,
    EStimType stimType,
    RealismHealthController realismHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.Stim;
    this.BodyPart = (EBodyPart) 0;
    this.StimType = stimType;
    this.RealHealthController = realismHealthController;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    if (!this._hasRemovedTrnqt && (this.StimType == EStimType.Regenerative || this.StimType == EStimType.Clotting))
    {
      this.RealHealthController.RemoveEffectsOfType(EHealthEffectType.Tourniquet);
      if (PluginConfig.EnableMedNotes.Value)
        NotificationManagerClass.DisplayMessageNotification("Removing Tourniquet Effects Due To Stim Type: " + this.StimType.ToString(), (ENotificationDurationType) 1, (ENotificationIconType) 0, new Color?());
      this._hasRemovedTrnqt = true;
    }
    int? duration1 = this.Duration;
    this.Duration = duration1.HasValue ? new int?(duration1.GetValueOrDefault() - 1) : new int?();
    int? duration2 = this.Duration;
    int num = 0;
    if (duration2.GetValueOrDefault() <= num & duration2.HasValue)
      this.Duration = new int?(0);
  }
}
