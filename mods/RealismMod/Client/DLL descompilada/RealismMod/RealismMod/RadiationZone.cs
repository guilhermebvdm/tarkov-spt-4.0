// Decompiled with JetBrains decompiler
// Type: RealismMod.RadiationZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RadiationZone : TriggerWithId, IZone
{
  private Dictionary<Player, PlayerZoneBridge> _containedPlayers = new Dictionary<Player, PlayerZoneBridge>();
  private Collider _zoneCollider;
  private bool _isSphere = false;
  private float _tick = 0.0f;
  private float _maxDistance = 0.0f;

  public EZoneType ZoneType { get; } = EZoneType.Radiation;

  public float ZoneStrength { get; set; } = 1f;

  public bool BlocksNav { get; set; }

  public bool UsesDistanceFalloff { get; set; }

  public bool IsAnalysable { get; set; } = false;

  public bool HasBeenAnalysed { get; set; } = false;

  public string Name { get; set; }

  public List<GameObject> ActiveDevices { get; set; }

  public InteractableSubZone InteractableData { get; set; }

  private void Start()
  {
    this._zoneCollider = ((Component) this).GetComponentInParent<Collider>();
    if (Object.op_Equality((Object) this._zoneCollider, (Object) null))
      Utils.Logger.LogError((object) "Realism Mod: No BoxCollider found in parent for RadiationZone");
    SphereCollider zoneCollider = this._zoneCollider as SphereCollider;
    if (Object.op_Inequality((Object) zoneCollider, (Object) null))
    {
      this._isSphere = true;
      this._maxDistance = zoneCollider.radius;
    }
    else
    {
      Vector3 size = (this._zoneCollider as BoxCollider).size;
      this._maxDistance = ((Vector3) ref size).magnitude / 2f;
      HazardPlayerSpawnManager.Register(this._zoneCollider);
    }
    this.Name = ((Object) this).name;
    this.ActiveDevices = new List<GameObject>();
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
    ++playerZoneBridge.RadZoneCount;
    if (this.BlocksNav)
      ++playerZoneBridge.ZonesThatBlockNavCount;
    playerZoneBridge.RadRates.Add(((Object) this).name, 0.0f);
    this._containedPlayers.Add(player, playerZoneBridge);
  }

  public virtual void TriggerExit(Player player)
  {
    if (!Object.op_Inequality((Object) player, (Object) null))
      return;
    PlayerZoneBridge containedPlayer = this._containedPlayers[player];
    --containedPlayer.RadZoneCount;
    if (this.BlocksNav)
      --containedPlayer.ZonesThatBlockNavCount;
    containedPlayer.RadRates.Remove(((Object) this).name);
    this._containedPlayers.Remove(player);
  }

  private void Update()
  {
    this._tick += Time.deltaTime;
    if ((double) this._tick < 1.0 / 1000.0)
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
        float num = this._isSphere ? this.CalculateRadStrengthSphere(((Component) key).gameObject.transform.position) : this.CalculateRadStrengthBox(((Component) key).gameObject.transform.position);
        playerZoneBridge.RadRates[((Object) this).name] = Mathf.Max(num, 0.0f);
      }
    }
    foreach (Player key in playerList)
      this._containedPlayers.Remove(key);
    this._tick = 0.0f;
  }

  private float CalculateRadStrengthBox(Vector3 playerPosition)
  {
    if (!this.UsesDistanceFalloff)
      return (float) ((double) this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? (double) PluginConfig.test10.Value : 1.0) / 1000.0);
    Vector3 vector3 = playerPosition;
    Bounds bounds = this._zoneCollider.bounds;
    Vector3 center = ((Bounds) ref bounds).center;
    return Mathf.Clamp(this._maxDistance - Vector3.Distance(vector3, center), 0.0f, this._maxDistance) / (this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? PluginConfig.test10.Value : 1f));
  }

  private float CalculateRadStrengthSphere(Vector3 playerPosition)
  {
    if (!this.UsesDistanceFalloff)
      return (float) ((double) this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? (double) PluginConfig.test10.Value : 1.0) / 1000.0);
    Vector3 vector3 = playerPosition;
    Bounds bounds = this._zoneCollider.bounds;
    Vector3 center = ((Bounds) ref bounds).center;
    float num = Vector3.Distance(vector3, center);
    double radius = (double) (this._zoneCollider as SphereCollider).radius;
    Vector3 localScale = ((Component) this).transform.localScale;
    double magnitude = (double) ((Vector3) ref localScale).magnitude;
    return Mathf.Max(0.0f, (float) (radius * magnitude) - num) / (this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? PluginConfig.test10.Value : 1f));
  }
}
