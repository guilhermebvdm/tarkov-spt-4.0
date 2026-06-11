// Decompiled with JetBrains decompiler
// Type: RealismMod.ToxicityEffect
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using System;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ToxicityEffect : ICustomHealthEffect
{
  public RealismHealthController RealHealthController { get; set; }

  public EBodyPart BodyPart { get; set; }

  public int? Duration { get; }

  public int TimeExisted { get; set; }

  public Player _Player { get; }

  public int Delay { get; set; }

  public EHealthEffectType EffectType { get; }

  public ToxicityEffect(
    int? dur,
    Player player,
    int delay,
    RealismHealthController realHealthController)
  {
    this.TimeExisted = 0;
    this.Duration = dur;
    this._Player = player;
    this.Delay = delay;
    this.EffectType = EHealthEffectType.Toxicity;
    this.RealHealthController = realHealthController;
    this.BodyPart = (EBodyPart) 1;
  }

  private float GetDrainRate()
  {
    float num1 = 1f - Mathf.Pow(Mathf.Abs(HazardTracker.DetoxicationRate), 0.3f);
    float num2 = 0.0f;
    float num3 = (float) (0.14000000059604645 * (1.0 + (double) PlayerState.ImmuneSkillStrong));
    if (Plugin.RealHealthController.IsCoughingInGas && (double) HazardTracker.TotalToxicityRate > (double) num3)
      num2 += (float) (-(double) HazardTracker.TotalToxicityRate - 8.0);
    float totalToxicity = HazardTracker.TotalToxicity;
    if ((double) totalToxicity >= 30.0)
    {
      if ((double) totalToxicity > 40.0)
      {
        if ((double) totalToxicity > 50.0)
        {
          if ((double) totalToxicity > 60.0)
          {
            if ((double) totalToxicity > 70.0)
            {
              if ((double) totalToxicity > 80.0)
              {
                if ((double) totalToxicity > 90.0)
                {
                  if ((double) totalToxicity >= 100.0)
                  {
                    if ((double) totalToxicity >= 100.0)
                      num2 += -8f;
                  }
                  else
                    num2 += -6f;
                }
                else
                  num2 += -4f;
              }
              else
                num2 += -3f;
            }
            else
              num2 += -2.5f;
          }
          else
            num2 += -1.5f;
        }
        else
          num2 += -1f;
      }
      else
        num2 += -0.75f;
    }
    else
      num2 += 0.0f;
    return num2 * num1;
  }

  public void Tick()
  {
    if (this.Delay > 0)
      return;
    ++this.TimeExisted;
    if (this.TimeExisted % 3 == 0)
    {
      float drainRate = this.GetDrainRate();
      if ((double) drainRate >= 0.0)
        return;
      for (int index = 0; index < this.RealHealthController.BodyPartsArr.Length; ++index)
      {
        EBodyPart ebodyPart = (EBodyPart) (int) this.RealHealthController.BodyPartsArr[index];
        drainRate *= ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth(ebodyPart, false).Maximum / (ebodyPart == null ? 240f : 120f);
        this._Player.ActiveHealthController.AddEffect<ToxicityDamage>(ebodyPart, new float?(0.0f), new float?(3f), new float?(2f), new float?(drainRate), (Action<ToxicityDamage>) null);
      }
    }
  }
}
