# 033 — Spec técnica: Seed do disco + onboarding universal + guard de alterações

**Mod:** Launcher4.0-v2
**Status:** Backlog
**Criado:** 2026-08-01
**Spec funcional:** [01-spec](./033-onboarding-seed-guard-01-spec.md)

## Visão geral técnica

Três mecanismos, todos no projeto `SPT.Launcher` (client). Nenhum toca o motor de sync `SPT.Launcher.Base/Sync` (o seed só **pré-popula** as preferências que o motor já consome via `IsOptionalModEnabled`). Reusa a base do item 030 sem quebrar contrato.

## Fontes de dado (confirmadas no código)

- `ModsConfigCatalog.OptionalMods` — `Item { Id, Category, ... }`. Tem a **categoria** de cada mod (id: `opcionais`/`pesados`/`performance`/`dev`), mas **não** os `paths`.
- `ManifestFile { path, optional, optionalId, optionalConfigId }` — os `files[]` do manifesto trazem o `optionalId` + `path` de cada arquivo de mod opcional. **É por aqui que o seed detecta "instalado"** (não pelo catálogo).
- `LauncherSettingsProvider`: `EnabledOptionals` (dict público, itemId→bool), `IsOptionalEnabled(id)` (`TryGetValue`), `SetOptionalEnabled(id, bool)` (grava), `ModsConfigsOnboardingDone` (bool), `SeenItemIds`, `IsDevMode`, `GamePath`.
- `ProfileViewModel.CheckForUpdatesCore(manual)`: pega hash → manifesto → `UpdateFromManifest` (`:549`, popula o catálogo) → `ShouldTriggerOnboarding` (`:629`, dentro do sync) → `SyncPlanner` (`:636`). O early-return do Dev Mode está em `:451-458`, **antes** de pegar o manifesto.

## Mecanismo 1 — Seed do disco

**Novo:** `OptionalModSeeder` (classe estática em `SPT.Launcher/Helpers/`), método:
```
static void Seed(IReadOnlyList<ManifestFile> manifestFiles, string gameRoot, LauncherSettingsProvider settings)
```
Lógica (idempotente — só toca ids **não** em `settings.EnabledOptionals`):
1. `hasPlugins` = existe algum `*.dll` em `<gameRoot>/BepInEx/plugins` (recursivo, `SearchOption.AllDirectories`). **Não** olha `plugins-disabled` (CC-13; CC-1c).
2. Agrupar `manifestFiles.Where(f => f.optional && optionalId != null)` por `optionalId` → `paths`.
3. Para cada `optionalId` **não decidido**:
   - Se `hasPlugins`: `installed` = algum `path` do grupo existe em disco (arquivo OU sob a pasta — `File.Exists(root/path) || Directory.Exists(root/path) || qualquer arquivo sob root/path`); `SetOptionalEnabled(id, installed)`.
   - Se `!hasPlugins`: `cat` = `ModsConfigCatalog.OptionalMods.FirstOrDefault(m => m.Id == id)?.Category`; `SetOptionalEnabled(id, cat ∈ {opcionais, pesados, performance})` (a `dev` fica desligada; categoria nula/desconhecida → desligada).
4. **Configs opcionais:** não são semeadas (nunca mexe em `EnabledOptionalConfigs`) — CA-033.5.

**Performance:** `SetOptionalEnabled` grava a cada chamada; para evitar N writes, o seed acumula num dict local e grava uma vez (novo overload `SetOptionalEnabledBatch(IDictionary<string,bool>)` ou popular `EnabledOptionals` direto + 1 `SaveSettings()`). Stub: adicionar `SeedOptionalDefaults(IDictionary<string,bool> seeded)` no provider que faz merge (só ids ausentes) + 1 save.

**Chamada:** em `CheckForUpdatesCore`, logo após `UpdateFromManifest` (`:549`), antes do gatilho de onboarding e do planner. Assim o `SyncPlanner` (`:636`) já lê o estado semeado via `IsOptionalModEnabled` → mod instalado não é quarentenado (CA-033.4). Cobre CC-7 (ordem seed antes do sync).

**Cobre:** CA-033.1, CA-033.2, CA-033.3, CA-033.4, CA-033.5, CC-1/1b/1c, CC-2, CC-3, CC-6, CC-16.

## Mecanismo 2 — Onboarding universal

**Gatilho** (`ProfileViewModel.ShouldTriggerOnboarding`, hoje `:323-331`): trocar a condição para **apenas** `!ModsConfigsOnboardingDone` — remover o teste de "pasta plugins sem .dll" e o de Dev Mode. Dispara para todos, 1x, qualquer versão (CA-033.6, CA-033.7, CC-12).

**Dev Mode × manifesto (ponto técnico central):** o seed e o onboarding precisam do manifesto (catálogo + `files[]`), mas o early-return do Dev Mode (`:451-458`) acontece antes de buscá-lo. **Decisão técnica (assunção registrada):** mover o early-return do Dev Mode para **depois** do bloco `UpdateFromManifest` + seed + gatilho-de-onboarding e **antes** do `SyncPlanner`. Em Dev Mode:
- Busca o manifesto de forma **tolerante** — se o hash/manifesto falhar rápido (servidor fora do ar no login rápido do dev), **pula** o ciclo inteiro sem o retry de 30s (preserva o "login rápido" que motivou o skip do Dev Mode). Só roda seed+onboarding se o manifesto vier.
- Após seed + gatilho, **retorna antes do `SyncPlanner`** — Dev Mode continua **não movendo/aplicando** nada (preserva CC-14 do 030). O onboarding, se disparado, navega para a tela; o dev confirma 1x.

`CheckForUpdatesCore` reestruturado (esboço):
```
devMode = IsDevMode
try pegar hash+manifesto (se devMode e falhar rápido → return sem retry longo)
UpdateFromManifest(...)                       // catálogo
OptionalModSeeder.Seed(allFiles, gameRoot, settings)   // NOVO — semeia
if (ShouldTriggerOnboarding()) { navega tela onboarding; return }
if (devMode && !manual) { IsUpdating=false; IsUpdateVisible=false; return }  // early-return MOVIDO p/ cá
... SyncPlanner + execução (inalterado) ...
```

**Mensagem inicial:** o `OnboardingDialog` (modal existente) ganha o texto explicativo. Novas chaves i18n (en+pt+LocaleData): reusar/estender as `onboarding_title/body/ok` já existentes do 030 (verificar se o texto atual serve; se precisar de um corpo mais explicativo, editar os valores, não criar chave nova).

**Cobre:** CA-033.6, CA-033.7, CA-033.10 (guard no onboarding — ver mec. 3), CC-4, CC-5, CC-10, CC-12, CC-17.

## Mecanismo 3 — Guard de alterações não salvas

**Detecção de "sujo"** (`ModsConfigsViewModel`): método `bool HasUnsavedChanges()` — compara cada `OptionalItemToggle.IsEnabled` com o salvo (`settings.IsOptionalEnabled(id)` / `IsOptionalConfigEnabled(id)`); retorna true se algum diverge. Ligar+desligar de volta = false (CC-8).

**Interceptar navegação por menu:** hoje os botões do sidebar da `ModsConfigsView` chamam `SaveAndReturnCommand` (Launcher), `OpenSettingsCommand`, `OpenKofiCommand`. Trocar por comandos que passam pelo guard:
```
private async void GuardedNavigate(Action navigate):
  bool onboarding = _onboarding;
  if (onboarding || HasUnsavedChanges()):
     var choice = await ShowConfirmDialog();   // [Salvar e sair][Descartar e sair][Cancelar]
     if choice == Cancel: return;
     if choice == Save: SaveChanges();
     if choice == Discard: /* não persiste; reverte toggles p/ o salvo se necessário p/ UI */
     navigate();
  else:
     navigate();
```
- **Launcher** (item ativo do topo hoje é `SaveAndReturnCommand`): vira `GuardedNavigate(NavigateBack)`. **Nota:** o `SaveAndReturn` atual **sempre** salva — mudar para respeitar o guard (só salva se o jogador escolher "Salvar"; o botão "Salvar e voltar" da área central continua sempre salvando, é o fluxo explícito).
- **Settings** (`OpenSettings`): `GuardedNavigate(() => NavigateTo(new SettingsViewModel(HostScreen)))`. Remover o `SaveChanges()` incondicional atual.
- **Buy us a coffee** (`OpenKofi`): abre link externo, **não navega** — não precisa de guard (mantém).
- **No onboarding** (`_onboarding == true`): o guard **sempre** dispara o diálogo (CA-033.10), mesmo sem alteração.

**Novo diálogo:** `ConfirmUnsavedDialog` (view + viewmodel em `Views/Dialogs/` + `ViewModels/Dialogs/`, espelhando o `OnboardingDialog`), 3 botões. Retorna enum `{ Save, Discard, Cancel }`. Novas chaves i18n (en+pt+LocaleData): título, corpo, e os 3 botões.

**Fechar/Logout (CC-14):** decisão = descartar sem aplicar. Como o apply só roda no `ProfileViewModel` ao detectar `PendingApply`, e o guard só grava `PendingApply` no caminho "Salvar", fechar a tela por fora (X/Logout) sem passar pelo Save simplesmente **não** grava nada → nada é aplicado. **Verificar:** que sair da `ModsConfigsView` por Logout/X não dispara `SaveChanges` em lugar nenhum (hoje não dispara; confirmar no `LogoutCommand`).

**Footer fixo (CA-033.11):** na `ModsConfigsView.axaml`, o Grid da área central (`Grid.Column=1`) passa a `RowDefinitions="*, Auto"` — o `ScrollViewer` (conteúdo rolável) na Row 0 e o botão "Salvar e voltar" num painel/`Border` na Row 1 (rodapé fixo, fora do scroll). Move o botão de dentro do `StackPanel` rolável para esse rodapé; o botão continua ligado ao `SaveAndReturnCommand` (fluxo explícito, sempre salva).

**Cobre:** CA-033.8, CA-033.9, CA-033.10, CA-033.11, CC-8, CC-9, CC-11, CC-14.

## Arquivos a tocar

| Arquivo | Mudança |
|---|---|
| `SPT.Launcher/Helpers/OptionalModSeeder.cs` | **novo** — a lógica do seed |
| `SPT.Launcher.Base/Helpers/LauncherSettingsProvider.cs` | `SeedOptionalDefaults(dict)` (merge só-ausentes + 1 save); expor `IsOptionalDecided(id)` se útil |
| `SPT.Launcher/ViewModels/ProfileViewModel.cs` | chamar o seed pós-`UpdateFromManifest`; mover early-return do Dev Mode; ajustar `ShouldTriggerOnboarding`; fetch tolerante em Dev Mode |
| `SPT.Launcher/ViewModels/ModsConfigsViewModel.cs` | `HasUnsavedChanges()`; `GuardedNavigate`; trocar comandos de menu; `SaveAndReturn` respeita guard |
| `SPT.Launcher/Views/Dialogs/ConfirmUnsavedDialogView.axaml(.cs)` | **novo** — diálogo 3 botões |
| `SPT.Launcher/ViewModels/Dialogs/ConfirmUnsavedDialogViewModel.cs` | **novo** |
| `SPT.Launcher/SPT_Data/Launcher/Locales/{English,Portuguese}.json` + `LocalizationProvider` | chaves do diálogo (título/corpo/3 botões) + revisar mensagem do onboarding |

## Testes (SPT.Launcher.Tests)

Novo `OptionalModSeederTests.cs`:
- `Seed_with_plugins_enables_installed_disables_absent` (CA-033.1)
- `Seed_without_plugins_enables_optional_heavy_performance_not_dev` (CA-033.2)
- `Seed_respects_already_decided_ids` (CA-033.3)
- `Seed_never_touches_configs` (CA-033.5)
- `Seed_is_idempotent_on_repeated_runs` (CC-6)
- `Seed_folder_mod_counts_as_installed_by_any_file` (CC-1)
- `Seed_ignores_plugins_disabled_for_hasPlugins_gate` (CC-1c/CC-13)

`ModsConfigsViewModel` guard (se testável sem UI — extrair `HasUnsavedChanges` como pura):
- `HasUnsavedChanges_false_when_toggles_match_saved` (CC-8)
- `HasUnsavedChanges_true_when_a_toggle_differs`

Usar/estender o fixture de disco existente dos testes de sync para o seed. O diálogo em si (Avalonia) e o gatilho de navegação são gate humano/in-game (G-*), não unit.

## Gates humanos (in-game)

- G-1: jogador com mods, sem Dev Mode, 1º login pós-2.8.0 → mods continuam em `plugins/` (nada quarentenado).
- G-2: jogador limpo (sem plugins) → onboarding liga Optional/Heavy/Performance; dev desligado.
- G-3: onboarding aparece 1x para todos (com/sem Dev Mode) e não repete após "Salvar".
- G-4: alterou um toggle → clicou Launcher/Settings → diálogo aparece; não alterou → sai direto.
- G-5: fechar/logout com alteração pendente → nada aplicado.

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Spec técnica criada via `/create-technical-spec` (dentro do `/g-autodev`). Decisão técnica registrada: early-return do Dev Mode movido para depois de catálogo+seed+onboarding, com fetch de manifesto tolerante em Dev Mode. |
| 2026-08-02 | Mec.3 implementado via `/g-autodev`: `OptionalToggleState` (Base, puro/testável) + `Settings.HasUnsavedOptionalChanges`; `ConfirmUnsavedDialog` (VM+View, 3 botões via DialogHost); `GuardedNavigate` no menu Launcher/Settings; footer fixo (`Grid RowDefinitions="*, Auto"`, CA-033.11); onboarding passou a refletir o seed (revisa D-5 — dev off). i18n `confirm_unsaved_*` (en+pt+LocaleData) + mensagem de onboarding reescrita. Commit `b02520b3`. |
| 2026-08-02 | Code-review adversarial (2 lentes independentes). Corrigidos: 🔴 "Descartar" no onboarding não concluía (re-loopava + bloqueava o 1º sync) → agora aceita o seed (reverte + SaveChanges); 🟠 Discard não revertia toggles (VM sobrevive no PUSH p/ Settings) → `RevertTogglesFromSaved`; 🟠 Cancel/Discard idênticos → Cancel isolado à esquerda; 🟡 copy do diálogo neutra. Deferido (dívida 🟢): `GenerateDefaultLocale` não cobre strings TRL (gap pré-existente). Commit `d592c038`. 13 testes do 033 verdes. |
