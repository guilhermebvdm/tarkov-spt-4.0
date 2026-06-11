// Decompiled with JetBrains decompiler
// Type: RealismMod.InteractionZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Interactive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class InteractionZone : InteractableObject, IZone
{
  public const float VALVE_AUDIO_VOL = 0.85f;
  public const float VALVE_STUCK_AUDIO_VOL = 0.7f;
  public const float LEAK_AUDIO_VOL = 0.3f;
  public const float BUTTON_AUDIO_VOL = 0.35f;
  public const float PANEL_LOOP_VOL = 0.55f;
  private EInteractableState _state;
  public List<ActionsTypesClass> InteractableActions = new List<ActionsTypesClass>();
  public List<IZone> HazardZones = new List<IZone>();
  private AudioSource _valveAudioSource;
  private AudioSource _loopAudioSource;
  private AudioSource _valveStuckSource;
  private AudioSource _buttonSource;
  private AudioSource _panelAudioSource;
  private GameObject _targetGameObject;
  private InteractableGroupComponent _groupParent;
  private bool _isRotating = false;
  private bool _buttonIsPressing = false;
  private bool _isDoingStuckRotation = false;
  private float _targetLoopVolume = 1f;
  private float _loopLerpSpeed = 0.1f;
  private float _baseLoopAudioVol = 1f;
  private bool _playLoopOnDesiredState = false;
  private string _onString = "On";
  private string _offString = "Off";
  private Action _onAction;
  private Action _offAction;

  public EZoneType ZoneType { get; }

  public float ZoneStrength { get; set; }

  public bool BlocksNav { get; set; }

  public bool UsesDistanceFalloff { get; set; }

  public bool IsAnalysable { get; set; }

  public bool HasBeenAnalysed { get; set; }

  public string Name { get; set; }

  public List<GameObject> ActiveDevices { get; set; }

  public InteractableSubZone InteractableData { get; set; }

  public event InteractionZone.OnStateChanged OnInteractableStateChanged;

  public EInteractableState State
  {
    get => this._state;
    set
    {
      this._state = value;
      InteractionZone.OnStateChanged interactableStateChanged = this.OnInteractableStateChanged;
      if (interactableStateChanged == null)
        return;
      interactableStateChanged(this, this.State);
    }
  }

  public List<GameObject> InteractableGameObjects { get; set; }

  private void Start()
  {
    this._onAction = this.InteractableData.InteractionType == EIneractableType.Valve ? new Action(this.TurnONValve) : new Action(this.TurnOnButton);
    this._offAction = this.InteractableData.InteractionType == EIneractableType.Valve ? new Action(this.TurnOffValve) : new Action(this.TurnOffButton);
    this._onString = this.InteractableData.InteractionType == EIneractableType.Valve ? "Turn Clockwise" : "Turn On";
    this._offString = this.InteractableData.InteractionType == EIneractableType.Valve ? "Turn AntiClockwise" : "Turn Off";
    this._playLoopOnDesiredState = this.InteractableData.InteractionType != EIneractableType.Valve;
    this._state = !this.InteractableData.Randomize ? this.InteractableData.StartingState : (Random.Range(0, 100) >= 50 ? EInteractableState.On : EInteractableState.Off);
    this._groupParent = ((Component) this).GetComponentInParent<InteractableGroupComponent>();
    ((MonoBehaviour) this).StartCoroutine(this.SetUpAudio());
    if (!string.IsNullOrWhiteSpace(this.InteractableData.TargeObject))
      this.GetChildObjects();
    this.GetHazardZones();
    this.InitActions();
  }

  private void Update()
  {
    this.LoopAudio(this._loopAudioSource, this._baseLoopAudioVol, this._loopLerpSpeed);
  }

  private void LoopAudio(AudioSource audioSource, float baseVolume, float speed)
  {
    if (Object.op_Equality((Object) audioSource, (Object) null))
      return;
    bool flag = this.State == this.InteractableData.DesiredEndState;
    this._targetLoopVolume = !this._playLoopOnDesiredState && !flag || this._playLoopOnDesiredState & flag ? baseVolume : 0.0f;
    if ((double) audioSource.volume == (double) this._targetLoopVolume)
      return;
    audioSource.volume = Mathf.MoveTowards(audioSource.volume, this._targetLoopVolume, speed * Time.deltaTime);
  }

  private void GetChildObjects()
  {
    Bounds bounds = ((Collider) ((Component) this).gameObject.GetComponentInParent<BoxCollider>()).bounds;
    Collider[] colliderArray = Physics.OverlapBox(((Bounds) ref bounds).center, ((Bounds) ref bounds).extents, Quaternion.identity);
    if (PluginConfig.ZoneDebug.Value)
      Utils.Logger.LogWarning((object) $"Found {colliderArray.Length} objects in the box collider bounds:");
    foreach (Collider collider in colliderArray)
    {
      if (((Object) ((Component) collider).gameObject).name == this.InteractableData.TargeObject)
      {
        this._targetGameObject = ((Component) ((Component) collider).gameObject.transform.parent).gameObject;
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) "===match");
      }
    }
  }

  private void GetHazardZones()
  {
    foreach (string zoneName in this.InteractableData.ZoneNames)
    {
      GameObject gameObject = GameObject.Find(zoneName);
      if (Object.op_Inequality((Object) gameObject, (Object) null))
      {
        if (PluginConfig.ZoneDebug.Value)
          Utils.Logger.LogWarning((object) ("found hazard zone GO " + zoneName));
        IZone component = gameObject.GetComponent<IZone>();
        if (component != null)
        {
          if (PluginConfig.ZoneDebug.Value)
            Utils.Logger.LogWarning((object) "found hazard component");
          this.HazardZones.Add(component);
        }
      }
    }
  }

  private IEnumerator SetUpAudio()
  {
    yield return (object) new WaitUntil((Func<bool>) (() => Plugin.RealismAudioController.ClipsAreReady));
    this.SetUpValveAudio();
    this.SetUpValveStuckAudio();
    this.SetUpButtonAudio();
    this.SetUpPanelLoopAudio();
    this.SetUpLeakAudio();
    this._loopAudioSource.Play();
  }

  private void SetUpButtonAudio()
  {
    this._buttonSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._buttonSource.clip = Plugin.RealismAudioController.InteractableClips["buttonpress.wav"];
    this._buttonSource.volume = 0.35f * GameWorldController.GetGameVolumeAsFactor();
    this._buttonSource.loop = false;
    this._buttonSource.playOnAwake = false;
    this._buttonSource.spatialBlend = 1f;
    this._buttonSource.minDistance = 1.5f;
    this._buttonSource.maxDistance = 5f;
    this._buttonSource.rolloffMode = (AudioRolloffMode) 0;
  }

  private void SetUpValveStuckAudio()
  {
    this._valveStuckSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._valveStuckSource.clip = Plugin.RealismAudioController.InteractableClips["valve_stuck.wav"];
    this._valveStuckSource.volume = 0.7f * GameWorldController.GetGameVolumeAsFactor();
    this._valveStuckSource.loop = false;
    this._valveStuckSource.playOnAwake = false;
    this._valveStuckSource.spatialBlend = 1f;
    this._valveStuckSource.minDistance = 1.5f;
    this._valveStuckSource.maxDistance = 5f;
    this._valveStuckSource.rolloffMode = (AudioRolloffMode) 0;
  }

  private void SetUpValveAudio()
  {
    this._valveAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._valveAudioSource.clip = Plugin.RealismAudioController.InteractableClips["valve_loop_3.wav"];
    this._valveAudioSource.volume = 0.85f * GameWorldController.GetGameVolumeAsFactor();
    this._valveAudioSource.loop = false;
    this._valveAudioSource.playOnAwake = false;
    this._valveAudioSource.spatialBlend = 1f;
    this._valveAudioSource.minDistance = 1.5f;
    this._valveAudioSource.maxDistance = 5f;
    this._valveAudioSource.rolloffMode = (AudioRolloffMode) 0;
  }

  private void SetUpPanelLoopAudio()
  {
    this._panelAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._panelAudioSource.clip = Plugin.RealismAudioController.InteractableClips["panel_hum_loop.wav"];
    this._panelAudioSource.volume = 0.55f * GameWorldController.GetGameVolumeAsFactor();
    this._panelAudioSource.loop = true;
    this._panelAudioSource.playOnAwake = false;
    this._panelAudioSource.spatialBlend = 1f;
    this._panelAudioSource.minDistance = 1.5f;
    this._panelAudioSource.maxDistance = 15f;
    this._panelAudioSource.rolloffMode = (AudioRolloffMode) 0;
  }

  private void SetUpLeakAudio()
  {
    this._loopAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._loopAudioSource.clip = this.InteractableData.InteractionType == EIneractableType.Valve ? Plugin.RealismAudioController.InteractableClips["gas_leak.wav"] : Plugin.RealismAudioController.InteractableClips["panel_hum_loop.wav"];
    this._loopAudioSource.volume = (this.InteractableData.InteractionType == EIneractableType.Valve ? 0.3f : 0.55f) * GameWorldController.GetGameVolumeAsFactor();
    this._baseLoopAudioVol = this._loopAudioSource.volume;
    this._loopLerpSpeed = this.InteractableData.InteractionType == EIneractableType.Valve ? 0.075f : 0.5f;
    this._loopAudioSource.loop = true;
    this._loopAudioSource.playOnAwake = false;
    this._loopAudioSource.spatialBlend = 1f;
    this._loopAudioSource.minDistance = 1.5f;
    this._loopAudioSource.maxDistance = 5f;
    this._loopAudioSource.rolloffMode = (AudioRolloffMode) 0;
    this._loopAudioSource.Play();
  }

  private IEnumerator AdjustVolume(float targetVolume, float speed, AudioSource audioSource)
  {
    targetVolume *= GameWorldController.GetGameVolumeAsFactor();
    while ((double) audioSource.volume != (double) targetVolume)
    {
      audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, speed * Time.deltaTime);
      yield return (object) null;
    }
    audioSource.volume = targetVolume;
  }

  private IEnumerator PlayRandomValveClipLoop()
  {
    while (this._isRotating)
    {
      if (!this._valveAudioSource.isPlaying)
      {
        string[] clips = new string[3]
        {
          "valve_loop_1.wav",
          "valve_loop_2.wav",
          "valve_loop_3.wav"
        };
        string clip = clips[Random.Range(0, clips.Length)];
        this._valveAudioSource.clip = Plugin.RealismAudioController.InteractableClips[clip];
        this._valveAudioSource.Play();
        clips = (string[]) null;
        clip = (string) null;
      }
      yield return (object) new WaitForSeconds(this._valveAudioSource.clip.length - 1f);
    }
  }

  private IEnumerator PlayButtonPressAudio(bool isTurningOn, bool isBlocked, bool sameState)
  {
    if (this._buttonIsPressing && !this._buttonSource.isPlaying)
    {
      if (isBlocked | isTurningOn)
        this.PlaySoundForAI();
      string file = isTurningOn ? "buttonpress.wav" : (isBlocked ? "buttonpress_blocked.wav" : "buttonpress_do_nothing.wav");
      this._buttonSource.clip = Plugin.RealismAudioController.InteractableClips[file];
      this._buttonSource.Play();
      file = (string) null;
    }
    yield return (object) new WaitForSeconds(this._buttonSource.clip.length + 1f);
  }

  private IEnumerator StopValveAudioLoop()
  {
    if (this._valveAudioSource.isPlaying)
      yield return (object) new WaitForSeconds(this._valveAudioSource.clip.length - this._valveAudioSource.time);
    this._valveAudioSource.Stop();
  }

  private void RotateValve(ref float elapsedTime, float duration, float maxSpeed)
  {
    float num = Mathf.Sin(elapsedTime / duration * 3.14159274f);
    this._targetGameObject.transform.Rotate(0.0f, 0.0f, maxSpeed * num * Time.deltaTime);
    elapsedTime += Time.deltaTime;
  }

  private IEnumerator ToggleButton(bool isTurningOn, bool isBlocked, bool sameState)
  {
    if (!this._buttonIsPressing)
    {
      this._buttonIsPressing = true;
      yield return (object) ((MonoBehaviour) this).StartCoroutine(this.PlayButtonPressAudio(isTurningOn, isBlocked, sameState));
      this._buttonIsPressing = false;
    }
  }

  private IEnumerator RotateStuck(float maxSpeed, float duration)
  {
    if (!this._isRotating && !this._isDoingStuckRotation)
    {
      this.PlaySoundForAI();
      this._isDoingStuckRotation = true;
      this._valveStuckSource.Play();
      float elapsedTime = 0.0f;
      while ((double) elapsedTime < (double) duration)
      {
        this.RotateValve(ref elapsedTime, duration, maxSpeed);
        yield return (object) null;
      }
      elapsedTime = 0.0f;
      while ((double) elapsedTime < (double) duration)
      {
        this.RotateValve(ref elapsedTime, duration, -maxSpeed);
        yield return (object) null;
      }
      this._isDoingStuckRotation = false;
    }
  }

  private void InitActions()
  {
    ((Component) this).gameObject.layer = LayerMask.NameToLayer("Interactive");
    this.InteractableActions.AddRange((IEnumerable<ActionsTypesClass>) new List<ActionsTypesClass>()
    {
      new ActionsTypesClass()
      {
        Name = this._onString,
        Action = this._onAction
      },
      new ActionsTypesClass()
      {
        Name = this._offString,
        Action = this._offAction
      }
    });
  }

  private IEnumerator RotateValveOverTime(float maxSpeed, float duration)
  {
    if (!this._isRotating && !this._isDoingStuckRotation)
    {
      this._isRotating = true;
      float elapsedTime = 0.0f;
      ((MonoBehaviour) this).StartCoroutine(this.PlayRandomValveClipLoop());
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.85f, 1f, this._valveAudioSource));
      while ((double) elapsedTime < (double) duration)
      {
        this.RotateValve(ref elapsedTime, duration, maxSpeed);
        yield return (object) null;
      }
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.0f, 1f, this._valveAudioSource));
      yield return (object) ((MonoBehaviour) this).StartCoroutine(this.StopValveAudioLoop());
      this._isRotating = false;
    }
  }

  public void Log()
  {
    Utils.Logger.LogWarning((object) $"{((Object) ((Component) this).gameObject).name} state is {this.State}");
  }

  protected void PlaySoundForAI() => Utils.GetYourPlayer().MovementContext.PlayBreachSound();

  private bool CanTurnValve(EInteractableState nextState)
  {
    bool flag1 = this._groupParent.ComplatedSteps >= this.InteractableData.CompletionStep;
    bool flag2 = this.State == nextState;
    if (!flag1 | flag2)
      ((MonoBehaviour) this).StartCoroutine(this.RotateStuck(nextState == EInteractableState.On ? 100f : -100f, 0.25f));
    return flag1 && !flag2;
  }

  private bool CanTurnOn(EInteractableState nextState)
  {
    bool flag1 = this._groupParent.ComplatedSteps >= this.InteractableData.CompletionStep;
    bool sameState = this.State == nextState;
    bool flag2 = this.State != nextState && nextState == EInteractableState.On;
    if (!flag1 | sameState)
      ((MonoBehaviour) this).StartCoroutine(this.ToggleButton(false, !flag1, sameState));
    return flag1 && !sameState;
  }

  public void TurnOnButton()
  {
    if (this._buttonIsPressing || !this.CanTurnOn(EInteractableState.On))
      return;
    ((MonoBehaviour) this).StopAllCoroutines();
    ((MonoBehaviour) this).StartCoroutine(this.ToggleButton(true, false, false));
    this.State = EInteractableState.On;
  }

  public void TurnOffButton()
  {
    if (this._buttonIsPressing || !this.CanTurnOn(EInteractableState.Off))
      return;
    ((MonoBehaviour) this).StopAllCoroutines();
    ((MonoBehaviour) this).StartCoroutine(this.ToggleButton(true, false, false));
    this.State = EInteractableState.Off;
  }

  public void TurnONValve()
  {
    if (this._isRotating || this._isDoingStuckRotation || !this.CanTurnValve(EInteractableState.On))
      return;
    ((MonoBehaviour) this).StopAllCoroutines();
    ((MonoBehaviour) this).StartCoroutine(this.RotateValveOverTime(300f, 5f));
    this.State = EInteractableState.On;
  }

  public void TurnOffValve()
  {
    if (this._isRotating || this._isDoingStuckRotation || !this.CanTurnValve(EInteractableState.Off))
      return;
    ((MonoBehaviour) this).StopAllCoroutines();
    ((MonoBehaviour) this).StartCoroutine(this.RotateValveOverTime(-300f, 5f));
    this.State = EInteractableState.Off;
  }

  public delegate void OnStateChanged(InteractionZone interacatble, EInteractableState newState);
}
