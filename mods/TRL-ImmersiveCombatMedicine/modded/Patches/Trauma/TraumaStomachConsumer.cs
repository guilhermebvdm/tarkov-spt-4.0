using Comfort.Common;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de ESTÔMAGO (spec 006): agachar involuntário probabilístico ao ZERAR o estômago.
    /// O roll nasce AQUI (o motor publica só a transição com o analgésico LATCHED — D8, TraumaEngine.cs:554-561);
    /// o agachar reusa a primitiva do 003 por chamada DIRETA — nunca pelo barramento OneShotPublished (o 003
    /// escuta qualquer InvoluntaryCrouch sem discriminar região, TraumaLegsConsumer.cs:130 — spec 006 §1.4).
    /// Dono-only herdado do motor (D16); bots INCLUSOS (decisão 11) — sem gate de headless.
    /// ZERO patch Harmony novo; ZERO alteração no motor (TraumaEngine/TraumaEngineState/TraumaMatrixResolver).</summary>
    public sealed class TraumaStomachConsumer : MonoBehaviour
    {
        private static TraumaStomachConsumer _instance;

        private bool _wasActive;
        private GameWorld _trackedWorld; // padrão 003/004: world-swap/transit + null-detect

        private static readonly TraumaRegion[] StomachRegions = { TraumaRegion.Stomach };

        private void Awake()
        {
            _instance = this;
            // Registro destrava o toast de 1ª ocorrência da linha (decisão 20; texto TraumaLocale.cs:21/:32).
            // O toast é gate do MOTOR (TraumaObservability.cs:57-77) — dispara na ENTRADA da linha,
            // independente do resultado do roll (funcional §10).
            TraumaConsumerRegistry.Register(TraumaConsumerId.StomachEffects, StomachRegions, IsActive);
            TraumaEngine.SubscribeWithSnapshot(OnTransition); // replay establishing — ref: TraumaEngine.cs:72
            // SEM TraumaEngine.OneShotPublished += ...  — vazamento impossível por construção (spec 006 §1.4)
        }

        /// <summary>Master legado + master Trauma 2.0 + toggle próprio (comportamento 9 do 002).
        /// SEM gate de headless (≠ 005): bots rolam/dipam no processo DONO deles — decisão 11.</summary>
        internal static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerStomachEffects.Value;
        }

        private void OnTransition(TraumaTransition t)
        {
            // ref: CR-01-04 do 004 — exceção de consumidor não pode subir p/ o StateChanged?.Invoke do motor
            try { OnTransitionCore(t); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] StomachConsumer.OnTransition: {ex.Message}");
            }
        }

        private void OnTransitionCore(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Stomach) return;
            if (!IsActive()) return;              // toggle off = ignora (motor segue publicando — 002)
            Player p = t.Player;
            if (p is null) return;
            if (t.To != TraumaLine.StomachZeroed) return; // saída da linha: nada a desfazer (one-shot puro;
                                                          //   adiado pendente morre na re-validação do pump)
            if (t.Establishing) return;           // spawn/religar/adoção: SEM roll, SEM efeito, SEM toast (funcional/AC)

            // ---- ROLL (entrega do 006 — TraumaEngineState.cs:29) ----
            // Analgésico = valor LATCHED que a transição carrega (D8 — instante da detecção da zerada;
            // NUNCA re-consultar IsUnderPainkiller aqui — corner da funcional). ref: TraumaEngine.cs:554-561/:86-88.
            float chance = t.PainkillerActive
                ? TRLImmersiveCombatMedicinePlugin.ConfigStomachCrouchChancePkPercent.Value
                : TRLImmersiveCombatMedicinePlugin.ConfigStomachCrouchChancePercent.Value;
            chance = Mathf.Clamp(chance, 0f, 100f);
            // Extremos DETERMINÍSTICOS (AC1): Random.value é inclusivo em 1.0 — sem o curto-circuito,
            // p=100 poderia falhar (value==1) e p=0 nunca deve suceder. ref: idioma Random.value em
            // VoiceAndHealthUtils.cs:51 (MedicalLogic.cs:366 usa Random.Range — mesmo gênero UnityEngine.Random,
            // não o idioma .value — PA-01-02).
            bool success = chance >= 100f || (chance > 0f && Random.value * 100f < chance);
            TraumaObservability.LogRoll(p, TraumaRegion.Stomach,
                t.PainkillerActive ? "zeroed-pk" : "zeroed", chance / 100f, success); // ref: TraumaObservability.cs:41
            if (!success) return; // falha → nenhum efeito físico (o toast é da LINHA, já tratado pelo motor)

            // ---- Cooldown compartilhado (player, kind=InvoluntaryCrouch) — decisão 19 / funcional §6 ----
            // Pré-check ANTES da primitiva (espelha a ordem do motor — TryPublishOneShot checa cooldown primeiro,
            // TraumaEngine.cs:590-594). Sucesso suprimido é LOGADO e NÃO re-tenta (zerada é evento único).
            if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryCrouch, out float cd) && cd > Time.time)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] stomach-crouch SUPPRESSED (cooldown) {p.ProfileId}");
                return;
            }
            // PA-01-01 (resolvido na review técnica 01): reserva ATÔMICA na decisão de tentar (espelha
            // TryPublishOneShot, que stampa no publish, não na execução) — sem isto, um roll que caia no
            // Defer (D7) fica sem stamp durante toda a espera e uma zerada de pernas na mesma janela
            // executaria livremente, permitindo 2 agachares se o jogador se levantar antes do pump do
            // estômago rodar. Desfeita pelos caminhos que NÃO executam (AbsorbIfCycleEngaged/NOOP pose-baixa
            // já chamam ReportOneShotCanceled — TraumaPose.cs:101-102/:123-124); Defer (TraumaPose.cs:197)
            // captura esta reserva fresca como PublishDeadline, preservando-a durante a espera do D7. A
            // execução real re-stampa dentro da primitiva (idempotente).
            TraumaEngine.ReportOneShotExecuted(p, TraumaOneShotKind.InvoluntaryCrouch);

            // ---- Efeito — chamada DIRETA da primitiva (sem publish; stamps são guard-por-stamp — spec 006 §1.4) ----
            // Desfechos possíveis, todos logados pela primitiva: EXECUTED | DEFERRED (D7) | NOOP pose-baixa |
            // ABSORB (ciclo 004 engajado — AbsorbIfCycleEngaged no topo, TraumaPose.cs:98/:119/:393).
            if (p.IsAI)
            {
                TraumaPose.BotCrouchDip(p, TraumaRegion.Stomach); // dip fire-and-forget — ref: TraumaPose.cs:383
                return;
            }
            if (!p.IsYourPlayer)
            {
                // CR-01-01: defesa extra — motor só publica donos (D16), espelho nunca chega (dead code hoje).
                // Refund do cooldown por segurança: mantém o invariante "toda reserva executa ou refunda"
                // (mesmo padrão de AbsorbIfCycleEngaged/NOOP) caso essa garantia do motor mude no futuro.
                if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryCrouch, out float dMirror))
                    TraumaEngine.ReportOneShotCanceled(p, TraumaOneShotKind.InvoluntaryCrouch, dMirror);
                return;
            }
            TraumaPose.TryInvoluntaryCrouch(p, TraumaRegion.Stomach, TraumaOneShotKind.InvoluntaryCrouch);
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1/003: mundo morreu — cancela SÓ as próprias entradas (ownership explícito; o CancelAll
                // do componente 003 no raid-end é redundância idempotente). Refund vira no-op (cooldowns já resetados).
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "raid-end");
                _trackedWorld = null; _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "world-swap"); // transit
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Toggle OFF mid-raid: rolls param (gate do OnTransitionCore); adiados DO ESTÔMAGO cancelados com
                // refund SEM varrer os de pernas (chave por região — funcional corner do toggle). Legado NÃO volta.
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "toggle-off");
            }
            // Religar mid-raid: NADA a estabelecer (one-shot puro) — estômago já zerado não rola (paridade establishing).
            _wasActive = active;
            if (!active) return;

            // Independência bidirecional (funcional §7): com 003 E 004 OFF, o 006 é o único a pumpar o
            // adiado D7 do estômago e a devolução do dip de bot. Ambos idempotentes com múltiplos chamadores
            // (pump 1×/frame — TraumaPose.cs:246-247; restores por deadline — :422-431).
            TraumaPose.PumpDeferred();
            TraumaPose.PumpBotRestores();
        }
    }
}
