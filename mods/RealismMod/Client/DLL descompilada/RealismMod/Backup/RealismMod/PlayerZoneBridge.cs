// Decompiled with JetBrains decompiler
// Type: RealismMod.PlayerZoneBridge
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class PlayerZoneBridge : MonoBehaviour
{
  private const float TIME_CHECK_INTERVAL = 10f;
  private const float SPAWN_TIME = 40f;
  private const float BOT_SPAWN_TIME = 20f;
  private const float TIME_MOVEMENT_THRESHOLD = 20f;
  private const float BOT_TIME_MOVEMENT_THRESHOLD = 10f;
  public Dictionary<string, bool> SafeZones = new Dictionary<string, bool>();
  public Dictionary<string, float> GasRates = new Dictionary<string, float>();
  public Dictionary<string, float> RadRates = new Dictionary<string, float>();
  private bool _isProtectedFromSafeZone = false;
  private string[] targetTags = new string[4]
  {
    "Radiation",
    "Gas",
    "RadAssets",
    "GasAssets"
  };
  private bool _isInHazardZone = false;
  private float _safeZoneCheckTimer = 0.0f;
  private float _botTimer = 0.0f;
  private float _timeActive = 0.0f;
  private bool _checkedSpawn = false;

  public Player _Player { get; set; }

  public bool IsBot { get; private set; } = false;

  public bool SpawnedInZone { get; private set; } = false;

  public int ZonesThatBlockNavCount { get; set; } = 0;

  public int RadZoneCount { get; set; } = 0;

  public int GasZoneCount { get; set; } = 0;

  public int SafeZoneCount { get; set; } = 0;

  public bool IsProtectedFromSafeZone => this._isProtectedFromSafeZone;

  public float TotalGasRate
  {
    get
    {
      if (this.IsProtectedFromSafeZone)
        return 0.0f;
      float totalGasRate = 0.0f;
      foreach (KeyValuePair<string, float> gasRate in this.GasRates)
        totalGasRate += gasRate.Value;
      return totalGasRate;
    }
  }

  public float TotalRadRate
  {
    get
    {
      if (this.IsProtectedFromSafeZone)
        return 0.0f;
      float totalRadRate = 0.0f;
      foreach (KeyValuePair<string, float> radRate in this.RadRates)
        totalRadRate += radRate.Value;
      return totalRadRate;
    }
  }

  private float SpawnTimeTheshold => this.IsBot ? 20f : 40f;

  private float MoveTimeTheshold => this.IsBot ? 10f : 20f;

  private void Start() => this.StartCoroutine(this.IsInHazardZone());

  private void Update()
  {
    this._timeActive += Time.deltaTime;
    this.BotCheck();
    this.CheckIfInHazard();
    this.CheckSafeZones();
  }

  private IEnumerator IsInHazardZone()
  {
    while (!this._isInHazardZone && !this._checkedSpawn)
    {
      BoxCollider zone = HazardPlayerSpawnManager.GetZoneContaining(this._Player.Transform.position);
      if (Object.op_Inequality((Object) zone, (Object) null))
        this._isInHazardZone = true;
      if (PluginConfig.ZoneDebug.Value)
        Utils.Logger.LogWarning((object) $"player spawned at hazard zone? {Object.op_Inequality((Object) zone, (Object) null)}");
      yield return (object) new WaitForSeconds(1f);
      zone = (BoxCollider) null;
    }
  }

  private bool BotHasGasmask()
  {
    if (this._Player?.Inventory == null || this._Player?.Equipment == null)
      return true;
    Item containedItem = this._Player.Inventory?.Equipment?.GetSlot((EquipmentSlot) 10)?.ContainedItem;
    return containedItem != null && TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(containedItem.TemplateId)).IsGasMask;
  }

  private void HandleBotGas(bool hasGasmask)
  {
    if (this.GasZoneCount <= 0 && !GameWorldController.DoMapGasEvent || this.IsProtectedFromSafeZone)
      return;
    float num1 = this.TotalGasRate + GameWorldController.CurrentGasEventStrengthBot * (this._Player.Environment == 1 ? 0.5f : 1f);
    if (!hasGasmask && (double) this.TotalGasRate > 0.05000000074505806)
    {
      double num2 = (double) this._Player.ActiveHealthController.ApplyDamage((EBodyPart) 1, (float) ((double) this.TotalGasRate * 10.0 * 0.75), GClass2855.PoisonDamage);
      if ((double) ((GClass2814<ActiveHealthController.GClass2813>) this._Player.ActiveHealthController).GetBodyPartHealth((EBodyPart) 1, false).Current <= 115.0)
        this._Player.Speaker.Play((EPhraseTrigger) 29, (ETagStatus) 8194, true, new int?());
    }
  }

  private void HandleBotRads(bool hasGasmask)
  {
    if (this.RadZoneCount <= 0 && !GameWorldController.DoMapRads || this.IsProtectedFromSafeZone)
      return;
    bool flag = this._Player.Environment == 0;
    float num1 = GameWorldController.DidExplosionClientSide & flag ? 2f : 1f;
    float num2 = (this.TotalRadRate + (GameWorldController.DoMapRads ? (float) (0.05000000074505806 + (flag ? (double) Plugin.RealismWeatherComponent.TargetRain * 0.15000000596046448 : 0.0)) : 0.0f)) * num1;
    float num3 = hasGasmask ? num2 * 0.5f : num2;
    if ((double) num3 > 0.40000000596046448 || GameWorldController.DidExplosionClientSide || GameWorldController.DoMapRads && !hasGasmask)
    {
      double num4 = (double) this._Player.ActiveHealthController.ApplyDamage((EBodyPart) 1, num3 * 10f, GClass2855.RadiationDamage);
      if (GameWorldController.DidExplosionClientSide && this._Player.Environment == 0)
        this._Player.Speaker.Play((EPhraseTrigger) 29, (ETagStatus) 8194, true, new int?());
    }
  }

  private void CheckIfInHazard()
  {
    if (this._checkedSpawn)
      return;
    if (this._isInHazardZone || (this.GasZoneCount > 0 || this.RadZoneCount > 0) && !this.IsProtectedFromSafeZone)
    {
      this.SpawnedInZone = true;
      this.MoveEntityToSafeLocation();
    }
    bool flag = this._Player.IsSprintEnabled || (this._Player.ProceduralWeaponAnimation.Mask & 2) > 0;
    if (this.SpawnedInZone || (double) this._timeActive >= (double) this.SpawnTimeTheshold || (double) this._timeActive >= (double) this.MoveTimeTheshold & flag)
      this._checkedSpawn = true;
  }

  private void MoveEntityToSafeLocation()
  {
    Vector3 position = this._Player.Transform.position;
    this._Player.Transform.position = ZoneSpawner.TryGetSafeSpawnPoint(this._Player, this.IsBot, this.ZonesThatBlockNavCount > 0, this.RadZoneCount > 0);
    if (!PluginConfig.ZoneDebug.Value)
      return;
    Utils.Logger.LogWarning((object) $"Realism Mod: Spawned in Hazard, moved to: {this._Player.Transform.position}, Original Position: {position},  Was Bot? {this.IsBot}, time remaining {this._timeActive}, ProfileId {this._Player.ProfileId}");
  }

  private void CheckSafeZones()
  {
    this._safeZoneCheckTimer += Time.deltaTime;
    if ((double) this._safeZoneCheckTimer < 1.0)
      return;
    this._isProtectedFromSafeZone = false;
    foreach (KeyValuePair<string, bool> safeZone in this.SafeZones)
    {
      if (safeZone.Value)
        this._isProtectedFromSafeZone = true;
    }
    this._safeZoneCheckTimer = 0.0f;
  }

  private void BotCheck()
  {
    this._botTimer += Time.deltaTime;
    this.IsBot = (this._Player.IsAI || Object.op_Inequality((Object) this._Player?.AIData?.BotOwner, (Object) null)) && !this._Player.IsYourPlayer;
    if ((double) this._botTimer < 10.0 || !this.IsBot || !Object.op_Inequality((Object) this._Player, (Object) null) || this._Player?.ActiveHealthController == null || this._Player.AIData.BotOwner.IsDead || !this._Player.HealthController.IsAlive)
      return;
    bool hasGasmask = this.BotHasGasmask();
    this.HandleBotGas(hasGasmask);
    this.HandleBotRads(hasGasmask);
    this._botTimer = 0.0f;
  }
}
