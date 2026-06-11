// Decompiled with JetBrains decompiler
// Type: RealismMod.Plugin
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using RealismMod.Audio;
using RealismMod.Health;
using RealismMod.VisualEffects;
using SPT.Common.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#nullable disable
namespace RealismMod;

[BepInPlugin("RealismMod", "RealismMod", "1.6.3")]
public class Plugin : BaseUnityPlugin
{
  private const string PLUGINVERSION = "1.6.3";
  public static Plugin Instance;
  public static HealthControllerClass BSGHealthController;
  public static Dictionary<Enum, Sprite> IconCache = new Dictionary<Enum, Sprite>();
  public static Dictionary<string, Sprite> LoadedSprites = new Dictionary<string, Sprite>();
  public static Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();
  private string _baseBundleFilepath;
  private static float _averageFPS = 0.0f;
  public static float FPS = 0.0f;
  public MountingUI MountingUIComponent;
  public static RealismHealthController RealHealthController;
  public static RealismWeatherController RealismWeatherComponent;
  public static RealismAudioController RealismAudioController;
  public static bool FikaPresent = false;
  public static bool FOVFixPresent = false;
  private bool _detectedMods = false;
  public static bool StartRechamberTimer = false;
  public static float ChamberTimer = 0.0f;
  public static bool CanLoadChamber = false;
  public static bool BlockChambering = false;
  private bool _gotProfileId = false;
  public static RealismConfig ServerConfig;
  public static RealismEventInfo ModInfo;
  public static Dictionary<string, RealismItem> ItemTemplateData = new Dictionary<string, RealismItem>();

  public static GameObject MountingUIGameObject { get; private set; }

  public static GameObject ExplosionGO { get; private set; }

  private GameObject RealismWeatherGameObject { get; set; }

  private GameObject AudioControllerGameObject { get; set; }

  private static T UpdateInfoFromServer<T>(string route) where T : class, IRealismInfo
  {
    Utils.Logger.LogWarning((object) ("update from server: " + route));
    new JsonSerializerSettings().MissingMemberHandling = (MissingMemberHandling) 0;
    try
    {
      return JsonConvert.DeserializeObject<T>(RequestHandler.GetJson(route));
    }
    catch (Exception ex)
    {
      Utils.Logger.LogError((object) $"REALISM MOD ERROR: FAILED TO FETCH DATA FROM SERVER USING ROUTE {route}: {ex.Message}");
      return default (T);
    }
  }

  public static void RequestRealismDataFromServer(EUpdateType updateType)
  {
    switch (updateType)
    {
      case EUpdateType.Full:
        Plugin.ServerConfig = Plugin.UpdateInfoFromServer<RealismConfig>("/RealismMod/GetConfig");
        Plugin.ModInfo = Plugin.UpdateInfoFromServer<RealismEventInfo>("/RealismMod/GetInfo");
        break;
      case EUpdateType.ModInfo:
        Plugin.ModInfo = Plugin.UpdateInfoFromServer<RealismEventInfo>("/RealismMod/GetInfo");
        break;
      case EUpdateType.ModConfig:
        Plugin.ServerConfig = Plugin.UpdateInfoFromServer<RealismConfig>("/RealismMod/GetConfig");
        break;
      case EUpdateType.TimeOfDay:
        Plugin.ModInfo = Plugin.UpdateInfoFromServer<RealismEventInfo>("/RealismMod/GetTimeOfDay");
        break;
    }
  }

  private async void CacheIcons()
  {
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.ShotDispersion, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.BluntThroughput, Resources.Load<Sprite>("characteristics/icons/armorMaterial"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.VerticalRecoil, Resources.Load<Sprite>("characteristics/icons/Ergonomics"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.HorizontalRecoil, Resources.Load<Sprite>("characteristics/icons/Recoil Back"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Dispersion, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.CameraRecoil, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.AutoROF, Resources.Load<Sprite>("characteristics/icons/bFirerate"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.SemiROF, Resources.Load<Sprite>("characteristics/icons/bFirerate"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.ReloadSpeed, Resources.Load<Sprite>("characteristics/icons/weapFireType"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.FixSpeed, Resources.Load<Sprite>("characteristics/icons/icon_info_raidmoddable"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.ChamberSpeed, Resources.Load<Sprite>("characteristics/icons/icon_info_raidmoddable"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.AimSpeed, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Firerate, Resources.Load<Sprite>("characteristics/icons/bFirerate"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Damage, Resources.Load<Sprite>("characteristics/icons/icon_info_bulletspeed"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Penetration, Resources.Load<Sprite>("characteristics/icons/armorClass"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.BallisticCoefficient, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.ArmorDamage, Resources.Load<Sprite>("characteristics/icons/armorMaterial"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.FragmentationChance, Resources.Load<Sprite>("characteristics/icons/icon_info_bloodloss"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.MalfunctionChance, Resources.Load<Sprite>("characteristics/icons/icon_info_raidmoddable"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.CanSpall, Resources.Load<Sprite>("characteristics/icons/icon_info_bulletspeed"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.SpallReduction, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.GearReloadSpeed, Resources.Load<Sprite>("characteristics/icons/weapFireType"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.CantADS, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.CanADS, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.NoiseReduction, Resources.Load<Sprite>("characteristics/icons/icon_info_loudness"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.ProjectileCount, Resources.Load<Sprite>("characteristics/icons/icon_info_bulletspeed"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Convergence, Resources.Load<Sprite>("characteristics/icons/Ergonomics"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.HBleedType, Resources.Load<Sprite>("characteristics/icons/icon_info_bloodloss"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.LimbHpPerTick, Resources.Load<Sprite>("characteristics/icons/icon_info_bloodloss"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.HpPerTick, Resources.Load<Sprite>("characteristics/icons/hpResource"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.RemoveTrnqt, Resources.Load<Sprite>("characteristics/icons/hpResource"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Comfort, Resources.Load<Sprite>("characteristics/icons/Weight"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.GasProtection, Resources.Load<Sprite>("characteristics/icons/icon_info_intoxication"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.RadProtection, Resources.Load<Sprite>("characteristics/icons/icon_info_radiation"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.PainKillerStrength, Resources.Load<Sprite>("characteristics/icons/hpResource"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.MeleeDamage, Resources.Load<Sprite>("characteristics/icons/icon_info_bloodloss"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.MeleePen, Resources.Load<Sprite>("characteristics/icons/icon_info_bulletspeed"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.OutOfRaidHP, Resources.Load<Sprite>("characteristics/icons/hpResource"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.StimType, Resources.Load<Sprite>("characteristics/icons/hpResource"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.DurabilityBurn, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Heat, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.MuzzleFlash, Resources.Load<Sprite>("characteristics/icons/Velocity"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Handling, Resources.Load<Sprite>("characteristics/icons/Ergonomics"));
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.AimStability, Resources.Load<Sprite>("characteristics/icons/SightingRange"));
    Sprite balanceSprite = await this.RequestResource<Sprite>(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\icons\\balance.png");
    Sprite recoilAngleSprite = await this.RequestResource<Sprite>(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\icons\\recoilAngle.png");
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.Balance, balanceSprite);
    Plugin.IconCache.Add((Enum) Attributes.ENewItemAttributeId.RecoilAngle, recoilAngleSprite);
    balanceSprite = (Sprite) null;
    recoilAngleSprite = (Sprite) null;
  }

  private void LoadTextures()
  {
    foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\masks\\", "*.png"))
      this.LoadTexture(file);
  }

  private void LoadSprites()
  {
    foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\icons\\", "*.png"))
      this.LoadSprite(file);
  }

  private async void LoadSprite(string path)
  {
    Dictionary<string, Sprite> dictionary = Plugin.LoadedSprites;
    string key = Path.GetFileName(path);
    Sprite sprite = await this.RequestResource<Sprite>(path);
    dictionary[key] = sprite;
    dictionary = (Dictionary<string, Sprite>) null;
    key = (string) null;
    sprite = (Sprite) null;
  }

  private async void LoadTexture(string path)
  {
    Dictionary<string, Texture> dictionary = Plugin.LoadedTextures;
    string key = Path.GetFileName(path);
    Texture texture = await this.RequestResource<Texture>(path, true);
    dictionary[key] = texture;
    dictionary = (Dictionary<string, Texture>) null;
    key = (string) null;
    texture = (Texture) null;
  }

  private async Task<T> RequestResource<T>(string path, bool isMask = false) where T : class
  {
    UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(path);
    UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();
    while (!((AsyncOperation) sendWeb).isDone)
      await Task.Yield();
    if (uwr.isNetworkError || uwr.isHttpError)
    {
      this.Logger.LogError((object) "Realism Mod: Failed To Fetch Resource");
      return default (T);
    }
    Texture2D texture = ((DownloadHandlerTexture) uwr.downloadHandler).texture;
    if (typeof (T) == typeof (Texture))
    {
      if (isMask)
      {
        Texture2D flipped = new Texture2D(((Texture) texture).width, ((Texture) texture).height);
        for (int y = 0; y < ((Texture) texture).height; ++y)
        {
          for (int x = 0; x < ((Texture) texture).width; ++x)
            flipped.SetPixel(x, ((Texture) texture).height - y - 1, texture.GetPixel(x, y));
        }
        flipped.Apply();
        texture = flipped;
        flipped = (Texture2D) null;
      }
      return texture as T;
    }
    if (typeof (T) == typeof (Sprite))
    {
      Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, (float) ((Texture) texture).width, (float) ((Texture) texture).height), new Vector2(0.5f, 0.5f));
      return sprite as T;
    }
    this.Logger.LogError((object) "Realism Mod: Unsupported resource type requested");
    return default (T);
  }

  private AssetBundle LoadAndInitializePrefabs(string bundlePath)
  {
    return AssetBundle.LoadFromFile(Path.Combine(this._baseBundleFilepath, bundlePath));
  }

  private void LoadBundles()
  {
    this._baseBundleFilepath = Path.Combine(Environment.CurrentDirectory, "BepInEx\\plugins\\Realism\\bundles\\");
    Assets.GooBarrelBundle = this.LoadAndInitializePrefabs("hazard_assets\\yellow_barrel.bundle");
    Assets.BlueBoxBundle = this.LoadAndInitializePrefabs("hazard_assets\\bluebox.bundle");
    Assets.RedForkLiftBundle = this.LoadAndInitializePrefabs("hazard_assets\\redforklift.bundle");
    Assets.ElectroForkLiftBundle = this.LoadAndInitializePrefabs("hazard_assets\\electroforklift.bundle");
    Assets.LabsCrateBundle = this.LoadAndInitializePrefabs("hazard_assets\\labscrate.bundle");
    Assets.UralBundle = this.LoadAndInitializePrefabs("hazard_assets\\ural.bundle");
    Assets.BluePalletBundle = this.LoadAndInitializePrefabs("hazard_assets\\bluepallet.bundle");
    Assets.BlueFuelPalletClothBundle = this.LoadAndInitializePrefabs("hazard_assets\\bluebarrelpalletcloth.bundle");
    Assets.BarrelPileBundle = this.LoadAndInitializePrefabs("hazard_assets\\barrelpile.bundle");
    Assets.LabsCrateSmallBundle = this.LoadAndInitializePrefabs("hazard_assets\\labscratesmall.bundle");
    Assets.YellowPlasticPalletBundle = this.LoadAndInitializePrefabs("hazard_assets\\yellowbarrelpallet.bundle");
    Assets.WhitePlasticPalletBundle = this.LoadAndInitializePrefabs("hazard_assets\\whitebarrelpallet.bundle");
    Assets.MetalFenceBundle = this.LoadAndInitializePrefabs("hazard_assets\\metalfence.bundle");
    Assets.RedContainerBundle = this.LoadAndInitializePrefabs("hazard_assets\\redcontainer.bundle");
    Assets.BlueContainerBundle = this.LoadAndInitializePrefabs("hazard_assets\\bluecontainer.bundle");
    Assets.LabsBarrelPileBundle = this.LoadAndInitializePrefabs("hazard_assets\\labsbarrelpile.bundle");
    Assets.RadSign1 = this.LoadAndInitializePrefabs("hazard_assets\\radsign1.bundle");
    Assets.TerraGroupFence = this.LoadAndInitializePrefabs("hazard_assets\\terragroupchainfence.bundle");
    Assets.FogBundle = this.LoadAndInitializePrefabs("hazard_assets\\fog.bundle");
    Assets.GasBundle = this.LoadAndInitializePrefabs("hazard_assets\\gas.bundle");
    Assets.ExplosionBundle = this.LoadAndInitializePrefabs("exp\\expl.bundle");
    Plugin.ExplosionGO = Assets.ExplosionBundle.LoadAsset<GameObject>("Assets/Explosion/Prefab/NUCLEAR_EXPLOSION.prefab");
    Object.DontDestroyOnLoad((Object) Plugin.ExplosionGO);
  }

  private void LoadMountingUI()
  {
    Plugin.MountingUIGameObject = new GameObject();
    this.MountingUIComponent = Plugin.MountingUIGameObject.AddComponent<MountingUI>();
    Object.DontDestroyOnLoad((Object) Plugin.MountingUIGameObject);
  }

  private void LoadWeatherController()
  {
    this.RealismWeatherGameObject = new GameObject();
    Plugin.RealismWeatherComponent = this.RealismWeatherGameObject.AddComponent<RealismWeatherController>();
    Object.DontDestroyOnLoad((Object) this.RealismWeatherGameObject);
  }

  private void LoadAudioController()
  {
    this.AudioControllerGameObject = new GameObject();
    Plugin.RealismAudioController = this.AudioControllerGameObject.AddComponent<RealismAudioController>();
    Object.DontDestroyOnLoad((Object) this.AudioControllerGameObject);
    Plugin.RealismAudioController.LoadAudioClips();
  }

  private void LoadHealthController()
  {
    Plugin.RealHealthController = new RealismHealthController(new DamageTracker());
  }

  private void Awake()
  {
    Utils.Logger = this.Logger;
    Plugin.Instance = this;
    new SprintPatch().Enable();
    try
    {
      Plugin.RequestRealismDataFromServer(EUpdateType.Full);
      this.LoadBundles();
      this.LoadSprites();
      this.LoadTextures();
      this.CacheIcons();
      ZoneData.DeserializeZoneData();
      TemplateStats.RequestAndProcessDataFromServer();
    }
    catch (Exception ex)
    {
      this.Logger.LogError((object) ex);
    }
    this.LoadMountingUI();
    this.LoadWeatherController();
    this.LoadHealthController();
    this.LoadAudioController();
    PluginConfig.InitConfigBindings(this.Config);
    MoveDaCube.InitTempBindings(this.Config);
    this.LoadGeneralPatches();
    if (Plugin.ServerConfig.enable_hazard_zones)
      this.LoadHazardPatches();
    if (Plugin.ServerConfig.malf_changes)
      this.LoadMalfPatches();
    if (Plugin.ServerConfig.recoil_attachment_overhaul)
      this.LoadRecoilPatches();
    this.LoadReloadPatches();
    if (Plugin.ServerConfig.realistic_ballistics)
      this.LoadBallisticsPatches();
    if (Plugin.ServerConfig.headset_changes)
      this.LoadDeafenPatches();
    if (Plugin.ServerConfig.gear_weight)
      new TotalWeightPatch().Enable();
    if (Plugin.ServerConfig.movement_changes)
      this.LoadMovementPatches();
    if (Plugin.ServerConfig.enable_stances)
      this.LoadStancePatches();
    if (Plugin.ServerConfig.enable_stances || Plugin.ServerConfig.realistic_ballistics)
      new ApplyComplexRotationPatch().Enable();
    if (Plugin.ServerConfig.enable_stances || Plugin.ServerConfig.headset_changes)
      new ADSAudioPatch().Enable();
    if (Plugin.ServerConfig.med_changes)
      this.LoadMedicalPatches();
    if (!Plugin.ServerConfig.med_changes && !Plugin.ServerConfig.food_changes)
      return;
    new ApplyItemStashPatch().Enable();
    new StimStackPatch1().Enable();
    new StimStackPatch2().Enable();
  }

  private void CheckForMods()
  {
    if (this._detectedMods || (int) Time.time % 5 != 0)
      return;
    this._detectedMods = true;
    if (Chainloader.PluginInfos.ContainsKey("com.fika.core"))
      Plugin.FikaPresent = true;
    if (Chainloader.PluginInfos.ContainsKey("FOVFix"))
      Plugin.FOVFixPresent = true;
  }

  private void CheckForProfileData()
  {
    if (this._gotProfileId)
      return;
    try
    {
      ISession clientBackEndSession = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
      ProfileData.PMCProfileId = ((IPlayerAndPetProfile) clientBackEndSession).Profile.Id;
      ProfileData.ScavProfileId = ((IPlayerAndPetProfile) clientBackEndSession).ProfileOfPet.Id;
      ProfileData.PMCLevel = ((IPlayerAndPetProfile) clientBackEndSession).Profile.Info.Level;
      if (Plugin.ServerConfig.enable_hazard_zones)
        HazardTracker.GetHazardValues(ProfileData.PMCProfileId);
      this._gotProfileId = true;
    }
    catch
    {
      if (PluginConfig.EnableGeneralLogging.Value)
        this.Logger.LogWarning((object) "Realism Mod: Error Getting Profile ID, Retrying");
    }
  }

  private void ZoneDebugUpdate()
  {
    if (Input.GetKeyDown((KeyCode) 257))
    {
      BifacialTransform transform = Utils.GetYourPlayer().Transform;
      Utils.LoadLoot(transform.position, transform.rotation, PluginConfig.TargetZone.Value);
      ManualLogSource logger1 = Utils.Logger;
      string[] strArray1 = new string[7];
      strArray1[0] = "\"position\": {\"x\":";
      Vector3 position = transform.position;
      strArray1[1] = position.x.ToString();
      strArray1[2] = ",\"y\":";
      position = transform.position;
      strArray1[3] = position.y.ToString();
      strArray1[4] = ",\"z\":";
      position = transform.position;
      strArray1[5] = position.z.ToString();
      strArray1[6] = "},";
      string str1 = string.Concat(strArray1);
      logger1.LogWarning((object) str1);
      ManualLogSource logger2 = Utils.Logger;
      string[] strArray2 = new string[7];
      strArray2[0] = "\"rotation\": {\"x\":";
      Quaternion rotation = transform.rotation;
      strArray2[1] = ((Quaternion) ref rotation).eulerAngles.x.ToString();
      strArray2[2] = ",\"y\":";
      Vector3 eulerAngles = transform.eulerAngles;
      strArray2[3] = eulerAngles.y.ToString();
      strArray2[4] = ",\"z\":";
      eulerAngles = transform.eulerAngles;
      strArray2[5] = eulerAngles.z.ToString();
      strArray2[6] = "}";
      string str2 = string.Concat(strArray2);
      logger2.LogWarning((object) str2);
    }
    if (Input.GetKeyDown((KeyCode) 256 /*0x0100*/))
      HazardTracker.WipeTracker();
    KeyboardShortcut keyboardShortcut = PluginConfig.AddZone.Value;
    if (Input.GetKeyDown(((KeyboardShortcut) ref keyboardShortcut).MainKey))
      ZoneSpawner.DebugZones();
    if (!Input.GetKeyDown((KeyCode) 261))
      return;
    Object.Instantiate<GameObject>(Plugin.ExplosionGO, new Vector3(PluginConfig.test1.Value, PluginConfig.test2.Value, PluginConfig.test3.Value), new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
  }

  private void GetFps()
  {
    Plugin._averageFPS += (float) (((double) Time.deltaTime / (double) Time.timeScale - (double) Plugin._averageFPS) * 0.035000000149011612);
    Plugin.FPS = 1f / Plugin._averageFPS;
    if (float.IsNaN(Plugin.FPS) || (double) Plugin.FPS <= 1.0)
      Plugin.FPS = 144f;
    Plugin.FPS = Mathf.Clamp(Plugin.FPS, 30f, 200f);
  }

  private void Update()
  {
    if (GameWorldController.GameStarted && PluginConfig.ZoneDebug.Value)
      MoveDaCube.Update();
    this.GetFps();
    this.CheckForProfileData();
    this.CheckForMods();
    if (Plugin.ServerConfig.enable_hazard_zones)
      HazardTracker.OutOfRaidUpdate();
    if (PluginConfig.ZoneDebug.Value)
      this.ZoneDebugUpdate();
    if (Plugin.ServerConfig.med_changes)
      Plugin.RealHealthController.ControllerUpdate();
    Utils.CheckIsReady();
    if (!Utils.PlayerIsReady)
      return;
    GameWorldController.GameWorldUpdate();
    if (Plugin.ServerConfig.headset_changes && Object.op_Inequality((Object) ScreenEffectsController.PrismEffects, (Object) null))
    {
      HeadsetGainController.AdjustHeadsetVolume();
      DeafenController.DoDeafening();
    }
    if (Plugin.ServerConfig.enable_stances)
      StanceController.StanceUpdate();
  }

  private void LoadGeneralPatches()
  {
    new InitiateShotPatch().Enable();
    new FlyingBulletPatch().Enable();
    new ChamberCheckUIPatch().Enable();
    new InventoryOpenPatch().Enable();
    new KeyInputPatch1().Enable();
    new KeyInputPatch2().Enable();
    new SyncWithCharacterSkillsPatch().Enable();
    new OnItemAddedOrRemovedPatch().Enable();
    new PlayerUpdatePatch().Enable();
    new PlayerInitPatch().Enable();
    new FaceshieldMaskPatch().Enable();
    new PlayPhrasePatch().Enable();
    new OnGameStartPatch().Enable();
    new OnGameEndPatch().Enable();
    new QuestCompletePatch().Enable();
    new ExfilInitPatch().Enable();
    new GamePlayerPatch().Enable();
    new RigConstructorPatch().Enable();
    new EquipmentPenaltyComponentPatch().Enable();
    if (!Plugin.ServerConfig.loot_changes)
      return;
    new StaticLootSpawnPatch().Enable();
    new RigidLootSpawnPatch().Enable();
  }

  private void LoadHazardPatches()
  {
    new HealthPanelPatch().Enable();
    new DropItemPatch().Enable();
    new GetAvailableActionsPatch().Enable();
    if (Plugin.ServerConfig.boss_spawns || Plugin.ServerConfig.spawn_waves)
      new BossSpawnPatch().Enable();
    new LampPatch().Enable();
    new AmbientSoundPlayerGroupPatch().Enable();
    new DayTimeAmbientPatch().Enable();
    new DayTimeSpawnPatch().Enable();
    new BirdPatch().Enable();
  }

  private void LoadMalfPatches()
  {
    new RemoveSillyBossForcedMalf().Enable();
    new GetTotalMalfunctionChancePatch().Enable();
    new IsKnownMalfTypePatch().Enable();
    if (!Plugin.ServerConfig.manual_chambering)
      return;
    new SetAmmoCompatiblePatch().Enable();
    new StartReloadPatch().Enable();
    new StartEquipWeapPatch().Enable();
    new SetAmmoOnMagPatch().Enable();
    new PreChamberLoadPatch().Enable();
  }

  private void LoadRecoilPatches()
  {
    if (PluginConfig.EnableMuzzleEffects.Value)
      new MuzzleEffectsPatch().Enable();
    new UpdateWeaponVariablesPatch().Enable();
    new SetAimingSlowdownPatch().Enable();
    new PwaWeaponParamsPatch().Enable();
    new UpdateSwayFactorsPatch().Enable();
    new GetOverweightPatch().Enable();
    new SetOverweightPatch().Enable();
    new BreathProcessPatch().Enable();
    new CamRecoilPatch().Enable();
    new VisualRecoilPatch().Enable();
    new TotalShotgunDispersionPatch().Enable();
    new GetDurabilityLossOnShotPatch().Enable();
    new FireratePitchPatch().Enable();
    new AutoFireRatePatch().Enable();
    new SingleFireRatePatch().Enable();
    new ErgoDeltaPatch().Enable();
    new ErgoWeightPatch().Enable();
    new PlayerErgoPatch().Enable();
    new ToggleAimPatch().Enable();
    new GetMalfunctionStatePatch().Enable();
    if (PluginConfig.EnableZeroShift.Value)
    {
      new CalibrationLookAt().Enable();
      new CalibrationLookAtScope().Enable();
    }
    new ModConstructorPatch().Enable();
    new WeaponConstructorPatch().Enable();
    new HRecoilDisplayStringValuePatch().Enable();
    new HRecoilDisplayDeltaPatch().Enable();
    new VRecoilDisplayStringValuePatch().Enable();
    new VRecoilDisplayDeltaPatch().Enable();
    new ModVRecoilStatDisplayPatchFloat().Enable();
    new ModVRecoilStatDisplayPatchString().Enable();
    new ErgoDisplayDeltaPatch().Enable();
    new ErgoDisplayStringValuePatch().Enable();
    new FireRateDisplayStringValuePatch().Enable();
    new ModErgoStatDisplayPatch().Enable();
    new GetAttributeIconPatches().Enable();
    new MagazineMalfChanceDisplayPatch().Enable();
    new BarrelModClassPatch().Enable();
    new AmmoCaliberPatch().Enable();
    new COIDeltaPatch().Enable();
    new CenterOfImpactMOAPatch().Enable();
    new COIDisplayDeltaPatch().Enable();
    new COIDisplayStringValuePatch().Enable();
    new GetTotalCenterOfImpactPatch().Enable();
    new RecalcWeaponParametersPatch().Enable();
    new AddRecoilForcePatch().Enable();
    new RecoilAnglesPatch().Enable();
    new ShootPatch().Enable();
    new RotatePatch().Enable();
  }

  private void LoadReloadPatches()
  {
    if (Plugin.ServerConfig.reload_changes)
    {
      new CanStartReloadPatch().Enable();
      new ReloadMagPatch().Enable();
      new QuickReloadMagPatch().Enable();
      new SetMagTypeCurrentPatch().Enable();
      new SetMagTypeNewPatch().Enable();
      new SetMagInWeaponPatch().Enable();
      new SetMalfRepairSpeedPatch().Enable();
      new BoltActionReloadPatch().Enable();
      new SetWeaponLevelPatch().Enable();
    }
    if (!Plugin.ServerConfig.reload_changes && !Plugin.ServerConfig.recoil_attachment_overhaul && !Plugin.ServerConfig.enable_stances)
      return;
    new ReloadWithAmmoPatch().Enable();
    new ReloadBarrelsPatch().Enable();
    new ReloadCylinderMagazinePatch().Enable();
    new OnMagInsertedPatch().Enable();
    new SetSpeedParametersPatch().Enable();
    new CheckAmmoPatch().Enable();
    new CheckChamberPatch().Enable();
    new RechamberPatch().Enable();
    new SetAnimatorAndProceduralValuesPatch().Enable();
    new AimPunchPatch().Enable();
  }

  private void LoadStancePatches()
  {
    new DoorAnimationOverride().Enable();
    new ChangeScopePatch().Enable();
    new DisableAimOnReloadPatch().Enable();
    new TacticalReloadPatch().Enable();
    new WeaponOverlapViewPatch().Enable();
    new CollisionPatch().Enable();
    new WeaponOverlappingPatch().Enable();
    new WeaponLengthPatch().Enable();
    new ApplySimpleRotationPatch().Enable();
    new InitTransformsPatch().Enable();
    new ZeroAdjustmentsPatch().Enable();
    new OnWeaponDrawPatch().Enable();
    new UpdateHipInaccuracyPatch().Enable();
    new SetFireModePatch().Enable();
    new OperateStationaryWeaponPatch().Enable();
    new SetTiltPatch().Enable();
    new BattleUIScreenPatch().Enable();
    new ChangePosePatch().Enable();
    new MountingAndCollisionPatch().Enable();
    new ShouldMoveWeapCloserPatch().Enable();
  }

  private void LoadMedicalPatches()
  {
    new SetQuickSlotPatch().Enable();
    new ApplyItemPatch().Enable();
    new BreathIsAudiblePatch().Enable();
    new SetMedsInHandsPatch().Enable();
    new ProceedMedsPatch().Enable();
    new RemoveEffectPatch().Enable();
    new StaminaRegenRatePatch().Enable();
    new HealthEffectsConstructorPatch().Enable();
    new HCApplyDamagePatch().Enable();
    new RestoreBodyPartPatch().Enable();
    new ToggleHeadDevicePatch().Enable();
    new HealCostDisplayShortPatch().Enable();
    new HealCostDisplayFullPatch().Enable();
    new HealthCanApplyItemBasePatch().Enable();
    new HealthHasPartsToApplyBasePatch().Enable();
    new TryGetPartsToApplyPatch().Enable();
    new MedsController2Patch().Enable();
    new Method8Patch().Enable();
    new MedEffectStartedPatch().Enable();
    new HealthControllerTryGetBodyPartToApplyPatch().Enable();
  }

  private void LoadMovementPatches()
  {
    if (PluginConfig.EnableMaterialSpeed.Value)
      new CalculateSurfacePatch().Enable();
    new ClampSpeedPatch().Enable();
    new SprintAccelerationPatch().Enable();
    new EnduranceSprintActionPatch().Enable();
    new EnduranceMovementActionPatch().Enable();
  }

  private void LoadDeafenPatches()
  {
    new PrismEffectsEnablePatch().Enable();
    new PrismEffectsDisablePatch().Enable();
    new UpdatePhonesPatch().Enable();
    new RegisterShotPatch().Enable();
    new ExplosionPatch().Enable();
    new GrenadeClassContusionPatch().Enable();
    new CovertMovementVolumePatch().Enable();
    new CovertMovementVolumeBySpeedPatch().Enable();
    new CovertEquipmentVolumePatch().Enable();
    new HeadsetConstructorPatch().Enable();
    new GunshotVolumePatch().Enable();
  }

  private void LoadBallisticsPatches()
  {
    new PenetrationUIPatch().Enable();
    new VelocityPatch().Enable();
    new CreateShotPatch().Enable();
    new ApplyArmorDamagePatch().Enable();
    new ApplyDamageInfoPatch().Enable();
    new SetPenetrationStatusPatch().Enable();
    new IsPenetratedPatch().Enable();
    new AfterPenPlatePatch().Enable();
    new IsShotDeflectedByHeavyArmorPatch().Enable();
    new ArmorLevelUIPatch().Enable();
    new ArmorLevelDisplayPatch().Enable();
    new ArmorClassStringPatch().Enable();
    new DamageInfoPatch().Enable();
    if (PluginConfig.EnableRagdollFix.Value)
      new ApplyCorpseImpulsePatch().Enable();
    new GetCachedReadonlyQualitiesPatch().Enable();
  }
}
