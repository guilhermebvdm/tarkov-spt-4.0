using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Garante a execução segura do alerta de UI do Fika chamado fora da Main Thread (ShowFikaMessage).
    /// O Fika loga o erro e retorna um GClass3835 vazio — este patch absorve qualquer exceção residual nesse path.
    /// Alvo: o overload de extensão em PreloaderUI (Fika.Core.UI.FikaUIGlobals), que contém o check de thread
    /// e o log de erro "[ShowFikaMessage]: You are trying to show error screen from non-main thread!".
    /// </summary>
    public class FikaMainThreadUISafetyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetType = AccessTools.TypeByName("Fika.Core.UI.FikaUIGlobals");
            if (targetType == null)
            {
                Plugin.Log?.LogWarning("[TRL-Fixes] FikaUIGlobals não encontrado — Fika não está instalado ou o nome do tipo mudou.");
                return null;
            }

            // Há dois overloads de ShowFikaMessage: um em PreloaderUI (this PreloaderUI) e um em ErrorScreen (this ErrorScreen).
            // AccessTools.Method sem parâmetros lança AmbiguousMatchException.
            // Selecionamos explicitamente o overload com PreloaderUI como primeiro parâmetro (método de extensão estático).
            var preloaderUIType = AccessTools.TypeByName("EFT.UI.PreloaderUI");
            if (preloaderUIType == null)
            {
                Plugin.Log?.LogWarning("[TRL-Fixes] PreloaderUI não encontrado — não foi possível resolver o overload de ShowFikaMessage.");
                return null;
            }

            var method = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "ShowFikaMessage"
                                  && m.GetParameters().Length > 0
                                  && m.GetParameters()[0].ParameterType == preloaderUIType);

            if (method == null)
            {
                Plugin.Log?.LogWarning("[TRL-Fixes] ShowFikaMessage(PreloaderUI, ...) não encontrado — o overload mudou no Fika.");
            }
            return method;
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
