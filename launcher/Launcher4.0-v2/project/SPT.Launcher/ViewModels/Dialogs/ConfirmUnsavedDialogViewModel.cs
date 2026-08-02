using SPT.Launcher.Helpers;

namespace SPT.Launcher.ViewModels.Dialogs
{
    /// <summary>Item 033 (Mecanismo 3): resultado do diálogo de alterações não salvas.</summary>
    public enum ConfirmUnsavedChoice
    {
        Cancel,
        Save,
        Discard,
    }

    /// <summary>
    /// Item 033 (CA-033.8/9/10): modal exibido ao sair da tela "Mods e Configs" por um item do menu quando
    /// há alteração pendente (ou sempre, no onboarding). 3 botões — [Salvar e sair] / [Descartar e sair] /
    /// [Cancelar]. Cada botão fecha o DialogHost devolvendo o token em <see cref="SaveResult"/>/<see
    /// cref="DiscardResult"/>/<see cref="CancelResult"/>; o ViewModel chamador mapeia de volta ao enum.
    /// Host = null (exibido via <see cref="ViewModelBase.ShowDialog(object)"/>), espelhando o OnboardingDialog.
    /// </summary>
    public class ConfirmUnsavedDialogViewModel : ViewModelBase
    {
        // Tokens devolvidos pelo CloseDialogCommand (CommandParameter). Mapeados em ModsConfigsViewModel.
        public const string SaveResult = "save";
        public const string DiscardResult = "discard";
        public const string CancelResult = "cancel";

        public string Title => LocalizationProvider.Instance.confirm_unsaved_title;
        public string Body => LocalizationProvider.Instance.confirm_unsaved_body;
        public string SaveText => LocalizationProvider.Instance.confirm_unsaved_save;
        public string DiscardText => LocalizationProvider.Instance.confirm_unsaved_discard;
        public string CancelText => LocalizationProvider.Instance.confirm_unsaved_cancel;

        public ConfirmUnsavedDialogViewModel() : base(null)
        {
        }
    }
}
