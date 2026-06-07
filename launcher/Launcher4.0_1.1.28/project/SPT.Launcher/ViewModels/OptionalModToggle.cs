using ReactiveUI;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Modelo de toggle para mods opcionais na UI.
    /// Gerado dinamicamente a partir do manifesto do servidor.
    /// </summary>
    public class OptionalModToggle : ReactiveObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => this.RaiseAndSetIfChanged(ref _isEnabled, value);
        }
    }
}
