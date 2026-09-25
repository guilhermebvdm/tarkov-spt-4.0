# 005 — camuflagem-natural-e-percepcao-ia · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [005-time-slicing-ia-seguro-02-spec-tech.md](005-time-slicing-ia-seguro-02-spec-tech.md)  
**Data:** 2026-09-15T22:42:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Isolamento de física: balas e movimentação nunca bloqueadas por Foliage | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | B — Edge Case | 🟡 Importante | Validação rigorosa de superfície (asfalto e interiores imunes) | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | A — Gap | 🟢 Menor | Telemetria de camuflagem e cobertura no overlay OnGUI de Debug | ✅ Resolvido em 2026-09-15 |

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

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**Isolamento de física: balas e movimentação nunca bloqueadas por Foliage**

**Problema:** Se o colisor do escudo fosse criado em layer inadequado (como `Default` ou `HighPolyCollider`), ele funcionaria como uma parede invisível contra o próprio jogador, impedindo sua movimentação ou bloqueando disparos de armas e granadas.

**Por que importa:** O combate do Tarkov exige que balas atravessem a grama e causem dano pleno.

**Sugestão:** Utilizar estritamente o layer `Foliage` (Layer 15) da Unity. O layer `Foliage` é testado exclusivamente pelas rotinas de visão de IA (`HighPolyWithTerrainMaskAI`), sendo 100% ignorado pelo layer de movimentação de jogador (`PlayerCollisionsMask`) e pelos projéteis balísticos (`HitColliderMask`).

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Camada configurada como `LayerMask.NameToLayer("Foliage")` em tempo de execução.

---

### PA-01-02 · B — Edge Case · 🟡 Importante

**Validação rigorosa de superfície (asfalto e interiores imunes)**

**Problema:** Deitar no chão de um galpão de concreto ou em estradas pavimentadas não deve conceder invisibilidade de capim.

**Por que importa:** Camuflagem fantasma em ambientes urbanos ou internos quebraria o fairplay e a imersão.

**Sugestão:** Executar raycast descendente periódico (a cada 0.5s) que valida se o colisor do piso é do tipo `TerrainCollider` ou se possui tags/nomes associados a terreno natural (`grass`, `dirt`, `ground`, `mud`). Caso contrário, desativa imediatamente o escudo rasteiro.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionada verificação no método `CheckIfOnVegetation()`.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Telemetria de camuflagem e cobertura no overlay OnGUI de Debug**

**Problema:** Sem indicação no HUD de debug, é difícil saber se a postura do jogador e o solo atual estão ativando a camuflagem com sucesso.

**Por que importa:** Permite ao jogador auditar no F11 a exata resposta do sistema ao deitar ou agachar no mato.

**Sugestão:** Exibir `Camuflagem: ATIVA (Grama/Copa) / INATIVA` no painel OnGUI do `PerformanceManager`.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionada métrica de status ao overlay.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |
