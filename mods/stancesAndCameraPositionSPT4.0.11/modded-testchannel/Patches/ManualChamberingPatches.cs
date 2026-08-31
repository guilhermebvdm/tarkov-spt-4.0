using System;
using System.Linq;
using System.Reflection;
using EFT;
using EFT.Animations;
using EFT.InputSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using Comfort.Common;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    using ReloadWeaponClass = EFT.Player.FirearmController.GClass2016;

    public static class ManualChamberingState
    {
        public static bool BlockChambering = false;

        // Default FALSE — alinhado ao RealismMod.
        // Vira `true` apenas via RechamberRound (puxar ferrolho manual) ou quando a feature está desligada.
        public static bool CanLoadChamber = false;

        // Flag de "raid recém-iniciada" — setada no GameWorldOnGameStartedPatch
        public static bool JustSpawned = false;
        public static bool AllowVanillaChamberUnload = false;

        public static void Reset()
        {
            BlockChambering = false;
            CanLoadChamber = false;
            JustSpawned = false;
            AllowVanillaChamberUnload = false;
        }
    }

    public class ManualChamberingComponent : MonoBehaviour
    {
        public Player.FirearmController FirearmController;
        public WeaponManagerClass WeaponStateClass;
        public AmmoItemClass Bullet;
        public GamePlayerOwner GamePlayerOwner;
        public float Timer = 0f;
        public int Phase = 0;

        void Update()
        {
            if (Phase == 0) return;

            Timer += Time.deltaTime;

            // Phase 1: Esperar a arma ir pro centro (Stance 0) antes de rodar a animação
            if (Phase == 1 && Timer >= 0.2f)
            {
                if (WeaponStateClass != null && Bullet != null && FirearmController != null && FirearmController.FirearmsAnimator != null)
                {
                    Plugin.Logger.LogInfo("[ManualChamber] Stance alcançada. Executando animação e estado.");
                    WeaponStateClass.RemoveAllShells();
                    FirearmController.FirearmsAnimator.SetAmmoInChamber(1f);
                    FirearmController.FirearmsAnimator.SetAmmoOnMag(FirearmController.Weapon.GetCurrentMagazineCount());
                    WeaponStateClass.SetRoundIntoWeapon(Bullet, 0);
                    FirearmController.FirearmsAnimator.Rechamber(true);
                }
                Phase = 2;
                Timer = 0f;
            }
            // Phase 2: Parar o trigger da animação
            else if (Phase == 2 && Timer >= 0.5f)
            {
                if (FirearmController != null && FirearmController.FirearmsAnimator != null)
                {
                    FirearmController.FirearmsAnimator.Rechamber(false);
                }
                Phase = 0;
                Timer = 0f;
            }
        }
    }

    public class StartReloadResetPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ReloadWeaponClass).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
                m.Name == "Start"
                && m.GetParameters().Length == 2
                && m.GetParameters()[1].ParameterType == typeof(Callback));
        }

        [PatchPrefix]
        private static void Prefix(ReloadWeaponClass __instance)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return;

                var fc = __instance.FirearmController_0;
                if (fc == null || fc.Weapon == null) return;

                var player = Traverse.Create(fc).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                ManualChamberingState.CanLoadChamber = true;
                ManualChamberingState.BlockChambering = false;
                Plugin.Logger.LogDebug("[ManualChamber] Reload start: flags resetadas (CanLoad=true, Block=false)");
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] StartReloadReset {ex.Message}"); }
        }
    }

    public class SetAmmoCompatiblePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(FirearmsAnimator), nameof(FirearmsAnimator.SetAmmoCompatible));

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, ref bool compatible)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return;
                if (ManualChamberingState.CanLoadChamber) return;

                var mainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer == null || !(mainPlayer.HandsController is Player.FirearmController fc) || fc.FirearmsAnimator != __instance) return;

                // Bloqueia a compatibilidade para impedir que o EFT faça auto-chambering de forma invasiva no spawn/equip
                if (fc.Weapon != null && fc.Weapon.HasChambers && fc.Weapon.ChamberAmmoCount == 0)
                {
                    compatible = false;
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] SetAmmoCompatible {ex.Message}"); }
        }
    }

    public class SetAmmoOnMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(FirearmsAnimator), nameof(FirearmsAnimator.SetAmmoOnMag));

        [PatchPrefix]
        private static bool Prefix(FirearmsAnimator __instance)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return true;

                var mainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer == null || !(mainPlayer.HandsController is Player.FirearmController fc) || fc.FirearmsAnimator != __instance) return true;

                if (ManualChamberingState.BlockChambering)
                {
                    ManualChamberingState.BlockChambering = false;
                    return false;
                }
                return true;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] SetAmmoOnMag {ex.Message}"); return true; }
        }
    }

    public class PreChamberLoadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(Player.FirearmController).GetMethod("method_18", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return;
                if (!Plugin._ManualChamberingOnReload.Value) return;

                var player = Traverse.Create(__instance).Field<Player>("_player").Value;
                if (player == null || !player.IsYourPlayer) return;

                if (__instance.Weapon != null && __instance.Weapon.HasChambers && __instance.Weapon.Chambers.Length == 1 && __instance.Weapon.ChamberAmmoCount == 0 && !__instance.IsStationaryWeapon)
                {
                    ManualChamberingState.BlockChambering = true;
                    Plugin.Logger.LogDebug("[ManualChamber] method_18 (auto-chamber): BlockChambering=true");
                }
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] PreChamberLoad {ex.Message}"); }
        }
    }

    public class ManualChamberingInputPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GamePlayerOwner), nameof(GamePlayerOwner.TranslateCommand));
        }

        private static void RechamberRound(Player.FirearmController fc, Player player)
        {
            ManualChamberingState.CanLoadChamber = true;
            ManualChamberingState.BlockChambering = false;

            int currentMagazineCount = fc.Weapon.GetCurrentMagazineCount();
            MagazineItemClass mag = fc.Weapon.GetCurrentMagazine();
            if (mag == null) return;

            fc.FirearmsAnimator.SetAmmoInChamber(0f);
            fc.FirearmsAnimator.SetAmmoOnMag(currentMagazineCount);
            fc.FirearmsAnimator.SetAmmoCompatible(true);

            GStruct154<GInterface424> gstruct = mag.Cartridges.PopTo(player.InventoryController, fc.Item.Chambers[0].CreateItemAddress());
            WeaponManagerClass weaponStateClass = Traverse.Create(fc).Field("weaponManagerClass").GetValue<WeaponManagerClass>();

            if (weaponStateClass != null && gstruct.Value != null)
            {
                Plugin.Logger.LogInfo("[ManualChamber] RechamberRound Iniciado: Aguardando transição de stance.");
                StanceManager.StartActionStance();

                var comp = player.gameObject.GetOrAddComponent<ManualChamberingComponent>();
                comp.FirearmController = fc;
                comp.WeaponStateClass = weaponStateClass;
                comp.Bullet = (AmmoItemClass)gstruct.Value.ResultItem;
                comp.Phase = 1;
                comp.Timer = 0f;
            }
        }

        [PatchPrefix]
        private static bool Prefix(GamePlayerOwner __instance, ECommand command)
        {
            try
            {
                if (!Plugin._EnableManualChambering.Value) return true;

                if (command == ECommand.UnloadMagazine)
                {
                    var player = __instance.Player;
                    if (player != null && player.IsYourPlayer)
                    {
                        StanceManager.StartActionStance();
                    }
                }

                if (command == ECommand.ChamberUnload)
                {
                    var player = __instance.Player;
                    if (player == null || !player.IsYourPlayer) return true;

                    var fc = player.HandsController as Player.FirearmController;
                    if (fc == null || fc.Weapon == null) return true;

                    if (fc.Weapon.HasChambers && fc.Weapon.Chambers.Length == 1 && fc.Weapon.ChamberAmmoCount == 0)
                    {
                        var mag = fc.Weapon.GetCurrentMagazine();
                        if (mag != null && mag.Count > 0)
                        {
                            if (fc.IsAiming) fc.ToggleAim();
                            RechamberRound(fc, player);
                            return false; 
                        }
                    }
                    else if (fc.Weapon.HasChambers && fc.Weapon.Chambers.Length == 1 && fc.Weapon.ChamberAmmoCount > 0)
                    {
                        // Esvaziamento nativo de câmara: forçar Stance 0 e permitir execução imediata pelo pipeline do EFT
                        StanceManager.StartActionStance();
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex) { Plugin.Logger.LogError($"[ManualChamber] ChamberInput {ex.Message}"); return true; }
        }
    }
}

