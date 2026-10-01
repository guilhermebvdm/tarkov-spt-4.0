# HandsAreNotBusy

**Versão base:** 1.6.0 · **Licença:** MIT (see LICENSE.md)
**Upstream:** https://github.com/Lacyway/HandsAreNotBusy.git @ `4f3616242f1671fa4f42e7881a185a9e50570f6a` (tag `1.6`)
**Forge:** https://github.com/Lacyway/HandsAreNotBusy/releases/tag/1.6

---

## O que é

Mod client BepInEx para Escape From Tarkov / SPT que adiciona uma tecla de atalho configurável (padrão `End`) para forçar o reset e destravamento do controlador de mãos (`HandsController`) quando ocorre o infame bug de *Hands are busy* (mãos travadas).

- **Documentação técnica:** [docs/README.md](docs/README.md)
- **Configurações F12:** [PROPRIEDADES.md](PROPRIEDADES.md)

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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/HandsAreNotBusy/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph HandsAreNotBusy` (ou `bash scripts/update-graphs.sh HandsAreNotBusy`).

## Comparar modificações com o original

```bash
diff -r mods/HandsAreNotBusy/original/ mods/HandsAreNotBusy/modded/
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

_Adicionado em 2026-09-05T02:26:26Z_
