# 030 — Tela "Mods e Configs" · Code review 01 (pré-release 2.8.0)

> **Data:** 2026-08-01<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [01-spec](./030-mods-e-configs-tela-01-spec.md) · [02-spec-tech](./030-mods-e-configs-tela-02-spec-tech.md) · [03-review-02](./030-mods-e-configs-tela-03-spec-tech-review-02.md)<br>

---

## Veredito

Revisão adversarial em **3 dimensões independentes** (motor de sync · servidor/ModUpdater · UI+ViewModel+i18n), rodada sobre o código já integrado à `main` (merge `aedf0c52`), antes de empacotar o launcher **2.8.0**.

**Contadores: 🔴 0 · 🟠 1 · 🟡 7 · 🟢 7**

**Nenhum bloqueador.** O item 030 está pronto para release. O único 🟠 é **operacional** (provisionamento de conteúdo no servidor de prod), não bug de código. Os caminhos coop-críticos (não vazar mod desligado, preservar plugins Fika, convergência do híbrido, precedência determinística) foram verificados corretos e, na maioria, cobertos por teste. i18n em paridade impecável (0 chaves órfãs).

**Nota transversal:** o código foi implementado com naming `optional-config`/`OptionalConfigToConfig`/`optionalConfigId`/`configs-optional.json`; as specs 01/02 ainda dizem `performance`/`performance.json`. Servidor↔cliente estão internamente consistentes — a **spec** é que ficou defasada (drift documentado, não é defeito).

## Índice

| ID | Dim | Cat | Impacto | Título |
|---|---|---|---|---|
| CR-01-01 | Motor | C | 🟡 | Guard coop-safe da quarentena de mods sem teste de regressão |
| CR-01-02 | Motor | C | 🟡 | Desligar config sobre base `config-force` desvia do texto da CA-030.2b |
| CR-01-03 | Motor | B | 🟢 | Colisão de `id` cross-eixo em `JustToggledIds` pode aplicar config à força |
| CR-01-04 | Motor | F | 🟢 | Update de rotina faz backup de versão intocada em `replaced/` (ruído) |
| CR-02-01 | Server | C | 🟠 | Provisionamento: conteúdo de prod precisa usar o naming `config-optional`/`*-optional.json` |
| CR-02-02 | Server | B | 🟡 | Race no `_manifestGenerating` (test-and-set não atômico) |
| CR-02-03 | Server | C | 🟡 | Mod opcional recusado (D-15/D-19) vira **obrigatório para todos** |
| CR-02-04 | Server | C | 🟡 | D-19 tratado diferente entre mods (recusa) e configs (pula arquivo, item fica parcial) |
| CR-02-05 | Server | F | 🟢 | Validações S-5 só vão para `Console.WriteLine` (invisível ao operador) |
| CR-02-06 | Server | B | 🟢 | `_manifestHash` publicado antes de `_manifestCache` (janela 503 no /refresh) |
| CR-03-01 | UI | B | 🟡 | `AllowSettings` fica preso em `false` ao voltar da tela (engrenagem some) |
| CR-03-02 | UI | B | 🟡 | `SaveAndReturn` ignora falha de `SaveSettings()` (persistência falha em silêncio) |
| CR-03-03 | UI | C | 🟢 | `mods_configs_game_running` definida mas nunca usada (tooltip do gate CA-030.23) |
| CR-03-04 | UI | B | 🟢 | Resumo do Profile não re-renderiza na troca de idioma |
| CR-03-05 | UI | E | 🟢 | Modal de onboarding é fire-and-forget não observado |

## Plano de aplicação

- **Launcher (entram no 2.8.0):** CR-01-01, CR-01-04, CR-03-01, CR-03-02, CR-03-03, CR-03-04, CR-03-05 + o teste de CR-01-02.
- **Servidor (deploy separado do mod C#):** CR-02-02, CR-02-03, CR-02-04, CR-02-05, CR-02-06.
- **Gate operacional (Guilherme, no servidor de prod):** CR-02-01.
- **Decisões de design confirmadas:** CR-01-02 (comportamento base-force é intencional — não-destrutivo + coop-correto; documentar + teste), CR-01-03 (unicidade global de `id` garantida via validação no servidor).

---

## Detalhamento

### Motor de sync

**CR-01-01 · C · 🟡** — Guard coop-safe de `QuarantineDisabledOptionalMods` (`SyncPlanner.cs:778-782`) está correto (`IsCoopEssentialPlugin` → preserva + warning), mas o caminho novo (um `Fika.*.dll` taggeado opcional e **desligado**) não tem teste — os testes coop-safe existentes só exercitam o `ScanExtras`. Fix: teste espelhando `Fika_plugin_extra_is_never_quarantined` pelo eixo de opcionais.

**CR-01-02 · C · 🟡** — No branch desligado, quando a base é `config-force`, o canal force **reclama** o alvo (sobrescreve a edição do player, com backup em `config-disabled/`) em vez de "preservar a edição" como o texto da CA-030.2b sugere. Comportamento é **intencional** (não-destrutivo + coop-correto: config forçada volta a valer). Fix: nota na spec + teste do sub-caso base-force.

**CR-01-03 · B · 🟢** — `JustToggledIds` (lista plana) é comparada contra `optionalId` E `optionalConfigId` (`SyncPlanner.cs:288`). `id` repetido cross-eixo faria alternar um mod marcar um item de config como recém-tocado → aplica config sobre a customização. Fix: servidor recusa `id` repetido entre `optionalMods[]` e configs.

**CR-01-04 · F · 🟢** — No branch ligado/intocado, um update de rotina do servidor guarda a versão **anterior intocada** em `optional-config/replaced/` (rótulo D-20 = "config do player") — semanticamente errado e incha a quarentena. Fix: só fazer backup quando há algo do player (`justToggled || !perfMatchesBaseline`).

### Servidor (ModUpdater.cs)

**CR-02-01 · C · 🟠** — O servidor lê de `mods_repo/BepInEx/config-optional/configs-optional.json` e `mods_repo/BepInEx/plugins-optional.json`. Se o operador seguir a spec antiga (`config-performance/`/`performance.json`), `LoadOptionalConfigDefs` retorna vazio (`:335`) e a feature **morre em silêncio**. Gate de deploy: confirmar os arquivos com o naming correto no servidor de prod.

**CR-02-02 · B · 🟡** — `if (_manifestGenerating) return; _manifestGenerating = true;` (`:410-411`) não é atômico → dupla geração concorrente. Estáticos compartilhados não-`volatile`. Fix: `Interlocked.CompareExchange`.

**CR-02-03 · C · 🟡** — Mod opcional recusado (D-15/D-19) tem os prefixos descartados, então seus arquivos casam com "nenhum prefixo opcional" e são emitidos como **normais/obrigatórios para todos** (`:490`). Num coop, um typo empurra gameplay pra todo mundo. Fix: não emitir os arquivos de um mod recusado.

**CR-02-04 · C · 🟡** — D-19 recusa o **mod inteiro** (`:268`) mas nos configs só pula o **arquivo** (`:364`), mantendo o item emitido — item parcialmente possuído na UI. Fix: unificar (descartar/sinalizar o item cujo arquivo foi roubado). (Resíduo: msg em `:362` ainda diz "performance".)

**CR-02-05 · F · 🟢** — Todas as validações S-5 saem só em `Console.WriteLine` (`:568`) — invisível ao operador de prod. Fix: array `contentWarnings` no manifesto.

**CR-02-06 · B · 🟢** — `_manifestHash` setado (`:592`) antes de `_manifestCache` (`:594`); após `/refresh`, janela curta em que o hash novo não tem manifesto buscável (503). Fix: `_manifestCache` antes de `_manifestHash`.

### UI + ViewModel + i18n

**CR-03-01 · B · 🟡** — `OpenModsConfigsCommand` faz `AllowSettings = false` (`ProfileViewModel.cs:314`) mas `SaveAndReturn` nunca restaura → a engrenagem de Configurações **some pelo resto da sessão** ao voltar pelo menu/resumo. Fix: `AllowSettings = true` antes do `NavigateBack()` (espelha `SettingsViewModel.GoBackCommand:271`).

**CR-03-02 · B · 🟡** — `SaveAndReturn` descarta o `bool` de `SaveSettings()` (`ModsConfigsViewModel.cs:189`); falha de disco → prefs/onboarding não persistem, sem aviso, e o sync roda com `PendingApply` só em memória → toggles somem no próximo login. Fix: notificar erro se `!SaveSettings()`.

**CR-03-03 · C · 🟢** — `mods_configs_game_running` ("Feche o jogo antes de aplicar") existe nos 2 locales mas nunca é referenciada; o gate CA-030.23 é só `IsEnabled` sem tooltip. Fix: `ToolTip.Tip` nos botões (Avalonia mostra tooltip em controle desabilitado).

**CR-03-04 · B · 🟢** — `ModsConfigsSummary` é materializado por `string.Format` e só recomputa no `WhenActivated`/pós-sync; não reage a `LocaleChanged` → fica no idioma anterior se trocar com o Profile aberto. Fix: assinar `LocalizationProvider.LocaleChanged`.

**CR-03-05 · E · 🟢** — `var _ = ShowDialog(new OnboardingDialogViewModel())` (`:51`) descarta o Task; exceção some sem log. Fix: `ContinueWith` logando falha.

## Verificado OK (destaques)

- **Precedência optional-config > config-force > config** determinística e testada (`Enabled_performance_suppresses_force_and_reports_it`).
- **Convergência do híbrido:** `OptionalConfigCopy` grava baseline (`SyncEngine.cs:324`); 2º sync sem I/O.
- **Quarentena reusa guards do ScanExtras:** protected/ignored/excluded, Dev Mode, coop-safe Fika, NRE-guard de regra Default; mod-pasta coberto arquivo-a-arquivo.
- **Servidor:** matching pasta E .dll (`IsUnderOrEqual`), D-15/D-18/R-11 corretos, swap atômico do `_fileMapCache`, `TryResolveUnder` barra traversal, resíduos do modelo antigo removidos do servidor C#.
- **i18n:** 16 chaves novas, todas presentes em en.json + pt.json + LocaleData; placeholders corretos; zero texto cru.
- **Onboarding sem loop:** quem aceita defaults conclui (CA-030.19/20/22); PendingApply cleanup remove só snapshot+marker (não Clear).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-01 | Guilherme | Code review 01 (pré-release 2.8.0) — 3 revisores adversariais; 0 🔴, 1 🟠, 7 🟡, 7 🟢. |
