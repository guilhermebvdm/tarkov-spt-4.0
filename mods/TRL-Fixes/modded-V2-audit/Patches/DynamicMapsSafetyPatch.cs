using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Proteção defensiva para o encerramento de raid no mod DynamicMaps.
    /// Evita a exceção NullReferenceException em ModdedMapScreen.OnRaidEnd() quando
    /// elementos de UI foram previamente descarregados ou o mapa não foi aberto/carregado.
    /// </summary>
    public class DynamicMapsSafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Patch direto no método OnRaidEnd do ModdedMapScreen
            var modMapScreenType = AccessTools.TypeByName("DynamicMaps.UI.ModdedMapScreen");
            if (modMapScreenType != null)
            {
                var onRaidEnd = AccessTools.Method(modMapScreenType, "OnRaidEnd");
                if (onRaidEnd != null)
                {
                    return onRaidEnd;
                }
                Plugin.Log?.LogWarning("[TRL-Fixes] DynamicMaps.UI.ModdedMapScreen.OnRaidEnd não encontrado — o DynamicMaps pode ter alterado sua estrutura.");
            }
            else
            {
                Plugin.Log?.LogInfo("[TRL-Fixes] DynamicMaps não detectado — DynamicMapsSafetyPatch desativado.");
            }

            return null;
        }

        [PatchFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Plugin.Log?.LogWarning($"[TRL-Fixes] Suprimida exceção no encerramento de raid do DynamicMaps (OnRaidEnd): {__exception.Message}");
            }
            return null; // Absorve qualquer exceção lançada no encerramento do DynamicMaps
        }
    }
}
