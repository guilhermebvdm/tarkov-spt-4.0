# 001 — portabilidade-spt-4 · Code Review 02

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** Não aplicável (fallback legado)
**Data:** 2026-07-17T22:15:00-03:00

> Análise crítica do código implementado. Cada achado recebe um ID `CR-02-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | D — Arquitetura | 🟠 Forte | Desativação de Follower de Bosses Incorreta | Pendente |
| CR-02-02 | E — Manutenção | 🟡 Médio | Reflection Insegura de Campo OnPlayerDead | Pendente |

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

### CR-02-01 · D — Arquitetura · 🟠 Forte

**Desativação de Follower de Bosses Incorreta**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:298-301`](../../Client/Components/DynamicSpawnManager.cs#L298-L301)

**Problema:**
```csharp
int count = info.DisableFollowers ? 1 : 1; 

var tBoss = GenerateBossAsync(bossType, BotDifficulty.normal, count);
```
O ternário acima define `count` como `1` em ambas as condições, o que anula e ignora a lógica de desativar ou manter escoltas/followers configurada no painel web, forçando o spawn de apenas 1 bot independente das configurações de followers do boss.

**Por que importa:**
A quantidade de bots no spawn de boss será sempre fixa em 1 (sem followers), impossibilitando bosses com escoltas nativas de operarem com capangas no mapa (reduzindo a dificuldade projetada).

**Sugestão:**
Corrigir a expressão ou recuperar o valor real de capangas quando `DisableFollowers` for falso.

---

### CR-02-02 · E — Manutenção · 🟡 Médio

**Reflection Insegura de Campo OnPlayerDead**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/BotDespawnManager.cs:193-204`](../../Client/Components/BotDespawnManager.cs#L193-L204)

**Problema:**
O código realiza uma varredura via Reflection do campo privado `OnPlayerDead` da classe `Player`:
```csharp
var field = typeof(Player).GetField("OnPlayerDead", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
```

**Por que importa:**
Assinaturas de campos de eventos e delegates privados no assembly do Tarkov decompilado mudam constantemente entre sub-versões (ex: 0.16.x do SPT 4.0). Depender de strings mágicas literais para invocar eventos de morte de player via Reflection bruta gera alto risco de incompatibilidade futura.

**Sugestão:**
Obter o delegate utilizando helpers do SPT (`AccessTools` do Harmony ou wrappers nativos) ou encapsular em try-catch silencioso (como já está feito) mas anotando a dependência de compatibilidade na documentação.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-17 | Code review 02 criada via `/code-review` |
