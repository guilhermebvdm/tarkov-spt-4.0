using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SPT.Launcher.Attributes;
using SPT.Launcher.Controllers;
using SPT.Launcher.Helpers;
using SPT.Launcher.ViewModels.Dialogs;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Item 030: tela "Mods e Configs" — 2 colunas (mods opcionais | configs de performance), com toggle
    /// "todos" por coluna, estado vazio, e onboarding no primeiro acesso. A tela NÃO sincroniza (PA-01-03):
    /// ao sair, grava as escolhas + a intenção pendente e devolve o player à aba Launcher, que aplica com
    /// o guard de sync existente.
    /// </summary>
    [RequireLoggedIn]
    public class ModsConfigsViewModel : ViewModelBase
    {
        private readonly bool _onboarding;

        // Listas planas (usadas por ToggleAll e SaveAndReturn); a UI renderiza os GRUPOS por categoria.
        public ObservableCollection<OptionalItemToggle> OptionalMods { get; } = new();
        public ObservableCollection<OptionalItemToggle> OptionalConfigs { get; } = new();

        public ObservableCollection<CategoryGroup> OptionalModGroups { get; } = new();
        public ObservableCollection<CategoryGroup> OptionalConfigGroups { get; } = new();

        public bool HasOptionalMods => OptionalMods.Count > 0;
        public bool HasOptionalConfigs => OptionalConfigs.Count > 0;

        public ReactiveCommand<Unit, Unit> GoLauncherCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAllOptionalCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAllOptionalConfigCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAndReturnCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenKofiCommand { get; }

        public ModsConfigsViewModel(IScreen Host, bool onboarding = false) : base(Host)
        {
            _onboarding = onboarding;

            // Menu lateral: Launcher/Settings passam pelo guard de alterações não salvas (Mecanismo 3);
            // "Salvar e voltar" (footer) e Ko-fi (link externo, não navega) ficam fora do guard.
            GoLauncherCommand = ReactiveCommand.CreateFromTask(() => GuardedNavigate(NavigateBack));
            ToggleAllOptionalCommand = ReactiveCommand.Create(() => ToggleAll(OptionalMods));
            ToggleAllOptionalConfigCommand = ReactiveCommand.Create(() => ToggleAll(OptionalConfigs));
            SaveAndReturnCommand = ReactiveCommand.Create(SaveAndReturn);
            OpenSettingsCommand = ReactiveCommand.CreateFromTask(
                () => GuardedNavigate(() => NavigateTo(new SettingsViewModel(HostScreen))));
            OpenKofiCommand = ReactiveCommand.Create(OpenKofi);

            LoadItems();

            if (_onboarding)
            {
                // CA-030.18: modal one-shot explicando a escolha (não bloqueia; roda após a tela montar).
                // CR-03-05: observa a Task descartada — se o DialogHost lançar, a falha é logada em vez de sumir.
                _ = ShowDialog(new OnboardingDialogViewModel())
                    .ContinueWith(t => LogManager.Instance.Exception(t.Exception!),
                        System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private bool IsEnglish =>
            string.Equals(LocalizationProvider.Instance.ietf_tag, "en", StringComparison.OrdinalIgnoreCase);

        private void LoadItems()
        {
            var settings = LauncherSettingsProvider.Instance;
            bool preferPt = !IsEnglish;

            foreach (var def in ModsConfigCatalog.OptionalMods)
            {
                // Item 033 (revisa D-5): no onboarding os toggles refletem o SEED (Mec.1, já gravado antes
                // de abrir esta tela) — mod instalado vem ligado, ausente desligado; sem plugin nenhum, o
                // seed ligou Optional/Heavy/Performance (dev NÃO). Fora do onboarding, idem: o estado salvo.
                // Item novo (não visto): desligado (D-6).
                bool enabled = settings.IsOptionalEnabled(def.Id);
                bool isNew = !_onboarding && !settings.SeenItemIds.Contains(def.Id);
                OptionalMods.Add(BuildToggle(def, enabled, isNew, preferPt));
            }

            foreach (var def in ModsConfigCatalog.OptionalConfigs)
            {
                // Onboarding (D-5): configs opcionais DESLIGADAS (o modal recomenda ligar quem tem máquina fraca).
                bool enabled = !_onboarding && settings.IsOptionalConfigEnabled(def.Id);
                bool isNew = !_onboarding && !settings.SeenItemIds.Contains(def.Id);
                OptionalConfigs.Add(BuildToggle(def, enabled, isNew, preferPt));
            }

            BuildGroups(OptionalMods, ModsConfigCatalog.OptionalModCategories, OptionalModGroups, preferPt);
            BuildGroups(OptionalConfigs, ModsConfigCatalog.OptionalConfigCategories, OptionalConfigGroups, preferPt);

            this.RaisePropertyChanged(nameof(HasOptionalMods));
            this.RaisePropertyChanged(nameof(HasOptionalConfigs));
        }

        private OptionalItemToggle BuildToggle(ModsConfigCatalog.Item def, bool enabled, bool isNew, bool preferPt)
        {
            return new OptionalItemToggle
            {
                Id = def.Id,
                IsOptionalConfig = def.IsOptionalConfig,
                Category = def.Category,
                Name = def.ResolveName(preferPt),
                Description = def.ResolveDescription(preferPt),
                IsEnabled = enabled,
                IsNew = isNew,
            };
        }

        /// <summary>
        /// Item 030: agrupa os toggles por categoria — as categorias declaradas (na ordem `order`) primeiro,
        /// depois um grupo "Outros" com os itens sem categoria ou com categoria desconhecida. Grupo vazio
        /// não aparece. Um único grupo sem nome (só "Outros") não mostra subtítulo (evita ruído).
        /// </summary>
        private static void BuildGroups(
            IEnumerable<OptionalItemToggle> toggles,
            IReadOnlyList<ModsConfigCatalog.Category> categories,
            ObservableCollection<CategoryGroup> groups,
            bool preferPt)
        {
            groups.Clear();
            var list = toggles.ToList();
            var byCat = list.ToLookup(t => t.Category ?? string.Empty);
            var known = new HashSet<string>(categories.Select(c => c.Id));

            foreach (var cat in categories.OrderBy(c => c.Order))
            {
                var items = byCat[cat.Id].ToList();
                if (items.Count == 0) continue;
                groups.Add(new CategoryGroup(cat.ResolveName(preferPt), items));
            }

            var orphans = list.Where(t => string.IsNullOrEmpty(t.Category) || !known.Contains(t.Category)).ToList();
            if (orphans.Count > 0)
            {
                // Se NÃO há categorias declaradas, o único grupo não recebe título (lista simples).
                string name = groups.Count == 0 && categories.Count == 0
                    ? null
                    : LocalizationProvider.Instance.mods_configs_uncategorized;
                groups.Add(new CategoryGroup(name, orphans));
            }
        }

        private static void ToggleAll(ObservableCollection<OptionalItemToggle> items)
        {
            // "todos": se algum está desligado, liga todos; senão desliga todos (D-11).
            bool anyOff = items.Any(i => !i.IsEnabled);
            foreach (var i in items) i.IsEnabled = anyOff;
        }

        /// <summary>Botão "Salvar e voltar" (footer): fluxo explícito — sempre salva e volta, sem guard.</summary>
        private void SaveAndReturn()
        {
            SaveChanges();
            NavigateBack();
        }

        /// <summary>
        /// Item 033 (Mecanismo 3): navega para fora da tela por um item do menu protegendo alterações não
        /// salvas. Dispara o diálogo se há alteração pendente OU no onboarding (CA-033.10 — garante que o
        /// jogador viu/decidiu na 1ª vez). Sem alteração e fora do onboarding: navega direto (CA-033.9,
        /// caso "entrei do Launcher, não mexi, clico Launcher de novo"). Cancelar aborta a navegação.
        /// </summary>
        private async Task GuardedNavigate(Action navigate)
        {
            if (_onboarding || HasUnsavedChanges())
            {
                var choice = await ShowConfirmDialog();
                if (choice == ConfirmUnsavedChoice.Cancel) return;   // fica na tela; toggles intactos
                if (choice == ConfirmUnsavedChoice.Save) SaveChanges();
                // Discard: não persiste — as escolhas em memória somem ao trocar de tela (CA-033.8/CC-14);
                // ao voltar, o novo VM relê do settings (inalterado), então o descarte é efetivo.
            }

            navigate();
        }

        /// <summary>
        /// Item 033 (CC-8): algum toggle diverge do salvo? Delega à lógica pura de <c>OptionalToggleState</c>
        /// (via Settings), então é o mesmo critério testado sem UI.
        /// </summary>
        public bool HasUnsavedChanges()
        {
            var current = OptionalMods.Concat(OptionalConfigs)
                .Select(t => (t.Id, t.IsOptionalConfig, t.IsEnabled));
            return LauncherSettingsProvider.Instance.HasUnsavedOptionalChanges(current);
        }

        /// <summary>Exibe o modal de 3 botões e mapeia o token do DialogHost ao enum (null/fechar → Cancel).</summary>
        private async Task<ConfirmUnsavedChoice> ShowConfirmDialog()
        {
            var result = await ShowDialog(new ConfirmUnsavedDialogViewModel()) as string;
            return result switch
            {
                ConfirmUnsavedDialogViewModel.SaveResult => ConfirmUnsavedChoice.Save,
                ConfirmUnsavedDialogViewModel.DiscardResult => ConfirmUnsavedChoice.Discard,
                _ => ConfirmUnsavedChoice.Cancel,   // clicou fora/ESC/Cancelar → não navega
            };
        }

        /// <summary>Abre a página de apoio (Ko-fi) no navegador — não sai da tela.</summary>
        private void OpenKofi() => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "https://ko-fi.com/umbigopreto",
            UseShellExecute = true
        });

        /// <summary>
        /// Persiste as escolhas + a intenção pendente, marca itens vistos e o onboarding concluído. NÃO
        /// sincroniza (§5.6) — quem aplica é o ProfileViewModel ao detectar PendingApply. Compartilhado por
        /// SaveAndReturn (→ Launcher) e OpenSettings (→ Configurações): sair por qualquer item do menu
        /// preserva as escolhas. No onboarding, dispara a ingestão inicial (CA-030.19) mesmo sem alteração.
        /// </summary>
        private void SaveChanges()
        {
            var settings = LauncherSettingsProvider.Instance;
            var all = OptionalMods.Concat(OptionalConfigs).ToList();

            int changedCount = 0;
            foreach (var item in all)
            {
                bool persisted = item.IsOptionalConfig
                    ? settings.IsOptionalConfigEnabled(item.Id)
                    : settings.IsOptionalEnabled(item.Id);

                // Persiste sempre no onboarding (grava o estado inicial aceito); fora dele, só quando mudou
                // em relação ao salvo. Só o estado EFETIVO conta — liga-desliga que voltou ao salvo não
                // dispara sync (o "was" da versão anterior era sempre == persisted, ramo morto — 🟢 CR).
                if (_onboarding || item.IsEnabled != persisted)
                {
                    if (item.IsOptionalConfig) settings.EnabledOptionalConfigs[item.Id] = item.IsEnabled;
                    else settings.EnabledOptionals[item.Id] = item.IsEnabled;
                }

                if (item.IsEnabled != persisted)
                {
                    if (!settings.PendingApply.Contains(item.Id)) settings.PendingApply.Add(item.Id);
                    changedCount++;
                }

                // CA-030.11: item visto ao SAIR da tela (não ao abrir).
                if (!settings.SeenItemIds.Contains(item.Id)) settings.SeenItemIds.Add(item.Id);
            }

            // PA-02-07: marca de onboarding e estado gravados SEMPRE, fora de qualquer early-return.
            bool firstRun = _onboarding && !settings.ModsConfigsOnboardingDone;
            if (_onboarding) settings.ModsConfigsOnboardingDone = true;

            // CA-030.22: fora do onboarding, sem alteração não dispara sync. No 1º acesso dispara sempre
            // (CA-030.19). O marker sinaliza ao ProfileViewModel que há o que aplicar (🟢 CR: com guard de
            // duplicata + um único SaveSettings).
            if ((changedCount > 0 || firstRun) && !settings.PendingApply.Contains(SyncTriggers.PendingApplyMarker))
            {
                settings.PendingApply.Add(SyncTriggers.PendingApplyMarker);
            }

            // CR-03-01: OpenModsConfigsCommand desliga AllowSettings ao abrir; restaura ao sair (espelha
            // SettingsViewModel.GoBackCommand) — senão a engrenagem de Configurações some pela sessão.
            settings.AllowSettings = true;
            // CR-03-02: falha de gravação não pode passar silenciosa — as prefs/onboarding não persistiriam
            // e o sync rodaria só com o PendingApply em memória (toggles somem no próximo login).
            if (!settings.SaveSettings())
                SendNotification("", LocalizationProvider.Instance.failed_to_save_settings,
                    Avalonia.Controls.Notifications.NotificationType.Error);
        }
    }

    /// <summary>Item 030: um grupo de itens sob uma categoria na tela "Mods e Configs".</summary>
    public sealed class CategoryGroup
    {
        public string Name { get; }
        public bool HasName => !string.IsNullOrEmpty(Name);
        public ObservableCollection<OptionalItemToggle> Items { get; }

        public CategoryGroup(string name, System.Collections.Generic.IEnumerable<OptionalItemToggle> items)
        {
            Name = name;
            Items = new ObservableCollection<OptionalItemToggle>(items);
        }
    }

    /// <summary>Item 030: marca sentinela em PendingApply que sinaliza ao ProfileViewModel para sincronizar.</summary>
    public static class SyncTriggers
    {
        public const string PendingApplyMarker = "__apply__";
    }
}
