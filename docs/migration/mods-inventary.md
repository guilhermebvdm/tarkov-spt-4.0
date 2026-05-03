---
title: Inventário de mods para migração SPT 3.x → 4.0
date: 2026-05-02
status: 🔵 Em andamento
authors: Guilherme
---

# Inventário de mods — Migração SPT 3.x → 4.0

Catálogo dos mods atualmente em uso no `tarkov-spt-3.0` que precisam ser avaliados para portar/recriar no SPT 4.0.

> **Atenção:** SPT 4.0 tem arquitetura incompatível com 3.x. Este documento serve como ponto de partida para decidir o que migrar, refazer ou descartar.

## Base — UltraFika-Plugin

Mod fundamental do projeto. Habilita multiplayer no SPT e serve como base sobre a qual os demais mods rodam. **Migração prioritária zero** — sem ele, o restante do ecossistema não tem sentido.

| Item | Detalhe |
|---|---|
| **Versão 3.x** | — |
| **Tipo** | Client (C# / BepInEx) |
| **Função** | Cliente multiplayer (Fika) |
| **Prioridade** | 🔥 Crítica — primeiro mod a ser migrado |
| **Status migração** | 🔵 Avaliar |
| **Bloqueia** | Todos os demais mods do projeto dependem desta base estar funcional |

## Mods Client (C# / BepInEx)

Mods que rodam em cima da base do UltraFika-Plugin.

| Mod | Versão 3.x | Função | Prioridade | Status migração |
|---|---|---|---|---|
| SAIN | — | Substituição do sistema de IA dos bots | Alta | 🔵 Avaliar |
| SPT-DynamicMaps | 0.5.7 | UI de mapas dinâmicos com tracking de quests | Alta | 🔵 Avaliar |
| IdleSprintFix | 1.2.2 | Fix do bug de sprint travado | Média | 🔵 Avaliar |

## Dependências

| Mod | Versão 3.x | Quem depende | Status |
|---|---|---|---|
| SPT-BigBrain | 1.3.2 | SAIN (sistema de combat layers) | 🔵 Avaliar |
| SPT-Waypoints | 1.7.1 | SAIN (waypoints de patrulha) | 🔵 Avaliar |

## Status disponíveis

- 🔵 **Avaliar** — ainda não decidido
- 🟢 **Migrar** — vai ser portado/recriado no 4.0
- 🟠 **Aguardar upstream** — esperando autor original lançar versão 4.0
- 🔴 **Bloqueado** — incompatibilidade arquitetural sem workaround
- ⚫ **Descartar** — não vai ser usado no 4.0

## Próximos passos

1. Para cada mod, verificar se autor original já lançou versão para SPT 4.0
2. Identificar dependências de Assembly-CSharp que mudaram entre 3.x e 4.0
3. Criar specs individuais em `docs/migration/<mod-name>/` para os que serão migrados
4. Atualizar este inventário conforme decisões forem tomadas

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-05-02 | Guilherme | +49 / -0 linhas |
| 2026-05-02 | Guilherme | +15 / -7 linhas |
