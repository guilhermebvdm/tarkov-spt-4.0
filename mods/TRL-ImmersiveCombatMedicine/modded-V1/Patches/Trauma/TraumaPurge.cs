using System.Text;
using TrueTrauma;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>
    /// Auditoria de resíduo de estado nas fronteiras de raid (item 020).
    ///
    /// O mod já limpa tudo — por três caminhos independentes de detecção de fim de raid (motor, helper
    /// de lifecycle dos consumidores e controller médico, todos por polling de GameWorld nulo). O que
    /// este arquivo resolve não é a limpeza: é que ela era INVISÍVEL. Sem contagem no log, um teste não
    /// consegue AFIRMAR que nada vazou, só supor — e o achado S1b do 1º teste in-game nasceu
    /// exatamente dessa dúvida ("este item está persistindo os efeitos entre as raids?").
    ///
    /// Nota de design 1 — mede na ENTRADA, não na saída. A limpeza de fim de raid é uma cascata
    /// assíncrona: cada consumidor limpa no seu próprio Tick e a ordem entre eles não é garantida no
    /// mesmo frame. Auditar no meio dela produziria falso positivo. Na entrada, tudo já deveria estar
    /// zerado, então contagem > 0 é vazamento REAL da raid anterior — a única coisa que importa medir.
    ///
    /// Nota de design 2 — as duas fases medem CONJUNTOS DIFERENTES, e é isso que torna a auditoria
    /// utilizável. Entre elas roda o sweep estabelecedor do motor, que repovoa records/caps/tremor de
    /// propósito quando o jogador entra na raid já ferido (spawn ferido é estado legítimo, não
    /// vazamento — é o que o S1b.1..S1b.4 do plano mestre especifica). Exigir zero nesses campos
    /// depois do sweep acusaria erro em cima do comportamento correto. Então:
    ///
    ///   • "before" → TUDO tem de ser zero: nada rodou ainda, nada é legítimo.
    ///   • "after"  → só o estado TRANSITÓRIO tem de ser zero (desmaio, cooldowns, voz, áudio).
    ///                Estado DERIVADO DE FERIMENTO é esperado e não entra na conta.
    /// </summary>
    internal static class TraumaPurge
    {
        internal const string PhaseBefore = "raid-start-before";
        internal const string PhaseAfter = "raid-start-after";

        /// <summary>
        /// Conta o estado vivo do mod e loga. Resíduo em "before" é vazamento da raid anterior
        /// (warning — a limpeza que roda em seguida resolve). Resíduo em "after" é falha do próprio
        /// mecanismo de limpeza (erro).
        /// </summary>
        internal static void Audit(string phase)
        {
            bool includeInjuryDerived = phase == PhaseBefore;

            // --- Estado TRANSITÓRIO: nunca legítimo no spawn, em nenhuma das duas fases ---
            int blackoutTimers = TraumaState.BlackoutTimers.Count;
            int blackoutStarts = TraumaState.BlackoutStartTimes.Count;
            int fainted = TraumaState.FaintedPlayerIds.Count;
            int grace = TraumaState.GraceTimers.Count;
            int voiceCd = TraumaState.VoiceCooldowns.Count;
            int botFaintCd = TraumaState.BotFaintCooldowns.Count;
            int oneShotCooldowns = TraumaEngine.ResidualCooldowns;
            int voiceWindows = TraumaVoice.ResidualCount;
            bool faintFlag = TraumaState.IsFainted;
            bool intensity = !Mathf.Approximately(TraumaState.EffectIntensity, 0f);
            bool audioDucked = !Mathf.Approximately(AudioListener.volume, 1f);

            int total = blackoutTimers + blackoutStarts + fainted + grace + voiceCd + botFaintCd
                      + oneShotCooldowns + voiceWindows
                      + (faintFlag ? 1 : 0) + (intensity ? 1 : 0) + (audioDucked ? 1 : 0);

            // --- Estado DERIVADO DE FERIMENTO: legítimo depois do sweep estabelecedor ---
            int records = 0, legCaps = 0, windowCaps = 0, pose = 0, botFall = 0;
            bool tremor = false;
            if (includeInjuryDerived)
            {
                records = TraumaEngine.ResidualRecords;
                legCaps = TraumaLegsConsumer.ResidualCount;
                windowCaps = TraumaSpeedCap.ResidualCount;
                pose = TraumaPose.ResidualCount;
                botFall = TraumaBotFall.ResidualCount;
                tremor = TraumaTremor.Owned != null;
                total += records + legCaps + windowCaps + pose + botFall + (tremor ? 1 : 0);
            }

            if (total == 0)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] purge {phase}: clean (total=0{(includeInjuryDerived ? "" : ", transient only")})");
                return;
            }

            // Só campo diferente de zero entra no detalhe — linha legível e grep direto ao culpado.
            var sb = new StringBuilder(192);
            Append(sb, "blackoutTimers", blackoutTimers);
            Append(sb, "blackoutStarts", blackoutStarts);
            Append(sb, "fainted", fainted);
            Append(sb, "grace", grace);
            Append(sb, "voiceCd", voiceCd);
            Append(sb, "botFaintCd", botFaintCd);
            Append(sb, "oneShotCooldowns", oneShotCooldowns);
            Append(sb, "voiceWindows", voiceWindows);
            Append(sb, "records", records);
            Append(sb, "legCaps", legCaps);
            Append(sb, "windowCaps", windowCaps);
            Append(sb, "poseQueue", pose);
            Append(sb, "botFallHolds", botFall);
            if (tremor) sb.Append(" tremor=owned");
            if (faintFlag) sb.Append(" isFainted=true");
            if (intensity) sb.Append($" effectIntensity={TraumaState.EffectIntensity:0.##}");
            if (audioDucked) sb.Append($" audio={AudioListener.volume:0.##}");

            string message = $"[Trauma2] purge {phase}: RESIDUAL total={total}{sb}";

            if (includeInjuryDerived) TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(message);
            else TRLImmersiveCombatMedicinePlugin.ModLogger.LogError(message);
        }

        private static void Append(StringBuilder sb, string label, int value)
        {
            if (value != 0) sb.Append(' ').Append(label).Append('=').Append(value);
        }
    }
}
