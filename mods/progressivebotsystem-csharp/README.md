# progressivebotsystem-csharp

**Versão base:** 2.0.0 · **Licença:** see LICENSE
**Upstream:** https://github.com/acidphantasm/progressivebotsystem-csharp @ `5b0e99e8d0c0235afb1eb679162e59027640fe9a` (branch `master`)
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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/progressivebotsystem-csharp/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph progressivebotsystem-csharp` (ou `bash scripts/update-graphs.sh progressivebotsystem-csharp`).

## Comparar modificações com o original

```bash
diff -r mods/progressivebotsystem-csharp/original/ mods/progressivebotsystem-csharp/modded/
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

_Adicionado em 2026-08-23T23:29:52Z_

## Mapa de código

Grafo AST do mod em [references/graphs/mods/progressivebotsystem-csharp/GRAPH_REPORT.md](../../references/graphs/mods/progressivebotsystem-csharp/GRAPH_REPORT.md). Regenerar: `bash scripts/update-graphs.sh progressivebotsystem-csharp`.

## Por que este mod está vendorizado aqui

Referência de leitura para o item [TRL-DynamicSpawn 011](../TRL-DynamicSpawn/backlog/011-perf-estoque-dificuldade/): auditoria confirmou (2026-08-23, tag 2.2.1) que o equipamento dos bots é selecionado por **Tier (nível do jogador) + papel**, e que a **dificuldade não influencia equipamento** (3 ocorrências de "difficulty" no código, todas em logging). Sem PROPRIEDADES.md: é server mod C# (sem `Config.Bind`/F12).
