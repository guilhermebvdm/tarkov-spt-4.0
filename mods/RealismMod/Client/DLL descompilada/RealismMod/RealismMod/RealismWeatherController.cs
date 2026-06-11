// Decompiled with JetBrains decompiler
// Type: RealismMod.RealismWeatherController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Weather;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RealismWeatherController : MonoBehaviour
{
  private static FieldInfo FogField = AccessTools.Field(typeof (WeatherDebug), "Fog");
  private static FieldInfo LighteningThunderField = AccessTools.Field(typeof (WeatherDebug), "LightningThunderProbability");
  private static FieldInfo RainField = AccessTools.Field(typeof (WeatherDebug), "Rain");
  private static FieldInfo TemperatureField = AccessTools.Field(typeof (WeatherDebug), "Temperature");
  private WeatherController wc;
  private float _elapsedTime = 0.0f;
  private float _gasFogTimer = 0.0f;
  private float _radRainTimer = 0.0f;
  private float _targetGasStrength = 0.05f;
  private float _targetGasCloudStrength = 0.4f;
  private float _radRainStrength = 0.15f;

  public float TargetCloudDensity { get; set; }

  public float TargetWindMagnitude { get; set; }

  public float TargetFog { get; set; }

  public float TargetLighteningThunder { get; set; }

  public float TargetRain { get; set; }

  public Vector2 TargetTopWindDirection { get; set; }

  public WeatherDebug.Direction TargetWindDirection { get; set; }

  private bool ModifyWeather
  {
    get
    {
      return Plugin.ModInfo.IsPreExplosion && GameWorldController.IsRightDateForExp || GameWorldController.DidExplosionClientSide || GameWorldController.DoMapGasEvent || GameWorldController.DoMapRads;
    }
  }

  private void Awake()
  {
  }

  private void Update()
  {
    if (!GameWorldController.MapWithDynamicWeather || !GameWorldController.GameStarted || !this.ModifyWeather)
      return;
    if (Object.op_Equality((Object) this.wc, (Object) null))
      this.wc = WeatherController.Instance;
    if (!Object.op_Inequality((Object) this.wc, (Object) null))
      return;
    if (Plugin.ModInfo.IsPreExplosion && !GameWorldController.DidExplosionClientSide && GameWorldController.IsRightDateForExp)
      this.DoPreExplosionWeather();
    else if (GameWorldController.DidExplosionClientSide)
      this.DoExplosionWeather();
    else if (GameWorldController.DoMapGasEvent)
      this.DoMapGasEventWeather();
    else if (GameWorldController.DoMapRads)
      this.DoMapRadWeather();
    this.wc.WeatherDebug.Enabled = true;
    this.wc.WeatherDebug.CloudDensity = this.TargetCloudDensity;
    this.wc.WeatherDebug.WindMagnitude = this.TargetWindMagnitude;
    this.wc.WeatherDebug.TopWindDirection = this.TargetTopWindDirection;
    this.wc.WeatherDebug.WindDirection = this.TargetWindDirection;
    RealismWeatherController.FogField.SetValue((object) this.wc.WeatherDebug, (object) this.TargetFog);
    RealismWeatherController.LighteningThunderField.SetValue((object) this.wc.WeatherDebug, (object) this.TargetLighteningThunder);
    RealismWeatherController.RainField.SetValue((object) this.wc.WeatherDebug, (object) this.TargetRain);
  }

  private void DoMapGasEventWeather()
  {
    this._gasFogTimer += Time.deltaTime;
    if ((double) this._gasFogTimer >= 200.0)
    {
      this._targetGasStrength = Random.Range(0.02f, 0.08f);
      this._targetGasCloudStrength = Random.Range(0.0f, 0.6f);
      this._gasFogTimer = 0.0f;
    }
    this.TargetRain = Mathf.Lerp(this.TargetRain, 0.0f, 0.025f * Time.deltaTime);
    this.TargetFog = Mathf.Lerp(this.TargetFog, this._targetGasStrength, 0.05f * Time.deltaTime);
    this.TargetCloudDensity = Mathf.Lerp(this.TargetCloudDensity, this._targetGasCloudStrength, 0.05f * Time.deltaTime);
    this.TargetLighteningThunder = Mathf.Lerp(this.TargetLighteningThunder, 0.0f, 0.1f * Time.deltaTime);
    this.TargetWindMagnitude = Mathf.Lerp(this.TargetWindMagnitude, -0.1f, 0.05f * Time.deltaTime);
  }

  private void DoMapRadWeather()
  {
    this._radRainTimer += Time.deltaTime;
    if ((double) this._radRainTimer >= 200.0)
    {
      this._radRainStrength = Mathf.Max(0.0f, Random.Range(-0.2f, 0.65f));
      this._radRainTimer = 0.0f;
    }
    float num = Mathf.Max(this._radRainStrength * 1.5f, 0.35f);
    this.TargetRain = Mathf.Lerp(this.TargetRain, this._radRainStrength, 0.05f * Time.deltaTime);
    this.TargetFog = Mathf.Lerp(this.TargetFog, this._radRainStrength * 0.025f, 0.025f * Time.deltaTime);
    this.TargetCloudDensity = Mathf.Lerp(this.TargetCloudDensity, num, 0.05f * Time.deltaTime);
    this.TargetWindMagnitude = Mathf.Lerp(this.TargetWindMagnitude, 0.0f, 0.05f * Time.deltaTime);
  }

  private void DoExplosionWeather()
  {
    float num = 200f;
    this._elapsedTime += Time.deltaTime;
    this.TargetWindDirection = (WeatherDebug.Direction) 4;
    this.TargetTopWindDirection = Vector2.up;
    if ((double) this._elapsedTime >= (double) num)
    {
      this.TargetRain = Mathf.Lerp(this.TargetRain, 2f, 0.025f * Time.deltaTime);
      this.TargetFog = Mathf.Lerp(this.TargetFog, 0.075f, 0.025f * Time.deltaTime);
      this.TargetCloudDensity = Mathf.Lerp(this.TargetCloudDensity, 1f, 0.025f * Time.deltaTime);
      this.TargetLighteningThunder = Mathf.Lerp(this.TargetLighteningThunder, 1f, 0.1f * Time.deltaTime);
      this.TargetWindMagnitude = Mathf.Lerp(this.TargetWindMagnitude, 0.1f, 0.05f * Time.deltaTime);
    }
    else
    {
      if ((double) this._elapsedTime < 10.0 || (double) this._elapsedTime >= (double) num)
        return;
      this.TargetRain = Mathf.Lerp(this.TargetRain, 0.0f, 0.05f * Time.deltaTime);
      this.TargetFog = Mathf.Lerp(this.TargetFog, 0.0f, 0.05f * Time.deltaTime);
      this.TargetCloudDensity = Mathf.Lerp(this.TargetCloudDensity, -0.75f, 0.25f * Time.deltaTime);
      this.TargetWindMagnitude = Mathf.Lerp(this.TargetWindMagnitude, 1.35f, 0.25f * Time.deltaTime);
    }
  }

  private void DoPreExplosionWeather()
  {
    this.TargetCloudDensity = 0.3f;
    this.TargetFog = 0.01f;
    this.TargetRain = 0.15f;
    this.TargetWindMagnitude = 0.0f;
    this.TargetLighteningThunder = 0.0f;
    this.TargetWindDirection = (WeatherDebug.Direction) 1;
    this.TargetTopWindDirection = Vector2.down;
  }
}
