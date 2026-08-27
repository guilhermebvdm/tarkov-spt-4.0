using HarmonyLib;
using EFT;
using UnityEngine;
using System.Reflection;
using System;

namespace TRLFixes.Patches
{
    public class FlashbangBotPatch
    {
        private static PropertyInfo _botOwnerProp;
        private static MethodInfo _setActiveMethod;
        private static readonly object[] _inactiveArgs = new object[] { false };

        public void Enable()
        {
            try
            {
                // Busca o tipo SAINActivationClass e faz cache estático de reflexão (AUD-01-01)
                var sainActivationType = AccessTools.TypeByName("SAIN.SAINComponent.Classes.SAINActivationClass");
                if (sainActivationType != null)
                {
                    _botOwnerProp = AccessTools.Property(sainActivationType, "BotOwner");
                    _setActiveMethod = AccessTools.Method(sainActivationType, "SetActive");

                    var manualUpdateMethod = AccessTools.Method(sainActivationType, "ManualUpdate");
                    if (manualUpdateMethod != null)
                    {
                        var harmony = new Harmony("com.trl.fixes.flashbang");
                        var prefixMethod = AccessTools.Method(typeof(FlashbangBotPatch), nameof(Prefix));
                        harmony.Patch(manualUpdateMethod, prefix: new HarmonyMethod(prefixMethod));
                        Plugin.Log?.LogInfo("TRL-Fixes: Hook no SAINActivationClass.ManualUpdate aplicado com sucesso!");
                    }
                }
                else
                {
                    Plugin.Log?.LogWarning("TRL-Fixes: SAIN não detectado. A correção da flashbang não será aplicada.");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar FlashbangBotPatch: {ex}");
            }
        }

        public static bool Prefix(object __instance)
        {
            try
            {
                if (__instance == null || _botOwnerProp == null || _setActiveMethod == null) return true;

                // Lê o BotOwner em cache sem lookup repetitivo de reflexão per-frame
                var botOwner = _botOwnerProp.GetValue(__instance) as BotOwner;
                if (botOwner != null && botOwner.FlashGrenade != null && botOwner.FlashGrenade.IsFlashed)
                {
                    // Bot está cego: suspende o SAIN usando o array de argumentos pré-alocado
                    _setActiveMethod.Invoke(__instance, _inactiveArgs);

                    // Força Vanilla a atirar cego caso o bot esteja em combate
                    if (botOwner.GetPlayer?.MovementContext != null)
                    {
                        botOwner.GetPlayer.MovementContext.SetBlindFire(1);
                    }

                    return false; // Pula o ManualUpdate do SAIN, mantendo-o suspenso
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes FlashbangPatch Error: {ex}");
            }

            return true; // Continua a execução do SAIN normalmente se não estiver cego
        }
    }
}
