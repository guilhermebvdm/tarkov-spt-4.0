using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Manimal.LoadAmmoAnim.CustomEFTData;
using Manimal.LoadAmmoAnim.Patches;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

// the Fika compat assembly is a separate DLL that hard-refs Fika.Core. it lives
// next to this one and gets sideloaded at Awake if Fika is installed. give it
// access to our internals so it can drive the per-player session state directly.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LoadAmmoAnimClientFika")]

namespace Manimal.LoadAmmoAnim
{
    [BepInPlugin(BuildInfo.ModGuid, "Manimal-LoadAmmoAnim", BuildInfo.Version)]
    // soft-dep on CLA so we load AFTER it. our plugin's GUID sorts alphabetically
    // before CLA's, which means without this hint BepInEx loads us first — and our
    // Awake-time IsInstalled check would then miss CLA in the plugin registry,
    // skipping the compat patches and breaking chained-mag loading.
    [BepInDependency("com.ozen.continuousloadammo", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private const string FikaGuid = "com.fika.core";
        private const string FikaCompatAssemblyFileName = "LoadAmmoAnimClientFika.dll";
        private const string FikaCompatTypeName = "Manimal.LoadAmmoAnim.Fika.FikaCompatModule";

        public static ManualLogSource LogSource;

        public static ConfigEntry<float> MagOffsetX { get; private set; }
        public static ConfigEntry<float> MagOffsetY { get; private set; }
        public static ConfigEntry<float> MagOffsetZ { get; private set; }
        public static ConfigEntry<float> MagRotX { get; private set; }
        public static ConfigEntry<float> MagRotY { get; private set; }
        public static ConfigEntry<float> MagRotZ { get; private set; }

        public static ConfigEntry<float> BulletOffsetX { get; private set; }
        public static ConfigEntry<float> BulletOffsetY { get; private set; }
        public static ConfigEntry<float> BulletOffsetZ { get; private set; }
        public static ConfigEntry<float> BulletRotX { get; private set; }
        public static ConfigEntry<float> BulletRotY { get; private set; }
        public static ConfigEntry<float> BulletRotZ { get; private set; }
        public static ConfigEntry<float> BulletHideStart { get; private set; }
        public static ConfigEntry<float> BulletHideEnd { get; private set; }

        // Animation speed multipliers
        public static ConfigEntry<float> LoadingSpeedMultiplier { get; private set; }
        public static ConfigEntry<float> UnloadingSpeedMultiplier { get; private set; }

        // Calibration tools
        public static ConfigEntry<KeyboardShortcut> SaveOffsetHotkey { get; private set; }

        private void Awake()
        {
            LogSource = Logger;
            LogSource.LogInfo("LoadAmmoAnim loaded!");

            InitConfiguration();

            // Initialize persistent offset store (offsets.json) and banned magazine store (BanAnimation.json) next to this DLL.
            string pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            OffsetFileStore.Initialize(pluginDir);
            BanAnimationStore.Initialize(pluginDir);

            // Class1204 hooks — detect mag-loading sessions + extend the first-bullet
            // delay to fit the bundle's draw clip. driver-side, unrelated to controller dispatch.
            new LoadAmmoAnimDetectPatch().Enable();
            new Class1204DrawDelayPatch().Enable();
            new Class1207UnloadDelayPatch().Enable();
            new RaidStartBundleWarmPatch().Enable();

            // dispatch patches. route LoadAmmoBundleItem through LoadAmmoBundleController
            // instead of the engine's default usable-item handlers. mirror of HackerMod's
            // four-patch shim for custom items.
            new SetInHandsBundlePatch().Enable();
            new ClientUsableBundleSmethod11Patch().Enable();
            new HandsControllerAnimationTypeBundlePatch().Enable();
            new UsableBundleInterfaceDispatchPatch().Enable();

            // defensive PWA guards — there's still a brief window during controller
            // teardown/respawn where WeaponRootAnim can be null/destroyed before the
            // new firearm controllers smethod_8 rebinds. these silence the NRE storm
            // for those frames.
            new ProcessEffectorsNullGuardPatch().Enable();
            new VisualPassNullGuardPatch().Enable();

            if (ContinuousLoadAmmoCompat.IsInstalled)
                ContinuousLoadAmmoCompat.EnablePatches();

            // sideload the Fika compat assembly only when Fika is installed. doing
            // it via reflection means the main DLL never resolves Fika.Core types,
            // so non-Fika setups load cleanly with no missing-dependency errors.
            if (Chainloader.PluginInfos.ContainsKey(FikaGuid))
                LoadFikaCompat();
        }

        private void InitConfiguration()
        {
            MagOffsetX = Config.Bind("1. Magazine Alignment (F12)", "Mag Position X (meters)", 0f,
                new ConfigDescription("Left (-) / Right (+) offset of the magazine in hands", new AcceptableValueRange<float>(-1f, 1f)));
            MagOffsetY = Config.Bind("1. Magazine Alignment (F12)", "Mag Position Y (meters)", 0f,
                new ConfigDescription("Down (-) / Up (+) offset of the magazine in hands", new AcceptableValueRange<float>(-1f, 1f)));
            MagOffsetZ = Config.Bind("1. Magazine Alignment (F12)", "Mag Position Z (meters)", 0f,
                new ConfigDescription("Back (-) / Forward (+) offset of the magazine in hands", new AcceptableValueRange<float>(-1f, 1f)));
            MagRotX = Config.Bind("1. Magazine Alignment (F12)", "Mag Rotation X (deg)", 0f,
                new ConfigDescription("Pitch rotation of the magazine", new AcceptableValueRange<float>(-180f, 180f)));
            MagRotY = Config.Bind("1. Magazine Alignment (F12)", "Mag Rotation Y (deg)", 0f,
                new ConfigDescription("Yaw rotation of the magazine", new AcceptableValueRange<float>(-180f, 180f)));
            MagRotZ = Config.Bind("1. Magazine Alignment (F12)", "Mag Rotation Z (deg)", 0f,
                new ConfigDescription("Roll rotation of the magazine", new AcceptableValueRange<float>(-180f, 180f)));

            BulletOffsetX = Config.Bind("2. Bullet Alignment (F12)", "Bullet Position X (meters)", 0f,
                new ConfigDescription("Left (-) / Right (+) offset of the bullet in fingers", new AcceptableValueRange<float>(-1f, 1f)));
            BulletOffsetY = Config.Bind("2. Bullet Alignment (F12)", "Bullet Position Y (meters)", 0f,
                new ConfigDescription("Down (-) / Up (+) offset of the bullet in fingers", new AcceptableValueRange<float>(-1f, 1f)));
            BulletOffsetZ = Config.Bind("2. Bullet Alignment (F12)", "Bullet Position Z (meters)", 0f,
                new ConfigDescription("Back (-) / Forward (+) offset of the bullet in fingers", new AcceptableValueRange<float>(-1f, 1f)));
            BulletRotX = Config.Bind("2. Bullet Alignment (F12)", "Bullet Rotation X (deg)", 0f,
                new ConfigDescription("Pitch rotation of the bullet", new AcceptableValueRange<float>(-180f, 180f)));
            BulletRotY = Config.Bind("2. Bullet Alignment (F12)", "Bullet Rotation Y (deg)", 0f,
                new ConfigDescription("Yaw rotation of the bullet", new AcceptableValueRange<float>(-180f, 180f)));
            BulletRotZ = Config.Bind("2. Bullet Alignment (F12)", "Bullet Rotation Z (deg)", 0f,
                new ConfigDescription("Roll rotation of the bullet", new AcceptableValueRange<float>(-180f, 180f)));

            BulletHideStart = Config.Bind("2. Bullet Alignment (F12)", "Bullet Hide Start (Cycle %)", 0.60f,
                new ConfigDescription(
                    "Point in the loading cycle (0.0 to 1.0) when the bullet disappears into the magazine.",
                    new AcceptableValueRange<float>(0f, 1f)));

            BulletHideEnd = Config.Bind("2. Bullet Alignment (F12)", "Bullet Hide End / Reappear (Cycle %)", 0.75f,
                new ConfigDescription(
                    "Point in the loading cycle (0.0 to 1.0) when the next bullet reappears in the hand.",
                    new AcceptableValueRange<float>(0f, 1f)));

            LoadingSpeedMultiplier = Config.Bind("3. Animation Speed (F12)", "Loading Speed Multiplier", 1f,
                new ConfigDescription(
                    "Multiplier applied to the loading animation speed. " +
                    "< 1 = slower (more time to calibrate magazine position). > 1 = faster.",
                    new AcceptableValueRange<float>(0.1f, 5f)));

            UnloadingSpeedMultiplier = Config.Bind("3. Animation Speed (F12)", "Unloading Speed Multiplier", 1f,
                new ConfigDescription(
                    "Multiplier applied to the unloading speed (draining a magazine). " +
                    "> 1 = empties magazine faster. < 1 = slower.",
                    new AcceptableValueRange<float>(0.1f, 10f)));

            SaveOffsetHotkey = Config.Bind("4. Calibration Tools (F12)", "Save Current Magazine Offset",
                new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl),
                "While the loading animation is active or holding the weapon, press this hotkey to save the current " +
                "magazine's position/rotation (including F12 slider deltas) to offsets.json.");
        }

        private void Update()
        {
            if (SaveOffsetHotkey != null && (SaveOffsetHotkey.Value.IsDown() || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S))))
            {
                SaveCurrentActiveOffset();
            }
        }

        public static void SaveCurrentActiveOffset()
        {
            try
            {
                var player = Singleton<GameWorld>.Instance?.MainPlayer;
                if (player == null)
                {
                    LogSource?.LogWarning("[LoadAmmoAnim] Cannot save offset: Player is null (not in raid).");
                    return;
                }

                var session = LoadAmmoAnimState.TryGet(player);
                string magTpl = session?.CurrentMagTemplateId;
                var mag = session?.CurrentMag;

                // If session mag is null (e.g. anim ended), fallback to firearm in hands
                if (string.IsNullOrEmpty(magTpl) && player.HandsController is Player.FirearmController firearm)
                {
                    var equippedMag = firearm.Item?.GetCurrentMagazine();
                    if (equippedMag != null)
                    {
                        mag = equippedMag;
                        magTpl = equippedMag.TemplateId;
                    }
                }

                if (string.IsNullOrEmpty(magTpl))
                {
                    NotificationManagerClass.DisplayMessageNotification("LoadAmmoAnim: Nenhum carregador ativo detectado para salvar!");
                    LogSource?.LogWarning("[LoadAmmoAnim] No magazine detected to save offset.");
                    return;
                }

                string caliber = (mag?.Cartridges?.Items_1?.FirstOrDefault() as AmmoItemClass)?.Caliber;
                OffsetData baseOffset = MagOffsetRegistry.GetOffset(caliber, magTpl, mag);

                var absoluteData = new OffsetData
                {
                    MagPosition = baseOffset.MagPosition + new Vector3(
                        MagOffsetX?.Value ?? 0f,
                        MagOffsetY?.Value ?? 0f,
                        MagOffsetZ?.Value ?? 0f),
                    MagRotation = baseOffset.MagRotation * Quaternion.Euler(
                        MagRotX?.Value ?? 0f,
                        MagRotY?.Value ?? 0f,
                        MagRotZ?.Value ?? 0f),
                    BulletPosition = baseOffset.BulletPosition + new Vector3(
                        BulletOffsetX?.Value ?? 0f,
                        BulletOffsetY?.Value ?? 0f,
                        BulletOffsetZ?.Value ?? 0f),
                    BulletRotation = baseOffset.BulletRotation * Quaternion.Euler(
                        BulletRotX?.Value ?? 0f,
                        BulletRotY?.Value ?? 0f,
                        BulletRotZ?.Value ?? 0f),
                };

                string name = mag?.Template?.Name ?? magTpl;
                OffsetFileStore.Save(magTpl, name, absoluteData);

                string msg = $"LoadAmmoAnim: Salvo offset para '{name}' ({magTpl}) no offsets.json!";
                LogSource?.LogInfo("[LoadAmmoAnim] " + msg);
                NotificationManagerClass.DisplayMessageNotification(msg);
            }
            catch (Exception ex)
            {
                LogSource?.LogError($"[LoadAmmoAnim] SaveCurrentActiveOffset threw: {ex.Message}");
            }
        }

        private static void LoadFikaCompat()
        {
            try
            {
                var here = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var dllPath = Path.Combine(here, FikaCompatAssemblyFileName);
                if (!File.Exists(dllPath))
                {
                    LogSource.LogWarning(
                        $"[LoadAmmoAnim] Fika detected but compat DLL missing at {dllPath}. " +
                        "multiplayer animations wont sync. reinstall the mod to restore the compat assembly.");
                    return;
                }

                var asm = Assembly.LoadFrom(dllPath);
                var type = asm.GetType(FikaCompatTypeName);
                if (type == null)
                {
                    LogSource.LogError($"[LoadAmmoAnim] couldnt find {FikaCompatTypeName} in {FikaCompatAssemblyFileName}.");
                    return;
                }

                var enable = type.GetMethod("Enable", BindingFlags.Public | BindingFlags.Static);
                if (enable == null)
                {
                    LogSource.LogError($"[LoadAmmoAnim] {FikaCompatTypeName}.Enable() not found.");
                    return;
                }

                enable.Invoke(null, null);
            }
            catch (Exception ex)
            {
                LogSource.LogError($"[LoadAmmoAnim] Fika compat load failed: {ex}");
            }
        }
    }
}
