# SPT-ContinuousLoadAmmo

**Versão base:** 1.1.6 · **Licença:** MIT
**Upstream:** https://github.com/ozen-m/SPT-ContinuousLoadAmmo @ `81712fd9398540a69afab8418d85b1153a1ba462` (branch `main`)
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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/SPT-ContinuousLoadAmmo/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph SPT-ContinuousLoadAmmo` (ou `bash scripts/update-graphs.sh SPT-ContinuousLoadAmmo`).

## Comparar modificações com o original

```bash
diff -r mods/SPT-ContinuousLoadAmmo/original/ mods/SPT-ContinuousLoadAmmo/modded/
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

_Adicionado em 2026-08-16T06:14:16Z_
