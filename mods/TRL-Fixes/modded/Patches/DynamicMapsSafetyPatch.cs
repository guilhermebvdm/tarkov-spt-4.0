using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Proteção defensiva para o encerramento de raid no mod DynamicMaps.
    /// Evita a exceção NullReferenceException em ModdedMapScreen.OnRaidEnd() quando elementos de UI foram previamente descarregados.
    /// </summary>
    public class DynamicMapsSafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = AccessTools.TypeByName("DynamicMaps.Patches.GameWorldOnDestroyPatch");
            if (targetType == null)
            {
                return null;
            }
            return AccessTools.Method(targetType, "PatchPrefix");
        }

        [PatchFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Plugin.Log?.LogWarning($"[TRL-Fixes] Suprimida exceção no encerramento de raid do DynamicMaps: {__exception.Message}");
            }
            return null; // Absorve qualquer exceção lançada no encerramento do DynamicMaps
        }
    }
}
