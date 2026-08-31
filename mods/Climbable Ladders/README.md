# Climbable Ladders

**Versão base:** 1.0.2 · **Licença:** Unknown
**Upstream:** https://github.com/bmpq/spt-ladders @ `d08f4bc925995a65c49d46d60efecca1a6ab7d94` (branch `main`)
**Forge:** 

---

## O que é

O **Climbable Ladders** é um mod client-side (C# / BepInEx) e cooperativo (Fika) para Escape From Tarkov / SPT 4.0 que transforma as escadas estáticas do cenário em elementos totalmente funcionais e escaláveis. 

O mod implementa um sistema completo de movimentação vertical, animação procedural via Cinemática Inversa (FinalIK), rig dinâmico de dedos ([ProceduralGrip](./modded/ladders.bep/ProceduralGrip.cs)), dreno de estamina modulado pelo peso do inventário, penalidades de dano por membros fraturados, integração suave com a mecânica de *Vaulting* da BSG no topo da subida e suporte completo à replicação multiplayer no **Fika Core**.

## Documentação Técnica

A documentação modular detalhada de todos os subsistemas encontra-se em:
👉 **[Documentação Técnica Modular (docs/README.md)](./docs/README.md)**

## Estrutura desta pasta

| Pasta | Conteúdo |
|---|---|
| `docs/` | **Documentação técnica e funcional completa**, artigos modulares, diagramas conceituais e guias de subsistemas. |
| `original/` | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| `modded/` | Cópia de trabalho contendo o código C#, prefabs e cenas Unity do mod. |
| `assets/` | Imagens, prints e documentação externa. |
| `backlog/` | Ideias, bugs e próximos passos de desenvolvimento. |
| `builds/` | Builds geradas para distribuição. |
| `scripts/` | Scripts auxiliares específicos deste mod. |
| `mod.json` | Metadados machine-readable (alimenta o inventário de mods). |

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/Climbable%20Ladders/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph "Climbable Ladders"` (ou `bash scripts/update-graphs.sh "Climbable Ladders"`).

## Comparar modificações com o original

```bash
diff -r "mods/Climbable Ladders/original/" "mods/Climbable Ladders/modded/"
```

## Atualizar do upstream

Reclonar o repositório oficial e sobrescrever `original/` (sem tocar em `modded/`):

```bash
# TODO: criar /update-mod
```

Após atualizar, o diff acima mostrará suas modificações + drift do upstream.

## Build

(TODO: documentar processo de build — geralmente em `scripts/build.sh` gerando artefato em `builds/`)

---

_Adicionado em 2026-08-16T01:22:08Z_
