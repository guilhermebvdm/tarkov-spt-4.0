using System;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace TRLFixes.Patches
{
    /// <summary>
    /// Previne NullReferenceException em BotWeaponManager.UpdateHandsController e BotWeaponSelector.OnWeaponTaken
    /// quando um bot desmaia, morre ou tem sua IA desativada enquanto uma transição de mãos/arma estava sendo
    /// finalizada em LateUpdate.
    /// Utiliza Harmony Finalizers para capturar e engolir qualquer NullReferenceException lançada pelo código vanilla.
    /// </summary>
    public class BotWeaponManagerSafetyPatch
    {
        private const float ThrottleSeconds = 5f;
        private static float _lastLogTime = -999f;
        private static int _swallowedCount;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.botweaponmanagersafety");

                var targetUpdateHands = AccessTools.Method(typeof(BotWeaponManager), nameof(BotWeaponManager.UpdateHandsController));
                if (targetUpdateHands != null)
                {
                    var prefixUpdateHands = AccessTools.Method(typeof(BotWeaponManagerSafetyPatch), nameof(PrefixUpdateHandsController));
                    var finalizerUpdateHands = AccessTools.Method(typeof(BotWeaponManagerSafetyPatch), nameof(FinalizerUpdateHandsController));
                    harmony.Patch(targetUpdateHands, prefix: new HarmonyMethod(prefixUpdateHands), finalizer: new HarmonyMethod(finalizerUpdateHands));
                }

                var targetOnWeaponTaken = AccessTools.Method(typeof(BotWeaponSelector), nameof(BotWeaponSelector.OnWeaponTaken));
                if (targetOnWeaponTaken != null)
                {
                    var prefixOnWeaponTaken = AccessTools.Method(typeof(BotWeaponManagerSafetyPatch), nameof(PrefixOnWeaponTaken));
                    var finalizerOnWeaponTaken = AccessTools.Method(typeof(BotWeaponManagerSafetyPatch), nameof(FinalizerOnWeaponTaken));
                    harmony.Patch(targetOnWeaponTaken, prefix: new HarmonyMethod(prefixOnWeaponTaken), finalizer: new HarmonyMethod(finalizerOnWeaponTaken));
                }

                Plugin.Log?.LogInfo("TRL-Fixes: Hooks e Finalizers em BotWeaponManager e BotWeaponSelector aplicados com sucesso!");
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar BotWeaponManagerSafetyPatch: {ex}");
            }
        }

        public static bool PrefixUpdateHandsController(BotWeaponManager __instance, IHandsController handsController, ref bool allFine)
        {
            allFine = false;

            if (__instance == null || __instance.BotOwner_0 == null || __instance.BotOwner_0.WeaponManager == null)
            {
                LogThrottled("BotOwner_0 ou WeaponManager e nulo no BotWeaponManager.UpdateHandsController");
                return false; // Aborta método vanilla com segurança
            }

            if (handsController is IFirearmHandsController firearm)
            {
                if (firearm.Item == null)
                {
                    LogThrottled("firearm.Item e nulo no BotWeaponManager.UpdateHandsController");
                    return false; // Aborta método vanilla com segurança, evitando NRE em method_4
                }
            }

            return true; // Prossegue com a execução vanilla normal
        }

        public static Exception FinalizerUpdateHandsController(Exception __exception, ref bool allFine)
        {
            if (__exception != null)
            {
                allFine = false;
                if (__exception is NullReferenceException)
                {
                    LogThrottled("NullReferenceException engolida em BotWeaponManager.UpdateHandsController");
                    return null; // Instrução para o Harmony engolir a exceção!
                }
            }
            return __exception;
        }

        public static bool PrefixOnWeaponTaken(BotWeaponSelector __instance)
        {
            if (__instance == null || __instance.BotOwner_0 == null)
            {
                LogThrottled("BotOwner_0 e nulo no BotWeaponSelector.OnWeaponTaken");
                return false; // Aborta OnWeaponTaken com segurança antes de ler BotOwner_0.BotState
            }

            return true;
        }

        public static Exception FinalizerOnWeaponTaken(Exception __exception)
        {
            if (__exception is NullReferenceException)
            {
                LogThrottled("NullReferenceException engolida em BotWeaponSelector.OnWeaponTaken");
                return null; // Instrução para o Harmony engolir a exceção!
            }
            return __exception;
        }

        private static void LogThrottled(string reason)
        {
            _swallowedCount++;
            float now = Time.time;
            if (now - _lastLogTime >= ThrottleSeconds)
            {
                _lastLogTime = now;
                Plugin.Log?.LogWarning($"TRL-Fixes [BotWeaponManagerSafety]: Interceptada falha de referencia nula ({reason}). Ocorrencias acumuladas: {_swallowedCount}");
            }
        }
    }
}
