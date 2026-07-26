using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using SPT.Launcher.Attributes;
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
        // Estado inicial de cada id ao abrir a tela — o diff contra ele decide o que aplicar (CC-18).
        private readonly Dictionary<string, bool> _initialState = new();

        public ObservableCollection<OptionalItemToggle> OptionalMods { get; } = new();
        public ObservableCollection<OptionalItemToggle> PerformanceItems { get; } = new();

        public bool HasOptionalMods => OptionalMods.Count > 0;
        public bool HasPerformanceItems => PerformanceItems.Count > 0;

        public ReactiveCommand<Unit, Unit> ToggleAllOptionalCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleAllPerformanceCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveAndReturnCommand { get; }

        public ModsConfigsViewModel(IScreen Host, bool onboarding = false) : base(Host)
        {
            _onboarding = onboarding;

            ToggleAllOptionalCommand = ReactiveCommand.Create(() => ToggleAll(OptionalMods));
            ToggleAllPerformanceCommand = ReactiveCommand.Create(() => ToggleAll(PerformanceItems));
            SaveAndReturnCommand = ReactiveCommand.Create(SaveAndReturn);

            LoadItems();

            if (_onboarding)
            {
                // CA-030.18: modal one-shot explicando a escolha (não bloqueia; roda após a tela montar).
                var _ = ShowDialog(new OnboardingDialogViewModel());
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
                // Onboarding (D-5): tudo LIGADO. Fora dele: o estado salvo. Item novo (não visto): desligado (D-6).
                bool enabled = _onboarding || settings.IsOptionalEnabled(def.Id);
                bool isNew = !_onboarding && !settings.SeenItemIds.Contains(def.Id);
                OptionalMods.Add(BuildToggle(def, enabled, isNew, preferPt));
            }

            foreach (var def in ModsConfigCatalog.PerformanceItems)
            {
                // Onboarding (D-5): performance DESLIGADA (o modal recomenda ligar quem tem máquina fraca).
                bool enabled = !_onboarding && settings.IsPerformanceItemEnabled(def.Id);
                bool isNew = !_onboarding && !settings.SeenItemIds.Contains(def.Id);
                PerformanceItems.Add(BuildToggle(def, enabled, isNew, preferPt));
            }

            this.RaisePropertyChanged(nameof(HasOptionalMods));
            this.RaisePropertyChanged(nameof(HasPerformanceItems));
        }

        private OptionalItemToggle BuildToggle(ModsConfigCatalog.Item def, bool enabled, bool isNew, bool preferPt)
        {
            _initialState[def.Id] = enabled;
            return new OptionalItemToggle
            {
                Id = def.Id,
                IsPerformance = def.IsPerformance,
                Name = def.ResolveName(preferPt),
                Description = def.ResolveDescription(preferPt),
                IsEnabled = enabled,
                IsNew = isNew,
            };
        }

        private static void ToggleAll(ObservableCollection<OptionalItemToggle> items)
        {
            // "todos": se algum está desligado, liga todos; senão desliga todos (D-11).
            bool anyOff = items.Any(i => !i.IsEnabled);
            foreach (var i in items) i.IsEnabled = anyOff;
        }

        /// <summary>
        /// Sair da tela (§5.6). NÃO sincroniza: grava as escolhas + a intenção pendente, marca os itens
        /// como vistos e o onboarding como concluído, e volta pra aba Launcher — que detecta PendingApply
        /// e aplica. No onboarding, sempre dispara a ingestão inicial (CA-030.19), mesmo sem alteração.
        /// </summary>
        private void SaveAndReturn()
        {
            var settings = LauncherSettingsProvider.Instance;
            var all = OptionalMods.Concat(PerformanceItems).ToList();

            int changedCount = 0;
            foreach (var item in all)
            {
                bool was = _initialState.TryGetValue(item.Id, out var v) && v;
                bool persisted = item.IsPerformance
                    ? settings.IsPerformanceItemEnabled(item.Id)
                    : settings.IsOptionalEnabled(item.Id);

                // Persiste sempre no onboarding (grava o estado inicial aceito); fora dele, quando mudou
                // em relação ao que está salvo. Marca pra aplicar quando o estado EFETIVO mudou.
                if (_onboarding || item.IsEnabled != persisted)
                {
                    if (item.IsPerformance) settings.EnabledPerformanceItems[item.Id] = item.IsEnabled;
                    else settings.EnabledOptionals[item.Id] = item.IsEnabled;
                }

                if (item.IsEnabled != persisted)
                {
                    if (!settings.PendingApply.Contains(item.Id)) settings.PendingApply.Add(item.Id);
                    changedCount++;
                }
                else if (item.IsEnabled != was)
                {
                    changedCount++; // liga-desliga que voltou ao salvo: conta pra decisão, sem re-aplicar
                }

                // CA-030.11: item visto ao SAIR da tela (não ao abrir).
                if (!settings.SeenItemIds.Contains(item.Id)) settings.SeenItemIds.Add(item.Id);
            }

            // PA-02-07: marca de onboarding e estado gravados SEMPRE, fora de qualquer early-return.
            bool firstRun = _onboarding && !settings.ModsConfigsOnboardingDone;
            if (_onboarding) settings.ModsConfigsOnboardingDone = true;

            settings.SaveSettings();

            // CA-030.22: fora do onboarding, sem alteração não dispara sync. No 1º acesso dispara sempre
            // (CA-030.19) — é a ingestão que instala os mods.
            if (changedCount > 0 || firstRun)
            {
                settings.PendingApply.Add(SyncTriggers.PendingApplyMarker);
                settings.SaveSettings();
            }

            NavigateBack();
        }
    }

    /// <summary>Item 030: marca sentinela em PendingApply que sinaliza ao ProfileViewModel para sincronizar.</summary>
    public static class SyncTriggers
    {
        public const string PendingApplyMarker = "__apply__";
    }
}
