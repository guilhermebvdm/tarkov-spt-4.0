# 006 — culling-capsulas-balas · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [006-culling-capsulas-balas-02-spec-tech.md](006-culling-capsulas-balas-02-spec-tech.md)  
**Data:** 2026-09-15T22:06:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Restauração de `EFTHardSettings.FLYING_SHELLS_VISIBLE_DISTANCE` ao término de raid | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | C — Erro de Lógica | 🟡 Importante | Simplificação do patch de `Player.FirearmController` evitando busca reflexiva frágil | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | B — Edge Case | 🟢 Menor | Suporte a armas com câmara múltipla (`StartSpawnAllShells`) | ✅ Resolvido em 2026-09-15 |

---

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

### PA-01-01 · A — Gap · 🟡 Importante

**Restauração de `EFTHardSettings.FLYING_SHELLS_VISIBLE_DISTANCE` ao término de raid**

**Problema:** A spec técnica prevê alterar `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE`, mas não define explicitamente onde e quando o valor padrão nativo (`25.0f`) é restaurado caso o mod seja desativado pelo usuário no F12 ou ao voltar para o menu principal.

**Por que importa:** Se o jogador alterar o valor no F12 e depois desativar o mod (`ModEnabled = false`), o motor do Tarkov reteria o último valor em memória estática entre partidas.

**Sugestão:** Armazenar `_originalFlyingShellsDistance = EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE` no início da raid em `PerformanceManager.cs` e restaurá-lo no método `OnRaidEnd()` / `OnDestroy()`.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionado ao `PerformanceManager` o armazenamento do valor original e restauração explícita no ciclo de encerramento da partida.

---

### PA-01-02 · C — Erro de Lógica · 🟡 Importante

**Simplificação do patch de `Player.FirearmController` evitando busca reflexiva frágil**

**Problema:** O stub propõe uma varredura reflexiva dinâmica em todos os tipos aninhados de `Player` para encontrar `StartSpawnShell`. Essa busca pode encontrar métodos homônimos ou falhar silenciosamente caso o compilador ofusque ou reorganize classes internas.

**Por que importa:** Como `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE` já é consultado diretamente pelo método nativo `method_7()` de `Player.cs` a cada frame, alterar a variável nativa já cobre 100% dos controllers de jogador sem precisar de patch reflexivo arriscado.

**Sugestão:** Focar o patch Harmony estritamente em `WeaponManagerClass.StartSpawnShell` e `StartSpawnAllShells` (que são os métodos públicos estáveis usados por todos os bots), deixando a parte de players coberta pelo ajuste de `EFTHardSettings.Instance.FLYING_SHELLS_VISIBLE_DISTANCE`.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** O patch Harmony foi focado exclusivamente em `WeaponManagerClass`, garantindo estabilidade e desacoplamento sem reflection complexa.

---

### PA-01-03 · B — Edge Case · 🟢 Menor

**Suporte a armas com câmara múltipla (`StartSpawnAllShells`)**

**Problema:** Espingardas de dois canos (Double Barrel shotgun) e revolvers disparam `StartSpawnAllShells` ao ejetar múltiplos cartuchos ao mesmo tempo.

**Por que importa:** Se apenas `StartSpawnShell` fosse patcheado, um bot disparando com espingarda de dois canos a 80m poderia ejetar duas cápsulas no mundo.

**Sugestão:** Incluir `StartSpawnAllShells` como alvo adicional no mesmo patch ou patch análogo.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Alvo adicionado ao `ShellSpawnCullingPatch`.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |
