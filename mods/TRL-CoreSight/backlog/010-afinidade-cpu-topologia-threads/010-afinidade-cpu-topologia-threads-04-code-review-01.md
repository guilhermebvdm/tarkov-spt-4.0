# 010 — afinidade-cpu-topologia-threads · Code Review 01

**Mod:** TRL-CoreSight
**Spec funcional:** [010-afinidade-cpu-topologia-threads-01-spec.md](010-afinidade-cpu-topologia-threads-01-spec.md)
**Spec técnica:** [010-afinidade-cpu-topologia-threads-02-spec-tech.md](010-afinidade-cpu-topologia-threads-02-spec-tech.md)
**Asbuild:** [010-afinidade-cpu-topologia-threads-05-asbuild.md](010-afinidade-cpu-topologia-threads-05-asbuild.md)
**Data:** 2026-09-19

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Contexto desta rodada

Antes desta review formal, uma rodada informal (`/gemini-handoff-review coresight-cpu-affinity-fix`) já havia identificado e corrigido, via `010-afinidade-cpu-topologia-threads-03-spec-tech-review-02.md` (PA-02-01 a PA-02-04), o problema de fundo da implementação original: `SetProcessAffinityMask` restringia o processo inteiro no modo `PrimaryCluster`, sufocando as worker threads do Job System/SAIN. Essa correção trocou para afinidade de thread única (`SetThreadAffinityMask` na Main Thread).

Durante a verificação dessa correção (fora do fluxo automatizado, aplicado manualmente antes desta review), foram encontrados e já resolvidos:
- ✅ **Condição de corrida entre `_needsMainThreadPin`/`_needsMainThreadUnpin`** (`CpuTopologyManager.cs`): trocar de `PrimaryCluster` → outro modo → `PrimaryCluster` novamente antes do próximo `Update()` deixava uma flag de "liberar" pendente que desfazia a fixação um frame depois de ela ter sido aplicada. Corrigido limpando `_needsMainThreadUnpin` ao reentrar no modo `PrimaryCluster`.
- ✅ **Ausência de liberação da Main Thread ao trocar de modo em pleno raid**: `SetProcessAffinityMask` não afeta a afinidade de uma thread já fixada por `SetThreadAffinityMask`; sair do `PrimaryCluster` sem liberar a thread a deixava presa na máscara estreita até o fim da raid. Corrigido com o par `_needsMainThreadUnpin`/`UnpinCallingThread()`.
- ✅ **Código morto**: o `case ECpuAffinityMode.PrimaryCluster` em `CalculateTargetMask()` nunca mais é alcançado (interceptado antes por `ApplyAffinity()`). Removido.

Essas 3 correções já estão no código atual e recompiladas (0 avisos, 0 erros) — não são reabertas como achados desta rodada. Os achados abaixo são os que restaram após essa verificação.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | Benchmark de frametime trunca silenciosamente após ~60.000 frames (~16-17 min) | ✅ Aplicado em 2026-09-19 |
| CR-01-02 | C — Gap vs. processo | 🟠 Forte | `PROPRIEDADES.md` não reflete o novo padrão de fábrica nem a nova ConfigEntry | ✅ Aplicado em 2026-09-19 |
| CR-01-03 | D — Arquitetura/processo | 🟡 Médio | `mod-backlog.md` mantém o item 010 em ⚪ apesar de já ter passado por build e 2 revisões técnicas | ✅ Aplicado em 2026-09-19 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-19

**Benchmark de frametime trunca silenciosamente após ~60.000 frames (~16-17 min a 60fps)**

**Local:** [`mods/TRL-CoreSight/modded/Core/FrametimeBenchmark.cs:20-52`](../../modded/Core/FrametimeBenchmark.cs#L20)

**Problema:** O buffer é um array fixo (`private readonly float[] _samples = new float[MAX_SAMPLES];` com `MAX_SAMPLES = 60000`). Em `OnUpdate()`, ao atingir a capacidade, novas amostras são simplesmente descartadas:
```csharp
if (_sampleCount < MAX_SAMPLES)
{
    _samples[_sampleCount++] = dt;
}
```
Não há log nem qualquer sinal de que a gravação parou. Raids de SPT frequentemente passam de 20-40 minutos, então o CSV final reflete só os primeiros ~16-17 minutos da partida — justamente a fase de spawn/rota inicial, não o período de combate mais intenso que normalmente é quando a estabilidade de frame mais importa.

**Por que importa:** O propósito inteiro desta feature (PA-02-04) é dar um número confiável pra decidir se a afinidade de CPU ajuda ou atrapalha. Um "1% low" calculado só sobre os primeiros 16 minutos de uma raid de 35 minutos pode facilmente esconder ou exagerar o efeito real da afinidade durante os tiroteios, invalidando a própria comparação que a feature existe pra habilitar — sem nenhum aviso ao usuário de que os dados são parciais.

**Sugestão:** Duas opções, qualquer uma resolve:
1. Transformar `_samples` num buffer circular (`_samples[_sampleCount % MAX_SAMPLES] = dt; _sampleCount++;` mantendo um `_totalFrames` separado para o total real), preservando sempre a janela mais recente em vez das primeiras amostras.
2. Se preferir manter o array simples, ao menos logar um aviso único na primeira vez que a captura for descartada (`if (_sampleCount >= MAX_SAMPLES && !_truncationWarned) { _truncationWarned = true; Plugin.LogSource?.LogWarning(...); }`) e anexar essa informação (`Truncated=true/false`) como coluna no CSV, para quem for comparar os números saber que a amostra é parcial.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Aceita a opção 2 (aviso único + coluna no CSV), por ser menos invasiva que trocar pra buffer circular.

**Aplicação:** `mods/TRL-CoreSight/modded/Core/FrametimeBenchmark.cs` — adicionado `_truncationWarned` (log de aviso único quando o buffer enche, via `LogWarning`) e coluna `Truncated` no CSV (`true`/`false`), incluindo no log final de sucesso um marcador `(TRUNCADO)` quando aplicável.

---

### CR-01-02 · C — Gap vs. processo · 🟠 Forte · ✅ Aplicado em 2026-09-19

**`PROPRIEDADES.md` não reflete o novo padrão de fábrica nem a nova ConfigEntry**

**Local:** [`mods/TRL-CoreSight/PROPRIEDADES.md:12-17`](../../PROPRIEDADES.md#L12)

**Problema:** A tabela da seção `0. Afinidade de CPU & Threads` ainda documenta `CpuAffinityMode` com padrão `Auto` (mudou para `PhysicalCoresOnly` nesta correção) e não lista a `ConfigEntry` nova `EnableFrametimeBenchmark` (`ModConfig.cs:49-54`) nem o aviso de que `PrimaryCluster` é um modo avançado que reduz núcleos totais.

**Por que importa:** `spt-mod-best-practices` §"Configuration" e `repo-workflow-best-practices` §7 exigem que toda `ConfigEntry` nova ou alterada tenha entrada correspondente em `PROPRIEDADES.md` — é a fonte única de verdade do F12 para quem for revisar ou dar suporte ao mod depois. Ficou fora do escopo do handoff original (deliberadamente, para não deixar o Gemini tocar nesse arquivo) e nunca foi fechado depois.

**Sugestão:** Atualizar a linha de `CpuAffinityMode` na tabela para refletir o padrão `PhysicalCoresOnly` e o tooltip atual (`ModConfig.cs:39`), e adicionar uma linha nova para `EnableFrametimeBenchmark` (tipo `bool`, padrão `false`, tooltip de `ModConfig.cs:53`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.

**Aplicação:** `mods/TRL-CoreSight/PROPRIEDADES.md` — linha de `CpuAffinityMode` atualizada (padrão `PhysicalCoresOnly`, tooltip completo incluindo o aviso sobre `PrimaryCluster`) e adicionada a linha `EnableFrametimeBenchmark`.

---

### CR-01-03 · D — Arquitetura/processo · 🟡 Médio · ✅ Aplicado em 2026-09-19

**`mod-backlog.md` mantém o item 010 em ⚪ apesar de já ter passado por build e 2 revisões técnicas**

**Local:** [`mods/TRL-CoreSight/backlog/mod-backlog.md:16`](../mod-backlog.md#L16)

**Problema:** A linha do item 010 ainda mostra status `⚪` (Backlog/não iniciado), mas o item já tem `02-spec-tech.md`, duas rodadas de `03-spec-tech-review-NN.md`, um `05-asbuild.md` de build (`v0.4.16`, 0 erros) e agora esta code review. Pelo fluxo descrito em `repo-workflow-best-practices` §6, `/code-mod` deveria ter avançado o status para 🟢 (ou 🟡 durante revisões prolongadas) — isso não aconteceu.

**Por que importa:** O `mod-backlog.md` é o índice que reflete o estado real de cada item pra quem olhar o mod de fora; um item com trabalho substancial já feito mas marcado como "não iniciado" é uma fonte de confusão para quem for continuar o trabalho ou decidir prioridade de outros itens.

**Sugestão:** Atualizar a coluna de status do item 010 para 🟢 (Entregue) assumindo que os achados 🟠/🟡 desta review não bloqueiam o fechamento (nenhum 🔴 nesta rodada).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto (nenhum 🔴 pendente nesta rodada).

**Aplicação:** `mods/TRL-CoreSight/backlog/mod-backlog.md:16` — status do item 010 alterado de `⚪` para `🔵` (convenção local deste mod para "entregue", usada por todos os itens 001-009).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-19 | Code review 01 criada via `/code-review` |
| 2026-09-19 | Aplicação automática de 3 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02, CR-01-03; rejeitados: nenhum |
