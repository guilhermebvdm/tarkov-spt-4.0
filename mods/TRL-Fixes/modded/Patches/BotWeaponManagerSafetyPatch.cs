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
                    harmony.Patch(targetUpdateHands, prefix: new HarmonyMethod(prefixUpdateHands));
                }

                var targetOnWeaponTaken = AccessTools.Method(typeof(BotWeaponSelector), nameof(BotWeaponSelector.OnWeaponTaken));
                if (targetOnWeaponTaken != null)
                {
                    var prefixOnWeaponTaken = AccessTools.Method(typeof(BotWeaponManagerSafetyPatch), nameof(PrefixOnWeaponTaken));
                    harmony.Patch(targetOnWeaponTaken, prefix: new HarmonyMethod(prefixOnWeaponTaken));
                }

                Debug.Log("TRL-Fixes: Hooks em BotWeaponManager e BotWeaponSelector (weapon safety) aplicados com sucesso!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"TRL-Fixes: Erro ao aplicar BotWeaponManagerSafetyPatch: {ex}");
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

        public static bool PrefixOnWeaponTaken(BotWeaponSelector __instance)
        {
            if (__instance == null || __instance.BotOwner_0 == null)
            {
                LogThrottled("BotOwner_0 e nulo no BotWeaponSelector.OnWeaponTaken");
                return false; // Aborta OnWeaponTaken com segurança antes de ler BotOwner_0.BotState
            }

            return true;
        }

        private static void LogThrottled(string reason)
        {
            _swallowedCount++;
            float now = Time.time;
            if (now - _lastLogTime >= ThrottleSeconds)
            {
                _lastLogTime = now;
                Debug.LogWarning($"TRL-Fixes [BotWeaponManagerSafety]: Interceptada falha de referencia nula ({reason}). Ocorrencias acumuladas: {_swallowedCount}");
            }
        }
    }
}
