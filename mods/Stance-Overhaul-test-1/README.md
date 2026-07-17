# Stance-Overhaul-test-1

**Versão base:** 1.0.0 · **Licença:** Unknown (sem arquivo de licenca no pacote)
**Upstream:** manual-delivery (sem git — recebido como zip) @ `unknown (entrega manual, sem historico git)` (branch `(none)`)
**Forge:** 

---

## O que é

**Fontaine-StanceOverhaul v1.0.0** (`com.fontaine.stanceoverhaul`) — build de **teste** do sistema de posturas do
Fontaine (autor do Realism / FOV Fix), recebida **manualmente como zip** (sem histórico git; a pasta `original/`
preserva o pacote como chegou). É o sistema de stances extraído/reconstruído como mod standalone: **6 posturas**
(Active Aim, High Ready, Low Ready, Short-Stocking, Patrol, Left Shoulder) com matriz completa de transições e
blends entre elas, tac sprint, mounting/bracing próprio, melee com a arma, efeitos de stamina/velocidade por
postura e bônus de precisão de quadril por dispositivo (lasers/lanternas/NVG).

⚠️ **Dependência obrigatória:** `RealismCommonLib` (`[BepInDependency]` no `Plugin.cs`) — o mod não carrega sem
ela. O `.csproj` também referencia assemblies do EFT/SPT; ver seção Build.

Interesse para o TRL: é o "concorrente direto" do nosso
[`stancesAndCameraPositionSPT4.0.11`](../stancesAndCameraPositionSPT4.0.11/) — vale estudar a matriz de
transições por par de posturas (nosso mod usa uma mola única com 2 velocidades) e o handling de colisão/mounting.

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

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/Stance-Overhaul-test-1/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph Stance-Overhaul-test-1` (ou `bash scripts/update-graphs.sh Stance-Overhaul-test-1`).

## Comparar modificações com o original

```bash
diff -r mods/Stance-Overhaul-test-1/original/ mods/Stance-Overhaul-test-1/modded/
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

_Adicionado em 2026-07-17T01:37:44Z_
