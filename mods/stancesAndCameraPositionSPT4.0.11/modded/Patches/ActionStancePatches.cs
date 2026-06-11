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
}
