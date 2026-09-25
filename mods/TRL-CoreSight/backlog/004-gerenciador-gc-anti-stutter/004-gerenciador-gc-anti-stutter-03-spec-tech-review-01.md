# 004 — gerenciador-gc-anti-stutter · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [004-gerenciador-gc-anti-stutter-02-spec-tech.md](004-gerenciador-gc-anti-stutter-02-spec-tech.md)  
**Data:** 2026-09-15T22:37:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 Importante | Reativação imediata do GC em caso de morte, extração ou encerramento da raid | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | C — Erro de Lógica | 🟡 Importante | Cooldown e atraso de estabilização de coleta no inventário | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | A — Gap | 🟢 Menor | Telemetria de memória e supressão no overlay OnGUI de Debug | ✅ Resolvido em 2026-09-15 |

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

### PA-01-01 · B — Edge Case · 🟡 Importante

**Reativação imediata do GC em caso de morte, extração ou encerramento da raid**

**Problema:** Se o jogador for eliminado no ápice de um tiroteio (momento em que `GCMode` está `Disabled`), ao carregar as telas de pós-raid ou retornar ao menu principal, o GC poderia continuar desligado se a transição não reverter a flag.

**Por que importa:** Sem o Garbage Collector ativo no menu principal ou no Hideout, o acúmulo de texturas e menus levaria a vazamento de memória e eventual travamento do cliente.

**Sugestão:** Implementar checagem no `OnUpdate()`: se `_mainPlayer.HealthController != null && !_mainPlayer.HealthController.IsAlive`, reativar `GarbageCollector.GCMode = GarbageCollector.Mode.Enabled` imediatamente. Além disso, cercar o `Cleanup()` com bloco de salvaguarda incondicional.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionada verificação de vida e salvaguarda idempotente no `Cleanup()` e `OnDestroy()`.

---

### PA-01-02 · C — Erro de Lógica · 🟡 Importante

**Cooldown e atraso de estabilização de coleta no inventário**

**Problema:** Ao abrir rapidamente o inventário várias vezes seguidas para transferir itens, se uma coleta completa fosse disparada a cada abertura, o jogador sentiria um travamento na tela de looting.

**Por que importa:** O objetivo do mod é eliminar stutters, não transferi-los para o inventário.

**Sugestão:** Exigir que o inventário permaneça aberto continuamente por pelo menos 0.5 segundos antes de disparar a coleta, e aplicar um cooldown estrito de no mínimo 20 segundos entre coletas consecutivas.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Mecânica de debounce de 0.5s e cooldown de 20s incorporada à lógica de coleta segura.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Telemetria de memória e supressão no overlay OnGUI de Debug**

**Problema:** A spec técnica inicial não especificava a exibição no HUD do status de supressão e da memória alocada gerenciada.

**Por que importa:** Essencial para constatar visualmente se o GC foi suprimido durante a visada ou disparo.

**Sugestão:** Exibir `GC: Suprimido / Normal` e `Mono Heap: X MB` na janela de debug (F11).

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Métrica adicionada ao `PerformanceManager.OnGUI()`.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |
