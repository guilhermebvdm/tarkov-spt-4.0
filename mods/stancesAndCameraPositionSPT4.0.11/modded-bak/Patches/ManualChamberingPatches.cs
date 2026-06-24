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
    using ChamberWeaponClass = EFT.Player.FirearmController.GClass2055;
    using ReloadWeaponClass = EFT.Player.FirearmController.GClass2016;

    public static class ManualChamberingState
    {
        public static bool BlockChambering = false;

        // Default FALSE — alinhado ao RealismMod (Plugin.cs:49). Com `true`, o StartEquipWeapPatch
        // carregava a primeira bala automaticamente no equip/spawn (sintoma "ainda chambera sozinho").
        // Vira `true` apenas via RechamberRound (puxar ferrolho manual) ou quando a feature/cenário
        // está desligado (comportamento vanilla).
        public static bool CanLoadChamber = false;

        // Flag de "raid recém-iniciada" — setada no GameWorldOnGameStartedPatch e consumida pelo
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
            // Phase 3: Esperar 0.2s pra chamar o Vanilla ChamberUnload
            else if (Phase == 3 && Timer >= 0.2f)
            {
                if (GamePlayerOwner != null)
                {
                    ManualChamberingState.AllowVanillaChamberUnload = true;
                    GamePlayerOwner.TranslateCommand(ECommand.ChamberUnload);
                }
                Phase = 0;
                Timer = 0f;
            }
        }
    }

    public class StartEquipWeapPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(ChamberWeaponClass).GetMethods(BindingFlags.Public | BindingFlags.Instance).First(m =>
                m.GetParameters().Length == 1
                && m.GetParameters()[0].Name == "onWeaponAppear");
        }

        [PatchPrefix]
        private static bool Prefix(ChamberWeaponClass __instance, Action onWeaponAppear)
        {
            if (!Plugin._EnableManualChambering.Value) return true;

            var fc = __instance.FirearmController_0;
            if (fc == null || fc.Weapon == null) return true;

            var player = Traverse.Create(fc).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer || player.MovementContext == null || player.MovementContext.CurrentState == null || player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return true;

            int chamberAmmoCount = __instance.Weapon_0.ChamberAmmoCount;
            bool isSpawnEquip = ManualChamberingState.JustSpawned;
            ManualChamberingState.JustSpawned = false;

            if (__instance.Weapon_0.HasChambers && chamberAmmoCount == 0)
            {
                bool blockThisEquip = isSpawnEquip ? Plugin._ManualChamberingOnRaidStart.Value : true;
                if (blockThisEquip)
                {
                    ManualChamberingState.CanLoadChamber = false;
                    ManualChamberingState.BlockChambering = true;
                    Plugin.Logger.LogDebug($"[ManualChamber] Equip vazio: auto-chamber BLOQUEADO (spawn={isSpawnEquip})");
                }
            }
            else
            {
                ManualChamberingState.CanLoadChamber = true;
                ManualChamberingState.BlockChambering = false;
            }

            __instance.Action_0 = onWeaponAppear;
            __instance.Start();
            __instance.FirearmsAnimator_0.SetActiveParam(active: true);
            __instance.FirearmsAnimator_0.SetLayerWeight(__instance.FirearmsAnimator_0.LACTIONS_LAYER_INDEX, 0);
            __instance.Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.WEAPON_SIZE_MODIFIER_PARAM_HASH, __instance.Weapon_0.CalculateCellSize().X);
            
            int currentMagazineCount = __instance.Weapon_0.GetCurrentMagazineCount();
            __instance.MagazineItemClass = __instance.Weapon_0.GetCurrentMagazine();
            __instance.FirearmController_0.AmmoInChamberOnSpawn = chamberAmmoCount;
            if (__instance.Weapon_0.HasChambers)
            {
                int animChamberCount = (ManualChamberingState.BlockChambering && chamberAmmoCount == 0) ? 1 : chamberAmmoCount;
                __instance.FirearmsAnimator_0.SetAmmoInChamber(animChamberCount);
            }
            else
            {
                __instance.FirearmsAnimator_0.SetHammerArmed(__instance.Weapon_0.Armed);
            }
            if (__instance.Weapon_0.GetCurrentMagazine() is CylinderMagazineItemClass cylinderMagazineItemClass)
            {
                bool hammerArmed = !__instance.Weapon_0.CylinderHammerClosed;
                __instance.FirearmsAnimator_0.SetHammerArmed(hammerArmed);
                __instance.FirearmsAnimator_0.SetCamoraIndex(cylinderMagazineItemClass.CurrentCamoraIndex);
                for (int i = 0; i < cylinderMagazineItemClass.Count; i++)
                {
                    if (cylinderMagazineItemClass.Camoras[i].ContainedItem != null)
                    {
                        __instance.Weapon_0.ShellsInChambers[i] = null;
                        __instance.WeaponManagerClass.RemoveShellInWeapon(i);
                    }
                }
            }
            if (__instance.Weapon_0.IsMultiBarrel)
            {
                for (int j = 0; j < __instance.Weapon_0.Chambers.Length; j++)
                {
                    if (__instance.Weapon_0.Chambers[j].ContainedItem != null)
                    {
                        __instance.Weapon_0.ShellsInChambers[j] = null;
                        __instance.WeaponManagerClass.RemoveShellInWeapon(j);
                    }
                }
            }
            __instance.FirearmsAnimator_0.SetAmmoOnMag(currentMagazineCount);
            __instance.Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
            __instance.Player_0.Skills.OnWeaponDraw(__instance.Weapon_0);
            
            bool compatible = (__instance.Bool_1 = __instance.MagazineItemClass == null || __instance.MagazineItemClass.IsAmmoCompatible(__instance.Weapon_0.Chambers));
            __instance.FirearmsAnimator_0.SetAmmoCompatible(compatible);
            if (__instance.Bool_1 && __instance.MagazineItemClass != null && __instance.MagazineItemClass.Count > 0 && __instance.FirearmController_0.Item.Chambers.Length != 0 && __instance.Weapon_0.MalfState.State == Weapon.EMalfunctionState.Misfire)
            {
                __instance.FirearmsAnimator_0.SetLayerWeight(__instance.FirearmsAnimator_0.MALFUNCTION_LAYER_INDEX, 0);
            }
            
            if (ManualChamberingState.CanLoadChamber && __instance.MagazineItemClass != null && chamberAmmoCount == 0 && currentMagazineCount > 0 && compatible && __instance.FirearmController_0.Item.Chambers.Length != 0)
            {
                Weapon.EMalfunctionState state = __instance.FirearmController_0.Item.MalfState.State;
                if (state == Weapon.EMalfunctionState.Misfire)
                {
                    __instance.FirearmController_0.Item.MalfState.ChangeStateSilent(Weapon.EMalfunctionState.None);
                }
                GStruct154<GInterface424> gStruct = __instance.MagazineItemClass.Cartridges.PopTo(player.InventoryController, __instance.FirearmController_0.Item.Chambers[0].CreateItemAddress());
                __instance.FirearmController_0.Item.MalfState.ChangeStateSilent(state);
                if (gStruct.Value != null)
                {
                    __instance.WeaponManagerClass.RemoveAllShells();
                    __instance.Player_0.UpdatePhones();
                    __instance.AmmoItemClass = (AmmoItemClass)gStruct.Value.ResultItem;
                }
            }

            return false;
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
            if (!Plugin._EnableManualChambering.Value) return;

            var fc = __instance.FirearmController_0;
            if (fc == null || fc.Weapon == null) return;

            var player = Traverse.Create(fc).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            ManualChamberingState.CanLoadChamber = true;
            ManualChamberingState.BlockChambering = false;
            Plugin.Logger.LogDebug("[ManualChamber] Reload start: flags resetadas (CanLoad=true, Block=false)");
        }
    }

    public class SetAmmoCompatiblePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(FirearmsAnimator), nameof(FirearmsAnimator.SetAmmoCompatible));

        [PatchPrefix]
        private static void Prefix(FirearmsAnimator __instance, ref bool compatible)
        {
            if (!Plugin._EnableManualChambering.Value) return;
            if (ManualChamberingState.CanLoadChamber) return;

            var mainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null) return;

            var fc = mainPlayer.HandsController as Player.FirearmController;
            if (fc == null || fc.FirearmsAnimator != __instance) return;

            compatible = false;
        }
    }

    public class SetAmmoOnMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(FirearmsAnimator), nameof(FirearmsAnimator.SetAmmoOnMag));

        [PatchPrefix]
        private static bool Prefix(FirearmsAnimator __instance)
        {
            if (!Plugin._EnableManualChambering.Value) return true;

            var mainPlayer = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
            if (mainPlayer == null) return true;

            var fc = mainPlayer.HandsController as Player.FirearmController;
            if (fc == null || fc.FirearmsAnimator != __instance) return true;

            if (ManualChamberingState.BlockChambering)
            {
                ManualChamberingState.BlockChambering = false;
                return false;
            }
            return true;
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
                if (ManualChamberingState.AllowVanillaChamberUnload)
                {
                    ManualChamberingState.AllowVanillaChamberUnload = false;
                    return true;
                }

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
                    Plugin.Logger.LogInfo("[ManualChamber] Vanilla UnloadChamber detectado. Forçando Stance 0 e aguardando.");
                    StanceManager.StartActionStance();
                    
                    var comp = player.gameObject.GetOrAddComponent<ManualChamberingComponent>();
                    comp.GamePlayerOwner = __instance;
                    comp.Phase = 3;
                    comp.Timer = 0f;
                    
                    return false; // Blocks immediate execution, allowing Phase 3 to execute it later
                }
            }
            return true;
        }
    }
}
