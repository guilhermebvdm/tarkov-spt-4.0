# 007 — Sincronização de arquivos por pasta · Spec técnica

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Spec funcional:** [007-sincronizacao-arquivos-01-spec.md](./007-sincronizacao-arquivos-01-spec.md)

## Arquitetura do motor — `SPT.Launcher.Base/Sync/` (namespace `SPT.Launcher.Sync`)

Pipeline em 3 fases, todas canceláveis e testáveis sem HTTP:

```
manifesto + baseline + disco ──► SyncPlanner (puro, só leitura)  ──► SyncPlan (ações)
SyncPlan ──► SyncEngine (executa: download via delegate, delete, move) ──► SyncResult
SyncResult ──► SyncBaseline.Save() + SyncReport (last-update.json)
```

| Arquivo | Responsabilidade |
|---|---|
| `Sync/SyncFolderRule.cs` | Enum `Default · PreserveDivergent · MirrorDelete · MirrorMoveDisabled` + parse dos nomes canônicos (`default`, `preserve-divergent`, `mirror-delete`, `mirror-move-disabled`) |
| `Sync/SyncRuleResolver.cs` | Prefixo→regra. Merge: mapa `folderRules` do manifesto (server) **sobrepõe** tabela fallback embutida no client. Match por prefixo mais longo, case-insensitive, separador normalizado `/` |
| `Sync/SyncBaseline.cs` | Load/Save de `sync-state.json` (dict path→hash MD5; keys normalizadas lowercase + `/`). Corrupto/ausente ⇒ vazio (primeiro run) |
| `Sync/SyncAction.cs` | `SyncActionKind` (`Download, PreserveCustomized, PreserveDevMode, DeleteExtra, MoveToDisabled`) + payload (path, hash server, destino do move, regra, motivo) |
| `Sync/SyncPlanner.cs` | Planejamento: percorre manifesto (obrigatórios + opcionais ativos) aplicando R1–R5; varre pastas espelhadas + `managedPaths` procurando extras. **Não escreve nada.** Retorna `SyncPlan` (ações + lista `UpToDate` p/ semear baseline + warnings) |
| `Sync/SyncPlannerOptions.cs` | gameRoot, DevMode, ignoredFiles, excludeFromCleanup, protectedPaths (ex.: `GetAllKnownOptionalPaths()`), managedPaths, activeOptionalGroups |
| `Sync/SyncEngine.cs` | Execução do plano. Download abstraído por `SyncDownloader` (delegate `Task<byte[]>(string relPath, CancellationToken)`); deleção abstraída por `Action<string>` (default `File.Delete`; a UI injeta lixeira). Apply atômico. Atualiza baseline em memória por arquivo aplicado; persiste baseline + report no `finally` (cobre erro e cancelamento) |
| `Sync/SyncResult.cs` | Contagens (Updated/Preserved/PreservedDevMode/Deleted/MovedToDisabled/Errors/Pending), `Cancelled`, warnings, entradas do report |
| `Sync/SyncReport.cs` | Escrita de `last-update.json` + contagem por pasta + `OpenReportFolder()` (`Process.Start explorer`) |
| `Models/Launcher/ManifestFile.cs` | **Consolidação** da classe duplicada (ver abaixo) |

### Decisões de design

1. **Planner/Engine separados** — o planner é puro (leitura de disco + hash), o engine só executa ações já decididas. Testes cobrem cada um isolado; a UI pode exibir o plano antes de aplicar.
2. **Download por delegate** (`SyncDownloader`), não por interface HTTP — o motor roda em testes com um dicionário em memória; em produção a UI injeta `RequestHandler.DownloadModFile` (requisito "testável sem HTTP").
3. **Deleção injetável** — o Base não referencia `Microsoft.VisualBasic`; o default é `File.Delete`, e o `ModUpdateViewModel` injeta lixeira (`Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile` com `RecycleOption`, mesmo mecanismo do fluxo legado). Mantém a segurança atual (lixeira) sem dependência nova no Base e sem poluir a lixeira nos testes.
4. **Apply atômico** — download → `<dest>.sync-tmp` no **mesmo diretório** (garante mesmo volume) → `File.Move(tmp, dest, overwrite: true)`. Falha em qualquer etapa: tmp removido, destino intocado, erro contado.
5. **Baseline convergente sem risco** — arquivos com hash local == server entram no baseline mesmo sem apply (CC7): nunca gera falso "não customizado" porque local==server por definição.
6. **Persistência no `finally`** — baseline e `last-update.json` são gravados também em cancelamento/exceção (E4/C4).

## Mapeamento pasta→regra

### Server-side (`ModUpdater.cs` — mudança mínima)

`Launcher-Updater/config.json` ganha campo **opcional** `folderRules` (objeto prefixo→nome da regra), lido e re-emitido **pass-through** no manifesto:

```json
"folderRules": { "BepInEx/config": "preserve-divergent", "BepInEx/plugins": "mirror-move-disabled" }
```

**Decisão registrada (desvio consciente do enunciado):** em vez de campo `folderRule` **por arquivo**, um mapa top-level `folderRules` no manifesto — mesmo poder de expressão (o client resolve por prefixo do mesmo jeito), mudança server ainda menor (1 leitura + 1 propriedade no objeto anônimo), manifesto menor, e o operador do server configura regra por pasta (que é o modelo mental do card), não por arquivo. O default config gerado quando `config.json` não existe ganha o bloco `folderRules` de exemplo.

### Client fallback (quando manifesto não traz `folderRules`)

Tabela embutida no `SyncRuleResolver` — **assunção A3** sobre o layout do `mods_repo` (não vejo o disco do server; os paths do manifesto são relativos ao `mods_repo` e espelham a raiz do jogo, pois o client faz `Path.Combine(gamePath, file.path)`):

| Prefixo | Regra |
|---|---|
| `config/`, `BepInEx/config/` | PreserveDivergent |
| `config-server/` | MirrorDelete |
| `patchers/`, `BepInEx/patchers/` | MirrorMoveDisabled |
| `plugins/`, `BepInEx/plugins/` | MirrorMoveDisabled |
| resto | Default |

Nomes "crus" do card (`config/`, `plugins/`…) **e** os equivalentes BepInEx entram ambos: se o `mods_repo` usa top-level literal, casa a 1ª coluna; se espelha o layout real do jogo, casa a 2ª. Prefixo sem match nos paths reais é inofensivo. Se o layout real usar outros prefixos (ex.: `SPT/user/...` para config-server), o operador corrige via `folderRules` no config do server **sem rebuild do launcher**.

## Baseline — `user/launcher/sync-state.json`

- Local: `SptPathHelper.SptRootPath/user/launcher/sync-state.json` (mesma pasta do `config.json` do launcher e do `manifest_hash.txt`).
- Formato: `{ "version": 1, "files": { "bepinex/config/x.cfg": "md5..." } }` — keys lowercase, `/`.
- Escrita: após cada apply (`Download` ⇒ set hash server; `DeleteExtra`/`MoveToDisabled` ⇒ remove entrada) + seed dos `UpToDate`. Persistido 1× no fim do run (finally).
- Primeiro run: arquivo ausente ⇒ baseline vazio ⇒ R1.5/CC1 (conservador).
- `GameStarter.CleanTempFiles` (wipe) apaga só `manifest_hash.txt` — baseline **sobrevive** ao wipe (correto: reflete disco, não perfil).

## Integração com o cleanup existente

- **`GameStarter.SetupGameFiles`**: lista fixa (BattlEye, Logs, ConsistencyInfo, `EscapeFromTarkov_BE.exe`, `Uninstall.exe`, `UnityCrashHandler64.exe`, `WinPixEventRuntime.dll`, `hwecho.dll`). Interseção com pastas-regra e `-disabled`: **nenhuma**. Decisão: **zero mudança no GameStarter** (mudança mínima = nenhuma); o "mecanismo novo equivalente" pedido é a proteção interna do planner (abaixo).
- **Proteções do planner contra deleção/move de extras** (R2.3): (a) paths do manifesto completo — inclui opcionais de grupos OFF (CC3); (b) `ignoredFiles` do manifesto (match por substring, como o fluxo atual); (c) `ExcludeFromCleanup` do settings (match por segmento/prefixo); (d) `protectedPaths` extras injetados pela UI (`OptionalModsHelper.GetAllKnownOptionalPaths()`); (e) qualquer path contendo segmento `*-disabled` (R3.4); (f) Dev Mode ON ⇒ preserva + aviso (R5.2).
- **Deleção de extras da ProfileViewModel (legado)**: continua ativa até P-007.1. Sem conflito destrutivo com o motor: o legado só deleta o que não está no manifesto dentro de `managedPaths`; os preservados de `config` estão no manifesto. ⚠️ O legado ainda **re-baixa** divergentes de `config` (reverte customização) até a integração — exatamente o gap que P-007.1 fecha.

## Consolidação do `ManifestFile` (2 defs → 1)

Restrições: `ProfileViewModel.cs` (intocável nesta sessão) usa `ManifestFile` sem qualificar **e** `OptionalModsHelper.ManifestFile` qualificado, e tem `using SPT.Launcher.Models.Launcher;`.

- Canônica: `SPT.Launcher.Base/Models/Launcher/ManifestFile.cs`, namespace `SPT.Launcher.Models.Launcher` (props `path/hash/size/optional/optionalGroup`, casing minúsculo mantido — contrato JSON do server).
- `ModUpdateViewModel.cs`: def local **removida** — o unqualified em `ProfileViewModel`/`ModUpdateViewModel` resolve pela using existente.
- `OptionalModsHelper.ManifestFile`: vira casca `public class ManifestFile : SPT.Launcher.Models.Launcher.ManifestFile { }` — preserva o nome aninhado usado pela ProfileViewModel (linha 413) sem duplicar campos.

## Cancelamento (4.1.2)

- `CancellationTokenSource` no `ModUpdateViewModel` cobrindo verificação (planner) e download (engine).
- `CancelCommand` → `ShowDialog(new ConfirmationDialogViewModel(null, aviso…))` (dialog existente; host null = DialogHost) → confirmado ⇒ `Cancel()`.
- Planner/Engine checam o token **entre arquivos** (`ThrowIfCancellationRequested`); `SyncDownloader` recebe o token (aborta o download em curso quando o transporte suportar).
- Engine captura `OperationCanceledException`, marca `Cancelled`, calcula `Pending`, persiste baseline+report no finally.

## Manifesto de mudanças (4.1.3)

`user/launcher/last-update.json`:

```json
{ "generatedAt": "…", "cancelled": false,
  "counts": { "updated": 3, "preserved": 1, "preservedDevMode": 0, "deleted": 2, "movedToDisabled": 1, "errors": 0, "pending": 0 },
  "entries": [ { "path": "BepInEx/plugins/X.dll", "action": "updated", "timestamp": "…" } ] }
```

`SyncReport.CountByTopFolder()` p/ a UI futura; `SyncReport.OpenReportFolder(dir)` = `Process.Start(explorer)` (M3).

## UI mínima deste item — `ModUpdateViewModel` (sem tocar ProfileView*)

- VM órfã hoje (nenhuma view a referencia) — vira a implementação de referência do motor: `CheckForUpdates` monta plano via planner; `UpdateMods` executa engine; progresso (`Progress/MaxProgress/StatusText`), `CancelCommand` com confirmação, `SummaryText` = "X atualizados · Y preservados · Z movidos p/ disabled" (+ deletados/erros/avisos Dev Mode quando > 0).
- Strings novas: literais PT (precedente do código atual: strings PT hardcoded na ProfileViewModel); keys `update_*` existentes reutilizadas onde já servem.

## Testes — `project/SPT.Launcher.Tests` (xUnit, net9.0, ref só ao Base)

Contra diretório temp real (`Path.GetTempPath()` + GUID, cleanup em `Dispose`):

1. PreserveDivergent: novo (R1.1) · igual-baseline (R1.3 baixa) · customizado (R1.4 preserva) · primeiro-run (R1.5 preserva).
2. MirrorDelete: extra deletado; protegidos (ignored/excludeFromCleanup/opcional OFF) intactos.
3. MirrorMoveDisabled: extra movido preservando subpasta; colisão no `-disabled` substitui (R3.3); `-disabled` não re-varrido (R3.4).
4. Baseline round-trip: save/load; seed de UpToDate; remoção em delete/move; corrupto ⇒ vazio.
5. Cancelamento: downloader cancela o CTS após o 1º arquivo ⇒ 1 aplicado, resto `Pending`, `Cancelled=true`, baseline só com o 1º.
6. Dev Mode: local ≠ manifesto ≠ baseline ⇒ preservado + warning; Dev Mode OFF ⇒ baixa.
7. Resolver: `folderRules` do server sobrepõe fallback; prefixo mais longo vence.
8. Atomicidade: downloader lança ⇒ destino intocado, tmp limpo, erro contado.
9. Report: last-update.json com counts + entries.

## Gates

- `dotnet build launcher/Launcher4.0beta/project/SPT.Launcher/SPT.Launcher.csproj -c Release` verde.
- `dotnet test launcher/Launcher4.0beta/project/SPT.Launcher.Tests/SPT.Launcher.Tests.csproj -c Release` verde.
- `dotnet build mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/TarkovRedLine.Server.csproj -c Release` verde (ModUpdater.cs editado).
- Lock transitório de build (agentes paralelos) ⇒ retry 20s, 3×. **Nunca** rodar o exe do launcher.

## Pendências (para o asbuild)

- **P-007.1** — integrar motor na ProfileViewModel/ProfileView (arquivos bloqueados nesta sessão): rotear `CheckForUpdates`/`DoUpdateMods` pelo motor, link "X arquivos foram atualizados" → `OpenReportFolder`, remover deleção legada de extras.
- **P-007.2** — E2E contra `D:\SPT` real (gates deste item são build+test).
