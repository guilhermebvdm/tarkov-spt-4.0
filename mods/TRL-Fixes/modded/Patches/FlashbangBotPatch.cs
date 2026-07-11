using HarmonyLib;
using EFT;
using UnityEngine;
using System.Reflection;
using System;

namespace TRLFixes.Patches
{
    public class FlashbangBotPatch
    {
        public void Enable()
        {
            var harmony = new Harmony("com.trl.fixes.flashbang");
            
            // Busca o tipo SAINActivationClass
            var sainActivationType = AccessTools.TypeByName("SAIN.SAINComponent.Classes.SAINActivationClass");
            if (sainActivationType != null)
            {
                var manualUpdateMethod = AccessTools.Method(sainActivationType, "ManualUpdate");
                var prefixMethod = AccessTools.Method(typeof(FlashbangBotPatch), nameof(Prefix));
                
                if (manualUpdateMethod != null)
                {
                    harmony.Patch(manualUpdateMethod, prefix: new HarmonyMethod(prefixMethod));
                    Debug.Log("TRL-Fixes: Hook no SAINActivationClass.ManualUpdate aplicado com sucesso!");
                }
            }
            else
            {
                Debug.LogError("TRL-Fixes: SAIN não detectado. A correção da flashbang não será aplicada.");
            }
        }

        public static bool Prefix(object __instance)
        {
            try
            {
                // SAINActivationClass herda de BotComponentClassBase que possui 'BotOwner'
                var botOwnerProp = AccessTools.Property(__instance.GetType(), "BotOwner");
                if (botOwnerProp != null)
                {
                BotOwner botOwner = botOwnerProp.GetValue(__instance) as BotOwner;
                if (botOwner != null && botOwner.FlashGrenade != null)
                {
                    // Verifica se o bot está sob efeito de flashbang
                    if (botOwner.FlashGrenade.IsFlashed)
                    {
                        // Bot está cego. Vamos chamar SetActive(false) no SAIN
                        var setActiveMethod = AccessTools.Method(__instance.GetType(), "SetActive");
                            if (setActiveMethod != null)
                            {
                                setActiveMethod.Invoke(__instance, new object[] { false });
                                
                                // Opcional: Força Vanilla a atirar cego caso o bot esteja atirando
                                if (botOwner.GetPlayer != null && botOwner.GetPlayer.MovementContext != null)
                                {
                                    botOwner.GetPlayer.MovementContext.SetBlindFire(1);
                                }
                                
                                return false; // Pula o ManualUpdate do SAIN, o mantendo suspenso
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"TRL-Fixes FlashbangPatch Error: {ex}");
            }
            
            return true; // Continua a execução do SAIN normalmente se não estiver cego
        }
    }
}

