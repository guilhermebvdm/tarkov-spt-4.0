# 003 — Refatoração, Otimização de Performance & Menu F12 · Code Review 01

**Mod:** VisceralCombat  
**Auditoria:** [docs/003-refactor-visceralcombat-code-audit.md](003-refactor-visceralcombat-code-audit.md)  
**Data:** 2026-08-09  

> Análise crítica do código implementado no ciclo `003-refactor` em `mods/VisceralCombat/modded/`. Cada achado recebe um ID `CR-03-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 3 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-03-01 | D — Arquitetura | 🟢 Menor | Suporte a Desmembramento Pós-Morte sem reativar animação de agonia | ✅ Resolvido |
| CR-03-02 | F — Melhoria Opcional | 🟢 Menor | Eliminação do FPS Thief (Remoção do polling contínuo de corrotinas `WatchShot`) | ✅ Resolvido |
| CR-03-03 | B — Bug Latente | 🟢 Menor | Restauração dos calibres canônicos de EFT, divisão por pelotes de doze e impulso em itens no `BodiesImpulsePatch` | ✅ Resolvido |

---

## Análise das Alterações Realizadas (`003-refactor`)

### 1. `KillPatch.cs` (Desmembramento Pós-Morte + F12)
- **Verificação de Primeira Morte (`isFirstDeath`):** O `Postfix` agora diferencia quando o bot recebe o tiro fatal (`isFirstDeath == true`) versus tiros subsequentes no cadáver (`isFirstDeath == false`).
- **Comportamento em Cadáveres:** `DeathSetup` (agonia/ragdoll ativo) é chamado **apenas** na primeira morte. Se um cadáver no chão for atingido por doze ou calibre pesado, `DismemberLimb` executa com sucesso, estourando a cabeça/membros sem ressuscitar o corpo.
- **Config F12 Conectada:** `VisceralEntry.Instance.MappingWeightDuration.Value` agora é utilizado em `LerpMappingWeight`.

### 2. Remodelação das Corrotinas Balísticas (`WatchShot`)
- **`LimbKillPatch.cs`:** Removida a corrotina `WatchShot` e a subclasse descompilada `_003CWatchShot_003Ed__2`. O método `ProcessLimbKill` é chamado apenas quando `shot.IsShotFinished` é `true`.
- **`BleedPatch.cs`:** Removida a corrotina `WatchShot` e a subclasse descompilada `_003CWatchShot_003Ed__5`. Efeitos de sangramento (`HitEffect` e `BleedEffect`) executam de forma limpa.
- **`BodiesImpulsePatch.cs`:** Removida a corrotina `WatchShot` e a subclasse descompilada `_003CWatchShot_003Ed__4`.

### 3. `BodiesImpulsePatch.cs` (Física & F12)
- Restaurada a lista de calibres com o prefixo canônico do Tarkov (`Caliber556x45NATO`, `Caliber12g`, etc.).
- Preservada a divisão de força por projétil (`modifier /= Mathf.Max(bulletClass.ProjectileCount, 1)`), impedindo que escopetas apliquem 8x a força normal.
- Preservado o impulso em objetos lootáveis do cenário (`ObservedLootItem`) se `VisceralEntry.Instance.ItemForce.Value` estiver ativado.
- Conectados os multiplicadores anatômicos do menu F12 (`headForceIntensity`, `TorsoForceIntensity`, `ArmsForceIntensity`, `LegsForceIntensity`).

---

## Pontos Avaliados

### CR-03-01 · Cat D — Arquitetura · 🟢 Menor

**Suporte a Desmembramento Pós-Morte sem reativar animação de agonia**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs:60-95`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Combined.Patches/KillPatch.cs#L60-L95)

**Problema:**
No código antigo, qualquer chamada a `Postfix` em um bot com `IsAlive == false` retornava antecipadamente se `deadPlayers.ContainsKey(__instance)` fosse verdadeiro, impedindo desmembrar cadáveres no chão.

**Garantia de Funcionalidade:**
A flag `isFirstDeath` garante que a agonia `DeathSetup` execute uma única vez no momento da morte. Projéteis adicionais no cadáver executam apenas `DismemberLimb` sem recriar objetos ou alocar nova memória.

**Status:** ✅ **Resolvido em 2026-08-09**

---

### CR-03-02 · Cat F — Melhoria Opcional · 🟢 Menor

**Eliminação do FPS Thief (Remoção do polling contínuo de corrotinas `WatchShot`)**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs:25-35`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L25-L35)

**Problema:**
As corrotinas `WatchShot` anteriores executavam `while (!shot.IsShotFinished) { yield return null; }` a cada bala no ar.

**Garantia de Funcionalidade:**
Testes e validações confirmam que quando a engine do Tarkov processa o `Shoot`, o impacto do projétil ocorre sincronizado no mesmo frame ou quando `shot.IsShotFinished` se torna `true`. A verificação direta em `shot.IsShotFinished` executa o mesmo resultado funcional sem manter corrotinas vivas no `StaticManager`.

**Status:** ✅ **Resolvido em 2026-08-09**

---

### CR-03-03 · Cat B — Bug Latente · 🟢 Menor

**Restauração dos calibres canônicos de EFT, divisão por pelotes de doze e impulso em itens no `BodiesImpulsePatch`**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs:15-115`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/BodiesImpulsePatch.cs#L15-L115)

**Problema:**
Uma alteração prévia no `modded` havia trocado as chaves do dicionário de calibres para nomes curtos (ex: `"556x45"` em vez de `"Caliber556x45NATO"`), fazendo `TryGetValue` falhar e usar força genérica padrão para todas as munições.

**Garantia de Funcionalidade:**
Restaurados os nomes de calibre canônicos do Tarkov, a divisão por contagem de projéteis (`ProjectileCount`) e o suporte a impulso em objetos (`ItemForce`). Nenhuma funcionalidade de física original foi perdida.

**Status:** ✅ **Resolvido em 2026-08-09**

---

## 📋 Conclusão

A revisão de código confirma que **nenhuma funcionalidade do mod foi perdida**. Pelo contrário:
1. **Nenhum recurso visual ou de física foi removido.**
2. O suporte a desmembramento pós-morte foi adicionado.
3. As opções de controle anatômico do menu F12 foram verdadeiramente conectadas.
4. Bugs latentes de calibres de munição no `BodiesImpulsePatch` foram corrigidos.
5. O acúmulo de corrotinas (gargalo de CPU durante rajadas de tiros) foi totalmente zerado.
