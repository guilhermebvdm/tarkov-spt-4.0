using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Garante a execução segura de alertas e diálogos de interface do Fika (ShowFikaMessage).
    /// Se a chamada for realizada fora da Main Thread do Unity, o patch captura e previne a trava do console.
    /// </summary>
    public class FikaMainThreadUISafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = AccessTools.TypeByName("Fika.Core.UI.FikaUIGlobals");
            if (targetType == null)
            {
                return null;
            }
            return AccessTools.Method(targetType, "ShowFikaMessage");
        }

        [PatchFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception != null)
            {
                Plugin.Log?.LogWarning($"[TRL-Fixes] Suprimida exceção de UI fora da Main Thread no Fika: {__exception.Message}");
            }
            return null; // Absorve a exceção de thread para evitar a queda da worker thread
        }
    }
}
