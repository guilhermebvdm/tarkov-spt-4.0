using ReactiveUI;

namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Item 030: um item da tela "Mods e Configs" — serve os DOIS eixos (mod opcional e config de
    /// performance). Gerado a partir do manifesto (optionalMods[] / optionalConfigs[]).
    /// </summary>
    public class OptionalItemToggle : ReactiveObject
    {
        public string Id { get; set; }

        /// <summary>true = eixo de configs opcionais; false = eixo de mods opcionais.</summary>
        public bool IsOptionalConfig { get; set; }

        /// <summary>Item 030: id da categoria do item (usado pra agrupar na tela). null → grupo default.</summary>
        public string Category { get; set; }

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

        /// <summary>Item novo (não visto pelo player) — mostra o marcador de novidade (CA-030.11/D-6).</summary>
        private bool _isNew;
        public bool IsNew
        {
            get => _isNew;
            set => this.RaiseAndSetIfChanged(ref _isNew, value);
        }
    }
}
