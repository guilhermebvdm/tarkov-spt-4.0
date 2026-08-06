# 008 — maxbot-dinamico-e-elites-nao-nativos · Code Review 01

**Mod:** TRL-DynamicSpawn
**Data:** 2026-08-06T01:54:00-03:00

> Análise crítica do código implementado para o MaxBot Dinâmico e o Sistema de Invasão Dinâmica de Elites/Rogues Não-Nativos no SPT 4.0 / FIKA.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 3 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟢 Menor | Comparação case-insensitive de nomes de zonas em `GetZoneFromConfig` | ✅ Aplicado |
| CR-01-02 | E — Legibilidade/manutenção | 🟢 Menor | Transição assíncrona do `dynamicCap` em ondas com múltiplos elites não-nativos | ✅ Aceito |
| CR-01-03 | D — Arquitetura | 🟢 Menor | Validação da cache de reflexão do `FikaHelper.IsClient` | ✅ Aceito |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recommended; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · F — Melhoria opcional · 🟢 Menor

**Comparação case-insensitive de nomes de zonas em `GetZoneFromConfig`**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:1190-1205`](../../Client/Components/DynamicSpawnManager.cs#L1190-L1205)

**Problema:** O método `GetZoneFromConfig` busca a zona de spawn na cena utilizando comparação estrita de string (`z.NameZone == selectedZoneName`). Se a Web UI ou o arquivo `config.json` contiverem divergências de maiúsculas/minúsculas (por exemplo, `zoneScavBase` em vez de `ZoneScavBase`), a busca falha e recorre ao fallback aleatório.

**Por que importa:** Garantir correspondência insensível a caixa (*case-insensitive*) torna o resolver de zonas mais resiliente contra variações de entrada de usuários ou integrações externas.

**Sugestão:** Substituir a comparação estrita por `string.Equals(z.NameZone, selectedZoneName, StringComparison.OrdinalIgnoreCase)` no LINQ `FirstOrDefault`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Aplicado com sucesso em [`DynamicSpawnManager.cs`](../../Client/Components/DynamicSpawnManager.cs).

---

### CR-01-02 · E — Legibilidade/manutenção · 🟢 Menor

**Transição assíncrona do `dynamicCap` em ondas com múltiplos elites não-nativos**

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:465-525`](../../Client/Components/DynamicSpawnManager.cs#L465-L525)

**Problema:** Na 1ª onda (`isFirstWave`), se múltiplos elites não-nativos (ex: 3 Rogues + 2 Raiders) forem enfileirados em mapas sem ondas nativas, o `GetSpecialBotsCount()` é avaliado antes que os perfis recém-criados atinjam o estado `IsAlive`. Por conta disso, a expansão do `dynamicCap` ocorre à medida que cada bot termina de spawnar, podendo postergar a injeção dos Scavs/PMCs restantes para o próximo ciclo de Warmup (30s).

**Por que importa:** Não causa travamentos nem perda de bots (a população se completa em 1–2 ciclos de 30s), mas representa um comportamento de spawn em duas etapas durante a 1ª onda de raids densas.

**Sugestão:** Manter como comportamento projetado por design, pois a injeção em etapas suaviza a carga de criação de entidades (*stutter prevention*).

**Decisão:**
- `[x]` Rejeitar (deferir / aceitar como dívida)

**Resolução:** Aceito como comportamento desejado por design de performance.

---

### CR-01-03 · D — Arquitetura · 🟢 Menor

**Validação da cache de reflexão do `FikaHelper.IsClient`**

**Local:** [`mods/TRL-DynamicSpawn/Client/Helpers/FikaHelper.cs:9-35`](../../Client/Helpers/FikaHelper.cs#L9-L35)

**Problema:** A inspeção do runtime do `Fika.Core` para avaliar `FikaBackendUtils.IsClient` poderia gerar custo de reflexão se executada repetidamente em patches de alta frequência.

**Por que importa:** O `FikaHelper` agora avalia o `AppDomain` e armazena o `PropertyInfo` estático uma única vez (`_reflectionEvaluated = true`), garantindo 0ms de busca e execução ultrarrápida.

**Sugestão:** Manter a implementação atual validada.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Validado e mantido.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-06 | Code review inicial executado com 0 bloqueadores 🔴, 0 fortes 🟠, 0 médios 🟡 e 3 menores 🟢. |
