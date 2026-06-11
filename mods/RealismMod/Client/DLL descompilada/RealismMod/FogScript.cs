// Decompiled with JetBrains decompiler
// Type: FogScript
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using RealismMod;
using UnityEngine;

#nullable disable
public class FogScript : MonoBehaviour
{
  private ParticleSystem _ps;
  private ParticleSystem.MainModule _mainModule;
  private ParticleSystem.ShapeModule _shapeModule;
  private ParticleSystem.CollisionModule _collisionModule;
  private ParticleSystem.EmissionModule _emissionModule;
  private bool _isLabs = false;
  private int _maxParticles = 1500;
  private float _timeExisted = 0.0f;
  private float _alpha = 0.08f;
  private float _speed = 2.5f;
  private float _dynamicOpacityModi = 1f;
  private ParticleSystem.MinMaxCurve _startSpeedCurve = new ParticleSystem.MinMaxCurve(1f, 1f);
  private ParticleSystem.MinMaxCurve _lifeTimeCurve = new ParticleSystem.MinMaxCurve(20f, 30f);

  public ParticleSystem.MinMaxCurve ParticleSize { get; set; }

  public Vector3 Scale { get; set; }

  public float ParticleRate { get; set; }

  public float OpacityModi { get; set; }

  public float DynamicOpacityModiTarget { get; set; } = 1f;

  public float SpeedModi { get; set; }

  public bool UsePhysics { get; set; }

  private void Start()
  {
    this._ps = ((Component) this).GetComponent<ParticleSystem>();
    this._mainModule = this._ps.main;
    this._shapeModule = this._ps.shape;
    this._emissionModule = this._ps.emission;
    this._collisionModule = this._ps.collision;
    ((ParticleSystem.CollisionModule) ref this._collisionModule).enabled = this.UsePhysics;
    this._isLabs = GameWorldController.CurrentMap == "laboratory";
    if (!this._isLabs)
      return;
    this.Scale = new Vector3(this.Scale.x * 1.1f, this.Scale.y, this.Scale.z * 1.1f);
    this._maxParticles = 5000;
    this.ParticleRate *= 6f;
    this.ParticleSize = new ParticleSystem.MinMaxCurve(18f, 30f);
    this._alpha *= 2.5f;
  }

  private void Update()
  {
    if ((double) this._timeExisted <= 4.0)
      this._timeExisted += Time.deltaTime;
    this._dynamicOpacityModi = Mathf.Lerp(this._dynamicOpacityModi, this.DynamicOpacityModiTarget, 0.05f * Time.deltaTime);
    ((ParticleSystem.ShapeModule) ref this._shapeModule).scale = this.Scale;
    ((ParticleSystem.MainModule) ref this._mainModule).maxParticles = this._maxParticles;
    ((ParticleSystem.MainModule) ref this._mainModule).gravityModifier = ParticleSystem.MinMaxCurve.op_Implicit(0.0f);
    ((ParticleSystem.MainModule) ref this._mainModule).simulationSpeed = ((double) this._timeExisted <= 2.0 ? 1000f : this._speed) * this.SpeedModi;
    ((ParticleSystem.MainModule) ref this._mainModule).startSpeed = this._startSpeedCurve;
    ((ParticleSystem.MainModule) ref this._mainModule).startSize = this.ParticleSize;
    ((ParticleSystem.MainModule) ref this._mainModule).startLifetime = this._lifeTimeCurve;
    ((ParticleSystem.EmissionModule) ref this._emissionModule).rateOverTime = ParticleSystem.MinMaxCurve.op_Implicit(this.ParticleRate);
    ((ParticleSystem.MainModule) ref this._mainModule).startColor = ParticleSystem.MinMaxGradient.op_Implicit(new Color(1f, 1f, 1f, (float) ((double) this._alpha * (double) this.OpacityModi * (double) this._dynamicOpacityModi * 0.85000002384185791)));
  }
}
