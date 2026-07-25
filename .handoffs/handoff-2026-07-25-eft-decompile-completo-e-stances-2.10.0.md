# Handoff — decompile EFT completo (cercado no harness) + mod stances 2.8.1→2.10.0

> **Data:** 2026-07-25 · **Autor:** sessão Claude (chat longo, continuação da "Sessão 11" do mod stances)
> **Branch:** `main` · **Repo:** `C:/Repos/spt/tarkov-spt-4.0`

## ⚡ Ação mais importante primeiro

Três coisas ficaram abertas, em ordem de prioridade:

1. **Gravar o `/update-memory` do mod stances** — estava no meio: o **plano foi apresentado e NÃO confirmado**
   (o usuário pediu este handoff antes de responder). Retomar com `/update-memory stancesAndCameraPositionSPT4.0.11`.
   O plano proposto (Sessão 11 cont. 6) está resumido em §"Mod stances" abaixo — inclui marcar **[P-11.2] (braço
   G36) como ✅ resolvida** (o usuário respondeu "já foi resolvido", sem detalhe de causa) e encerrar o **F2**.

2. **Reconciliar a divergência de git** (detalhe em §"Estado do git"). Não há perda de trabalho — tudo está no
   `main` atual e em disco — mas existe um commit paralelo `298adf6a` ("docs: add FIKA desync prevention plan and
   ignore eft-decompiled") que **diverge** do HEAD e mexe no `.gitignore` do `eft-decompiled`. Merge precisa de olho
   humano (pode conflitar com as 3 regras de gitignore que adicionei).

3. **Gates humanos in-game** (nada bloqueia código, só validação) — ver §"Pendências in-game".

## Frente 1 — Decompile completo do EFT, cercado no harness (CONCLUÍDO)

**O quê:** o dump `references/eft-decompiled/` estava parcial (102 namespaces vazios porque `ilspycmd -p` aborta no
1º método indecompilável). Substituído por um harness C# que itera tipo a tipo com `try/catch`. Plano completo
(4 revisões + verificação empírica): **`C:/Users/guime/.claude/plans/toasty-watching-wombat.md`**.

**Resultado:** 8.683 tipos · 0 namespaces vazios (era 102) · 8 stubs `// DECOMPILE-ERROR` (0,09%) · grafo 111.732
nós. `ProceduralWeaponAnimation`, `ActiveHealthController`, `GClass2348` (= `EFT.LocalizationExtensions`) agora
existem no dump e no grafo. Aliases SPT 4.1 injetados (4.763 tipos).

**Commits:** `8beefae0` (dump+índice+aliases) · `5fcb0311` (docs+hook+manifest) · `61c820a8` (review 1) ·
`eb1a604f` (review 2). Todos no `main` atual.

**Artefatos-chave (todos versionados; o dump `.cs` e o grafo são gitignored/regeneráveis):**
- Harness: `.agents/tools/decompile-eft/` (Program.cs + csproj, NuGet `ICSharpCode.Decompiler` 10.1.0.8386 fixado)
- Runner: `scripts/decompile-eft.sh` (gera em temp → valida → substitui com rollback; `--dry-run` disponível)
- Índice versionado: `references/eft-decompiled/types-index.json` (582 KB, todos os FQNs + status + alias41)
- Provenance: `references/eft-decompiled/.provenance.json`
- Hook ativo: `.agents/hooks/remind-use-graph.sh` (PreToolUse Bash, registrado em `.claude/settings.json`)
- Docs da regra: `references/eft-decompiled/README.md`, AP-09 em `docs/technical/spt-antipatterns.md`, `AGENTS.md`
  (§Observações + setup), skill `graph-code-navigation`.

**A regra que ficou cercada** (para próximas sessões): decompilado/grafo é a 1ª parada; existência confere-se no
`types-index.json` (nunca num grep vazio — o dump é gitignored); `ilspycmd -t` só se o tipo for stub, fora do
índice, ou o dump não estiver na máquina. Regenerar: `bash scripts/decompile-eft.sh` (NUNCA `ilspycmd -p`). O hook
lembra isso automaticamente e tem escape `# allow-ilspy`.

**PENDENTE (decisão do usuário):** hospedar o zip do dump num **host privado** (é IP da BSG; público viola o README
do repo). O `manifest.json` já tem a estrutura com `download: null` — falta só URL + sha256. Enquanto isso, o dump
é regenerável localmente (quem tem o jogo) ou via Syncthing entre as máquinas do usuário. ⚠️ O **notebook** do
usuário (sem o jogo) perde os `.cs` no próximo pull — cobrir via Syncthing ou host.

## Frente 2 — Mod stances 2.8.1 → 2.10.0 (CONCLUÍDO, in-game pendente)

Continuação direta da Sessão 11 (cont. 5, que parou em 2.7.1/2.8.0). Tudo commitado, tudo deployado em
`D:/SPT/BepInEx/plugins/RealisticMobility/` (EFT estava fechado). Memória do mod: `mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md`.

- **v2.8.1** (`47d30935`) — code-review da 2.7.1 (pedido pelo usuário): colisão de Order na Stance 2 (waypoint
  relativo 17/16 colidia com Forward/Backward e Up/Down). Doc: `backlog/017-.../017-...-04-code-review-v271.md`.
- **v2.8.2** (`316b6581`) — ADS Waypoint movido pro **rodapé** de cada stance (Order −1/−2), a pedido do usuário.
- **v2.9.0** (`8c9ae609`) — **30 configs calibradas do servidor promovidas a default de código**; 2 exceções NÃO
  promovidas (`Debug Transition Metrics`=false, `Mouse Wheel Modifier`=LeftAlt). Tuple `_stanceDefaults` ganhou
  campo `AdsWaypoint`. ⚠️ Defaults só afetam install limpo / chave ausente — `.cfg` existente mantém os valores.
- **Item 019 → v2.10.0** (`ed9cf500`) — **chamber-check ammo UI**: ao checar a câmara, mostra o painel nativo com a
  bala e o tipo (reutiliza `Player.OnShowAmmoDetails`). Gate GO via ilspycmd, `Patches/ChamberCheckAmmoPatch.cs`,
  code-review adversarial (0 🔴). Docs em `backlog/019-checar-camara-ui/`.
- **Item 018** (`8c9ae609`) — backlog "rastejar rápido / high-crawl", só ideia (`backlog/018-rastejar-rapido/`).

## Pendências in-game (gates humanos — nada bloqueia código)

- **[P-11.6] item 019** — testar in-game. **PRIORIDADE:** o "Empty" com câmara vazia (via Manual Chambering do
  item 010) — é a única suposição load-bearing (que `CheckChamber()` retorna `true` com câmara vazia). Precisa da
  tecla "Check Chamber" bindada nos controles do EFT (não vem por default). Depois: carregada, toggle F12, Fika.
- **[P-11.5] F1+F3** — calibrar `ADS Waypoint Time` por stance + `Compression`/`Pivot`; testar troca de arma no
  ADS-in, scope, Fika.
- **[P-11.1]** — velocidade presa devagar ao levantar do agachado (cap stale). Não investigado a fundo.
- Subir 2.8–2.10 ao **servidor** via `config-server` do launcher (DLL + `.cfg`).
- **[P-11.2] braço G36 → resolver como ✅** na memória (usuário disse "já foi resolvido").

## Estado do git (ATENÇÃO)

Várias sessões paralelas commitando. Situação atual verificada:
- HEAD = `main` = `4e636a6f` (commits de outra sessão: rnnoise/noise-suppression/gitmodules no topo).
- **Meu trabalho ESTÁ presente** no HEAD atual e em disco (confirmado: `types-index.json`, harness, hook, scripts,
  e o dump `.cs`).
- Existe `298adf6a` ("docs: add FIKA desync prevention plan and ignore eft-decompiled") que **contém** meu trabalho
  mas **diverge** de `4e636a6f` (`git merge-base --is-ancestor 4e636a6f 298adf6a` = NÃO).
- ⚠️ `298adf6a` mexe em "ignore eft-decompiled" — pode conflitar com as 3 regras que adicionei ao `.gitignore`
  (`references/eft-decompiled/Assembly-CSharp/`, `references/graphs/eft-decompiled/`, `/.decompile-dryrun/`).
  Ao reconciliar, garantir que as 3 sobrevivam.
- **Não commitar/push sem revisar** — trabalho de múltiplas sessões no ar; usar commit cirúrgico (regra do
  CLAUDE.md §4).

## Suggested skills (próxima sessão)

- `/update-memory stancesAndCameraPositionSPT4.0.11` — **primeiro**, para gravar a Sessão 11 cont. 6 (plano pronto).
- `memory-curation` — carregada automaticamente pelo update-memory.
- `graph-code-navigation` — agora que o grafo do EFT está completo, é a via primária (query_graph/get_node no MCP
  `graphify-eft`) antes de qualquer descompilação.
- `/g-review-content` — se for mexer no plano do decompile ou revisar a Fase 2 (host).

## Contexto de memória

`Memória consultada: mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md — snapshot Sessão 11 (cont. 5, v2.7.1/2.8.0). Snapshot do topo está DESATUALIZADO (ainda diz v2.5.0) — o /update-memory pendente corrige. Pendências que afetam: [P-11.5], [P-11.1] abertas; [P-11.2] a marcar resolvida.`
