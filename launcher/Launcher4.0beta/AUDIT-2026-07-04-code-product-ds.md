# Auditoria do Launcher TRL 2.0 — Código · Produto · Design System

> **Data:** 2026-07-04<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Método:** 4 revisões paralelas read-only (código frontend, código backend/motor de sync, produto/aceite, design system) cruzando código real com specs do backlog e o DS grafite v2.<br>
> **Escopo:** todo o `launcher/Launcher4.0beta/project/` no estado **em disco** (inclui mudanças visuais ainda não commitadas: Taquila `bg1`, painéis 50%, top bar 30%, cantos agudos DWM). Upstream `launcher/Launcher4.0/` fora de escopo.

---

## Sumário executivo

**Contagem consolidada:** 3 🔴 · ~20 🟡 · ~20 🟢.

**Aprovado limpo:** itens **012, 014, 016, 017** (e núcleos de 004/007/008/013). O motor de sync (`SyncEngine`/`SyncPlanner`/`SyncPathUtil`) é sólido — guard anti-traversal, apply atômico, baseline com hash real, 016/017 corretos. O tema (`Assets/Theme/**`) é sólido e disciplinado (radius 0, vermelho < 5%, tokens `Trl*` como única fonte de cor). **Nenhum item tem quebra dura de aceite.**

**Os 3 bloqueadores** são de segurança/dados/DS, não de aceite funcional:

| # | Bloqueador | Onde | Tipo |
|---|---|---|---|
| **B1** | **RCE no auto-update** — TLS desligado + exe executado sem verificar assinatura/hash | `LauncherUpdateHelper.cs:22,76` | 🔴 Segurança |
| **B2** | **`deleteFiles` do manifesto apaga caminho arbitrário** sem guard de raiz (traversal `../../`), em todo login | `ProfileViewModel.cs:644-659` | 🔴 Perda de dados |
| **B3** | **`SettingsView` inteira não migrou** ao DS (~20 hex + sidebar/cards próprios) | `SettingsView.axaml` | 🔴 DS |

**Top riscos de negócio:** 005 (senha gravada no perfil errado por match case-insensitive), 010 (delete não-atômico deixa conta sem senha), 009 (2 dos 4 toggles do card não existem).

**Gaps de coop (Fika PVE):** mirror-move pode quarentenar `Fika.Core.dll`; excluir conta do host no meio de sessão coop; toggle de opcionais falha em silêncio por base-URL errada.

---

## 🔴 Bloqueadores

### B1 — Auto-update: RCE (TLS off + sem verificação de assinatura)
`LauncherUpdateHelper.cs:22` e `:76`
- **Problema:** `CheckAndUpdateAsync`/`DownloadAndPatchAsync` usam `ServerCertificateCustomValidationCallback = (...) => true` (aceita qualquer certificado) e baixam `SPT.Launcher_Update.exe`, executado por um `.bat` que substitui o launcher **sem checar hash/assinatura**. A `serverUrl` vem de um Gist público (`ConnectServerViewModel.cs:54`).
- **Falha:** MITM na rede (ou Gist/DNS comprometido) responde `/redline/launcher/version` com versão maior e serve exe arbitrário em `/redline/launcher/download`. Como o cert não é validado, nem precisa de cert válido → **execução remota de código** na máquina do jogador.
- **Fix:** pinar cert/chave pública do servidor **e** verificar assinatura (ou hash assinado) do exe antes de rodar o `.bat`. Bloqueia distribuição em produção.

### B2 — `deleteFiles` do manifesto sem guard de raiz (traversal → apaga arquivo do SO)
`ProfileViewModel.cs:644-659`
- **Problema:** a lista `deleteFiles` é processada **fora** do engine, com `Path.Combine(gamePath, deleteFile)` + `DeleteToRecycleBin` — sem `ResolveUnderRoot`. É o modelo de ameaça que o engine passou a defender em CR-01-05, mas este caminho legado bypassa o guard.
- **Falha:** server adulterado envia `deleteFiles: ["../../Windows/System32/kernel32.dll"]` ou caminho absoluto → `Path.Combine` resolve o `..`/absoluto → arquivo do usuário/SO apagado (p/ lixeira). Roda **automático em todo login**.
- **Fix:** passar cada `deleteFile` pelo mesmo `ResolveUnderRoot`/contenção sob GameRoot antes de deletar (ou rotear como ação `DeleteExtra` do engine).

### B3 — `SettingsView` não migrou ao Design System
`SettingsView.axaml` (hex em `:30,36,41,47,49,55,63,72,147,179,190,213`; literais `White/LightGray/Gray` em `:20,56,64,119,124,141,144,152,184,206,210,218,222,223`)
- **Problema:** a tela define paleta própria (`#1A1A1A`, `#222`, `#F2111111`, `#333`, `#111`…) e cores literais em vez de tokens `Trl*`, recria `SidebarMenu`/`PanelCard`/`CleanupButton` divergentes. É o maior furo de pureza do launcher, ao lado de dialogs 100% migrados.
- **Fix:** migrar para `TrlBgPanel/Raised/Input`, `TrlFg*`, `TrlEdge*`; sidebar → `cc:TrlSidebarNav`; cards → `cc:TrlPanel`.

---

## 🟡 Importantes

### Segurança
- **Authkey do Tailscale reusável embutida no binário** — `TailscaleHelper.cs:17` (`FallbackAuthKey`). Qualquer um com o exe entra na tailnet do servidor coop. → usar authkeys efêmeras via endpoint autenticado.
- **`/redline/profile/get` devolve senha em plaintext** a quem postar o username — `PasswordController.cs:275-281` (contorna o gate D2). (Server-side.)

### Regras de negócio / aceite
- **005 — colisão case-insensitive grava senha no perfil errado.** Core registra username case-**sensitive** ("Bob"≠"bob"), mas a escrita casa por `OrdinalIgnoreCase` no **1º arquivo enumerado** e a chave do cofre é `ToLowerInvariant()` → duas contas colidem na mesma entrada; migração lazy apaga a variante da outra. `PasswordController.cs:152,174-186`. **Gate humano:** inspecionar `redline_passwords.json` de produção antes do deploy.
- **010 — delete não-atômico.** Cofre é limpo (`ChangePasswordAsync("")`) **antes** do `RemoveAsync`; se o remove falhar/`NoConnection` entre as 2 requests, a conta sobrevive no server **sem senha**. `ProfileViewModel.cs:1112-1118`.
- **009 — 2 dos 4 toggles do card não existem.** `optionalGroups` do server tem só `gore`/`grass`/`hollywood`; templates `PiPDisable/` e `IRL/` ficam órfãos (nunca renderizam). `config.json:26-58` × `ProfileViewModel.cs:616-627`. Gap de **conteúdo do server**, não do launcher.
- **009 — descrição nova só alcança `hollywood`.** Com os nomes de pasta entregues, o join casa só `hollywood`; `gore` cai no fallback do `config.json` antigo, `grass` nem tem template. "Descrição em todos" só passa via fallback legado. Depende de renomear pastas (P-009.3).
- **009 — PiP × ExternalResolution é só texto**, não lógica (`PiPDisable/description.json:4-5`); deferido P-009.1 (teste in-game).
- **006 — bypass Dev Mode persistido nunca mostra a UX de erro.** Instalação com `IsDevMode=true` salvo cai no branch "prossegue sem VPN"; auto-reset que forçaria `false` está comentado. `ConnectServerViewModel.cs:85-89`, `LauncherSettingsProvider.cs:367,56-61`.
- **007 — cancelar não visível na "verificação".** `CanCancelUpdate=true` só após o fetch do manifesto; durante os 5×3s de retry + countdown de 30s o botão fica oculto. `ProfileViewModel.cs:663` vs `560-593`.
- **007 — link "X arquivos atualizados" some em run só-seed.** `HasLastUpdate = updatedCount>0`; login que só semeia/preserva/move grava o `last-update.json` mas não mostra o link. `ProfileViewModel.cs:735,927-931`.
- **013 — footers Login/Register são `x:Static` read-once.** Falha **transitória** do fetch no connect congela `"—"` a sessão inteira nessas telas (só ProfileView se recupera). `LoginView.axaml:62`.
- **Confirmação frágil de wipe/remove.** `WipeConfirmCommand`/`RemoveProfileCommand` usam `if (result is bool b && !b) return;` — só abortam com `false`; `null`/não-bool **prosseguem**. `ProfileViewModel.cs:1088,1182`. Hoje protegido (diálogo só retorna bool), mas é trap latente (ESC/click-away passa a resetar/remover sem confirmar). → alinhar com `DeleteAccountCommand` (`is not bool ... || !...`).
- **Comandos `async Task` ligados como método → exceções não observadas.** `StartGameCommand` (e Wipe/Delete/ChangeEdition) — exceção após o 1º `await` some e deixa flags presas (`GameRunning`/`AllowSettings`) → JOGAR/settings travados até reiniciar. `ProfileViewModel.cs:960`.

### Thread-safety de UI
- **`ConnectServer` atualiza props bound fora da UI thread.** Todo o método roda em `Task.Run`; `SetProperty` dispara `PropertyChanged` sem marshalling; `Progress<int>` postado na pool. `ConnectServerViewModel.cs:32,39,107`. → `Dispatcher.UIThread.Post`.
- **Toggle de opcional faz I/O + MD5 síncronos na UI thread.** `OnOptionalToggled` inicia na UI thread e sem `ConfigureAwait(false)`; `WriteAllBytes`/`CreateDirectory`/MD5 rodam na UI. `OptionalModsHelper.cs:255,354,368`.

### Motor de sync — caminhos legados fora do engine
- **`OptionalModsHelper` — traversal + write não-atômico + delete permanente.** `Path.Combine(GamePath, file.path)` sem guard; `File.WriteAllBytes` direto (sem temp+move); remoção com `File.Delete` (**não vai p/ lixeira**). `OptionalModsHelper.cs:234-255,301-303,351-354`. → reusar `SyncEngine`/`ResolveUnderRoot`+atômico+lixeira.
- **`GetServerBaseUrl` derruba porta e força http.** Retorna `http://{host}` (porta 80) a partir de `https://host:6969`; toggle de opcional bate em `http://host/launcher/mods/...` → cada arquivo lança exceção engolida como Warning → mod aparece "ativado" mas nada baixa. `OptionalModsHelper.cs:45-57`. **Gap de coop.**

### Coop-sync (Fika PVE)
- **Mirror-move quarentena plugin client-only ausente do manifesto.** Fallback marca `plugins`/`patchers` como `mirror-move-disabled` sem `folderRules`; qualquer arquivo sob `plugins` fora do manifesto vai p/ `plugins-disabled`. Se `Fika.Core.dll` não estiver no manifesto → coop quebra (recuperável mas silencioso). `SyncRuleResolver.cs:32-35` + `SyncPlanner.cs:253-263`. → garantir Fika no manifesto/`ignoredFiles`.
- **010 — excluir conta do host durante sessão coop.** Botão gated só no estado **local** (`!GameRunning && !IsUpdating`); remove `{id}.json` server-side no meio da raid dos clientes. `ProfileView.axaml:162`.
- **006 — auth headless dos clientes extras depende de authkey reusável/`--unattended`.** Se single-use, só o 1º cliente entra headless. Gate operacional (não verificável em código).

### Design System / legibilidade (inclui mudanças recentes)
- **Painéis de auth a 50% sobre Taquila crua, SEM overlay.** Login/Register aplicam `bg1` full-bleed sem o `TrlPhotoOverlayBrush` que ClassSelection/Profile têm; `tokens.css:257-259` normatiza scrim de ~86-93%. Labels `trl-muted` (#9B9A96) / footer `faint` sobre regiões claras da arte podem cair abaixo de AA. `Tokens.axaml:82`, `LoginView.axaml:19`, `RegisterView.axaml:18`. → adicionar `TrlPhotoOverlayBrush` atrás do painel de auth **ou** subir o alpha para ~90% (`#E61B1B1D`).
- **Top bar a 30% — glifos min/close borderline** sobre arte clara (`ButtonForeground = TrlFgMuted #9B9A96`). `Legacy.axaml:57-58`, `Tokens.axaml`. → subir alpha da barra (~50%) ou usar `TrlFg` (não muted) nos glifos.
- **Duas sidebars diferentes** para o mesmo menu: `ProfileView` usa `cc:TrlSidebarNav` (280px, token-puro); `SettingsView` usa `Border #111111` (250px, `Button.SidebarMenu`). `SettingsView.axaml:72`. → unificar em `cc:TrlSidebarNav`.
- **`ModInfoView`+`ModInfoCard`+`TotalModsCard` presos em classes legadas** (`.card/.acc/.alt`), alcançáveis via `OpenModsInfoCommand`; sustentam os shims do `Legacy.axaml` que deveriam morrer no 014. `ModInfoView.axaml:22,60,79`.
- **Cores da barra da notificação são nomes crus do Avalonia** (`DodgerBlue/Gold/ForestGreen/IndianRed/Gray`). `SPTNotificationViewModel.cs:22,27,32,37,42`. → mapear p/ tokens.
- **Cor do dot Dev Mode vem do VM como hex cru** (`#4CAF50`/`#555555`) + `Border CornerRadius="10"`. `SettingsViewModel.cs:50`, `SettingsView.axaml:195`.

---

## 🟢 Menores (seleção)

- **5 custom controls legados órfãos** (código morto, não instanciados): `ProfileCard`, `DetailedProfileCard`, `TotalModsCard`, `GameLaunchBar`, `LoginBox` (com literais `IndianRed`/`Gray`). → deletar.
- **Código morto com risco próprio:** `WireGuardHelper` (bypass TLS `:153` + `WaitForExit` bloqueante), `FikaConfigHelper`, `ProfileViewModel.GameVersionCheck`. → remover (reduz superfície).
- **Senha de Dev Mode hardcoded** — `SettingsViewModel.cs:30` (`"Redline123"`).
- **`ModUpdateView.axaml:47` — `CornerRadius="4"`** (único radius ≠ 0 em view migrada).
- **Texto on-danger literal `White`** — `Button.axaml:70,76,81`, `TitleBar.axaml:90` (sem `TrlFgOnDanger`).
- **`ImageSourceConverter` decodifica bitmap na UI thread e não descarta os antigos** — `:29` (hitch/churn com fundos grandes tipo Taquila).
- **Cancelamento não aborta o arquivo em voo** (`WebRequest` síncrono em `Task.Run`); comentário do engine impreciso. `SyncEngine.cs:116-118`, `RequestHandler.cs:206-227`.
- **`ResolveUnderRoot` não resolve symlink/junction** dentro da raiz. `SyncEngine.cs:248-258` (exige link pré-plantado).
- **MD5 como âncora de integridade** (colisão forjável) — manifesto+baseline. `SyncPathUtil.cs:88-110`.
- **`managedPaths` sem teto** — server pode pedir deleção ampla. `SyncPlanner.cs:264-274`.
- **Race latente dois-engines** sobre o mesmo `sync-state.json` (só se `ModUpdateView` voltar à navegação). `ProfileViewModel.cs:472` + `ModUpdateViewModel.cs:173-296`.
- **004:** fundo `bg1` diverge de `bg-hero` que spec/asbuild/code-review afirmam (`ClassSelectionView.axaml:18`); cache de ícone por basename sem guarda; string PT hardcoded (`ClassSelectionViewModel.cs:137`).
- **005:** `password_debug_log.txt` a cada request sem rotação (`PasswordController.cs:104,240`); item sem `01-spec`/`05-asbuild`.
- **013:** `Request.Send()` retorna `null` em erro → NRE como controle de fluxo.
- **014:** literal de fallback `"2.0.0"` duplicado em `LauncherUpdateHelper.cs:14`.
- **`GetExistingProfiles`** popula `ExistingProfiles` que a `LoginView` redesenhada não renderiza (trabalho desperdiçado). `LoginViewModel.cs:231-289`.
- **Log mente:** `Substring(0,8)` de hash curto lança e loga "falha ao salvar" apesar de já ter salvo. `ProfileViewModel.cs:770`.

---

## Verificado e OK (não são defeitos)
- **P/Invoke DWM** (`MainWindow.axaml.cs:23-54`): marshalling, guarda de plataforma, handle nulo, HRESULT ignorado — corretos.
- Guard `ResolveUnderRoot` do engine cobre `..`/absoluto/sibling-prefix; applies atômicos same-volume; baseline grava hash real; **016** (média = soma-bytes/soma-tempo, guardas div/0) e **017** (seed nunca sobrescreve/deleta, TOCTOU re-check, no-op sem `config-server`, case-insensitive) corretos.
- **017 × 007 reconciliados no código:** `config-server` no fallback = `seed-if-missing`; mirror-delete morto (só via `folderRules` explícito, que desliga o seed).
- Fallback de classes → editions vanilla (try/catch/finally). **012** removido 100%. **014** versão single-source. Tema/dialogs migrados, radius 0, vermelho disciplinado.

---

## Recomendação de sequência

1. **Antes de qualquer distribuição em produção:** B1 (RCE auto-update) + B2 (`deleteFiles` guard). São os dois que causam dano real na máquina do jogador.
2. **Antes do deploy da DLL do PasswordController:** 005 (inspecionar `redline_passwords.json` + corrigir match/cofre) e o plaintext de `/redline/profile/get`.
3. **Robustez rápida (baixo custo):** confirmação de wipe/remove (`is not bool`), `async Task` commands com try/catch restaurando flags, `GetServerBaseUrl` preservando porta/esquema, thread-safety do ConnectServer.
4. **Coop:** garantir Fika no manifesto; gate de excluir-conta considerando sessão remota.
5. **Conteúdo do server (009):** adicionar grupos `PiPDisable`/`IRL` no `config.json` + alinhar nomes de pasta com os templates.
6. **DS:** B3 (migrar `SettingsView`), deletar os 5 controls órfãos, corrigir `ModUpdateView:47`, reforçar scrim das telas de auth (mudança recente), unificar sidebar.

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-04 | Guilherme | Criação — auditoria consolidada (código/produto/DS) via 4 revisões paralelas. |
