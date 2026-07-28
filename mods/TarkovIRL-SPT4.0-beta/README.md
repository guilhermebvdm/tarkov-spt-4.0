# TarkovIRL (SPT 4.0 Beta)

Mod principal de retrabalho na física de armas, movimentação de corpo/mão, desacoplamento de mira (Free Aim / Deadzone), paralaxe e inércia do personagem em Escape From Tarkov.

## Recursos Principais
- **Weapon Deadzone & Free Aim:** Área morta configurável e mira desacoplada da câmera.
- **Weapon Sway & Parallax:** Balanço dinâmico e desalinhamento físico de miras baseado no peso da arma, ergonomia e velocidade de rotação do jogador.
- **Efficiency System:** Indicador visual de eficiência baseado em peso, fadiga e ferimentos corporais.
- **Footstep & Stance Dynamics:** Física de passos, troca de postura e transição de pescoço/cabeça.

## Estrutura do Mod
- `PrimeMover.cs` — Ponto de entrada BepInEx, inicializador de patches e vinculação do menu F12.
- `SwayController.cs`, `ParallaxController.cs`, `FreeAimController.cs`, `FootstepController.cs` — Controllers das mecânicas visuais/físicas.
- `PROPRIEDADES.md` — Mapeamento detalhado de todas as opções de configuração F12.
- `backlog/` — Especificações funcionais, técnicas e revisões de código.
- `docs/` — Documentação técnica.
- `memory/sessions.md` — Histórico de sessões, refatorações e pendências.
