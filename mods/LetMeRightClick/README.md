# LetMeRightClick

**Versão base:** 1.0.0 · **Licença:** CC-BY-NC-ND-4.0 ⚠️
**Upstream:** https://github.com/Lacyway/LetMeRightClick.git @ `51e375822c04335ca1cfeb7b48cd91a08f0f7715` (branch `(detached)`, tag `v1.0.0`)
**Forge:** 

> ⚠️ **Licença NoDerivatives.** O upstream é distribuído sob Creative Commons BY-NC-ND 4.0 — proíbe explicitamente redistribuir versões modificadas. `modded/` pode ser usado para referência/teste local, mas **qualquer distribuição pública de uma versão alterada deste mod viola a licença do autor.** Confirmar com o autor (Lacyway) antes de publicar qualquer coisa derivada.

---

## O que é

Client mod BepInEx (GUID `com.lacyway.lmrc`) que adiciona um menu de contexto (botão direito) em `ItemUiContext.GetItemContextInteractions` — patch único `ItemUiContext_GetItemContextInteractions_Patch`. Objetivo: permitir interações de botão direito em situações que o jogo vanilla não expõe (ex.: slots/itens específicos).

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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/LetMeRightClick/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph LetMeRightClick` (ou `bash scripts/update-graphs.sh LetMeRightClick`).

## Comparar modificações com o original

```bash
diff -r mods/LetMeRightClick/original/ mods/LetMeRightClick/modded/
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

_Adicionado em 2026-09-07T02:07:34Z_
