using System.Collections.Generic;
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Observabilidade (D19 + comportamento 8): transições SEMPRE logadas; infra de rolls OFERECIDA aos
    /// consumidores (nenhum roll existe no 002); toast de 1ª ocorrência gateado por consumidor ativo (decisão 20).
    /// Formatos de log ESTÁVEIS/grep-áveis (spec 002 §8 — os ACs são validados por grep).</summary>
    internal static class TraumaObservability
    {
        /// <summary>Linhas já TOASTADAS nesta raid (1×/estado/raid). Só marca quando o toast EXIBE de fato:
        /// supressão por no-consumer NÃO consome a 1ª ocorrência (ligar o consumidor depois ainda mostra
        /// o toast na próxima transição da linha). Zerado no ResetForNewRaid (AC8).</summary>
        private static readonly HashSet<TraumaLine> _seenToasts = new HashSet<TraumaLine>();

        internal static void ResetSeen()
        {
            _seenToasts.Clear();
        }

        internal static void LogTransition(in TraumaTransition t)
        {
            Player p = t.Player;
            string id = p != null ? p.ProfileId : "?";
            string nick = (p != null && p.Profile != null) ? p.Profile.Nickname : "?";
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] {id}/{nick} {t.Region}: {t.From} -> {t.To} reason={t.Reason} mask={FormatMask(t.ReasonMask)} pk={FormatBool(t.PainkillerActive)} establishing={FormatBool(t.Establishing)}");
        }

        internal static void LogOneShot(Player p, TraumaOneShotKind kind, bool suppressedByCooldown)
        {
            string id = p != null ? p.ProfileId : "?";
            if (suppressedByCooldown)
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] one-shot SUPPRESSED (cooldown) {kind} {id}"); // AC6
            else
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] one-shot {kind} {id}");
        }

        /// <summary>Consumida pelos itens 003/006/007 (D19: dano/vida/p/resultado). No 002 só existe — sem call site interno
        /// (AC3 valida a AUSÊNCIA de log de roll nas mudanças de analgésico do estômago).</summary>
        public static void LogRoll(Player p, TraumaRegion region, string condition, float probability, bool result)
        {
            string id = p != null ? p.ProfileId : "?";
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] roll {id} {region} {condition} p={probability:0.###} result={FormatBool(result)}");
        }

        /// <summary>Log do stub Debug Test Consumer (AC6 — replay establishing=true do SubscribeWithSnapshot tardio).</summary>
        internal static void LogDebugConsumer(in TraumaTransition t)
        {
            Player p = t.Player;
            string id = p != null ? p.ProfileId : "?";
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] debug-consumer {id} {t.Region}: {t.From} -> {t.To} establishing={FormatBool(t.Establishing)}");
        }

        internal static void MaybeToastFirstOccurrence(in TraumaTransition t)
        {
            // Gates (spec 002 §5): (1) establishing nunca toasta; entrada de linha apenas (To != None);
            // (2) feedback LOCAL — bots/headless = no-op silencioso; (3) 1ª ocorrência da linha na raid;
            // (4) consumidor ativo da região (decisão 20) — sem consumidor, loga a supressão (AC5).
            if (t.Establishing) return;
            if (t.To == TraumaLine.None) return;
            Player p = t.Player;
            if (p == null || !p.IsYourPlayer) return;
            if (_seenToasts.Contains(t.To)) return;
            if (!TraumaConsumerRegistry.AnyActiveFor(t.Region))
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] toast SUPPRESSED (no consumer) {t.To}"); // AC5
                return; // não consome a 1ª ocorrência
            }
            _seenToasts.Add(t.To);
            // ref: assinatura confirmada por ilspycmd (review 1 da spec): static void DisplayMessageNotification(
            //   string message, ENotificationDurationType duration = Default, ENotificationIconType iconType = Default, Color? textColor = null)
            // Idioma lido no DISPLAY-TIME (P8) — TraumaLocale.Get faz a leitura viva.
            NotificationManagerClass.DisplayMessageNotification(TraumaLocale.Get(TraumaLocale.MapLine(t.To)));
        }

        private static string FormatBool(bool value)
        {
            return value ? "true" : "false"; // minúsculo p/ casar com os greps dos ACs (establishing=true)
        }

        private static string FormatMask(TraumaChangeReason mask)
        {
            // Flags.ToString() gera "A, B" — troca por "|" p/ manter o log em token único grep-ável.
            return mask.ToString().Replace(", ", "|");
        }
    }
}
