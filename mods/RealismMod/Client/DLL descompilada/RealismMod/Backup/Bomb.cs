// Decompiled with JetBrains decompiler
// Type: Bomb
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using RealismMod;
using UnityEngine;

#nullable disable
public class Bomb : MonoBehaviour
{
  public AnimationCurve SizeCurve;
  [Range(1f, 2500f)]
  public float NukeDuration = 2500f;
  [Range(1f, 1024f)]
  public float SizeCurve_multiply;
  public float LightRadius = 2048f;
  public float LightPower = 64f;
  public AnimationCurve Light2Power_curve;
  public AnimationCurve LightPower_curve;
  public AnimationCurve LightXCurve;
  public Transform ShockWaveTransform;
  public float sizeSpeed = 1f;
  private Vector3 _currentShockwaveVector;
  private float _currentShockwaveSize;
  public Light BlastLight;
  public Light BlastLight2;
  public float Emmis_mush;
  public float Emmis_steam;
  public AnimationCurve Mat_SizeCurve;
  public float _mat_SizeCurve_multiply;
  private float _elapsedTime;
  private float _xPos;
  private float _yPos;
  private float _zPos;

  private void Start()
  {
    this._currentShockwaveVector = new Vector3(0.0f, 0.0f, 0.0f);
    this._elapsedTime = 0.0f;
    this._currentShockwaveSize = 0.0f;
    GameWorldController.DidExplosionClientSide = true;
  }

  private void Update()
  {
    this._currentShockwaveSize += Time.deltaTime * this.sizeSpeed;
    this._currentShockwaveVector = new Vector3(this._currentShockwaveSize, this._currentShockwaveSize, this._currentShockwaveSize);
    this.ShockWaveTransform.localScale = this._currentShockwaveVector;
    this._elapsedTime += Time.deltaTime;
    this.BlastLight.intensity = this.LightPower_curve.Evaluate(this._elapsedTime / this.NukeDuration);
    this.BlastLight.range = this.LightRadius;
    this._xPos = this.LightXCurve.Evaluate(this._elapsedTime / this.NukeDuration);
    this._yPos = this.LightXCurve.Evaluate(this._elapsedTime / this.NukeDuration);
    this._zPos = this.LightXCurve.Evaluate(this._elapsedTime / this.NukeDuration);
    if ((double) this._elapsedTime <= (double) this.NukeDuration)
      return;
    Object.Destroy((Object) ((Component) this).gameObject);
  }
}
