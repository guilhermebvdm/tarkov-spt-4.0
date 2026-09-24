# 001 — Otimização do Limitador de IA por Distância (AI Limiter + LOD) · Code Review 01

**Mod:** SAIN
**Spec funcional:** [001-perf-limitador-ia-lod-01-spec.md](001-perf-limitador-ia-lod-01-spec.md)
**Spec técnica:** [001-perf-limitador-ia-lod-02-spec-tech.md](001-perf-limitador-ia-lod-02-spec-tech.md)
**Asbuild:** [001-perf-limitador-ia-lod-05-asbuild.md](001-perf-limitador-ia-lod-05-asbuild.md)
**Data:** 2026-09-20

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** topo de `mods/SAIN/memory/sessions.md`, Sessões 1 e 2. Pendências que afetam este item: **[P-2.1]** 🟡 e **[P-2.2]** 🟢 (ambas sobre `DirectionDataJob`/pipeline de distância via Job) — ver `CR-01-02` abaixo, que documenta que este item aumentou a dependência nesse pipeline.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | `ClosestPlayerDistanceSqr` tem default `0f` (não `-1f`) — janela de até 0.5s no spawn do bot com `DistanceToClosestHuman` incorreto (0m) | ✅ Aplicado em 2026-09-20 |
| CR-01-02 | D — Arquitetura | 🟢 Menor | LOD passou a depender do pipeline `DirectionDataJob`, antes independente — aumenta exposição às pendências `[P-2.1]`/`[P-2.2]` | Pendente |

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

### CR-01-01 · B — Bug latente · 🟡 Médio · ✅ Aplicado em 2026-09-20

**`ClosestPlayerDistanceSqr` tem default `0f`, não `-1f` — janela de até 0.5s no spawn do bot com distância incorreta**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:236-242`](../../modded-multithread/SAIN/Components/BotComponent.cs#L236)

**Problema:** O guard de fallback verifica `if (aiLimitDist >= 0f)`:
```csharp
float aiLimitDist = AILimit.ClosestPlayerDistanceSqr;
if (aiLimitDist >= 0f)
{
    _cachedDistToHuman = aiLimitDist;
    DistanceToClosestHuman = aiLimitDist;
    return aiLimitDist;
}
```
`SAINAILimit.ClosestPlayerDistanceSqr` (`SAINAILimit.cs:12`) é um auto-property `{ get; private set; }` **sem inicializador** — seu valor default em C# é `0f`, não `-1f`. O único lugar que seta `-1f` explicitamente é o ramo de `Bot.EnemyController.ActiveHumanEnemy` (`SAINAILimit.cs:32`). Isso significa que, **antes da primeira execução de `SAINAILimit.CheckAILimit()`** (que só acontece dentro de `TickClassGroup(_tickWhenActiveClasses, currentTime)`, chamado em `BotComponent.cs:333` — **depois** de `GetMinDistanceToHumanPlayer()` já ter rodado em `BotComponent.cs:283, na mesma chamada de `ManualUpdate()`), `ClosestPlayerDistanceSqr` ainda está no seu default `0f`. `0f >= 0f` é verdadeiro, então o código trata esse `0f` como "distância real de 0 metros até o jogador mais próximo" em vez de cair no fallback.

**Por que importa:** Todo bot, ao ser instanciado, tem sua primeira chamada de `ManualUpdate()` lendo um `AILimit.ClosestPlayerDistanceSqr` que nunca foi calculado de verdade. Como `_nextDistCheckTime`/cache é de 0.5s, esse `0f` incorreto fica cacheado em `DistanceToClosestHuman` (propriedade **pública**) por até 0.5 segundos antes da próxima leitura poder corrigir. O efeito no LOD em si é inofensivo (`isCloseToHuman` fica `true`, força Tier 0 — direção seguraem excesso de responsividade, não de menos). Mas `DistanceToClosestHuman` é uma propriedade pública que outros mods podem ler (é exatamente o tipo de contrato que o item promete preservar) — expor `0` como se fosse a distância real, mesmo que brevemente, é um dado incorreto entregue pela API pública do SAIN.

**Sugestão:** Trocar o guard de `aiLimitDist >= 0f` para `aiLimitDist > 0f` em `BotComponent.cs:237`. Distância real de exatamente `0.0f` metros (bot e jogador ocupando a mesma coordenada) é fisicamente inatingível na prática, então usar `>` em vez de `>=` exclui apenas o valor-default não inicializado, sem custo para o caso real. Não requer tocar em `SAINAILimit.cs` (mantém `PA-01-01`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto. Discutido com o usuário se seria melhor corrigir na raiz (`SAINAILimit.ClosestPlayerDistanceSqr` default `-1f`) — decidido manter o escopo mínimo e não reabrir `SAINAILimit.cs`, deixando a inconsistência do default `0f` registrada como dívida técnica separada (candidata a um item de backlog futuro dedicado a `SAINAILimit.cs`, não a este item).

**Aplicação:** `mods/SAIN/modded-multithread/SAIN/Components/BotComponent.cs:239` — `>= 0f` trocado por `> 0f`, com comentário explicando o motivo. Recompilado com 0 erros.

---

### CR-01-02 · D — Arquitetura · 🟢 Menor

**LOD passou a depender do pipeline `DirectionDataJob`, antes independente — aumenta exposição às pendências `[P-2.1]`/`[P-2.2]`**

**Local:** [`BotComponent.cs:236`](../../modded-multithread/SAIN/Components/BotComponent.cs#L236) (consumo de `AILimit.ClosestPlayerDistanceSqr`, cuja cadeia de origem passa por `PlayerDistanceData` → `DirectionDataJob`).

**Problema:** Antes deste item, `BotComponent.GetMinDistanceToHumanPlayer()` calculava a distância de forma 100% síncrona e independente (`PlayerSpawnTracker.FindClosestHumanPlayer` via `Vector3`), sem tocar no pipeline do `DirectionDataJob`. Depois deste item, a decisão de tier do LOD (que controla a taxa de atualização de toda a árvore de decisão do bot) passa a depender, na maior parte do tempo, de um valor que se origina desse Job — o mesmo componente com um crash já corrigido mas **não validado in-game** (`[P-2.1]`) e um débito arquitetural aberto sobre compartilhamento de referência viva (`[P-2.2]`).

**Por que importa:** Isso não é um bug introduzido por este item — o fallback síncrono (`BotComponent.cs:244-256`) continua existindo e cobre o caso de ausência total de valor. Mas se o bug de `[P-2.1]` se manifestar de alguma forma sutil não coberta pelo clamp defensivo já aplicado (ex.: valor de distância desatualizado em vez de exceção), esse problema agora se propagaria também pro LOD (afetando taxa de atualização de todos os bots), não só pro `EnemyPlaceRaycastJob`/`VisionRaycastJob` que já eram consumidores conhecidos. É um acoplamento novo que a spec técnica já reconhece explicitamente (não é uma surpresa), mas vale registrar formalmente como motivo para priorizar a validação in-game de `[P-2.1]` — que já estava pendente, mas agora tem mais uma razão para não ficar esquecida.

**Sugestão:** Não é uma mudança de código — é uma recomendação de sequenciamento: validar `[P-2.1]` in-game (reproduzir morte/despawn de bot durante a janela de 1 frame do job, como já descrito na pendência) **junto** com a validação in-game deste item (`001-perf-limitador-ia-lod`), já que agora compartilham superfície de risco. Registrar isso na atualização de memória (`/update-memory`) ao fechar este item.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-20 | Code review 01 criada via `/code-review` |
