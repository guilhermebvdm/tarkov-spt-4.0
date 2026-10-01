# TRL-CoreSight — Advanced Performance & Vision Culling Suite

**Versão base:** 0.1.0 · **Licença:** MIT  
**Target SPT:** 4.0.13 (EFT 0.16.9)  
**Compatibilidade:** Singleplayer & Coop (FIKA)

---

## O que é

O **TRL-CoreSight** é uma suíte de otimização de alto desempenho focada em dois pilares fundamentais do Escape from Tarkov:

1. **GPU & Visão (Sight)**:
   - **Dynamic Interior Shadow Culling**: Redução cirúrgica do alcance de sombras ao entrar em quartos ou ambientes fechados, impedindo que a Unity gaste passadas de cascata desenhando geometria externa invisível.
   - **LOD Bias Dinâmico**: Redução suave do LOD de objetos distantes quando o jogador está em movimento ou em áreas de curta distância, elevando o nível de detalhe dinamicamente apenas ao mirar (ADS) ou usar ópticas.
   - **Frustum & Portal Culling Assist**: Otimização do descarte de pequenos objetos/detritos que não agregam valor tático através de portas e janelas.

2. **CPU & Lógica (Core)**:
   - **Occluded Bot Animation LOD**: Redução da taxa de atualização de animações do esqueleto (`Animator`) de bots e Scavs que estão ocluídos atrás de paredes sólidas ou fora do cone de visão do jogador, aliviando dezenas de chamadas de transformação por frame.
   - **Preservação Física e Tática 100%**: As hitboxes ([`BodyPartCollider`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/BodyPartCollider.cs)), navegação NavMesh ([`BotMover`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/BotMover.cs)) e comportamento da IA permanecem ativos e perfeitamente sincronizados no servidor/FIKA.
   - **Zero Memory Leaks**: Limpeza preventiva e determinística de caches estáticos e referências ao final de cada raid.

---

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| `modded/` | Código-fonte C# BepInEx do mod client. |
| `backlog/` | Especificações funcionais (`01-spec.md`), técnicas (`02-spec-tech.md`) e reviews. |
| `memory/` | Memória de sessões, lições e decisões de arquitetura (`sessions.md`). |
| `builds/` | Binários compilados (`TRL-CoreSight.dll`). |
| `assets/` | Diagramas, capturas e material visual. |
| `scripts/` | Scripts auxiliares específicos do mod. |
| `docs/` | Documentos técnicos e diagnósticos de arquitetura ([Diagnóstico de CPU e Motor](docs/diagnostico-desempenho-cpu-e-motor.md)). |
| `PROPRIEDADES.md` | Catálogo de configurações do menu F12 (BepInEx). |
| `mod.json` | Metadados do mod para o workspace. |

---

## Workflow de Desenvolvimento

Este mod segue rigorosamente as convenções estabelecidas em [WORKFLOW.md](../../WORKFLOW.md) e [.agents/conventions.md](../../.agents/conventions.md):
1. Especificações antes de código (`01-spec` → `02-spec-tech` → `03-review` → `05-asbuild`).
2. Isolamento de build em `mods/TRL-CoreSight/builds/` (sem cópia para a pasta do jogo durante o desenvolvimento).
3. Bump obrigatório de versão SemVer a cada compilação.
