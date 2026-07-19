using System;
using System.Reflection;
using BepInEx.Bootstrap;
using EFT;

namespace Band_Aid
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
    }
}
