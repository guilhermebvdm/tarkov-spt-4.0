using System;
using System.Reflection;
using BepInEx.Bootstrap;
using EFT;

namespace TRLImmersiveCombatMedicine.Medical
{
    /// <summary>
    ///     076 — ponte (reflection) para o mod CustomClasses. O perk "Restorative Surgery" (Médico de Combate) zera
    ///     a penalidade de HP MÁXIMO da cirurgia quando o OPERADOR é Médico. Decisão de arquitetura (opção B,
    ///     2026-07-19): o ICM é dono da cirurgia de ALIADO e chama a API do CustomClasses (dono da classe) —
    ///     soft-dependency por REFLECTION, então CustomClasses ausente = no-op (penalidade vanilla). Nada de
    ///     dependência dura entre os mods. Espelho: CustomClasses/modded/Client/CombatMedicSurgery.cs.
    /// </summary>
    public static class CustomClassesBridge
    {
        private const string Guid = "customclasses.mdj.client";

        private static bool _resolved;
        private static MethodInfo _adjust;        // public static float CombatMedicSurgery.Adjust(Player, float)
        private static MethodInfo _setExternal;   // public static void  CombatMedicSurgery.SetExternalHandling(bool)
        private static MethodInfo _allyTimeMult;  // 077 — public static float CombatMedicAllyPerks.AllyHealTimeMult(bool)
        private static MethodInfo _allyMobile;    // 077 — public static bool  CombatMedicAllyPerks.AllyMobileSurgeon()

        private static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;
            try
            {
                if (!Chainloader.PluginInfos.TryGetValue(Guid, out var info) || info?.Instance == null) return;
                var asm = info.Instance.GetType().Assembly;
                var type = asm.GetType("CustomClasses.Client.CombatMedicSurgery");
                _adjust = type?.GetMethod("Adjust", new[] { typeof(Player), typeof(float) });
                _setExternal = type?.GetMethod("SetExternalHandling", new[] { typeof(bool) });

                // 077 — perks de tempo/movimento do Médico na cura de aliado (classe irmã).
                var allyType = asm.GetType("CustomClasses.Client.CombatMedicAllyPerks");
                _allyTimeMult = allyType?.GetMethod("AllyHealTimeMult", new[] { typeof(bool) });
                _allyMobile = allyType?.GetMethod("AllyMobileSurgeon", Type.EmptyTypes);
            }
            catch { /* CustomClasses ausente / assinatura mudou → no-op */ }
        }

        /// <summary>True se o CustomClasses (com o perk) está presente. Se false, o penalty fica vanilla.</summary>
        public static bool Available { get { Resolve(); return _adjust != null; } }

        /// <summary>Penalty de cirurgia ajustado pelo perk do OPERADOR (ou intacto se o CustomClasses estiver ausente).</summary>
        public static float AdjustSurgeryPenalty(Player doctor, float penalty)
        {
            Resolve();
            try
            {
                if (_adjust != null) return (float)_adjust.Invoke(null, new object[] { doctor, penalty });
            }
            catch { /* falha de reflection → penalty vanilla (fail-open) */ }
            return penalty;
        }

        /// <summary>
        ///     Marca (para o patch nativo do CustomClasses PULAR) que o ICM está aplicando a cirurgia do packet no
        ///     ActiveHealthController do paciente — o penalty já veio ajustado pela classe do OPERADOR no envio.
        ///     SEMPRE parear true/false num try/finally.
        /// </summary>
        public static void SetExternalHandling(bool value)
        {
            Resolve();
            try { _setExternal?.Invoke(null, new object[] { value }); }
            catch { }
        }

        /// <summary>
        ///     077 — fator de tempo do procedimento de aliado p/ o operador-Médico (🔧 Swift Surgeon na cirurgia /
        ///     🔧 Rapid Care nos demais). <c>1</c> = tempo padrão (não-Médico, perk off, ou CustomClasses ausente).
        ///     <b>Fail-OPEN</b> (tempo normal). O ICM multiplica seu <c>UseTime</c> por este valor.
        /// </summary>
        public static float AllyHealTimeMult(bool isSurgery)
        {
            Resolve();
            try { if (_allyTimeMult != null) return (float)_allyTimeMult.Invoke(null, new object[] { isSurgery }); }
            catch { /* falha de reflection → tempo padrão */ }
            return 1f;
        }

        /// <summary>
        ///     077 — true só se o operador local é Médico com 🔧 <b>Mobile Surgery</b> ligado. <b>Fail-SAFE:</b> false
        ///     (imobiliza) se o CustomClasses estiver ausente. O ICM usa isto p/ decidir se liga <c>HealingLegs</c>
        ///     na cirurgia de aliado (sem isto, ninguém andaria — comportamento seguro do ICM standalone).
        /// </summary>
        public static bool AllyMobileSurgeon()
        {
            Resolve();
            try { if (_allyMobile != null) return (bool)_allyMobile.Invoke(null, new object[0]); }
            catch { /* falha de reflection → imobiliza (seguro) */ }
            return false;
        }
    }
}
