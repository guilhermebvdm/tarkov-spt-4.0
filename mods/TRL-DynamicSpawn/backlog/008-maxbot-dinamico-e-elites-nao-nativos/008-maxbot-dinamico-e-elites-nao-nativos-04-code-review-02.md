# 008 — maxbot-dinamico-e-elites-nao-nativos · Code Review 02

**Mod:** TRL-DynamicSpawn
**Data:** 2026-08-12T22:10:00-03:00

> Análise crítica do código implementado para a Restrição Noturna dos Cultistas (v3.2.4) e Invasão Completa do Trio Goons (v3.2.5) no SPT 4.0 / FIKA.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 3 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | F — Melhoria opcional | 🟢 Menor | Tratamento de exceção com fallback no `IsNightTimeForCultists` | ✅ Aplicado |
| CR-02-02 | D — Arquitetura | 🟢 Menor | Injeção conjunta do Trio Goons (Knight, BigPipe, BirdEye) em mapas não-nativos | ✅ Aplicado |
| CR-02-03 | F — Melhoria opcional | 🟢 Menor | Pré-carregamento em lote dos perfis do Trio Goons no `AddToTargetBackup` | ✅ Aplicado |

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

### CR-02-01 · F — Melhoria opcional · 🟢 Menor

**Tratamento de exceção com fallback no `IsNightTimeForCultists`**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:1275-1293`](../../Client/Components/DynamicSpawnManager.cs#L1275-L1293)

**Análise:** O método `IsNightTimeForCultists()` calcula o horário atual da partida via `Singleton<GameWorld>.Instance.GameDateTime.Calculate().Hour`. O uso de um bloco `try-catch` com retorno `true` (fallback seguro) garante que, caso o objeto do relógio ainda não esteja instanciado no frame inicial, o mod não quebrará a raid nem impedirá o spawn inadvertidamente.

**Decisão:**
- `[x]` Aceitar sugestão (Manter implementação atual)

---

### CR-02-02 · D — Arquitetura · 🟢 Menor

**Injeção conjunta do Trio Goons (Knight, BigPipe, BirdEye) em mapas não-nativos**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:575-588`](../../Client/Components/DynamicSpawnManager.cs#L575-L588)

**Análise:** Em invasões não-nativas do `bossKnight` (quando `DisableFollowers == false`), o mod agora enfileira os 3 integrantes dedicados (`bossKnight`, `followerBigPipe`, `followerBirdEye`) vinculados à mesma `selectedZone`. Por pertencerem à categoria `elites`, são colocados no topo do `spawnList` e injetados juntos no segundo 0 da onda.

**Decisão:**
- `[x]` Aceitar sugestão (Manter implementação atual)

---

### CR-02-03 · F — Melhoria opcional · 🟢 Menor

**Pré-carregamento em lote dos perfis do Trio Goons no `AddToTargetBackup`**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:148-154`](../../Client/Components/DynamicSpawnManager.cs#L148-L154)

**Análise:** Ao detectar `BossKnight.Enable == true`, o cliente solicita o pré-carregamento síncrono dos perfis de `bossKnight`, `followerBigPipe` e `followerBirdEye` no `AddToTargetBackup` do SPT durante o carregamento inicial da raid. Isso evita latência e *stutters* na requisição HTTP no momento da injeção no mapa.

**Decisão:**
- `[x]` Aceitar sugestão (Manter implementação atual)

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-06 | Code review 01 executado para o MaxBot Dinâmico e Elites Não-Nativos. |
| 2026-08-12 | Code review 02 executado para a Restrição Noturna dos Cultistas e Invasão Completa do Trio Goons. |
