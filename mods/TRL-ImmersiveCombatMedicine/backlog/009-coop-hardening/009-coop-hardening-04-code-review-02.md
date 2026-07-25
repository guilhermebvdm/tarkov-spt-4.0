# 009 — Coop/bots: hardening do Trauma 2.0 · Code Review 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [009-coop-hardening-01-spec.md](009-coop-hardening-01-spec.md)
**Spec técnica:** [009-coop-hardening-02-spec-tech.md](009-coop-hardening-02-spec-tech.md)
**Asbuild:** [009-coop-hardening-05-asbuild.md](009-coop-hardening-05-asbuild.md)
**Data:** 2026-07-25

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-02-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

**Memória consultada:** topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4, P-4.1 — débito do boilerplate `Update()` aberto em 006 code-review-01 CR-01-02 e 008 code-review-01 CR-01-01; P-4.5 — 009/010 pendentes de início) + entradas que citam o item 009. Nenhuma pendência 🔴 do mod bloqueia esta rodada. P-4.1 é exatamente o que A4 fecha; nenhuma pendência nova relevante foi aberta desde a rodada 01.

**CR-01-01 (rodada 01) já está `✅ Aplicado`** — campo `_onToggleOn` do Stomach removido, `null` literal no call site, recompilado (0 erros, `CS0649` eliminado). Não revisitado nesta rodada.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | D — Arquitetura | 🟢 Menor | `IsActive` fica com 2 delegates independentes por consumidor (um no `TraumaConsumerRegistry`, outro em `_isActiveDelegate`) | Pendente |

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

## Verificação desta rodada (segunda passada — ângulos além da regressão linha-a-linha da rodada 01)

A rodada 01 já cobriu, com prova em `git diff`, a equivalência linha-a-linha dos 4 `Update()`/`Awake()` (velho vs. novo), a remoção dos campos `_wasActive`/`_trackedWorld`, o `readonly` ausente em `_lifecycle`, o bookkeeping fora dos callbacks do Arms, a fidelidade do `_onToggleOn` nulo do Stomach, o fix de `Random` e o comentário isolado em `TraumaVoice.cs`. Esta rodada **não repetiu** essa verificação célula-a-célula — em vez disso, refez a comparação **de forma independente** (a partir do `git diff HEAD` real, não da narrativa da rodada 01) e buscou ângulos novos.

1. **Regressão re-confirmada de forma independente via `git diff HEAD`** (não a partir da leitura da rodada 01): como o working tree ainda está **não commitado** desde o `/code-mod` deste item, `git diff HEAD -- <arquivo>` expõe o "antes" real (código pré-009, ainda em `HEAD`) contra o "depois" atual — uma prova mais direta do que reconstruir o original a partir de comentários `// ref:`. Os 4 diffs completos (`TraumaLegsConsumer.cs`, `TraumaFallCycleConsumer.cs`, `TraumaArmsConsumer.cs`, `TraumaStomachConsumer.cs`) foram lidos por inteiro: em nenhum dos 4, a extração move, reordena ou altera uma única chamada de negócio — só desloca o texto do corpo do `if`/`else if` do `Update()` antigo para um método nomeado, byte-a-byte. O diff de `TraumaVoice.cs` (`git diff HEAD`) mostra exclusivamente as 6 linhas do bloco de comentário `DECISÃO A3` — nenhum caractere de código. Confirma, de forma independente e por uma via de evidência diferente da rodada 01, que não há regressão em nenhum dos 5 arquivos.
2. **`graphify affected`/`explain`** (skill `graph-code-navigation`) rodado contra `references/graphs/mods/TRL-ImmersiveCombatMedicine/graph.json` para `TraumaConsumerLifecycle`, `Tick`, `TraumaLegsConsumer` e `TraumaVoice`: o grafo deste mod tem extração rasa para este recorte (nós com `Degree: 1`, sem arestas `calls` capturadas entre os 4 consumidores e o helper novo, `affected` retornando vazio para símbolos que sabidamente têm 4 call sites reais). Não é evidência de ausência de callers — é o grafo **não tendo granularidade** para esta pergunta (regra do skill: "grafo aponta, leitura prova"). A prova real usada nesta rodada foi a leitura completa dos 5 arquivos + os diffs (item 1 acima), não o grafo.
3. **Dependência externa em reflection sobre os campos antigos:** grep por `AccessTools.Field.*Trauma(Legs|FallCycle|Arms|Stomach)Consumer` e por `_wasActive|_trackedWorld` em todo `mods/TRL-ImmersiveCombatMedicine/` (não só na pasta `Trauma/`) — zero ocorrência de reflection externa sobre os campos removidos; as únicas menções restantes de `_wasActive`/`_trackedWorld` são texto de documentação (specs/reviews/asbuild). Nenhum patch de terceiro (dentro do mod) dependia dos nomes de campo antigos.
4. **Instanciação dos 4 consumidores** (`TRLImmersiveCombatMedicinePlugin.cs:264-267`, `gameObject.AddComponent<Trauma*Consumer>()`): confirmado que cada um é adicionado **uma única vez**, no `GameObject` persistente do próprio plugin (não por player, não por raid) — o padrão singleton local (`_instance` estático) que a spec assume permanece válido; A4 não introduz um caminho de re-instanciação que pudesse duplicar o helper por consumidor.
5. **Interação com o motor (`TraumaConsumerRegistry`/`TraumaEngine`):** `TraumaConsumerRegistry.Register(id, regions, isActive)` (`TraumaEngineState.cs:132-134`) já armazena o MESMO `Func<bool> IsActive` passado por cada consumidor num `Entry` interno, consultado depois por `AnyActiveFor` (`TraumaEngineState.cs:137-148`, chamada `e.IsActive()` na linha 145) para gatear o toast de 1ª ocorrência (decisão 20). A4 adiciona um **segundo** cache do mesmo delegate (`_isActiveDelegate = IsActive;`) só para o `Tick()`. Nenhum dos dois pontos de registro foi tocado pela rodada 01 sob esse ângulo — ver CR-02-01 abaixo.
6. **Consistência de nomenclatura:** `TraumaConsumerLifecycle`/`Tick()` seguem o padrão `Trauma<Substantivo>` já usado por `TraumaPose`, `TraumaEngine`, `TraumaSpeedCap`, `TraumaTremor`, `TraumaVoice`, `TraumaBotFall`, `TraumaObservability` — sem colisão de estilo. Nomes dos 4 callbacks (`OnWorldGone/OnWorldSwap/OnToggleOff/OnToggleOn`) re-confirmados (independentemente da rodada 01 e da review técnica 02) como não-colidentes com nenhuma mensagem mágica do Unity nem com métodos existentes em qualquer outro arquivo do mod.
7. **Hideout/coop:** nenhum dos 4 consumidores ganhou ou perdeu um guard de contexto (`HideoutPlayer`, `IsYourPlayer`, `IsOwnedHere`) — todos os guards de domínio (fora do escopo do `Update()`/`Awake()` tocado por A4) permanecem exatamente onde estavam, em `OnTransitionCore`/`ApplyLine`/`OnOneShotCore`. A4 não introduziu nem removeu superfície de guard — consistente com o mandato "zero mudança de comportamento".
8. **`TraumaEngine.cs` (o motor) não foi migrado para o mesmo helper** — decisão já investigada e fechada na review técnica 01 ("5º candidato", ponto 48) com a razão de que o motor tem um esqueleto mais rico (guard de hideout, flag `_raidStarted`). Não reaberta aqui por falta de evidência nova (a spec funcional veda reabrir decisões fechadas sem razão concreta nova).

Nenhum dos pontos 1-4 e 6-8 vira achado — todos **confirmam** a implementação. O ponto 5 é o único ângulo genuinamente novo desta rodada, formalizado abaixo.

---

## Pontos

### CR-02-01 · D — Arquitetura · 🟢 Menor

**`IsActive` fica com 2 delegates independentes por consumidor — um dentro de `TraumaConsumerRegistry`, outro em `_isActiveDelegate`**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaEngineState.cs:122,132-134`](../../modded/Patches/Trauma/TraumaEngineState.cs#L122) (armazenamento no registry) e, por exemplo, [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaLegsConsumer.cs:45-56`](../../modded/Patches/Trauma/TraumaLegsConsumer.cs#L45) (cache duplicado no consumidor — mesmo padrão nos outros 3)

**Problema:** cada `Awake()` faz, em sequência:

```csharp
TraumaConsumerRegistry.Register(TraumaConsumerId.LegsEffects, LegsRegions, IsActive); // cria delegate #1, guardado em Entry.IsActive
...
_isActiveDelegate = IsActive; // cria delegate #2 (A4), guardado no consumidor
```

`TraumaConsumerRegistry.Register` (`TraumaEngineState.cs:132-134`) já guarda o `Func<bool> isActive` recebido dentro de um `Entry` privado, consultado por `AnyActiveFor` (`:137-148`) para gatear o toast de 1ª ocorrência (decisão 20). A4 cria um **segundo** delegate apontando para o mesmo método estático `IsActive()`, cacheado no campo novo `_isActiveDelegate`, para uso exclusivo do `TraumaConsumerLifecycle.Tick()`. Isso se repete nos 4 consumidores — 4 pares de delegates redundantes.

**Por que importa:** é puramente cosmético — ambos os delegates são criados **uma única vez** em `Awake()` (não por frame), então não há custo de alocação repetida nem risco de dessincronia (os dois sempre apontam para o MESMO método estático `IsActive()` de cada classe, que lê `ConfigEntry.Value` ao vivo — nenhum dos dois pode "atrasar" em relação ao outro). Não é um bug, não afeta comportamento, e a rodada 01 não tinha motivo para olhar essa interseção porque focou no `Update()`/campos antigos, não no `Awake()`/registry. Vale registrar para consciência arquitetural: um consumidor futuro que copiar o padrão A4 sem perceber que o registry já guarda o mesmo predicado pode achar que precisa "descobrir" como obter o `Func<bool>` de volta do registry — não precisa, e não vale a pena tentar (ver sugestão).

**Sugestão:** recomendação do revisor é **não mudar nada** — expor um getter em `TraumaConsumerRegistry` para devolver o delegate já armazenado (ex.: `TryGetIsActive(TraumaConsumerId id, out Func<bool> isActive)`) eliminaria a duplicação, mas trocaria uma alocação desprezível de 4 objetos (criados 1x na vida do plugin) por acoplamento novo entre o registry (motor) e o helper de lifecycle (A4) — pior trade-off do que o problema que resolveria. Deixado como decisão explícita do usuário abaixo em vez de pré-resolvido pelo revisor.

**Decisão:**
- `[x]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-25 | Code review 02 criada via `/code-review` — segunda passada independente (não repetiu a verificação linha-a-linha da rodada 01; refez a comparação a partir de `git diff HEAD` real e buscou ângulos novos: `graphify affected`/`explain` no grafo do mod, dependência de reflection sobre campos removidos, instanciação dos 4 consumidores, interação com `TraumaConsumerRegistry`/`TraumaEngine`, consistência de nomenclatura, guards de hideout/coop). 0 bloqueadores; 1 achado 🟢 opcional (CR-02-01, duplicação cosmética de delegate `IsActive` entre o registry e o helper A4, recomendação do revisor é não mudar nada) — pendente de decisão do usuário. Item 009 permanece pronto para fechar (nenhum bloqueador em nenhuma das 2 rodadas). |
