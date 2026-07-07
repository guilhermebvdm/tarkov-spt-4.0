# 022 — Robustez de comandos e thread-safety de UI · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./022-command-ui-robustness-00-kickoff.md) · [01-spec](./022-command-ui-robustness-01-spec.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Contexto de código (confirmado por leitura)

**Como os comandos são ligados.** Em `ProfileView.axaml` os comandos são `Command="{Binding StartGameCommand}"` (`:223`), `{Binding WipeConfirmCommand}` (`:155`), `{Binding RemoveProfileCommand}` (não bound direto na view atual, mas público), `{Binding DeleteAccountCommand}` (`:161`), `{Binding ChangeEditionCommand}` (`:149`). São **métodos `async Task`** bound por nome — o Avalonia gera um `ICommand` que **invoca sem `await`**. Consequência: a `Task` retornada é descartada; exceção após o 1º `await` vira *unobserved* (só sobe no finalizer via `TaskScheduler.UnobservedTaskException`) → falha silenciosa e flags presas.

**Modelo de notificação não marshala.** `NotifyPropertyChangedBase.SetProperty` (`SPT.Launcher.Base/Utilities/NotifyPropertyChangedBase.cs:17-25`) chama `RaisePropertyChanged` → `PropertyChanged?.Invoke(...)` **síncrono na thread chamadora**. `ConnectServerModel` (`:13`) e `LauncherSettingsProvider` herdam esse comportamento. Logo, escrever essas props de dentro de um `Task.Run` dispara os bindings fora da UI thread.

**Flags e sua semântica.** `LauncherSettingsProvider.CanStartGame => !GameRunning && !IsUpdating` (`LauncherSettingsProvider.cs:182`). `GameRunning`/`IsUpdating` re-raise `CanStartGame` (`:165,:177`). `ProfileViewModel.CanStartGame` espelha o singleton (`:298`) e re-raise no `PropertyChanged` do provider (`:217-224`). Portanto, `GameRunning` preso em `true` desabilita **JOGAR** (`IsEnabled="{Binding CanStartGame}"`, `ProfileView.axaml:223`).

## Achados com file:line reais

| # | Onde | Trecho atual | Problema |
|---|---|---|---|
| A1 | `ProfileViewModel.cs:1088` (`WipeConfirmCommand`, 1081-1091) | `if (result is bool b && !b) return;` | `null`/não-bool prossegue p/ `WipeProfile` |
| A2 | `ProfileViewModel.cs:1182` (`RemoveProfileCommand`, 1176-1211) | `if (result is bool b && !b) return;` | idem p/ `RemoveAsync` |
| A0 (padrão-alvo) | `ProfileViewModel.cs:1104` (`DeleteAccountCommand`) | `if (result is not bool confirmed \|\| !confirmed) return;` | **correto** — alinhar A1/A2 a este |
| B1 | `ProfileViewModel.cs:960-1019` (`StartGameCommand`) | `AllowSettings=false`(:962) → `await LoginAsync`(:965) → `AllowSettings=true`(:968) → `GameRunning=true`(:977) → `await WipeProfile`(:981)/`await LaunchGame`(:992) | sem try/catch: exceção pré-968 trava `AllowSettings=false`; pós-977 trava `GameRunning=true` |
| B2 | `ProfileViewModel.cs:1081,1097,1176,1071` | Wipe/Delete/Remove/ChangeEdition `async Task` sem try/catch | exceção pós-`await` some (sem log, sem toast) |
| C1 | `ConnectServerViewModel.cs:30-36` + `:39` | `WhenActivated → Task.Run(ConnectServer)` | todo `ConnectServer` na pool |
| C2 | `ConnectServerViewModel.cs:43,49,57,95-97,109-111,143-147,175,180-181` | `connectModel.* =` e `LauncherSettingsProvider.Instance.AllowSettings =` | escritas bound na pool → `PropertyChanged` fora da UI thread |
| C3 | `ConnectServerViewModel.cs:107-112` | `new Progress<int>(...)` construído dentro do `Task.Run` | captura SyncContext da pool → handler posta na pool |
| C4 | `ConnectServerViewModel.cs:185-195` (`RetryCommand`) | `Task.Run(ConnectServer)` | 2º ponto de entrada, mesmo problema |
| D1 | `LoginView.axaml:63-65`, `RegisterView.axaml:62-64` | `ServerVersion="{x:Static base:ServerManager.TrlServerVersion}"` | read-once; `"—"` transitório congela a sessão |
| D0 (padrão-alvo) | `ProfileViewModel.cs:226-237` | refetch async + `Dispatcher.UIThread.Post(() => ServerVersion = refreshed)` | pattern reativo a replicar |

## Abordagem

### Grupo A — confirmação (A1, A2)

Trocar, em `WipeConfirmCommand` (`:1088`) e `RemoveProfileCommand` (`:1182`):

```csharp
// antes
if (result is bool b && !b) return;
// depois (idêntico a DeleteAccountCommand:1104)
if (result is not bool b || !b) return;
```

Diff mínimo, sem tocar assinatura nem binding. Cobre AC-022.1..4.

### Grupo B — robustez async + flags (B1, B2)

Introduzir **um helper guardado** em `ProfileViewModel` e manter os nomes públicos dos comandos (zero mudança de binding, minimiza conflito no hub):

```csharp
private async Task GuardedAsync(Func<Task> body, string context, Action onError = null)
{
    try { await body(); }
    catch (Exception ex)
    {
        LogManager.Instance.Error($"[Profile] {context}: {ex.Message}\n{ex.StackTrace}");
        onError?.Invoke();
        SendNotification("", LocalizationProvider.Instance./*<erro genérico>*/..., NotificationType.Error);
    }
}
```

Refatorar cada comando para `public async Task XCommand() => await GuardedAsync(XCore, "...", onError);`, movendo o corpo atual para um `private async Task XCore()`.

- **`StartGameCommand`** → `onError: () => { LauncherSettingsProvider.Instance.GameRunning = false; LauncherSettingsProvider.Instance.AllowSettings = true; }`. Restauração **só no catch** (CC-3/AC-022.7): o caminho feliz preserva `GameRunning=true`, resetado apenas por `GameExitCallback` (`:1243`). Isso cobre B1 nos dois pontos (pré-968 restaura `AllowSettings`; pós-977 restaura `GameRunning`).
- **`WipeConfirmCommand`/`RemoveProfileCommand`/`DeleteAccountCommand`/`ChangeEditionCommand`** → `onError: null` (não gerenciam `GameRunning`; CC-2). Só log + toast.

**Alternativa avaliada (não escolhida):** converter para `ReactiveCommand.CreateFromTask` com `ThrownExceptions.Subscribe(...)` (padrão já usado em `UpdateModsCommand`/`VerifyFilesCommand`/`CancelUpdateCommand`, `:211-213`). Mais idiomático, mas troca o binding de método→`ICommand` property e mexe em mais linhas de um arquivo compartilhado por 019-023 → maior risco de conflito. Fica registrado como opção; o requisito é comportamental.

### Grupo C — thread-safety do ConnectServer (C1-C4)

Manter o `Task.Run` (há chamadas **bloqueantes** — `ServerManager.PingServer()` síncrono `:153`, `LoadDefaultServerAsync`, `TailscaleHelper.*` — que não podem ir para a UI thread), e **marshalar as mutações de estado bound**. Adicionar helper local:

```csharp
private static void OnUi(Action a) => Dispatcher.UIThread.Post(a);
```

- Envolver cada escrita de `connectModel.*` e de `LauncherSettingsProvider.Instance.AllowSettings` em `OnUi(() => ...)`. Pontos: `:43,49,57(*),95-97,104,109-111,117,126,135,143-147,158,175,180-181`. *(`:57`/`:64` gravam `Server.Url` — modelo não bound diretamente; marshalar por segurança se observado; confirmar no diff.)*
- **C3:** construir o `Progress<int>` **na UI thread** (ou fazer o handler postar): trocar por handler que já usa `OnUi`. Como `Progress<T>` captura o contexto na construção, a via mais simples aqui é o handler chamar `OnUi(() => { connectModel.IsDownloading = true; ... })`.
- **C4:** `RetryCommand` chama o mesmo `ConnectServer`, então herda o fix; suas próprias escritas (`:187-189`) rodam na UI thread (invocação de comando) — deixar como estão ou padronizar via `OnUi` (idempotente).
- `NavigateTo` (`:171`, `:973`, `:1037`…) já marshala (`ViewModelBase.cs:112`) — não mexer (CC-4).

### Grupo D — footer reativo (D1) — correlato 013

Replicar o pattern do `ProfileViewModel:226-237` em `LoginViewModel` (ctor `:41`) e `RegisterViewModel` (ctor `:48`):

- Adicionar propriedade reativa `public string ServerVersion { get; }` (RaiseAndSetIfChanged) inicializada com `ServerManager.TrlServerVersion`; se `== "—"`, `Task.Run(() => { var v = ServerManager.RefreshTrlServerVersionIfUnknown(); Dispatcher.UIThread.Post(() => ServerVersion = v); })`. `LauncherVersion` é constante (`LauncherUpdateHelper.CurrentVersion`) → pode continuar `x:Static`.
- Em `LoginView.axaml:65` e `RegisterView.axaml:64`, trocar `ServerVersion="{x:Static base:ServerManager.TrlServerVersion}"` por `ServerVersion="{Binding ServerVersion}"`.

`ServerManager.RefreshTrlServerVersionIfUnknown()` (`ServerManager.cs:53`) e o default `"—"` (`:24`) já existem — sem mudança no Base.

## Arquivos a tocar

| Arquivo | Grupo | Mudança |
|---|---|---|
| `ViewModels/ProfileViewModel.cs` | A, B | 2 guards de confirmação; helper `GuardedAsync`; wrap dos 5 comandos + split `*Core` |
| `ViewModels/ConnectServerViewModel.cs` | C | helper `OnUi`; marshalling das escritas bound; Progress na UI |
| `ViewModels/LoginViewModel.cs` | D | prop reativa `ServerVersion` + refetch |
| `ViewModels/RegisterViewModel.cs` | D | idem |
| `Views/LoginView.axaml` | D | binding do footer (`:65`) |
| `Views/RegisterView.axaml` | D | binding do footer (`:64`) |
| `SPT.Launcher.Tests/…` | testes | novo teste do predicado de confirmação (ver Plano de teste) |

Sem mudanças no `SPT.Launcher.Base` além de leitura (D reusa API existente). Sem mudanças de contrato de rede/DTO.

## Contratos / DTOs

Nenhum contrato novo. `GuardedAsync(Func<Task> body, string context, Action onError = null)` e `OnUi(Action)` são helpers internos. Opcional (recomendado p/ testabilidade): extrair o predicado puro

```csharp
internal static bool IsConfirmed(object result) => result is bool b && b;
```

para uma classe estática (ex.: `Helpers/DialogResult.cs`) e usar `if (!DialogResult.IsConfirmed(result)) return;` — isola a regra "abortar no ambíguo" num ponto testável por unit test (o resto é UI-bound e não unit-testável sem harness de dispatcher).

## Riscos

- **R1 — `finally` cego zera jogo iniciado.** Mitigado: restauração de `GameRunning` **só no `onError`/catch** (AC-022.7). Cobrir com GH-022.2 (forçar falha) + inspeção do caminho feliz.
- **R2 — Marshalling incompleto no ConnectServer.** Se sobrar uma escrita bound fora do `OnUi`, o `InvalidOperationException` pode reaparecer só sob timing específico (heisenbug). Mitigar com varredura linha-a-linha das props de `connectModel`/`AllowSettings` e teste in-game em cliente extra (GH-022.1).
- **R3 — `Dispatcher.UIThread.Post` sem dispatcher (contexto de teste/headless).** Em produção o dispatcher existe; em teste evitar exercitar esses caminhos (foco unit no predicado puro).
- **R4 — Deadlock por marshalling errado.** Usar `Post` (fire-and-forget, não bloqueia a pool) e **nunca** `InvokeAsync(...).Wait()` dentro do `Task.Run` (bloquearia). `Post` é o correto aqui.
- **R5 — Conflito de merge no hub `ProfileViewModel.cs`.** Ver nota de paralelismo; manter diffs localizados e nomes de comando estáveis.
- **R6 (coop) — flag presa em cliente.** Um `GameRunning` preso num **cliente** (não-host) bloqueia rejoin sem reiniciar o launcher — solo=host mascara. GH-022.4 valida no cliente. O fix B beneficia diretamente o coop.

## Plano de teste

**Unit (xUnit, `SPT.Launcher.Tests/`).** O projeto hoje só tem `Sync/*` (sem testes de VM; `ProfileViewModel` depende de estáticos `AccountManager`/`LauncherSettingsProvider.Instance`/`DialogHost`/`ServerManager` → não instanciável em teste sem refactor amplo). Escopo unit viável:

- `Commands/DialogResultTests.cs` (novo) sobre o predicado extraído `IsConfirmed`: `true→true`; `false→false`; `null→false`; `"x"/0/objeto→false`. Cobre AC-022.1..4 de forma determinística.

**Fora do unit (por design).** Restauração de flags (Grupo B), marshalling (Grupo C) e footer reativo (Grupo D) são UI-bound / dependem de dispatcher e de I/O real → verificados por **inspeção de diff** + **gates humanos in-game** (GH-022.1..5). Não forçar mocks frágeis de dispatcher.

**Gates de build.** `dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` verdes. Nunca rodar o exe.

## Nota de paralelismo

- **`ProfileViewModel.cs` — hub de 019-023.** É o arquivo mais disputado do backlog. Este item toca: guards de confirmação (`:1088`,`:1182`), `StartGameCommand` (`:960-1019`) e os comandos wipe/delete/remove/change-edition. **Não** toca `OnOptionalToggled`/optionals (019/021), `ForceCheckForUpdates`/sync (007/023), nem o bloco de `ServerVersion` já existente (`:226-237`). Recomenda-se sequenciar 022 junto ou logo após os demais itens que editam este arquivo para reduzir conflito; diffs localizados e nomes de comando estáveis.
- **`ConnectServerViewModel.cs` — compartilhado com 006** (bypass Dev Mode / VPN, `:85-89`). 022 mexe em marshalling de escritas bound; 006 mexe na lógica de branch Dev Mode. Áreas distintas do mesmo método `ConnectServer` → coordenar merge.
- **`LoginView.axaml` / `RegisterView.axaml` / `LoginViewModel.cs` / `RegisterViewModel.cs` — compartilhados com 013** (versão server dinâmica). O footer reativo (Grupo D) é literalmente o correlato do 013; **decisão de ownership pendente (PD-022.2)**. Se 013 for entregue antes, o Grupo D pode virar no-op.
- **Não compartilha** `OptionalModsHelper` (019/021) nem `Legacy.axaml` (024/025) — este item não os toca.
