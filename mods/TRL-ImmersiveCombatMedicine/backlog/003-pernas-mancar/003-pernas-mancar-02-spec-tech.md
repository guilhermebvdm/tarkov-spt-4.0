# 003 — Pernas: Mancar N1/N2 + agachar involuntário · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (aguardando review)
**Spec funcional:** [003-pernas-mancar-01-spec.md](003-pernas-mancar-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Segunda fonte canônica: [docs/trauma-primitives.md](../../docs/trauma-primitives.md) — P1 (mancar/velocidade), P4 (pose one-shot/guards), P6 (bots), com os textos CORRIGIDOS da verificação e da Rodada 2. Terceira fonte: o contrato do motor 002 **implementado** ([TraumaEngine.cs](../../modded/Patches/Trauma/TraumaEngine.cs), [TraumaEngineState.cs](../../modded/Patches/Trauma/TraumaEngineState.cs)) — assinaturas citadas por `arquivo:linha` do próprio mod.

## 1. Estratégia

**Primeiro consumidor real do motor 002 — evento-first, 1 patch Harmony novo (sprint), zero polling próprio.**

1. **Consumidor `TraumaLegsConsumer`** (MonoBehaviour no GO do plugin, padrão do repo): assina o motor via `TraumaEngine.SubscribeWithSnapshot` (replay `Establishing=true` cobre assinatura tardia e religar do toggle) + `TraumaEngine.OneShotPublished` ([TraumaEngine.cs:21-22,72](../../modded/Patches/Trauma/TraumaEngine.cs)). Dono-only (D16) é herdado: o motor só rastreia/publica donos (`IsOwnedHere`, [TraumaEngine.cs:110](../../modded/Patches/Trauma/TraumaEngine.cs)). Registra-se no `TraumaConsumerRegistry` para `Legs` — o que destrava o toast de 1ª ocorrência do motor (decisão 20; [TraumaEngineState.cs:132,137](../../modded/Patches/Trauma/TraumaEngineState.cs)).
2. **Cap de velocidade por causa PRÓPRIA** (P1): `MovementContext.AddStateSpeedLimit(cap, (Player.ESpeedLimit)1000)` / `RemoveStateSpeedLimit` — composição vanilla é por **MÍNIMO** do dicionário `SpeedLimits` (público, :384; D12 corrigido; [MovementContext.cs:1672](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672), :1790, :1798-1824). **Re-write de cap (N1↔N2 e rebaixamento por analgésico): `RemoveStateSpeedLimit(causa)` + `AddStateSpeedLimit(novoCap, causa)`** — `AddStateSpeedLimit` é **NO-OP quando a causa já existe** (:1672-1679: só adiciona com `!ContainsKey`; review 1, bloqueador); o par Remove+Add NÃO abre janela sem cap: ambos apenas marcam `SpeedLimitIsDirty` (method_5) e o recompute é único, no `ProcessSpeedLimits` (:2553-2558). **Derivação em runtime (decisão 18):** `cap = alvo% × MovementContext.MaxSpeed` — `MaxSpeed` é getter puro que já compõe Strength e os multiplicadores do CustomClasses (baseline composto, D12; [MovementContext.cs:910](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910)). **Delta nunca negativo por construção:** min() só desacelera e o mod NUNCA remove a causa vanilla `HealthCondition` — quando a penalidade vanilla (0.3/0.2) for mais dura que o alvo, o total experienciado é o vanilla e o caso é logado como CLAMP (excluído da medição ±5 p.p. — AC4). `StateSpeedLimit` (:639) fica STALE até o recompute (dirty-flag) — o log de aplicação computa o esperado LOCALMENTE (min sobre `SpeedLimits` + cap novo); a classificação CLAMP oficial do AC4 sai do re-log do postfix (§2).
3. **Sprint (N2) — gate em `CanSprint` (review 1, strong):** o cap custom NÃO morde sprint (`StateSprintSpeedLimit` só honra a causa `Fall` — MovementContext.cs:1803-1806, P1) e `EnableSprint(false)` ([MovementContext.cs:2783](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2783)) é corte MOMENTÂNEO: sob analgésico `CanSprint` volta `true` (`OnPainkillers` curto-circuita ANTES dos checks de perna, :1256-1258) e re-apertar Shift — ou o SAIN re-decidir — destrava. Mecanismo adotado: **postfix no getter virtual `MovementContext.CanSprint`** (:1240) forçando `false` quando a linha do jogador é N2-tier e o toggle está on; `ObservedMovementContext` sobrescreve `CanSprint` (fika ObservedMovementContext.cs:34) → espelhos imunes por construção (AP-03 a favor). `EnableSprint(false)` permanece na APLICAÇÃO do cap só para cortar sprint EM CURSO. **Desvio registrado da rec. P1** (EnableSprint + flag `SprintDisabled` re-imposto em postfix de `UpdateSpeedLimitByHealth`): o flag corre risco de ser limpo pelos recomputes vanilla (method_0/method_28) — o gate no getter é a fonte estável. O postfix em `Player.UpdateSpeedLimitByHealth` (virtual, Player.cs:29068; no-op só em ObservedPlayer) permanece com papel único de RE-LOG de calibração pós-recompute (fonte da classificação CLAMP — AC4).
4. **Primitiva compartilhada de agachar** (`TraumaPose`, reusada pelo 006): one-shot **só-para-baixo** via `SetPoseLevel(0f)` sem force e sem lock (decisão 5; [MovementContext.cs:2139](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139) — `bool SetPoseLevel(float, bool force = false)`); pose corrente já ≤ agachado ou prone → no-op sem consumir cooldown. Guards D7 3 eixos (P4): (a) vault/ar — `IsGrounded` + `CurrentState.Name` fora de {ClimbOver, ClimbUp, VaultingFallDown, VaultingLanding, Jump, FallDown}; (b) BTR — `Player.BtrState == EPlayerBtrState.Outside` ([Player.cs:25413](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25413)) com re-arme por `OnBtrStateChanged` (:25540); (c) escada — soft-dependency tarkin-ladders por reflection (`Type.GetType("tarkin.ladders.bep.PlayerLadderController, tarkin.ladders.bep")`). Guard falhou → **adiar** (fila com dedup por `(player, kind)`); na execução re-valida o snapshot (`GetLine(p, Legs) == LegsCrouchPlusLimpN2`) — não exige mais → **CANCELA** e devolve o cooldown (`ReportOneShotCanceled`, API nova no motor — só apaga se o stamp corrente ainda for o do publish); executou → `ReportOneShotExecuted` (cooldown conta da execução — D7, [TraumaEngine.cs:117](../../modded/Patches/Trauma/TraumaEngine.cs)). **Voz: SEM voz no 003** (decisão registrada — revisitar com o P5 nos itens 004/006).
5. **Bots (decisão 11):** cap idêntico no `MovementContext` do bot no dono (host/headless) — P1 provou que SAIN (em combate) e BotMover vanilla (fora) convergem no funil `MovementState.ChangeSpeed → SetCharacterMovementSpeed → ClampedSpeed=min(v, StateSpeedLimit)`; zero interferência na IA. Dip de agachar 1× com **devolução imediata** (decisão 16 via P6 rec. (7)): fora de combate `BotOwner.SetPose(0f)` ([BotOwner.cs:1120](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotOwner.cs#L1120)) + restauração própria em ~0,7 s; em combate SAIN, reflection `BotComponent.Mover.Pose.SetTargetPose(0f)` (null-check + no-op, padrão AggroHelper) e o smooth-damp do SAIN devolve. SEM camada BigBrain aqui — ela é o mecanismo do 004.
6. **Interim Cair→N2 (comportamento 6 da funcional):** o motor publica a linha REAL (`LegsFallCycle`); o consumidor mapeia `LegsFallCycle → efeito N2` e **IGNORA** `OneShotPublished(InvoluntaryFall)` (pertence ao 004). A coluna com-analgésico das linhas Cair já chega resolvida pelo motor como `LegsLimpN1`/`LegsLimpN2` ([TraumaMatrixResolver.cs](../../modded/Patches/Trauma/TraumaMatrixResolver.cs) `ResolveLegs`) — a responsabilidade permanente do 003 é automática, nada a mapear.
7. **Aposentadoria do legado de pernas (D10, comportamento 7):** todo o sistema legado de pernas sai — bloco humano E bloco de bots 90 s do `MainLoopPatch` (o branch de 90 s é a causa-raiz do "levanta e nunca mais cai", P6), sub-bloco de pernas do `HealthPatches`, branches de perna do `CanStandAt` e campos órfãos do `TraumaState`. `Sistema de Pernas` (config antigo) permanece bindado porém **inerte** (migração/remoção no item 010). Desmaio/estômago/braços intactos (fronteiras 007/006/005).

**Alternativas descartadas:** (a) postfix nos getters `MaxSpeed`/`SprintingSpeed` — eixo do CustomClasses, colisão multiplicativa (P1 recomendação, "NÃO usar"); (b) causa `ESpeedLimit.HealthCondition` — o recompute vanilla a limpa a cada tick de saúde (P1 correção); (c) escrever flags `EPhysicalCondition.Left/RightLegDamaged` — o recompute sobrescreve (P1); (d) clamp de pose/velocidade por frame — viola o orçamento (corner da funcional) e briga com SAIN (P6); (e) camada BigBrain para o dip — desproporcional para one-shot sem hold (P6 rec. (7) prescreve o caminho leve; hold é 004).

## 2. Pontos de patch

**2 patches Harmony novos** + hooks C# do motor 002 (o "ponto de patch" real do consumidor):

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/MovementContext.cs:1240` — `CanSprint` (getter virtual)](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1240) | Postfix (NOVO) | Gate de sprint do N2 (inclusive sob analgésico — o curto-circuito `OnPainkillers` :1256-1258 devolve `true` antes dos checks de perna): força `__result=false` quando `GetLine(player)==N2-tier` + toggle on. Overrides auditados (AP-03): `ObservedMovementContext` sobrescreve `CanSprint` (fika ObservedMovementContext.cs:34) → espelhos NUNCA passam pelo patch da base — dono-only por construção. |
| [`EFT/Player.cs:29068` — `UpdateSpeedLimitByHealth`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L29068) (virtual) | Postfix (NOVO) | RE-LOG de calibração pós-recompute vanilla (a mudança da causa `HealthCondition` acontece aqui) — fonte oficial da classificação CLAMP do AC4. Sem side-effect de gameplay. Overrides auditados (AP-03): `ObservedPlayer` no-opa (fika ObservedPlayer.cs:462-465); `FikaPlayer`/`FikaBot` usam a base (P1 evid.). |

| Hook C# (motor 002) | Assinatura | Uso |
|---|---|---|
| `TraumaEngine.SubscribeWithSnapshot` | `void (Action<TraumaTransition>)` — replay `Establishing=true` dos estados ativos | Entrada/saída/rebaixamento de N1/N2 (From/To/Establishing/PainkillerActive) — [TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs) |
| `TraumaEngine.OneShotPublished` | `event Action<Player, TraumaOneShotKind, TraumaLine>` (já cooldown-gated) | `InvoluntaryCrouch` → primitiva de agachar; `InvoluntaryFall` → IGNORADO (interim) — :22 |
| `TraumaEngine.GetLine` | `TraumaLine (Player, TraumaRegion)` | Re-validação do disparo adiado + religar toggle mid-raid — :48 |
| `TraumaEngine.ReportOneShotExecuted` | `void (Player, TraumaOneShotKind)` | Cooldown conta da EXECUÇÃO no disparo adiado (D7) — :117 |
| `TraumaEngine.ReportOneShotCanceled` | `void (Player, TraumaOneShotKind, float publishDeadline)` — **API NOVA no motor** (+ consulta interna `TryGetOneShotDeadline`) | Cancelamento de adiado devolve o cooldown (corner: "cooldown não consumido") — remove SÓ se o stamp corrente == deadline do publish (re-ancorado por `ReportOneShotExecuted` não é apagado); null-guard de player |
| `TraumaConsumerRegistry.Register` | `void (TraumaConsumerId, TraumaRegion[], Func<bool>)` | `LegsEffects` cobre `Legs` — destrava toast (decisão 20) — [TraumaEngineState.cs:132](../../modded/Patches/Trauma/TraumaEngineState.cs) |

**APIs de aplicação (sem patch — chamadas diretas no dono):** `MovementContext.AddStateSpeedLimit(float, Player.ESpeedLimit)` (:1672) · `RemoveStateSpeedLimit` (:1790) · `StateSpeedLimit` get (:639) · `MaxSpeed` get (:910) · `EnableSprint(bool)` (:2783) · `SetPoseLevel(float, bool=false)` (:2139) · `Player.BtrState`/`OnBtrStateChanged` (:25413/:25540) · `BotOwner.SetPose(float)` (BotOwner.cs:1120). Enum `Player.ESpeedLimit` tem 9 valores (Player.cs:1584-1595) — causa `1000` está fora do range (inferência C# marcada no P1; smoke test no checklist).

## 3. Novas propriedades F12 (BepInEx)

Seção nova `7. Trauma 2.0 (Pernas)` + 2 edições na seção 6/2. `PROPRIEDADES.md` atualizado na entrega.

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `7. Trauma 2.0 (Pernas)` | `N1 Target Total Speed Percent` | float | `80` | 50 a 95 | — | Velocidade TOTAL experienciada no Mancar N1, em % do baseline (composto com classe/skill). Se a penalidade vanilla for mais dura que o alvo, vale o vanilla (clamp logado — nunca acelera o jogador). |
| `7. Trauma 2.0 (Pernas)` | `N2 Target Total Speed Percent` | float | `55` | 30 a 90 | — | Velocidade TOTAL experienciada no Mancar N2, em % do baseline. Mesma regra de clamp do N1. |
| `7. Trauma 2.0 (Pernas)` | `Block Sprint On N2` | bool | `true` | — | — | Em Mancar N2 o sprint fica bloqueado, inclusive sob analgésico (o vanilla libera sprint com analgésico; este toggle mantém o bloqueio do mod). N1 segue a regra vanilla. |
| `7. Trauma 2.0 (Pernas)` | `Bot Crouch Dip Seconds` | float | `0.7` | 0.3 a 1.5 | Sim | Duração do dip de agachar de bot FORA de combate antes de devolver a pose (em combate o SAIN restaura sozinho). |
| `6. Trauma 2.0 (Consumidores)` | `Legs Effects (item 003)` | bool | **`true`** (era `false`) | — | — | Mancar N1/N2 + agachar involuntário (item 003). Governado pelo master Trauma 2.0; desligar mid-raid desfaz caps e cancela agachares pendentes. |
| `2. Mecanicas (Trauma)` | `Sistema de Pernas` | bool | `true` | — | — | (INERTE desde a v1.3.0 — substituído pelo Trauma 2.0 / Legs Effects. Remoção da key no item 010.) |

Estado neutro: toggle 003 off = zero efeito de pernas do mod (só vanilla + logs do motor). Configs lidas por `.Value` a cada uso (sem cache).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaLegsConsumer.cs` | CRIAR | Componente no GO do plugin: assinatura do motor (snapshot+one-shot), mapa linha→efeito (incl. interim `LegsFallCycle→N2`), aplicação/remoção/re-derivação do cap, sprint-block N2, dip de bot, edges do toggle mid-raid (religar = estabelecer do snapshot sem one-shot/toast; desligar = desfazer tudo), bomba do adiamento (pump da fila do TraumaPose). |
| `modded/Patches/Trauma/TraumaPose.cs` | CRIAR | Primitiva COMPARTILHADA de agachar involuntário (006 reusa): `CanForcePose` (guards D7 3 eixos), `TryInvoluntaryCrouch` só-para-baixo, fila de adiados com re-validação por snapshot + cancelamento, cache reflection do tarkin-ladders. |
| `modded/Patches/Trauma/SpeedLimitPatches.cs` | CRIAR | 2 postfixes (§2): gate de sprint N2 em `MovementContext.CanSprint` (getter virtual — segura inclusive sob analgésico) + re-log de calibração em `Player.UpdateSpeedLimitByHealth` (fonte da classificação CLAMP do AC4). |
| `modded/Patches/Trauma/TraumaEngine.cs` | MODIFICAR | (1) API nova `ReportOneShotCanceled(Player, TraumaOneShotKind, float publishDeadline)` + consulta interna `TryGetOneShotDeadline` — devolve o cooldown SÓ se o stamp corrente for o do publish (null-guard; re-ancorado não é apagado); (2) **verificação do achado de teste "log `reconcile sweep` sem gate verbose"**: na fonte v1.2.1 o gate JÁ existe ([TraumaEngine.cs:585-586](../../modded/Patches/Trauma/TraumaEngine.cs)) — passo 1 da implementação é forense do DLL implantado (lição de memória: build velha mascara fix); se o log reproduzir com Verbose OFF em build atual, reforçar o gate e cobrir o caminho que loga; senão registrar como DLL/cfg stale no memory. |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Binds §3 (seção 7 + default ON do toggle 003 + tooltip inerte do Sistema de Pernas); `AddComponent<TraumaLegsConsumer>()`; bump 1.3.0. |
| `modded/Patches/Trauma/MovementPatches.cs` | MODIFICAR | **Aposentar pernas legadas (D10):** remover o bloco humano (prone force + voz + `LegPenaltyTimers`) E o bloco de bots 90 s (`BotLegsBrokenStartTimes` — clamp de pé permanente, causa-raiz P6). `MainLoopPatch` fica só desmaio/grace/braços. |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Remover SÓ o sub-bloco de pernas do Postfix (seed de `ImpactTimers`/`LegPenaltyTimers` + `IsInPronePose` + voz em hit de perna — a fronteira do 002 §7 cai AQUI, junto com os consumidores). Desmaio, estômago e braços INTACTOS (fronteiras 007/006/005). |
| `modded/Patches/Trauma/InputPatches.cs` | MODIFICAR | `CanStandAt`: remover branches `legsBroken` (10 s humano / 90 s bot) e `ImpactTimers`; manter o branch de blackout. |
| `modded/Patches/Trauma/TraumaState.cs` | MODIFICAR | Remover campos órfãos (`ImpactTimers`, `LegPenaltyTimers`, `BotLegsBrokenStartTimes`) + entradas no `ResetAll`. |
| `modded/Patches/Trauma/TraumaLocale.cs` | MODIFICAR | Calibrar (se preciso) os textos EN/PT das linhas de perna — chaves JÁ existem desde o 002 ([TraumaLocale.cs](../../modded/Patches/Trauma/TraumaLocale.cs)); nenhuma chave nova. |
| `PROPRIEDADES.md` | MODIFICAR | Seção 7 nova; default ON do toggle 003; `Sistema de Pernas` marcado inerte com nota de migração p/ 010 (gate de entrega). |

## 5. Stubs de código

> Pré-código: assinaturas completas + corpo mínimo plausível. Cada referência tem `// ref:`. Assinaturas do EFT re-verificadas no dump; contrato do motor citado do código implementado do 002.

```csharp
// modded/Patches/Trauma/TraumaLegsConsumer.cs
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de PERNAS (item 003): mancar N1/N2 + agachar involuntário. Primeiro consumidor real do motor 002.</summary>
    public sealed class TraumaLegsConsumer : MonoBehaviour
    {
        internal const Player.ESpeedLimit TraumaCause = (Player.ESpeedLimit)1000; // ref: P1 — causa própria fora do enum (Player.cs:1584-1595); dict aceita chave fora do range (inferência marcada)

        /// <summary>Efeito aplicado por DONO rastreado. Keyed por Player (review 1, achado 5): string quebrava
        /// RemoveCap/religar (precisa da referência p/ desfazer e re-estabelecer); limpeza no null-detect do Update.</summary>
        private readonly Dictionary<Player, TraumaLine> _applied = new Dictionary<Player, TraumaLine>();
        private bool _wasActive; // edge do toggle mid-raid

        private void Awake()
        {
            TraumaConsumerRegistry.Register(TraumaConsumerId.LegsEffects, new[] { TraumaRegion.Legs }, IsActive); // destrava toast (decisão 20)
            TraumaEngine.SubscribeWithSnapshot(OnTransition);   // replay establishing cobre assinatura tardia — ref: TraumaEngine.cs:72
            TraumaEngine.OneShotPublished += OnOneShot;         // ref: TraumaEngine.cs:22
        }

        private static bool IsActive()
        {
            // master legado + master 2.0 (o motor já para de publicar sem eles) + toggle próprio
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerLegsEffects.Value;
        }

        private void OnTransition(TraumaTransition t)
        {
            // Só Legs; toggle off = ignora (motor segue publicando — comportamento 9 do 002).
            // Mapa linha→efeito: LegsLimpN1→cap N1 | LegsLimpN2/LegsCrouchPlusLimpN2/LegsFallCycle→cap N2 (INTERIM: FallCycle vira N2 — comportamento 6)
            // None→remover. Establishing aplica cap SEM one-shot (one-shot nem chega: motor suprime — 002).
            // N1↔N2/rebaixamento: SEMPRE via ApplyCap (Remove+Add — review 1, BLOQUEADOR: AddStateSpeedLimit é
            // NO-OP se a causa já existe, MovementContext.cs:1672-1679); sem janela sem cap (recompute único).
        }

        private void OnOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            if (!IsActive() || kind != TraumaOneShotKind.InvoluntaryCrouch) return; // InvoluntaryFall = item 004 (interim)
            if (p.IsAI)
            {
                TraumaPose.BotCrouchDip(p); // review 1, achado 4: bot NÃO entra na fila de adiados — fire-and-forget
                return;                     // (dip reporta ReportOneShotExecuted internamente — cooldown vale p/ bot também)
            }
            TraumaPose.TryInvoluntaryCrouch(p); // humano: executa | adia | no-op (só-para-baixo)
        }

        private void ApplyCap(Player p, TraumaLine line)
        {
            var mc = p.MovementContext;
            float pct = LineTargetPercent(line) / 100f;              // N1→config N1 | N2-tier→config N2
            float baseline = mc.MaxSpeed;                             // ref: MovementContext.cs:910 — baseline composto (Strength + CustomClasses postfix, D12)
            float cap = Mathf.Clamp01(pct) * baseline;
            // Re-write da MESMA causa exige Remove+Add (review 1, BLOQUEADOR — Add é no-op com causa existente :1672-1679);
            // sem flicker: ambos só marcam SpeedLimitIsDirty (method_5) e o recompute é único no ProcessSpeedLimits (:2553-2558)
            p.RemoveStateSpeedLimit(TraumaCause);                     // ref: Player.cs:25835 → MovementContext.cs:1790
            p.AddStateSpeedLimit(cap, TraumaCause);                   // ref: Player.cs:25820 → MovementContext.cs:1672 (min-composição :1798-1824)
            // review 1, achado 3: StateSpeedLimit fica STALE até o recompute (dirty-flag) — esperado computado LOCALMENTE:
            // float expected = cap; foreach (var kv in mc.SpeedLimits) expected = Mathf.Min(expected, kv.Value); // ref: dict público MovementContext.cs:384
            // bool clampedExpected = expected < cap - 0.001f; // classificação CLAMP OFICIAL do AC4 sai do re-log do postfix (§2)
            if (line != TraumaLine.LegsLimpN1 && TRLImmersiveCombatMedicinePlugin.ConfigBlockSprintOnN2.Value)
                mc.EnableSprint(false);                               // corta sprint EM CURSO; quem SEGURA é o gate CanSprint (§2 — review 1, strong)
            // log aplicação: "[Trauma2] legs cap <profileId> line=<line> target=<pct> cap=<cap:0.###> baseline=<baseline:0.###> expected=<expected:0.###> clamped=<bool>"
        }

        private void RemoveCap(Player p)
        {
            p.RemoveStateSpeedLimit(TraumaCause);                     // ref: Player.cs:25835 → MovementContext.cs:1790
            p.UpdateSpeedLimitByHealth();                             // AP-04: devolve sprint/caps ao estado canônico vanilla (recompute oficial — Player.cs:29068)
            // log: "[Trauma2] legs cap OFF <profileId>"
        }

        private void Update()
        {
            // (a) edge do toggle: off→on = estabelecer do snapshot — itera Singleton<GameWorld>.Instance.RegisteredPlayers
            //     + TraumaEngine.IsOwnedHere (dependência INTERNAL registrada — mesmo assembly; review 1, achado 5)
            //     + GetLine por dono; SEM one-shot/toast (corner religar);
            //     on→off = RemoveCap em todos os _applied (keyed por Player) + TraumaPose.CancelAll("toggle-off")
            // (b) GameWorld null (padrão N1) → _applied.Clear() + TraumaPose.CancelAll("raid-end") (caps morrem com o mundo)
            // (c) TraumaPose.PumpDeferred() — re-checa guards D7; contexto válido → re-valida GetLine e executa/cancela
            // (d) dip de bot pendente: restauração após ConfigBotCrouchDipSeconds (fora de combate)
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaPose.cs
using System;
using System.Collections.Generic;
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Primitiva COMPARTILHADA de agachar involuntário (003/006). One-shot SÓ-PARA-BAIXO, sem lock (decisão 5).</summary>
    internal static class TraumaPose
    {
        private static Type _ladderType; // tarkin-ladders soft-dep — ref: P4 rec. (3c); resolver 1x, warn se falhar com o mod presente
        /// <summary>Fila de adiados com DEDUP por (player, kind) — enqueue de par já presente é no-op (review 1, achado 6).
        /// PublishDeadline = stamp de cooldown capturado no enqueue via TraumaEngine.TryGetOneShotDeadline —
        /// o cancel só devolve cooldown se o stamp corrente ainda for esse (re-ancorado não é apagado).</summary>
        private static readonly List<DeferredCrouch> _deferred = new List<DeferredCrouch>();

        private struct DeferredCrouch
        {
            internal Player Player;
            internal TraumaOneShotKind Kind;   // dedup por (player, kind) — primitiva compartilhada (006 reusa)
            internal TraumaLine RequiredLine;
            internal float PublishDeadline;    // p/ ReportOneShotCanceled(player, kind, publishDeadline)
        }

        /// <summary>Guards D7 (3 eixos, TODOS adiam): vault/ar, BTR, escada (tarkin-ladders).</summary>
        internal static bool CanForcePose(Player p)
        {
            // (a) ref: P4 rec. (3a) — MovementContext.IsGrounded (:1089) && CurrentState.Name (:732) ∉ {ClimbOver, ClimbUp, VaultingFallDown, VaultingLanding, Jump, FallDown}
            // (b) ref: Player.cs:25413 — p.BtrState == EPlayerBtrState.Outside
            // (c) ref: P4 rec. (3c) — _ladderType == null || p.GetComponent(_ladderType) == null
            return false;
        }

        /// <summary>Agachar one-shot: pose já ≤ agachado ou prone → NO-OP sem consumir cooldown (funcional §3);
        /// guard falhou → ADIA (registra em _deferred); ok → SetPoseLevel(0f) SEM force (animação vanilla, sem lock — P4 rec. (1)).</summary>
        internal static void TryInvoluntaryCrouch(Player p)
        {
            // p.MovementContext.PoseLevel (:1016) <= ~0f || p.IsInPronePose → no-op ("[Trauma2] crouch NOOP (pose already low) <id>")
            // !CanForcePose → _deferred.Add(...) ("[Trauma2] crouch DEFERRED (<guard>) <id>")
            // ok = p.MovementContext.SetPoseLevel(0f) (:2139); ok=false → adiar (guard interno recusou)
            // executou → TraumaEngine.ReportOneShotExecuted(p, InvoluntaryCrouch) ("[Trauma2] crouch EXECUTED <id>") — D7: cooldown da execução
        }

        /// <summary>Pump (1x/frame pelo consumidor): contexto válido → RE-VALIDA TraumaEngine.GetLine(p, Legs) == RequiredLine;
        /// mudou (curado/analgésico) → CANCELA + ReportOneShotCanceled(p, Kind, PublishDeadline) (cooldown devolvido
        /// SÓ se o stamp não foi re-ancorado — review 1, achado 6).</summary>
        internal static void PumpDeferred() { }

        /// <summary>Cancela todos os adiados (toggle-off / fim de raid) com ReportOneShotCanceled(..., PublishDeadline) + log
        /// "[Trauma2] crouch CANCELED (<motivo>) <id>".</summary>
        internal static void CancelAll(string reason) { }

        /// <summary>Dip de bot (P6 rec. (7)) — FIRE-AND-FORGET, nunca entra na fila de adiados (review 1, achado 4):
        /// fora de combate BotOwner.SetPose(0f) (ref: BotOwner.cs:1120) + restauração própria em ConfigBotCrouchDipSeconds;
        /// em combate SAIN reflection Mover.Pose.SetTargetPose(0f) (null-check/no-op — padrão AggroHelper). Devolução
        /// imediata (decisão 16). Dip APLICADO → TraumaEngine.ReportOneShotExecuted(botPlayer, InvoluntaryCrouch)
        /// (cooldown vale p/ bot também); log "[Trauma2] bot dip <profileId> mode=<sain|vanilla>".</summary>
        internal static void BotCrouchDip(Player botPlayer) { }
    }
}
```

```csharp
// modded/Patches/Trauma/SpeedLimitPatches.cs
using HarmonyLib;
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Gate de sprint do N2 (review 1, strong): EnableSprint(false) é corte momentâneo e CanSprint
    /// devolve true sob OnPainkillers (curto-circuito :1256-1258) — re-apertar Shift/SAIN re-decidir destravava.
    /// O gate no getter é a fonte estável (desvio registrado da rec. P1: o flag SprintDisabled corre risco de
    /// ser limpo pelos recomputes vanilla method_0/method_28).</summary>
    [HarmonyPatch(typeof(MovementContext), nameof(MovementContext.CanSprint), MethodType.Getter)] // ref: MovementContext.cs:1240 (virtual; ObservedMovementContext sobrescreve :34 — espelhos imunes, AP-03 auditado)
    internal static class CanSprintPatch
    {
        static void Postfix(MovementContext __instance, ref bool __result)
        {
            // if (!__result) return; // já bloqueado — nada a fazer
            // player do contexto (campo _player — padrão TraumaState.PlayerField já usado no CantStandUpPatch)
            // consumidor ativo + ConfigBlockSprintOnN2 + TraumaEngine.GetLine(player, Legs) é N2-tier → __result = false;
        }
    }

    /// <summary>RE-LOG de calibração pós-recompute vanilla (a causa HealthCondition muda AQUI) — fonte oficial
    /// da classificação CLAMP do AC4 (review 1, achado 3). Sem side-effect de gameplay.</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateSpeedLimitByHealth))] // ref: Player.cs:29068 (virtual; ObservedPlayer no-opa — dono-only, AP-03 auditado)
    internal static class UpdateSpeedLimitByHealthPatch
    {
        static void Postfix(Player __instance)
        {
            // consumidor ativo + _applied contém __instance →
            //   expected = min(capPróprio, valores de __instance.MovementContext.SpeedLimits)  // ref: dict público :384
            //   clamped = expected < capPróprio − 0.001f
            //   log: "[Trauma2] legs cap RECOMPUTE <profileId> cap=<cap:0.###> expected=<expected:0.###> clamped=<bool>"  // fonte CLAMP do AC4
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaEngine.cs — ADIÇÕES (APIs novas; resto do motor intocado)
/// <summary>Cancelamento de one-shot ADIADO (D7 + corner do cancelamento): devolve o cooldown stampado no
/// publish — o efeito nunca executou. Remove SÓ se o stamp corrente ainda for o do publish: um cooldown
/// re-ancorado por ReportOneShotExecuted NÃO é apagado (review 1 do 003, achado 6).</summary>
public static void ReportOneShotCanceled(Player player, TraumaOneShotKind kind, float publishDeadline)
{
    // if (player is null || _instance == null) return; // null-guard (achado 6); `is null` = padrão fake-null do motor
    // var key = (player.ProfileId, kind);
    // if (_instance._cooldownUntil.TryGetValue(key, out float d) && Mathf.Approximately(d, publishDeadline))
    //     _instance._cooldownUntil.Remove(key);
}

/// <summary>Consulta interna p/ a fila de adiados capturar o deadline do publish no enqueue (mesmo assembly).</summary>
internal static bool TryGetOneShotDeadline(Player player, TraumaOneShotKind kind, out float deadline)
{
    // deadline = _cooldownUntil[(player.ProfileId, kind)] se existir; senão false
    deadline = 0f; return false;
}
```

## 6. Fluxo de dados

```
[dano/cura/fratura/analgésico no DONO]
        ▼
[motor 002: eventos AHC → linha de pernas (TraumaMatrixResolver.ResolveLegs)]
        │ StateChanged / OneShotPublished(InvoluntaryCrouch) / (InvoluntaryFall → IGNORADO, interim)
        ▼
[TraumaLegsConsumer.OnTransition]
        ├── mapa: LimpN1→capN1 | LimpN2/CrouchPlusLimpN2/FallCycle→capN2 | None→remover
        ▼
[ApplyCap: cap = alvo% × MaxSpeed(:910) → Remove(:1790)+Add(:1672) na causa 1000 (Add é no-op com causa existente — bloqueador)]
        │ dirty-flag → recompute ÚNICO no ProcessSpeedLimits (:2553-2558); min() do SpeedLimits (:1798-1824)
        │ compõe com vanilla 0.3/0.2 e Skills Extended (Swamp) — expected local no log; CLAMP oficial no postfix
        ▼
[ClampedSpeed → SmoothedCharacterMovementSpeed → animator/root-motion (humano E bot)]
        │ peer: PlayerStateData empacota a velocidade suavizada → espelho aplica (P1 dono/peers — sem protocolo custom)
        ▼
[N2: EnableSprint(false) corta sprint EM CURSO + gate CanSprint (postfix :1240) SEGURA — inclusive sob analgésico]

[OneShot InvoluntaryCrouch] → TraumaPose.TryInvoluntaryCrouch → no-op(≤agachado) | SetPoseLevel(0f)(:2139) | adia(D7)
        └ adiado → PumpDeferred → guards ok → re-valida GetLine → executa(ReportOneShotExecuted) | cancela(ReportOneShotCanceled)
        └ pose do humano/bot replica ao peer via PlayerStateData (PoseLevel packed — P4 dono/peers)
```

Exemplo AC2 (zerar 2 pernas): motor publica `Legs: LegsLimpN1 -> LegsCrouchPlusLimpN2 reason=Damage` + `one-shot InvoluntaryCrouch` → consumidor aplica capN2 (log com effective/clamped) + `TryInvoluntaryCrouch` (agacha com animação vanilla; levanta livre em seguida). Analgésico: motor rebaixa p/ `LegsLimpN1` (`reason=PainkillerGained`) → cap re-escrito p/ N1 via Remove+Add na mesma causa (recompute único — sem janela sem cap); expiração → re-entra `CrouchPlusLimpN2` (`PainkillerLost`) e o motor RE-PUBLICA o one-shot (decisão 14) — cooldown do motor decide.

## 7. Riscos e dependências

- **Patches existentes:** `MainLoopPatch` (desmaio/grace/braços permanecem — remoção cirúrgica só dos 2 blocos de perna); `HealthPatches` Postfix (só o sub-bloco de pernas sai; desmaio/estômago/braços são fronteira 007/006/005); `CantStandUpPatch` (branch de blackout permanece). A aposentadoria é MAIOR que o mínimo ("bloco humano do MainLoopPatch"): a funcional #7 exige o legado de pernas "permanentemente inerte independente de config antiga", o que arrasta HealthPatches/InputPatches/TraumaState — **abertura 1** abaixo.
- **Compatibilidade:** CustomClasses (getters MaxSpeed — nosso baseline JÁ compõe por ler o getter vivo; sem patch nos getters = sem colisão), Skills Extended (causa `Swamp` ortogonal — min() compõe), SAIN/ORBIT (funil ChangeSpeed clampado — P1; dip em combate via reflection SAIN com no-op se falhar), Realism (causa `Aiming` — ortogonal). Bots "cheater" do SAIN escrevem velocidade com force fora do funil — cosmético, aceito (P1 risco 5).
- **Ordem de inicialização:** consumidor criado no `Awake` do plugin DEPOIS do motor (`AddComponent<TraumaEngine>()` primeiro — replay do `SubscribeWithSnapshot` no Awake é vazio e inofensivo; estados chegam pelos eventos/re-sweep do raid start).
- **Baseline drift:** `MaxSpeed` muda mid-raid (Strength level-up, buffs) — cap re-derivado a cada transição e re-logado no postfix do recompute; drift fora desses momentos fica ≤ próximo evento (aceito; medição AC4 usa percurso curto).
- **Medição ±5 p.p. (AC4):** nas células SEM analgésico o vanilla 0.3/0.2 (~45%/30% do walk máximo) é normalmente mais duro que os alvos 80/55% → caso CLAMP (logado, excluído da medição — regra da funcional). A medição efetiva do alvo acontece nas células COM analgésico (vanilla se remove sozinho e SÓ a nossa causa segura o cap — P1, "independência perfeita").

### Aberturas explícitas para os reviewers

1. **Escopo da aposentadoria legada:** proposta = remover TAMBÉM o sub-bloco de pernas do `HealthPatches`, os branches de perna do `CanStandAt` e os campos órfãos do `TraumaState` (não só o bloco humano do `MainLoopPatch`) — sem isso o "inerte independente de config antiga" da funcional não fecha e sobra código morto perigoso (prone-em-hit conflita com o interim sem-queda). Confirmar.
2. **Sprint no N1:** o cap custom não morde sprint e o vanilla libera sprint sob analgésico → jogador N1+analgésico pode correr a 100%. Proposta: aceitar como limitação registrada (paridade vanilla; N2 bloqueia via toggle). Alternativa (patch no getter `SprintingSpeed` com compensação +1f) foi descartada — colisão multiplicativa com CustomClasses (P1 LIMITAÇÃO).
3. **`ReportOneShotCanceled` no motor:** única mudança no 002 (além do item de verificação do log) — assinatura com `publishDeadline` + consulta interna `TryGetOneShotDeadline` (o cancel não pode apagar cooldown re-ancorado por execução). Alternativa sem tocar o motor — consumidor ignora o cooldown residual — viola o corner "cooldown não consumido". Confirmar a API.
4. **Dip de bot em combate:** visibilidade do dip sob smooth-damp do SAIN pode ser baixa (P6 risco 2) — validar in-game; se ilegível, segurar o target 0.3-0.5 s (knob `Bot Crouch Dip Seconds` já cobre o fora-de-combate).
5. **Causa `(ESpeedLimit)1000`:** inferência de linguagem não exercitada in-game (P1 risco 1) — primeiro smoke test do checklist loga `StateSpeedLimit` com a causa aplicada; se algum mod futuro fizer switch exaustivo sobre o enum, trocar o valor.
6. **Vault com pose forçada:** efeito de `SetPoseLevel` durante `Vaulting*` não exercitado (Rodada 2 do P4) — o guard D7 já ADIA nesses estados; incluir 1 caso no smoke test.

## 8. Checklist de implementação

- [ ] Forense do DLL implantado ANTES de codar (achado "reconcile sweep sem gate"): strings do DLL em `D:/SPT/BepInEx/plugins/TRL-ImmersiveCombatMedicine` vs fonte v1.2.1 (gate em TraumaEngine.cs:585-586); corrigir OU registrar stale-DLL no memory.
- [ ] `TraumaEngine.cs`: `ReportOneShotCanceled(player, kind, publishDeadline)` (null-guard; remove stamp SÓ se == deadline do publish) + `TryGetOneShotDeadline` interna.
- [ ] `TraumaPose.cs`: guards D7 + só-para-baixo + fila de adiados (re-validação/cancelamento) + dip de bot + cache tarkin-ladders (warn se o mod estiver na pasta e o tipo não resolver).
- [ ] `TraumaLegsConsumer.cs`: registry + SubscribeWithSnapshot + mapa de linhas (interim FallCycle→N2, InvoluntaryFall ignorado) + ApplyCap/RemoveCap (logs de calibração) + edges do toggle + pump.
- [ ] `SpeedLimitPatches.cs`: postfix `CanSprint` (gate N2 — segura sob analgésico) + postfix `UpdateSpeedLimitByHealth` (re-log RECOMPUTE = fonte CLAMP do AC4).
- [ ] Plugin: configs §3 (seção 7; toggle 003 default ON; tooltip inerte no Sistema de Pernas) + `AddComponent<TraumaLegsConsumer>()` + bump 1.3.0.
- [ ] Aposentadoria legada: `MovementPatches` (2 blocos de perna), `HealthPatches` (sub-bloco de pernas SÓ), `InputPatches` (branches de perna/impact), `TraumaState` (campos órfãos + ResetAll).
- [ ] `PROPRIEDADES.md` + regenerar grafo do mod (`/update-mod-graph`) no commit da entrega.
- [ ] Smoke test (mapeia ACs por grep): causa 1000 aplicada (`legs cap ... expected=`), AC1 (N1 entra/sai ≤1 s, cap OFF volta baseline), AC2 (crouch EXECUTED 1× + capN2; analgésico rebaixa/re-escala + re-agacha respeitando cooldown), AC3 (Q2+analgésico→N1; expiração→interim N2 SEM crouch), AC4 (percurso fixo; `clamped=true` do log RECOMPUTE do postfix excluído da medição), AC5 (bot manca no host/headless + peer vê; dip 1× + retomada SAIN), AC6 (legado inerte com Sistema de Pernas ON no cfg antigo), AC7 (coop: espelho sem efeito próprio), AC8 (reset entre raids; spawn ferido establishing sem crouch/toast).
- [ ] Smoke test extra (review 1): **N2 + analgésico + re-apertar sprint** → gate `CanSprint` segura (sem destravar); **agachar disparando DURANTE sprint** → transição segura (corner da funcional).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Consumidor herda o lifecycle do motor (ResetForNewRaid publica saídas/limpa); §5 `Update` (b) N1 null-detect limpa `_applied`/adiados; caps vivem no MovementContext que morre com o Player — sem estado vazando entre raids (AC8). |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a player — AP-02 | ✅ | Efeitos só via transições do motor, que só publica DONOS (IsOwnedHere, D16); postfix §2 gateado por `GetLine` (espelhos nunca têm linha) e o próprio alvo é no-op em ObservedPlayer. |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | 2 patches em alvos virtuais, ambos com override auditado A FAVOR: `CanSprint` (ObservedMovementContext sobrescreve :34 — espelhos nunca passam pela base) e `UpdateSpeedLimitByHealth` (no-op só em ObservedPlayer — P1 evid.); demais APIs são chamadas públicas tipadas (sem reflection, exceto soft-deps tarkin-ladders/SAIN com no-op). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Caps via AddStateSpeedLimit/RemoveStateSpeedLimit (API pública, causa própria); undo chama `UpdateSpeedLimitByHealth()` (recompute oficial); pose via SetPoseLevel (funil do input vanilla — P4); NUNCA escrever flags EPhysicalCondition (§1 alternativas). |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | §5 Update (b) + motor AC8; `_applied`/`_deferred` limpos no null-detect; nada persiste (checklist smoke AC8). |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: defaults/faixas/tooltips; estado neutro (toggle off) = zero efeito; `Sistema de Pernas` documentado INERTE (migração 010). |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | ✅ | `RemoveCap` chama `UpdateSpeedLimitByHealth()` que dispara o próprio postfix — postfix é SÓ log (sem side-effect) e não re-chama o método; postfix de `CanSprint` só lê `GetLine`/config e escreve `__result` (nenhuma chamada que re-dispare o getter) → sem recursão. |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | `_applied` re-derivado das transições do motor (fonte-verdade re-derivada por evento+polling no 002); adiados re-validam `GetLine` NA execução (§5 PumpDeferred); cap re-derivado por transição (baseline vivo). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (baseada no motor 002 implementado v1.2.1 + trauma-primitives P1/P4/P6 corrigidos) |
| 2026-07-19 | Review técnica rodada 1 aplicada — 7 achados; bloqueador do re-write de cap (`AddStateSpeedLimit` é no-op com causa existente :1672-1679 → padrão Remove+Add, recompute único no ProcessSpeedLimits :2553-2558); strong do sprint (gate em `CanSprint` virtual, desvio da rec. P1 registrado); log de calibração com expected local + CLAMP oficial no postfix; fork bot→BotCrouchDip fire-and-forget com ReportOneShotExecuted; `_applied` keyed por Player + religar via RegisteredPlayers/IsOwnedHere; cancel com publishDeadline + dedup da fila; +2 casos de smoke e decisão SEM voz no 003 |
