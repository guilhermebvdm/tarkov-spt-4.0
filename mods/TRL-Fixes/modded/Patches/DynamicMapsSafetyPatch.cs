using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Proteção defensiva para o encerramento de raid no mod DynamicMaps.
    /// Evita a exceção NullReferenceException em ModdedMapScreen.OnRaidEnd() quando
    /// elementos de UI foram previamente descarregados ou o mapa não foi carregado corretamente.
    ///
    /// NOTA: A tentativa anterior de patchar GameWorldOnDestroyPatch.PatchPrefix não alcançava o
    /// OnRaidEnd porque ele é chamado internamente DENTRO do PatchPrefix do DynamicMaps.
    /// O PatchFinalizer no caller não captura exceções geradas em chamadas internas.
    /// Solução: patchar diretamente ModdedMapScreen.OnRaidEnd com PatchFinalizer.
    /// </summary>
    public class DynamicMapsSafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Tenta o alvo primário: o método OnRaidEnd diretamente
            var modMapScreenType = AccessTools.TypeByName("DynamicMaps.UI.ModdedMapScreen");
            if (modMapScreenType != null)
            {
                var onRaidEnd = AccessTools.Method(modMapScreenType, "OnRaidEnd");
                if (onRaidEnd != null)
                {
                    return onRaidEnd;
                }
                Plugin.Log?.LogWarning("[TRL-Fixes] DynamicMaps.UI.ModdedMapScreen.OnRaidEnd não encontrado — o DynamicMaps pode ter mudado sua API.");
            }

            // Fallback: patchar o GameWorldOnDestroyPatch.PatchPrefix como antes
            var patchType = AccessTools.TypeByName("DynamicMaps.Patches.GameWorldOnDestroyPatch");
            if (patchType != null)
            {
                Plugin.Log?.LogWarning("[TRL-Fixes] DynamicMaps.UI.ModdedMapScreen não encontrado — usando fallback GameWorldOnDestroyPatch.PatchPrefix.");
                return AccessTools.Method(patchType, "PatchPrefix");
            }

            Plugin.Log?.LogWarning("[TRL-Fixes] DynamicMaps não encontrado — DynamicMapsSafetyPatch desativado.");
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
