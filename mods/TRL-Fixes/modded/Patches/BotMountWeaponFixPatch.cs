using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Correção completa para permitir que bots de IA (Rogues exUsec e outros) operem armas estáticas (NSV, AGS-30).
    /// Resolve 3 problemas distintos da IA/Engine:
    /// 1. Ativação da Camada 10 (StationaryWS) e transição de CurUsingLogic de NoSupress (Usable=false) -> MgSuppress (Usable=true).
    /// 2. Resolução de comparação de itens por ID de string no method_4 (evita DropCurWeapon() por ponteiro C#).
    /// 3. Destravamento de rede no FikaPlayer/Player (bypassa a checagem WaitingForCallback do FIKA para bots de IA).
    /// </summary>
    public class BotMountWeaponFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Constructor(typeof(ExUsecBrainClass), new[] { typeof(BotOwner) });
        }

        [PatchPostfix]
        private static void PatchPostfix(ExUsecBrainClass __instance)
        {
            if (__instance == null || __instance.Owner == null) return;

            try
            {
                // Garante que a camada 10 (StationaryWithSuppressLayer) esteja na lista de execução ativa da IA
                __instance.ActivateLayers(new List<int> { 10 });

                // Se não estiver vinculado a nenhuma metralhadora, busca no raio de 100m
                if (__instance.StationaryWeaponLink_0 == null)
                {
                    __instance.Owner.WeaponManager?.Stationary?.CheckWantTakeStationary(100f);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"[TRL-Fixes] Erro ao ativar camada 10 para bot '{__instance.Owner.Profile?.Nickname}': {ex.Message}");
            }
        }
    }

    public class GClass81ShallUseNowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass81), nameof(GClass81.ShallUseNow));
        }

        [PatchPrefix]
        private static bool PatchPrefix(GClass81 __instance)
        {
            if (__instance == null || __instance.BotOwner_0 == null) return true;

            var stationary = __instance.BotOwner_0.WeaponManager?.Stationary;
            if (stationary != null && stationary.CurLink != null)
            {
                var supress = __instance.BotOwner_0.SuppressStationary;
                if (supress != null && (supress.CurUsingLogic == null || !supress.CurUsingLogic.Usable))
                {
                    bool isGrenade = stationary.CurLink.IsGrenade();
                    supress.CurUsingLogic = isGrenade ? (AbstractSuppressStationary)supress.ArtillerySuppress : (AbstractSuppressStationary)supress.MgSuppress;
                }
            }
            return true;
        }
    }

    public class BotStationaryWeaponDataMethod4Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotStationaryWeaponData), "method_4");
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotStationaryWeaponData __instance, Player.FirearmController fireArms)
        {
            if (__instance == null || __instance.BotOwner_0 == null) return true;

            EFT.InventoryLogic.Item firearmItem = fireArms?.Item;
            EFT.InventoryLogic.Item stationItem = __instance.CurLink?.Weapon?.Item;

            string firearmId = firearmItem?.Id;
            string stationId = stationItem?.Id;

            if (firearmItem != null && stationItem != null && (firearmItem == stationItem || (firearmId != null && firearmId == stationId) || firearmItem.TemplateId == stationItem.TemplateId))
            {
                __instance.BotOwner_0.WeaponManager.StationaryTaken(fireArms, __instance.CurLink.Weapon);
                __instance.CanLeave = true;
                return false; // Bypassa o DropCurWeapon() por desigualdade de ponteiros C#
            }

            return true;
        }
    }

    public class FikaPlayerOperateStationaryWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            Type fikaPlayerType = AccessTools.TypeByName("Fika.Core.Main.Players.FikaPlayer, Fika.Core");
            if (fikaPlayerType != null)
            {
                return AccessTools.Method(fikaPlayerType, "OperateStationaryWeapon");
            }
            return null;
        }

        [PatchPrefix]
        private static bool PatchPrefix(EFT.Player __instance, StationaryWeapon stationaryWeapon, StationaryPacketStruct.EStationaryCommand command)
        {
            if (__instance == null || !__instance.IsAI || stationaryWeapon == null) return true;

            if (command == StationaryPacketStruct.EStationaryCommand.Occupy)
            {
                if (stationaryWeapon.Locked && !stationaryWeapon.IsOperator(__instance.ProfileId))
                {
                    stationaryWeapon.Unlock(null);
                }

                stationaryWeapon.SetOperator(__instance.ProfileId, isAI: true);
                __instance.MovementContext.StationaryWeapon = stationaryWeapon;
                __instance.MovementContext.InteractionParameters = stationaryWeapon.GetInteractionParameters();
                __instance.MovementContext.PlayerAnimatorSetApproached(b: false);
                __instance.MovementContext.PlayerAnimatorSetStationary(b: true);
                __instance.RemoveLeftHandItem();
                __instance.MovementContext.PlayerAnimatorSetStationaryAnimation((int)stationaryWeapon.Animation);
                return false; // Bypassa a checagem de WaitingForCallback do FIKA para bots de IA
            }

            return true;
        }
    }

    public class PlayerOperateStationaryWeaponPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EFT.Player), nameof(EFT.Player.OperateStationaryWeapon));
        }

        [PatchPrefix]
        private static bool PatchPrefix(EFT.Player __instance, StationaryWeapon stationaryWeapon, StationaryPacketStruct.EStationaryCommand command)
        {
            if (__instance == null || !__instance.IsAI || stationaryWeapon == null) return true;

            if (command == StationaryPacketStruct.EStationaryCommand.Occupy)
            {
                if (stationaryWeapon.Locked && !stationaryWeapon.IsOperator(__instance.ProfileId))
                {
                    stationaryWeapon.Unlock(null);
                }

                stationaryWeapon.SetOperator(__instance.ProfileId, isAI: true);
                __instance.MovementContext.StationaryWeapon = stationaryWeapon;
                __instance.MovementContext.InteractionParameters = stationaryWeapon.GetInteractionParameters();
                __instance.MovementContext.PlayerAnimatorSetApproached(b: false);
                __instance.MovementContext.PlayerAnimatorSetStationary(b: true);
                __instance.RemoveLeftHandItem();
                __instance.MovementContext.PlayerAnimatorSetStationaryAnimation((int)stationaryWeapon.Animation);
            }

            return true;
        }
    }
}
