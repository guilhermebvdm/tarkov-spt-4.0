using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Linq;
using System.Reflection;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Garante a execução segura do alerta de UI do Fika chamado fora da Main Thread (ShowFikaMessage).
    /// O Fika loga o erro e retorna um objeto vazio ao ser chamado fora da Main Thread.
    /// Este patch intercepta no Prefix: se estiver fora da Main Thread, despacha a execução para
    /// a Main Thread via Diz.Utils.AsyncWorker.RunInMainTread e retorna um objeto vazio,
    /// suprimindo o log de erro e garantindo que o alerta de erro/mensagem seja exibido na UI.
    /// </summary>
    public class FikaMainThreadUISafetyPatch : ModulePatch
    {
        private static MethodInfo _cachedTargetMethod;

        protected override MethodBase GetTargetMethod()
        {
            var targetType = AccessTools.TypeByName("Fika.Core.UI.FikaUIGlobals");
            if (targetType == null)
            {
                Plugin.Log?.LogInfo("[TRL-Fixes] FikaUIGlobals não encontrado — Fika não está instalado ou o nome do tipo mudou.");
                return null;
            }

            // Selecionamos explicitamente o overload com PreloaderUI como primeiro parâmetro usando type safety em compilação (AUD-01-05)
            var preloaderUIType = typeof(EFT.UI.PreloaderUI);

            var method = targetType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "ShowFikaMessage"
                                  && m.GetParameters().Length > 0
                                  && m.GetParameters()[0].ParameterType == preloaderUIType);

            if (method == null)
            {
                Plugin.Log?.LogWarning("[TRL-Fixes] ShowFikaMessage(PreloaderUI, ...) não encontrado — o overload mudou no Fika.");
            }
            _cachedTargetMethod = method;
            return method;
        }

        [PatchPrefix]
        private static bool Prefix(object preloaderUI, string header, string message, object buttonType, float waitingTime, Action acceptCallback, Action endTimeCallback, ref object __result)
        {
            if (!Diz.Utils.AsyncWorker.CheckIsMainThread())
            {
                Diz.Utils.AsyncWorker.RunInMainTread(() =>
                {
                    try
                    {
                        _cachedTargetMethod?.Invoke(null, new object[] { preloaderUI, header, message, buttonType, waitingTime, acceptCallback, endTimeCallback });
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log?.LogWarning($"[TRL-Fixes] Falha ao despachar ShowFikaMessage para Main Thread: {ex.Message}");
                    }
                });

                if (_cachedTargetMethod != null && _cachedTargetMethod.ReturnType != typeof(void))
                {
                    __result = Activator.CreateInstance(_cachedTargetMethod.ReturnType);
                }
                return false; // Ignora o método original na thread secundária (evita o LogError e garante exibição)
            }
            return true;
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
