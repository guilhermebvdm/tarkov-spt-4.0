# 022 — Robustez de comandos e thread-safety de UI · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) (código-client) · **Severidade:** 🟡 · **Deps:** —

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Achados
- **Confirmação frágil** (`ProfileViewModel.cs:1088,1182`): `WipeConfirmCommand`/`RemoveProfileCommand` usam `if (result is bool b && !b) return;` → `null`/não-bool **prossegue** para o wipe/remove. Trap latente (ESC/click-away passa a resetar/remover sem confirmar). Alinhar com `DeleteAccountCommand` (`is not bool ... || !...`).
- **`async Task` commands** (`StartGameCommand` `:960`, Wipe/Delete/ChangeEdition): exceção após o 1º `await` some (unobserved) e deixa flags presas (`GameRunning`/`AllowSettings`) → JOGAR/settings travados até reiniciar. Envolver em try/catch que restaura o estado (ou `ReactiveCommand` com `ThrownExceptions` observado).
- **ConnectServer fora da UI thread** (`ConnectServerViewModel.cs:32,39,107`): todo o `ConnectServer` roda em `Task.Run`; `SetProperty` dispara `PropertyChanged` sem marshalling e `Progress<int>` posta na pool → risco de `InvalidOperationException: from invalid thread`. Marshalizar via `Dispatcher.UIThread.Post`.

## Correlato 🟢
Footer de versão do Login/Register é `x:Static` read-once (`LoginView.axaml:62`) → falha transitória do 013 congela `"—"` a sessão inteira; tornar reativo como no ProfileView.

## Critérios de aceite (seed)
- Nenhum comando destrutivo prossegue sem confirmação booleana **explícita**.
- Exceções em comandos async não deixam flags de UI presas.
- Escritas de propriedades bound sempre na UI thread.
