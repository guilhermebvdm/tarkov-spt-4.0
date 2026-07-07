# 017 — Seed `config` a partir de `config-server` · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Specs:** [00-kickoff](./017-seed-config-de-config-server-00-kickoff.md) · [01-spec](./017-seed-config-de-config-server-01-spec.md) · [02-spec-tech](./017-seed-config-de-config-server-02-spec-tech.md)

> Desvio de processo: sessão autônoma; review 03 fundida na 04 pós-código (sem doc `-03`).

## O que foi construído

Nova estratégia de sync **`SeedIfMissingByName`**: para cada `BepInEx/config-server/<rel>` do server, copia para `BepInEx/config/<rel>` do usuário **só se ausente por nome** — nunca deleta, nunca sobrescreve, sem hash/baseline. Reuso total das primitivas do 007 (planner puro, apply atômico, guard de GameRoot, downloader injetável); um kind de ação novo, zero fork.

### Motor — `SPT.Launcher.Base/Sync/`

| Arquivo | Mudança |
|---|---|
| `SyncFolderRule.cs` | `SeedIfMissingByName = 4` + parser `"seed-if-missing"` |
| `SyncPathUtil.cs` | `DeriveSeedTarget(originalPath, normalizedMatchedPrefix)` — tira `-server` do prefixo casado, preserva casing do remainder, `null` sem remainder |
| `SyncAction.cs` | kind `SeedCopy` + prop `SeedTargetRelative` (destino; `RelativePath` = fonte config-server p/ download) |
| `SyncPlan.cs` | `SeedCount` → soma em `IoActionCount` |
| `SyncResult.cs` | `Seeded` → `Summary` (`· N semeados`) |
| `SyncReport.cs` | `seeded` nos `counts` |
| `SyncRuleResolver.cs` | fallback embutido: `config-server` + `BepInEx/config-server` → `seed-if-missing` (não-destrutivo ⇒ seguro no default, ao contrário do mirror-delete do 007) |
| `SyncPlanner.cs` | seed tratado no loop **antes** do missing/hash (fonte é server-only); `ScanExtras` pula prefixos seed junto com preserve-divergent |
| `SyncEngine.cs` | case `SeedCopy`: valida destino sob GameRoot → re-checa existência (TOCTOU ⇒ `seed-skipped`, sem baixar) → baixa da fonte → `ApplyAtomic` no destino → `Seeded++`, **sem baseline** |

### Client — UI (`SPT.Launcher`)

- `ModUpdateViewModel.cs`: `BuildPlanSummary` mostra `N p/ semear`; `PopulateFileStatuses` mapeia `SeedCopy` → `to-seed` (🌱) exibindo o **destino** (`config/<rel>`); `MapEntryStatus` cobre `seeded`/`seed-skipped`; ícone 🌱.
- `ProfileViewModel.cs`: log do plano inclui `N seeds` (o seed roda automático no mesmo motor — sem outra mudança de fluxo).

### Server — `mods/TarkovRedLine4.0/.../ModUpdater.cs`

- `config.json` default ganha `["BepInEx/config-server"] = "seed-if-missing"` (só afeta instalação nova; servers existentes usam o fallback do client). Scan do `mods_repo` e `/download` já cobrem os paths config-server.

## Reconciliação com o 007 (registrada)

O `config-server → MirrorDelete` do 007 estava **fora do fallback** (CR-01-03) e **nunca** era setado pelo `config.json` default do server ⇒ na prática era `Default` (código opt-in inativo). O 017 o substitui por `SeedIfMissingByName`. `MirrorDelete` continua no enum p/ ativação explícita via `folderRules`.

## Decisões e assunções

1. **A-017.1** — "mesmo nome" = path relativo (subpastas + casing preservados).
2. **A-017.2** — presença só por nome; sem hash/baseline; apply atômico + guard de GameRoot mantidos.
3. **A-017.3** — seed sem memória: não grava baseline p/ o semeado.
4. **A-017.4** — non-destrutivo (nunca deleta/sobrescreve) ⇒ seguro como fallback default.
5. **A-017.5** — contrato de operação: defaults de seed em `mods_repo/BepInEx/config-server/`.
6. **Cleanup do GameStarter** não toca `BepInEx/config` (só top-level fixos) ⇒ o semeado está a salvo por construção.
7. **TOCTOU** — engine re-checa existência no apply e pula (`seed-skipped`) sem baixar/erro.
8. **Bordas documentadas** — prefixo seed sem `-server` semeia p/ si mesmo (degenerado); mesmo `<rel>` em `config` e `config-server` é setup patológico (não duplicar defaults).

## Testes — `SPT.Launcher.Tests/Sync/`

`SyncSeedTests.cs` (11): `DeriveSeedTarget_maps_source_to_config_preserving_casing` (3 casos) · `DeriveSeedTarget_returns_null_when_no_file_remainder` · `Seed_copies_when_target_is_absent` · `Seed_does_not_overwrite_existing_target_even_with_different_content` · `Seed_preserves_subfolders_by_relative_path` · `Seed_copies_only_the_absent_sibling_in_a_subfolder` · `Server_without_config_server_is_a_noop_for_seeding` · `Deleted_seed_reappears_on_next_seed_memoryless` · `Seeded_file_survives_a_managed_path_mirror_sweep` · `Seed_skips_at_apply_when_target_appeared_after_planning` · `Seed_does_not_write_a_baseline_entry_for_the_seeded_file`. `SyncRuleResolverTests.cs`: 2 casos atualizados p/ o novo default.

## Gates

```
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s) (169 warnings pré-existentes)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 69/69, 0 falhas (55 + 14 do 017)
dotnet build TarkovRedLine.Server.csproj -c Release    → 0 Erro(s) (warnings pré-existentes)
```

## Pendências

- **P-017.1 — E2E contra o server real** (gate humano; memória do repo: escrita em SPT exige validação no jogo): popular `mods_repo/BepInEx/config-server/` com defaults reais, rodar "Verificar arquivos" no `D:\SPT`, confirmar (a) arquivo ausente é criado em `BepInEx/config/`, (b) arquivo já existente com conteúdo diferente **não** é tocado, (c) subpasta preservada, (d) apagar um semeado e re-verificar recria, (e) `last-update.json` mostra `seeded`.
- **P-017.2 — coop/Fika:** o seed é por-máquina (cada cliente semeia seu `BepInEx/config`). Sem gap de coop-sync aparente (config é local do cliente), mas validar que os defaults distribuídos são coerentes entre host e peers.
