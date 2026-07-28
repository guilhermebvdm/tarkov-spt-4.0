# TRL-DynamicSpawn (v3.1.2)

Sistema de geração dinâmica de bots, limites por mapa, despespawn inteligente (culling por linha de visão/distância) e overlay em tempo real com o SPT-DynamicMaps.

## Módulos do Mod
- `Client/` — Mod de cliente BepInEx C# (limites de bots, despespawn de bots fora de combate, culling LoS e overlays de mapa).
- `Server/` — Mod de servidor TypeScript/JavaScript SPT 4.0 para injeção e manipulação do sistema de ondas e profiles.
- `config/` — Arquivos de configuração JSON do servidor.

## Governança
- `PROPRIEDADES.md` — Tabela completa de configurações F12 (Caps de bots, LoS, Smooth Spawning, Despawn Bubble, Overlay).
- `backlog/` — Tarefas, specs e documentações de desativação de ondas nativas.
- `memory/sessions.md` — Registros de sessão e lições aprendidas do mod.
