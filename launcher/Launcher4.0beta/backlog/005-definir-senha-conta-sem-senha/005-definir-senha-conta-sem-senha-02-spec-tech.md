# 005 — Definir senha em conta sem senha · Spec técnica (auditoria)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Kickoff:** [005-definir-senha-conta-sem-senha-00-kickoff.md](./005-definir-senha-conta-sem-senha-00-kickoff.md)

> Auditoria READ-ONLY do fluxo existente. Nenhum código alterado neste item. Caminhos de launcher relativos a `launcher/Launcher4.0beta/project/`, de server relativos a `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/`.

## 1. Traçado do fluxo

### 1.1 Onde o dialog dispara — detecção de "conta sem senha"

Fluxo do botão Login (`SPT.Launcher/ViewModels/LoginViewModel.cs`, `LoginCommand`):

1. `LoginViewModel.cs:51` → `AccountManager.LoginAsync(Login)`.
2. `SPT.Launcher.Base/Controllers/AccountManager.cs:62` → `RequestHandler.RequestLogin()` = `POST /launcher/profile/login` (SPT core). **O core NÃO valida senha** — `LauncherController.Login` compara apenas `Username` (`references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs:82-94`). Qualquer senha "loga".
3. `AccountManager.cs:69` → `RequestHandler.RequestAccount()` = `POST /redline/profile/get` (`RequestHandler.cs:53`) → devolve o bloco `info` do profile **com a senha injetada do cofre** (ver §1.2) → vira `SelectedAccount` (`AccountInfo` com campo `password`, `SPT.Launcher.Base/Models/SPT/AccountInfo.cs:16`).
4. De volta ao `LoginViewModel`, com `status == OK`:
   - `LoginViewModel.cs:58` — `storedPassword = SelectedAccount?.password ?? ""`.
   - `LoginViewModel.cs:61` — **validação de senha é client-side**: `storedPassword` não-vazia e diferente da digitada → `Logout()` + notificação `incorrect_login`.
   - `LoginViewModel.cs:69` — **condição "conta sem senha"**: `string.IsNullOrEmpty(storedPassword)` → instancia `CreatePasswordDialogViewModel` e `await ShowDialog(...)` (`LoginViewModel.cs:71-72`).
5. Resultado do dialog (`LoginViewModel.cs:74`): `result is string` não-branca → `AccountManager.ChangePasswordAsync(newPassword)`; sucesso → `Login.Password = newPassword` + segue o login. Falha → notificação + `Logout()`. Null/vazio (cancelou) → `Logout()` + permanece na tela de login (`LoginViewModel.cs:90-94`).

O dialog (`SPT.Launcher/Views/Dialogs/CreatePasswordDialogView.axaml:47-55`) confirma via `DialogHost.CloseDialogCommand` com `CommandParameter={Binding Password}`; Cancelar fecha sem parâmetro (retorna null). Gate do botão Confirmar: `CanConfirm` = senha ≥ 3 chars e igual à confirmação (`CreatePasswordDialogViewModel.cs:38-40`).

### 1.2 Para onde a senha vai — server

`AccountManager.ChangePassword` (`AccountManager.cs:235-265`) monta `ChangeRequestData(username, senhaAtual, novaSenha)` → `RequestHandler.RequestChangePassword` = `POST /redline/password/change` (`RequestHandler.cs:71-74`) → `Controllers/PasswordController.cs:31` (`ChangePassword`). O controller:

- **Valida**: apenas `username` não-vazio e `change != null` (`PasswordController.cs:33`). `change == ""` é aceito (usado de propósito pelo reset via HWID, `LoginViewModel.cs:196`). **A senha atual (`request.password`) é ignorada** — ver D2.
- **Persiste em 3 lugares** (match de username **case-insensitive** no arquivo, `PasswordController.cs:69`):
  1. `user/profiles/<id>.json` → `info.password` (`PasswordController.cs:72-75`);
  2. **cofre** `user/profiles/redline_passwords.json` → `vault[username] = senha` (`PasswordController.cs:80-87`) — existe porque o `Info` do SPT 4.0 não tem campo `Password` (`references/.../Models/Eft/Profile/SptProfile.cs:79-105`), então o save do core pode descartar o `info.password` do arquivo;
  3. memória do `SaveServer` → `ProfileInfo.ExtensionData["password"]` (`PasswordController.cs:97-110`), para o próximo save do core re-serializar o campo.
- **Responde** `"OK"`/`"FAILED"` text/plain; o launcher trata `!= "OK"` como `UpdateFailed` e exceção como `NoConnection` (`AccountManager.cs:244-252`).
- No sucesso, o launcher também grava a senha em `SelectedAccount.password`, `AutoLoginCreds` e settings (`AccountManager.cs:254-262`).

Na volta, a leitura (`POST /redline/profile/get`, `PasswordController.cs:148-193`) re-injeta `vault[username]` em `info.password` antes de responder (`PasswordController.cs:170-180`) — o cofre é a fonte de verdade efetiva.

## 2. Corner cases

| Caso | Comportamento observado | Veredito |
|---|---|---|
| **Cancelar o dialog** | `result` null → `Logout()` + fica na tela de login (`LoginViewModel.cs:90-94`). Dialog é **obrigatório**, não adiável — sem senha não entra. | OK |
| **Conta legada sem senha** | Profile sem `info.password` e sem entrada no cofre → `AccountInfo.password` fica `""` → dialog dispara. Profile legado COM `info.password` no arquivo → senha respeitada na validação client-side. | OK |
| **Senha definida duas vezes** | Segunda escrita sobrescreve arquivo + cofre + memória; idempotente. Exceção multiplayer: dois clientes logando na mesma conta sem senha ao mesmo tempo → dois dialogs → last-write-wins no cofre; o primeiro usuário passa a errar a senha sem aviso (recuperável só pelo reset HWID). Edge raro, sem fix proposto agora — registrar. | OK c/ ressalva |
| **Offline no login** | `RequestLogin` lança → `NoConnection` → desativa auto-login ou navega pro `ConnectServerViewModel` (`LoginViewModel.cs:138-151`). | OK |
| **Offline no meio do dialog** | `ChangePassword` lança → `NoConnection` ≠ OK → notificação de erro + `Logout()` (`LoginViewModel.cs:83-88`). Nada persiste local sem persistir no server — sem divergência. | OK |
| **Reset via HWID** | `ChangePasswordAsync("")` zera a senha (server aceita `""`); próximo login cai na condição "sem senha" e força o dialog. Coerente com o design. | OK |

## 3. Veredito

**O fluxo do dialog em si FUNCIONA** no caminho projetado: conta sem senha → dialog obrigatório → persiste no cofre → logins seguintes validam client-side. Mas a auditoria achou defeitos concretos ao redor:

### Defeitos

- **D1 (funcional, o mais provável gatilho do item): a senha digitada no REGISTRO é descartada.** `RegisterViewModel` coleta senha (`RegisterViewModel.cs:65-71`) → `ClassSelectionViewModel` → `AccountManager.RegisterAsync` → `POST /launcher/profile/register` (SPT core). O `CreateAccount` do core cria o `Info` **sem senha** (`LauncherController.cs:117-129`) e o launcher **nunca chama** `/redline/password/change` no fluxo de registro (única chamada de conta em `ClassSelectionViewModel.cs:87` é `RegisterAsync`). Resultado: toda conta nova nasce "sem senha" no server; no primeiro login manual o usuário é forçado a criar senha DE NOVO — e qualquer senha diferente da original passa.
- **D2 (segurança): `/redline/password/change` não exige a senha atual.** `request.password` chega no payload mas nunca é lido (`PasswordController.cs:33`); qualquer cliente que saiba o username troca a senha de qualquer conta. No contexto atual (server privado Fika via Tailscale + gate real no HWID Manager porta 7075) o risco é entre conhecidos, mas é hijack trivial de conta.
- **D3 (segurança): `/redline/profile/get` devolve a senha em plaintext** para quem postar um username (`PasswordController.cs:170-183`) — é assim que a validação client-side funciona. Somado ao core não validar senha no login, a senha do TRL é protetiva só contra login acidental. Registrar como limitação arquitetural conhecida (senhas também ficam plaintext em profile json, cofre e settings do launcher).
- **D4 (bug latente): casing do cofre.** Match do profile é case-insensitive (`PasswordController.cs:69`, `:168`), mas a chave do cofre usa o casing enviado pelo cliente e o lookup do `JsonObject` é case-sensitive (`:86` vs `:176`). Login com casing diferente do usado ao definir a senha → cofre não acha → `info.password` pode vir vazio → dialog re-dispara e o cofre acumula chaves duplicadas ("Bob" e "bob").
- **D5 (menores):** log de debug `password_debug_log.txt` sem rotação, escrito a cada request e em `Directory.GetCurrentDirectory()` (CWD-dependente, diverge do `BaseDirectory` usado pro `ProfilesPath`) (`PasswordController.cs:40-41`); `ChangePassword` varre também `redline_passwords.json` (o `ProfileInfo` pula, `:161`; o change não, `:49`) — só ruído; variável `userFound` morta (`:52`, warning CS0219); a atualização em memória assume que `ExtensionData` round-tripa no save do core (existe no NuGet 4.0.2 — o build prova — mas o comportamento de re-serialização não foi validado em jogo; mitigado pelo cofre).

### Fix mínimo proposto para 005L (W2)

1. **Mata o D1 (launcher, ~3 linhas):** em `ClassSelectionViewModel`, após `RegisterAsync == OK` (`ClassSelectionViewModel.cs:87-91`), chamar `await AccountManager.ChangePasswordAsync(_password)` para semear o cofre; falha → notificar e seguir (o usuário cai no dialog no próximo login, comportamento atual).
2. **Opcional 005S (server, se quiser endurecer — fora do W2):** no `/redline/password/change`, exigir `request.password == vault[username]` quando o cofre já tem senha não-vazia (troca livre só quando não há senha) — resolve D2 sem quebrar o reset HWID (que zera o cofre antes); normalizar a chave do cofre para lowercase invariant nas três operações + migração lazy das chaves existentes — resolve D4.
3. D3/D5 ficam registrados; não bloqueiam o 005.
