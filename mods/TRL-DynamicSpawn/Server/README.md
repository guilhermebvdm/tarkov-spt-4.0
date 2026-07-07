# TRL-DynamicSpawn — Server

Componente de servidor do **TRL-DynamicSpawn**, o mod de spawn dinâmico do TRL. O servidor hospeda o **painel de controle web** (Razor Pages, nativo no SPT) que define presets, timers, dificuldade e elites por mapa; o cliente baixa essas configurações no carregamento da raid e executa a lógica de spawn ao vivo (avaliação do mapa a cada 6 min, sem filas nem spawn instantâneo).

- **Arquitetura e regras de spawn (fonte de verdade):** [../TRL_DYNAMIC_SPAWN_DOCS.md](../TRL_DYNAMIC_SPAWN_DOCS.md)
- **Cliente (BepInEx/C#):** [../Client/](../Client/)
- **Ciclo de desenvolvimento:** [WORKFLOW.md](../../../WORKFLOW.md)

Config em `config/`; painel em `Web/` + `wwwroot/`. Build/instalação via `/compile-mod`.
