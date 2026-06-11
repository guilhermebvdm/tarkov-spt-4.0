// Decompiled with JetBrains decompiler
// Type: RealismMod.RadiationEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using System;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RadiationEffect : ICustomHealthEffect
{
  private int _bleedTimer = 0;

  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public RadiationEffect(
    int? dur,
    Player player,
    int delay,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.Radiation;
    this.RealHealthController = realHealthController;
    this.BodyPart = (EBodyPart) 1;
  }

  private void DoBleed()
  {
    float num = (float) Random.Range(1, 101);
    if ((double) HazardTracker.TotalRadiation < 30.0 || (double) num > (double) HazardTracker.TotalRadiation)
      return;
    this.RealHealthController.AddBaseEFTEffectIfNoneExisting(this._Player, "LightBleeding", (EBodyPart) (int) this.RealHealthController.BodyPartsArr[Random.Range(0, this.RealHealthController.BodyPartsArr.Length)], new float?(), new float?(), new float?(), new float?());
  }

  private float GetDrainRate()
  {
    float num = 1f - Mathf.Pow(Mathf.Abs(HazardTracker.RadTreatmentRate), 0.3f);
    float totalRadiation = HazardTracker.TotalRadiation;
    return ((double) totalRadiation < 30.0 ? 0.0f : ((double) totalRadiation <= 40.0 ? -0.15f : ((double) totalRadiation <= 50.0 ? -0.25f : ((double) totalRadiation <= 60.0 ? -0.4f : ((double) totalRadiation < 70.0 ? -0.7f : ((double) totalRadiation <= 80.0 ? -1.2f : ((double) totalRadiation <= 90.0 ? -1.6f : ((double) totalRadiation < 100.0 ? -2f : ((double) totalRadiation >= 100.0 ? -3f : 0.0f))))))))) * num;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    ++this.TimeExisted;
    ++this._bleedTimer;
    if (this.TimeExisted % 3 == 0)
    {
      float drainRate = this.GetDrainRate();
      if ((double) drainRate >= 0.0)
        return;
      for (int index = 0; index < this.RealHealthController.BodyPartsArr.Length; ++index)
      {
        EBodyPart ebodyPart = (EBodyPart) (int) this.RealHealthController.BodyPartsArr[index];
        drainRate *= ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(ebodyPart, false).Maximum / (ebodyPart == null ? 240f : 120f);
        this._Player.ActiveHealthController.AddEffect<RadiationDamage>(ebodyPart, new float?(0.0f), new float?(3f), new float?(2f), new float?(drainRate), (Action<RadiationDamage>) null);
      }
      if ((double) this._bleedTimer > (double) Mathf.Max((float) (600.0 * (1.0 - (double) HazardTracker.TotalRadiation / 100.0)), 30f))
      {
        this.DoBleed();
        this._bleedTimer = 0;
      }
    }
  }
}
