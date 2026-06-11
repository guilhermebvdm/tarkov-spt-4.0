// Decompiled with JetBrains decompiler
// Type: RealismMod.LabsSafeZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class LabsSafeZone : TriggerWithId, IZone
{
  private const float MAIN_VOLUME = 0.5f;
  private const float SHUT_VOLUME = 0.46f;
  private const float OPEN_VOLUME = 0.17f;
  private Dictionary<Player, PlayerZoneBridge> _containedPlayers = new Dictionary<Player, PlayerZoneBridge>();
  private BoxCollider _zoneCollider;
  private float _tick = 0.0f;
  private float _distanceToCenter = 0.0f;
  private AudioSource _mainAudioSource;
  private AudioSource _doorShutAudioSource;
  private AudioSource _doorOpenAudioSource;
  private List<Door> _doors = new List<Door>();
  private Dictionary<WorldInteractiveObject, EDoorState> _previousDoorStates = new Dictionary<WorldInteractiveObject, EDoorState>();

  public EZoneType ZoneType { get; } = EZoneType.SafeZone;

  public float ZoneStrength { get; set; } = 0.0f;

  public bool BlocksNav { get; set; }

  public bool UsesDistanceFalloff { get; set; }

  public bool IsActive { get; set; } = true;

  public bool? DoorType { get; set; }

  public bool IsAnalysable { get; set; } = false;

  public bool HasBeenAnalysed { get; set; } = false;

  public string Name { get; set; }

  public List<GameObject> ActiveDevices { get; set; }

  public InteractableSubZone InteractableData { get; set; }

  private void Start()
  {
    this._zoneCollider = ((Component) this).GetComponentInParent<BoxCollider>();
    if (Object.op_Equality((Object) this._zoneCollider, (Object) null))
    {
      Utils.Logger.LogError((object) "Realism Mod: No BoxCollider found in parent for SafeZone");
    }
    else
    {
      ((MonoBehaviour) this).StartCoroutine(this.SetUpAudio());
      this.CheckForDoors();
      this.Name = ((Object) this).name;
      this.ActiveDevices = new List<GameObject>();
    }
  }

  private IEnumerator SetUpAudio()
  {
    yield return (object) new WaitUntil((Func<bool>) (() => Plugin.RealismAudioController.ClipsAreReady));
    this.SetUpAndPlayMainAudio();
    this.SetUpDoorShutAudio();
    this.SetUpDoorOpenAudio();
  }

  private void SetUpAndPlayMainAudio()
  {
    this._mainAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._mainAudioSource.clip = Plugin.RealismAudioController.HazardZoneClips["labs-hvac.wav"];
    this._mainAudioSource.volume = 0.5f * GameWorldController.GetGameVolumeAsFactor();
    this._mainAudioSource.loop = true;
    this._mainAudioSource.playOnAwake = false;
    this._mainAudioSource.spatialBlend = 1f;
    this._mainAudioSource.minDistance = 3.5f;
    this._mainAudioSource.maxDistance = 15f;
    this._mainAudioSource.rolloffMode = (AudioRolloffMode) 0;
    this._mainAudioSource.Play();
  }

  private void SetUpDoorShutAudio()
  {
    this._doorShutAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._doorShutAudioSource.clip = Plugin.RealismAudioController.HazardZoneClips["door_shut.wav"];
    this._doorShutAudioSource.volume = 0.46f * GameWorldController.GetGameVolumeAsFactor();
    this._doorShutAudioSource.loop = false;
    this._doorShutAudioSource.playOnAwake = false;
    this._doorShutAudioSource.spatialBlend = 1f;
    this._doorShutAudioSource.minDistance = 3.5f;
    this._doorShutAudioSource.maxDistance = 15f;
    this._doorShutAudioSource.rolloffMode = (AudioRolloffMode) 0;
  }

  private void SetUpDoorOpenAudio()
  {
    this._doorOpenAudioSource = ((Component) this).gameObject.AddComponent<AudioSource>();
    this._doorOpenAudioSource.clip = Plugin.RealismAudioController.HazardZoneClips["door_open.wav"];
    this._doorOpenAudioSource.volume = 0.17f * GameWorldController.GetGameVolumeAsFactor();
    this._doorOpenAudioSource.loop = false;
    this._doorOpenAudioSource.playOnAwake = false;
    this._doorOpenAudioSource.spatialBlend = 1f;
    this._doorOpenAudioSource.minDistance = 3.5f;
    this._doorOpenAudioSource.maxDistance = 15f;
    this._doorOpenAudioSource.rolloffMode = (AudioRolloffMode) 0;
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

  private void CheckForDoors()
  {
    BoxCollider zoneCollider = this._zoneCollider;
    foreach (Component component1 in Physics.OverlapBox(Vector3.op_Addition(((Component) zoneCollider).transform.position, zoneCollider.center), Vector3.op_Division(zoneCollider.size, 2f), Quaternion.identity))
    {
      Door component2 = component1.GetComponent<Door>();
      if (Object.op_Inequality((Object) component2, (Object) null) && ((WorldInteractiveObject) component2).Operatable)
      {
        if (((WorldInteractiveObject) component2).KeyId == "5c1d0f4986f7744bb01837fa" || ((WorldInteractiveObject) component2).KeyId == "5c1d0c5f86f7744bb2683cf0")
          ((Object) component2).name = "automatic_door";
        this._doors.Add(component2);
        this._previousDoorStates.Add((WorldInteractiveObject) component2, ((WorldInteractiveObject) component2).DoorState);
      }
    }
  }

  private bool KeysMatch(Player player, string doorKey)
  {
    if (player.MovementContext.InteractionInfo.Result == null)
      return false;
    KeyComponent key = ((GClass3424) player.MovementContext.InteractionInfo.Result).Key;
    return doorKey == key.Template.KeyId;
  }

  private bool AnyDoorsOpen(WorldInteractiveObject activeWorldObject)
  {
    foreach (Door door in this._doors)
    {
      if (door != activeWorldObject && ((WorldInteractiveObject) door).DoorState == 4)
        return true;
    }
    return false;
  }

  private IEnumerator PlayDoorInteractionSound(
    WorldInteractiveObject door,
    EDoorState prevState,
    EDoorState currentState)
  {
    bool isOpening = prevState == 1 && currentState == 8 || prevState == 2 && currentState == 8;
    bool isClosing = prevState == 4 && currentState == 8;
    float time = 0.0f;
    float timeLimit = prevState != 1 || !(((Object) door).name == "automatic_door") ? (isOpening ? 0.75f : 1f) : 4f;
    while ((double) time < (double) timeLimit)
    {
      time += Time.deltaTime;
      yield return (object) null;
    }
    bool otherDoorsCurrentlyOpen = this.AnyDoorsOpen(door);
    if (!otherDoorsCurrentlyOpen & isOpening && (double) Mathf.Abs(door.CurrentAngle) > (double) Mathf.Abs(door.GetAngle((EDoorState) 2)))
    {
      this._doorOpenAudioSource.Play();
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.17f, 1f, this._doorOpenAudioSource));
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.0f, 0.25f, this._doorShutAudioSource));
      this.IsActive = false;
    }
    else if (!otherDoorsCurrentlyOpen & isClosing && (double) Mathf.Abs(door.CurrentAngle) < (double) Mathf.Abs(door.GetAngle((EDoorState) 4)))
    {
      this._doorShutAudioSource.Play();
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.46f, 1f, this._doorShutAudioSource));
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.0f, 0.25f, this._doorOpenAudioSource));
      this.IsActive = true;
    }
    if (!this.IsActive)
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.0f, 0.1f, this._mainAudioSource));
    if (this.IsActive)
      ((MonoBehaviour) this).StartCoroutine(this.AdjustVolume(0.5f, 0.1f, this._mainAudioSource));
  }

  private void CheckDoorState(WorldInteractiveObject door)
  {
    EDoorState previousDoorState = this._previousDoorStates[door];
    EDoorState doorState = door.DoorState;
    if (doorState != previousDoorState)
      ((MonoBehaviour) this).StartCoroutine(this.PlayDoorInteractionSound(door, previousDoorState, doorState));
    this._previousDoorStates[door] = door.DoorState;
  }

  private void CalculateSafeZoneDepth(Vector3 playerPosition)
  {
    Bounds bounds1 = ((Collider) this._zoneCollider).bounds;
    Vector3 extents = ((Bounds) ref bounds1).extents;
    Vector3 vector3 = playerPosition;
    Bounds bounds2 = ((Collider) this._zoneCollider).bounds;
    Vector3 center = ((Bounds) ref bounds2).center;
    this._distanceToCenter = Mathf.Clamp01(Vector3.Distance(vector3, center) / ((Vector3) ref extents).magnitude);
  }

  public virtual void TriggerEnter(Player player)
  {
    if (!Object.op_Inequality((Object) player, (Object) null))
      return;
    PlayerZoneBridge playerZoneBridge;
    ((Component) player).TryGetComponent<PlayerZoneBridge>(ref playerZoneBridge);
    if (Object.op_Equality((Object) playerZoneBridge, (Object) null))
      playerZoneBridge = ((Component) player).gameObject.AddComponent<PlayerZoneBridge>();
    if (Object.op_Equality((Object) playerZoneBridge._Player, (Object) null))
      playerZoneBridge._Player = player;
    ++playerZoneBridge.SafeZoneCount;
    if (this.BlocksNav)
      ++playerZoneBridge.ZonesThatBlockNavCount;
    playerZoneBridge.SafeZones.Add(((Object) this).name, this.IsActive);
    this._containedPlayers.Add(player, playerZoneBridge);
  }

  public virtual void TriggerExit(Player player)
  {
    if (!Object.op_Inequality((Object) player, (Object) null))
      return;
    PlayerZoneBridge containedPlayer = this._containedPlayers[player];
    --containedPlayer.SafeZoneCount;
    if (this.BlocksNav)
      --containedPlayer.ZonesThatBlockNavCount;
    containedPlayer.SafeZones.Remove(((Object) this).name);
    this._containedPlayers.Remove(player);
  }

  private void Update()
  {
    this._tick += Time.deltaTime;
    if ((double) this._tick < 0.25)
      return;
    List<Player> playerList = new List<Player>();
    foreach (KeyValuePair<Player, PlayerZoneBridge> containedPlayer in this._containedPlayers)
    {
      Player key = containedPlayer.Key;
      PlayerZoneBridge playerZoneBridge = containedPlayer.Value;
      if (Object.op_Equality((Object) key, (Object) null) || Object.op_Equality((Object) playerZoneBridge, (Object) null))
      {
        playerList.Add(key);
      }
      else
      {
        this.CalculateSafeZoneDepth(key.Position);
        playerZoneBridge.SafeZones[((Object) this).name] = this.IsActive && (double) this._distanceToCenter <= 0.68999999761581421;
      }
    }
    foreach (Player key in playerList)
      this._containedPlayers.Remove(key);
    foreach (WorldInteractiveObject door in this._doors)
      this.CheckDoorState(door);
    this._tick = 0.0f;
  }
}
