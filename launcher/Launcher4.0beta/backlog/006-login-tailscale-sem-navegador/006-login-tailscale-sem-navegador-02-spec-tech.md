# 006 — Login Tailscale sem navegador · Spec técnica / Auditoria + hardening (as-built)

**Launcher:** Launcher4.0beta · **Data:** 2026-07-03 · **Insumo:** [00-kickoff](./006-login-tailscale-sem-navegador-00-kickoff.md)

> Auditoria caminho a caminho do fluxo Tailscale + hardening aplicado. Referências de linha **pós-edição** (arquivo:linha). Build NÃO rodado nesta sessão (validado pelo orquestrador).

## Escopo tocado

- `project/SPT.Launcher/Helpers/TailscaleHelper.cs` — hardening (única fonte de lógica Tailscale)
- `project/SPT.Launcher/ViewModels/ConnectServerViewModel.cs` — call site: tratamento de erro (mecanismo existente: `connectModel.InfoText`/`ConnectionFailed` + `LogManager`; nenhuma view/dialog nova)

Únicos call sites de `TailscaleHelper` no launcher: `ConnectServerViewModel.cs` (`EnsureTailscaleConnected` e `ConfigureFikaAsync`). Nenhum outro caller encontrado (grep repo-wide).

## Auditoria — onde um navegador poderia abrir

| # | Caminho | Veredito antes | Estado após hardening |
|---|---------|----------------|----------------------|
| A | **CLI `tailscale up` sem authkey** | Seguro — `up` só roda com authkey não-vazia; authkey nunca é vazia (fallback embutido). CLI com `--authkey` autentica server-side, nunca abre navegador; em falha imprime erro no stderr e sai com código ≠ 0. | Mantido; agora exit code + stderr são capturados e logados (`RunTailscaleUp`, TailscaleHelper.cs:162-204). |
| B | **Gist inacessível + fallback inválida/expirada** | **FURO ENCONTRADO.** `up` falhava silenciosamente (exit code ignorado) e o código seguia para iniciar a GUI (`tailscale-ipn`) mesmo sem autenticação — GUI em estado NeedsLogin **pode abrir o navegador de login sozinha** (comportamento first-run do client Windows). | **Fechado.** Exit code verificado; em falha: GUI não é iniciada, qualquer `tailscale-ipn` já em execução é **morto** (`KillTailscaleGui`, :253-269), erro logado e `false` propagado ao caller. |
| C | **MSI install first-run GUI** | Parcial. `/quiet /norestart` suprime a UI sequence do MSI → o instalador em si não lança GUI/navegador pós-install. **Porém** o MSI recria a entrada de autostart (Run key), e a limpeza de autostart rodava só ANTES do install — uma máquina recém-instalada ficava com autostart armado até a próxima execução do launcher; no próximo boot do Windows, GUI não-autenticada podia abrir navegador. | **Fechado.** `DisableGuiAutostart()` é re-executado imediatamente APÓS o install do MSI (TailscaleHelper.cs:96-99). `TS_AUTHKEY` mantido nos args do msiexec (inócuo se a propriedade não for reconhecida pelo MSI; auth real acontece via CLI no passo seguinte). |
| D | **GUI autostart no boot do Windows** | Coberto — remoção de Run keys HKCU/HKLM + atalhos Startup (user e common) a cada start do launcher. | Mantido, extraído para `DisableGuiAutostart()` (:206-232) e agora chamado 2×: no início E pós-install (fecha a janela do caminho C). |
| E | **Login expirado em sessão existente** | Coberto em parte — `up --authkey --reset` roda a cada start do launcher, renovando o login sem browser. Se a key também estivesse expirada, caía no caminho B (furo). | Coberto — key expirada agora cai no caminho B fechado (erro claro, GUI morta, sem navegador). Sessão anterior ainda válida (IP presente) conta como sucesso mesmo com `up` falho (:135-151). |
| F | **Ordem GUI vs autenticação** | GUI era iniciada ANTES da confirmação de IP (logo após o `up`, sem checar resultado). | **Invertido:** GUI só inicia APÓS IP confirmado (`StartGuiIfNotRunning` chamado dentro do loop de espera de IP, :141-147) — GUI autenticada não tem motivo para abrir navegador. |
| G | **Hangs de processo (não-navegador, robustez)** | `WaitForExit()` sem timeout no msiexec e no `up` (o `up` pode pendurar indefinidamente com control plane inacessível). | Timeouts: msiexec 5 min (:89-93), `tailscale up` 60 s com kill (:183-188). Fetch da gist com timeout de 10 s (:38). |

**Veredito geral:** o desenho anterior (`up --authkey --unattended --reset` + supressão de autostart) já cobria o caminho feliz, mas havia **2 furos reais** (B e C) — ambos dependiam de authkey inválida/expirada ou de boot pós-install, por isso nunca apareceram em teste com key válida. Fechados nesta sessão. Nenhum outro `Process.Start` com URL existe no fluxo de conexão (os únicos browser-opens do launcher são botões explícitos de usuário — Ko-fi etc.).

## Mudanças de código

### `Helpers/TailscaleHelper.cs`

- `EnsureTailscaleConnected()`: **`Task` → `Task<bool>`** — falha total (2 tentativas sem IP) agora retorna `false` com `LogManager.Error` explícito; antes retornava em silêncio.
- Novo `RunTailscaleUp(authKey)`: captura exit code + stderr do `tailscale up`; timeout 60 s; retorna sucesso/falha.
- Em falha do `up`: `KillTailscaleGui()` mata `tailscale-ipn` em execução (previne popup de login do GUI não-autenticado).
- GUI (`StartGuiIfNotRunning`) só inicia após IP confirmado.
- `DisableGuiAutostart()` extraído e chamado também pós-install do MSI.
- Gist fetch: timeout 10 s + tratamento de resposta vazia (cai no fallback).
- Constantes extraídas (`TailscalePath`, `AuthKeyGistUrl`, `FallbackAuthKey`); removida variável morta `justInstalled`.
- `GetTailscaleIp()` e `ConfigureFikaAsync()` inalterados.

### `ViewModels/ConnectServerViewModel.cs` (call site, ~:72-101)

- Consome o novo retorno `bool`:
  - **Sucesso** → segue para `ConfigureFikaAsync` (inalterado).
  - **Falha + Dev Mode** → log warning e prossegue sem VPN (Dev Mode pode usar servidor local; assunção registrada abaixo).
  - **Falha + modo normal** → `connectModel.ConnectionFailed = true` + `InfoText` com mensagem clara ("Falha na rede P2P (Tailscale)...") + `AllowSettings = true` + `return`. Usa exatamente o mecanismo de erro já existente da tela (mesmo padrão do "server unavailable"), que exibe o botão de retry (`RetryCommand`). Nenhuma view nova.

## Assunções registradas

1. **Falha de VPN é fatal fora do Dev Mode** — sem IP Tailscale a URL do servidor (100.x) é inalcançável; abortar com retry é melhor UX do que deixar o load do servidor falhar 2× com mensagem genérica. Em Dev Mode prossegue (servidor pode ser local).
2. **`TS_AUTHKEY` no msiexec mantido** — não é propriedade documentada do MSI oficial do Tailscale (provável no-op), mas é inócua e a auth real é via CLI; removê-la mudaria comportamento sem ganho.
3. **Matar `tailscale-ipn` não-autenticado é aceitável** — instalação gerenciada pelo launcher para servidor privado; o risco de popup de navegador supera o custo de fechar o tray app (ele é reaberto pós-conexão).
4. **Sessão válida preexistente conta como sucesso** mesmo se `up` falhar (key expirada mas nó ainda logado) — IP presente = conectado.

## Validável somente em máquina limpa (gate humano)

- **Install first-run real**: MSI silencioso + UAC + criação/remoção da Run key + primeira auth via authkey — precisa de máquina sem Tailscale.
- **Comportamento do `tailscale-ipn` em NeedsLogin**: a premissa de que o GUI pode auto-abrir navegador nesse estado vem do comportamento conhecido do client Windows; confirmar na versão do MSI embarcado.
- **Authkey expirada de verdade** (gist bloqueada + fallback expirada): confirmar que o launcher mostra a mensagem de erro + retry e que nenhum navegador abre.
- **Boot do Windows pós-install** sem rodar o launcher de novo: confirmar que o autostart não volta (fix do caminho C).
- **UAC negado** no msiexec: deve cair em erro claro (sem navegador) — caminho coberto por código, não testado.
- Build/compile: **não executado nesta sessão** (proibido — outro agente buildava o csproj em paralelo); validação de compilação fica com o orquestrador.
