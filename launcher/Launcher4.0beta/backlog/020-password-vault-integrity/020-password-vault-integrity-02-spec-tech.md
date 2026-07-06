# 020 — Integridade do cofre de senhas · Spec técnica

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [00-kickoff](./020-password-vault-integrity-00-kickoff.md) · [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md)<br>

---

> Spec funcional em [01-spec](./020-password-vault-integrity-01-spec.md). Este doc mapeia a implementação com `file:line` **confirmados por leitura** em 2026-07-04. O código já carrega os fixes D2/D4/CR-01-01/CR-01-02 de 005/010; este item ataca os **resíduos** que a auditoria isolou.

## Achado → ponto exato no código

### A1 — Colisão de casing (server)
`mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/PasswordController.cs`

| Ponto | Linha | O que está errado |
|---|---|---|
| Leitura do cofre — loop de match | `:60` | `string.Equals(entry.Key, username, OrdinalIgnoreCase)` — casa `Bob` e `bob` como o mesmo. |
| Chave canônica | `:54`, `:64` | `username.ToLowerInvariant()` — força chave lowercase, colapsando duas contas do core. |
| Match do arquivo de perfil (change) | `:152` | `string.Equals(fileUsername, request.username, OrdinalIgnoreCase)` no **1º arquivo enumerado** — grava no perfil errado. |
| Migração lazy (remove duplicatas) | `:174-183` | `Where(... OrdinalIgnoreCase).Remove(...)` — apaga a variante de casing da **outra** conta. |
| Gravação da chave | `:185` | `vault[request.username.ToLowerInvariant()] = request.change` — persiste lowercase. |
| Match em memória | `:199` | `OrdinalIgnoreCase` no `ProfileInfo.Username`. |
| Match do `/redline/profile/get` | `:267` | `OrdinalIgnoreCase`. |

**Fix (DP-020.A):** unificar o critério de igualdade em **um único ponto** e reusá-lo em `:60`, `:152`, `:199`, `:267` e na chave de `:54/:64/:185`.
- **A1 (case-sensitive):** trocar todos para `StringComparison.Ordinal` e usar `request.username` cru como chave (sem `ToLowerInvariant`). **Pré-condição:** confirmar o core (ver §Verificação do core).
- **A2 (canônico + bloqueio):** manter a chave lowercase, mas adicionar guard no registro que recusa username cujo lowercase já existe. O registro é do core (`/launcher/profile/register`) — o bloqueio teria que ser um controller TRL que intercepta/valida antes, ou um check no `password/change` inicial. Detalhar na fase de código conforme DP-020.A.

### A2 — Delete não-atômico (launcher)
`launcher/Launcher4.0beta/project/SPT.Launcher/ViewModels/ProfileViewModel.cs:1097-1160` (`DeleteAccountCommand`)

- `:1112` — `AccountStatus vaultStatus = await AccountManager.ChangePasswordAsync("");` roda **antes** do remove (`:1118`). O comentário `:1108-1111` justifica a ordem (evitar herança por username reciclado), mas o efeito colateral é a janela de "conta sem senha": se `ChangePasswordAsync("")` sucede e `RemoveAsync` (`:1118`) falha (`NoConnection`, `:1143`), a conta **sobrevive com senha vazia** → takeover.
- Além disso, `ChangePasswordAsync("")` grava senha **vazia** no cofre (via `PasswordController.cs:185`), não remove a chave → viola BR-020.3.

**Fix:**
1. **Reordenar:** `RemoveAsync()` primeiro; só no `case OK` (`:1122`) limpar o cofre.
2. **Trocar o mecanismo de limpeza:** em vez de `ChangePasswordAsync("")`, chamar um novo `AccountManager.DeleteVaultEntryAsync()` → `RequestHandler` → `POST /redline/password/delete` que **remove a chave** (não grava vazio). Falha aqui é best-effort (loga warning) e é reconciliada pela limpeza de órfãos (A4).
3. Manter a limpeza de `AutoLoginCreds`/`LastUsername`/`LastPassword` (`:1125-1132`) no `case OK`.

O `DeleteAccountCommand` já usa o guard robusto `result is not bool confirmed || !confirmed` (`:1104`) — **manter** (não regredir para o padrão frágil `is bool b && !b` de `WipeConfirmCommand:1088`/`RemoveProfileCommand:1182`).

### A3 — Plaintext em `/redline/profile/get` (server + launcher) — sujeito a DP-020.B
`PasswordController.cs:273-281` injeta `json["info"]["password"] = vaultPassword` e devolve o `info` inteiro. Consumido em:
- `RequestHandler.cs:53` (`RequestAccount` → `/redline/profile/get`).
- `AccountManager.cs:69-81` (`Login`): `SelectedAccount = Json.Deserialize<AccountInfo>(json)` — `SelectedAccount.password` vem do eco.
- `LoginViewModel.cs:58-66`: validação client-side compara `Login.Password != storedPassword`.

**Fix (se DP-020.B = corrigir agora):**
1. Server: novo `POST /redline/password/verify {username, password}` → `{status: OK|WRONG|NO_PASSWORD}` (reusa `TryGetVaultPassword` já existente, `:42-92`). `/redline/profile/get` para de injetar a senha (`:275-277` removido; retorna `info` sem `password`).
2. Launcher: `AccountManager.Login` (`:81`) passa a setar `SelectedAccount.password = password` (a senha **digitada**, disponível no parâmetro `:54`) após o deserialize. `LoginViewModel.cs:58-66` troca a comparação local por uma chamada ao verify; `NO_PASSWORD` cai no branch de criar senha (`:69-95`).
3. Downstream (`ChangePassword`/`Remove`/`Wipe` em `AccountManager`) continua enviando `SelectedAccount.password` como "senha atual" — agora a digitada, preservando o gate D2 (`PasswordController.cs:119-129`).

**Se DP-020.B = deferir:** documentar o débito e **não** tocar o eco; entregar só A1+A2+A4.

### A4 — Órfãos no cofre (server) — sujeito a DP-020.C
Nem `/launcher/profile/remove` (core) nem `wipe` tocam o `redline_passwords.json`. Não existe hoje endpoint de delete de cofre.

**Fix:**
1. Novo `POST /redline/password/delete {username}` no `PasswordController` — remove a chave (critério de DP-020.A), escrita atômica (temp+move).
2. **Reconciliação** (DP-020.C): método que enumera `redline_passwords.json`, cruza com `Directory.GetFiles(ProfilesPath, "*.json")` (usernames vivos via `info.username`, mesmo critério de match) e remove chaves sem perfil correspondente. Rodar no `password/delete` e, se DP-020.C aprovar, um sweep no boot do server (mod `postDBLoad`/`OnLoad`).

## Arquivos a tocar

| Arquivo | Camada | Mudança |
|---|---|---|
| `PasswordController.cs` | Server (TRL mod) | Unificar critério de casing (`:54,60,64,152,174-183,185,199,267`); novo `password/delete`; método de reconciliação; escrita atômica do cofre; (DP-020.B) `password/verify` + parar de injetar senha em `:275-277`. |
| `ProfileViewModel.cs` | Launcher | `DeleteAccountCommand:1097-1160` — reordenar (remove→limpar cofre), trocar `ChangePasswordAsync("")` por delete real. `WipeProfile:1021-1067` — tratar órfã em re-register falho. |
| `AccountManager.cs` | Launcher.Base | Novo `DeleteVaultEntryAsync/DeleteVaultEntry`; (DP-020.B) `Login:81` seta `password` da digitada; verify. |
| `RequestHandler.cs` | Launcher.Base | Novo `RequestDeleteVaultEntry` → `/redline/password/delete`; (DP-020.B) `RequestVerifyPassword` → `/redline/password/verify`; parar de depender do eco em `:53`. |
| `LoginViewModel.cs` | Launcher | (DP-020.B) `:58-66` validação via verify; `:189-208` reset — reavaliar `ChangePasswordAsync("")` no novo modelo. |
| `ClassSelectionViewModel.cs` | Launcher | `:132` seed de senha inicial — só validar que segue funcional com o novo critério (provável no-op). |
| **(novo)** `VaultKeyMatcher.cs` | Launcher.Base | Função pura de match/normalização de username (extraída para teste unitário — ver §Testes). |

## Contratos / DTOs

- **Existente** `ChangeRequestData` (`PasswordController.cs:12-17`): `{ username, password, change }` — reusado.
- **`POST /redline/password/delete`** → request `{ username }`; response `"OK"` / `"FAILED"` (text/plain, padrão do controller). Idempotente (username inexistente → `OK`).
- **(DP-020.B) `POST /redline/password/verify`** → request `{ username, password }`; response JSON `{ status: "OK" | "WRONG" | "NO_PASSWORD" }` (padrão do `HwidManager`, que já responde `{ status }`).
- **(DP-020.B)** `/redline/profile/get` — response `info` **sem** o campo `password` (`AccountInfo` do launcher tolera ausência? **verificar** o binding em `AccountInfo`/`Login:81` na fase de código).

## Escrita atômica do cofre (CC-5)

Hoje `:186` e `:158` fazem `File.WriteAllText` direto — corrupção sob concorrência coop. Padronizar em um helper `WriteVaultAtomic(path, json)`: escreve `path + ".tmp"` e `File.Move(tmp, path, overwrite:true)`. Aplicar em todas as escritas do cofre (change, delete, reconciliação).

## Verificação do comportamento do core (pré-condição de DP-020.A)

Antes de escolher A1 (case-sensitive), confirmar empiricamente como o core casa username no login/registro:
- Ler o `LoginController`/`ProfileController` do SPT core (fora do sandbox, referência) OU testar in-game: registrar `Bob`, tentar logar `bob`.
- Se o core casa **case-sensitive** → A1 é seguro e é a paridade correta.
- Se casa **case-insensitive** → A1 quebra (perfil `Bob`, chave de cofre `bob` ausente) ⇒ escolher **A2** (canônico lowercase + bloqueio no registro). Este é o default seguro sem a verificação.

## Riscos

- **R-1 (alto) — Divergência launcher↔core em casing.** Se A1 for escolhido sem confirmar o core, senha "some" no relogin com casing diferente. Mitiga: verificação empírica obrigatória; A2 como fallback.
- **R-2 (alto) — Blast radius do plaintext (DP-020.B).** Remover o eco toca 6 fluxos (login, reset-HWID, create-password, change, remove, wipe). Um deles deixar de setar `SelectedAccount.password` corretamente ⇒ gate D2 barra troca legítima. Mitiga: setar `password` da digitada logo no `Login:81`; teste manual de cada fluxo (GH-2).
- **R-3 (médio) — Migração de cofre legado (CC-1).** Apagar a chave errada perde a senha de um perfil vivo. Mitiga: GH-1 (inspeção antes do deploy); migração não-destrutiva por default; logar cada remoção.
- **R-4 (médio) — Órfão residual se delete-cofre falhar após remove OK.** Mitiga: reconciliação idempotente (A4); best-effort no launcher não bloqueia a exclusão.
- **R-5 (médio-coop) — Escrita concorrente do JSON.** Dois clientes coop trocando senha corrompem o cofre. Mitiga: escrita atômica (temp+move). Nota: temp+move **não** serializa writers concorrentes (last-writer-wins), mas evita JSON truncado. Lock de arquivo é over-engineering para o volume esperado; registrar como aceito.
- **R-6 (baixo) — `AccountInfo` sem `password`.** Se o binding assumir o campo presente, remover o eco pode dar null. Verificar na fase de código.

## Plano de teste

### Unit (`SPT.Launcher.Tests`, xUnit) — restrição de projeto
O `SPT.Launcher.Tests.csproj` referencia **só** `SPT.Launcher.Base` (`:17`) — **não** referencia o server mod TRL nem o projeto `SPT.Launcher` (UI). Logo:
- **Testável direto:** a função pura de match/normalização extraída para `VaultKeyMatcher.cs` (Base). Novo `VaultKeyMatcherTests.cs`: `Bob`≠`bob` (A1) OU canônico colidente detectado (A2); idempotência; nulls/vazios. Segue o estilo de `SyncRuleResolverTests.cs` (Theory/InlineData).
- **Não testável aqui:** `PasswordController` (server) e a ordem do `DeleteAccountCommand` (UI, `ProfileViewModel`). Opções: (a) extrair a lógica de sequência do delete para um helper em Base testável; (b) novo projeto `TarkovRedLine.Server.Tests` referenciando o server mod para cobrir casing/verify/delete/reconciliação (recomendado se DP-020.B for aprovado — a superfície de segurança justifica). Decidir na fase de código.

### Cobertura de não-regressão
- Reaproveitar/estender os testes de 005 do gate D2 e leitura fail-closed sob o novo critério de casing (AC-020.13).

### Gates humanos
GH-1..GH-4 do [01-spec](./020-password-vault-integrity-01-spec.md) — inspeção de produção do `redline_passwords.json`, ciclo completo in-game, casing `Bob`/`bob`, e coop. **Escrita em arquivo SPT exige validação no jogo, não só build verde.**

## Nota de paralelismo (arquivos compartilhados com outros itens)

- **`ProfileViewModel.cs`** — hub dos itens **019-023**. Este item toca `DeleteAccountCommand` (`:1097-1160`) e `WipeProfile` (`:1021-1067`). Coordenar merges: 010 (delete), 019/021/022/023 podem tocar a mesma classe. Manter os edits cirúrgicos e não reintroduzir o guard frágil `is bool b && !b`.
- **`PasswordController.cs`** — compartilhado com os itens **005** (senha) e **010** (delete), já entregues. Este item **altera o critério de casing** que 005/D4 estabeleceu (lowercase) — é uma **reversão consciente** de parte de D4 conforme DP-020.A; registrar no as-build para não parecer regressão.
- **`AccountManager.cs` / `RequestHandler.cs`** (Base) — compartilhados por qualquer item de conta (005, 010, 019). Endpoints novos (`password/delete`, `password/verify`) são aditivos.
- **`LoginViewModel.cs`** — compartilhado com fluxos de auth (005, 006 Dev Mode bypass, 013 footers). Só tocar se DP-020.B = corrigir agora.
- **`OptionalModsHelper` (019/021), `Legacy.axaml` (024/025)** — **não** tocados por este item (registrado só para o mapa de paralelismo da sessão).

## Gates

Ver [01-spec §Gates](./020-password-vault-integrity-01-spec.md#gates). Build: `SPT.Launcher.csproj`, `SPT.Launcher.Tests.csproj`, `TarkovRedLine.Server.csproj` em Release, verdes. Nunca rodar o exe.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — spec técnica com file:line confirmados; DP-020.A/B/C e restrição do projeto de testes registradas. |
