# 007 — Sincronização de arquivos por pasta · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Specs:** [01-spec](./007-sincronizacao-arquivos-01-spec.md) · [02-spec-tech](./007-sincronizacao-arquivos-02-spec-tech.md)

> Desvio de processo registrado: reviews 03/04 fundidas neste as-built (instrução do coordenador — sessão autônoma). Restrição da sessão: `ProfileView.axaml`/`ProfileViewModel.cs` bloqueados (outro agente) — apenas lidos.

## O que foi construído

### Motor — `SPT.Launcher.Base/Sync/` (namespace `SPT.Launcher.Sync`)

| Arquivo | Papel |
|---|---|
| `SyncFolderRule.cs` | Enum das 4 regras + parser dos nomes canônicos (`default`, `preserve-divergent`, `mirror-delete`, `mirror-move-disabled`) |
| `SyncPathUtil.cs` | Normalização de path (lowercase, `/`), prefix-match por segmento, detector de `-disabled`, MD5 |
| `SyncRuleResolver.cs` | Prefixo→regra, longest-prefix; `folderRules` do server sobrepõe tabela fallback embutida |
| `SyncBaseline.cs` | `user/launcher/sync-state.json` (dict path→MD5); ausente/corrupto ⇒ vazio (primeiro run conservador) |
| `SyncAction.cs` / `SyncPlan.cs` | Modelo do plano (Download · PreserveCustomized · PreserveDevMode · DeleteExtra · MoveToDisabled) + `UpToDate` p/ semear baseline + warnings |
| `SyncPlannerOptions.cs` | gameRoot, DevMode, ignoredFiles, excludeFromCleanup, protectedPaths, managedPaths, opcionais ativos |
| `SyncPlanner.cs` | Planejamento puro (só leitura): R1–R5 da spec + varredura de extras com proteções R2.3 + resolução de casing real do disco p/ os alvos `-disabled` |
| `SyncEngine.cs` | Execução: download via delegate `SyncDownloader` (testável sem HTTP), deleção injetável (`Action<string>`, default `File.Delete`), apply atômico (`<dest>.sync-tmp` + `File.Move(overwrite)`), cancelamento entre arquivos, baseline+report persistidos no `finally` |
| `SyncResult.cs` | Contagens, `Cancelled`, `Pending`, `Summary` ("X atualizados · Y preservados · Z movidos p/ disabled…") |
| `SyncReport.cs` | `last-update.json` (4.1.3), `CountByTopFolder()`, `OpenReportFolder()` (explorer) |
| `Models/Launcher/ManifestFile.cs` | Classe canônica consolidada (era duplicada em `ModUpdateViewModel.cs` e `OptionalModsHelper.cs`) |

### UI deste item — `SPT.Launcher/ViewModels/ModUpdateViewModel.cs`

Reescrito sobre o motor (VM órfã — nenhuma view a referencia hoje; vira implementação de referência): plano via `SyncPlanner`, apply via `SyncEngine`, `CancelCommand` com `ConfirmationDialogViewModel` (4.1.2), `SummaryText`, `OpenLastUpdateFolder()`, lixeira injetada no delete (mesmo mecanismo legado), status/ícones novos (`preserved`/`moved`/`deleted`). Nenhum XAML tocado.

### Consolidação `ManifestFile`

- Canônica em `SPT.Launcher.Models.Launcher` (Base). Def local do `ModUpdateViewModel` removida — `ProfileViewModel` resolve pela `using SPT.Launcher.Models.Launcher;` que já tinha (zero edição nela).
- `OptionalModsHelper.ManifestFile` virou casca `: SPT.Launcher.Models.Launcher.ManifestFile` (nome aninhado preservado p/ a ProfileViewModel linha 413).

### Server — `mods/TarkovRedLine4.0/Server/.../ModUpdater.cs` (mudança mínima)

- Lê `folderRules` (objeto prefixo→regra) do `Launcher-Updater/config.json` e re-emite no manifesto (pass-through).
- Default config gerado (só quando `config.json` não existe) ganha bloco `folderRules` de exemplo (`BepInEx/config` preserve, `BepInEx/patchers|plugins` move-disabled).

### GameStarter — decisão: zero mudança

`SetupGameFiles` remove só artefatos fixos do EFT live (BattlEye, Logs, `EscapeFromTarkov_BE.exe`…) — sem interseção com pastas-regra/`-disabled`. `CleanTempFiles` (wipe) apaga só `manifest_hash.txt`; baseline sobrevive (correto). O "mecanismo equivalente" pedido é a proteção interna do planner: manifesto completo (opcionais OFF incl.), `ignoredFiles`, `ExcludeFromCleanup`, `protectedPaths` (`GetAllKnownOptionalPaths()`), segmentos `-disabled`, Dev Mode.

## Testes — `project/SPT.Launcher.Tests` (novo, no `Launcher.sln`)

xUnit net9.0, referencia só o Base, roda contra diretório temp real. **39/39 verdes**: PreserveDivergent (novo · igual-baseline · customizado · primeiro-run · seed CC7), MirrorDelete (+proteções ignored/exclude/opcional OFF), MirrorMoveDisabled (subpasta preservada · colisão substitui · `-disabled` não re-varrido · idempotência do 2º run), Default managedPaths, Dev Mode (3 casos), baseline round-trip (+corrupto), cancelamento entre arquivos (parcial consistente, report `cancelled`), atomicidade (destino intocado, sem `.sync-tmp` órfão, baseline sem o falho), report (counts+entries+por-pasta), resolver (fallback, override server, longest-prefix, segment-aware, nome inválido).

## Assunções registradas

1. **A1 — "substituir os iguais" = igual ao baseline** da última sync (não server×local). Já registrada no kickoff; implementada como R1 (spec 01).
2. **A2 — layout do `mods_repo`**: não vejo o disco do server; os paths do manifesto espelham a raiz do jogo (o client faz `Path.Combine(gamePath, path)`). Assumi `config`→`BepInEx/config`, `patchers`→`BepInEx/patchers`, `plugins`→`BepInEx/plugins`; `config-server` sem equivalente conhecido → fallback só o prefixo literal `config-server/`. A tabela fallback inclui os nomes crus do card **e** os BepInEx (prefixo sem match é inócuo).
3. **A3 — mapa `folderRules` top-level em vez de campo por arquivo** (desvio consciente do enunciado): mesmo poder de expressão, mudança server menor, configurável pelo operador sem rebuild do launcher. O server em produção não tem `folderRules` no config hoje ⇒ vale o fallback do client até configurarem.
4. **A4 — Dev Mode ON sincroniza** (só protege divergentes do baseline + extras, com aviso) — upgrade sobre o legado que pulava a sync inteira. Extra local sob Dev Mode = build de dev ⇒ preservado + aviso.
5. **A5 — colisão no `-disabled`**: o arquivo recém-movido substitui o antigo (a versão mais nova do usuário vale).
6. **A6 — MirrorDelete no primeiro run deleta mesmo sem baseline** (card manda espelhar; deleção via lixeira quando executado pela UI — deleter injetado).
7. **A7 — deleção via lixeira só na UI**: o Base não referencia `Microsoft.VisualBasic`; default `File.Delete`, `ModUpdateViewModel` injeta lixeira (paridade com o legado, testes não poluem a lixeira).
8. **A8 — strings novas em PT hardcoded** (precedente do código atual); keys `update_*` existentes reutilizadas onde servem.
9. **A9 — cancelamento não aborta o download em curso** (o `RequestHandler.DownloadModFile` legado não é cancelável mid-transfer); o token vale entre arquivos — o arquivo em voo termina atômico ou é descartado.
10. **A10 — local do estado**: `SptPathHelper.SptRootPath/user/launcher/` (mesma pasta do `config.json` do launcher e `manifest_hash.txt`).

## Gates

```
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s)  (148 warnings pré-existentes de nullability/CA1416)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 39/39, 0 falhas
dotnet build TarkovRedLine.Server.csproj -c Release    → 0 Erro(s)
```

## Pendências

- **P-007.1 — Integração ProfileView/ProfileViewModel** (arquivos bloqueados nesta sessão): rotear `CheckForUpdates`/`DoUpdateMods` pelo motor (substituindo o loop inline e a deleção legada de extras), link "X arquivos foram atualizados" → `SyncReport.OpenReportFolder`, botão cancelar no fluxo de login. Até lá o legado ainda re-baixa `config` divergente no fluxo de login (gap conhecido).
- **P-007.2 — E2E contra `D:\SPT` real** (memória do repo: escrita em SPT exige validação no jogo): rodar verificação+apply contra o server real, validar `sync-state.json`/`last-update.json`/`plugins-disabled` no disco e configurar `folderRules` no `Launcher-Updater/config.json` do server conforme o layout real do `mods_repo`.
- **P-007.3 — View p/ ModUpdateViewModel**: a VM está pronta (progresso/cancelar/resumo) mas não há `ModUpdateView.axaml`; criar quando a UI do item for desenhada (tokens `Trl*`).
- Item 008 (configs de performance) reusa este motor, conforme kickoff.
