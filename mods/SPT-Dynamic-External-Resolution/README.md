# SPT-Dynamic-External-Resolution

**Versão base:** 4.0.0 · **Licença:** MIT
**Upstream:** https://github.com/Shibatsui/SPT-Dynamic-External-Resolution.git @ `f8a2bc7df909ecc94955eb22bff337539f129512` (branch `main`)
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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/SPT-Dynamic-External-Resolution/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph SPT-Dynamic-External-Resolution` (ou `bash scripts/update-graphs.sh SPT-Dynamic-External-Resolution`).

## Comparar modificações com o original

```bash
diff -r mods/SPT-Dynamic-External-Resolution/original/ mods/SPT-Dynamic-External-Resolution/modded/
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

_Adicionado em 2026-09-15T04:59:44Z_
