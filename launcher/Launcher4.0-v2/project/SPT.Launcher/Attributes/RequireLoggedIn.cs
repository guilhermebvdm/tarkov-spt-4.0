using SPT.Launcher.Helpers;
using SPT.Launcher.Models;
using SPT.Launcher.ViewModels;
using ReactiveUI;

namespace SPT.Launcher.Attributes
{
    public class RequireLoggedIn : NavigationPreCondition
    {
        public override NavigationPreConditionResult TestPreCondition(IScreen Host)
        {
            // Verificação local — sem HTTP request
            // Evita loop: timeout → navega ConnectServer → Tailscale down+up → rede instável
            if (AccountManager.SelectedAccount != null 
                && !string.IsNullOrEmpty(AccountManager.SelectedAccount.username))
            {
                return NavigationPreConditionResult.FromSuccess();
            }

            return NavigationPreConditionResult.FromError(LocalizationProvider.Instance.login_failed, new ConnectServerViewModel(Host));
        }
    }
}
