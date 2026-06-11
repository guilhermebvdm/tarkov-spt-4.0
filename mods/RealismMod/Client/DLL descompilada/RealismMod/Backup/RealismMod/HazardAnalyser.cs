// Decompiled with JetBrains decompiler
// Type: RealismMod.HazardAnalyser
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class HazardAnalyser : MonoBehaviour
{
  public List<ActionsTypesClass> Actions = new List<ActionsTypesClass>();
  private AudioSource _audioSource;
  private Vector3 _position;
  private Quaternion _rotation;
  private List<IZone> _intersectingZones = new List<IZone>();
  private float _stallChance = 90f;
  private bool _stalledPreviously = false;
  public string _instanceId = "";
  private bool _deactivated = false;
  private float _placementTimer = 0.0f;
  private bool _placedItem = false;
  private float _gameVolume = 1f;

  public bool CanTurnOn { get; private set; } = true;

  public LootItem _LootItem { get; set; }

  public Player _Player { get; set; } = (Player) null;

  public IPlayer _IPlayer { get; set; } = (IPlayer) null;

  public EZoneType TargetZoneType { get; set; }

  public IZone TargetZone { get; private set; } = (IZone) null;

  public AudioClip AudioClips { get; set; }

  private void SetUpTransforms()
  {
    Quaternion rotation = ((Component) this._Player).gameObject.transform.rotation;
    Vector3 eulerAngles = ((Quaternion) ref rotation).eulerAngles;
    eulerAngles.x = -90f;
    eulerAngles.y = 0.0f;
    this._rotation = Quaternion.Euler(new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z));
  }

  private void SetUpActions()
  {
    this.Actions.AddRange((IEnumerable<ActionsTypesClass>) new List<ActionsTypesClass>()
    {
      new ActionsTypesClass()
      {
        Name = "Turn On",
        Action = new Action(this.TurnOn)
      }
    });
  }

  private AudioSource SetUpAudio(string clip, GameObject go)
  {
    this._gameVolume = GameWorldController.GetGameVolumeAsFactor();
    AudioSource audioSource = go.AddComponent<AudioSource>();
    audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips[clip];
    audioSource.volume = 1f * this._gameVolume;
    audioSource.loop = false;
    audioSource.playOnAwake = false;
    audioSource.spatialBlend = 1f;
    audioSource.minDistance = 0.7f;
    audioSource.maxDistance = 35f;
    audioSource.rolloffMode = (AudioRolloffMode) 0;
    return audioSource;
  }

  private void GetTargetZone(IZone zone, EZoneType[] targetZones)
  {
    if (this.TargetZone != null || zone == null)
      return;
    bool flag1 = this.TargetZoneType == EZoneType.Radiation && (zone.ZoneType == EZoneType.RadAssets || zone.ZoneType == EZoneType.Radiation);
    bool flag2 = this.TargetZoneType == EZoneType.Gas && (zone.ZoneType == EZoneType.Gas || zone.ZoneType == EZoneType.GasAssets);
    if (!zone.IsAnalysable || !(flag1 | flag2))
      return;
    this.TargetZone = zone;
  }

  public bool CheckIfZoneTypeMatches()
  {
    return !this._intersectingZones.Any<IZone>((Func<IZone, bool>) (z => z.ZoneType == EZoneType.SafeZone)) && this.TargetZone != null;
  }

  private void AddSelfToDevicesList()
  {
    if (this.TargetZone == null)
      return;
    this.TargetZone.ActiveDevices.Add(((Component) this).gameObject);
  }

  private void DeleteSelfFromDevicesList()
  {
    if (this.TargetZone == null)
      return;
    this.TargetZone.ActiveDevices.Remove(((Component) this).gameObject);
  }

  public bool ZoneAlreadyHasDevice()
  {
    if (this.TargetZone == null)
      return false;
    this.TargetZone.ActiveDevices.RemoveAll((Predicate<GameObject>) (d => Object.op_Equality((Object) d, (Object) null) || !d.activeSelf || !d.gameObject.activeSelf));
    List<HazardAnalyser> hazardAnalyserList = new List<HazardAnalyser>();
    foreach (GameObject activeDevice in this.TargetZone.ActiveDevices)
    {
      HazardAnalyser hazardAnalyser;
      if (activeDevice.gameObject.TryGetComponent<HazardAnalyser>(ref hazardAnalyser))
        hazardAnalyserList.Add(hazardAnalyser);
    }
    foreach (HazardAnalyser hazardAnalyser in hazardAnalyserList)
    {
      if (hazardAnalyser._instanceId != this._instanceId)
        return true;
    }
    return false;
  }

  private void DeactivateZone()
  {
    if (this.TargetZone == null)
      return;
    this.TargetZone.HasBeenAnalysed = true;
  }

  protected void RemovePhysicsInteractions(GameObject go)
  {
    Rigidbody component1 = go.GetComponent<Rigidbody>();
    Collider component2 = go.GetComponent<Collider>();
    if (Object.op_Inequality((Object) component1, (Object) null))
    {
      component1.useGravity = false;
      component1.isKinematic = true;
    }
    if (!Object.op_Inequality((Object) component2, (Object) null))
      return;
    component2.enabled = false;
  }

  private void Start()
  {
    this.SetUpTransforms();
    this.SetUpActions();
    this._audioSource = this.SetUpAudio("switch_off.wav", ((Component) this).gameObject);
    this._instanceId = MongoID.op_Implicit(MongoID.Generate(true));
    this._stallChance = this.TargetZoneType == EZoneType.Radiation ? 100f : 90f;
  }

  private void OnTriggerEnter(Collider other)
  {
    EZoneType[] ezoneTypeArray;
    if (this.TargetZoneType != EZoneType.Gas)
      ezoneTypeArray = new EZoneType[2]
      {
        EZoneType.Radiation,
        EZoneType.RadAssets
      };
    else
      ezoneTypeArray = new EZoneType[2]
      {
        EZoneType.Gas,
        EZoneType.GasAssets
      };
    EZoneType[] targetZones = ezoneTypeArray;
    IZone zone;
    if (!((Component) other).gameObject.TryGetComponent<IZone>(ref zone))
      return;
    this._intersectingZones.Add(zone);
    this.GetTargetZone(zone, targetZones);
  }

  private void Update()
  {
    this._placementTimer += Time.deltaTime;
    if (Object.op_Equality((Object) ((Component) this).gameObject, (Object) null) || this._deactivated)
      return;
    RaycastHit raycastHit;
    if (!this._placedItem && EFTPhysicsClass.Raycast(new Ray(Vector3.op_Addition(this._Player.PlayerBones.LootRaycastOrigin.position, Vector3.op_Division(this._Player.PlayerBones.LootRaycastOrigin.forward, 2f)), this._Player.PlayerBones.LootRaycastOrigin.forward), ref raycastHit, 2f, LayerMask.op_Implicit(LayerMaskClass.HighPolyWithTerrainMask)) && (double) Mathf.Abs(((RaycastHit) ref raycastHit).point.y - this._Player.Transform.position.y) <= 0.079999998211860657)
    {
      this._position = ((RaycastHit) ref raycastHit).point;
      this._position.y += 0.055f;
      ((Component) this).gameObject.transform.position = this._position;
      ((Component) this).gameObject.transform.rotation = this._rotation;
      this._placedItem = true;
      this.RemovePhysicsInteractions(((Component) this).gameObject);
    }
    if (this._placedItem || (double) this._placementTimer < 0.10000000149011612)
      return;
    this._position = ((Component) this._Player).gameObject.transform.position;
    this._position.y += 0.055f;
    ((Component) this).gameObject.transform.position = this._position;
    ((Component) this).gameObject.transform.rotation = this._rotation;
    this._placedItem = true;
    this.RemovePhysicsInteractions(((Component) this).gameObject);
  }

  private void PlaySoundForAI()
  {
    if (!Singleton<BotEventHandler>.Instantiated)
      return;
    Singleton<BotEventHandler>.Instance.PlaySound(this._IPlayer, ((Component) this).transform.position, 40f, (AISoundType) 0);
  }

  private void TurnOn()
  {
    if (!this.CanTurnOn)
      return;
    this.StartCoroutine(this.DoLogic());
  }

  private IEnumerator DoLogic()
  {
    this.PlaySoundForAI();
    float time = 0.0f;
    if (this.CheckIfZoneTypeMatches())
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["start_success.wav"];
      this._audioSource.Play();
      this.CanTurnOn = false;
      this.AddSelfToDevicesList();
      float clipLength = this._audioSource.clip.length;
      while ((double) time <= (double) clipLength - 0.15000000596046448)
      {
        time += Time.deltaTime;
        yield return (object) null;
      }
      this.PlaySoundForAI();
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["analyser_loop.wav"];
      this._audioSource.loop = true;
      this._audioSource.Play();
      time = 0.0f;
      clipLength = this._audioSource.clip.length;
      int loops = Random.Range(1, 4);
      bool shouldStall = !this._stalledPreviously && (double) Random.Range(1, 100) >= (double) this._stallChance;
      if (shouldStall)
        loops /= 2;
      while ((double) time <= (double) clipLength * (double) loops)
      {
        time += Time.deltaTime;
        yield return (object) null;
      }
      this.PlaySoundForAI();
      if (shouldStall)
      {
        this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["stalling.wav"];
        this._audioSource.Play();
        this.CanTurnOn = true;
        this._stalledPreviously = true;
      }
      else
      {
        this._audioSource.loop = false;
        this.ReplaceItem();
        this.DeactivateZone();
      }
    }
    else
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["failed_start.wav"];
      this._audioSource.Play();
      this.CanTurnOn = true;
    }
  }

  private void ReplaceItem()
  {
    this.DeleteSelfFromDevicesList();
    string str = this.TargetZoneType == EZoneType.Gas ? "670120df4f0c4c37e6be90ae" : "670120ce354987453daf3d0c";
    LootItem lootItem = Singleton<GameWorld>.Instance.SetupItem(Singleton<ItemFactoryClass>.Instance.CreateItem(MongoID.op_Implicit(MongoID.Generate(true)), str, (GClass825) null), this._IPlayer, this._position, this._rotation);
    this.RemovePhysicsInteractions(((Component) lootItem).gameObject);
    this.SetUpAudio("success_end.wav", ((Component) lootItem).gameObject).Play();
    this._deactivated = true;
    Object.Destroy((Object) ((Component) this).gameObject, 0.5f);
    if (!PluginConfig.ZoneDebug.Value)
      return;
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) primitive).name = "deviceStartPos";
    primitive.transform.localScale = new Vector3(0.5f, 0.5f, 100f);
    primitive.transform.position = ((Component) lootItem).transform.position;
    primitive.transform.rotation = ((Component) lootItem).transform.rotation;
    primitive.GetComponent<Renderer>().material.color = Color.red;
    Object.Destroy((Object) primitive.GetComponent<Collider>());
  }

  private void OnDisable() => this.DeleteSelfFromDevicesList();
}
