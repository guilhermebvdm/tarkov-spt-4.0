// Decompiled with JetBrains decompiler
// Type: RealismMod.ZoneSpawner
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
namespace RealismMod;

public static class ZoneSpawner
{
  public const float MinBotSpawnDistanceFromPlayer = 150f;

  private static QuestDataClass GetQuest(string questId)
  {
    return ((IPlayerAndPetProfile) Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession()).Profile.QuestsData.FirstOrDefault<QuestDataClass>((Func<QuestDataClass, bool>) (q => q.Id == questId));
  }

  public static bool CheckQuestStatus(string questId, EQuestStatus[] questStatuses)
  {
    bool flag = false;
    QuestDataClass quest = ZoneSpawner.GetQuest(questId);
    if (quest != null)
    {
      foreach (int questStatuse in questStatuses)
      {
        EQuestStatus equestStatus = (EQuestStatus) questStatuse;
        if (quest.Status == equestStatus)
        {
          flag = true;
          break;
        }
      }
    }
    return flag;
  }

  public static bool HasMetQuestCriteria(string[] questIds, EQuestStatus[] questStatuses)
  {
    foreach (string questId in questIds)
    {
      if (ZoneSpawner.CheckQuestStatus(questId, questStatuses))
        return true;
    }
    return false;
  }

  public static bool ShouldSpawnDynamicZones()
  {
    int num;
    if (!PluginConfig.ZoneDebug.Value && ProfileData.PMCLevel < 20)
      num = ZoneSpawner.HasMetQuestCriteria(new string[2]
      {
        "66dad1a18cbba6e558486336",
        "670ae811bd43cbf026768126"
      }, new EQuestStatus[2]
      {
        (EQuestStatus) 2,
        (EQuestStatus) 4
      }) ? 1 : 0;
    else
      num = 1;
    return num != 0;
  }

  public static void DebugSpawnPoints()
  {
    foreach (Vector3 safeSpawn in ZoneData.GetSafeSpawns())
    {
      Vector3 vector3;
      // ISSUE: explicit constructor call
      ((Vector3) ref vector3).\u002Ector(safeSpawn.x, safeSpawn.y, safeSpawn.z);
      GameObject gameObject = new GameObject(vector3.ToString());
      gameObject.transform.position = vector3;
      BoxCollider collider = gameObject.AddComponent<BoxCollider>();
      collider.size = new Vector3(1f, 500f, 1f);
      ZoneSpawner.VisualizeDebugZone(((Object) gameObject).name, gameObject.transform, collider);
    }
  }

  public static Vector3 TryGetSafeSpawnPoint(
    Player entity,
    bool isBot,
    bool blocksNav,
    bool isInRads)
  {
    IEnumerable<Vector3> safeSpawns = ZoneData.GetSafeSpawns();
    if (PluginConfig.ZoneDebug.Value)
      Utils.Logger.LogWarning((object) $"isbot {isBot}, blocksNav {blocksNav}, spawns null {safeSpawns == null}, currentMap {GameWorldController.CurrentMap} isInRads{isInRads}");
    if (safeSpawns == null || isBot && !blocksNav || !isBot && GameWorldController.CurrentMap == "laboratory" && !isInRads)
      return entity.Transform.position;
    IEnumerable<Vector3> source = safeSpawns;
    Player player = Utils.GetYourPlayer();
    if (isBot)
      source = safeSpawns.Where<Vector3>((Func<Vector3, bool>) (s => (double) Vector3.Distance(s, player.Transform.position) >= 150.0));
    return source.Any<Vector3>() || !isBot ? source.OrderBy<Vector3, float>((Func<Vector3, float>) (s => Vector3.Distance(s, entity.Transform.position))).First<Vector3>() : safeSpawns.OrderByDescending<Vector3, float>((Func<Vector3, float>) (s => Vector3.Distance(s, player.Transform.position))).First<Vector3>();
  }

  public static void CreateZones(ZoneCollection collection)
  {
    List<HazardGroup> zones = ZoneData.GetZones(collection.ZoneType, GameWorldController.CurrentMap);
    if (zones == null)
      return;
    if (PluginConfig.ZoneDebug.Value)
      ZoneSpawner.DebugSpawnPoints();
    foreach (HazardGroup hazardLocation in zones)
    {
      if (collection.ZoneType == EZoneType.Gas || collection.ZoneType == EZoneType.GasAssets)
        ZoneSpawner.CreateZone<GasZone>(hazardLocation, collection.ZoneType);
      if (collection.ZoneType == EZoneType.Radiation || collection.ZoneType == EZoneType.RadAssets)
        ZoneSpawner.CreateZone<RadiationZone>(hazardLocation, collection.ZoneType);
      if (collection.ZoneType == EZoneType.SafeZone)
        ZoneSpawner.CreateZone<LabsSafeZone>(hazardLocation, collection.ZoneType);
      if (collection.ZoneType == EZoneType.Quest)
        ZoneSpawner.CreateZone<QuestZone>(hazardLocation, collection.ZoneType);
      if (collection.ZoneType == EZoneType.Interactable)
        ZoneSpawner.CreateZone<InteractionZone>(hazardLocation, collection.ZoneType);
    }
  }

  private static bool ShouldSpawnZone(HazardGroup hazardLocation, EZoneType zoneType)
  {
    if (PluginConfig.ZoneDebug.Value)
      return true;
    if (!Plugin.FikaPresent)
    {
      int num1;
      if (hazardLocation.QuestToBlock != null)
        num1 = ZoneSpawner.CheckQuestStatus(hazardLocation.QuestToBlock, new EQuestStatus[1]
        {
          (EQuestStatus) 4
        }) ? 1 : 0;
      else
        num1 = 0;
      if (num1 != 0)
        return false;
      int num2;
      if (hazardLocation.QuestToEnable != null)
        num2 = !ZoneSpawner.CheckQuestStatus(hazardLocation.QuestToEnable, new EQuestStatus[1]
        {
          (EQuestStatus) 4
        }) ? 1 : 0;
      else
        num2 = 0;
      if (num2 != 0)
        return false;
      bool flag = (double) ProfileData.PMCLevel <= 10.0 && (double) hazardLocation.SpawnChance < 1.0 && zoneType != EZoneType.Radiation && zoneType != EZoneType.RadAssets && GameWorldController.CurrentMap != "laboratory";
      float num3 = !flag || !(GameWorldController.CurrentMap == "sandbox") ? (flag ? 0.25f : 1f) : 0.0f;
      return (double) Random.value <= (double) Mathf.Clamp01(Mathf.Max(hazardLocation.SpawnChance * num3, 0.01f));
    }
    DateTime utcNow = DateTime.UtcNow;
    return (double) ((utcNow.Year * 1000000 + utcNow.Month * 10000 + utcNow.Day * 100) % 101) <= (double) hazardLocation.SpawnChance * 100.0;
  }

  private static bool AnalsyableQuestChecker(string[] quests, EQuestStatus[] statuses)
  {
    bool flag = false;
    if (quests != null && quests.Length != 0)
    {
      foreach (string quest in quests)
      {
        if (ZoneSpawner.CheckQuestStatus(quest, statuses))
          flag = true;
      }
    }
    return flag;
  }

  private static bool CheckIsAnalysable(Analysable analysable)
  {
    if (analysable.NoRequirement)
      return true;
    return !ZoneSpawner.AnalsyableQuestChecker(analysable.DisabledBy, new EQuestStatus[1]
    {
      (EQuestStatus) 2
    }) | ZoneSpawner.AnalsyableQuestChecker(analysable.EnabledBy, new EQuestStatus[1]
    {
      (EQuestStatus) 2
    });
  }

  public static void AddGasVisual(
    Zone subZone,
    GameObject hazardZone,
    EZoneType zoneType,
    Vector3 position,
    Vector3 rotation,
    Vector3 size)
  {
    GameObject gameObject = Object.Instantiate<GameObject>(Assets.FogBundle.LoadAsset<GameObject>("Assets/Fog/Gray Volume Fog.prefab"), position, Quaternion.Euler(rotation));
    gameObject.transform.SetParent(hazardZone.transform, true);
    ParticleSystem[] componentsInChildren = gameObject.GetComponentsInChildren<ParticleSystem>();
    ((Object) gameObject).name = subZone.Name + "_particle_system";
    foreach (ParticleSystem particleSystem in componentsInChildren)
    {
      FogScript fogScript = ((Component) particleSystem).gameObject.AddComponent<FogScript>();
      ((Component) particleSystem).gameObject.transform.rotation = Quaternion.Euler(rotation);
      ((Component) particleSystem).gameObject.transform.position = position;
      ParticleSystem.ShapeModule shape = particleSystem.shape;
      fogScript.Scale = Vector3.op_Multiply(size, subZone.VisZoneSizeMulti);
      fogScript.UsePhysics = subZone.VisUsePhysics;
      fogScript.SpeedModi = subZone.VisSpeedModi;
      fogScript.OpacityModi = subZone.VisOpacityModi;
      fogScript.ParticleRate = subZone.VisParticleRate;
      fogScript.ParticleSize = new ParticleSystem.MinMaxCurve(4f, 9f);
    }
  }

  private static void SetUpSubZone<T>(
    HazardGroup hazardLocation,
    Zone subZone,
    GameObject zoneGroup,
    EZoneType zoneType,
    bool isBufferZone = false)
    where T : MonoBehaviour, IZone
  {
    string name = subZone.Name;
    Vector3 position;
    // ISSUE: explicit constructor call
    ((Vector3) ref position).\u002Ector(subZone.Position.X, subZone.Position.Y, subZone.Position.Z);
    Vector3 rotation;
    // ISSUE: explicit constructor call
    ((Vector3) ref rotation).\u002Ector(subZone.Rotation.X, subZone.Rotation.Y, subZone.Rotation.Z);
    Vector3 size;
    // ISSUE: explicit constructor call
    ((Vector3) ref size).\u002Ector(subZone.Size.X, subZone.Size.Y, subZone.Size.Z);
    bool flag1 = zoneType == EZoneType.Gas || zoneType == EZoneType.GasAssets;
    GameObject hazardZone = new GameObject(name);
    T obj = hazardZone.AddComponent<T>();
    obj.UsesDistanceFalloff = subZone.UsesDistanceFalloff;
    float num = 1f;
    if (flag1 && !Plugin.FikaPresent && !PluginConfig.ZoneDebug.Value && GameWorldController.CurrentMap != "laboratory")
      num = Random.Range(0.9f, 1.15f);
    obj.ZoneStrength = subZone.Strength * num;
    obj.IsAnalysable = subZone?.Analysable != null && ZoneSpawner.CheckIsAnalysable(subZone.Analysable);
    hazardZone.transform.position = position;
    hazardZone.transform.rotation = Quaternion.Euler(rotation);
    hazardZone.AddComponent<TriggerWithId>().SetId(name);
    string str = hazardLocation.Assets == null || !name.Contains("dynamicquest") ? name : "dynamic" + GameWorldController.CurrentMap;
    ((TriggerWithId) hazardZone.AddComponent<ExperienceTrigger>()).SetId(str);
    ((TriggerWithId) hazardZone.AddComponent<PlaceItemTrigger>()).SetId(str);
    hazardZone.layer = LayerMask.NameToLayer("Triggers");
    ((Object) hazardZone).name = name;
    BoxCollider collider = hazardZone.AddComponent<BoxCollider>();
    ((Collider) collider).isTrigger = true;
    collider.size = size;
    ((Object) collider).name = name + "_Collider";
    bool flag2 = (GameWorldController.DoMapGasEvent || GameWorldController.DoMapRads) && obj.ZoneType != EZoneType.GasAssets && obj.ZoneType != EZoneType.RadAssets;
    obj.BlocksNav = !flag2 && subZone.BlockNav;
    if (obj.BlocksNav)
    {
      NavMeshObstacle navMeshObstacle = hazardZone.AddComponent<NavMeshObstacle>();
      navMeshObstacle.carving = true;
      navMeshObstacle.center = collider.center;
      navMeshObstacle.size = collider.size;
    }
    if (zoneType == EZoneType.Interactable)
      obj.InteractableData = subZone.Interactable;
    if (((isBufferZone ? 0 : (subZone.UseVisual ? 1 : 0)) & (flag1 ? 1 : 0)) != 0 && PluginConfig.ShowGasEffects.Value)
      ZoneSpawner.AddGasVisual(subZone, hazardZone, zoneType, position, rotation, size);
    hazardZone.transform.SetParent(zoneGroup.transform, true);
    if (!PluginConfig.ZoneDebug.Value || isBufferZone)
      return;
    ZoneSpawner.VisualizeDebugZone(name, hazardZone.transform, collider, obj.ZoneType);
  }

  private static void VisualizeDebugZone(
    string name,
    Transform parentTransform,
    BoxCollider collider,
    EZoneType zoneType = EZoneType.Gas)
  {
    GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
    ((Object) primitive).name = name + "_Visual_Debug";
    primitive.transform.parent = parentTransform;
    primitive.transform.localScale = collider.size;
    primitive.transform.localPosition = collider.center;
    primitive.transform.rotation = ((Component) collider).transform.rotation;
    primitive.GetComponent<Renderer>().material.color = zoneType == EZoneType.Radiation || zoneType == EZoneType.RadAssets ? new Color(0.0f, 1f, 0.0f, 0.15f) : new Color(1f, 0.0f, 0.0f, 0.15f);
    Object.Destroy((Object) primitive.GetComponent<Collider>());
    MoveDaCube.AddComponentToExistingGO(primitive, name);
  }

  private static void AddBufferZone<T>(
    HazardGroup hazardLocation,
    Zone subZone,
    GameObject zoneGroup,
    EZoneType zoneType)
    where T : MonoBehaviour, IZone
  {
    Zone subZone1 = new Zone();
    subZone1.Name = subZone.Name + "_Buffer_Zone";
    subZone1.UsesDistanceFalloff = false;
    subZone1.Strength = 10f;
    subZone1.BlockNav = subZone.BlockNav;
    subZone1.Position = subZone.Position;
    subZone1.Rotation = subZone.Rotation;
    Vector3 vector3 = Vector3.op_Multiply(new Vector3(subZone.Size.X, subZone.Size.Y, subZone.Size.Z), 1.4f);
    subZone1.Size = new Size()
    {
      X = vector3.x,
      Y = vector3.y,
      Z = vector3.z
    };
    ZoneSpawner.SetUpSubZone<T>(hazardLocation, subZone1, zoneGroup, zoneType, true);
  }

  public static void CreateZone<T>(HazardGroup hazardLocation, EZoneType zoneType) where T : MonoBehaviour, IZone
  {
    if (hazardLocation.IsTriggered || !ZoneSpawner.ShouldSpawnZone(hazardLocation, zoneType))
      return;
    ZoneSpawner.HandleZoneAssets(hazardLocation);
    ZoneSpawner.HandleZoneLoot(hazardLocation);
    GameObject zoneGroup = new GameObject(zoneType.ToString() + Utils.GenId());
    foreach (Zone zone in hazardLocation.Zones)
    {
      ZoneSpawner.SetUpSubZone<T>(hazardLocation, zone, zoneGroup, zoneType);
      if (zone.UseVisual && (zoneType == EZoneType.Gas || zoneType == EZoneType.GasAssets))
        ZoneSpawner.AddBufferZone<T>(hazardLocation, zone, zoneGroup, zoneType);
    }
    if (zoneType != EZoneType.Interactable)
      return;
    zoneGroup.AddComponent<InteractableGroupComponent>().GroupData = hazardLocation.InteractableGroup;
  }

  public static void HandleZoneAssets(HazardGroup zone)
  {
    if (zone.Assets == null)
      return;
    foreach (Asset asset in zone.Assets)
    {
      if (Utils.SystemRandom.Next(101) <= asset.Odds || Plugin.FikaPresent)
      {
        if (asset.RandomizeRotation)
          asset.Rotation.Y = GClass1203.Range(Utils.SystemRandom, 0.0f, 360f);
        Vector3 vector3_1;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_1).\u002Ector(asset.Position.X, asset.Position.Y, asset.Position.Z);
        Vector3 vector3_2;
        // ISSUE: explicit constructor call
        ((Vector3) ref vector3_2).\u002Ector(asset.Rotation.X, asset.Rotation.Y, asset.Rotation.Z);
        GameObject andLoadAsset = ZoneSpawner.GetAndLoadAsset(asset.AssetName);
        if (Object.op_Equality((Object) andLoadAsset, (Object) null))
          Utils.Logger.LogError((object) ("Realism Mod: Error Loading Asset From Bundle For Asset: " + asset.AssetName));
        Object.Instantiate<GameObject>(andLoadAsset, vector3_1, Quaternion.Euler(vector3_2));
      }
    }
  }

  public static void HandleZoneLoot(HazardGroup zone)
  {
    if (zone.Loot == null || Plugin.FikaPresent)
      return;
    foreach (Loot loot in zone.Loot)
    {
      if (Utils.SystemRandom.Next(101) <= loot.Odds)
      {
        if (loot.RandomizeRotation)
          loot.Rotation.Y = GClass1203.Range(Utils.SystemRandom, 0.0f, 360f);
        Vector3 postion;
        // ISSUE: explicit constructor call
        ((Vector3) ref postion).\u002Ector(loot.Position.X, loot.Position.Y, loot.Position.Z);
        Vector3 rotation;
        // ISSUE: explicit constructor call
        ((Vector3) ref rotation).\u002Ector(loot.Rotation.X, loot.Rotation.Y, loot.Rotation.Z);
        string tempalteId = loot?.LootOverride == null || loot.LootOverride.Count <= 0 ? ZoneSpawner.GetLootTempalteIdFromTier(loot.Type) : ZoneSpawner.GetLootTempalteIdFromOverride(loot.LootOverride);
        ZoneSpawner.LoadLooseLoot(postion, rotation, tempalteId);
      }
    }
  }

  public static string GetLootTempalteIdFromTier(string lootTier)
  {
    Dictionary<string, int> items;
    switch (lootTier)
    {
      case "highTier":
        items = DynamicRadZoneLoot.HighTier;
        break;
      case "midTier":
        items = DynamicRadZoneLoot.MidTier;
        break;
      default:
        items = DynamicRadZoneLoot.LowTier;
        break;
    }
    return Utils.GetRandomWeightedKey(items);
  }

  public static string GetLootTempalteIdFromOverride(Dictionary<string, int> lootOdds)
  {
    return Utils.GetRandomWeightedKey(lootOdds);
  }

  public static GameObject GetAndLoadAsset(string assetName)
  {
    switch (assetName)
    {
      case "GooBarrel":
        return Assets.GooBarrelBundle.LoadAsset<GameObject>("Assets/Labs/yellow_barrel.prefab");
      case "BlueBox":
        return Assets.BlueBoxBundle.LoadAsset<GameObject>("Assets/Prefabs/polytheneBox (6).prefab");
      case "RedForkLift":
        return Assets.RedForkLiftBundle.LoadAsset<GameObject>("Assets/Prefabs/autoloader.prefab");
      case "ElectroForkLift":
        return Assets.ElectroForkLiftBundle.LoadAsset<GameObject>("Assets/Prefabs/electroCar (2).prefab");
      case "LabsCrate":
        return Assets.LabsCrateBundle.LoadAsset<GameObject>("Assets/Prefabs/woodBox_medium.prefab");
      case "Ural":
        return Assets.UralBundle.LoadAsset<GameObject>("Assets/Prefabs/ural280_closed_update.prefab");
      case "BluePallet":
        return Assets.BluePalletBundle.LoadAsset<GameObject>("Assets/Prefabs/pallete_plastic_blue (10).prefab");
      case "BlueFuelPalletCloth":
        return Assets.BlueFuelPalletClothBundle.LoadAsset<GameObject>("Assets/Prefabs/pallet_barrel_heap_update.prefab");
      case "BarrelPile":
        return Assets.BarrelPileBundle.LoadAsset<GameObject>("Assets/Prefabs/barrel_pile (1).prefab");
      case "LabsCrateSmall":
        return Assets.LabsCrateSmallBundle.LoadAsset<GameObject>("Assets/Prefabs/woodBox_small (2).prefab");
      case "YellowPlasticPallet":
        return Assets.YellowPlasticPalletBundle.LoadAsset<GameObject>("Assets/Prefabs/pallet_barrel_plastic_clear_P (4).prefab");
      case "WhitePlasticPallet":
        return Assets.WhitePlasticPalletBundle.LoadAsset<GameObject>("Assets/Prefabs/pallet_barrel_plastic_clear_P (5).prefab");
      case "MetalFence":
        return Assets.MetalFenceBundle.LoadAsset<GameObject>("Assets/Prefabs/fence_metall_part3_update.prefab");
      case "LabsBarrelPile":
        return Assets.LabsBarrelPileBundle.LoadAsset<GameObject>("Assets/Realism Hazard Prefabs/Prefab/Barrel_plastic_clear_set_01.prefab");
      case "RedContainer":
        return Assets.RedContainerBundle.LoadAsset<GameObject>("Assets/Prefabs/container_6m_red_close.prefab");
      case "BlueContainer":
        return Assets.BlueContainerBundle.LoadAsset<GameObject>("container_6m_blue_close (1)");
      case "RadSign1":
        return Assets.RadSign1.LoadAsset<GameObject>("Assets/prefabs/Rad Sign 1.prefab");
      case "TerraGroupFence":
        return Assets.TerraGroupFence.LoadAsset<GameObject>("Assets/prefabs/fence_ema_nocurt (10).prefab");
      default:
        return (GameObject) null;
    }
  }

  private static void LoadLooseLoot(Vector3 postion, Vector3 rotation, string tempalteId)
  {
    Quaternion rotation1 = Quaternion.Euler(rotation);
    Utils.LoadLoot(postion, rotation1, tempalteId);
  }

  public static void DebugZones()
  {
    string str = PluginConfig.TargetZone.Value;
    GameObject gameObject1 = GameObject.Find(str);
    if (Object.op_Equality((Object) gameObject1, (Object) null))
    {
      GameObject gameObject2 = new GameObject(str);
      gameObject2.transform.position = new Vector3(PluginConfig.test4.Value, PluginConfig.test5.Value, PluginConfig.test6.Value);
      gameObject2.transform.rotation = Quaternion.Euler(new Vector3(PluginConfig.test7.Value, PluginConfig.test8.Value, PluginConfig.test9.Value));
      gameObject2.AddComponent<TriggerWithId>().SetId(str);
      gameObject2.layer = LayerMask.NameToLayer("Triggers");
      ((Object) gameObject2).name = str;
      BoxCollider boxCollider = gameObject2.AddComponent<BoxCollider>();
      ((Collider) boxCollider).isTrigger = true;
      boxCollider.size = new Vector3(PluginConfig.test1.Value, PluginConfig.test2.Value, PluginConfig.test3.Value);
      GameObject primitive = GameObject.CreatePrimitive((PrimitiveType) 3);
      ((Object) primitive).name = str + "Visual";
      primitive.transform.parent = gameObject2.transform;
      primitive.transform.localScale = boxCollider.size;
      primitive.transform.localPosition = boxCollider.center;
      primitive.transform.rotation = ((Component) boxCollider).transform.rotation;
      primitive.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f, 0.25f);
      Object.Destroy((Object) primitive.GetComponent<Collider>());
      Utils.Logger.LogWarning((object) ("player pos " + Utils.GetYourPlayer().Transform.position.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone pos " + gameObject2.transform.position.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone rot " + gameObject2.transform.rotation.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone size " + gameObject2.GetComponent<BoxCollider>().size.ToString()));
    }
    else
    {
      gameObject1.transform.position = new Vector3(PluginConfig.test4.Value, PluginConfig.test5.Value, PluginConfig.test6.Value);
      gameObject1.transform.rotation = Quaternion.Euler(new Vector3(PluginConfig.test7.Value, PluginConfig.test8.Value, PluginConfig.test9.Value));
      BoxCollider component = gameObject1.GetComponent<BoxCollider>();
      component.size = new Vector3(PluginConfig.test1.Value, PluginConfig.test2.Value, PluginConfig.test3.Value);
      GameObject gameObject3 = GameObject.Find(str + "Visual");
      gameObject3.transform.parent = gameObject1.transform;
      gameObject3.transform.localScale = component.size;
      gameObject3.transform.localPosition = component.center;
      gameObject3.transform.rotation = ((Component) component).transform.rotation;
      Utils.Logger.LogWarning((object) ("player pos " + Utils.GetYourPlayer().Transform.position.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone pos " + gameObject1.transform.position.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone rot " + gameObject1.transform.rotation.ToString()));
      Utils.Logger.LogWarning((object) ("gasZone size " + gameObject1.GetComponent<BoxCollider>().size.ToString()));
    }
  }
}
