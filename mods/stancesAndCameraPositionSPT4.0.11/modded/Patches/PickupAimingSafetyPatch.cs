using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Previne a trava do personagem (MovementContext stuck) durante a transição de mira ao equipar ou pegar itens diretamente do chão (Pickup).
    /// </summary>
    public class PickupAimingSafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.PropertySetter(typeof(Player.FirearmController), "IsAiming");

        [PatchFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                Plugin.Logger.LogWarning($"[StanceOverhaul] Preveniu trava do MovementContext no FirearmController.IsAiming (pickup/equip race condition): {__exception.Message}");
                return null; // Engole a exceção para permitir que o MovementContext conclua a transição de estado sem travar o jogador
            }
            return __exception;
        }
    }
}
