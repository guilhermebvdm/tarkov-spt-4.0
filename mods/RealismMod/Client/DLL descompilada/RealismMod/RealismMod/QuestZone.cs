// Decompiled with JetBrains decompiler
// Type: RealismMod.QuestZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class QuestZone : TriggerWithId, IZone
{
  private Dictionary<Player, PlayerZoneBridge> _containedPlayers = new Dictionary<Player, PlayerZoneBridge>();
  private BoxCollider _zoneCollider;
  private float _tick = 0.0f;

  public EZoneType ZoneType { get; } = EZoneType.Quest;

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
    this._zoneCollider = ((Component) this).GetComponentInParent<BoxCollider>();
    if (Object.op_Equality((Object) this._zoneCollider, (Object) null))
      Utils.Logger.LogError((object) "Realism Mod: No BoxCollider found in parent for RadiationZone");
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
    this._containedPlayers.Add(player, playerZoneBridge);
  }

  public virtual void TriggerExit(Player player)
  {
    if (!Object.op_Inequality((Object) player, (Object) null))
      return;
    PlayerZoneBridge containedPlayer = this._containedPlayers[player];
    this._containedPlayers.Remove(player);
  }

  private void Update()
  {
    this._tick += Time.deltaTime;
    if ((double) this._tick < 0.5)
      return;
    List<Player> playerList = new List<Player>();
    foreach (KeyValuePair<Player, PlayerZoneBridge> containedPlayer in this._containedPlayers)
    {
      Player key = containedPlayer.Key;
      PlayerZoneBridge playerZoneBridge = containedPlayer.Value;
      if (Object.op_Equality((Object) key, (Object) null) || Object.op_Equality((Object) playerZoneBridge, (Object) null))
        playerList.Add(key);
    }
    foreach (Player key in playerList)
      this._containedPlayers.Remove(key);
    this._tick = 0.0f;
  }
}
