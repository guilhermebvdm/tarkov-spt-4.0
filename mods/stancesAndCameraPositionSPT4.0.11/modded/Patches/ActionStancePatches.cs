using System;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using EFT.InventoryLogic;
using Comfort.Common;
using HarmonyLib;

namespace CameraRotationMod.Patches
{
    public class ActionStancePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Dummy method to make Aki patcher happy if we do multiple manual patches, 
            // but usually ModulePatch patches a single method.
            // Let's patch CheckAmmo.
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.CheckAmmo));
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
            }
        }
    }

    public class ActionStanceCheckChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.CheckChamber));
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
            }
        }
    }

    public class ActionStanceExamineWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.ExamineWeapon));
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
            }
        }
    }

    public class ActionStanceReloadPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // GClass2015 é a base de todas as operações de recarga (GClass2016, AmmoPackReloadOperationClass, etc.)
            return AccessTools.Method(typeof(Player.FirearmController.GClass2015), "Start", new Type[] { typeof(Callback) });
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController.GClass2015 __instance, ref Callback callback)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = __instance.Player_0;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
                var orig = callback;
                callback = new Callback((res) =>
                {
                    if (orig != null)
                    {
                        if (res.Succeed) orig.Succeed();
                        else orig.Fail(res.Error);
                    }
                    StanceManager.EndActionStance(forceCancel: !res.Succeed);
                });
            }
        }
    }

    public class ActionStanceCheckFireModePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), nameof(Player.FirearmController.CheckFireMode));
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
            }
        }
    }

    public class ActionStanceOnIdlePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "method_45");
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.EndActionStance();
            }
        }
    }

    public class ActionStanceUnloadMagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // GClass2050 é a operação de retirar mag da arma.
            // Precisamos buscar explicitamente o método Start virtual com 3 parâmetros para não pegar o Start() herdado da classe base.
            return typeof(Player.FirearmController.GClass2050).GetMethod(
                "Start",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof(MagazineItemClass), typeof(Slot), typeof(Callback) },
                null
            );
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController.GClass2050 __instance, MagazineItemClass magazine, Slot from, ref Callback callback)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            var player = __instance.Player_0;
            if (player != null && player.IsYourPlayer)
            {
                StanceManager.StartActionStance();
                var orig = callback;
                callback = new Callback((res) =>
                {
                    if (orig != null)
                    {
                        if (res.Succeed) orig.Succeed();
                        else orig.Fail(res.Error);
                    }
                    StanceManager.EndActionStance(forceCancel: !res.Succeed);
                });
            }
        }
    }

    // Item 008 (06-fix-02): "Esvaziar câmara" (unload chamber). GClass2046 é a operação de unload-chamber
    // (deriva de GClass2013; Start() sem parâmetros + RemoveAmmoFromChamber()). Patchar a base cobre os
    // derivados (inclui FixMalfunctionOperationClass — corrigir malfunção também levanta a arma; aceito).
    // Fim da ação: reusa ActionStanceOnIdlePatch (method_45). Conflito com o item 010 evitado por estado
    // da câmara: o swap só dispara com ChamberAmmoCount > 0 (esvaziar); o manual chambering (010) age em
    // câmara VAZIA via ECommand.ChamberUnload ANTES desta operação ser criada.
    public class ActionStanceUnloadChamberPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController.GClass2046), "Start", new System.Type[0]);
        }

        [PatchPrefix]
        private static void Prefix(object __instance)
        {
            if (Plugin._EnableActionStanceSwap != null && !Plugin._EnableActionStanceSwap.Value) return;

            // Resolve o FirearmController da operação (campo FirearmController_0, como nas outras operations).
            var fc = Traverse.Create(__instance).Field<Player.FirearmController>("FirearmController_0").Value;
            Player player = fc != null ? Traverse.Create(fc).Field<Player>("_player").Value : null;

            // Fallback defensivo: em SP a operação é sempre do jogador local.
            if (player == null)
            {
                var mp = Comfort.Common.Singleton<GameWorld>.Instance?.MainPlayer;
                if (mp != null) { player = mp; fc = mp.HandsController as Player.FirearmController; }
            }

            if (player == null || !player.IsYourPlayer || fc == null || fc.Weapon == null) return;

            int chamberAmmo = fc.Weapon.ChamberAmmoCount;
            Plugin.Logger.LogDebug($"[ActionStance] UnloadChamber Start (ChamberAmmoCount={chamberAmmo})");

            // Hipótese a validar in-game (revisão #1): esvaziar dispara com câmara CHEIA. Guard evita
            // swap espúrio no caminho de chamber-from-mag (câmara vazia), tratado pelo item 010.
            if (chamberAmmo > 0)
                StanceManager.StartActionStance();
        }
    }
}
