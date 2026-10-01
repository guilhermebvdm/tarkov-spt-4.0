using BepInEx;
using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.Ballistics;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using SimpleDeclutter.Patches;
using Koenigz.PerfectCulling.EFT;
using BepInEx.Logging;
using UnityEngine.SceneManagement;

namespace SimpleDeclutter
{
    [BepInPlugin("somtam.simple.declutter", "Simple Declutter", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ShadowQuality defaultShadows = QualitySettings.shadows;
        public static ManualLogSource LogSource;
        private static RaidSettings activeRaidSettings;
        private static List<GameObject> allGameObjectsList = new List<GameObject>();
        private static List<GameObject> savedClutterObjects = new List<GameObject>();
        internal static bool isOnMap = false;
        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("Declutter plugin loaded!");

            Settings.Init(Config);

            SceneManager.sceneUnloaded += OnSceneUnloaded;

            // Register the SettingChanged event
            Settings.declutterEnabledConfig.SettingChanged += OnApplyDeclutterSettingChanged;
            Settings.framesaverPotatoShadow.SettingChanged += OnApplyFrameSaverChanged;

            new RaidStartPatch().Enable();

            InitializeClutterNames();
        }
        private void OnApplyDeclutterSettingChanged(object sender, EventArgs e)
        {
            if (isOnMap) ApplyDeclutter();
        }
        private void OnApplyFrameSaverChanged(object sender, EventArgs e)
        {
            if (isOnMap) ApplyFrameSavers();
        }
        public static void ApplyDeclutter()
        {
            if (Settings.declutterEnabledConfig.Value && EnabledForMap())
            {
                LogSource.LogInfo($"Clutter removed");
                DeClutterEnabled();
            }
            else
            {
                LogSource.LogInfo($"Clutter enabled");
                ReClutterEnabled();
            }

        }
        public static void ApplyFrameSavers()
        {
            if (Settings.framesaverPotatoShadow.Value && EnabledForMap())
            {
                LogSource.LogInfo($"Shadows disabled");
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            else
            {
                LogSource.LogInfo($"Shadows enabled");
                QualitySettings.shadows = defaultShadows;
            }
        }
        private void OnSceneUnloaded(Scene scene)
        {
            allGameObjectsList.Clear();
            savedClutterObjects.Clear();
            isOnMap = false;
        }
        private static bool EnabledForMap()
        {
            // Is declutter enabled for this map?
            var session = (TarkovApplication)Singleton<ClientApplication<ISession>>.Instance;
            if (session == null) throw new Exception("No session!");
            activeRaidSettings = (RaidSettings)(typeof(TarkovApplication).GetField("_raidSettings", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(session));

            var enabled = false;
            switch (activeRaidSettings?.LocationId)
            {
                case "Lighthouse": if (Settings.EnableLighthouse.Value) enabled = true; break;
                case "Woods": if (Settings.EnableWoods.Value) enabled = true; break;
                case "factory4_night":
                case "factory4_day": if (Settings.EnableFactory.Value) enabled = true; break;
                case "bigmap": if (Settings.EnableCustoms.Value) enabled = true; break;
                case "RezervBase": if (Settings.EnableReserve.Value) enabled = true; break;
                case "Interchange": if (Settings.EnableInterchange.Value) enabled = true; break;
                case "TarkovStreets": if (Settings.EnableStreets.Value) enabled = true; break;
                case "Sandbox":
                case "Sandbox_high": if (Settings.EnableGroundZero.Value) enabled = true; break;
                case "Shoreline": if (Settings.EnableShoreline.Value) enabled = true; break;
                case "laboratory": if (Settings.EnableLab.Value) enabled = true; break;
                case null: break;
                default: break;
            }

            return enabled;
        }
        private static void DeClutterEnabled()
        {
            InitializeClutterNames(); // update values

            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == false) continue;

                // if (Settings.declutterUnscrutinizedEnabledConfig.Value == true)
                // {
                // obj.SetActive(false);
                // continue;
                // }

                bool foundClutterName = clutterNameDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()) && clutterNameDictionary[key]);
                if (foundClutterName)
                    obj.SetActive(false);
                else
                    obj.SetActive(true);

            }
        }
        private static void ReClutterEnabled()
        {
            foreach (GameObject obj in savedClutterObjects)
            {
                if (obj.activeSelf == false)
                {
                    obj.SetActive(true);
                }
            }
        }
        internal static IEnumerator GetValidDeclutterTargets()
        {
            // Loop until the coroutine has finished
            while (true)
            {
                if (allGameObjectsList != null && allGameObjectsList.Count > 0)
                {
                    // Coroutine has finished, and allGameObjectsList is populated
                    GameObject[] allGameObjectsArray = allGameObjectsList.ToArray();
                    foreach (GameObject obj in allGameObjectsArray)
                        if (obj != null) ShouldDisableObject(obj);
                }
                yield break;
            }
        }
        internal static IEnumerator GetAllGameObjectsInSceneCoroutine()
        {
            GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (GameObject obj in gameObjects)
            {
                bool isLODGroup = obj.GetComponent<LODGroup>() != null;
                bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
                bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
                bool isGoodThing = isLODGroup || isStaticDeferredDecal || isParticleSystem;

                if (Settings.declutterDecalsEnabledConfig.Value)
                {
                    isGoodThing = isLODGroup || isStaticDeferredDecal;
                }
                else
                {
                    isGoodThing = isLODGroup;
                }

                bool isTarkovContainer = obj.GetComponent<LootableContainer>() != null;
                bool isTarkovContainerGroup = obj.GetComponent<LootableContainersGroup>() != null;
                bool isTarkovObservedItem = obj.GetComponent<ObservedLootItem>() != null;
                bool isTarkovItem = obj.GetComponent<LootItem>() != null;
                bool isTarkovWeaponMod = obj.GetComponent<WeaponModPoolObject>() != null;
                bool hasRainCondensator = obj.GetComponent<RainCondensator>() != null;
                bool isLocalPlayer = obj.GetComponent<LocalPlayer>() != null;
                bool isPlayer = obj.GetComponent<Player>() != null;
                bool isBotOwner = obj.GetComponent<BotOwner>() != null;
                bool isCullingObject = obj.GetComponent<CullingObject>() != null;
                bool isCullingLightObject = obj.GetComponent<CullingLightObject>() != null;
                bool isCullingGroup = obj.GetComponent<CullingGroup>() != null;
                bool isDisablerCullingObject = obj.GetComponent<DisablerCullingObject>() != null;
                bool isObservedCullingManager = obj.GetComponent<ObservedCullingManager>() != null;
                bool isPerfectCullingCrossSceneGroup = obj.GetComponent<PerfectCullingCrossSceneGroup>() != null;
                bool isScreenDistanceSwitcher = obj.GetComponent<ScreenDistanceSwitcher>() != null;
                bool isBakedLodContent = obj.GetComponent<BakedLodContent>() != null;
                bool isGuidComponent = obj.GetComponent<GuidComponent>() != null;
                bool isOcclusionPortal = obj.GetComponent<OcclusionPortal>() != null;
                bool isMultisceneSharedOccluder = obj.GetComponent<MultisceneSharedOccluder>() != null;
                bool isWindowBreaker = obj.GetComponent<WindowBreaker>() != null;
                bool isBallisticCollider = obj.GetComponent<BallisticCollider>() != null;
                bool isBotSpawner = obj.GetComponent<BotSpawner>() != null;
                bool isBadThing = isTarkovContainer || isTarkovContainerGroup || isTarkovObservedItem || isTarkovItem || isTarkovWeaponMod ||
                                  hasRainCondensator || isLocalPlayer || isPlayer || isBotOwner || isCullingObject || isCullingLightObject ||
                                  isCullingGroup || isDisablerCullingObject || isObservedCullingManager || isPerfectCullingCrossSceneGroup ||
                                  isBakedLodContent || isScreenDistanceSwitcher || isGuidComponent || isOcclusionPortal || isBotSpawner ||
                                  isMultisceneSharedOccluder || isWindowBreaker || isBallisticCollider;

                if (isGoodThing && !isBadThing)
                {
                    allGameObjectsList.Add(obj);
                }
            }
            yield break;
        }
        private static Dictionary<string, bool> clutterNameDictionary = new Dictionary<string, bool>
        {
        };
        private static void InitializeClutterNames()
        {
            clutterNameDictionary["turniket_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["tray_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["electronic_box"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["styrofoam_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["polyethylene_set"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["penyok_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["trashbag_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["kaska"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["boot_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["garbage_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["garbage_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["garbage_stone"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["garbage_paper"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["cable"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["drawing_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["_paper"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper1"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper2"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper3"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper4"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper5"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper6"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper7"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper8"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["paper9"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan1"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan2"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan3"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan4"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan5"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan6"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan7"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan8"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pan9"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster1"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster2"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster3"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster4"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster5"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster6"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster7"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster8"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["poster9"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["_junk"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["junk_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["_trash"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["trash_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["cardboard_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["_cardboard"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["sticks"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["cloth_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["pants_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["shirt_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["dishes_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["cutlery_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["book_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["books_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["folder_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["folders_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["magazine_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["magazines_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["fuel_tube"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["city_garbage_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["city_road_garbage"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["reserve_garbage_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["reserve_road_garbage"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["garbage_parking_"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["goshan_garbage"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["package_garbage"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["wood_board"] = Settings.declutterGarbageEnabledConfig.Value;
            clutterNameDictionary["leaves_"] = Settings.declutterGarbageEnabledConfig.Value;

            clutterNameDictionary["trash_pile_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_trash_pile"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["crushed_concrete"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["crushed_concreate"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["baked_garbage"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["garbage"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["garbage_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_garbage"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["garbage_constructor"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_garb"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["garb_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_scrap"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["scrap_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["heap_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_heap"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_pile"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["pile_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_stuff"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_rubble"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["rubble_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["scatter_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_scatter"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["scattered_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_scattered"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["_floorset"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["floorset_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["brick_pile"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen01"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen02"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen03"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen04"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen05"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen06"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen07"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen08"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["poletelen09"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky1"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky2"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky1_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky2_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky3_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky4_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky5_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["vetky6_"] = Settings.declutterHeapsEnabledConfig.Value;
            clutterNameDictionary["tile_broken_"] = Settings.declutterHeapsEnabledConfig.Value;


            clutterNameDictionary["shotshell_"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["shells_"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["_shotshell"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["_shells"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["rifleshell_"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["_rifleshell"] = Settings.declutterSpentCartridgesEnabledConfig.Value;
            clutterNameDictionary["rifle_shells_"] = Settings.declutterSpentCartridgesEnabledConfig.Value;

            clutterNameDictionary["canned"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["canned_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["can_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["juice_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["carton_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["_creased"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["bottle"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["bottle_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["crackers_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["oat_flakes"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["chocolate_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["biscuits"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["package_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["cigarette_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["medkit_"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["_cup"] = Settings.declutterFakeFoodEnabledConfig.Value;
            clutterNameDictionary["plasticcup_"] = Settings.declutterFakeFoodEnabledConfig.Value;

            clutterNameDictionary["goshan_decal"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["ground_decal"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["decalgraffiti"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["blood_"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["_blood"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["sand_decal"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["decal_dirt"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["decal_drip"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["decal_"] = Settings.declutterDecalsEnabledConfig.Value;
            clutterNameDictionary["decals_"] = Settings.declutterDecalsEnabledConfig.Value;

            clutterNameDictionary["puddle"] = Settings.declutterPuddlesEnabledConfig.Value;
            clutterNameDictionary["puddles_"] = Settings.declutterPuddlesEnabledConfig.Value;
            clutterNameDictionary["_puddles"] = Settings.declutterPuddlesEnabledConfig.Value;
            clutterNameDictionary["puddle group"] = Settings.declutterPuddlesEnabledConfig.Value;

            clutterNameDictionary["_glass"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["brokenglass_"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["glass_crush"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["plite_crush"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["lesa_crush"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["shards_"] = Settings.declutterShardsEnabledConfig.Value;
            clutterNameDictionary["_shards"] = Settings.declutterShardsEnabledConfig.Value;

        }
        private static Dictionary<string, bool> dontDisableDictionary = new Dictionary<string, bool>
        {
            { "item_", true },
            { "weapon_", true },
            { "barter_", true },
            { "mod_", true },
            { "audio", true },
            { "container", true },
            { "trigger", true },
            { "culling", true },
            { "collider", true },
            { "colider", true },
            { "group", true },
            { "manager", true },
            { "scene", true },
            { "player", true },
            { "portal", true },
            { "bakelod", true },
            { "door", true },
            { "shadow", true },
            { "mine", true }
        };
        private static bool ShouldDisableObject(GameObject obj)
        {
            if (obj == null)
            {
                // Handle the case when obj is null for whatever reason.
                return false;
            }

            bool isStaticDeferredDecal = obj.GetComponent<StaticDeferredDecal>() != null;
            bool isParticleSystem = obj.GetComponent<ParticleSystem>() != null;
            bool isGoodThing = isStaticDeferredDecal || isParticleSystem;
            GameObject childGameMeshObject = null;
            GameObject childGameColliderObject = null;
            bool childHasMesh = false;
            float sizeOnY = 3f;
            bool childHasCollider = false;
            bool foundClutterName = false;
            bool dontDisableName = dontDisableDictionary.Keys.Any(key => obj.name.ToLower().Contains(key.ToLower()));
            //EFT.UI.ConsoleScreen.LogError("Found Lod Group " + obj.name);
            // if (declutterUnscrutinizedEnabledConfig.Value == true)
            // {
            //     foundClutterName = true;
            // }
            // else
            // {
            foundClutterName = clutterNameDictionary.Keys.Any(key =>
            {
                return obj.name.ToLower().Contains(key.ToLower());
            });


            // }
            if (foundClutterName && !dontDisableName)
            {
                //EFT.UI.ConsoleScreen.LogError("Found Clutter Name" + obj.name);
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;
                    bool isTarkovContainer = childGameMeshObject.GetComponent<LootableContainer>() != null;
                    bool isTarkovContainerGroup = childGameMeshObject.GetComponent<LootableContainersGroup>() != null;
                    bool isTarkovObservedItem = childGameMeshObject.GetComponent<ObservedLootItem>() != null;
                    bool isTarkovItem = childGameMeshObject.GetComponent<LootItem>() != null;
                    bool isTarkovWeaponMod = childGameMeshObject.GetComponent<WeaponModPoolObject>() != null;
                    bool hasRainCondensator = childGameMeshObject.GetComponent<RainCondensator>() != null;
                    bool isLocalPlayer = childGameMeshObject.GetComponent<LocalPlayer>() != null;
                    bool isPlayer = childGameMeshObject.GetComponent<Player>() != null;
                    bool isBotOwner = childGameMeshObject.GetComponent<BotOwner>() != null;
                    bool isCullingObject = childGameMeshObject.GetComponent<CullingObject>() != null;
                    bool isCullingLightObject = childGameMeshObject.GetComponent<CullingLightObject>() != null;
                    bool isCullingGroup = childGameMeshObject.GetComponent<CullingGroup>() != null;
                    bool isDisablerCullingObject = childGameMeshObject.GetComponent<DisablerCullingObject>() != null;
                    bool isObservedCullingManager = childGameMeshObject.GetComponent<ObservedCullingManager>() != null;
                    bool isPerfectCullingCrossSceneGroup = childGameMeshObject.GetComponent<PerfectCullingCrossSceneGroup>() != null;
                    bool isScreenDistanceSwitcher = childGameMeshObject.GetComponent<ScreenDistanceSwitcher>() != null;
                    bool isBakedLodContent = childGameMeshObject.GetComponent<BakedLodContent>() != null;
                    bool isGuidComponent = childGameMeshObject.GetComponent<GuidComponent>() != null;
                    bool isOcclusionPortal = childGameMeshObject.GetComponent<OcclusionPortal>() != null;
                    bool isMultisceneSharedOccluder = childGameMeshObject.GetComponent<MultisceneSharedOccluder>() != null;
                    bool isWindowBreaker = childGameMeshObject.GetComponent<WindowBreaker>() != null;
                    bool isBotSpawner = childGameMeshObject.GetComponent<BotSpawner>() != null;
                    bool isBadThing = isTarkovContainer || isTarkovContainerGroup || isTarkovObservedItem || isTarkovItem || isTarkovWeaponMod ||
                                      hasRainCondensator || isLocalPlayer || isPlayer || isBotOwner || isCullingObject || isCullingLightObject ||
                                      isCullingGroup || isDisablerCullingObject || isObservedCullingManager || isPerfectCullingCrossSceneGroup ||
                                      isBakedLodContent || isScreenDistanceSwitcher || isGuidComponent || isOcclusionPortal || isBotSpawner ||
                                      isMultisceneSharedOccluder || isWindowBreaker;
                    if (isBadThing)
                    {
                        return false;
                    }
                }
                foreach (Transform child in obj.transform)
                {
                    childGameMeshObject = child.gameObject;
                    if (child.GetComponent<MeshRenderer>() != null && !childGameMeshObject.name.ToLower().Contains("shadow") && !childGameMeshObject.name.ToLower().StartsWith("col") && !childGameMeshObject.name.ToLower().EndsWith("der"))
                    {
                        childHasMesh = true;
                        // Exit the loop since we've found what we need
                        break;
                    }
                }
                if (!childHasMesh && !isGoodThing)
                {
                    return false;
                }
                foreach (Transform child in obj.transform)
                {
                    if ((child.GetComponent<MeshCollider>() != null || child.GetComponent<BoxCollider>() != null) && child.GetComponent<BallisticCollider>() == null)
                    {
                        childGameColliderObject = child.gameObject;
                        if (childGameColliderObject != null && childGameColliderObject.activeSelf)
                        {
                            childHasCollider = true;
                            // Exit the loop since we've found what we need
                            break;
                        }
                    }
                }
                if (isGoodThing)
                {
                    sizeOnY = 0.1f;
                }
                else if (childHasMesh)
                {
                    sizeOnY = GetMeshSizeOnY(childGameMeshObject);
                }
                else
                {
                    return false;
                }
                if ((childHasMesh || isGoodThing) && (!childHasCollider || isGoodThing) && sizeOnY <= 2f * Settings.declutterScaleOffsetConfig.Value)
                {
                    savedClutterObjects.Add(obj);
                    return true;
                }
            }
            // else
            // {
            //     Plugin.LogSource.LogInfo($"NFI Object ${obj.name.ToLower()}");
            // }

            return false;

        }
        private static float GetMeshSizeOnY(GameObject childGameObject)
        {
            MeshRenderer meshRenderer = childGameObject?.GetComponent<MeshRenderer>();
            if (meshRenderer != null && meshRenderer.enabled)
            {
                Bounds bounds = meshRenderer.bounds;
                return bounds.size.y;
            }
            return 0.0f;
        }
    }
}
