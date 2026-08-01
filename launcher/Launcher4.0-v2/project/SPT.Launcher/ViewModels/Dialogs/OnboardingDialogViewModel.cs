using SPT.Launcher.Helpers;
using ReactiveUI;

namespace SPT.Launcher.ViewModels.Dialogs
{
    /// <summary>
    /// Item 030 (CA-030.18): modal one-shot do onboarding — explica a escolha de mods/configs no primeiro
    /// acesso. Só "OK" (fecha). Host = null quando exibido via <see cref="ViewModelBase.ShowDialog(object)"/>.
    /// </summary>
    public class OnboardingDialogViewModel : ViewModelBase
    {
        public string Title => LocalizationProvider.Instance.onboarding_title;
        public string Body => LocalizationProvider.Instance.onboarding_body;
        public string OkText => LocalizationProvider.Instance.onboarding_ok;

        public OnboardingDialogViewModel() : base(null)
        {
        }
    }
}
