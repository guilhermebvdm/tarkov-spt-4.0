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
        // StartEquipWeapPatch para distinguir o spawn (cenário 1) de uma troca de arma mid-raid (cenário 2),
        // já que ambos passam pela mesma operação de equip (GClass2055).
        public static bool JustSpawned = false;

        // Reset completo de estado — chamado em OnRaidStart e OnRaidEnd (anti-resíduo entre raids,
        // troca de arma, morte, extração).
        public static void Reset()
        {
            BlockChambering = false;
            CanLoadChamber = false;
            JustSpawned = false;
        }
    }

    public class ManualChamberingComponent : MonoBehaviour
    {
        public Player.FirearmController FirearmController;
        public float Timer = 0f;
        public bool IsActive = false;

        void Update()
        {
            if (IsActive)
            {
                Timer += Time.deltaTime;
                if (Timer >= 0.5f)
                {
                    if (FirearmController != null && FirearmController.FirearmsAnimator != null)
                    {
                        FirearmController.FirearmsAnimator.Rechamber(false);
                    }
                    IsActive = false;
                    Timer = 0f;
                }
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
            ManualChamberingState.JustSpawned = false; // consome no primeiro equip pós-spawn

            if (__instance.Weapon_0.HasChambers && chamberAmmoCount == 0)
            {
                // Cenário 1 (spawn) respeita _ManualChamberingOnRaidStart; cenário 2 (equip mid-raid) usa o master.
                bool blockThisEquip = isSpawnEquip ? Plugin._ManualChamberingOnRaidStart.Value : true;
                if (blockThisEquip)
                {
                    ManualChamberingState.CanLoadChamber = false;
                    ManualChamberingState.BlockChambering = true;
                    Plugin.Logger.LogDebug($"[ManualChamber] Equip vazio: auto-chamber BLOQUEADO (spawn={isSpawnEquip})");
                }
                else
                {
                    ManualChamberingState.CanLoadChamber = true; // spawn + OnRaidStart=off -> vanilla
                    Plugin.Logger.LogDebug("[ManualChamber] Equip vazio: spawn com OnRaidStart=off -> auto-chamber vanilla");
                }
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
                __instance.FirearmsAnimator_0.SetAmmoInChamber(chamberAmmoCount);
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

    // Reset de flags no INÍCIO do reload — espelha RealismMod.StartReloadPatch (que a port não tinha).
    // Sem este reset, estado residual de um equip/chamber anterior pode travar o reload (softlock).
    // O bloqueio do auto-chamber pós-reload, quando aplicável, é feito pelo PreChamberLoadPatch (method_18).
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
            // method_18 é o auto-chamber genérico (predominante após reload com câmara vazia).
            // _ManualChamberingOnReload é a aproximação de cenário 3; sem ponto-só-reload limpo em 0.16.
            if (!Plugin._ManualChamberingOnReload.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            if (__instance.Weapon != null && __instance.Weapon.HasChambers && __instance.Weapon.Chambers.Length == 1 && __instance.Weapon.ChamberAmmoCount == 0 && !__instance.IsStationaryWeapon)
            {
                // Só BlockChambering (como RealismMod.PreChamberLoadPatch) — NÃO mutar CanLoadChamber aqui
                // (a port fazia, criando estado residual entre operações).
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
                weaponStateClass.RemoveAllShells();
                AmmoItemClass bullet = (AmmoItemClass)gstruct.Value.ResultItem;
                fc.FirearmsAnimator.SetAmmoInChamber(1f);
                fc.FirearmsAnimator.SetAmmoOnMag(fc.Weapon.GetCurrentMagazineCount());
                weaponStateClass.SetRoundIntoWeapon(bullet, 0);
                fc.FirearmsAnimator.Rechamber(true);

                var comp = player.gameObject.GetOrAddComponent<ManualChamberingComponent>();
                comp.FirearmController = fc;
                comp.IsActive = true;
                comp.Timer = 0f;
            }
        }

        [PatchPrefix]
        private static bool Prefix(GamePlayerOwner __instance, ECommand command)
        {
            if (!Plugin._EnableManualChambering.Value) return true;

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
            }
            return true;
        }
    }
}
