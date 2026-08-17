# LoadAmmoAnim

Mod que adiciona animações em primeira pessoa e sincronização coop de carregamento de munição em magazines e armas.

## Estrutura

- `original/` — Código-fonte original intocado clonado do repositório upstream (tag `v1.6.0`, commit `5dc153a`).
- `modded/` — Código com modificações, correções e otimizações arquiteturais.
- `assets/` — Recursos, bundles (`stanags_container.bundle`) e release oficial.
- `backlog/` — Rastreamento de tarefas e correções.
- `builds/` — Binários compilados.

## Projetos

- `LoadAmmoAnimClient` (`LoadAmmoAnimClient.csproj`) — Lógica client BepInEx principal e patches de animação.
- `LoadAmmoAnimClientFika` (`LoadAmmoAnimClientFika.csproj`) — Módulo de compatibilidade e sincronização multiplayer para FIKA.
- `LoadAmmoAnimServer` (`LoadAmmoAnimServer.csproj`) — Mod de servidor C# (SPT 4.0) responsável por registrar o item virtual customizado e injetar os bundles via `WTTServerCommonLib`.
