# TRL-PvpMode

**Versão:** 0.8.0 · **Licença:** MIT · **SPT:** 4.0.x · **EFT:** 0.16.9
**Dependência:** Fika (`com.fika.core`) — dependência dura
**Forge:** _(não publicado)_

---

## O que é

Mod de **regras de modo de jogo** para o servidor Tarkov Red Line. O primeiro modo implementado é
**vidas por raid**: ao morrer, em vez de a partida acabar, o personagem é desligado no lugar onde caiu
(sem poder se mover, tela escurecida) e o jogador escolhe renascer em outro ponto de spawn do mapa,
gastando uma vida — com o equipamento intacto e a nova posição sincronizada para o anfitrião, os
outros jogadores e as IAs.

> **Este mod não é um fork.** É código próprio. Uma tentativa anterior de respawn, que destruía e
> recriava o jogador do zero, está arquivada em `mods/TRL-PvpMode-deprecated/` e **não** serve de base —
> ela existia para permitir cadáver saqueável, requisito que foi descartado.

## Identidade (padrão TRL)

| Onde | Valor |
|---|---|
| GUID (`BepInPlugin`) | `com.trl.pvpmode` |
| Nome do plugin | `TRL-PvpMode` |
| `AssemblyName` | `TRL-PvpMode` |
| Namespace C# | `TarkovRedLine.PvpMode` |
| Pasta em `BepInEx/plugins/` | `TRL-PvpMode/` |
| Arquivo de config (F12) | `com.trl.pvpmode.cfg` |

⚠️ **Trocar o GUID apaga a configuração de todos os usuários** (o BepInEx deriva o nome do `.cfg` dele).
A identidade acima está congelada — ver `.claude/skills/trl-mod-publishing`.

A versão precisa bater em **três** lugares: `Plugin.cs` (`PluginVersion`), `TRL-PvpMode.csproj`
(`Version`/`AssemblyVersion`/`FileVersion`) e `CHANGELOG.md`.

## Estrutura desta pasta

| Pasta | Conteúdo |
|---|---|
| `modded/` | Código-fonte. Não há `original/` — o mod não vem de upstream. |
| `modded/References/` | DLLs de referência para compilar (gitignored; copiar de outro mod TRL em máquina nova). |
| `backlog/` | Itens de trabalho, ideias, bugs. |
| `builds/` | Artefatos gerados para distribuição. |
| `docs/` | Documentação técnica do mod. |
| `memory/` | Memória cronológica das sessões (`sessions.md`). |

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo do código (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/TRL-PvpMode/GRAPH_REPORT.md).
Regenerar após mudanças: `/update-mod-graph TRL-PvpMode`.

## Build

```bash
# a partir da raiz do repositório
/compile-mod TRL-PvpMode
```

A saída vai para `builds/`. Toda compilação incrementa a versão semver — ver
[feedback de versionamento](../../AGENTS.md).
