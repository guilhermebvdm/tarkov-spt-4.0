# TRL-WeatherSync

**Versão base:** 1.0.0 · **Licença:** MIT  
**SPT Version:** 4.0.13 · **EFT Version:** 0.16.9  
**Status:** 🔵 Em planejamento (Roadmap)

---

## O que é

O **TRL-WeatherSync** é um mod de ecossistema atmosférico projetado para o **SPT 4.0** e **FIKA (Coop)** com duas missões fundamentais:

1. **Sincronização Contínua de Clima em Raid:** Eliminar definitivamente o desync de clima multiplayer entre Host e Clientes no FIKA, transmitindo e sincronizando nuvens, chuva, vento, neblina, neve, trovões e variações dinâmicas em tempo real durante a partida.
2. **Gerenciador do Ciclo Natural de Estações:** Controlar e automatizar a progressão natural e contínua das estações do ano (Primavera $\rightarrow$ Verão $\rightarrow$ Outono $\rightarrow$ Inverno) no Tarkov, com efeitos sazonais autênticos (folhagem, acúmulo de neve, temperatura e iluminação).

---

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| `ROADMAP.md` | Detalhamento arquitetural completo das funcionalidades planejadas e pipeline de rede. |
| `original/` | Base de referência intocada. |
| `modded/` | Código de desenvolvimento ativo do mod. |
| `assets/` | Texturas, ícones, presets climáticos e documentação visual. |
| `backlog/` | Tarefas e itens de trabalho versionados do mod. |
| `builds/` | Binários compilados (`.dll`) isolados para testes e distribuição. |
| `docs/` | Especificações técnicas, diagramas de rede e relatórios. |
| `memory/` | Registro cronológico das sessões e decisões de engenharia (`sessions.md`). |
| `scripts/` | Scripts de build, automação e sincronização. |
| `mod.json` | Metadados do mod para o inventário do repositório. |

---

## Workflow de desenvolvimento

Ciclo completo de backlog, especificações, revisões, código e grafos: ver [WORKFLOW.md](../../WORKFLOW.md).
