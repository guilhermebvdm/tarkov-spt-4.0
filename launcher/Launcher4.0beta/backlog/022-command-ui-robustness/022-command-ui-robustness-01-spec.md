# 022 — Robustez de comandos e thread-safety de UI · Spec funcional

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./022-command-ui-robustness-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

## Objetivo

Endurecer três classes de fragilidade de robustez no launcher (todas identificadas na auditoria de 2026-07-04, severidade 🟡), sem mudar o comportamento observável do caminho feliz:

1. **Confirmação frágil de ações destrutivas** — `WipeConfirmCommand` e `RemoveProfileCommand` só abortam quando o diálogo retorna `false`; qualquer resultado ambíguo (`null`, não-bool, dismiss por ESC/click-away) **prossegue** para o reset/remoção do perfil. Alinhar com o padrão seguro já usado em `DeleteAccountCommand`.
2. **Exceções não observadas em comandos `async Task`** — comandos ligados por nome (`StartGameCommand`, `WipeConfirmCommand`, `RemoveProfileCommand`, `DeleteAccountCommand`, `ChangeEditionCommand`) não são aguardados pelo binding: uma exceção após o primeiro `await` some silenciosamente e pode deixar flags de UI presas (`GameRunning`/`AllowSettings`) → botão **JOGAR** e a tela de **Settings** travados até reiniciar o launcher.
3. **Escrita de propriedades bound fora da UI thread** — todo o `ConnectServer` roda em `Task.Run`; escritas em `connectModel.*` e em `LauncherSettingsProvider.Instance.AllowSettings` disparam `PropertyChanged` na thread de pool, e o `Progress<int>` posta na pool → risco de `InvalidOperationException: Call from invalid thread` em qualquer conexão.

**Correlato (🟢, correção do 013):** footer de versão de `LoginView`/`RegisterView` é `x:Static` read-once — uma falha transitória do fetch de versão congela `"—"` a sessão inteira nessas telas; torná-lo reativo como no `ProfileView`.

Não introduz feature nova, UI nova, nem muda contratos de rede. É trabalho de robustez/segurança-de-runtime.

## Critérios de aceite (testáveis)

### Grupo A — Confirmação de ações destrutivas

- [ ] **AC-022.1** — *Dado* o diálogo de confirmação de wipe, *quando* o usuário confirma (resultado `true`), *então* `WipeProfile` é executado.
- [ ] **AC-022.2** — *Dado* o diálogo de confirmação de wipe, *quando* o usuário nega (`false`) **ou** dispensa o diálogo sem escolher (resultado `null`/não-bool: ESC, click-away, fechamento programático), *então* `WipeProfile` **não** é executado.
- [ ] **AC-022.3** — *Dado* o diálogo de confirmação de remoção de perfil, *quando* o resultado é qualquer coisa diferente de `true` (`false`, `null`, não-bool), *então* `AccountManager.RemoveAsync` **não** é chamado.
- [ ] **AC-022.4** — A regra de decisão passa a ser **"prossegue somente com `true` explícito"** (`result is not bool b || !b` → abortar), idêntica à de `DeleteAccountCommand` (`ProfileViewModel.cs:1104`). Verificável por leitura do diff: nenhum comando destrutivo usa mais o padrão `result is bool b && !b`.

### Grupo B — Robustez de comandos async / flags de UI

- [ ] **AC-022.5** — *Dado* que `StartGameCommand` lança exceção **antes** de `GameRunning=true` (ex.: `LoginAsync` estoura), *então* `AllowSettings` volta a `true` e o usuário recebe uma notificação de erro; a tela de Settings continua acessível.
- [ ] **AC-022.6** — *Dado* que `StartGameCommand` lança exceção **depois** de `GameRunning=true` (ex.: `LaunchGame`/`WipeProfile` estoura), *então* `GameRunning` volta a `false` (→ `CanStartGame=true`, botão **JOGAR** reabilitado) e uma notificação de erro é exibida.
- [ ] **AC-022.7** — *Dado* o caminho feliz de `StartGameCommand` (jogo inicia), *então* `GameRunning` **permanece** `true` (não pode ser zerado por um `finally` cego) e continua sendo resetado só pelo `GameExitCallback`.
- [ ] **AC-022.8** — *Dado* que `WipeConfirmCommand`, `RemoveProfileCommand`, `DeleteAccountCommand` ou `ChangeEditionCommand` lançam exceção após um `await`, *então* a exceção é logada via `LogManager` e o usuário recebe notificação de erro (nada de falha 100% silenciosa).
- [ ] **AC-022.9** — Nenhum comando `async Task` bound por nome deixa exceção pós-`await` cair como *unobserved task exception*: cada um tem tratamento explícito (try/catch ou wrapper guardado).

### Grupo C — Thread-safety do ConnectServer

- [ ] **AC-022.10** — *Dado* que `ConnectServer` roda em `Task.Run` (pool), *quando* atualiza `connectModel.InfoText/ConnectionFailed/IsDownloading/DownloadProgress` ou `LauncherSettingsProvider.Instance.AllowSettings`, *então* a escrita (e o `PropertyChanged` resultante) ocorre na UI thread.
- [ ] **AC-022.11** — *Dado* o fluxo de download da atualização do launcher, *quando* o `Progress<int>` reporta percentuais, *então* o handler que muta propriedades bound roda na UI thread.
- [ ] **AC-022.12** — Fluxo completo de conexão (connect inicial + `RetryCommand`) executa sem lançar `InvalidOperationException` de thread inválida, com toda a UI de progresso atualizando visualmente. *(gate de validação in-game — ver Gates.)*

### Grupo D — Footer de versão reativo (correlato 013)

- [ ] **AC-022.13** — *Dado* que o fetch de versão do server falhou transitoriamente no connect (`ServerManager.TrlServerVersion == "—"`), *quando* o usuário chega em `LoginView`/`RegisterView`, *então* um refetch assíncrono barato atualiza o footer sem congelar `"—"` — mesmo comportamento reativo já existente no `ProfileView` (`ProfileViewModel.cs:226-237`).
- [ ] **AC-022.14** — O footer de `LoginView`/`RegisterView` deixa de usar `x:Static` para `ServerVersion`; passa a bind reativo (`{Binding ServerVersion}`) alimentado pelo respectivo ViewModel.

## Regras de negócio

- **RN-1 — Abortar no ambíguo.** Para qualquer ação destrutiva (wipe, remove, delete, change-edition), a ausência de um `true` **explícito** significa **não executar**. "Sem resposta" nunca é "sim".
- **RN-2 — Flags de UI seguem o estado real.** `GameRunning` só é `true` enquanto o processo do jogo deveria estar rodando; qualquer caminho de falha o retorna a `false`. `AllowSettings` volta a `true` sempre que o comando termina (sucesso ou erro), exceto quando o launcher está de fato reiniciando (auto-update).
- **RN-3 — Falha nunca é silenciosa.** Toda exceção de comando é logada e comunicada ao usuário por notificação. Não é aceitável um clique que "não faz nada" sem rastro.
- **RN-4 — Toda escrita em propriedade bound acontece na UI thread.** Vale para models `INotifyPropertyChanged` (que aqui **não** marshalam sozinhos) e para singletons de estado observados por bindings.

## Corner cases

- **CC-1 — `ChangeEditionCommand` já é seguro na confirmação** (usa positive-match `result is SPTEdition edition`, `ProfileViewModel.cs:1075`); entra no escopo **apenas** pela robustez async (Grupo B), não pela confirmação.
- **CC-2 — `WipeProfile` compartilhado.** É chamado de `StartGameCommand` (que gerencia `GameRunning`) **e** de `WipeConfirmCommand`/`ChangeEditionCommand` (que não gerenciam). O tratamento de erro não pode assumir que há flag de `GameRunning` para restaurar nesses dois últimos.
- **CC-3 — `finally` cego quebra o caminho feliz.** Restaurar `GameRunning=false` num `finally` zeraria o estado de jogo iniciado com sucesso (ver AC-022.7). A restauração de `GameRunning` tem de ser **só no catch**.
- **CC-4 — `NavigateTo` já marshala.** `NavigateTo`/`NavigateBack` usam `Dispatcher.UIThread.InvokeAsync` internamente (`ViewModelBase.cs:106-127`); não precisam de marshalling adicional no ConnectServer.
- **CC-5 — `RetryCommand` reusa o mesmo caminho.** `RetryCommand` (`ConnectServerViewModel.cs:185`) também dispara `Task.Run(ConnectServer)`; o fix de thread-safety tem de cobrir os dois pontos de entrada, não só o `WhenActivated`.
- **CC-6 — Progress capturado no lugar errado.** O `Progress<int>` é hoje construído **dentro** do `Task.Run` (`:107`), capturando o `SynchronizationContext` da pool (nulo) → posta na pool. Construí-lo na UI thread **ou** marshalar dentro do handler.
- **CC-7 — `DeleteAccountCommand` continua não-atômico** (esvazia o cofre antes do remove, `:1112-1118`) — isso é o item **010**, **fora do escopo** deste 022; aqui só se garante que uma exceção nesse fluxo não some.

## Fora de escopo

- Refatorar `DialogHost.Show` para garantir um retorno tipado não-nulo (o fix trata o retorno ambíguo no chamador; hardening do host fica para outra frente).
- Tornar `DeleteAccountCommand`/`RemoveProfileCommand` atômicos (item 010) e o gate de excluir-conta durante sessão coop (item 010).
- Converter os comandos para `ReactiveCommand` em massa (é uma alternativa técnica avaliada na 02, mas não é requisito — o requisito é o comportamento, não o mecanismo).
- Qualquer mudança em `OptionalModsHelper` (thread/atomicidade dos toggles) — pertence a 019/021.
- Migração de DS do `SettingsView` (B3), scrim de auth, e demais achados de design.
- O RCE do auto-update (B1) e o guard do `deleteFiles` (B2) — bloqueadores 🔴 próprios.

## Gates humanos

> Regra do projeto: escrita em arquivos SPT / comportamento de runtime precisa de **validação no jogo**, não só build verde.

- [ ] **GH-022.1 — Validação in-game do fluxo de conexão (Grupo C).** Rodar o launcher real conectando ao servidor Fika (com e sem update pendente do launcher) e confirmar: barra de progresso e textos atualizam, **nenhum** `InvalidOperationException` de thread nos logs, `RetryCommand` idem. Solo=host mascara: validar também num **cliente extra** (não-host).
- [ ] **GH-022.2 — Validação in-game das flags travadas (Grupo B).** Forçar uma falha de `LaunchGame` (ex.: GamePath inválido/EXE ausente) e confirmar que **JOGAR** volta a ficar clicável e Settings acessível sem reiniciar o launcher; repetir forçando falha antes do login (rede fora) e confirmar Settings destravado.
- [ ] **GH-022.3 — Confirmação destrutiva por dismiss (Grupo A).** No diálogo de wipe e no de remove, dispensar via **ESC** e **click-away** e confirmar que **nada** é resetado/removido; confirmar que o "Sim" ainda funciona.
- [ ] **GH-022.4 — Coop:** validar num cliente conectado a uma sessão coop ativa que um erro transitório de re-login/lançamento **não** deixa o cliente com JOGAR travado (bloquearia rejoin sem reiniciar).
- [ ] **GH-022.5 — Regressão de footer (Grupo D):** simular fetch de versão falho no connect e confirmar que Login/Register recuperam a versão (não ficam presos em `"—"`).

## Decisões que precisam do humano (produto)

- **PD-022.1 — UX de erro dos comandos.** Ao capturar exceção inesperada em `StartGameCommand`/wipe/delete, qual a mensagem e o destino? Proposta: notificação de erro genérica localizada + permanecer na tela (sem navegar), restaurando flags. Confirmar texto/tom.
- **PD-022.2 — Ownership do footer reativo (Grupo D) × item 013.** O correlato toca `LoginView.axaml`/`RegisterView.axaml` e os VMs de Login/Register — arquivos que o **013 (versão server dinâmica)** também mexe. Decidir: entregar no 022 (coordenando merge) ou delegar ao 013. Default proposto: entregar no 022 por já estar na superfície auditada, sinalizando a sobreposição.
- **PD-022.3 — "Abortar no ambíguo" como regra global.** Confirmar que tratar `null`/não-bool como **não-confirmação** é o comportamento desejado para todas as ações destrutivas (é o default seguro; hoje só é latente porque o diálogo sempre devolve bool).

## Gates de build

`dotnet build SPT.Launcher.csproj -c Release` · `dotnet test SPT.Launcher.Tests.csproj -c Release` — verdes. Nunca rodar o exe (a validação de runtime é manual, ver Gates humanos).
