// Decompiled with JetBrains decompiler
// Type: RealismMod.GasZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class GasZone : TriggerWithId, IZone
{
  private Dictionary<Player, PlayerZoneBridge> _containedPlayers = new Dictionary<Player, PlayerZoneBridge>();
  private Collider _zoneCollider;
  private bool _isSphere = false;
  private float _tick = 0.0f;
  private float _maxDistance = 0.0f;
  private FogScript[] _fogScript;
  private float _strengthModi = 1f;

  public EZoneType ZoneType { get; } = EZoneType.Gas;

  public float ZoneStrength { get; set; } = 1f;

  public float ZoneStrengthTargetModi { get; set; } = 1f;

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
      Utils.Logger.LogError((object) "Realism Mod: No BoxCollider found in parent for GasZone");
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
    this._fogScript = ((Component) this).GetComponentsInChildren<FogScript>();
    this.ModifyVisualEffects(true);
  }

  private void ModifyVisualEffects(bool onInit)
  {
    foreach (FogScript fogScript in this._fogScript)
    {
      if (onInit)
      {
        float num = Mathf.Pow(1f + this.CalculateGasStrengthBox(Vector3.zero, true), 0.1f);
        fogScript.ParticleRate *= num;
        fogScript.OpacityModi *= num;
      }
      else
        fogScript.DynamicOpacityModiTarget = this.ZoneStrengthTargetModi;
    }
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
    ++playerZoneBridge.GasZoneCount;
    if (this.BlocksNav)
      ++playerZoneBridge.ZonesThatBlockNavCount;
    playerZoneBridge.GasRates.Add(((Object) this).name, 0.0f);
    this._containedPlayers.Add(player, playerZoneBridge);
  }

  public virtual void TriggerExit(Player player)
  {
    if (!Object.op_Inequality((Object) player, (Object) null))
      return;
    PlayerZoneBridge containedPlayer = this._containedPlayers[player];
    --containedPlayer.GasZoneCount;
    if (this.BlocksNav)
      --containedPlayer.ZonesThatBlockNavCount;
    containedPlayer.GasRates.Remove(((Object) this).name);
    this._containedPlayers.Remove(player);
  }

  private void Update()
  {
    this._tick += Time.deltaTime;
    if ((double) this._tick >= 1.0 / 1000.0)
    {
      List<Player> playerList = new List<Player>();
      foreach (KeyValuePair<Player, PlayerZoneBridge> containedPlayer in this._containedPlayers)
      {
        Player key = containedPlayer.Key;
        PlayerZoneBridge playerZoneBridge = containedPlayer.Value;
        if (Object.op_Equality((Object) key, (Object) null) || Object.op_Equality((Object) playerZoneBridge, (Object) null))
        {
          playerList.Add(key);
          return;
        }
        float num = this._isSphere ? this.CalculateGasStrengthSphere(((Component) key).gameObject.transform.position) : this.CalculateGasStrengthBox(((Component) key).gameObject.transform.position);
        playerZoneBridge.GasRates[((Object) this).name] = Mathf.Max(num * this._strengthModi, 0.0f);
      }
      foreach (Player key in playerList)
        this._containedPlayers.Remove(key);
      this.ModifyVisualEffects(false);
      this._tick = 0.0f;
    }
    this._strengthModi = Mathf.Lerp(this._strengthModi, this.ZoneStrengthTargetModi, 0.05f * Time.deltaTime);
  }

  private float CalculateGasStrengthBox(Vector3 playerPosition, bool getStaticValue = false)
  {
    if ((double) this._strengthModi == 0.0)
      return 0.0f;
    if (!this.UsesDistanceFalloff)
      return (float) ((double) this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? (double) PluginConfig.test10.Value : 1.0) / 1000.0);
    double num;
    if (!getStaticValue)
    {
      Vector3 vector3 = playerPosition;
      Bounds bounds = this._zoneCollider.bounds;
      Vector3 center = ((Bounds) ref bounds).center;
      num = (double) Vector3.Distance(vector3, center);
    }
    else
      num = 1.0;
    return Mathf.Clamp(this._maxDistance - (float) num, 0.0f, this._maxDistance) / (this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? PluginConfig.test10.Value : 1f));
  }

  private float CalculateGasStrengthSphere(Vector3 playerPosition)
  {
    if ((double) this._strengthModi == 0.0)
      return 0.0f;
    if (!this.UsesDistanceFalloff)
      return (float) ((double) this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? (double) PluginConfig.test10.Value : 1.0) / 1000.0);
    Vector3 vector3 = playerPosition;
    Bounds bounds = this._zoneCollider.bounds;
    Vector3 center = ((Bounds) ref bounds).center;
    float num = Vector3.Distance(vector3, center);
    bounds = this._zoneCollider.bounds;
    Vector3 extents = ((Bounds) ref bounds).extents;
    float magnitude = ((Vector3) ref extents).magnitude;
    return Mathf.Clamp(this._maxDistance - Mathf.Max(0.0f, num - magnitude), 0.0f, this._maxDistance) / (this.ZoneStrength * (PluginConfig.ZoneDebug.Value ? PluginConfig.test10.Value : 1f));
  }
}
