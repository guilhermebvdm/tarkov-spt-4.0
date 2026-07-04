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
    }
}
