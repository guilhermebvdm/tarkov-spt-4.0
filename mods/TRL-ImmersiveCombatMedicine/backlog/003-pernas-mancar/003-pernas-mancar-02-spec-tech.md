# 003 — Pernas: Mancar N1/N2 + agachar involuntário · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (aguardando review)
**Spec funcional:** [003-pernas-mancar-01-spec.md](003-pernas-mancar-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Segunda fonte canônica: [docs/trauma-primitives.md](../../docs/trauma-primitives.md) — P1 (mancar/velocidade), P4 (pose one-shot/guards), P6 (bots), com os textos CORRIGIDOS da verificação e da Rodada 2. Terceira fonte: o contrato do motor 002 **implementado** ([TraumaEngine.cs](../../modded/Patches/Trauma/TraumaEngine.cs), [TraumaEngineState.cs](../../modded/Patches/Trauma/TraumaEngineState.cs)) — assinaturas citadas por `arquivo:linha` do próprio mod.

## 1. Estratégia

**Primeiro consumidor real do motor 002 — evento-first, 1 patch Harmony novo (sprint), zero polling próprio.**

1. **Consumidor `TraumaLegsConsumer`** (MonoBehaviour no GO do plugin, padrão do repo): assina o motor via `TraumaEngine.SubscribeWithSnapshot` (replay `Establishing=true` cobre assinatura tardia e religar do toggle) + `TraumaEngine.OneShotPublished` ([TraumaEngine.cs:21-22,72](../../modded/Patches/Trauma/TraumaEngine.cs)). Dono-only (D16) é herdado: o motor só rastreia/publica donos (`IsOwnedHere`, [TraumaEngine.cs:110](../../modded/Patches/Trauma/TraumaEngine.cs)). Registra-se no `TraumaConsumerRegistry` para `Legs` — o que destrava o toast de 1ª ocorrência do motor (decisão 20; [TraumaEngineState.cs:132,137](../../modded/Patches/Trauma/TraumaEngineState.cs)).
2. **Cap de velocidade por causa PRÓPRIA** (P1): `MovementContext.AddStateSpeedLimit(cap, (Player.ESpeedLimit)1000)` / `RemoveStateSpeedLimit` — composição vanilla é por **MÍNIMO** do dicionário `SpeedLimits` (D12 corrigido; [MovementContext.cs:1672](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672), :1790, :1798-1824). **Derivação em runtime (decisão 18):** `cap = alvo% × MovementContext.MaxSpeed` — `MaxSpeed` é getter puro que já compõe Strength e os multiplicadores do CustomClasses (baseline composto, D12; [MovementContext.cs:910](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L910)). **Delta nunca negativo por construção:** min() só desacelera e o mod NUNCA remove a causa vanilla `HealthCondition` — quando a penalidade vanilla (0.3/0.2) for mais dura que o alvo, o total experienciado é o vanilla e o caso é logado como CLAMP (excluído da medição ±5 p.p. — AC4). Leitura do efetivo via `StateSpeedLimit` ([MovementContext.cs:639](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L639)).
3. **Sprint (N2):** o cap custom NÃO morde sprint (`StateSprintSpeedLimit` só honra a causa `Fall` — MovementContext.cs:1803-1806, P1). Em N2 o consumidor chama `EnableSprint(false)` ([MovementContext.cs:2783](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2783)) e re-impõe via **postfix em `Player.UpdateSpeedLimitByHealth`** (virtual, Player.cs:29068; o recompute vanilla roda a cada add/remove de efeito e LIBERA sprint sob analgésico — `OnPainkillers` curto-circuita `CanSprint` ANTES dos checks de perna, MovementContext.cs:1256-1258, Rodada 2 do P1). Override no-op só em `ObservedPlayer` (fika ObservedPlayer.cs:462-465) — dono-only por construção (AP-03 a favor).
4. **Primitiva compartilhada de agachar** (`TraumaPose`, reusada pelo 006): one-shot **só-para-baixo** via `SetPoseLevel(0f)` sem force e sem lock (decisão 5; [MovementContext.cs:2139](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139) — `bool SetPoseLevel(float, bool force = false)`); pose corrente já ≤ agachado ou prone → no-op sem consumir cooldown. Guards D7 3 eixos (P4): (a) vault/ar — `IsGrounded` + `CurrentState.Name` fora de {ClimbOver, ClimbUp, VaultingFallDown, VaultingLanding, Jump, FallDown}; (b) BTR — `Player.BtrState == EPlayerBtrState.Outside` ([Player.cs:25413](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25413)) com re-arme por `OnBtrStateChanged` (:25540); (c) escada — soft-dependency tarkin-ladders por reflection (`Type.GetType("tarkin.ladders.bep.PlayerLadderController, tarkin.ladders.bep")`). Guard falhou → **adiar**; na execução re-valida o snapshot (`GetLine(p, Legs) == LegsCrouchPlusLimpN2`) — não exige mais → **CANCELA** e devolve o cooldown (`ReportOneShotCanceled`, API nova no motor); executou → `ReportOneShotExecuted` (cooldown conta da execução — D7, [TraumaEngine.cs:117](../../modded/Patches/Trauma/TraumaEngine.cs)).
5. **Bots (decisão 11):** cap idêntico no `MovementContext` do bot no dono (host/headless) — P1 provou que SAIN (em combate) e BotMover vanilla (fora) convergem no funil `MovementState.ChangeSpeed → SetCharacterMovementSpeed → ClampedSpeed=min(v, StateSpeedLimit)`; zero interferência na IA. Dip de agachar 1× com **devolução imediata** (decisão 16 via P6 rec. (7)): fora de combate `BotOwner.SetPose(0f)` ([BotOwner.cs:1120](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BotOwner.cs#L1120)) + restauração própria em ~0,7 s; em combate SAIN, reflection `BotComponent.Mover.Pose.SetTargetPose(0f)` (null-check + no-op, padrão AggroHelper) e o smooth-damp do SAIN devolve. SEM camada BigBrain aqui — ela é o mecanismo do 004.
6. **Interim Cair→N2 (comportamento 6 da funcional):** o motor publica a linha REAL (`LegsFallCycle`); o consumidor mapeia `LegsFallCycle → efeito N2` e **IGNORA** `OneShotPublished(InvoluntaryFall)` (pertence ao 004). A coluna com-analgésico das linhas Cair já chega resolvida pelo motor como `LegsLimpN1`/`LegsLimpN2` ([TraumaMatrixResolver.cs](../../modded/Patches/Trauma/TraumaMatrixResolver.cs) `ResolveLegs`) — a responsabilidade permanente do 003 é automática, nada a mapear.
7. **Aposentadoria do legado de pernas (D10, comportamento 7):** todo o sistema legado de pernas sai — bloco humano E bloco de bots 90 s do `MainLoopPatch` (o branch de 90 s é a causa-raiz do "levanta e nunca mais cai", P6), sub-bloco de pernas do `HealthPatches`, branches de perna do `CanStandAt` e campos órfãos do `TraumaState`. `Sistema de Pernas` (config antigo) permanece bindado porém **inerte** (migração/remoção no item 010). Desmaio/estômago/braços intactos (fronteiras 007/006/005).

**Alternativas descartadas:** (a) postfix nos getters `MaxSpeed`/`SprintingSpeed` — eixo do CustomClasses, colisão multiplicativa (P1 recomendação, "NÃO usar"); (b) causa `ESpeedLimit.HealthCondition` — o recompute vanilla a limpa a cada tick de saúde (P1 correção); (c) escrever flags `EPhysicalCondition.Left/RightLegDamaged` — o recompute sobrescreve (P1); (d) clamp de pose/velocidade por frame — viola o orçamento (corner da funcional) e briga com SAIN (P6); (e) camada BigBrain para o dip — desproporcional para one-shot sem hold (P6 rec. (7) prescreve o caminho leve; hold é 004).

## 2. Pontos de patch

**1 patch Harmony novo** + hooks C# do motor 002 (o "ponto de patch" real do consumidor):

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/Player.cs:29068` — `UpdateSpeedLimitByHealth`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L29068) (virtual) | Postfix (NOVO) | Recompute vanilla roda a cada add/remove de efeito e libera sprint sob analgésico; o postfix re-impõe `EnableSprint(false)` quando a linha efetiva do jogador é N2-tier e o consumidor está ativo + re-loga o cap efetivo (calibração AC4). Overrides auditados (AP-03): `ObservedPlayer` no-opa (fika ObservedPlayer.cs:462-465 — espelhos imunes, dono-only por construção); `FikaPlayer`/`FikaBot` usam a base (P1 evid.). |

| Hook C# (motor 002) | Assinatura | Uso |
|---|---|---|
| `TraumaEngine.SubscribeWithSnapshot` | `void (Action<TraumaTransition>)` — replay `Establishing=true` dos estados ativos | Entrada/saída/rebaixamento de N1/N2 (From/To/Establishing/PainkillerActive) — [TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs) |
| `TraumaEngine.OneShotPublished` | `event Action<Player, TraumaOneShotKind, TraumaLine>` (já cooldown-gated) | `InvoluntaryCrouch` → primitiva de agachar; `InvoluntaryFall` → IGNORADO (interim) — :22 |
| `TraumaEngine.GetLine` | `TraumaLine (Player, TraumaRegion)` | Re-validação do disparo adiado + religar toggle mid-raid — :48 |
| `TraumaEngine.ReportOneShotExecuted` | `void (Player, TraumaOneShotKind)` | Cooldown conta da EXECUÇÃO no disparo adiado (D7) — :117 |
| `TraumaEngine.ReportOneShotCanceled` | `void (Player, TraumaOneShotKind)` — **API NOVA no motor** | Cancelamento de adiado devolve o cooldown (corner da funcional: "cooldown não consumido") |
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
| `modded/Patches/Trauma/SpeedLimitPatches.cs` | CRIAR | Postfix em `Player.UpdateSpeedLimitByHealth` (§2): re-assert de `EnableSprint(false)` no N2 + re-log do efetivo pós-recompute. |
| `modded/Patches/Trauma/TraumaEngine.cs` | MODIFICAR | (1) API nova `ReportOneShotCanceled(Player, TraumaOneShotKind)` — remove o stamp de cooldown (corner do cancelamento); (2) **verificação do achado de teste "log `reconcile sweep` sem gate verbose"**: na fonte v1.2.1 o gate JÁ existe ([TraumaEngine.cs:585-586](../../modded/Patches/Trauma/TraumaEngine.cs)) — passo 1 da implementação é forense do DLL implantado (lição de memória: build velha mascara fix); se o log reproduzir com Verbose OFF em build atual, reforçar o gate e cobrir o caminho que loga; senão registrar como DLL/cfg stale no memory. |
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

        /// <summary>Efeito aplicado por jogador (dono): linha efetiva + cap corrente (p/ desfazer/re-derivar sem flicker).</summary>
        private readonly Dictionary<string, TraumaLine> _applied = new Dictionary<string, TraumaLine>();
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
            // N1↔N2: AddStateSpeedLimit re-escreve o valor da MESMA causa — sem janela sem cap (corner do flicker).
        }

        private void OnOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            if (!IsActive() || kind != TraumaOneShotKind.InvoluntaryCrouch) return; // InvoluntaryFall = item 004 (interim)
            TraumaPose.TryInvoluntaryCrouch(p); // executa | adia | no-op (só-para-baixo)
        }

        private void ApplyCap(Player p, TraumaLine line)
        {
            var mc = p.MovementContext;
            float pct = LineTargetPercent(line) / 100f;              // N1→config N1 | N2-tier→config N2
            float baseline = mc.MaxSpeed;                             // ref: MovementContext.cs:910 — baseline composto (Strength + CustomClasses postfix, D12)
            float cap = Mathf.Clamp01(pct) * baseline;
            p.AddStateSpeedLimit(cap, TraumaCause);                   // ref: wrapper virtual Player.cs:25820 → MovementContext.cs:1672 (min-composição :1798-1824)
            float effective = mc.StateSpeedLimit;                     // ref: MovementContext.cs:639 — min() do dict
            bool clamped = effective < cap - 0.001f;                  // vanilla 0.3/0.2 mais duro que o alvo → clamp (delta nunca negativo por construção)
            if (line != TraumaLine.LegsLimpN1 && TRLImmersiveCombatMedicinePlugin.ConfigBlockSprintOnN2.Value)
                mc.EnableSprint(false);                               // ref: MovementContext.cs:2783; re-assert no postfix (recompute vanilla libera sob analgésico)
            // log AC4: "[Trauma2] legs cap <profileId> line=<line> target=<pct> cap=<cap:0.###> baseline=<baseline:0.###> effective=<effective:0.###> clamped=<bool>"
        }

        private void RemoveCap(Player p)
        {
            p.RemoveStateSpeedLimit(TraumaCause);                     // ref: Player.cs:25835 → MovementContext.cs:1790
            p.UpdateSpeedLimitByHealth();                             // AP-04: devolve sprint/caps ao estado canônico vanilla (recompute oficial — Player.cs:29068)
            // log: "[Trauma2] legs cap OFF <profileId>"
        }

        private void Update()
        {
            // (a) edge do toggle: off→on = estabelecer do snapshot (GetLine por dono; SEM one-shot/toast — corner religar);
            //     on→off = RemoveCap em todos os _applied + TraumaPose.CancelAll("toggle-off") (corner desligar)
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
        private static readonly List<DeferredCrouch> _deferred = new List<DeferredCrouch>();

        private struct DeferredCrouch { internal Player Player; internal TraumaLine RequiredLine; }

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
        /// mudou (curado/analgésico) → CANCELA + ReportOneShotCanceled (cooldown devolvido — corner do cancelamento).</summary>
        internal static void PumpDeferred() { }

        /// <summary>Cancela todos os adiados (toggle-off / fim de raid) com ReportOneShotCanceled + log
        /// "[Trauma2] crouch CANCELED (<motivo>) <id>".</summary>
        internal static void CancelAll(string reason) { }

        /// <summary>Dip de bot (P6 rec. (7)): fora de combate BotOwner.SetPose(0f) (ref: BotOwner.cs:1120) + restauração própria;
        /// em combate SAIN reflection Mover.Pose.SetTargetPose(0f) (null-check/no-op — padrão AggroHelper). Devolução imediata (decisão 16).</summary>
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
    /// <summary>Re-assert do sprint-block N2 após o recompute vanilla (que LIBERA sprint sob analgésico —
    /// OnPainkillers curto-circuita CanSprint antes dos checks de perna, MovementContext.cs:1256-1258 / P1 Rodada 2).</summary>
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateSpeedLimitByHealth))] // ref: Player.cs:29068 (virtual; ObservedPlayer no-opa — dono-only, AP-03 auditado)
    internal static class UpdateSpeedLimitByHealthPatch
    {
        static void Postfix(Player __instance)
        {
            // consumidor ativo + linha N2-tier do __instance (TraumaEngine.GetLine) + ConfigBlockSprintOnN2
            //   → __instance.MovementContext.EnableSprint(false)  // ref: MovementContext.cs:2783
            // + re-log verbose do StateSpeedLimit efetivo (baseline pode ter mudado — calibração AC4)
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaEngine.cs — ADIÇÃO (API nova; resto do motor intocado)
/// <summary>Cancelamento de one-shot ADIADO (D7 + corner do cancelamento): devolve o cooldown stampado
/// no publish — o efeito nunca executou, a próxima publicação não deve ser suprimida.</summary>
public static void ReportOneShotCanceled(Player player, TraumaOneShotKind kind)
{
    // _instance?._cooldownUntil.Remove((player.ProfileId, kind));
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
[ApplyCap: cap = alvo% × MaxSpeed(:910) → AddStateSpeedLimit(cap, causa 1000)(:1672)]
        │ min() do SpeedLimits (:1798-1824) compõe com vanilla 0.3/0.2 e Skills Extended (Swamp) — clamp logado
        ▼
[ClampedSpeed → SmoothedCharacterMovementSpeed → animator/root-motion (humano E bot)]
        │ peer: PlayerStateData empacota a velocidade suavizada → espelho aplica (P1 dono/peers — sem protocolo custom)
        ▼
[N2: EnableSprint(false) + postfix UpdateSpeedLimitByHealth re-impõe pós-recompute]

[OneShot InvoluntaryCrouch] → TraumaPose.TryInvoluntaryCrouch → no-op(≤agachado) | SetPoseLevel(0f)(:2139) | adia(D7)
        └ adiado → PumpDeferred → guards ok → re-valida GetLine → executa(ReportOneShotExecuted) | cancela(ReportOneShotCanceled)
        └ pose do humano/bot replica ao peer via PlayerStateData (PoseLevel packed — P4 dono/peers)
```

Exemplo AC2 (zerar 2 pernas): motor publica `Legs: LegsLimpN1 -> LegsCrouchPlusLimpN2 reason=Damage` + `one-shot InvoluntaryCrouch` → consumidor aplica capN2 (log com effective/clamped) + `TryInvoluntaryCrouch` (agacha com animação vanilla; levanta livre em seguida). Analgésico: motor rebaixa p/ `LegsLimpN1` (`reason=PainkillerGained`) → cap re-escrito p/ N1 na MESMA causa (sem flicker); expiração → re-entra `CrouchPlusLimpN2` (`PainkillerLost`) e o motor RE-PUBLICA o one-shot (decisão 14) — cooldown do motor decide.

## 7. Riscos e dependências

- **Patches existentes:** `MainLoopPatch` (desmaio/grace/braços permanecem — remoção cirúrgica só dos 2 blocos de perna); `HealthPatches` Postfix (só o sub-bloco de pernas sai; desmaio/estômago/braços são fronteira 007/006/005); `CantStandUpPatch` (branch de blackout permanece). A aposentadoria é MAIOR que o mínimo ("bloco humano do MainLoopPatch"): a funcional #7 exige o legado de pernas "permanentemente inerte independente de config antiga", o que arrasta HealthPatches/InputPatches/TraumaState — **abertura 1** abaixo.
- **Compatibilidade:** CustomClasses (getters MaxSpeed — nosso baseline JÁ compõe por ler o getter vivo; sem patch nos getters = sem colisão), Skills Extended (causa `Swamp` ortogonal — min() compõe), SAIN/ORBIT (funil ChangeSpeed clampado — P1; dip em combate via reflection SAIN com no-op se falhar), Realism (causa `Aiming` — ortogonal). Bots "cheater" do SAIN escrevem velocidade com force fora do funil — cosmético, aceito (P1 risco 5).
- **Ordem de inicialização:** consumidor criado no `Awake` do plugin DEPOIS do motor (`AddComponent<TraumaEngine>()` primeiro — replay do `SubscribeWithSnapshot` no Awake é vazio e inofensivo; estados chegam pelos eventos/re-sweep do raid start).
- **Baseline drift:** `MaxSpeed` muda mid-raid (Strength level-up, buffs) — cap re-derivado a cada transição e re-logado no postfix do recompute; drift fora desses momentos fica ≤ próximo evento (aceito; medição AC4 usa percurso curto).
- **Medição ±5 p.p. (AC4):** nas células SEM analgésico o vanilla 0.3/0.2 (~45%/30% do walk máximo) é normalmente mais duro que os alvos 80/55% → caso CLAMP (logado, excluído da medição — regra da funcional). A medição efetiva do alvo acontece nas células COM analgésico (vanilla se remove sozinho e SÓ a nossa causa segura o cap — P1, "independência perfeita").

### Aberturas explícitas para os reviewers

1. **Escopo da aposentadoria legada:** proposta = remover TAMBÉM o sub-bloco de pernas do `HealthPatches`, os branches de perna do `CanStandAt` e os campos órfãos do `TraumaState` (não só o bloco humano do `MainLoopPatch`) — sem isso o "inerte independente de config antiga" da funcional não fecha e sobra código morto perigoso (prone-em-hit conflita com o interim sem-queda). Confirmar.
2. **Sprint no N1:** o cap custom não morde sprint e o vanilla libera sprint sob analgésico → jogador N1+analgésico pode correr a 100%. Proposta: aceitar como limitação registrada (paridade vanilla; N2 bloqueia via toggle). Alternativa (patch no getter `SprintingSpeed` com compensação +1f) foi descartada — colisão multiplicativa com CustomClasses (P1 LIMITAÇÃO).
3. **`ReportOneShotCanceled` no motor:** única mudança no 002 (além do item de verificação do log). Alternativa sem tocar o motor — consumidor ignora o cooldown residual — viola o corner "cooldown não consumido". Confirmar a API.
4. **Dip de bot em combate:** visibilidade do dip sob smooth-damp do SAIN pode ser baixa (P6 risco 2) — validar in-game; se ilegível, segurar o target 0.3-0.5 s (knob `Bot Crouch Dip Seconds` já cobre o fora-de-combate).
5. **Causa `(ESpeedLimit)1000`:** inferência de linguagem não exercitada in-game (P1 risco 1) — primeiro smoke test do checklist loga `StateSpeedLimit` com a causa aplicada; se algum mod futuro fizer switch exaustivo sobre o enum, trocar o valor.
6. **Vault com pose forçada:** efeito de `SetPoseLevel` durante `Vaulting*` não exercitado (Rodada 2 do P4) — o guard D7 já ADIA nesses estados; incluir 1 caso no smoke test.

## 8. Checklist de implementação

- [ ] Forense do DLL implantado ANTES de codar (achado "reconcile sweep sem gate"): strings do DLL em `D:/SPT/BepInEx/plugins/TRL-ImmersiveCombatMedicine` vs fonte v1.2.1 (gate em TraumaEngine.cs:585-586); corrigir OU registrar stale-DLL no memory.
- [ ] `TraumaEngine.cs`: `ReportOneShotCanceled` (remove stamp de `_cooldownUntil`).
- [ ] `TraumaPose.cs`: guards D7 + só-para-baixo + fila de adiados (re-validação/cancelamento) + dip de bot + cache tarkin-ladders (warn se o mod estiver na pasta e o tipo não resolver).
- [ ] `TraumaLegsConsumer.cs`: registry + SubscribeWithSnapshot + mapa de linhas (interim FallCycle→N2, InvoluntaryFall ignorado) + ApplyCap/RemoveCap (logs de calibração) + edges do toggle + pump.
- [ ] `SpeedLimitPatches.cs`: postfix `UpdateSpeedLimitByHealth` (sprint-block N2 + re-log).
- [ ] Plugin: configs §3 (seção 7; toggle 003 default ON; tooltip inerte no Sistema de Pernas) + `AddComponent<TraumaLegsConsumer>()` + bump 1.3.0.
- [ ] Aposentadoria legada: `MovementPatches` (2 blocos de perna), `HealthPatches` (sub-bloco de pernas SÓ), `InputPatches` (branches de perna/impact), `TraumaState` (campos órfãos + ResetAll).
- [ ] `PROPRIEDADES.md` + regenerar grafo do mod (`/update-mod-graph`) no commit da entrega.
- [ ] Smoke test (mapeia ACs por grep): causa 1000 aplicada (`legs cap ... effective=`), AC1 (N1 entra/sai ≤1 s, cap OFF volta baseline), AC2 (crouch EXECUTED 1× + capN2; analgésico rebaixa/re-escala + re-agacha respeitando cooldown), AC3 (Q2+analgésico→N1; expiração→interim N2 SEM crouch), AC4 (percurso fixo, clamped=true excluído), AC5 (bot manca no host/headless + peer vê; dip 1× + retomada SAIN), AC6 (legado inerte com Sistema de Pernas ON no cfg antigo), AC7 (coop: espelho sem efeito próprio), AC8 (reset entre raids; spawn ferido establishing sem crouch/toast).

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Consumidor herda o lifecycle do motor (ResetForNewRaid publica saídas/limpa); §5 `Update` (b) N1 null-detect limpa `_applied`/adiados; caps vivem no MovementContext que morre com o Player — sem estado vazando entre raids (AC8). |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a player — AP-02 | ✅ | Efeitos só via transições do motor, que só publica DONOS (IsOwnedHere, D16); postfix §2 gateado por `GetLine` (espelhos nunca têm linha) e o próprio alvo é no-op em ObservedPlayer. |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | Único patch: `UpdateSpeedLimitByHealth` virtual — override no-op APENAS em ObservedPlayer (P1 evid.; dono-only por construção); demais APIs são chamadas públicas tipadas (sem reflection, exceto soft-deps tarkin-ladders/SAIN com no-op). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Caps via AddStateSpeedLimit/RemoveStateSpeedLimit (API pública, causa própria); undo chama `UpdateSpeedLimitByHealth()` (recompute oficial); pose via SetPoseLevel (funil do input vanilla — P4); NUNCA escrever flags EPhysicalCondition (§1 alternativas). |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | §5 Update (b) + motor AC8; `_applied`/`_deferred` limpos no null-detect; nada persiste (checklist smoke AC8). |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: defaults/faixas/tooltips; estado neutro (toggle off) = zero efeito; `Sistema de Pernas` documentado INERTE (migração 010). |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | ✅ | `RemoveCap` chama `UpdateSpeedLimitByHealth()` que dispara o próprio postfix — postfix é idempotente (re-assert de EnableSprint + log) e NÃO re-chama o método → sem recursão. |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | `_applied` re-derivado das transições do motor (fonte-verdade re-derivada por evento+polling no 002); adiados re-validam `GetLine` NA execução (§5 PumpDeferred); cap re-derivado por transição (baseline vivo). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (baseada no motor 002 implementado v1.2.1 + trauma-primitives P1/P4/P6 corrigidos) |
