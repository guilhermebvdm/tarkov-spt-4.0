---
title: Inventário de mods para migração SPT 3.x → 4.0
date: 2026-05-02
status: 🔵 Em andamento
authors: Guilherme
---

# Inventário de mods — Migração SPT 3.x → 4.0

Catálogo dos mods atualmente em uso no `tarkov-spt-3.0` que precisam ser avaliados para portar/recriar no SPT 4.0.

> **Atenção:** SPT 4.0 tem arquitetura incompatível com 3.x. Este documento serve como ponto de partida para decidir o que migrar, refazer ou descartar.

## Convenções de preenchimento

Cada mod tem dois links: versão 3.x (referência) e versão 4.x (target da migração).

- `[texto](url)` — link encontrado
- `🔍 buscar` — ainda não pesquisado
- `—` — confirmado que não existe (ex: autor não lançou versão 4.x)

Quando o link 4.x for `—`, decidir o status: `🔧 Desenvolver`, `🟠 Aguardar upstream` ou `⚫ Não incluir`.

## Base — UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Tipo** | Client (C# / BepInEx) |
| **Função** | Cliente multiplayer (Fika) |
| **Link 3.x** | 🔍 buscar |
| **Link 4.x** | 🔍 buscar |
| **Prioridade** | 🔥 Crítica — primeiro mod a ser migrado |
| **Status migração** | 🔵 Avaliar |
| **Bloqueia** | Todos os demais mods do projeto dependem desta base estar funcional |

## Mods Client (C# / BepInEx)

Mods que rodam em cima da base do UltraFika-Plugin.

| Mod | Link 3.x | Link 4.x | Função | Prioridade | Status |
|---|---|---|---|---|---|
| SAIN | 🔍 buscar | 🔍 buscar | Substituição do sistema de IA dos bots | Alta | 🔵 Avaliar |
| SPT-DynamicMaps | 🔍 buscar (v0.5.7) | 🔍 buscar | UI de mapas dinâmicos com tracking de quests | Alta | 🔵 Avaliar |
| IdleSprintFix | 🔍 buscar (v1.2.2) | 🔍 buscar | Fix do bug de sprint travado | Média | 🔵 Avaliar |

## Dependências

Mods que outros mods precisam para funcionar (ex: SAIN depende de BigBrain e Waypoints).

| Mod | Link 3.x | Link 4.x | Quem depende | Status |
|---|---|---|---|---|
| SPT-BigBrain | 🔍 buscar (v1.3.2) | 🔍 buscar | SAIN (sistema de combat layers) | 🔵 Avaliar |
| SPT-Waypoints | 🔍 buscar (v1.7.1) | 🔍 buscar | SAIN (waypoints de patrulha) | 🔵 Avaliar |

## Status disponíveis

- 🔵 **Avaliar** — ainda não decidido
- 🟢 **Portar** — adaptar código existente do 3.x para 4.0
- 🔧 **Desenvolver** — criar do zero no 4.0 (autor não lançou e não dá pra portar)
- 🟠 **Aguardar upstream** — esperando autor original lançar versão 4.0
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround conhecido
- ⚫ **Não incluir** — fora do escopo do projeto

## Próximos passos

1. Pesquisar links 3.x de cada mod (SPT Hub, GitHub, etc.) e preencher
2. Pesquisar links 4.x — para cada mod sem versão 4.x, decidir entre `🔧 Desenvolver`, `🟠 Aguardar upstream` ou `⚫ Não incluir`
3. Identificar dependências de Assembly-CSharp que mudaram entre 3.x e 4.0
4. Criar specs individuais em `docs/migration/<mod-name>/` para os que serão portados ou desenvolvidos
5. Atualizar este inventário conforme decisões forem tomadas

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-05-02 | Guilherme | +49 / -0 linhas |
| 2026-05-02 | Guilherme | +15 / -7 linhas |
| 2026-05-03 | Guilherme | +32 / -17 linhas |
