using ReactiveUI;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Modelo de toggle para mods opcionais na UI.
    /// Gerado dinamicamente a partir do manifesto do servidor.
    /// Item 009: Name/Description reativos — o enriquecimento tardio via optionals-list
    /// (description.json por grupo) precisa refletir na UI depois do bind inicial.
    /// </summary>
    public class OptionalModToggle : ReactiveObject
    {
        public string Id { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }

        /// <summary>
        /// Item 021 (R-3): guard consumido uma vez pelo handler do toggle. Quando a UI reverte
        /// <see cref="IsEnabled"/> programaticamente após uma falha total (D-021.A), o Subscribe
        /// re-dispara sincronamente — este flag faz o handler ignorar esse disparo (sem loop de
        /// "desativar"). Não é reativo de propósito: só um sinal transitório de controle.
        /// </summary>
        public bool SuppressToggleHandler { get; set; }
    }
}
