// Decompiled with JetBrains decompiler
// Type: RealismMod.HealthDrain
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.HealthSystem;

#nullable disable
namespace RealismMod;

public class HealthDrain : ActiveHealthController.GClass2813, IEffect, GInterface308, GInterface323
{
  private float _hpPerTick;
  private float _time;
  private EBodyPart _bodyPart;

  public virtual void Started()
  {
    this._hpPerTick = this.Strength;
    this.SetHealthRatesPerSecond(this._hpPerTick, 0.0f, 0.0f, 0.0f);
    this._bodyPart = this.BodyPart;
  }

  public virtual void RegularUpdate(float deltaTime)
  {
    this._time += deltaTime;
    if ((double) this._time < 3.0)
      return;
    this._time -= 3f;
    if ((double) ((GClass2814<ActiveHealthController.GClass2813>) this.HealthController).GetBodyPartHealth(this._bodyPart, false).Current <= 0.0)
      return;
    double num = (double) this.HealthController.ApplyDamage(this._bodyPart, this._hpPerTick, GClass2855.Existence);
  }
}
