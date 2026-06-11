// Decompiled with JetBrains decompiler
// Type: RealismMod.Transmitter
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

public class Transmitter : MonoBehaviour
{
  public List<ActionsTypesClass> Actions = new List<ActionsTypesClass>();
  protected AudioSource _audioSource;
  protected Vector3 _position;
  protected Quaternion _rotation;
  protected List<IZone> _intersectingZones = new List<IZone>();
  public string _instanceId = "";
  protected float _placementTimer = 0.0f;
  protected bool _placedItem = false;
  protected float _gameVolume = 1f;

  public bool CanTurnOn { get; protected set; } = true;

  public LootItem _LootItem { get; set; }

  public Player _Player { get; set; } = (Player) null;

  public IPlayer _IPlayer { get; set; } = (IPlayer) null;

  public string[] TargetQuestZones { get; set; }

  public EZoneType TargetZoneType { get; private set; } = EZoneType.Quest;

  public IZone TargetZone { get; private set; } = (IZone) null;

  public AudioClip AudioClips { get; set; }

  public string QuestTrigger { get; set; }

  protected void SetUpTransforms()
  {
    Quaternion rotation = ((Component) this._Player).gameObject.transform.rotation;
    Vector3 eulerAngles = ((Quaternion) ref rotation).eulerAngles;
    eulerAngles.x = -90f;
    eulerAngles.y = 0.0f;
    this._rotation = Quaternion.Euler(new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z));
  }

  protected AudioSource SetUpAudio(string clip, GameObject go)
  {
    this._gameVolume = GameWorldController.GetGameVolumeAsFactor();
    AudioSource audioSource = go.AddComponent<AudioSource>();
    audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips[clip];
    audioSource.volume = 1.05f * this._gameVolume;
    audioSource.loop = false;
    audioSource.playOnAwake = false;
    audioSource.spatialBlend = 1f;
    audioSource.minDistance = 0.8f;
    audioSource.maxDistance = 35f;
    audioSource.rolloffMode = (AudioRolloffMode) 0;
    return audioSource;
  }

  protected void PlaySoundForAI()
  {
    if (!Singleton<BotEventHandler>.Instantiated)
      return;
    Singleton<BotEventHandler>.Instance.PlaySound(this._IPlayer, ((Component) this).transform.position, 40f, (AISoundType) 0);
  }

  protected void AddSelfToDevicesList()
  {
    if (this.TargetZone == null)
      return;
    this.TargetZone.ActiveDevices.Add(((Component) this).gameObject);
  }

  protected void DeleteSelfFromDevicesList()
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
    List<Transmitter> transmitterList = new List<Transmitter>();
    foreach (GameObject activeDevice in this.TargetZone.ActiveDevices)
    {
      Transmitter transmitter;
      if (activeDevice.gameObject.TryGetComponent<Transmitter>(ref transmitter))
        transmitterList.Add(transmitter);
    }
    foreach (Transmitter transmitter in transmitterList)
    {
      if (transmitter._instanceId != this._instanceId)
        return true;
    }
    return false;
  }

  protected void DeactivateZone()
  {
    if (this.TargetZone == null)
      return;
    this.TargetZone.HasBeenAnalysed = true;
  }

  protected void GetTargetZone(IZone zone, EZoneType[] targetZones)
  {
    if (this.TargetZone != null || zone == null)
      return;
    bool flag = this.TargetZoneType == EZoneType.Quest && zone.ZoneType == EZoneType.Quest;
    if (!(zone.IsAnalysable & flag))
      return;
    this.TargetZone = zone;
  }

  private void OnDisable() => this.DeleteSelfFromDevicesList();

  protected void SetUpActions()
  {
    this.Actions.AddRange((IEnumerable<ActionsTypesClass>) new List<ActionsTypesClass>()
    {
      new ActionsTypesClass()
      {
        Name = "Activate",
        Action = new Action(this.Activate)
      }
    });
  }

  protected bool IsInRightLocation()
  {
    foreach (IZone intersectingZone in this._intersectingZones)
    {
      QuestZone questZone;
      if (Object.op_Inequality((Object) (questZone = intersectingZone as QuestZone), (Object) null))
      {
        ((IEnumerable<string>) this.TargetQuestZones).Contains<string>(((Object) questZone).name);
        return true;
      }
    }
    return false;
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

  protected void Start()
  {
    this.RemovePhysicsInteractions(((Component) this).gameObject);
    this.SetUpTransforms();
    this.SetUpActions();
    this._instanceId = MongoID.op_Implicit(MongoID.Generate(true));
    this._audioSource = this.SetUpAudio("switch_off.wav", ((Component) this).gameObject);
  }

  protected void Update()
  {
    this._placementTimer += Time.deltaTime;
    if (Object.op_Equality((Object) ((Component) this).gameObject, (Object) null))
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

  protected void OnTriggerEnter(Collider other)
  {
    EZoneType[] ezoneTypeArray;
    if (this.TargetZoneType != EZoneType.Quest)
      ezoneTypeArray = new EZoneType[1]{ EZoneType.Quest };
    else
      ezoneTypeArray = new EZoneType[1]{ EZoneType.Quest };
    EZoneType[] targetZones = ezoneTypeArray;
    IZone zone;
    if (!((Component) other).gameObject.TryGetComponent<IZone>(ref zone))
      return;
    this._intersectingZones.Add(zone);
    this.GetTargetZone(zone, targetZones);
  }

  protected void Activate()
  {
    if (!this.CanTurnOn)
      return;
    this.StartCoroutine(this.DoLogic());
  }

  protected virtual IEnumerator DoLogic()
  {
    float time = 0.0f;
    if (this.IsInRightLocation())
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["numbers.wav"];
      this._audioSource.Play();
      this.AddSelfToDevicesList();
      this.CanTurnOn = false;
      float clipLength = this._audioSource.clip.length;
      while ((double) time <= (double) clipLength)
      {
        this.PlaySoundForAI();
        time += Time.deltaTime;
        yield return (object) null;
      }
      this.SpawnQuestTrigger();
      this.DeactivateZone();
      this.DeleteSelfFromDevicesList();
    }
    else
    {
      this._audioSource.clip = Plugin.RealismAudioController.DeviceAudioClips["switch_off.wav"];
      this._audioSource.Play();
      this.CanTurnOn = true;
    }
  }

  protected void SpawnQuestTrigger()
  {
    HazardGroup hazardLocation = ZoneData.QuestZoneLocations.Shoreline.FirstOrDefault<HazardGroup>((Func<HazardGroup, bool>) (hl => hl.Zones.Any<Zone>((Func<Zone, bool>) (z => z.Name == this.QuestTrigger))));
    hazardLocation.IsTriggered = false;
    ZoneSpawner.CreateZone<QuestZone>(hazardLocation, EZoneType.Quest);
  }
}
