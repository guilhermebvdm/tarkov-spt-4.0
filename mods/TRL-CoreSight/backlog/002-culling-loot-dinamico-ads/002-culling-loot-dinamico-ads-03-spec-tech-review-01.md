# 002 — culling-loot-dinamico-ads · Review Técnica 01

**Mod:** TRL-CoreSight  
**Spec técnica revisada:** [002-culling-loot-dinamico-ads-02-spec-tech.md](002-culling-loot-dinamico-ads-02-spec-tech.md)  
**Data:** 2026-09-15T22:26:00Z  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Tipagem genérica do patch em `GameWorld.RegisterLoot<T>` | ✅ Resolvido em 2026-09-15 |
| PA-01-02 | B — Edge Case | 🟡 Importante | Restauração de visibilidade ao morrer ou entrar em modo Spectator | ✅ Resolvido em 2026-09-15 |
| PA-01-03 | A — Gap | 🟢 Menor | Telemetria de itens ocluídos no overlay OnGUI de Debug | ✅ Resolvido em 2026-09-15 |

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

**Tipagem genérica do patch em `GameWorld.RegisterLoot<T>`**

**Problema:** O stub propôs `MakeGenericMethod(typeof(InteractableObject))`, porém no código do EFT ([`LootItem.cs:210`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.Interactive/LootItem.cs#L210)), a chamada ocorre com `gameWorld.RegisterLoot(this)`, inferindo `T = LootItem`. Um patch focado apenas em `InteractableObject` poderia não interceptar chamadas diretas de `LootItem`.

**Por que importa:** Se o patch não interceptar a instanciação genérica correta, itens dropados dinamicamente em raid não serão adicionados ao tracker de culling.

**Sugestão:** Fazer o patch em `LootItem.Init` ou interceptar a assinatura genérica correspondente `MakeGenericMethod(typeof(LootItem))`. Além disso, o gerenciador pode efetuar uma sincronização leve periódica (a cada 10s) da lista `GameWorld.LootList` para garantir 100% de captura sem depender exclusivamente de patches em métodos genéricos.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Ajustado o patch para cobrir a tipagem genérica adequada e adicionada uma varredura de sincronização resiliente na lista `GameWorld.LootList`.

---

### PA-01-02 · B — Edge Case · 🟡 Importante

**Restauração de visibilidade ao morrer ou entrar em modo Spectator**

**Problema:** Se o jogador morrer ou a raid terminar abruptamente, itens que estavam com `method_10(false)` permaneceriam invisíveis caso outro jogador no modo cooperativo (FIKA) ou a câmera livre tentasse inspecionar a área.

**Por que importa:** No FIKA, outros companheiros vivos precisam enxergar o loot normalmente.

**Sugestão:** Se `_mainPlayer == null` ou `!_mainPlayer.HealthController.IsAlive`, disparar `RestoreAll()` para restaurar a visibilidade de 100% do loot da partida.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Verificação de vida adicionada ao `OnUpdate()`. Se o jogador estiver morto, o culling restaura todos os itens imediatamente.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Telemetria de itens ocluídos no overlay OnGUI de Debug**

**Problema:** A spec técnica não definiu uma métrica visível para acompanhar quantos itens de loose loot estão sendo culled em tempo real.

**Por que importa:** Sem telemetria, é difícil para o desenvolvedor e para o usuário final conferirem no F12 se o sistema está operando e qual a economia de draw calls gerada.

**Sugestão:** Adicionar a propriedade pública `CulledLootCount` ao `LootCullingManager` e exibi-la no `OnGUI` do `PerformanceManager`.

**Decisão:**
- `[x]` Aceitar sugestão  
**Resolução:** Adicionada métrica `Culled Loot: N` ao overlay de debug.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Review técnica 01 concluída: 0 bloqueadores 🔴, 2 importantes 🟡 resolvidos, 1 menor 🟢 resolvido. Aprovado para implementação de código (`/code-mod`). |
