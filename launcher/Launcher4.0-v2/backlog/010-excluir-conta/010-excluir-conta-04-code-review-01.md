# 010 — Botão "Excluir conta" · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Commit revisado:** `50cfce1` · **Insumos:** [01-spec](./010-excluir-conta-01-spec.md) · [05-asbuild](./010-excluir-conta-05-asbuild.md)

> Review de contexto limpo (revisor não escreveu o código). Escopo: fluxo de exclusão (`DeleteAccountDialogViewModel/View`, `ProfileViewModel.DeleteAccountCommand`), restyle da ProfileView e a adoção do `TrlDialogChrome` pelos 5 dialogs (parte visual/contratos de resultado; a metade server do 005L está no review próprio). Build gate: `dotnet build SPT.Launcher.csproj` → **0 erros**. Nota: working tree tem WIP do item 007 em `ProfileViewModel.cs`/`ProfileView.axaml`; leitura feita no estado exato do commit via `git show 50cfce1:`.

**Placar:** 0 🔴 · 4 🟡 · 3 🟢

---

## Fluxo de exclusão

### CR-01-01 [🟡] Excluir conta deixa a senha órfã no cofre do server — username reciclado herda a senha do dono anterior

`DeleteAccountCommand` → `AccountManager.RemoveAsync()` → `POST /launcher/profile/remove` (SPT core) remove o profile, mas **nada limpa a entrada em `user/profiles/redline_passwords.json`**. Cenário concreto: A exclui a conta `foo` (cofre mantém `"foo": "senhaDeA"`). B registra `foo` com senha própria. No fluxo atual o seed do 004L (`ClassSelectionViewModel.cs:130`) regrava o cofre logo após o registro — passa no gate D2 por construção (o `request.password` é a senha injetada do cofre no login pós-registro). **Mas** se B fechar o launcher entre o registro e o seed, ou o seed falhar (o plano do 004L é "falha → notificar e seguir"), o próximo login de B exige `senhaDeA` na validação client-side (`LoginViewModel.cs:58-66`) → B travado sem saber por quê; e A conhece a senha efetiva da conta de B (a injeção do `/redline/profile/get` devolve a senha órfã). **Fix launcher-only (2 linhas):** em `DeleteAccountCommand`, após o `confirmed` e **antes** do `RemoveAsync`, chamar `await AccountManager.ChangePasswordAsync("")` best-effort (gate passa — envia a senha atual; cofre esvazia; falha não bloqueia a exclusão). Alternativa server: limpar a entrada do cofre quando o profile some (registrado como CR-01-04 no review do 005L).

### CR-01-02 [🟡] `LastUsername`/`LastPassword` não são limpos — LoginView renasce pré-preenchida com credenciais da conta excluída

`DeleteAccountCommand` limpa `AutoLoginCreds` (correto e na ordem certa), mas se `RememberUsername` estiver ativo, `LauncherSettingsProvider.LastUsername/LastPassword` continuam com a conta morta; o construtor da LoginView (`LoginViewModel.cs:236-241`) pré-preenche os campos com ela → clique em entrar → `login_failed` sem contexto. **Fix:** no caso `OK`, limpar também `LastUsername`/`LastPassword` antes do `SaveSettings()` (que já é chamado).

### CR-01-03 [🟡] Gate `CanStartGame` só no EXCLUIR — RESETAR PROGRESSO (WIPE) e MUDAR EDIÇÃO seguem clicáveis com o jogo rodando

`ProfileView.axaml:138-151` (commit): só o botão novo ganhou `IsEnabled="{Binding CanStartGame}"`. Cenário: jogo em execução (`GameRunning=true` → `CanStartGame=false` — verificado que o provider notifica e o VM re-emite, `LauncherSettingsProvider.cs:159-182` + `ProfileViewModel.cs:147-153`), EXCLUIR fica cinza, mas WIPE dispara `WipeProfile` = **RemoveAsync + RegisterAsync no meio da sessão** — mais destrutivo do que o próprio delete que foi bloqueado. Pré-existente (o restyle não piorou), mas o item que introduziu o gate deixou os irmãos destrutivos descobertos e a inconsistência agora é visível lado a lado. **Fix:** mesmo `IsEnabled` nos botões WIPE e MUDAR EDIÇÃO.

### CR-01-06 [🟢 menor] Trim assimétrico no match de confirmação

`DeleteAccountDialogViewModel.cs:26`: `TypedUsername?.Trim() == Username` — o alvo não é trimado. Se algum registro legado tiver username com espaço nas pontas, o match nunca fecha e a conta fica inexcluível pelo dialog. Edge improvável; opcional `== Username?.Trim()`.

**Verificações limpas do fluxo:**
- **Comparação do username:** case-sensitive com `Trim()` só no digitado, conforme spec; o dialog exibe o alvo exato em `trl-mono` + watermark — e o alvo é `AccountManager.SelectedAccount.username` (o **mesmo** campo que o `RemoveAsync` usa), não o `ProfileInfo.Nickname` da barra inferior. Sem risco de digitar o nickname certo e excluir a conta errada.
- **`RequestRemove` false vs exceção:** `Remove()` (`AccountManager.cs:159-186`) — `false` → `UpdateFailed` → notificação e permanece na tela, conta intacta; exceção → `NoConnection` → notificação + `ConnectServerView`. Ambos os braços do `DeleteAccountCommand` conferem com a spec.
- **Pós-OK, ordem à prova de falha:** `AutoLoginCreds = null` + `SaveSettings()` acontecem **antes** do `Logout()` e da navegação — se a navegação falhar, o auto-login já está morto. `Logout()` após `Remove()` é idempotente (`SelectedAccount = null` duas vezes, sem NRE).
- **Resultado do dialog:** `result is not bool confirmed || !confirmed` — null (✕ sem parâmetro não existe aqui; todos os fechamentos mandam `Boolean` explícito) e qualquer não-bool caem em cancelar. É o padrão null-safe que os callers legados deveriam usar (ver CR-01-05).
- **Reentrada/duplo-clique:** comando é método `async Task` via method-binding do Avalonia — o controle fica desabilitado enquanto a Task retornada está em voo (mesmo mecanismo dos comandos pré-existentes `WipeConfirmCommand`/`ChangeEditionCommand`). Segundo `DialogHost.Show` concorrente não é alcançável por duplo-clique; validar no runtime junto com o resto.

---

## Restyle da ProfileView

### CR-01-04 [🟡] Asbuild afirma overlay `TrlPhotoOverlayBrush` sobre o BG — não existe no XAML do commit

`010-excluir-conta-05-asbuild.md`: "Overlay `TrlPhotoOverlayBrush` adicionado sobre o BG (padrão das views já migradas)". No `ProfileView.axaml` do commit (linhas 96-102) o `Image` de fundo é seguido direto pelo Grid de conteúdo — sem `Border`/`Rectangle` com o brush. Login/Register/ClassSelection têm o overlay; a ProfileView não. Consequência real: textos `trl-muted`/`trl-faint` da barra de update flutuam sobre a foto sem o escurecimento padronizado → legibilidade dependente da imagem. **Fix:** adicionar o overlay (1 elemento entre o Image e o Grid) — ou corrigir o asbuild se a omissão foi intencional.

**Verificações limpas do restyle:**
- **Bindings/commands 1:1 com o VM (sem órfãos):** `OpenSettingsCommand`, `OpenKofiCommand`, `LogoutCommand`, `ChangeEditionCommand`, `WipeConfirmCommand`, `DeleteAccountCommand` (novo), `VerifyFilesCommand`, `StartGameCommand`, `OptionalMods`, `ServerVersion`, `IsUpdateVisible`/`UpdateStatusText`/`UpdateProgress`/`UpdateMaxProgress`, `ProfileInfo.*`, `CurrentId`, `SideImage`, `Background`, `CanStartGame` — todos existem no `ProfileViewModel` do commit. Handler code-behind `CopyIdToClipboard` presente e delega a `vm.CopyCommand`.
- **2 botões da sidebar → `OpenSettingsCommand`:** bug pré-existente (LAUNCHER e CONFIGURAÇÕES apontam pro mesmo comando) preservado como estava, documentado no asbuild — não piorou.
- **ProgressBar de XP:** segue bound (`ProfileInfo.XPLevelProgress`, Maximum 100); a de update segue bound a `UpdateProgress`/`UpdateMaxProgress`. Overrides de cor removidos → tema `ProgressBar.axaml` assume.
- **Classes/tokens usados existem no tema do commit:** `Button.sm/.primary/.danger/.ghost/.icon` (Button.axaml), `trl-label/mono/accent/muted/faint/danger` (Text.axaml), `TrlText2Xs/Xs/Sm/Md/Lg` (Typography.axaml), brushes `TrlBgHover/BgActive/FgMuted/BgPanel/Edge/Accent` (Tokens.axaml). `Button.nav` local é token-pure. `TrlPanel`/`TrlSidebarNav` têm ControlTheme registrado.
- **`Json.Deserialize<bool>` do remove:** resposta SPT core zlib "true"/"false" — caminho pré-existente inalterado.

---

## Dialogs → TrlDialogChrome

### CR-01-05 [🟢] Padrão legado `result is bool b && !b` interpreta null como confirmado — hoje inalcançável, mas frágil

Callers: `WipeConfirmCommand` (`ProfileViewModel.cs:819` no commit), `RemoveProfileCommand` (`:896`), `GameStarterFrontend.cs:36` (variante `result != null && ... && !confirmation`). Com null eles **prosseguem** — no caso do wipe, prosseguem para `WipeProfile`. Hoje nenhum caminho de UI do `ConfirmationDialogView` produz null: ✕ e RECUSAR mandam `s:Boolean False` explícito, CONFIRMAR manda True, e o `DialogHost` do MainWindow não define `CloseOnClickAway` (default false, DialogHost.Avalonia 0.8.0, sem fechamento por Esc embutido). Mas o invariante mora a um `CloseOnClickAway="True"` de distância de virar **wipe sem confirmação**. O `DeleteAccountCommand` já usa o padrão seguro. **Fix barato:** inverter os três para `if (result is not bool b || !b) return;`.

### CR-01-07 [🟢 menor] RegisterDialog é código morto — restyle aplicado a dialog sem caller

`RegisterDialogViewModel` não tem nenhum call site no commit (grep: só a classe e a view; o fluxo de registro real é RegisterView → ClassSelection). O ✕-retorna-null ali é contrato não-exercitado. Sem dano; candidato à limpeza D5 do 005L.

**Verificações limpas dos dialogs:**
- **DataContext/commands preservados:** os 5 restyles são só XAML; todos os bindings (`Question`, `AllowConfirm`, `ConfirmButtonText`, `Title/Message/Password/ConfirmPassword/CanConfirm`, `editions.*`, `WarningMessage/ButtonText`) existem nos VMs intactos. `TrlDialogChrome.Title="{Binding Title}"` no CreatePassword funciona (StyledProperty + DataContext herdado).
- **Semântica do ✕ por dialog, conferida contra os callers:** Confirmation → `False` explícito (obrigatório — ver CR-01-05); CreatePassword → null → `LoginViewModel.cs:74` checa `is string` → cancela/logout ✔; ChangeEdition → null → `ChangeEditionCommand` checa `is SPTEdition` → no-op ✔; Warning → resultado ignorado pelos callers ✔; DeleteAccount → `False` explícito ✔. No `GameStarterFrontend`, ✕=False significa "recusar patch" → `TaskCanceledException` — semântica correta.
- **ChangeEdition `.danger` + aviso de wipe:** confirmar dispara `WipeProfile` e o texto exibido é `wipe_warning` = "Alterar a edição da conta exige o Wipe do perfil. O progresso será resetado." — o usuário é avisado. Confirmar sem edição selecionada → parâmetro null → tratado como cancelar (pré-existente).
- **`HeaderContent` no chrome:** propriedade aditiva com default null — usos existentes do `TrlDialogChrome` sem ✕ renderizam DockPanel com presenter vazio, sem quebra.

---

## Resoluções (2026-07-04, /apply-code-review)

| CR | Resolução |
|---|---|
| CR-01-01 🟡 | **Aplicado** — `ProfileViewModel.DeleteAccountCommand`: `await AccountManager.ChangePasswordAsync("")` best-effort ANTES do `RemoveAsync` (esvazia o cofre; gate D2 passa por construção). Falha só loga warning e segue com a exclusão. `// ref: CR-01-01` no código. |
| CR-01-02 🟡 | **Aplicado** — no braço `OK`, `LastUsername = ""` e `LastPassword = ""` antes do `SaveSettings()` já existente. `// ref: CR-01-02`. |
| CR-01-03 🟡 | **Aplicado** — `IsEnabled="{Binding CanStartGame}"` também em RESETAR PROGRESSO (WIPE) e MUDAR EDIÇÃO DO PERFIL (`ProfileView.axaml`, comentários `ref: CR-01-03`). |
| CR-01-04 🟡 | **Aplicado (doc only)** — claim falsa do overlay removida do asbuild; overlay segue ausente por decisão (registrado lá como pendência visual da validação em runtime). |
| CR-01-05 🟢 | Não endereçado nesta rodada (padrão legado em 3 call sites pré-existentes; hoje inalcançável — todos os fechamentos do ConfirmationDialog mandam `Boolean` explícito). Fica como candidato de limpeza. |
| CR-01-06 🟢 | Não endereçado (edge improvável; comportamento conforme spec). |
| CR-01-07 🟢 | Não endereçado (RegisterDialog sem caller — candidato à limpeza futura). |

Gates: build launcher **0 erros** · `dotnet test` **52/52** · build TarkovRedLine.Server **0 erros**.
