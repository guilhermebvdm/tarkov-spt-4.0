# 002 — Motor de estados de trauma · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (aguardando review)
**Spec funcional:** [002-motor-estados-01-spec.md](002-motor-estados-01-spec.md)
**Criado:** 2026-07-18

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. **Neste item, a segunda fonte canônica é [docs/trauma-primitives.md](../../docs/trauma-primitives.md)** (spike 001, verificado em 2 rodadas adversariais): tipos fora do dump (102 namespaces vazios — `EFT.HealthSystem` é um deles) são citados pelos artefatos ilspycmd do spike (`scratchpad/spike001/*.cs`), com a linha do doc de primitivas que os prova. Onde o doc tem "Correções da verificação", esta spec usa o **texto corrigido**.

## 1. Estratégia

**Motor 100% dirigido a eventos C# do health controller — ZERO patches Harmony novos.** O veredito do P10 (observação de estado, [trauma-primitives.md:493-495](../../docs/trauma-primitives.md)) é EVENTO-first: todas as transições da matriz (Zerar/des-Zerar/Quebrar/des-Quebrar/analgésico) têm evento nativo no dono, declarado na própria interface `IHealthController`. O motor:

1. **Componente no GameObject do plugin** (`gameObject.AddComponent<TraumaEngine>()` no `Awake`) — padrão já provado no repo: o boot do EFT destrói GOs órfãos criados no chainloader; o manager do BepInEx sobrevive a sessão inteira ([TRLImmersiveCombatMedicinePlugin.cs:79-85](../../modded/TRLImmersiveCombatMedicinePlugin.cs)).
2. **Autoridade dono-only (D16)** por predicado de tipo: `player.HealthController is ActiveHealthController` — humano local (`ClientHealthController : GClass3010 : ActiveHealthController`) e bots no host/headless (`BotHealthController : GClass3010 : ActiveHealthController`) passam; espelhos (`NetworkHealthControllerAbstractClass`) são excluídos naturalmente (P10 Recomendação (1), [trauma-primitives.md:495](../../docs/trauma-primitives.md); evidência :525).
3. **Conjunto rastreado dirigido por evento:** registro via `GameWorld.OnPersonAdd` (cobre bots que spawnam mid-raid) + sweep inicial de `RegisteredPlayers` no raid start; remoção via `Player.OnPlayerDeadOrUnspawn` (dispara em `OnDead` E em `Dispose` — cobre morte, extração, despawn e desconexão). Nunca varredura por frame.
4. **Reavaliação consolidada por frame:** handlers de evento apenas marcam `(player, região)` como dirty (com bitmask de motivos); `TraumaEngine.Update()` processa o dirty-set 1×/frame em ordem determinística (players por ordem de inserção; regiões na ordem do enum). Cumpre decisão 14 ("na hora" = **≤1 frame após o evento** — mesmo frame ou o seguinte; a ordem de update entre MonoBehaviours não garante same-frame) e o corner de rajada (reavaliação única, eventos ordenados).
5. **Polling de reconciliação ≤4 Hz** via acumulador no `Update` — SÓ para os caminhos comprovadamente sem evento: `FullRestoreBodyPart` (sem Invoke de `BodyPartRestoredEvent`), revive do Fika (`RestoreBodyPartNoEvents`) e transit heal (P10 evidência :524; D19 confirmado em [trauma-primitives.md:505](../../docs/trauma-primitives.md)).
6. **Resolução da matriz como função pura:** contagens por região (zeradas/quebradas, D4: mesmo membro conta as duas) + predicado de analgésico → avalia TODAS as linhas satisfeitas e escolhe a de maior severidade (decisão 2), com ranking codificado na ordem numérica do enum (D1 pernas; decisão 3 braços). Estômago é linha única LATCHED com analgésico do instante da entrada (D8) — imune a reavaliação por mudança de analgésico.

**Alternativas descartadas:** (a) Postfix Harmony em `Player.ApplyDamageInfo` (padrão do sistema legado, HealthPatches.cs) — não vê cura/expiração de efeito, é alvo virtual (AP-03) e duplicaria o que os eventos do AHC já entregam; (b) polling por frame (padrão do `MainLoopPatch` legado) — viola D19/orçamento; (c) hook de expiração de analgésico via `EffectRemovedEvent` — **refutado pelo P3**: `Removed` só dispara após o FadeOut do item (+5 a +50 s); o instante em que `Active` flipa false é `EffectResidualEvent` ([trauma-primitives.md:142](../../docs/trauma-primitives.md)).

A **aposentadoria da injeção legacy** (decisão 21) sai neste item: remove-se o roll de 30% fratura / 15 de dano ao tentar levantar com 2 pernas zeradas (`MovementPatches.cs:151-170`), mantendo o resto do sistema legado de pernas (re-forçar prone, voz, `LegPenaltyTimers`) — que só será substituído nos itens 003/004.

## 2. Pontos de patch

**Nenhum patch Harmony novo.** O único ponto Harmony tocado é o patch manual **já existente** do plugin, cujo corpo é estendido:

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/GameWorld.cs` — `OnGameStarted`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs) (patch manual existente, [TRLImmersiveCombatMedicinePlugin.cs:117-121](../../modded/TRLImmersiveCombatMedicinePlugin.cs)) | Prefix (existente, corpo estendido) | `OnRaidStartCleanup` passa a chamar também `TraumaEngine.OnRaidStarted()` (reset idempotente + subscribe + avaliação inicial estabelecedora). Dispara de novo na chegada de transit (novo GameWorld). |

**Hooks por evento C# (o "ponto de patch" real do motor)** — assinaturas na própria interface `IHealthController` (fora do dump; provadas por ilspycmd + protótipo compilado no spike — `scratchpad/spike001/IHealthController.cs:36-66,72,74,90` via [trauma-primitives.md:509,518](../../docs/trauma-primitives.md)):

| Hook | Assinatura | Transição coberta | Evidência |
|---|---|---|---|
| `BodyPartDestroyedEvent` | `Action<EBodyPart, EDamageType>` | Zerar (Invoke em `DestroyBodyPart`, só na transição) | AHC:3867-3877 (P10 evid. :519) |
| `BodyPartRestoredEvent` | `Action<EBodyPart, ValueStruct>` | des-Zerar (cirurgia local E remota — `ApplySurgeryFromNetwork` chama `RestoreBodyPart` nativo) | AHC:3891-3910 + [BandAidNetworkHandler.cs:544](../../modded/Patches/Medical/BandAidNetworkHandler.cs) (P10 evid. :522-523) |
| `EffectStartedEvent` | `Action<IEffect>` filtrado `e is GInterface342 \| GInterface358 \| GInterface350` | Quebrar / analgésico começa (de-escala na hora) | AHC state machine :585-635 (P10 evid. :520-521) |
| `EffectResidualEvent` | `Action<IEffect>` mesmo filtro | des-Quebrar / **expiração do analgésico** (decisão 14 — instante em que `Active` flipa false; `ForceRemove`/`ForceResidue` percorrem o mesmo state machine, logo cura remota via `method_15<T>` dispara igual) | P3 correção ([trauma-primitives.md:142](../../docs/trauma-primitives.md)) + P10 evid. :520-522 + [BandAidNetworkHandler.cs:502-509](../../modded/Patches/Medical/BandAidNetworkHandler.cs) |
| `EffectRemovedEvent` | `Action<IEffect>` mesmo filtro | cinto de segurança (nunca mecanismo primário) | P10 Recomendação (2) |
| `ApplyDamageEvent` | `Action<EBodyPart, float, DamageInfoStruct>` | contexto de "motivo" da transição (tipo/valor do dano). **NÃO fornece vida pré-tiro** (dispara APÓS `ChangeHealth` — domínio do P7/item 007) | AHC ~3411 (âncora P7, [001-spike-primitivas-02-spec-tech.md:55](../001-spike-primitivas/001-spike-primitivas-02-spec-tech.md)) + P10 Recomendação (7) |
| `GameWorld.OnPersonAdd` | `event Action<IPlayer>` | registro de player/bot (inclusive mid-raid) — invocado no fim de `RegisterPlayer` | [GameWorld.cs:991](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L991) (declaração) e [:2305](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2305) (Invoke) |
| `Player.OnPlayerDeadOrUnspawn` | `event GDelegate71` = `void (Player)` | remoção do rastreamento (morte, extração, despawn, desconexão) — invocado em `OnDead` e em `Dispose` | [Player.cs:25510](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25510) + Invokes [:30554](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L30554)/[:31415](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31415) + [GDelegate71.cs:3](../../../../references/eft-decompiled/Assembly-CSharp/GDelegate71.cs#L3); uso no mod: [BandAidController.cs:557](../../modded/Patches/Medical/BandAidController.cs) |

> Por que **não** `EffectAddedEvent`: efeito em `Added` ainda não está `Active` (`Active => State == Started`, AHC:231); `Started` é a transição semântica de "condição começou" — mesma razão pela qual o vanilla assina `EffectStartedEvent` no `Player.Init` (Player.cs:28660-28666, P10 evid. :520 e P3 evid. :167).

## 3. Novas propriedades F12 (BepInEx)

Seções novas — as seções 1–4 existentes ([TRLImmersiveCombatMedicinePlugin.cs:45-65](../../modded/TRLImmersiveCombatMedicinePlugin.cs)) não mudam. `PROPRIEDADES.md` do mod será atualizado na entrega (gate do checklist §8). Keys em EN (migração dos textos antigos é o item 010).

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `5. Trauma 2.0 (Motor)` | `Enable Trauma 2.0` | bool | `true` | — | — | Liga o motor de estados de trauma. Sem consumidores ligados não há NENHUM efeito de gameplay — só rastreamento e log. Desligar mid-raid publica a saída de todos os estados ativos. |
| `5. Trauma 2.0 (Motor)` | `Include Adrenaline As Painkiller` | bool | `true` | — | — | Berserk/adrenalina conta como analgésico (paridade com o jogo — é o que o EFT considera em OnPainkillers). |
| `5. Trauma 2.0 (Motor)` | `One-Shot Cooldown Seconds` | float | `4.0` | 3 a 5 | — | Anti-thrash (decisão 19): o mesmo one-shot involuntário (agachar/cair) não re-dispara nesse intervalo, por jogador e por tipo. Ciclos internos dos consumidores são isentos. |
| `5. Trauma 2.0 (Motor)` | `Reconciliation Polling Hz` | float | `2.0` | 1 a 4 | Sim | Frequência do polling de reconciliação (cobre só caminhos sem evento: cirurgia FullRestore, revive do Fika, transit heal). Teto 4 Hz (D19). |
| `5. Trauma 2.0 (Motor)` | `Verbose Engine Log` | bool | `false` | — | Sim | Loga detalhes de avaliação/polling. Transições de estado e supressões são SEMPRE logadas, independente desta opção. |
| `6. Trauma 2.0 (Consumidores)` | `Legs Effects (item 003)` | bool | `false` | — | — | Placeholder — efeitos de mancar N1/N2. Sem função até o item 003. |
| `6. Trauma 2.0 (Consumidores)` | `Fall Cycle (item 004)` | bool | `false` | — | — | Placeholder — cair + ciclo de levantar. Sem função até o item 004. |
| `6. Trauma 2.0 (Consumidores)` | `Arms Effects (item 005)` | bool | `false` | — | — | Placeholder — tremor + cancela-ADS. Sem função até o item 005. |
| `6. Trauma 2.0 (Consumidores)` | `Stomach Effects (item 006)` | bool | `false` | — | — | Placeholder — agachar involuntário do estômago. Sem função até o item 006. |
| `6. Trauma 2.0 (Consumidores)` | `Blackout 2.0 (item 007)` | bool | `false` | — | — | Placeholder — desmaio percentual. Sem função até o item 007 (o desmaio ATUAL segue no toggle antigo "Sistema de Desmaio"). |
| `6. Trauma 2.0 (Consumidores)` | `Debug Test Consumer` | bool | `false` | — | Sim | Consumidor de teste SEM efeito de gameplay: registra-se ATIVO para as TRÊS regiões (pernas/braços/estômago), destravando o toast/i18n para validação (AC5 da spec funcional). |

**Semântica dos toggles (comportamento 9 da funcional):** o motor publica sempre que `Ativar Mod` (master legado) **e** `Enable Trauma 2.0` estiverem on; consumidores se auto-gateiam pelos toggles da seção 6 e, ao serem desligados mid-raid, desfazem os próprios efeitos (regra do corner do master aplicada por consumidor — itens 003+). Estado neutro do motor = rastrear e publicar, zero efeito. `Avançado = Sim` implementado via atributo do ConfigurationManager (`IsAdvanced`), DLL já referenciada em `modded/References/`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaEngine.cs` | CRIAR | Componente MonoBehaviour no GO do plugin: registro/remoção por evento, handlers→dirty, consolidação 1×/frame, polling de reconciliação, cooldown anti-thrash, avaliação estabelecedora, master-off mid-raid, API estática de eventos/snapshot/consulta. |
| `modded/Patches/Trauma/TraumaEngineState.cs` | CRIAR | Contrato: enums (`TraumaRegion`, `TraumaLine` com ranking, `TraumaChangeReason`, `TraumaOneShotKind`, `TraumaConsumerId`), structs (`TraumaTransition`, `TraumaSnapshot`), registro por jogador (`PlayerTraumaRecord`) e `TraumaConsumerRegistry`. |
| `modded/Patches/Trauma/TraumaMatrixResolver.cs` | CRIAR | Resolução PURA da matriz: contagens→linha por região (todas as linhas satisfeitas → maior severidade; D1/decisão 3/D4); estômago não passa aqui (latch D8 no engine). |
| `modded/Patches/Trauma/TraumaLocale.cs` | CRIAR | i18n EN/PT do motor: `IsGamePortuguese()` (P8), tabela de strings por enum, leitura no display-time, fallback EN na race de boot. |
| `modded/Patches/Trauma/TraumaObservability.cs` | CRIAR | Infra de log: transições (sempre), supressões (toast/cooldown) e `LogRoll(...)` oferecida aos consumidores 003/006/007 (D19) — exercitada no 002 só por transição. Toast de 1ª ocorrência (gateado por consumidor ativo, decisão 20). |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Binds das configs novas (§3); `gameObject.AddComponent<TraumaEngine>()` junto aos componentes existentes (:84-85); `OnRaidStartCleanup` (:200-207) chama `TraumaEngine.OnRaidStarted()`. |
| `modded/Patches/Trauma/MovementPatches.cs` | MODIFICAR | **Aposentar injeção legacy (decisão 21):** remover `:151-170` (roll de 30% fratura via `DoFracture` + 15 de dano via `ApplyDamage`); MANTER re-forçar prone + voz + `LegPenaltyTimers[id]=now` (:172-179) — o resto do sistema legado de pernas sai nos itens 003/004. Consumidores de `LegPenaltyTimers` seguem válidos ([InputPatches.cs:88](../../modded/Patches/Trauma/InputPatches.cs) — gate de 10 s no `CanStandAt`). |
| `modded/Patches/Trauma/HealthPatches.cs` | NÃO MODIFICAR (fronteira) | O rastreamento do motor NÃO passa pelo patch `ApplyDamageInfo` legado — é por eventos do AHC (§2). O postfix legado (desmaio/estômago/pernas/braços atuais) permanece intacto até os itens 003–007. Registrar a fronteira em comentário é opcional na implementação. |
| `modded/Patches/Trauma/TraumaState.cs` | NÃO MODIFICAR | Estado do sistema LEGADO. O motor tem estado próprio (`PlayerTraumaRecord`) — sem acoplamento. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar seções 5 e 6 novas do F12 (gate de entrega). |

## 5. Stubs de código

> Pré-código: assinaturas completas + corpo mínimo plausível, SEM implementação. Cada referência ao EFT tem `// ref:`. Tipos fora do dump citam o artefato ilspycmd do spike via doc de primitivas (P3/P8/P10).

```csharp
// modded/Patches/Trauma/TraumaEngineState.cs
using System;
using System.Collections.Generic;
using EFT;
using EFT.HealthSystem; // IHealthController, ValueStruct — fora do dump; ref: scratchpad/spike001/IHealthController.cs (trauma-primitives.md P10)

namespace TRLImmersiveCombatMedicine.Trauma;

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
/// o motivo PRIMÁRIO (flag de maior precedência) + a máscara completa (vai no log — §8).
/// Precedência (maior→menor): EngineDisabled > InitialEvaluation > PainkillerGained/PainkillerLost >
/// BodyPartRestored/FractureHealed > Damage/FractureGained > Reconciliation.
/// </summary>
[Flags]
public enum TraumaChangeReason
{
    None             = 0,
    Damage           = 1 << 0,
    FractureGained   = 1 << 1,
    FractureHealed   = 1 << 2,
    BodyPartRestored = 1 << 3,
    PainkillerGained = 1 << 4,
    PainkillerLost   = 1 << 5,
    InitialEvaluation = 1 << 6, // avaliação estabelecedora (boot/transit/religar master) — Establishing=true; não combina
    Reconciliation   = 1 << 7,  // detectado pelo polling (caminho sem evento)
    EngineDisabled   = 1 << 8   // master off mid-raid → saída de todos os estados; não combina
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
    public readonly bool Establishing;          // true = SEM one-shot e SEM toast (comportamento 5)
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
    // delegates guardados p/ -=: Action<EBodyPart,EDamageType>, Action<EBodyPart,ValueStruct>, Action<IEffect> ×3, Action<EBodyPart,float,DamageInfoStruct>
}

/// <summary>Registry de consumidores (comportamento 9): motor publica sempre; toast é gateado por consumidor ativo (decisão 20).</summary>
public static class TraumaConsumerRegistry
{
    /// <param name="regions">Regiões de estado cobertas — um consumidor pode cobrir VÁRIAS (review 1, achado 1).
    /// null/vazio = consumidor sem região de estado (ex.: Blackout2/007, que consome IsUnderPainkiller e infra de log).
    /// O Debug Test Consumer registra-se para as TRÊS regiões — é o que destrava o toast do AC5.</param>
    public static void Register(TraumaConsumerId id, TraumaRegion[] regions, Func<bool> isActive) { /* dicionário estático */ }
    public static bool AnyActiveFor(TraumaRegion region) { return false; /* itera os registros: OR dos isActive() que cobrem a região */ }
}
```

```csharp
// modded/Patches/Trauma/TraumaEngine.cs
using System;
using System.Collections.Generic;
using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma;

/// <summary>
/// Motor de estados Trauma 2.0. Componente no GameObject do PLUGIN (padrão do repo —
/// TRLImmersiveCombatMedicinePlugin.cs:79-85). Dono-only (D16). Sem Harmony próprio.
/// </summary>
public sealed class TraumaEngine : MonoBehaviour
{
    // ---- Contrato público (comportamento 3 da funcional) ----
    public static event Action<TraumaTransition> StateChanged;
    public static event Action<Player, TraumaOneShotKind, TraumaLine> OneShotPublished; // já cooldown-gated

    public static TraumaLine GetLine(Player player, TraumaRegion region) { return TraumaLine.None; /* lookup no record */ }
    public static bool TryGetSnapshot(Player player, out TraumaSnapshot snapshot) { snapshot = default; return false; }

    /// <summary>Assinante tardio: recebe replay dos estados ativos como Establishing=true antes das transições futuras.</summary>
    public static void SubscribeWithSnapshot(Action<TraumaTransition> handler) { /* replay + StateChanged += handler */ }

    /// <summary>Consulta "analgésico ativo agora" (consumida pelo 007 — decisões 9/15). Predicado do P3.</summary>
    public static bool IsUnderPainkiller(Player player)
    {
        // ref: predicado vanilla OnPainkillers — Player.cs:29070 (dump); trauma-primitives.md P3 Recomendação (1)
        IHealthController hc = player?.HealthController;
        if (hc == null) return false;
        if (hc.FindActiveEffect<GInterface358>() != null) return true;  // PainKiller — ref: GInterface358 público, scratchpad/spike001/GInterface358.cs (P3 evid.)
        return TRLImmersiveCombatMedicinePlugin.ConfigIncludeAdrenaline.Value
            && hc.FindActiveEffect<GInterface350>() != null;            // Berserk/adrenalina — ref: Player.cs:29070
    }

    /// <summary>Autoridade D16: humano local e bots do host/headless têm ActiveHealthController; espelhos são NetworkHealthControllerAbstractClass.</summary>
    internal static bool IsOwnedHere(Player p)
    {
        // ref: trauma-primitives.md P10 Recomendação (1) — "guard de autoridade natural"; fika ClientHealthController.cs:14, BotHealthController.cs:11-12
        return p != null && p.HealthController is ActiveHealthController;
    }

    /// <summary>Item 004/006 (adiamento D7): re-ancora o cooldown do one-shot na EXECUÇÃO (comportamento 6 — "cooldown conta da execução").</summary>
    public static void ReportOneShotExecuted(Player player, TraumaOneShotKind kind) { /* re-stamp _cooldownUntil */ }

    // ---- Lifecycle (AP-01) ----
    /// <summary>Chamado do prefix existente de GameWorld.OnGameStarted (Plugin.OnRaidStartCleanup) — inclusive transit (novo GameWorld).</summary>
    internal static void OnRaidStarted()
    {
        // 1. ResetForNewRaid() (idempotente); 2. subscribe GameWorld.OnPersonAdd; 3. sweep RegisteredPlayers → TrackPlayer(establishing:true)
        // ref: OnPersonAdd — GameWorld.cs:991 (declaração), :2305 (Invoke no fim de RegisterPlayer — cobre bots mid-raid)
        // ref: RegisteredPlayers — uso existente no mod, BandAidNetworkHandler.cs:480
    }

    /// <summary>Limpeza TOTAL, idempotente: unsubscribe de tudo, zera records, cooldowns e toasts-vistos (AC8).</summary>
    internal static void ResetForNewRaid() { }

    private void Update()
    {
        // (a) master legado + master Trauma 2.0: se qualquer um caiu com estados ativos → publicar saídas (Reason=EngineDisabled) e UntrackAll; religar → re-sweep estabelecedor
        // (b) null-detect de GameWorld (padrão N1 do BandAidController.cs:151-166): GameWorld sumiu → ResetForNewRaid()
        // (c) dirty-set: consolidar 1×/frame em ordem determinística (players por inserção; regiões na ordem do enum)
        // (d) polling: _pollAccumulator += Time.deltaTime; se >= 1/ConfigPollingHz → Reconcile() (re-deriva contagens de TODOS os rastreados; nunca por frame)
        // Orçamento: sem alocação no caminho quente — dirty como bool[3] no record; listas pré-alocadas reusadas.
    }

    // ---- Rastreamento ----
    private void OnPersonAdd(IPlayer person)
    {
        // ref: GameWorld.cs:991/2305 — dispara p/ QUALQUER registro (donos e espelhos); IsOwnedHere filtra
        var p = person as Player;
        if (!IsOwnedHere(p)) return;
        TrackPlayer(p, establishing: true); // primeira avaliação de bot mid-raid é estabelecedora (corner da funcional)
    }

    private void TrackPlayer(Player p, bool establishing)
    {
        // Idempotente (OnPersonAdd + sweep podem se sobrepor). Assinaturas EXATAS (P10 Recomendação (2), protótipo compilado :509):
        IHealthController hc = p.HealthController;
        // hc.BodyPartDestroyedEvent += (EBodyPart part, EDamageType type) => MarkDirty(rec, part, TraumaChangeReason.Damage);           // Zerar — ref: AHC DestroyBodyPart :3867-3877 (P10 evid. :519)
        // hc.BodyPartRestoredEvent  += (EBodyPart part, ValueStruct before) => MarkDirty(rec, part, TraumaChangeReason.BodyPartRestored); // des-Zerar — ref: AHC RestoreBodyPart :3891-3910 (P10 evid. :523)
        // hc.EffectStartedEvent     += OnEffectEvent;   // Quebrar / analgésico começa
        // hc.EffectResidualEvent    += OnEffectEvent;   // des-Quebrar / analgésico EXPIRA — ref: P3 correção (NÃO EffectRemovedEvent — Removed = FadeOut +5..50s)
        // hc.EffectRemovedEvent     += OnEffectEvent;   // cinto de segurança
        // hc.ApplyDamageEvent       += (EBodyPart part, float damage, DamageInfoStruct info) => { /* só contexto de motivo */ }; // ref: AHC ~3411 (âncora P7)
        // p.OnPlayerDeadOrUnspawn   += UntrackPlayer;   // ref: Player.cs:25510 (GDelegate71 = void(Player) — GDelegate71.cs:3); Invoke em OnDead :30554 e Dispose :31415
        // Estado inicial pelo QUERY, não por evento (Player.Init já rodou PropagateAllEffects — P3 Recomendação (3)):
        // EvaluatePlayer(rec, establishing) → linhas iniciais; establishing NÃO publica one-shot nem toast (só log "estabelecido").
    }

    private void OnEffectEvent(IEffect e)
    {
        // Filtro por INTERFACE, nunca nested type concreto (P10 Recomendação (5)):
        // e is GInterface342 (fratura — ref: GClass3009.cs:960-963 via P10 evid. :517) → MarkDirty(rec, RegionOf(e.BodyPart), FractureGained|FractureHealed conforme Started/Residual)
        // e is GInterface358 || e is GInterface350 (analgésico) → MarkDirty pernas+braços com PainkillerGained (Started) / PainkillerLost (Residual) (estômago NÃO — latch D8);
        //   re-check de expiração é feito na consolidação re-derivando IsUnderPainkiller (doses de itens diferentes = instâncias separadas — P3 Recomendação (2))
    }

    private void UntrackPlayer(Player p)
    {
        // Limpeza SEM transições espúrias (comportamento 1): unsubscribe simétrico + remove record + limpa cooldowns do jogador. Sem StateChanged.
    }

    // ---- Avaliação consolidada ----
    private void EvaluatePlayer(PlayerTraumaRecord rec, bool establishing)
    {
        // 1. pk = IsUnderPainkiller(rec.Player); pkChanged = pk != rec.LastPainkiller
        // 2. Contagens (D4 — mesmo membro conta as duas condições):
        //    zeroedLegs = IsBodyPartDestroyed(LeftLeg)+(RightLeg); brokenLegs = IsBodyPartBroken(LeftLeg)+(RightLeg); idem braços
        //    ref: IHealthController.IsBodyPartDestroyed/IsBodyPartBroken — scratchpad/spike001/IHealthController.cs:72,74 (P10 evid. :516-517)
        // 3. Pernas/braços: newLine = TraumaMatrixResolver.Resolve*(counts, pk); diff → transição
        //    (From, To, Reason = flag de MAIOR precedência de PendingReasons[região], ReasonMask = máscara completa, establishing, pk)
        // 4. Estômago (latch D8): entrada quando IsBodyPartDestroyed(Stomach) flipa true → StomachPainkillerAtEntry = pk (congelado);
        //    saída quando flipa false; mudança de analgésico NÃO gera transição de estômago (AC3; nova chance só em NOVA zerada — decisão 7)
        // 5. Publicação (ordem determinística Legs→Arms→Stomach): StateChanged; se !establishing e a linha nova embute one-shot:
        //    entrar em LegsCrouchPlusLimpN2 → TryPublishOneShot(InvoluntaryCrouch); entrar em LegsFallCycle → TryPublishOneShot(InvoluntaryFall)
        //    (re-entrada por expiração de analgésico TAMBÉM publica — decisão 14; cooldown decide)
        // 6. TraumaObservability.LogTransition(...); TraumaObservability.MaybeToastFirstOccurrence(...) (gate: humano local + consumidor ativo da região)
    }

    private bool TryPublishOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
    {
        // Cooldown anti-thrash (decisão 19): Dictionary<(string profileId, TraumaOneShotKind), float> _cooldownUntil;
        // agora < deadline → LogOneShot(suppressed:true), return false; senão publica OneShotPublished + stamp ConfigOneShotCooldown.Value
        return false;
    }

    private void Reconcile()
    {
        // Re-deriva contagens de todos os rastreados e roda EvaluatePlayer(rec, establishing:false) só p/ divergentes
        // (Reason=Reconciliation). Cobre FullRestoreBodyPart (sem evento — AHC :3912-3921, P10 evid. :524),
        // revive Fika (RestoreBodyPartNoEvents — fika ClientHealthController.cs:196-207) e transit heal.
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaMatrixResolver.cs
namespace TRLImmersiveCombatMedicine.Trauma;

/// <summary>Resolução PURA da matriz (docs/trauma-matrix.md): avalia TODAS as linhas cuja condição casa
/// e retorna a de maior severidade (decisão 2) — ranking = ordem numérica do enum (D1 / decisão 3).</summary>
internal static class TraumaMatrixResolver
{
    internal static TraumaLine ResolveLegs(int zeroed, int broken, bool painkiller)
    {
        // Sem analgésico:  Z1→N1 | Q1→N1 | Z1+Q1→N2 | Z2→CrouchPlusLimpN2 | Q2→FallCycle | Z2+Q2→FallCycle
        // Com analgésico:  Z1→None | Q1→None | Z1+Q1→N1 | Z2→N1 | Q2→N1 | Z2+Q2→N2
        // Combos mistos (ex.: Z2+Q1): max() das linhas satisfeitas → CrouchPlusLimpN2 sem analgésico / N1 com.
        return TraumaLine.None;
    }

    internal static TraumaLine ResolveArms(int zeroed, int broken, bool painkiller)
    {
        // Sem analgésico:  Z1/Q1/Z1+Q1→Tremor | Z2→AdsCancel4s | Q2→AdsCancel3s | Z2+Q2→AdsCancel2s
        // Com analgésico:  Z1/Q1/Z1+Q1→None | Z2/Q2/Z2+Q2→Tremor
        return TraumaLine.None;
    }
    // Estômago NÃO tem resolver de coluna: linha única latched no engine (D8).
}
```

```csharp
// modded/Patches/Trauma/TraumaLocale.cs
using System;

namespace TRLImmersiveCombatMedicine.Trauma;

/// <summary>Chaves de texto do motor. No 002 entram as linhas de estado (toast de 1ª ocorrência) — textos calibráveis nos consumidores.</summary>
internal enum TraumaTextId { LegsLimpN1, LegsLimpN2, LegsCrouch, LegsFall, ArmsTremor, ArmsAdsCancel, StomachZeroed }

/// <summary>i18n EN default + PT (decisão 22). Tabela PRÓPRIA do plugin — NUNCA injeção de chave no locale do servidor
/// ("key".Localized() devolve chave crua no headless — P8 Recomendação (4)).</summary>
internal static class TraumaLocale
{
    /// <summary>Idioma do jogo é PT? Ler no MOMENTO DE EXIBIR (leitura viva; cobre troca mid-session). NUNCA cachear no Awake.</summary>
    internal static bool IsGamePortuguese()
    {
        // ref: trauma-primitives.md P8 Recomendação (1) — usar o CAMPO estático (null-check sem efeito colateral;
        // o GETTER estático constrói o singleton com "en" default + Resources.LoadAll se chamado cedo).
        // ref: LocaleManagerClass_1 — scratchpad/spike001/LocaleManagerClass.cs:136; String_0 — :197-209; id "po" — SPT database languages.json (P8 evid. :432)
        LocaleManagerClass lm = LocaleManagerClass.LocaleManagerClass_1;
        return lm != null && string.Equals(lm.String_0, "po", StringComparison.OrdinalIgnoreCase);
        // Race de boot: lm == null → false → fallback EN (corner da funcional). Headless: Fika força "en" → false (comportamento desejado — P8).
    }

    internal static string Get(TraumaTextId id)
    {
        // dois arrays estáticos EN/PT indexados por enum; IsGamePortuguese() ? PT : EN; chave sem texto → EN.
        return string.Empty;
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaObservability.cs
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma;

/// <summary>Observabilidade (D19 + comportamento 8): transições SEMPRE logadas; infra de rolls OFERECIDA aos
/// consumidores (nenhum roll existe no 002); toast de 1ª ocorrência gateado por consumidor ativo (decisão 20).</summary>
internal static class TraumaObservability
{
    internal static void LogTransition(in TraumaTransition t)
    {
        // Formato estável p/ os ACs (grep-áveis):
        // "[Trauma2] <profileId>/<nick> <Region>: <From> -> <To> reason=<Reason> mask=<ReasonMask> pk=<bool> establishing=<bool>"
        // reason = primário (maior precedência); mask = bitmask completa da consolidação (rajadas/coincidências auditáveis).
    }

    internal static void LogOneShot(Player p, TraumaOneShotKind kind, bool suppressedByCooldown)
    {
        // "[Trauma2] one-shot <kind> <profileId>" | "[Trauma2] one-shot SUPPRESSED (cooldown) <kind> <profileId>" (AC6)
    }

    /// <summary>Consumida pelos itens 003/006/007 (D19: dano/vida/p/resultado). No 002 só existe — sem call site interno.</summary>
    public static void LogRoll(Player p, TraumaRegion region, string condition, float probability, bool result) { }

    internal static void MaybeToastFirstOccurrence(in TraumaTransition t)
    {
        // Gates: (1) t.Establishing == false; (2) t.Player.IsYourPlayer (toast é feedback local; bots/headless = no-op);
        // (3) 1ª ocorrência da LINHA nesta raid (HashSet<TraumaLine> _seen, zerado no ResetForNewRaid) —
        //     _seen só marca quando o toast EXIBE de fato: supressão por no-consumer NÃO consome a 1ª ocorrência
        //     (ligar o consumidor depois ainda mostra o toast na próxima transição da linha);
        // (4) TraumaConsumerRegistry.AnyActiveFor(t.Region) — SEM consumidor: log "[Trauma2] toast SUPPRESSED (no consumer) <line>" (AC5).
        // Exibição: NotificationManagerClass.DisplayMessageNotification(TraumaLocale.Get(...))
        //   ref: static void DisplayMessageNotification(string message, ENotificationDurationType duration = Default,
        //        ENotificationIconType iconType = Default, Color? textColor = null) — confirmada por ilspycmd na review 1
        //        (scratchpad/spike001/NotificationManagerClass.review1.cs:523); uso em produção: fika FikaBot.cs:306;
        //        call-site no dump: GamePlayerOwner.cs:899.
    }
}
```

## 6. Fluxo de dados

```
[dano/cura/fratura/analgésico no DONO]                       [caminhos SEM evento: FullRestore, revive Fika, transit heal]
        │                                                                     │
        ▼                                                                     ▼
[eventos IHealthController do AHC]                            [polling ≤4 Hz re-deriva IsBodyPartDestroyed/Broken]
  BodyPartDestroyed/Restored, EffectStarted/Residual/Removed                  │
        │  (handlers só marcam dirty — sem lógica)                            │
        ▼                                                                     │
[TraumaEngine.Update — consolidação 1×/frame] ◄───────────────────────────────┘
        │  contagens por região + IsUnderPainkiller (P3)
        ▼
[TraumaMatrixResolver — linha mais severa por região (D1/decisão 3); estômago latched (D8)]
        │  diff vs PlayerTraumaRecord.Lines
        ▼
[publicação em ordem determinística: StateChanged / OneShotPublished (cooldown decisão 19; establishing suprime)]
        │
        ├──► TraumaObservability.LogTransition (sempre) + MaybeToastFirstOccurrence (gate consumidor, i18n P8)
        └──► consumidores futuros 003–007 (SubscribeWithSnapshot / GetLine / IsUnderPainkiller)
```

Passo a passo (exemplo AC2 — zerar e curar perna): tiro zera `LeftLeg` no dono → `DestroyBodyPart` invoca `BodyPartDestroyedEvent` (AHC:3867-3877) → handler marca dirty(Legs, Damage) → `Update` consolida **≤1 frame depois (mesmo frame ou o seguinte)**: `zeroedLegs=1, broken=0, pk=false` → `ResolveLegs` = `LegsLimpN1` → `StateChanged(None→LegsLimpN1, Damage)` + log. Médico REMOTO cura: handler de pacote do mod chama `method_15<T>`/`RestoreBodyPart` nativos ([BandAidNetworkHandler.cs:502-509,544](../../modded/Patches/Medical/BandAidNetworkHandler.cs)) → disparam os MESMOS eventos no dono (D17 confirmado — P10 correção, [trauma-primitives.md:504](../../docs/trauma-primitives.md)) → `StateChanged(LegsLimpN1→None, BodyPartRestored)` ≤1 s (≤1 frame após o evento).

## 7. Riscos e dependências

- **Patches existentes em `modded/Patches/`:** sem conflito por design — o motor é read-only sobre o estado do EFT. O sistema legado (HealthPatches/MovementPatches/InputPatches) continua APLICANDO efeitos (prone forçado, CanStandAt) até os itens 003+; o motor apenas observa. Único cruzamento: a remoção da injeção legacy (`DoFracture`/`ApplyDamage`, MovementPatches.cs:151-170) — verificado por grafo+grep que `LegPenaltyTimers`/`ImpactTimers` (mantidos) têm consumidores em InputPatches.cs:62-95 e MovementPatches.cs:179-184 **e ESCRITOR mantido em [HealthPatches.cs:114-121](../../modded/Patches/Trauma/HealthPatches.cs)** (seed de `ImpactTimers`/`LegPenaltyTimers` no hit — o code-mod NÃO pode limpar essa escrita achando que é resto da injeção aposentada).
- **Compatibilidade com outros mods:** nenhum mod da load order assina/patcheia os eventos do AHC de forma conflitante (eventos multicast — coexistem); o handler do motor deve ser autossuficiente e nunca depender de ordem relativa a outros handlers (P3 risco 4). SAIN/ORBIT fora do domínio (P3 evid. :175).
- **Ordem de inicialização:** componente criado no `Awake`, mas inerte até `OnRaidStarted()` (prefix de `GameWorld.OnGameStarted`). Locale NUNCA lido no Awake (P8). Configs lidas por `.Value` a cada uso (sem cache).
- **Bots com PainKiller PERMANENTE (work=+∞):** bosses E qualquer bot com `Boss.EFFECT_PAINKILLER=true` na dificuldade (P3 correção b2, [trauma-primitives.md:155](../../docs/trauma-primitives.md)) ficam estáveis na coluna "Com analgésico" — o motor os trata como estado estável, sem timer de expiração pendente (**ratificado na review 1**).
- **Rodada 2 do doc de primitivas** (seção "ajustes de coerência"): esta spec já usa os textos corrigidos de P3 (EffectResidualEvent; Berserk configurável; EFFECT_PAINKILLER) e P10 (D17/D19).

### Aberturas explícitas para os reviewers

1. **Toast — RESOLVIDA (review 1):** assinatura confirmada por ilspycmd no assembly real: `static void NotificationManagerClass.DisplayMessageNotification(string message, ENotificationDurationType duration = Default, ENotificationIconType iconType = Default, Color? textColor = null)` (artefato `scratchpad/spike001/NotificationManagerClass.review1.cs:523`; uso em produção: fika `FikaBot.cs:306`; call-site no dump: GamePlayerOwner.cs:899). Toast é o plano A — sem fallback log-only.
2. **Contrato fino do one-shot `InvoluntaryFall` com o item 004:** o motor publica o one-shot na (re-)entrada de `LegsFallCycle` e a linha contínua; a re-derrubada do ciclo (X s do bot — decisão 16) é interna ao 004 e ISENTA de cooldown. Validar na spec do 004 que `OneShot + linha contínua + ReportOneShotExecuted` (adiamento D7) bastam.
3. **Estômago via reconciliação:** se a zerada do estômago for detectada pelo POLLING (evento perdido — janela ≤1/PollingHz), `StomachPainkillerAtEntry` usa o predicado do instante da DETECÇÃO (aproximação do D8). Aceitável? (janela ≤500 ms no default).
4. **Filtro de analgésico restrito a `GInterface358`/`GInterface350`:** stims compostos hipotéticos vivendo só como `Stimulator` (GInterface377) escapariam (P10 risco 2) — sem caso na load order atual (P3 evid.: analgesia de stim vem de `effects_damage.Pain` do template). Aceito como limitação documentada.
5. **Dupla master (`Ativar Mod` legado + `Enable Trauma 2.0`) — FECHADA (review 1):** semântica ratificada — motor exige os DOIS on; o corner do master da spec funcional exige que "Ativar Mod" desligue tudo.
6. **`OnPersonAdd` × sweep inicial — FECHADA (review 1):** ordem ratificada = **subscribe de `OnPersonAdd` ANTES do sweep** de `RegisteredPlayers`, com `TrackPlayer` idempotente absorvendo a sobreposição; smoke test coop logando os 5 eventos segue como primeiro passo do code-mod (risco P10 3).
7. **Player-câmera do headless — FECHADA (review 1):** ratificado como inócuo (nunca toma dano; nenhuma transição ocorre); sem gate extra.

## 8. Checklist de implementação

- [ ] `TraumaEngineState.cs`: enums + structs + `PlayerTraumaRecord` + `TraumaConsumerRegistry`.
- [ ] `TraumaMatrixResolver.cs`: tabelas de pernas/braços (D1/decisão 3/D4) como função pura + testes de mesa nos comentários (combos mistos).
- [ ] `TraumaEngine.cs`: rastreamento (OnPersonAdd/sweep/OnPlayerDeadOrUnspawn), subscriptions com unsubscribe simétrico, dirty→consolidação, latch D8, one-shots+cooldown, establishing, master-off, polling, `ReportOneShotExecuted`.
- [ ] `TraumaLocale.cs` (P8) + `TraumaObservability.cs` (formatos de log estáveis/grep-áveis).
- [ ] Plugin: configs §3 + `AddComponent<TraumaEngine>()` + `OnRaidStartCleanup` → `TraumaEngine.OnRaidStarted()`.
- [ ] `MovementPatches.cs`: remover :151-170 (injeção legacy — decisão 21), preservando :172-179.
- [ ] Toast com a assinatura confirmada na review 1: `NotificationManagerClass.DisplayMessageNotification(string, ENotificationDurationType = Default, ENotificationIconType = Default, Color? = null)` (`scratchpad/spike001/NotificationManagerClass.review1.cs:523`; uso: fika `FikaBot.cs:306`).
- [ ] `PROPRIEDADES.md`: seções 5/6 novas.
- [ ] Regenerar grafo do mod (`/update-mod-graph TRL-ImmersiveCombatMedicine`) no commit da entrega.

**Validação por log (o 002 não tem consumidor — mapeia os ACs da funcional):**

- [ ] AC1: raid inteira com motor on e consumidores off → diff de gameplay zero; log SEM one-shot executado; levantar com 2 pernas zeradas NÃO fratura/dá dano (grep ausência de `DoFracture` legacy).
- [ ] AC2: `[Trauma2] ... Legs: None -> LegsLimpN1 reason=Damage` no zerar; `-> None reason=BodyPartRestored` ≤1 s após cura própria, cura REMOTA (D17) e cirurgia.
- [ ] AC3: 2 pernas quebradas + analgésico → `LegsFallCycle -> LegsLimpN1 reason=PainkillerGained` no mesmo segundo; expiração → `-> LegsFallCycle reason=PainkillerLost` no mesmo segundo; ZERO linhas de transição de Stomach nas duas mudanças e ZERO log de roll (D8).
- [ ] AC4: spawn ferido/transit → linhas `establishing=true`, sem one-shot, sem toast.
- [ ] AC5: `Debug Test Consumer` on → toast 1×/estado/raid em EN (jogo EN) e PT (jogo "po"); off → `toast SUPPRESSED (no consumer)`.
- [ ] AC6: duas entradas do mesmo one-shot em <3 s → 1 `one-shot` + 1 `one-shot SUPPRESSED (cooldown)`; `SubscribeWithSnapshot` tardio recebe replay `establishing=true` (log do stub).
- [ ] AC7 (Fika 2+ processos): log de cada processo contém APENAS seus donos (host: bots + seu player; client: só o próprio); espelhos ausentes do log.
- [ ] AC8: raid1→exit/morte/alt-F4→raid2 → `ResetForNewRaid` no log e nenhum estado/cooldown/toast-visto herdado.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | §2 (prefix OnGameStarted existente) + §5 `Update` (b) null-detect de GameWorld (padrão N1 do repo) + `ResetForNewRaid` idempotente; alt-F4 mata o processo (estado estático não persiste); transit re-dispara OnGameStarted. |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a ação de player — AP-02 | ✅ | `IsOwnedHere` (type-check ActiveHealthController — D16) em `OnPersonAdd`/`TrackPlayer` (§5); espelhos nunca entram no rastreamento. |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | Nenhum patch Harmony novo (§2). Interfaces ofuscadas (GInterface342/350/358) citadas com evidência do spike e usadas por type-check (compile-time, sem reflection). O override no-op de eventos em espelhos é justamente o que o D16 explora (P10 evid. :525-527). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | O motor NÃO muda estado do EFT (read-only). A única escrita REMOVIDA é a injeção legacy (`DoFracture`/`ApplyDamage`) — decisão 21, §4. |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | AC8 + §5 `ResetForNewRaid` (records, cooldowns, toasts-vistos) + validação por log (§8). |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: defaults/faixas/tooltips + semântica dos toggles (motor publica sempre; consumidores gateiam efeito; estado neutro = rastrear+logar). |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | N/A | Motor não adiciona patch e não re-invoca método patcheado; handlers de evento só marcam dirty (nenhuma chamada que re-dispare os próprios eventos no mesmo stack). |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | Linhas cacheadas em `PlayerTraumaRecord` são re-derivadas do estado-verdade (`IsBodyPartDestroyed/Broken` + predicado) na consolidação E no polling de reconciliação (§5 `Reconcile`). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Spec técnica criada via `/create-technical-spec` (baseada em trauma-primitives.md P1/P3/P8/P10 + Rodada 2) |
| 2026-07-18 | Review técnica rodada 1 aplicada — 6 achados (2 médios: registry multi-região p/ AC5, bitmask de motivos com precedência; 4 menores: ≤1 frame, semântica do _seen, assinatura do toast confirmada, escritor legacy HealthPatches.cs:114-121) + 4 aberturas ratificadas/fechadas |
