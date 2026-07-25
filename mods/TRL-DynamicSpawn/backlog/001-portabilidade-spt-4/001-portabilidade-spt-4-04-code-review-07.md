# 001 — portabilidade-spt-4 · Code Review 07 (Hotfix NRE)

**Mod:** TRL-DynamicSpawn
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Spec técnica:** [001-portabilidade-spt-4-02-spec-tech.md](001-portabilidade-spt-4-02-spec-tech.md)
**Asbuild:** Não aplicável
**Data:** 2026-07-18T00:24:00-03:00

> Análise e documentação de Hotfix em tempo real envolvendo a Engine de Spawns Assíncronos do SPT-AKI 4.0.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-07-01 | A — Crítico | 🔴 Bloqueador | NullReferenceException no SPT Core Profile Generator | ✅ Resolvido |

---

## Pontos

### ✅ CR-07-01 · A — Crítico · 🔴 Bloqueador (Resolvido)
*NullReferenceException no SPT Core Profile Generator (BotsPresets.CreateProfile)*

**Local:** [`mods/TRL-DynamicSpawn/Client/Components/DynamicSpawnManager.cs:174`](../../Client/Components/DynamicSpawnManager.cs#L174)

**Problema (Bug Latente no SPT 4.0):**
As rotinas `GenerateBotsAsync` e `GenerateBossAsync` estavam inicializando manualmente um `new BotSpawnParams()` vazio e injetando no modelo de requisição de perfis:
```csharp
BotSpawnParams spawnParams = new BotSpawnParams();
BotProfileDataClass profileData = new BotProfileDataClass(side, spawnType, difficulty, 0f, spawnParams);
```
No Tarkov vanilla (antigo) e SPT 3.x, enviar os parâmetros vazios não surtia efeitos colaterais. No entanto, no ecossistema do SPT 4.0, o core do BepInEx repassa esse `BotSpawnParams` para o Node.js em formato JSON. Por ser uma estrutura vazia recém-alocada em memória C#, alguns ponteiros e sub-listas internas causavam estouro de referência `NullReferenceException` durante o mapping assíncrono em `BotCreatorClass.GenerateProfile`, travando a onda e a engine do mod permanentemente na raid atual.

**Resolução:**
O parâmetro de `spawnParams` foi substituído por uma passagem direta explícita de `null` no construtor `BotProfileDataClass`.
```csharp
BotProfileDataClass profileData = new BotProfileDataClass(side, spawnType, difficulty, 0f, null);
```
A API do BepInEx do SPT detecta o valor nulo de maneira segura e lida com a requisição de perfis utilizando templates isolados padrão em memória, prevenindo crashes.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-18 | Code review 07 (Hotfix) gerada e documentada via solicitação. |
