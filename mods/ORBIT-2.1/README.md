# ORBIT-2.1

**Versão base:** 2.1.0 · **Licença:** MIT
**Upstream:** https://github.com/Chazut/ORBIT @ `8fd7e661073aa8b46ec8dc00a2b1d62ea7875ea7` (branch `2.1`)
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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/ORBIT-2.1/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph ORBIT-2.1` (ou `bash scripts/update-graphs.sh ORBIT-2.1`).

## Comparar modificações com o original

```bash
diff -r mods/ORBIT-2.1/original/ mods/ORBIT-2.1/modded/
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

_Adicionado em 2026-10-01T01:12:35Z_
