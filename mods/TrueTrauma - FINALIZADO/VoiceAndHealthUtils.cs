using System;
using UnityEngine;
using EFT;
using HarmonyLib;

namespace TrueTrauma
{
    public static class HealthUtils
    {
        public static bool IsPartDestroyed(Player p, EBodyPart part)
        {
            if (p == null || p.HealthController == null) return false;
            try
            {
                var health = p.HealthController.GetBodyPartHealth(part);
                return p.HealthController.IsBodyPartBroken(part) || health.Current < 1f;
            }
            catch { return false; }
        }
    }

    public static class VoiceHelper
    {
        public static void SafePlayVoice(Player player, string triggerName)
        {
            if (player == null || !player.HealthController.IsAlive) return;
            try
            {
                Type enumType = typeof(EPhraseTrigger);
                object triggerValue = Enum.Parse(enumType, triggerName);
                var sayMethod = player.GetType().GetMethod("Say");
                sayMethod?.Invoke(player, new object[] { triggerValue, true, 0f, 0, 100, false });
            }
            catch { }
        }

        public static void TriggerTraumaVoice(Player player, string type)
        {
            if (player == null) return;
            string id = player.ProfileId;

            if (TraumaState.VoiceCooldowns.ContainsKey(id))
            {
                if (Time.time < TraumaState.VoiceCooldowns[id]) return;
            }

            string phrase = "OnBreath";
            switch (type)
            {
                case "Leg": phrase = (UnityEngine.Random.value > 0.5f) ? "OnAgony" : "OnLegBroken"; break;
                case "Arm": phrase = (UnityEngine.Random.value > 0.5f) ? "OnAgony" : "OnHandBroken"; break;
                case "Gut": phrase = "OnPain"; break;
                case "TryAim": phrase = "OnBreath"; break;
            }

            SafePlayVoice(player, phrase);
            TraumaState.VoiceCooldowns[id] = Time.time + 2f;
        }
    }

    // PATCH DE SILÊNCIO (CORRIGIDO)
    [HarmonyPatch(typeof(Player), "Say")]
    class SilenceVoicePatch
    {
        static bool Prefix(Player __instance, EPhraseTrigger phrase)
        {
            if (!TrueTraumaPlugin.ConfigMasterEnabled.Value || !TrueTraumaPlugin.ConfigBlackoutEnabled.Value) return true;

            // Se o jogador estiver desmaiado
            if (__instance != null && TraumaState.BlackoutTimers.ContainsKey(__instance.ProfileId))
            {
                // --- CORREÇÃO AQUI ---
                // Se for o nosso sinal secreto de rede (Desmaiei ou Acordei), DEIXA PASSAR!
                if (phrase == EPhraseTrigger.OnYourOwn || phrase == EPhraseTrigger.OnBreath)
                {
                    return true;
                }

                // Qualquer outra fala (gritos de dor, mumbles manuais) é bloqueada
                return false;
            }
            return true;
        }
    }
}