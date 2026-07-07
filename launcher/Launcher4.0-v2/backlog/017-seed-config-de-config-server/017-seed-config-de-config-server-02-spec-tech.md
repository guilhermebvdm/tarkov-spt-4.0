# 017 — Seed `config` a partir de `config-server` · Spec técnica

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Spec:** [01-spec](./017-seed-config-de-config-server-01-spec.md) · **Dep:** motor de sync do item 007

> Desvio de processo registrado (instrução do coordenador): review 03 fundida na 04 pós-código (não há doc `-03`).

## Reconciliação com o item 007 (obrigatória)

O 007 mapeava `config-server` como **`MirrorDelete`** — porém **fora do fallback embutido** (ref: CR-01-03): só ativava via `folderRules` **explícito** do server, e o `config.json` default gerado pelo `ModUpdater` **nunca** o setava. Ou seja: em produção o `config-server` era, na prática, `Default` — o mirror-delete era código morto/opt-in.

O 017 **substitui** essa semântica: `config-server` passa a ser a estratégia nova **`SeedIfMissingByName`** (config-server → config, cópia só-se-ausente). Duas mudanças em relação ao 007:

1. **Mapeamento de pasta.** Não é mais espelho no lado do usuário: `config-server` (server) é **fonte** que popula `config` (usuário). Source path ≠ destination path.
2. **Critério de existência.** Só por **nome** (existe no disco?), nunca por hash/baseline. O oposto do `PreserveDivergent` do 007 (que compara com baseline).

Como o seed é **não-destrutivo** (nunca deleta/sobrescreve — o oposto do mirror-delete), ele **entra no fallback embutido** do client com segurança: mesmo servers antigos (config.json sem regra de config-server) semeiam os defaults. O `enum SyncFolderRule.MirrorDelete` continua existindo para quem quiser ativá-lo explicitamente via `folderRules`.

## Mudanças (motor — `SPT.Launcher.Base/Sync/`)

| Arquivo | Mudança |
|---|---|
| `SyncFolderRule.cs` | `enum` ganha `SeedIfMissingByName = 4`; parser aceita `"seed-if-missing"` |
| `SyncPathUtil.cs` | `DeriveSeedTarget(originalPath, normalizedMatchedPrefix)` — mapeia `<name>-server/<rel>` → `<name>/<rel>` (tira o sufixo `-server` do prefixo casado; preserva o casing do remainder; `null` sem remainder) |
| `SyncAction.cs` | novo kind `SeedCopy`; nova prop `SeedTargetRelative` (destino de escrita; `RelativePath` fica a **fonte** config-server, usada no download) |
| `SyncPlan.cs` | `SeedCount`; entra no `IoActionCount` |
| `SyncResult.cs` | contador `Seeded`; entra no `Summary` (`· N semeados`) |
| `SyncReport.cs` | `seeded` nos `counts` do last-update.json |
| `SyncRuleResolver.cs` | fallback embutido ganha `config-server` e `BepInEx/config-server` → `seed-if-missing` |
| `SyncPlanner.cs` | trata `SeedIfMissingByName` no loop principal **antes** da lógica missing/hash (a fonte config-server é pasta server-only — o usuário nunca a tem no disco); em `ScanExtras`, prefixos seed são pulados como `PreserveDivergent` (defesa se um managedPath cobrir config-server) |
| `SyncEngine.cs` | case `SeedCopy`: valida destino sob GameRoot → re-checa existência (TOCTOU; se surgiu, `seed-skipped` sem baixar) → baixa da **fonte** (`RelativePath`) → `ApplyAtomic` no **destino** (`SeedTargetRelative`) → conta `Seeded`, **sem** gravar baseline |

## Fluxo do planner (seed)

```
rule = Resolve(path, out matchedPrefix)
if rule == SeedIfMissingByName:
    targetRel = DeriveSeedTarget(path, matchedPrefix)   // config/<rel>
    if targetRel == null: continue                       // sem remainder
    if !File.Exists(GameRoot/targetRel):
        Actions += SeedCopy { RelativePath = path (fonte), SeedTargetRelative = targetRel }
    continue                                              // presente → no-op
```

O download reusa o `SyncDownloader` injetável: para o seed ele é chamado com a **fonte** (`config-server/<rel>`) e o resultado é escrito no **destino** (`config/<rel>`). Zero fork do engine — só um case novo.

## Server — `TarkovRedLine.Server/Controllers/ModUpdater.cs`

Mudança mínima: o `config.json` default (só escrito em instalação nova) ganha `["BepInEx/config-server"] = "seed-if-missing"` no `folderRules`. **Servers existentes não precisam de nada** — o fallback do client cobre. O scan do `mods_repo` já coloca `BepInEx/config-server/<rel>` no manifesto e o `/launcher/mods/download` já serve esses paths pelo `_fileMapCache`. Contrato de operação (A-017.5): defaults de seed ficam em `mods_repo/BepInEx/config-server/`.

## Interações verificadas

- **Cleanup do `GameStarter`** (A-017.x): só toca arquivos/pastas top-level fixos (`BattlEye`, `Logs`, `ConsistencyInfo`, exes, `hwecho.dll`) — **não** mexe em `BepInEx/config`. O arquivo semeado está a salvo por construção.
- **Overlay 008.** Overlay atua sobre `config`/`plugins` (paths do pack); config-server não colide. O merge do 008 não é afetado (seed não é manifest entry no lado do usuário).
- **Extras.** Arquivo semeado vive em `config` (`PreserveDivergent`), nunca é manifest entry → em `ScanExtras` cai no `continue` do preserve-divergent, **não** é deletado, mesmo com `managedPaths` cobrindo `BepInEx`.
- **Dev Mode.** O seed roda igual (só cria ausentes; nunca reverte build local). O auto-check do login continua pulando em Dev Mode; o seed acontece na verificação manual.

## Casos de borda

- **Prefixo seed sem `-server`** (operador setou `folderRules` estranho, ex.: `defaults`): `DeriveSeedTarget` devolve o próprio prefixo → seed "para si mesmo" (degenerado, documentado; não ocorre com config-server).
- **Mesmo `<rel>` em `config` (manifest entry) E `config-server` (seed):** setup patológico do operador (default em dois lugares). Ordem no plano pode variar; a re-checagem de existência no engine mitiga clobber. Orientação: não duplicar defaults entre `config` e `config-server`.

## Testes (`SPT.Launcher.Tests/Sync/`)

`SyncSeedTests.cs` (novo): alvo ausente→copia · alvo presente c/ conteúdo diferente→não toca · subpasta preservada · só o irmão ausente é copiado · server sem config-server→no-op · apagado reaparece (memoryless) · semeado sobrevive ao mirror de managedPath · TOCTOU (surgiu entre plano/apply→seed-skipped, sem baixar) · sem baseline gravado · `DeriveSeedTarget` (casing + null). `SyncRuleResolverTests.cs`: dois casos atualizados (config-server default = `SeedIfMissingByName`, não `Default`).

## Gates

Ver [05-asbuild](./017-seed-config-de-config-server-05-asbuild.md).
