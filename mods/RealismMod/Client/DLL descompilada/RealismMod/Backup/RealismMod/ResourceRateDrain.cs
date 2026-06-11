// Decompiled with JetBrains decompiler
// Type: RealismMod.ResourceRateDrain
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.HealthSystem;

#nullable disable
namespace RealismMod;

public class ResourceRateDrain : 
  ActiveHealthController.GClass2813,
  IEffect,
  GInterface308,
  GInterface323
{
  private float _resourcePerTick;
  private float _time;
  private EBodyPart _bodyPart;

  public virtual void Started()
  {
    this._resourcePerTick = Plugin.RealHealthController.ResourcePerTick;
    this._bodyPart = this.BodyPart;
    this.SetHealthRatesPerSecond(0.0f, -this._resourcePerTick, -this._resourcePerTick, 0.0f);
  }

  public virtual void RegularUpdate(float deltaTime)
  {
    this._time += deltaTime;
    if ((double) this._time < 3.0)
      return;
    this._time -= 3f;
    this._resourcePerTick = Plugin.RealHealthController.ResourcePerTick;
    this.SetHealthRatesPerSecond(0.0f, -this._resourcePerTick * PluginConfig.EnergyRateMulti.Value, -this._resourcePerTick * PluginConfig.HydrationRateMulti.Value, 0.0f);
  }
}
