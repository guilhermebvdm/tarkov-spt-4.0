# 007 — Sincronização de arquivos por pasta · As-built

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 (2ª passada: P-007.1/P-007.3 resolvidas) · **Specs:** [01-spec](./007-sincronizacao-arquivos-01-spec.md) · [02-spec-tech](./007-sincronizacao-arquivos-02-spec-tech.md)

> Desvio de processo registrado: reviews 03/04 fundidas neste as-built (instrução do coordenador — sessão autônoma). O motor foi entregue com `ProfileView*` sob lock de outro agente (commit 0d355f3); após o merge do UI-pack (50cfce1, ProfileView restylada com TrlSidebarNav/TrlPanel) o lock caiu e P-007.1 (integração) + P-007.3 (ModUpdateView) foram concluídas na 2ª passada.

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

Reescrito sobre o motor: plano via `SyncPlanner`, apply via `SyncEngine`, `CancelCommand` com `ConfirmationDialogViewModel` (4.1.2), `SummaryText`, `OpenLastUpdateFolder()`, lixeira injetada no delete (mesmo mecanismo legado), status/ícones novos (`preserved`/`moved`/`deleted`), `IsBusy` p/ estados da view.

### Integração P-007.1 — `ProfileViewModel.cs` + `ProfileView.axaml` (2ª passada, pós-UI-pack)

- **Fluxo de login/verify roda o motor**: o loop inline legado (scan MD5 + deleção de extras em `managedPaths` + `File.WriteAllBytesAsync` direto) foi substituído por `SyncPlanner` + `SyncEngine` — o `config` divergente do baseline agora é **preservado** no login (gap do legado fechado). Mantidos: retry do manifesto (5×3s + countdown 30s), população dos toggles opcionais, `deleteFiles` explícito do server (lixeira), save do `manifest_hash.txt` (**pulado se cancelado**, forçando rescan no próximo login).
- **Cancelamento (4.1.2)**: `CancelUpdateCommand` + `CanCancelUpdate`; botão "CANCELAR" `.ghost sm` ao lado da ProgressBar da área de update; confirmação via `ConfirmationDialogViewModel` com alerta de estado parcial; token cobre planejamento e apply.
- **Link 4.1.3**: propriedades `LastUpdateText`/`HasLastUpdate` + botão `.link` "N arquivo(s) foram atualizados — ver detalhes" → `SyncReport.OpenReportFolder(user/launcher)`. Contagem populada após cada run **e** carregada do `last-update.json` anterior no ctor (persiste entre sessões).
- **Dev Mode (decisão D1 da 2ª passada)**: o auto-check do login **continua pulando** em Dev Mode (login rápido; server pode nem estar de pé em dev), mas a **verificação manual** ("VERIFICAR ARQUIVOS" / `UpdateModsCommand`) roda o motor com proteção R5 (divergentes do baseline preservados + aviso). Meio-termo entre o legado (pulava sempre) e a spec R5 pura (sincronizava sempre).
- **Removidos**: `DoUpdateMods` legado, `GetFileMD5`, `_filesToUpdate`/`_filesToDelete`. `UpdateModsCommand` agora delega p/ `ForceCheckForUpdates` (o legado já era auto-apply sem botão).
- **Restyle preservado**: só a `StackPanel` da barra de update foi tocada na view (cancelar + link); tudo com tokens `Trl*`/classes do tema, zero hex.

### P-007.3 — `Views/ModUpdateView.axaml` (+ `.axaml.cs`)

View da VM de update (resolvida pelo `ViewLocator` por convenção de nome): título + status, `ProgressBar` do tema com "CANCELAR" `.ghost sm`, resumo (`SummaryText`), lista de arquivos (ícone/nome/tamanho) em `Border` com `TrlBgPanelBrush`/`TrlEdgeBrush`, ações "VERIFICAR NOVAMENTE" (`.sm`, `!IsBusy`), "ATUALIZAR" (`.primary`, `CanUpdate`) e link "ABRIR PASTA DO RELATÓRIO" (4.1.3). Codebehind no padrão `ReactiveUserControl<T>` (igual `ModInfoView`).

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
2. **A2 — layout do `mods_repo`**: não vejo o disco do server; os paths do manifesto espelham a raiz do jogo (o client faz `Path.Combine(gamePath, path)`). Assumi `config`→`BepInEx/config`, `patchers`→`BepInEx/patchers`, `plugins`→`BepInEx/plugins`. A tabela fallback inclui os nomes crus do card **e** os BepInEx (prefixo sem match é inócuo). **Revisado (CR-01-03):** `config-server → mirror-delete` SAIU do fallback — a regra mais destrutiva só ativa via `folderRules` explícito do server (default seguro até P-007.2).
3. **A3 — mapa `folderRules` top-level em vez de campo por arquivo** (desvio consciente do enunciado): mesmo poder de expressão, mudança server menor, configurável pelo operador sem rebuild do launcher. O server em produção não tem `folderRules` no config hoje ⇒ vale o fallback do client até configurarem.
4. **A4 — Dev Mode ON sincroniza** (só protege divergentes do baseline + extras, com aviso) — upgrade sobre o legado que pulava a sync inteira. Extra local sob Dev Mode = build de dev ⇒ preservado + aviso.
5. **A5 — colisão no `-disabled`**: o arquivo recém-movido substitui o antigo (a versão mais nova do usuário vale).
6. **A6 — MirrorDelete no primeiro run deleta mesmo sem baseline** (card manda espelhar; deleção via lixeira quando executado pela UI — deleter injetado). **Revisado (CR-01-03):** só se aplica quando o server ativou a regra via `folderRules` — sem isso, nenhum MirrorDelete existe.
7. **A7 — deleção via lixeira só na UI**: o Base não referencia `Microsoft.VisualBasic`; default `File.Delete`, `ModUpdateViewModel` injeta lixeira (paridade com o legado, testes não poluem a lixeira).
8. **A8 — strings novas em PT hardcoded** (precedente do código atual); keys `update_*` existentes reutilizadas onde servem.
9. **A9 — cancelamento não aborta o download em curso** (o `RequestHandler.DownloadModFile` legado não é cancelável mid-transfer); o token vale entre arquivos — o arquivo em voo termina atômico ou é descartado.
10. **A10 — local do estado**: `SptPathHelper.SptRootPath/user/launcher/` (mesma pasta do `config.json` do launcher e `manifest_hash.txt`).

## Gates

```
1ª passada (motor):
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s)  (148 warnings pré-existentes de nullability/CA1416)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 39/39, 0 falhas
dotnet build TarkovRedLine.Server.csproj -c Release    → 0 Erro(s)

2ª passada (integração P-007.1 + view P-007.3):
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 39/39, 0 falhas

3ª passada (apply do code review 04-01 — 1 🔴 + 5 🟡, todos aplicados):
dotnet build SPT.Launcher.csproj -c Release            → 0 Erro(s)
dotnet test  SPT.Launcher.Tests.csproj -c Release      → Aprovado! 52/52, 0 falhas
                                                          (39 + 8 novos do review + 5 do SyncOverlayTests, track 008)
```

## Code review aplicado (3ª passada)

Review adversarial em [007-sincronizacao-arquivos-04-code-review-01.md](./007-sincronizacao-arquivos-04-code-review-01.md) — ver a seção "Resoluções" lá para o detalhe por achado. Resumo: guard de reentrância no sync da ProfileViewModel (`Interlocked` + `CheckForUpdatesCore` p/ o retry recursivo + `CanVerifyFiles` no botão) [CR-01-01 🔴]; `ignoredFiles` não filtra mais o manifesto (SPT core volta a atualizar) [CR-01-02]; `config-server → mirror-delete` fora do fallback (só via `folderRules` do server) [CR-01-03]; `sync-state.json`/`last-update.json` com escrita atômica temp+move [CR-01-04]; guard anti-traversal `ResolveUnderRoot` em todo write/move/delete do engine [CR-01-05]; 8 testes novos cobrindo os cenários acima [CR-01-06].

## Pendências

- ~~**P-007.1** — Integração ProfileView/ProfileViewModel~~ → **resolvida na 2ª passada** (ver seção "Integração P-007.1").
- **P-007.2 — E2E contra `D:\SPT` real** (gate humano; memória do repo: escrita em SPT exige validação no jogo): rodar login+verify contra o server real, validar `sync-state.json`/`last-update.json`/`plugins-disabled` no disco, o link "X arquivos foram atualizados" abrindo a pasta, o cancelamento no meio de um download e configurar `folderRules` no `Launcher-Updater/config.json` do server conforme o layout real do `mods_repo`.
- ~~**P-007.3** — View p/ ModUpdateViewModel~~ → **resolvida na 2ª passada** (`Views/ModUpdateView.axaml`). Obs.: nenhuma navegação aponta p/ `ModUpdateViewModel` ainda — a tela existe e resolve pelo ViewLocator; plugá-la num menu é decisão de UX futura (o fluxo principal de update é o da ProfileView).
- Item 008 (configs de performance) reusa este motor, conforme kickoff.
