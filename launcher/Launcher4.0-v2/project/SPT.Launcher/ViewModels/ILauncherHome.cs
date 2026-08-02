namespace SPT.Launcher.ViewModels
{
    /// <summary>
    /// Marcador da tela "home" do launcher (o Launcher/Profile) para a navegação por menu. O
    /// <see cref="ViewModelBase.NavigateMenu"/> procura por esta interface na navigation stack para saber
    /// até onde "voltar" antes de abrir a tela do item clicado — mantendo a pilha rasa e a navegação
    /// determinística sem a base conhecer o tipo concreto.
    /// </summary>
    public interface ILauncherHome
    {
    }
}
