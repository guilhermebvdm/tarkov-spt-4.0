using SPT.Launcher.Attributes;
using SPT.Launcher.Controllers;
using SPT.Launcher.Models;
using SPT.Launcher.ViewModels.Notifications;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using ReactiveUI;
using Splat;
using System;
using System.Threading.Tasks;
using DialogHostAvalonia;

namespace SPT.Launcher.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel, IRoutableViewModel
    {
        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        protected WindowNotificationManager NotificationManager => Locator.Current.GetService<WindowNotificationManager>();

        public string? UrlPathSegment => Guid.NewGuid().ToString().Substring(0, 7);

        public IScreen HostScreen { get; }

        /// <summary>
        /// Delay the return of the viewmodel
        /// </summary>
        /// <param name="Milliseconds">The amount of time in milliseconds to delay</param>
        /// <returns>The viewmodel after the delay time</returns>
        /// <remarks>Useful to delay the navigation to another view. For instance, to allow an animation to complete.</remarks>
        private async Task<ViewModelBase> WithDelay(int Milliseconds)
        {
            await Task.Delay(Milliseconds);

            return this;
        }

        /// <summary>
        /// Tests all preconditions of a viewmodel
        /// </summary>
        /// <param name="ViewModel"></param>
        /// <returns>The first failed precondition or a successful precondition if all tests pass</returns>
        /// <remarks>Execution of preconditions stops at the first failed condition</remarks>
        private NavigationPreConditionResult TestPreConditions(ViewModelBase ViewModel)
        {
            var attribs = ViewModel.GetType().GetCustomAttributes(typeof(NavigationPreCondition), true);

            foreach(var attrib in attribs)
            {
                if(attrib is NavigationPreCondition condition)
                {
                    NavigationPreConditionResult result = condition.TestPreCondition(HostScreen);

                    if(!result.Succeeded)
                    {
                        var vmTypeName = ViewModel.GetType().Name;

                        LogManager.Instance.Warning($"[{vmTypeName}] Failed pre-condition check: {attrib.GetType().Name}");
                        return result;
                    }
                }
            }

            return NavigationPreConditionResult.FromSuccess();
        }

        /// <summary>
        /// Process the results of the precondition tests
        /// </summary>
        /// <param name="ViewModel"></param>
        /// <returns>The viewmodel that should be loaded</returns>
        private ViewModelBase ProcessViewModelResults(ViewModelBase ViewModel)
        {
            NavigationPreConditionResult result = TestPreConditions(ViewModel);

            if (!result.Succeeded)
            {
                ViewModel = result.ViewModel;
            }

            return ViewModel;
        }

        /// <summary>
        /// Navigate to another viewmodel after a delay
        /// </summary>
        /// <param name="ViewModel"></param>
        /// <param name="Milliseconds"></param>
        /// <returns></returns>
        public async Task NavigateToWithDelay(ViewModelBase ViewModel, int Milliseconds)
        {
            ViewModel = ProcessViewModelResults(ViewModel);

            if (ViewModel == null) return;

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                HostScreen.Router.Navigate.Execute(await ViewModel.WithDelay(Milliseconds));
            });
        }

        /// <summary>
        /// Navigate to another viewmodel
        /// </summary>
        /// <param name="ViewModel"></param>
        public void NavigateTo(ViewModelBase ViewModel)
        {
            ViewModel = ProcessViewModelResults(ViewModel);

            if (ViewModel == null) return;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    HostScreen.Router.Navigate.Execute(ViewModel);
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[CRITICAL UI NAV EXCEPTION] Ao carregar {ViewModel.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        LogManager.Instance.Error($"Inner => {ex.InnerException.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Navegação por item de MENU (comportamento "abas", não "voltar"): leva SEMPRE à tela do item
        /// clicado, venha o usuário de onde vier, mantendo a pilha rasa. Volta até a home (<see
        /// cref="ILauncherHome"/> — o Launcher/Profile) descartando o que estiver acima, e então empilha o
        /// alvo. <paramref name="target"/> = null significa "ir à própria home" (fica só no Launcher).
        /// Implementa a mesma regra de <see cref="Helpers.MenuNavigationPlan"/> (que a testa exaustivamente),
        /// usando só os commands oficiais do Router. Home ausente (modo enxuto/pré-login) → degrada: navega
        /// direto ao alvo, sem tocar na pilha.
        /// </summary>
        public void NavigateMenu(ViewModelBase target)
        {
            // Roteia o alvo pelas MESMAS pré-condições do NavigateTo (ex.: [RequireLoggedIn]) — senão um
            // item de menu empilharia uma tela logada-only sem sessão. Se a pré-condição anular o alvo, sai.
            ViewModelBase resolved = target != null ? ProcessViewModelResults(target) : null;
            if (target != null && resolved == null) return;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var stack = HostScreen.Router.NavigationStack;

                int homeIdx = -1;
                for (int i = 0; i < stack.Count; i++)
                {
                    if (stack[i] is ILauncherHome) { homeIdx = i; break; }
                }

                if (homeIdx < 0)
                {
                    // Sem Launcher na pilha (ex.: Settings aberto pela engrenagem ANTES do login): não há
                    // "home". Abre o alvo (já validado por pré-condição); "ir ao Launcher" (target null)
                    // degrada para voltar UMA tela — nunca no-op, senão o item LAUNCHER travaria o usuário.
                    if (resolved != null) HostScreen.Router.Navigate.Execute(resolved);
                    else if (stack.Count > 1) HostScreen.Router.NavigateBack.Execute();
                    return;
                }

                // Volta até a home (nº fixo de pops — sem loop dependente de estado) descartando o que
                // estiver acima dela; depois empilha o alvo. Se target == null, para na própria home.
                int popCount = stack.Count - (homeIdx + 1);
                for (int i = 0; i < popCount; i++)
                {
                    HostScreen.Router.NavigateBack.Execute();
                }

                if (resolved != null)
                {
                    HostScreen.Router.Navigate.Execute(resolved);
                }
            });
        }

        /// <summary>
        /// Navigate to the previous viewmodel
        /// </summary>
        public void NavigateBack()
        {
            var ViewModel = HostScreen.Router.NavigationStack[HostScreen.Router.NavigationStack.Count - 2];

            if(ViewModel is ViewModelBase vmBase)
            {
                var result = TestPreConditions(vmBase);

                if (!result.Succeeded)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (result.ViewModel == null) return;

                        HostScreen.Router.Navigate.Execute(result.ViewModel);
                        return;
                    });
                }
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                HostScreen.Router.NavigateBack.Execute();
            });
        }

        /// <summary>
        /// A convenience method for sending notifications
        /// </summary>
        /// <param name="Title"></param>
        /// <param name="Message"></param>
        /// <param name="Type"></param>
        public void SendNotification(string Title, string Message, NotificationType Type = NotificationType.Information)
        {
            NotificationManager.Show(new SPTNotificationViewModel(HostScreen, Title, Message, Type));
        }

        /// <summary>
        /// A convenience method for showing dialogs
        /// </summary>
        /// <param name="ViewModel"></param>
        /// <returns></returns>
        public async Task<object?> ShowDialog(object ViewModel)
        {
            return await DialogHost.Show(ViewModel);
        }

        public ViewModelBase(IScreen Host)
        {
            HostScreen = Host;
        }
    }
}
