# 001 — Otimização do Limitador de IA por Distância (AI Limiter + LOD) · Review Técnica 01

**Mod:** SAIN
**Spec técnica revisada:** [001-perf-limitador-ia-lod-02-spec-tech.md](001-perf-limitador-ia-lod-02-spec-tech.md)
**Data:** 2026-09-19

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** topo de `mods/SAIN/memory/sessions.md`, Sessões 1 e 2. Pendência que afeta diretamente este item: **[P-2.1]** 🟡 (fix de crash em `DirectionDataJob.cs` aplicado mas não validado in-game) e **[P-2.2]** 🟢 (débito arquitetural sobre compartilhamento de referência viva entre `PlayerComponent` e buffers de job) — ver `PA-01-01` abaixo, que conecta diretamente a essas duas pendências.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | `AUD-01-01` assume que as duas fontes de distância são equivalentes — não são | ✅ Resolvido em 2026-09-19 |
| PA-01-02 | B — Edge Case | 🟡 Importante | `CheckAILimit()` não atualiza a distância quando há inimigo humano ativo — o dead-band do LOD pode usar valor obsoleto/`-1` | ✅ Resolvido em 2026-09-19 |
| PA-01-03 | C — Erro de Lógica | 🟢 Menor | Alegação da §9 checklist item 6 de que "defaults preservam valores hoje hardcoded" é imprecisa para o novo campo `LODCloseDistanceMargin` | ✅ Resolvido em 2026-09-19 |
| PA-01-04 | C — Erro de Lógica | 🟢 Menor | Estilo de `[MinMax]` nos stubs inconsistente com o resto do arquivo (2 args em vez de 3 explícitos) | ✅ Resolvido em 2026-09-19 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador · ✅ Resolvido em 2026-09-19

**`AUD-01-01` assume que `SAINAILimit` e `BotComponent` calculam a mesma distância — na verdade usam dois mecanismos diferentes**

**Problema:** A spec técnica (§1, §6) propõe que `SAINAILimit.CheckAILimit()` pare de chamar `FindClosestHumanPlayer` e passe a ler `Bot.DistanceToClosestHuman`, tratando as duas fontes como o mesmo dado calculado duas vezes. Não são. Lendo o código real:

- `SAINAILimit.cs:40` chama `GameWorldComponent.Instance.PlayerTracker.FindClosestHumanPlayer(out float closestPlayerDistance, PlayerComponent, out _)` — a sobrecarga que recebe um `PlayerComponent` "consultante" (`PlayerSpawnTracker.cs:61-86`). Essa sobrecarga itera `quierrier.OtherPlayersData.DataList` e lê `otherPlayer.DistanceData.Distance` (`PlayerSpawnTracker.cs:75`) — que por sua vez é `PlayerDistanceData.Distance` → `Data.MainDirectionData.Distance` (`PlayerDistanceData.cs:49-52`), um valor **populado por um Job assíncrono** (`DirectionDataJob`, a mesma classe da pendência **[P-2.1]** da memória do mod — crash já corrigido mas **não validado in-game**, e da pendência **[P-2.2]** — débito arquitetural sobre a referência compartilhada entre o job e o `PlayerComponent`).
- `BotComponent.GetMinDistanceToHumanPlayer()` (`BotComponent.cs:227-248`) chama a **outra** sobrecarga — `FindClosestHumanPlayer(out float closestPlayerSqrMag, Vector3 targetPosition, out Player player)` (`PlayerSpawnTracker.cs:36-58`) — que faz uma varredura síncrona e fresca sobre `AlivePlayersDictionary.Values`, calculando `(component.Position - targetPosition).sqrMagnitude` diretamente, **sem depender de nenhum job**.

São dois pipelines de dado genuinamente diferentes: um passa por um Job multithread com sua própria cadência de atualização e um histórico recente de bug de correção de vida útil (`ArgumentOutOfRangeException`, Sessão 2 da memória); o outro é um cálculo síncrono direto na thread principal, recalculado a cada 0.5s.

**Por que importa:** A spec funcional (`001-perf-limitador-ia-lod-01-spec.md`, critério de aceite "Comportamento sensorial do `SAINAILimit`... permanece idêntico ao atual") exige que a degradação sensorial do `SAINAILimit` não mude. Se a implementação seguir a spec técnica como está e trocar a fonte de dado de `SAINAILimit` para o pipeline síncrono do `BotComponent`, isso é uma mudança de comportamento real, não só de eficiência — os valores de distância podem divergir (staleness diferente do Job vs. cálculo fresco; e potencialmente semântica diferente, já que uma sobrecarga vem de uma lista por-par já rastreada pelo próprio bot e a outra é uma varredura global). Isso viola diretamente o contrato de não-regressão que é a razão de existir deste item, e ainda amplia a superfície de exposição às pendências **[P-2.1]**/**[P-2.2]** (fazer mais código depender do pipeline do Job antes de validar o fix de crash já aplicado é o oposto do que se quer).

**Sugestão:** Inverter a direção da unificação. Em vez de fazer `SAINAILimit` adotar o pipeline do `BotComponent`, manter `SAINAILimit` com sua fonte atual (`FindClosestHumanPlayer` via `PlayerComponent`/`OtherPlayersData`) — que já é o mecanismo testado e em produção para esse propósito — e fazer o **LOD** (`BotComponent.GetMinDistanceToHumanPlayer`, usado só para bucketing de tier, tolerante a menor precisão) reusar o valor que `SAINAILimit` já expõe publicamente: `SAINAILimit.ClosestPlayerDistanceSqr` (`SAINAILimit.cs:12`, já `public`). Reescrever a §1/§5/§6 da spec técnica nessa direção: `BotComponent` lê `Bot.<caminho para a instância de SAINAILimit>.ClosestPlayerDistanceSqr` em vez de chamar `GetMinDistanceToHumanPlayer()`, com fallback para o cálculo síncrono próprio **apenas** quando `SAINAILimit.ClosestPlayerDistanceSqr` for `-1f` (caso `Bot.EnemyController.ActiveHumanEnemy` esteja ativo — ver `PA-01-02`) ou quando `SAINAILimit` ainda não tiver rodado seu primeiro ciclo. Isso elimina a mesma redundância sem trocar a fonte de dado que a spec funcional promete preservar, e não aumenta a dependência no pipeline de Job já sob suspeita.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Spec técnica reescrita (§1, §4, §5, §6, §8) na direção sugerida — `BotComponent.GetMinDistanceToHumanPlayer()` agora lê `AILimit.ClosestPlayerDistanceSqr` primeiro; `SAINAILimit.cs` não é mais tocado por este item. Descoberta adicional durante a correção: `ClosestPlayerDistanceSqr`, apesar do nome, já é distância **linear** (não squared) — documentado explicitamente na §1 como armadilha de nome, com aviso para não aplicar `Mathf.Sqrt()` a esse valor especificamente (o fallback síncrono, esse sim, continua precisando de `Sqrt`).

---

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-09-19

**`CheckAILimit()` não atualiza a distância quando há inimigo humano ativo — consequência direta de `PA-01-01`, afeta o dead-band do LOD**

**Problema:** `SAINAILimit.cs:29-33` — quando `Bot.EnemyController.ActiveHumanEnemy` é verdadeiro, `CurrentAILimit` vira `None` e `ClosestPlayerDistanceSqr` vira `-1f` **sem nunca chamar `FindClosestHumanPlayer`** nesse ciclo. Ou seja, `ClosestPlayerDistanceSqr` não é "a distância atual" em todo momento — é "a última distância calculada, ou -1 se em combate ativo agora". A spec técnica (§5, stub de `BotComponent.cs`) não trata esse caso: o pseudo-código lê `distToHuman` como se fosse sempre um valor de distância válido.

**Por que importa:** Se a sugestão de `PA-01-01` for aceita (LOD passa a ler de `SAINAILimit`), o `BotComponent` precisa saber diferenciar "não há dado ainda" (`-1f`, esperado durante combate) de "está muito longe" (`float.MaxValue`, esperado sem humano vivo) — hoje a spec técnica não distingue os dois casos, e tratar `-1f` como uma distância real quebraria a comparação `distToHuman <= closeDist` (uma distância negativa sempre "vence" qualquer limiar de perto). Nesse cenário específico (combate ativo com humano) o bot **deveria** estar em Tier 0 de qualquer forma (via `inCombat`/`isUnderFire`, já cobertos em `BotComponent.cs:267`), então o efeito prático tende a ser benigno — mas a spec técnica precisa declarar isso explicitamente, não deixar implícito.

**Sugestão:** Adicionar ao §5/§6 da spec técnica: ao consumir a distância de `SAINAILimit` (per `PA-01-01`), tratar `ClosestPlayerDistanceSqr < 0f` como "sem dado numérico — mas irrelevante, porque `Bot.EnemyController.ActiveHumanEnemy` já implica Tier 0 por outro caminho (`isCloseToHuman` pode ficar `false` nesse ramo sem prejuízo, já que `inCombat`/`isUnderFire` cobrem o caso)". Explicitar essa garantia como comentário no código final, não deixar implícito.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Stub de `GetMinDistanceToHumanPlayer()` na §5 agora checa `aiLimitDist >= 0f` explicitamente antes de usar o valor de `AILimit`, com comentário citando `SAINAILimit.cs:29-33` e a garantia de que `inCombat`/`isUnderFire` cobrem Tier 0 nesse cenário.

---

### PA-01-03 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-19

**Checklist §9 item 6 da spec técnica alega que os defaults "preservam exatamente os valores hoje hardcoded" — impreciso para `LODCloseDistanceMargin`**

**Problema:** `LODCloseDistanceMargin` (default proposto: `10f`) é um campo inteiramente novo — hoje não existe margem nenhuma (o comportamento atual equivale a margem `0`). A afirmação da spec técnica de que os defaults preservam o comportamento atual é verdadeira para `LODCloseDistance`/`LODMidDistance`/os dois intervalos, mas não para a margem, que **muda** o comportamento de tier-switching por design (é o próprio objetivo de `AUD-01-02`).

**Por que importa:** Não é um erro que impeça a implementação, mas o item 6 da §9 é usado pelo `/code-review` posterior como evidência de conformidade — deixar essa imprecisão sem nota pode fazer uma revisão futura aceitar "sem mudança de comportamento" quando na verdade há uma mudança de comportamento intencional (positiva) nessa fronteira específica.

**Sugestão:** Ajustar a redação do item 6 da §9 para: "Defaults de `LODCloseDistance`/`LODMidDistance`/intervalos preservam os valores hoje hardcoded; `LODCloseDistanceMargin` é comportamento novo (hoje equivale a 0), mudança intencional coberta pelo critério de aceite de `AUD-01-02` na spec funcional — não é uma regressão silenciosa."

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Redação do item 6 da §9 corrigida exatamente como sugerido.

---

### PA-01-04 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-09-19

**Stubs de `[MinMax(...)]` usam 2 argumentos; todo o resto do arquivo real usa 3 explícitos**

**Problema:** Os stubs da §5 usam `[MinMax(20f, 100f)]` (2 args). Confirmado em `ConfigAttributes.cs:64` que o construtor aceita `(float min, float max, float rounding = 100f)` — então 2 args **compila** (usa o default de `rounding`), não é um erro de compilação. Mas todo campo existente em `AILimitSettings.cs` (`AILimitUpdateFrequency`, `AILimitRanges`, `MaxVisionRanges`, `MaxHearingRanges`) usa a forma explícita de 3 argumentos, mesmo quando o terceiro bate com o default.

**Por que importa:** Puramente estilístico — não afeta funcionamento. Inconsistência de estilo num arquivo que hoje é 100% consistente.

**Sugestão:** Ajustar os stubs da §5 para incluir o terceiro argumento explicitamente (ex.: `[MinMax(20f, 100f, 1f)]`, seguindo o padrão de `MaxVisionRanges`/`MaxHearingRanges` que usam rounding `1f` para valores em metros).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Stubs ajustados — `1f` de rounding para os campos de distância (metros), `100f` para os de intervalo (segundos, mesmo padrão de `AILimitUpdateFrequency`... na verdade esse usa `10f`; mantido `100f` por serem valores fracionários pequenos onde mais casas decimais fazem sentido no editor).
