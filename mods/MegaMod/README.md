# MegaMod

**Versão base:** 4.0.1 · **Licença:** Unknown
**Upstream:** https://github.com/CWXDEV/MegaMod.git @ `f768c4771fef00265929c9ebaf8c52d4d870a5e5` (branch `(detached)`)
**Forge:** 

---

## O que é

MegaMod (desenvolvido por CWX) é uma suíte multifuncional de utilitários e patches client-side (BepInEx) para o SPT Tarkov. Entre suas funcionalidades mais relevantes para estudo de renderização e física de vegetação estão o **GrassCutter** (que manipula e desativa a grama do mapa diretamente via `GPUInstancerDetailManager`) e o **BushWhacker** (que identifica e controla os colisores de arbustos e desaceleração do tipo `ObstacleCollider`).

Possui também opções de QoL para alarme da Reserve, chave mestra, visualização de inventário e ferramentas de depuração gráfica.

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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/MegaMod/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph MegaMod` (ou `bash scripts/update-graphs.sh MegaMod`).

## Comparar modificações com o original

```bash
diff -r mods/MegaMod/original/ mods/MegaMod/modded/
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

_Adicionado em 2026-09-16T00:22:44Z_
