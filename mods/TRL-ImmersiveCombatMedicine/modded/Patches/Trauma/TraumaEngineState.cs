using System;
using System.Collections.Generic;
using EFT;
using EFT.HealthSystem; // IHealthController, ValueStruct — fora do dump; ref: ilspycmd IHealthController (trauma-primitives.md P10)

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Regiões de ESTADO da matriz (docs/trauma-matrix.md). Desmaio (tórax/cabeça) é EVENTO — domínio do item 007, fora do motor de estados.</summary>
    public enum TraumaRegion { Legs = 0, Arms = 1, Stomach = 2 } // ordem = ordem determinística de publicação por frame

    /// <summary>
    /// Linhas da matriz. A ordem numérica DENTRO de cada região É o ranking de severidade:
    /// pernas = D1 (Cair+ciclo > Agachar+N2 > Mancar N2 > Mancar N1 > Nada);
    /// braços = decisão 3 (Z2+Q2 2s > Q2 3s > Z2 4s > Tremor > Nada). Comparação numérica resolve "mais severa".
    /// </summary>
    public enum TraumaLine
    {
        None = 0,
        // Pernas (D1)
        LegsLimpN1 = 10,
        LegsLimpN2 = 11,
        LegsCrouchPlusLimpN2 = 12, // "Zerar 2 sem analgésico": one-shot agachar + N2 contínuo
        LegsFallCycle = 13,        // "Quebrar 2" e "Zerar 2 + Quebrar 2" sem analgésico (ciclo 3s/15s é do item 004)
        // Braços (decisão 3 — toda linha AdsCancel inclui Tremor)
        ArmsTremor = 20,
        ArmsTremorAdsCancel4s = 21, // Zerar 2
        ArmsTremorAdsCancel3s = 22, // Quebrar 2 (fratura dói mais — intencional)
        ArmsTremorAdsCancel2s = 23, // Zerar 2 + Quebrar 2
        // Estômago (linha única; roll p=75/25 é do item 006 — motor publica entrada/saída + analgésico DA ENTRADA, D8)
        StomachZeroed = 30
    }

    /// <summary>
    /// BITMASK: múltiplas causas podem coincidir na mesma consolidação (rajada zera perna E quebra braço;
    /// analgésico + dano no mesmo frame). O motor acumula a máscara por região no record; a transição publica
    /// o motivo PRIMÁRIO + a máscara completa (vai no log — spec 002 §8).
    /// Os bits estão NUMERADOS EM ORDEM DE PRECEDÊNCIA (review 2): o BIT MAIS ALTO setado da máscara É o
    /// motivo primário (highest-set-bit — sem tabela paralela).
    /// Precedência (maior→menor): EngineDisabled > InitialEvaluation > PainkillerLost/PainkillerGained >
    /// BodyPartRestored/FractureHealed > FractureGained/Damage > Reconciliation.
    /// </summary>
    [Flags]
    public enum TraumaChangeReason
    {
        None             = 0,
        Reconciliation   = 1 << 0,  // detectado pelo polling (caminho sem evento) — menor precedência
        Damage           = 1 << 1,
        FractureGained   = 1 << 2,
        FractureHealed   = 1 << 3,
        BodyPartRestored = 1 << 4,
        PainkillerGained = 1 << 5,
        PainkillerLost   = 1 << 6,
        InitialEvaluation = 1 << 7, // avaliação estabelecedora (boot/transit/religar master) — Establishing=true; não combina
        EngineDisabled   = 1 << 8   // master off mid-raid → saída de todos os estados; não combina — maior precedência
    }

    /// <summary>One-shots PUBLICADOS pelo motor no 002 (p=100% embutidos em linha de pernas).
    /// O agachar do estômago (p=75/25) é rolado/publicado pelo item 006; desmaio pelo 007.</summary>
    public enum TraumaOneShotKind { InvoluntaryCrouch, InvoluntaryFall }

    public enum TraumaConsumerId { LegsEffects, FallCycle, ArmsEffects, StomachEffects, Blackout2, DebugTest }

    public readonly struct TraumaTransition
    {
        public readonly Player Player;
        public readonly TraumaRegion Region;
        public readonly TraumaLine From;
        public readonly TraumaLine To;              // None em saída total
        public readonly TraumaChangeReason Reason;     // motivo PRIMÁRIO = flag de maior precedência da máscara (doc do enum)
        public readonly TraumaChangeReason ReasonMask; // máscara COMPLETA acumulada na consolidação (pode ter múltiplos bits)
        public readonly bool Establishing;          // true = SEM one-shot e SEM toast (comportamento 5 da spec funcional)
        public readonly bool PainkillerActive;      // predicado no instante da transição; p/ StomachZeroed é o valor LATCHED da entrada (D8)

        public TraumaTransition(Player player, TraumaRegion region, TraumaLine from, TraumaLine to,
            TraumaChangeReason reason, TraumaChangeReason reasonMask, bool establishing, bool painkillerActive)
        { Player = player; Region = region; From = from; To = to; Reason = reason; ReasonMask = reasonMask; Establishing = establishing; PainkillerActive = painkillerActive; }
    }

    public struct TraumaSnapshot
    {
        public TraumaLine Legs;
        public TraumaLine Arms;
        public TraumaLine Stomach;
        public bool StomachPainkillerAtEntry; // D8 — congelado na entrada da zerada
        public bool UnderPainkiller;          // predicado vivo (P3)
    }

    /// <summary>Registro por jogador rastreado. Guarda delegates assinados p/ unsubscribe simétrico (P10 Recomendação (6)).</summary>
    internal sealed class PlayerTraumaRecord
    {
        internal Player Player;
        internal IHealthController Hc;                       // sempre ActiveHealthController-derived (IsOwnedHere)
        internal readonly TraumaLine[] Lines = new TraumaLine[3]; // indexado por TraumaRegion
        internal bool StomachPainkillerAtEntry;              // latch D8
        internal bool LastPainkiller;                        // p/ derivar Gained/Lost no diff consolidado
        /// <summary>Bitmask de motivos ACUMULADA por região desde a última consolidação (review 1, achado 2).
        /// Dirty ≡ PendingReasons[região] != None — um campo só; zerada após publicar.</summary>
        internal readonly TraumaChangeReason[] PendingReasons = new TraumaChangeReason[3];

        // Delegates guardados p/ -= simétrico. OnEffectHandler é UMA closure por-record
        // (IEffect NÃO expõe o dono — review 2), assinada nos 3 eventos de efeito.
        internal Action<EBodyPart, EDamageType> OnDestroyedHandler;
        internal Action<EBodyPart, ValueStruct> OnRestoredHandler;
        internal Action<IEffect> OnEffectHandler;
        internal Action<EBodyPart, float, DamageInfoStruct> OnApplyDamageHandler;

        // Contexto de "motivo" do ApplyDamageEvent (tipo/valor do dano) — consumo em log
        // verbose e pelos itens 003+; NÃO fornece vida pré-tiro (domínio do P7/item 007).
        internal EDamageType LastDamageType;
        internal float LastDamageValue;
    }

    /// <summary>Registry de consumidores (comportamento 9): motor publica sempre; toast é gateado por consumidor ativo (decisão 20).</summary>
    public static class TraumaConsumerRegistry
    {
        private sealed class Entry
        {
            internal TraumaRegion[] Regions;
            internal Func<bool> IsActive;
        }

        private static readonly Dictionary<TraumaConsumerId, Entry> _entries = new Dictionary<TraumaConsumerId, Entry>();

        /// <param name="id">Identidade do consumidor (re-registro substitui — idempotente).</param>
        /// <param name="regions">Regiões de estado cobertas — um consumidor pode cobrir VÁRIAS (review 1, achado 1).
        /// null/vazio = consumidor sem região de estado (ex.: Blackout2/007, que consome IsUnderPainkiller e infra de log).
        /// O Debug Test Consumer registra-se para as TRÊS regiões — é o que destrava o toast do AC5.</param>
        /// <param name="isActive">Predicado vivo (lido a cada consulta — toggles F12 mid-raid respeitados).</param>
        public static void Register(TraumaConsumerId id, TraumaRegion[] regions, Func<bool> isActive)
        {
            _entries[id] = new Entry { Regions = regions, IsActive = isActive };
        }

        public static bool AnyActiveFor(TraumaRegion region)
        {
            foreach (KeyValuePair<TraumaConsumerId, Entry> kv in _entries)
            {
                Entry e = kv.Value;
                if (e.Regions == null || e.IsActive == null) continue;
                for (int i = 0; i < e.Regions.Length; i++)
                {
                    if (e.Regions[i] == region && e.IsActive()) return true;
                }
            }
            return false;
        }
    }
}
