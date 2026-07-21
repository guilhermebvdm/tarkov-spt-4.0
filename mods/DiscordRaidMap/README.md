# DiscordRaidMap

**Versão base:** unknown · **Licença:** MIT
**Upstream:** https://github.com/Fiodorwellfme/DiscordRaidMap @ `8298e85b84defaabdbb61962c9bd874a73f38ba1` (branch `main`)
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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/DiscordRaidMap/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph DiscordRaidMap` (ou `bash scripts/update-graphs.sh DiscordRaidMap`).

## Comparar modificações com o original

```bash
diff -r mods/DiscordRaidMap/original/ mods/DiscordRaidMap/modded/
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

_Adicionado em 2026-07-21T01:38:02Z_
