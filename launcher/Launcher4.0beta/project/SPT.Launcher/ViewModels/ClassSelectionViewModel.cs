using SPT.Launcher.Attributes;
using SPT.Launcher.Helpers;
using SPT.Launcher.Models.SPT;
using SPT.Launcher.Models.Launcher;
using ReactiveUI;
using Splat;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using Avalonia.Threading;
using Avalonia.Controls.Notifications;

namespace SPT.Launcher.ViewModels
{
    public class ClassProfile
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public ObservableCollection<string> Advantages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Disadvantages { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> Skills { get; set; } = new ObservableCollection<string>();
    }

    [RequireServerConnected]
    public class ClassSelectionViewModel : ViewModelBase
    {
        private string _username;
        private string _password;

        public ObservableCollection<ClassProfile> AvailableClasses { get; set; } = new ObservableCollection<ClassProfile>();

        private ClassProfile _selectedClass;
        public ClassProfile SelectedClass
        {
            get => _selectedClass;
            set => this.RaiseAndSetIfChanged(ref _selectedClass, value);
        }

        private string _registerErrorMsg;
        public string RegisterErrorMsg
        {
            get => _registerErrorMsg;
            set => this.RaiseAndSetIfChanged(ref _registerErrorMsg, value);
        }

        public ReactiveCommand<Unit, Unit> GoToRegisterCommand { get; set; }
        public ReactiveCommand<Unit, Unit> FinalizeAccountCommand { get; set; }

        public ClassSelectionViewModel(IScreen Host, string username, string password) : base(Host)
        {
            _username = username;
            _password = password;

            LoadMockClasses();
            if (AvailableClasses.Count > 0)
                SelectedClass = AvailableClasses[3]; // Seleciona Caçador como no mockup

            GoToRegisterCommand = ReactiveCommand.Create(() => 
            {
                NavigateTo(new RegisterViewModel(HostScreen, _username));
            });

            FinalizeAccountCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                RegisterErrorMsg = "";
                
                if (SelectedClass == null) return;

                // Em um cenário real, usaremos o SelectedClass.Name mapeado para a edition do server
                string editionToUse = SelectedClass.Name;

                // Fallback de segurança: Se a classe não for uma edition real, o SPT Launcher atual 
                // por padrão pega a primeira edition válida do SelectedServer.
                // Como você mencionou que as classes vão ser versões das "editions" com nomes alterados depois,
                // vamos enviar a edition escolhida pelo usuário. Se o server rejeitar, o usuário verá o erro.
                
                // MOCK fallback: para evitar erro de servidor durante protótipo se as classes ainda não existirem no server:
                if (ServerManager.SelectedServer != null && ServerManager.SelectedServer.editions.Length > 0)
                {
                    // editionToUse = ServerManager.SelectedServer.editions[0]; 
                    // No futuro, certifique-se que ServerManager contém os nomes "Armeiro", "Batedor", etc.
                }

                AccountStatus registerResult = await AccountManager.RegisterAsync(_username, _password, editionToUse);

                if (registerResult == AccountStatus.OK)
                {
                    SendNotification(LocalizationProvider.Instance.profile_created, _username, NotificationType.Success);
                    
                    // Auto-Login e ir pro Profile
                    var loginModel = new LoginModel { Username = _username, Password = _password };
                    AccountStatus loginStatus = await AccountManager.LoginAsync(loginModel);
                    
                    if (loginStatus == AccountStatus.OK)
                    {
                        NavigateTo(new ProfileViewModel(HostScreen));
                    }
                    else
                    {
                        NavigateTo(new LoginViewModel(HostScreen));
                    }
                }
                else
                {
                    RegisterErrorMsg = "Erro ao criar conta: " + registerResult.ToString();
                }
            });
        }

        private void LoadMockClasses()
        {
            AvailableClasses.Add(new ClassProfile { Name = "Armeiro" });
            AvailableClasses.Add(new ClassProfile { Name = "Batedor" });
            AvailableClasses.Add(new ClassProfile { Name = "Gerente de Operações" });
            
            AvailableClasses.Add(new ClassProfile 
            { 
                Name = "Caçador",
                Description = "Um especialista em reconhecimento e combate a longa distância, o Caçador domina a arte da camuflagem e do tiro de precisão. Ideal para eliminar ameaças de longe sem ser detectado.",
                Advantages = new ObservableCollection<string> { "+ Rastreamento de Pegadas", "+ Dano de Longo Alcance", "+ Camuflagem Passiva" },
                Disadvantages = new ObservableCollection<string> { "- Velocidade de Movimento", "- Velocidade de Recarga", "- Capacidade de Carga" },
                Skills = new ObservableCollection<string> { "Binóculos de Reconhecimento", "Munição de Precisão", "Armadilha de Urso" }
            });
            
            AvailableClasses.Add(new ClassProfile { Name = "Médico de Combate" });
            AvailableClasses.Add(new ClassProfile { Name = "Operador Furtivo" });
            AvailableClasses.Add(new ClassProfile { Name = "Operador Tático" });
            AvailableClasses.Add(new ClassProfile { Name = "Peladão" });
            AvailableClasses.Add(new ClassProfile { Name = "Saqueador" });
            AvailableClasses.Add(new ClassProfile { Name = "Sobrevivencialista" });
        }
    }
}
