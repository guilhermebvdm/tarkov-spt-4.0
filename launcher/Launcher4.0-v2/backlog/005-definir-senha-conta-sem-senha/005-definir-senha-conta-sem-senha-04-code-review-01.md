# 005 — Definir senha em conta sem senha (005L) · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Commit revisado:** `50cfce1` · **Insumo:** [02-spec-tech](./005-definir-senha-conta-sem-senha-02-spec-tech.md) (auditoria D1-D5 + registro 005L)

> Review de contexto limpo (revisor não escreveu o código). Escopo: `mods/TarkovRedLine4.0/Server/TarkovRedLine.Server/Controllers/PasswordController.cs` inteiro pós-mudança (gate D2 + cofre case-insensitive D4 + limpeza D5), validado contra `HwidManager.cs` e todos os callers do launcher. Build gate: `dotnet build TarkovRedLine.Server.csproj -c Release` → **0 erros** (31 warnings pré-existentes, batem com o asbuild).

**Placar:** 0 🔴 · 2 🟡 · 3 🟢

---

## Cenários de segurança rastreados (D2)

**(a) Usuário esqueceu a senha → reset HWID ainda funciona? SIM — confirmado no código, e a alegação do asbuild está correta.**
`HwidManagerController.ResetPassword` (`HwidManager.cs:96-140`) **só valida HWID** — não zera o cofre nem toca senha (a auditoria original supunha que zerava; o asbuild corrigiu certo). Quem efetiva o reset é o launcher: `LoginViewModel.ResetPasswordCommand` → após status OK, `AccountManager.LoginAsync(username, "")` (`LoginViewModel.cs:189`) — o login SPT não valida senha server-side e o `/redline/profile/get` **injeta a senha do cofre** em `SelectedAccount.password`; em seguida `ChangePasswordAsync("")` (`:196`) monta `ChangeRequestData(username, SelectedAccount.password, "")` (`AccountManager.cs:237`) — ou seja, `request.password` chega com a **senha real do cofre**, o gate passa, o cofre vira `""`. A alegação "o launcher loga antes e manda a senha real do cofre" confere linha a linha. Depois, `request.change == ""` passa a validação de entrada (`request?.change == null` — só null falha), e o relogin cai no CreatePasswordDialog (cofre vazio → troca livre). Ciclo fechado.

**(b) Conta sem senha no cofre → primeira senha continua livre? SIM.** Gate em `PasswordController.cs:84-89`: `GetVaultPassword` retorna null (sem entrada/sem arquivo) ou `""` (pós-reset) → `!string.IsNullOrEmpty(...)` false → sem exigência. Cobre conta nova, conta legada sem cofre e conta pós-reset.

**(c) O launcher manda a senha ATUAL ou a NOVA em `request.password`? Sempre a ATUAL, em todos os caminhos.** Os 3 call sites (`LoginViewModel.cs:76` CreatePasswordDialog, `:196` reset, `ClassSelectionViewModel.cs:130` seed do 004L) passam por `AccountManager.ChangePassword`, que **sempre** envia `SelectedAccount.password` (atual, vinda da injeção do cofre no login) em `password` e a nova em `change`. No caminho do CreatePasswordDialog o atual é `""` e o cofre está vazio → livre; no seed do 004L a conta é recém-registrada → cofre vazio (exceto o caso de username reciclado — ver CR-01-04) → livre. Nenhum caller manda a senha nova no campo errado.

**(d) A migração lazy do D4 pode perder a senha mais recente? Na ESCRITA não; na LEITURA sim (sombreamento) — ver CR-01-01.**

---

## Achados

### CR-01-01 [🟡] D4 leitura: primeiro match em ordem de documento — duplicata legada mais antiga sombreia a senha mais recente (regressão de login para quem digitava o casing "novo")

`GetVaultPassword` (`PasswordController.cs:45-51`) retorna a **primeira** entrada case-insensitive na ordem do JSON. Duplicatas legadas são exatamente o artefato do bug pré-D4 que o 005 corrige: ex. registro como `Foo` (senha `a`) + sessão posterior logando `foo` onde a injeção exact-match falhava → CreatePasswordDialog regravava `foo: b`. Cofre: `{"Foo":"a","foo":"b"}`, com `b` sendo a senha que o usuário conhece. Pós-commit: `GetVaultPassword("foo")` devolve `a` (primeira) → injeção devolve `a` → validação client-side (`LoginViewModel.cs:61`) rejeita `b` → **lockout**; pré-commit, logar como `foo` devolvia `b` e funcionava. Gate e injeção usam a mesma função, então entre si nunca divergem (o reset HWID funciona e, de bônus, consolida as duplicatas na próxima escrita) — o dano é só no login, mas é regressão real. **Fix (leitura):** priorizar a chave canônica lowercase (formato pós-D4, sempre a escrita mais recente), depois match exato de casing, depois qualquer case-insensitive. **Pré-deploy:** inspecionar o `redline_passwords.json` de produção por chaves que colidem case-insensitive — se houver, tratar este achado como bloqueante do deploy até o fix da ordem de leitura.

### CR-01-02 [🟡] Gate fail-open: erro de leitura do cofre é indistinguível de "sem senha" → troca liberada sem senha atual

`GetVaultPassword` engole qualquer exceção e retorna null (`PasswordController.cs:53-56`): IOException por leitura concorrente com o `File.WriteAllText` do change (sem lock/escrita atômica), JSON corrompido, valor não-string (`GetValue<string>` lança) — todos viram "não há senha" e o gate D2 libera a troca cega que ele existe para impedir. Para gate de segurança o correto é fail-closed: distinguir "arquivo não existe / não há entrada" (→ livre) de "erro lendo o cofre" (→ `FAILED`). Severidade contida em 🟡 e não 🔴 porque o D3 (senha plaintext no `/redline/profile/get`, ressalva já registrada na spec) torna o gate contornável por consulta de qualquer forma — o fail-open só encurta um atalho que já existe.

### CR-01-03 [🟢 menor] Overclaim no asbuild: "dois clientes criam senha ao mesmo tempo → segundo falha explícito" só vale se as requests serializarem

Se ambas as requests lerem o cofre vazio antes de qualquer escrita (janela read→write sem lock no endpoint), ambas passam o gate e a última escrita vence silenciosamente — comportamento igual ao anterior. O gate melhora o caso sequencial (segundo cliente com cofre já populado falha), não o simultâneo. Corrigir a frase do asbuild ou aceitar como está.

### CR-01-04 [🟢] (cross-ref item 010) Exclusão de conta deixa a entrada do cofre órfã — username reciclado herda a senha do dono anterior

Detalhado como **CR-01-01 do review do 010** ([010-excluir-conta-04-code-review-01.md](../010-excluir-conta/010-excluir-conta-04-code-review-01.md)): `/launcher/profile/remove` não toca o cofre. Metade server possível: limpar/ignorar entrada do cofre quando o profile correspondente não existe mais (ex.: no `profile/get` que retorna NotFound, ou na próxima escrita). Fix imediato proposto é launcher-side (esvaziar o cofre antes do remove).

### CR-01-05 [🟢 menor] Skip do cofre por sufixo de filename

`file.EndsWith("redline_passwords.json")` (`PasswordController.cs:97` e `:220`) também pularia um profile hipotético `xyz_redline_passwords.json`. Teórico (nomes de profile são GUIDs no SPT); `Path.GetFileName(file) == "redline_passwords.json"` seria exato. Sem urgência.

---

## Áreas auditadas, sem achados

- **Contrato inalterado:** sucesso `"OK"` / falha `"FAILED"` em `text/plain`, exatamente o que `AccountManager.ChangePassword` compara (`STATUS_OK = "OK"`); `RequestChangePassword` já usa `decompressResponse: false`. Nenhuma mudança de payload — confere com "server-only, sem mudança de contrato" (P-005.2 dispensada corretamente).
- **Posição do gate:** roda após a validação de entrada e o check do diretório, antes do loop de profiles — nega cedo sem tocar arquivo; `[DENIED]` no debug log registra username, **não** registra senhas.
- **Escrita D4:** normalização `ToLowerInvariant()` + remoção de todas as variantes de casing antes de gravar — na escrita nenhuma senha "mais recente" se perde (o valor gravado é a verdade nova). `ToList()` antes do `Remove` evita mutação durante enumeração.
- **Injeção no `profile/get`:** usa o mesmo `GetVaultPassword` do gate → os dois nunca divergem entre si; `vaultPassword != null` injeta inclusive `""` (correto pós-reset: força o CreatePasswordDialog no login seguinte).
- **D5 parcial:** `userFound` (CS0219) removido; skip do cofre elimina o ruído de log nos dois endpoints; `password_debug_log.txt` em CWD e validação do round-trip de `ExtensionData` pulados **com anotação** no asbuild — coerente com o escopo declarado.
- **Compatibilidade com wipe e registro:** `WipeProfile` (Remove+Register com a mesma senha) e o seed do 004L mantêm `request.password` = senha injetada → gate transparente para os fluxos existentes.
- **Dialogs do 005L (XAML):** cobertos no review do 010 (mesmo pacote visual) — semântica do ✕ conferida caller a caller, nenhum binding/command alterado.

---

## Resoluções (2026-07-04, /apply-code-review)

| CR | Resolução |
|---|---|
| CR-01-01 🟡 | **Aplicado** — `GetVaultPassword` virou `TryGetVaultPassword` com leitura priorizada: (1) chave canônica lowercase (formato pós-D4 = escrita mais recente), (2) match exato de casing, (3) qualquer match case-insensitive. Duplicata legada mais antiga não sombreia mais a senha mais recente. Gate e injeção usam a mesma função (nunca divergem entre si). `// ref: CR-01-01`. **Gate humano pré-deploy registrado** (aqui e no registro 005L do 02-spec-tech): inspecionar o `redline_passwords.json` de produção por chaves que colidem case-insensitive ANTES do deploy — se houver colisão, validar manualmente qual senha é a vigente antes de liberar. |
| CR-01-02 🟡 | **Aplicado (fechado barato)** — `TryGetVaultPassword` retorna `false` em erro de leitura (parse/IO/valor não-string), distinto de "sem entrada" (`true` + null); o gate D2 responde `FAILED` nesse caso (fail-closed, log `[DENIED] Vault unreadable`). A **injeção** do `/redline/profile/get` permanece best-effort (sem injeção em erro) — lá não há decisão de segurança e falhar fechado derrubaria o login por erro transitório. `// ref: CR-01-02`. |
| CR-01-03 🟢 | Frase do overclaim corrigida no registro 005L do 02-spec-tech (o gate melhora o caso sequencial; o simultâneo continua last-write-wins — sem lock no endpoint). |
| CR-01-04 🟢 | **Resolvido via 010 CR-01-01** — o launcher agora esvazia o cofre (`ChangePasswordAsync("")`) antes do remove no `DeleteAccountCommand`. Metade server (limpar entrada órfã quando o profile não existe) fica como melhoria futura. |
| CR-01-05 🟢 | Não endereçado (teórico — profiles são GUIDs; `EndsWith` suficiente). |

Gates: build TarkovRedLine.Server Release **0 erros** · build launcher **0 erros** · `dotnet test` **52/52**. Deploy da DLL continua pendente (rotina padrão), agora condicionado ao gate humano do CR-01-01.
