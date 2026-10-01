# TRL-WeatherSync

**Versão base:** 1.0.0 · **Licença:** MIT  
**SPT Version:** 4.0.13 · **EFT Version:** 0.16.9  
**Status:** 🟢 Em desenvolvimento — item 001 (v1.1.1) e item 002 (v1.0.0) implementados e compilados; falta validação in-game dos dois

---

## O que é

O **TRL-WeatherSync** é um mod de ecossistema atmosférico projetado para o **SPT 4.0** e **FIKA (Coop)** com duas missões fundamentais:

1. **Sincronização Contínua de Clima em Raid:** Eliminar definitivamente o desync de clima multiplayer entre Host e Clientes no FIKA, transmitindo e sincronizando nuvens, chuva, vento, neblina, neve, trovões e variações dinâmicas em tempo real durante a partida.
2. **Gerenciador do Ciclo Natural de Estações:** Controlar e automatizar a progressão natural e contínua das estações do ano (Primavera $\rightarrow$ Verão $\rightarrow$ Outono $\rightarrow$ Inverno) no Tarkov, com efeitos sazonais autênticos (folhagem, acúmulo de neve, temperatura e iluminação).

---

## ⚠️ Requisito de instalação

**Este mod precisa estar instalado em TODOS os participantes da raid** (Host e Clientes) — não é opcional por jogador. Se um jogador entrar numa raid FIKA sem o TRL-WeatherSync instalado enquanto os demais o têm, ele pode sofrer erros de rede que afetam o movimento de todos os jogadores da partida (não só o clima). Confirme que todo o grupo instalou o mod antes de iniciar uma raid coop — inclusive num servidor `Fika-Headless`.

**Lado servidor (item 002 — Gerenciador Ciclo Natural Estações):** todas as instalações FIKA da mesma raid precisam ter o **mesmo** `modded/Server/Config/season-cycle.json` — cada peer roda seu próprio SPT Server local, então a estação/pesos de clima só ficam consistentes entre jogadores se o arquivo for idêntico em todas as máquinas. Depois de editar `season-cycle.json`, **reinicie o SPT Server** — o arquivo é lido só uma vez, no boot do servidor, não recarrega sozinho.

---

## Estrutura desta pasta

| Pasta / Arquivo | Conteúdo |
|---|---|
| `ROADMAP.md` | Detalhamento arquitetural completo das funcionalidades planejadas e pipeline de rede. |
| `original/` | Base de referência intocada. |
| `modded/Client/` | Plugin BepInEx (item 001 — sincronização de clima em raid). Instala em `BepInEx/plugins/`. |
| `modded/Server/` | Mod de servidor C# (item 002 — ciclo de estações). Instala em `SPT/user/mods/`; hoje via build manual (`dotnet build`), sem `/compile-mod` (ver §7 da spec técnica do item 002). |
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
