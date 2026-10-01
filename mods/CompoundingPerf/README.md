# CompoundingPerf

**Versão base:** 1.3.0 · **Licença:** MIT
**Upstream:** https://github.com/EchoStarz/CompoundingPerf.git @ `c5a487afde9da8efd95863070b0f3ff99368c572` (branch `(detached)`)
**Forge:** 

---

## O que é

(TODO: descrever o mod em 1-2 parágrafos)

## Estrutura desta pasta

| Pasta | Conteúdo |
|---|---|
| `original/` | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| `modded/` | Cópia de trabalho. Modificações vão aqui. |
| `assets/` | Imagens, prints, documentação externa. |
| `backlog/` | Ideias, bugs, próximos passos. |
| `builds/` | Builds geradas para distribuição. |
| `scripts/` | Scripts auxiliares específicos deste mod. |
| `mod.json` | Metadados machine-readable (alimenta o inventário de mods). |

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/CompoundingPerf/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph CompoundingPerf` (ou `bash scripts/update-graphs.sh CompoundingPerf`).

## Comparar modificações com o original

```bash
diff -r mods/CompoundingPerf/original/ mods/CompoundingPerf/modded/
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

_Adicionado em 2026-09-23T00:46:02Z_
