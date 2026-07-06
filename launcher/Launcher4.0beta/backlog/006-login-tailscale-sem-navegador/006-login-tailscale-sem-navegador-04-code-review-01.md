# 006 — Login Tailscale sem navegador · Code Review 01 (adversarial)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Commit revisado:** `30c75b2` · **Insumo:** [02-spec-tech](./006-login-tailscale-sem-navegador-02-spec-tech.md)

> Review de contexto limpo (revisor não escreveu o código). Escopo: `Helpers/TailscaleHelper.cs` + `ViewModels/ConnectServerViewModel.cs`. Referências de linha no estado pós-commit. Build gate: `dotnet build SPT.Launcher.csproj -c Release` → **0 erros** (127 warnings pré-existentes de nullability/CA1416).

**Placar:** 2 🔴 · 3 🟡 · 2 🟢

---

## TailscaleHelper.cs

### CR-01-01 [🔴 bloqueador] `RunTailscaleUp` bloqueia indefinidamente — o timeout de 60s é inalcançável no cenário que ele existe para cobrir

`TailscaleHelper.cs:177-186`: a ordem é `Start()` → `StandardError.ReadToEnd()` (síncrono) → `StandardOutput.ReadToEnd()` → **só então** `WaitForExit(60_000)`.

`ReadToEnd()` só retorna quando o processo **fecha o stream** (na prática, quando sai). Cenário de falha concreto: control plane inacessível (o exato cenário citado no comentário da linha 181 — "'up' can hang if the control plane is unreachable") → `tailscale up` fica vivo tentando reconectar → `StandardError.ReadToEnd()` bloqueia para sempre → o código **nunca chega** no `WaitForExit(60_000)`, o kill nunca dispara. Resultado no launcher: `ConnectServer` nunca completa, `InfoText` fica travado em "Conectando na rede P2P (Tailscale)..." sem erro, sem botão de retry, sem `AllowSettings` — o usuário precisa matar o launcher. Isso derrota o objetivo central do item (erro claro + retry).

Defeito secundário no mesmo bloco: ler stderr até o fim **antes** de drenar stdout é o anti-pattern clássico de deadlock de pipe duplo — se o filho encher o buffer (~4 KB) de stdout enquanto estamos bloqueados no stderr, ambos os lados travam. O comentário `// drain to avoid pipe deadlock` (linha 179) não cumpre o que promete: o dreno precisa ser **concorrente**, não sequencial.

**Fix:** iniciar as duas leituras como tasks logo após `Start()` (`var errTask = p.StandardError.ReadToEndAsync(); var outTask = p.StandardOutput.ReadToEndAsync();`), então `WaitForExit(60_000)`; no timeout, `Kill()` e só depois `await` das tasks (elas completam quando o processo morre). Tornar `RunTailscaleUp` async encaixa naturalmente no caller.

### CR-01-02 [🔴 bloqueador] `DisableGuiAutostart` aborta na primeira exceção — sem elevação, o fix do caminho C (autostart pós-MSI) pode ser no-op e a limpeza da pasta Startup nunca roda

`TailscaleHelper.cs:209-227`: as 4 remoções (Run HKCU, Run HKLM, `Startup\Tailscale.lnk` do usuário, `CommonStartup\Tailscale.lnk`) estão num **único** `try/catch { }`. O launcher não tem `app.manifest` (verificado: nenhum `requestedExecutionLevel` no projeto) → roda `asInvoker`, não-elevado. `Registry.LocalMachine.OpenSubKey(@"...\Run", writable: true)` (linha 215) **lança `SecurityException`** para processo não-elevado → o catch engole → **as linhas 218-224 (atalhos Startup) nunca executam**, mesmo que a deleção do `.lnk` do usuário não exija elevação nenhuma.

Cenário de falha concreto (exatamente o furo C que o commit declara fechado): máquina limpa, launcher não-elevado → MSI instala (msiexec elevado via UAC) e recria o autostart → `DisableGuiAutostart()` pós-MSI roda não-elevado → HKCU ok (vazio), HKLM lança, resto pulado → **autostart em HKLM e/ou Startup folder sobrevive** → próximo boot do Windows sobe `tailscale-ipn`; se a sessão estiver não-autenticada (key expirada — caminho B+C combinados), o GUI pode abrir o navegador de login. O furo C fica silenciosamente aberto, e o `catch { }` não loga nada.

**Fix:** um `try/catch` **por operação** (com log de `Warning` em cada falha) para que HKCU + os 2 `.lnk` sempre rodem; para o valor em HKLM sem elevação, ou aceitar e logar explicitamente ("Run key HKLM não removível sem admin"), ou remover via o próprio msiexec elevado (property/transform). Manter o gate humano da spec (validar em máquina limpa onde o MSI realmente grava o autostart).

### CR-01-03 [🟡 relevante] IP presente conta como sucesso mesmo com `up` falho — VPN funcionalmente morta vira "server unavailable" genérico

`TailscaleHelper.cs:135-148` + assunção 4 da spec: após `RunTailscaleUp` falhar, um IP `100.x` na interface é aceito como conectado. O adapter Tailscale pode **reter o IP com status Up** em estados em que o túnel não passa tráfego (key expirada no control server com nó ainda "logado", control plane fora do ar com relays indisponíveis). Cenário: key expira → `up` falha → GUI é morta (`KillTailscaleGui`) → IP stale ainda presente → retorna `true` → `ConfigureFikaAsync` grava o IP stale no config do Fika → `LoadDefaultServerAsync` falha 2× → usuário vê o "server unavailable" genérico, **nunca** a mensagem clara de VPN que este item construiu. A UX de erro do item raramente dispara justamente nos estados degradados.

**Fix:** quando `upSucceeded == false`, não aceitar o IP da interface cegamente — validar com `tailscale status --json` (`BackendState == "Running"`) ou um probe TCP curto ao IP Tailscale do servidor antes de retornar `true`.

### CR-01-04 [🟡 relevante] Bypass de Dev Mode dispara em instalação limpa — `IsDevMode` nasce `true` por default

`ConnectServerViewModel.cs:85-89` trata falha de VPN como não-fatal em Dev Mode. Porém `Settings()` (`SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs:352`) seta **`IsDevMode = true` quando o config não existe** — ou seja, toda instalação nova — e o auto-reset de Dev Mode está comentado (`LauncherSettingsProvider.cs:56-61`). Cenário: usuário novo (a população-alvo do item 006), Tailscale falha na primeira execução → branch Dev Mode → "prosseguindo sem VPN" silencioso → conexão segue contra a URL default de dev (`https://147.15.29.24:7073`, já que Dev Mode também pula o fetch da gist na linha 47) → falha genérica. O erro claro + retry deste item **nunca aparece para usuário novo**. A ativação de Dev Mode é protegida por senha na UI (`SettingsViewModel.ToggleDevModeCommand`), mas o default anula o gate.

**Fix:** default `IsDevMode = false` no config novo (ou reativar o auto-reset comentado). Alternativa mínima sem tocar no default: condicionar o bypass a `IsDevMode && config já existia`.

### CR-01-05 [🟡 relevante] AuthKey da gist entra sem validação na linha de comando (tailscale e msiexec)

`TailscaleHelper.cs:39` (fetch), `:170` (`Arguments = $"up --authkey={authKey} ..."`, sem aspas) e `:74` (`TS_AUTHKEY=\"{authKey}\"`). O conteúdo da gist pública é interpolado direto nos argumentos. Cenário: gist comprometida ou editada com conteúdo contendo espaço/aspas — ex.: `tskey-x --exit-node=100.66.6.6` — vira **argumentos extras do `tailscale up`** (redirecionar tráfego do usuário por um exit node hostil) ou quebra o quoting do msiexec. Sendo este o commit de hardening, o input externo do fluxo merece o mesmo rigor do exit code.

**Fix:** validar o formato antes de usar (`^tskey-[A-Za-z0-9-]+$`); se não casar, logar e usar `FallbackAuthKey`.

### CR-01-06 [🟢 menor] Higiene de handles de processo

`tsProcess` (`:165`), o `process` do msiexec (`:77`) e os `Process[]` de `GetProcessesByName` em `StartGuiIfNotRunning` (`:237`) nunca são `Dispose()`d (em `KillTailscaleGui` são). Após `Kill()` por timeout não há `WaitForExit()` de confirmação. E `process.Kill()` num msiexec **elevado** a partir de launcher não-elevado lança Access Denied (engolido) — o kill do MSI é inócuo além de arriscado (instalação parcial). Impacto real baixo (processo de curta duração), mas é leak de handle em código de retry.

**Fix:** `using var` nos `Process`, e no timeout do msiexec apenas logar e seguir (não tentar `Kill`).

### CR-01-07 [🟢 menor] Pior caso ~160s+ sem progresso nem cancelamento

Por tentativa: gist 10s + `up` 60s + espera de IP 10s + (na 1ª execução) MSI até 5 min — ×2 tentativas, com `InfoText` estático em "Conectando na rede P2P (Tailscale)...". Não é defeito funcional (com CR-01-01 corrigido, termina), mas o usuário não tem como abortar nem sabe em que passo está. **Fix opcional:** atualizar `InfoText` por passo e/ou reduzir o timeout do `up` na segunda tentativa.

---

## ConnectServerViewModel.cs

**Auditado, sem achados próprios além do CR-01-04.** Verificado especificamente:

- **Fluxo de retry preservado:** o branch novo usa exatamente o mecanismo existente — `ConnectionFailed = true` exibe o botão bound a `RetryCommand` (`Views/ConnectServerView.axaml:34-35`), que reseta o flag e re-executa `ConnectServer` inteiro (re-fetch da URL da gist incluído). `AllowSettings = true` é restaurado no branch de falha, consistente com o branch "server unavailable" existente (:147).
- **Todos os caminhos terminam:** sucesso → Fika → segue; falha+dev → segue; falha+normal → `return` com erro visível. Exceções inesperadas caem no catch de `ConnectServer` (:177-182) que também seta `ConnectionFailed`.
- Propriedades do `connectModel` setadas de thread de background — padrão pré-existente em todo o método, não introduzido por este commit.

---

## Resoluções (2026-07-03, aplicadas pelo orquestrador — commit desta rodada)

| Achado | Resolução |
|---|---|
| CR-01-01 🔴 | ✅ Aplicado — `ReadToEndAsync()` concorrente nas duas streams logo após `Start()`, `WaitForExit(60s)` → kill no timeout → await das tasks só após exit. `// ref: CR-01-01` no código. |
| CR-01-02 🔴 | ✅ Aplicado — try/catch POR operação com `LogManager.Warning` em cada falha; HKLM sem elevação agora loga explicitamente e não impede a limpeza dos `.lnk`. `// ref: CR-01-02`. |
| CR-01-03 🟡 | ✅ Aplicado — IP presente com `up` falho só conta como sucesso se `tailscale status --json` reportar `BackendState=Running` (`IsBackendRunning()`, timeout 10s). `// ref: CR-01-03`. |
| CR-01-04 🟡 | ✅ Aplicado — `IsDevMode = false` em config novo (`LauncherSettingsProvider.cs`). Assunção registrada: instalação limpa é jogador; o dev (Guilherme) reativa via toggle com senha. `// ref: CR-01-04`. |
| CR-01-05 🟡 | ✅ Aplicado — `IsValidAuthKey()` (`^tskey-[A-Za-z0-9-]+$`) valida o conteúdo da gist antes de interpolar em `tailscale up`/msiexec; malformado → fallback embutida. `// ref: CR-01-05`. |
| CR-01-06/07 🟢 | ⏭️ Aceitos sem ação nesta rodada (higiene de handles/`using` em Process) — anotados para o passe de limpeza do item 014. |
