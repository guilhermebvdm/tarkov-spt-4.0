using System.Collections.Generic;
using BepInEx.Logging;
using System.Reflection;

namespace TrueTrauma
{
    public static class TraumaState
    {
        public static ManualLogSource Logger;
        public static FieldInfo PlayerField;

        // --- LISTA DE IDs DESMAIADOS (HOST AUTHORITY) ---
        // O Host usa isso para filtrar a visao dos bots
        public static HashSet<string> FaintedPlayerIds = new HashSet<string>();

        // Timers normais
        public static readonly Dictionary<string, float> BlackoutTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> BlackoutStartTimes = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> GraceTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> ImpactTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> AimingFatigueTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> VoiceCooldowns = new Dictionary<string, float>();
        public static Dictionary<string, float> BotLegsBrokenStartTimes = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> LegPenaltyTimers = new Dictionary<string, float>();
        public static float EffectIntensity = 0f;

        // ref: CR-01-27 — renovação de DoContusion por intervalo (não por frame)
        public static readonly Dictionary<string, float> ContusionRenewTimers = new Dictionary<string, float>();

        // Faint State Trackers
        public static bool IsFainted = false;

        /// <summary>
        /// ref: CR-01-09/13 — limpeza TOTAL do estado de trauma (início E fim de raid).
        /// Estáticos vivem a sessão inteira; qualquer campo fora desta lista vaza entre raids.
        /// </summary>
        public static void ResetAll()
        {
            FaintedPlayerIds.Clear();
            BlackoutTimers.Clear();
            BlackoutStartTimes.Clear();
            GraceTimers.Clear();
            ImpactTimers.Clear();
            AimingFatigueTimers.Clear();
            VoiceCooldowns.Clear();
            BotLegsBrokenStartTimes.Clear();
            LegPenaltyTimers.Clear();
            ContusionRenewTimers.Clear();
            EffectIntensity = 0f;
            IsFainted = false;
        }
    }
}
