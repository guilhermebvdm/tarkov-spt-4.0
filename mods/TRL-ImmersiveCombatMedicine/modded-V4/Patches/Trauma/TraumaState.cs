using System.Collections.Generic;
using BepInEx.Logging;
using System.Reflection;

namespace TRLImmersiveCombatMedicine.Trauma
{
    public static class TraumaState
    {
        public static ManualLogSource Logger;
        public static FieldInfo PlayerField;

        // --- LISTA DE IDs DESMAIADOS (HOST AUTHORITY) ---
        // O Host usa isso para filtrar a visao dos bots
        public static HashSet<string> FaintedPlayerIds = new HashSet<string>();

        // Timers normais
        // ref: spec 003 §4 (D10) — ImpactTimers/LegPenaltyTimers/BotLegsBrokenStartTimes removidos junto com o
        // sistema legado de pernas (escritores e consumidores saíram em MovementPatches/HealthPatches/InputPatches)
        // ref: spec 005 §1.7 (D10) — AimingFatigueTimers removido junto com a fadiga de mira legada de braços
        // (escritor único saiu do MovementPatches; o cancela-ADS é do TraumaArmsConsumer, timer por evento)
        public static readonly Dictionary<string, float> BlackoutTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> BlackoutStartTimes = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> GraceTimers = new Dictionary<string, float>();
        public static readonly Dictionary<string, float> VoiceCooldowns = new Dictionary<string, float>();
        public static float EffectIntensity = 0f;

        // ref: CR-01-27 — renovação de DoContusion por intervalo (não por frame)
        // ref: CR-04-13 — cooldown de re-desmaio de bots (humanos usam o grace)
        public static readonly Dictionary<string, float> BotFaintCooldowns = new Dictionary<string, float>();

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
            VoiceCooldowns.Clear();
            BotFaintCooldowns.Clear();
            EffectIntensity = 0f;
            IsFainted = false;
        }
    }
}
