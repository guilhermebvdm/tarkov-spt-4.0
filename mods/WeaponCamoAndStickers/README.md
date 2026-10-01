# WeaponCamoAndStickers

**Versão base:** 1.17.1 · **Licença:** MIT
**Upstream:** https://github.com/7Bpencil/SPT.WeaponCamoAndStickers.git @ `c839392373f689c3767eb6086e3a0b219937a15f` (branch `(detached)`, tag `1.17.1`)
**Forge:** 

---

## O que é

Client mod BepInEx (GUID `7Bpencil.WeaponCamoAndStickers`, soft-dependency de `com.fika.core`) que adiciona camuflagens/decalques (camos, stickers, máscaras) customizáveis em armas, com um editor visual in-game ("Camo Editor") para posicionar/rotacionar/escalar texturas sobre o modelo 3D da arma. Suporta presets salvos, spawn automático de camuflagem em bots (Goons/PMC/outros chefes/scavs, com chance configurável por categoria) e integração opcional com FIKA (carrega um assembly `.Fika.dll` separado se o FIKA estiver presente). Ver `mods/WeaponCamoAndStickers/PROPRIEDADES.md` para as opções do F12.

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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/WeaponCamoAndStickers/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph WeaponCamoAndStickers` (ou `bash scripts/update-graphs.sh WeaponCamoAndStickers`).

## Comparar modificações com o original

```bash
diff -r mods/WeaponCamoAndStickers/original/ mods/WeaponCamoAndStickers/modded/
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

_Adicionado em 2026-09-07T02:08:27Z_
