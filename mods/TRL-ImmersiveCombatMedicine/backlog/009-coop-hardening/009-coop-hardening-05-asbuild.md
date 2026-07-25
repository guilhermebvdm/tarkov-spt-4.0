# 009 — Coop/bots: hardening do Trauma 2.0 · As-Built

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [009-coop-hardening-01-spec.md](009-coop-hardening-01-spec.md)
**Spec técnica:** [009-coop-hardening-02-spec-tech.md](009-coop-hardening-02-spec-tech.md)
**Última review técnica:** [009-coop-hardening-03-spec-tech-review-02.md](009-coop-hardening-03-spec-tech-review-02.md)
**Build inicial:** 2026-07-20

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.

> **Escopo deste build: SÓ A3 e A4** (os únicos sub-itens de código da spec técnica). A1/A2 (documentação — `docs/trauma-compat-suite.md`) e o Bloco B (protocolos de teste manual) são trabalho de outra tarefa, fora deste `/code-mod`.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaConsumerLifecycle.cs` | `struct TraumaConsumerLifecycle` com `Tick(...)` — detecção compartilhada de mundo nulo/world-swap/toggle ON↔OFF via 5 callbacks cacheados; campo mutável (não `readonly`, PA-01-03). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaLegsConsumer.cs` | `Update()` reduzido a `_lifecycle.Tick(...)` + lógica per-tick inalterada; `_wasActive`/`_trackedWorld` removidos (PA-01-01); 4 callbacks (`OnWorldGone/OnWorldSwap/OnToggleOff/OnToggleOn`) extraídos verbatim; delegates cacheados em `Awake()`. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaFallCycleConsumer.cs` | Idem (mesmo padrão) — `Update()` reduzido; `TickHumanCycle()`/`PumpDeferred()`/`TraumaBotFall.Pump()` inalterados; campos antigos removidos. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaArmsConsumer.cs` | Idem — `OnWorldGone`/`OnWorldSwap` contêm SÓ `TearDownLocal(...)`+`ResetLockout()` (bookkeeping ficou fora, PA-01-02); poda/watchdog/deadline de timer per-tick inalterados; campos antigos removidos. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaStomachConsumer.cs` | Idem, mais simples — `_onToggleOn` nunca atribuído em `Awake()` (fica `null`, `?.Invoke()` vira no-op, igual ao original sem ação de religar); campos antigos removidos. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaVoice.cs` | Comentário de decisão A3 acima de `PlayStrong` (colisão de voz 004×005 aceita sem arbitragem) — zero mudança de assinatura/lógica. |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRLImmersiveCombatMedicinePlugin.cs` | Bump de versão 1.9.0 → 1.9.1 (`BepInPlugin` + log do `Awake`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/modded/TRL-ImmersiveCombatMedicine.csproj` | Bump de versão 1.9.0 → 1.9.1 (`<Version>`). |
| MODIFICADO | `mods/TRL-ImmersiveCombatMedicine/backlog/mod-backlog.md` | Status do item 009: ⚪ → 🟢 (Bloco A implementado; Bloco B como pendência viva, ver nota abaixo). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | A — Gap · 🔴 | Campos antigos `_wasActive`/`_trackedWorld` removidos dos 4 consumidores (Legs/FallCycle/Arms/Stomach) — bookkeeping vive só em `TraumaConsumerLifecycle`. Confirmado por grep: zero ocorrência restante nos 4 arquivos. |
| PA-01-02 | C — Erro de lógica · 🟡 | Em `TraumaArmsConsumer`, `OnWorldGone`/`OnWorldSwap` contêm SÓ as 2 chamadas (`TearDownLocal`/`ResetLockout`) — o bookkeeping `_trackedWorld = null`/`_wasActive = IsActive()`/`_trackedWorld = gw`/`return` NÃO foi copiado para dentro dos callbacks (fica implícito em `Tick()`). |
| PA-01-03 | A — Gap · 🟡 | Campo `_lifecycle` declarado SEM `readonly` nos 4 consumidores E no próprio `struct` — comentário de aviso replicado em todos os 5 pontos (`TraumaConsumerLifecycle.cs` + 4 consumidores). |
| PA-02-01 | A — Gap · 🟢 | Comentário sobre o replay síncrono de `SubscribeWithSnapshot` (pode invocar `OnTransition` dentro do próprio `Awake()`, antes do cache dos delegates) replicado nos 4 `Awake()` reais, junto da chamada `TraumaEngine.SubscribeWithSnapshot(OnTransition)`. |

## Mudanças posteriores

| Data | Origem | Resumo |
| --- | --- | --- |
| 2026-07-25 | CR-01-01 (code-review 01) | `TraumaStomachConsumer.cs`: campo `_onToggleOn` (sempre `null`, warning `CS0649`) removido; call site de `Tick()` passa `null` literal. Recompilado — 0 erros, warning eliminado. |
| 2026-07-25 | CR-02-01 (code-review 02, aceito com modificação) | 4 consumidores: delegate `_isActiveDelegate = IsActive` agora criado ANTES de `TraumaConsumerRegistry.Register(...)` e a MESMA referência é passada ao `Register` (em vez de `Register(..., IsActive)` criar um 2º delegate independente). Elimina a duplicação apontada sem expor getter novo no registry (rejeitada a sugestão literal do achado, que trocaria alocação desprezível por acoplamento novo motor↔helper). Recompilado — 0 erros. |

## Notas de execução

- **Verificação de regressão (Passo 4 do `/code-mod`, obrigatória):** para cada um dos 4 consumidores, `git diff` foi lido linha a linha após a edição, confirmando que toda condição, chamada e ORDEM relativa de execução do `Update()`/`Awake()` original persiste idêntica no novo código — nenhuma reordenação, nenhuma melhoria não solicitada. Nenhuma divergência encontrada em nenhum dos 4 arquivos.
- **Bloco B (protocolos de teste manual, B1/B2):** entregue como documento em [`docs/trauma-coop-test-protocol.md`](../../docs/trauma-coop-test-protocol.md) (roteiro B1 smoke-test solo + B2 protocolo 2 PCs, 9 cenários) — a EXECUÇÃO real do roteiro segue pendente (validação manual, fora do escopo de código). Critério de "entregue" da spec funcional (Bloco A implementado + Bloco B como roteiro pronto) satisfeito.
- **A1/A2:** `docs/trauma-compat-suite.md` já existe no repo (entregue antes deste `/code-mod`, fora do escopo desta spec técnica que cobre só A3/A4) — não foi tocado por este build.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-20 | Build concluído via `/code-mod` — A3 (comentário de decisão em `TraumaVoice.cs`) e A4 (`TraumaConsumerLifecycle` + migração dos 4 consumidores) implementados. Versão 1.9.0 → 1.9.1. |
