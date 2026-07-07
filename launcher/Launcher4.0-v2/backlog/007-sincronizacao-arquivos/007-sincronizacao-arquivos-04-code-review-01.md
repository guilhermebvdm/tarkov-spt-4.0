# 007 — Sincronização de arquivos por pasta · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Revisor:** agente adversarial (contexto limpo) · **Commits revisados:** `0d355f3` (motor + testes) · `612f4d8` (integração ProfileViewModel + ModUpdateView)

> Item de MAIOR risco do projeto: DELETA e MOVE arquivos do usuário. Revisão implacável focada em cenários de destruição. Escopo: `SPT.Launcher.Base/Sync/*` (11 arquivos), `Models/Launcher/ManifestFile.cs`, `SPT.Launcher.Tests/*`, `ViewModels/{ProfileViewModel,ModUpdateViewModel}.cs`, `Views/{ProfileView,ModUpdateView}.axaml`, `Helpers/OptionalModsHelper.cs`, `ModUpdater.cs`.

## Placar

| Severidade | Qtd | IDs |
|---|---|---|
| 🔴 Bloqueante | 1 | CR-01-01 |
| 🟡 Atenção | 5 | CR-01-02, CR-01-03, CR-01-04, CR-01-05, CR-01-06 |
| 🟢 Limpo (declarado) | — | ver seção "Áreas auditadas e limpas" |

## Gates

```
dotnet build SPT.Launcher/SPT.Launcher.csproj -c Release            → 0 Erro(s), 150 Aviso(s) (pré-existentes: nullability/CA1416)
dotnet test  SPT.Launcher.Tests/SPT.Launcher.Tests.csproj -c Release → Aprovado! 39/39, 0 falhas (89 ms)
dotnet build TarkovRedLine.Server.csproj -c Release                 → 0 Erro(s), 32 Aviso(s)
```

Todos verdes. (Exe do launcher NÃO executado, conforme instrução.)

---

## 🔴 CR-01-01 — Reentrância: dois `CheckForUpdates` concorrentes rodam dois motores destrutivos sobre os MESMOS arquivos

**Arquivo:** `SPT.Launcher/ViewModels/ProfileViewModel.cs` (`CheckForUpdates`, `ForceCheckForUpdates`, `InitializeAsync`, campo `_syncCts`)

**Cenário concreto (arquivo-a-arquivo):**

1. Login → `ProfileViewModel` é construído → ctor dispara `_ = InitializeAsync()` → `await CheckForUpdates()` (auto-check, **não** é `ReactiveCommand`, sem gate).
2. Esse fluxo fica preso em awaits de rede longos: retry do manifesto `5 × Task.Delay(3000)` (15 s) + countdown `30 × Task.Delay(1000)` (30 s). Durante esses awaits a UI thread está livre.
3. O botão **"VERIFICAR ARQUIVOS"** (`ProfileView.axaml` linha 202) está `IsEnabled="{Binding CanStartGame}"` — **não** amarrado a nenhum flag de sync-em-progresso. O usuário clica.
4. `VerifyFilesCommand` → `ForceCheckForUpdates()` → deleta `manifest_hash.txt` → `CheckForUpdates(manual:true)`. Agora existem **duas corrotinas `CheckForUpdates` intercaladas**.
5. Ambas chegam a `planner.BuildPlanAsync` + `engine.ExecuteAsync` e operam sobre o **mesmo `GameRoot`** e o **mesmo `sync-state.json`/`last-update.json`**:
   - Dois `SyncEngine` fazem `File.Move`/`_deleteFile`/`File.WriteAllBytes`+`File.Move` concorrentes em paths sobrepostos → `IOException` no melhor caso; no pior, um engine move `BepInEx/plugins/X.dll` para `-disabled` enquanto o outro tenta baixá-lo, ou um deleta o `.sync-tmp` do outro.
   - Dois `baseline.Save()` (`File.WriteAllText`, **não atômico**) na mesma `sync-state.json` → escrita intercalada → **JSON corrompido**. Na próxima sessão o baseline corrompido vira vazio (CR-01-04) → `config-server` volta a mirror-delete.
6. Além do disco: o campo `_syncCts` é **compartilhado** e sobrescrito pelo 2º run — `CancelUpdate` passa a cancelar só o 2º; o 1º fica órfão e não cancelável.

`ModUpdateViewModel` tem o gate certo (`if (IsChecking || IsUpdating) return;` no topo de `CheckForUpdates`/`UpdateMods`). `ProfileViewModel` — o fluxo destrutivo real de login — **não tem gate nenhum**. `CheckForUpdates` seta `LauncherSettingsProvider.Instance.IsUpdating = true` mas nunca **checa** esse flag na entrada para abortar.

Reforço: reentrância também dispara por **navegar-sair-e-voltar** para a ProfileView (nova VM, novo auto-check, enquanto a instância antiga ainda aguarda rede) e por clicar **VERIFICAR** e depois **JOGAR→re-login→...** — qualquer combinação que dispare um 2º `CheckForUpdates` antes do 1º terminar.

**Impacto:** corrupção de estado do baseline + operações de arquivo concorrentes (delete/move) sobre a instalação do usuário. Exatamente a classe de bug que este item precisa blindar.

**Fix proposto:**

- Guard de reentrância em `ProfileViewModel.CheckForUpdates` — o mais simples espelhando o `ModUpdateViewModel`:
  ```csharp
  private int _syncRunning; // 0 = idle, 1 = running
  private async Task CheckForUpdates(bool manual = false)
  {
      if (Interlocked.CompareExchange(ref _syncRunning, 1, 0) != 0)
      {
          LogManager.Instance.Info("[Profile] Sync já em andamento — ignorando disparo concorrente.");
          return;
      }
      try { /* corpo atual */ }
      finally { Interlocked.Exchange(ref _syncRunning, 0); /* + finally atual */ }
  }
  ```
  (Um `bool` simples não basta porque o auto-check e o clique podem entrar quase simultaneamente antes do primeiro `await`; `Interlocked`/`SemaphoreSlim(1,1)` fecha a janela. Já existe precedente de `SemaphoreSlim` para os toggles opcionais — `_optionalToggleSemaphore`.)
- O guard deve cobrir também o retry recursivo (`await CheckForUpdates(manual)` na linha ~416): passar o "lock" adiante ou re-checar. Como o recursivo é do MESMO fluxo, considerar extrair o corpo para um método privado sem guard e o guard só no ponto de entrada público.
- Bônus: amarrar `IsEnabled` do botão "VERIFICAR ARQUIVOS" a `!IsUpdating` (ou a um novo `CanVerify`) para feedback visual + defesa em profundidade.

---

## 🟡 CR-01-02 — `IsIgnored` aplicado ao loop de download do manifesto pula updates do SPT core (regressão + desvio de spec)

**Arquivo:** `SPT.Launcher.Base/Sync/SyncPlanner.cs` (linha 79: `if (IsIgnored(normalized)) continue;` dentro do loop de arquivos do manifesto) + `IsIgnored` (substring `Contains`)

**Cenário concreto:**

- O config padrão do server (`ModUpdater.cs`) gera `ignoredFiles = ["BepInEx/plugins/spt", "user/mods/spt"]` — os diretórios do **SPT core**, que também estão **no manifesto** (o mods_repo os embarca).
- `IsIgnored` faz `normalizedPath.Contains(ignored)` (substring, semântica legada). Para um arquivo do manifesto `bepinex/plugins/spt/spt-core.dll`, `Contains("bepinex/plugins/spt")` = true → `continue`.
- Resultado: esse arquivo **não** entra em `Actions` (nem Download, nem Preserve), **nem** em `UpToDate`. Quando o server sobe uma versão nova do SPT core, o launcher **silenciosamente não atualiza** os arquivos sob `BepInEx/plugins/spt` e `user/mods/spt`.

**Diferença vs legado (`612f4d8^`):** o loop de download antigo (`filesToCheck` na `ProfileViewModel` legada) **não** aplicava `ignoredFiles` — baixava todo arquivo do manifesto desatualizado. `ignoredFiles` só era consultado na varredura de extras em `managedPaths` (proteção contra **deleção**, nunca contra **update**). A spec (R2.3) confirma: `ignoredFiles` é proteção de extra contra deleção, não filtro do manifesto. Aplicá-lo ao loop de download é regressão funcional **e** desvio de spec.

**Impacto:** não é destrutivo (só deixa de atualizar), mas pode deixar o SPT core do cliente defasado do server → quebra indireta do jogo/coop. Passou despercebido porque **nenhum teste** cobre "arquivo do manifesto cujo path casa um ignored substring" (ver CR-01-06).

**Fix proposto:** remover a checagem `IsIgnored` do loop do manifesto (linha 79) — mantê-la só no `ScanExtras` (linha 190), onde é a semântica correta (proteger extra de deleção/move). Arquivos do manifesto já são protegidos de deleção por `manifestPaths.Contains(...)`; não precisam de `IsIgnored` para nada no fluxo de download.

---

## 🟡 CR-01-03 — `config-server → mirror-delete` ativo por DEFAULT via fallback do client, sobre layout NÃO verificado (A2)

**Arquivos:** `SPT.Launcher.Base/Sync/SyncRuleResolver.cs` (`FallbackRules["config-server"] = "mirror-delete"`) + `ModUpdater.cs` (default config **não** inclui `config-server`)

**Cenário concreto:**

- `MirrorDelete` é a regra mais destrutiva (deleta extras). O default config gerado pelo server só configura `BepInEx/config|patchers|plugins`. O `config-server → mirror-delete` **só** vem da tabela fallback embutida no client, **ligada por padrão**.
- A própria spec-tech marca isso como **assunção A2 não verificada** ("não vejo o disco do server") e o as-build deixa a validação real como **P-007.2** (E2E contra `D:\SPT`, ainda pendente).
- Se, no `mods_repo` real, houver uma pasta cujo path resolvido case o prefixo `config-server/`, e o usuário tiver arquivos locais lá fora do manifesto, o **primeiro run** já os move para a lixeira (CC1 assume isso). Sem baseline, sem confirmação por-arquivo, sem E2E validado.

Mitigantes reais: (a) se `config-server/` não existir no disco do usuário, `Directory.Exists(rootDir)` é falso e o scan é pulado (nada acontece); (b) deleção vai para a lixeira (deleter injetado pela UI). Ainda assim, ligar a regra mais destrutiva **por default** apoiada numa assunção explicitamente não confirmada é o maior risco pré-produção do item.

**Impacto:** deleção em massa (recuperável via lixeira) de uma pasta real no primeiro login, se o layout divergir da assunção.

**Fix proposto (escolher um):**
1. **Remover `config-server` da tabela fallback** e exigir que o operador ative via `folderRules` no `Launcher-Updater/config.json` — assim a regra destrutiva só liga com configuração explícita e consciente (o mecanismo de override já existe e é testado). É o mais alinhado com "mudança server sem rebuild".
2. Ou: manter no fallback mas **gate atrás de P-007.2** — não fazer release para usuários até o E2E confirmar o que existe em `config-server/` no `D:\SPT` real, e documentar no as-build que o primeiro run foi validado. Registrar a decisão.

---

## 🟡 CR-01-04 — `SyncBaseline.Save` e `SyncReport.Write` são escritas NÃO atômicas (crash no meio corrompe o estado)

**Arquivos:** `SPT.Launcher.Base/Sync/SyncBaseline.cs` (`Save` → `File.WriteAllText`) · `SyncReport.cs` (`Write` → `File.WriteAllText`)

**Cenário concreto:**

- O motor é meticuloso com apply atômico dos arquivos do usuário (`<dest>.sync-tmp` + `File.Move(overwrite)`), mas o **próprio arquivo de estado** (`sync-state.json`) é gravado com `File.WriteAllText` cru no `finally`. Se o processo morre no meio dessa escrita (kill do launcher, crash, energia), o `sync-state.json` fica truncado/corrompido.
- Próximo run: `SyncBaseline.Load` pega o JSON inválido → catch → **baseline vazio** (primeiro run). Efeito: (a) `config` inteiro re-tratado como customizado → para de atualizar arquivos de config genuinamente intocados até o usuário deletá-los à mão; (b) `config-server` volta ao mirror-delete de "primeiro run" (CR-01-03).
- Combina com CR-01-01: dois `Save()` concorrentes garantem o mesmo estrago sem precisar de crash.

**Impacto:** perda silenciosa do baseline. Direção "segura" para `config` (preserva), mas destrava mirror-delete de `config-server` e degrada a convergência do baseline. Irônico dado que os applies são atômicos.

**Fix proposto:** aplicar o mesmo padrão temp+move do engine em `SyncBaseline.Save` e `SyncReport.Write`:
```csharp
string tmp = FilePath + ".tmp";
File.WriteAllText(tmp, json);
File.Move(tmp, FilePath, overwrite: true);
```

---

## 🟡 CR-01-05 — Sem validação de que o path resolvido fica sob `GameRoot` (traversal `..` no Download; transporte HTTP)

**Arquivos:** `SyncPathUtil.ToLocalPath` (`Path.Combine`, sem normalização de `..`) · `SyncEngine` (Download/Delete/Move usam o path do manifesto direto)

**Cenário concreto:**

- `Normalize` é só para **comparação** (lowercase + `/`); não remove `..`. `ToLocalPath(root, "config-server/../../../Windows/System32/x.dll")` → `Path.Combine` mantém os `..`; ao passar para `File.WriteAllBytes`/`File.Move`, o .NET resolve o `..` e escreve **fora** do `GameRoot`.
- Na prática o manifesto do server nasce de `Path.GetRelativePath(modsPath, file)` (não produz `..`), e o transporte é a porta 7075 sobre Tailscale (WireGuard). Mas o download roda em **HTTP** (`http://{host}` no `OptionalModsHelper`; `request.RemoteEndPoint`), então um manifesto adulterado (MITM na LAN, ou config do server comprometido) tem **zero** defesa client-side no apply.
- `DeleteExtra`/`MoveToDisabled` vêm de `Path.GetRelativePath` de arquivos enumerados no disco (não traversáveis), então o vetor real é o **Download**.

**Impacto:** defesa em profundidade ausente numa operação que escreve arquivos. Baixa probabilidade (server confiável + Tailscale), mas o custo do fix é trivial e o item é o mais sensível do projeto.

**Fix proposto:** antes de qualquer `ApplyAtomic`/`MoveWithOverwrite`/`_deleteFile`, validar que o path absoluto resolvido começa com `Path.GetFullPath(gameRoot)`:
```csharp
string full = Path.GetFullPath(dest);
if (!full.StartsWith(Path.GetFullPath(_gameRoot) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException($"path escapes game root: {action.RelativePath}");
```
(Contar como erro por-arquivo, não derrubar o run.)

---

## 🟡 CR-01-06 — Lacunas de cobertura de teste nos cenários de destruição

Os 39 testes cobrem bem o caminho feliz e várias proteções, mas faltam os cenários de maior risco. Buracos concretos + teste proposto:

| # | Lacuna | Teste proposto |
|---|---|---|
| a | **`IsIgnored` no loop do manifesto** (CR-01-02) — nenhum teste afirma que um arquivo do manifesto cujo path casa um `ignoredFiles` É baixado. Essa ausência escondeu a regressão. | `Config: manifest file whose path contains an ignored substring is still downloaded when outdated` (`IgnoredFiles=["plugins/spt"]`, manifesto com `BepInEx/plugins/spt/core.dll` desatualizado → espera `Download`). |
| b | **Traversal `..`** (CR-01-05) — nenhum teste com path do manifesto contendo `..`. | `Download: manifest path with '..' does not escape game root` (após fix, espera `error` contado e nenhum arquivo escrito fora de `Root`). |
| c | **IOException em Move/Delete mid-run** — só `Failed_download_leaves_destination_untouched` cobre falha de **download**. Move/Delete com arquivo travado (EFT aberto) não têm teste. | `Move/Delete: exception in strategy is counted as error and run continues` (injetar `deleteFile: _ => throw new IOException()` e um `MoveToDisabled` sobre destino travado; esperar `Errors>0`, `Cancelled=false`, próximas ações aplicadas). |
| d | **Corrupção do baseline durante Save** (CR-01-04) — `Corrupt_file_yields_empty_baseline` cobre corrupção prévia, mas não a atomicidade da escrita. | Após o fix temp+move: `Save writes via temp file (no partial sync-state.json on interrupted write)`. |
| e | **Reentrância** (CR-01-01) — não há teste do guard na VM. | Difícil em unit puro; ao menos um teste do motor: `Two concurrent ExecuteAsync on the same baseline/report do not corrupt state` documentando a necessidade do guard na camada VM. |
| f | **Dev Mode em `config-server` (MirrorDelete)** — só `plugins` (MirrorMoveDisabled) tem teste de preservação de extra por Dev Mode. | `DevMode preserves extras in mirror-delete folders` (extra em `config-server/` com DevMode ON → `PreserveDevMode`, não `DeleteExtra`). |

Nenhuma dessas lacunas é bloqueante por si; são a rede de segurança que falta para um componente que deleta/move arquivos. Priorizar (a) e (c).

---

## Áreas auditadas e limpas (🟢)

- **`SyncRuleResolver`** — normalização resolve `\`vs`/` e case (lowercase antes de comparar Ordinal); `IsUnderPrefix` é segment-aware (`plugins-disabled` **não** casa `plugins`, testado); longest-prefix por `OrderByDescending(Length)`; override do server sobrepõe fallback; nome de regra inválido é descartado (fallback sobrevive). Sólido.
- **Apply atômico** (`ApplyAtomic`) — temp no mesmo diretório (mesmo volume) → `File.Move(overwrite)`; em falha remove o tmp e re-lança sem mascarar; teste confirma zero `.sync-tmp` órfão e destino intocado.
- **Cancelamento entre arquivos** — `ThrowIfCancellationRequested` antes de cada ação de IO; arquivo em voo termina atômico (A9 documentado: `DownloadModFile` não é abortável mid-transfer); `Pending = ioTotal - ioDone` correto; baseline/report persistidos no `finally`; `manifest_hash.txt` pulado quando `Cancelled` (força rescan). Consistente.
- **Baseline round-trip** — normalização de keys (lowercase + `/`), hash lowercase, `Remove` case/sep-insensível, corrupto/ausente → vazio. Semeadura de `UpToDate` (CC7) é segura por definição (local == server).
- **Colisão em `-disabled`** (`MoveWithOverwrite`) — substitui o backup antigo; **perda do backup anterior é aceita explicitamente** por A5/R3.3 ("versão mais nova vale"). Não é bug, é decisão registrada — mas fica o registro de que é perda de dado consentida.
- **Proteções de extra** (`ScanExtras`) — manifesto completo (opcionais OFF incl., CC3), `ignoredFiles`, `ExcludeFromCleanup`, `protectedPaths` (`GetAllKnownOptionalPaths`), segmento `-disabled` (R3.4), Dev Mode. `handled` HashSet + `.Distinct()` evitam dupla-ação quando roots aninham. `Directory.EnumerateFiles` só pega arquivos (nunca deleta diretório). `config` (PreserveDivergent) nunca tem extra tocado.
- **Consolidação `ManifestFile`** — canônica em `SPT.Launcher.Models.Launcher`; casca `OptionalModsHelper.ManifestFile : ...` preserva o nome aninhado usado pela ProfileViewModel; sem duplicação de campos. Compila.
- **Server `folderRules` pass-through** (`ModUpdater.cs`) — lê objeto prefixo→regra do `config.json`, re-emite no manifesto; `Replace("..","")` no endpoint de download (defesa server-side existe, ao contrário do client — ver CR-01-05); default config só gerado quando ausente.
- **GameStarter** — `SetupGameFiles`/`CleanTempFiles` sem interseção com pastas-regra/`-disabled` (confirmado); baseline sobrevive ao wipe. Zero-mudança justificada.
- **Integração legada preservada** (vs `612f4d8^`, um a um): retry 5×3s ✓, countdown 30s + retry recursivo ✓, `deleteFiles` explícito (lixeira) ✓, população de toggles opcionais ✓, save do `manifest_hash` ✓ (agora pulado se cancelado), `GetAllKnownOptionalPaths` ✓, MD5 via `SyncPathUtil.ComputeMd5` ✓. Mudança de comportamento benigna: extras em `BepInEx/plugins` agora **movem** para `-disabled` (antes: delete) — mais seguro e alinhado ao card. `GetFileMD5`/`DoUpdateMods`/`_filesToUpdate`/`_filesToDelete` removidos corretamente.

## Notas (não-achados, contexto)

- **`user/mods` managedPath deleta mods instalados à mão** — qualquer server-mod que o usuário instale manualmente sob `user/mods/` fora do manifesto vira extra → lixeira no login. É **comportamento legado preservado** (não regressão), mas segue sendo footgun herdado. Fora do escopo deste item.
- **Dois bases de path para estado do launcher** — baseline/report em `SptPathHelper.SptRootPath/user/launcher`, mas `manifest_hash.txt` em `gamePath/SPT/user/launcher`. Se `SptRootPath != gamePath/SPT`, ficam em pastas diferentes. Consistente consigo mesmo (o link abre onde o report É escrito); apenas registrar.
- **`GameVersionCheck`** definido e nunca chamado em ambos (velho e novo) — dead code pré-existente, não introduzido aqui.

---

## Resoluções (2026-07-04, /apply-code-review)

Todos os 6 achados aplicados; marcadores `// ref: CR-01-NN` no código. Gates pós-fix: build launcher **0 Erro(s)** · `dotnet test` **52/52** (39 pré-existentes + 8 novos deste review + 5 do `SyncOverlayTests` do track 008, que compartilha o projeto).

| ID | Resolução |
|---|---|
| CR-01-01 🔴 | `ProfileViewModel`: `CheckForUpdates(manual)` virou ponto de entrada único com gate `Interlocked.CompareExchange(ref _syncGate, 1, 0)`; corpo extraído p/ `CheckForUpdatesCore(manual)` **sem** guard — o retry recursivo (countdown 30s) chama o Core direto e mantém o "lock" do mesmo fluxo lógico. `_syncCts` só é criado dentro do run guardado → sem overwrite por run concorrente. `ForceCheckForUpdates` retorna cedo se `IsSyncRunning` (não deleta `manifest_hash.txt` no meio de um sync). UI: nova propriedade composta `CanVerifyFiles => CanStartGame && !IsSyncRunning` (raise nos dois gatilhos) e o botão VERIFICAR ARQUIVOS na `ProfileView.axaml` passou de `CanStartGame` p/ `CanVerifyFiles`. Edições cirúrgicas (W3 em paralelo no mesmo arquivo — seções de opcionais/overlay intocadas). |
| CR-01-02 🟡 | `SyncPlanner`: checagem `IsIgnored` removida do loop do manifesto (comentário explicativo no lugar); mantida só no `ScanExtras` (semântica legada: protege extra de deleção, nunca bloqueia update). SPT core volta a atualizar. |
| CR-01-03 🟡 | Opção **1** aplicada: `config-server → mirror-delete` removido da `FallbackRules` do `SyncRuleResolver`. **Decisão registrada:** a regra mais destrutiva só ativa via `folderRules` EXPLÍCITO do server (mecanismo já existente e testado) — default seguro até o E2E P-007.2; o operador liga no `Launcher-Updater/config.json` sem rebuild do launcher. Sem folderRules, `config-server` cai em Default (extras intocados, pois não é managedPath). Testes de config-server atualizados p/ ativar a regra via `SyncTestFixture.ResolverWithConfigServerMirror()` (o que também exercita o caminho real de override do server) + teste novo do default seguro. |
| CR-01-04 🟡 | `SyncBaseline.Save` e `SyncReport.Write` agora escrevem via temp+move (`.tmp` + `File.Move(overwrite)`), mesmo padrão dos applies — crash no meio não trunca `sync-state.json`/`last-update.json`. |
| CR-01-05 🟡 | `SyncEngine.ResolveUnderRoot`: `Path.GetFullPath` do destino tem que começar com a raiz resolvida (`OrdinalIgnoreCase`) — aplicado no Download (**antes** de baixar), no DeleteExtra e nos DOIS lados do MoveToDisabled (origem e `-disabled`). Violação → `InvalidOperationException` capturada pelos catches por-arquivo (erro contado + entry no relatório, run continua). |
| CR-01-06 🟡 | 8 testes novos: (a) `Manifest_file_matching_ignored_substring_is_still_downloaded` (trava a regressão do CR-01-02); (b) `Download_path_with_traversal_does_not_escape_game_root` (downloader nem é chamado; nada escrito fora do root); (c) `Delete_failure_mid_run_does_not_abort_remaining_actions` + `Move_failure_on_locked_target_does_not_abort_remaining_actions` (IOException/lock → erro por-arquivo, run segue); (d) `Baseline_save_is_atomic_and_leaves_no_temp_file`; (f) `DevMode_preserves_extras_in_mirror_delete_folders`; + `Mirror_delete_requires_explicit_server_rule` e `ConfigServer_extra_without_explicit_rule_is_untouched` (CR-01-03). **(e) não aplicado**: teste de dois `ExecuteAsync` concorrentes no mesmo baseline seria flaky por natureza (Dictionary não thread-safe é exatamente o que o guard da VM impede); a proteção real é o CR-01-01 — registrado como coberto pela camada VM. |
