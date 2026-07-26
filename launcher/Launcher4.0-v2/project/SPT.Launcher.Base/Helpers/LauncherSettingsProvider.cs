/* LauncherSettingsProvider.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 */


using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.Launcher;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using SPT.Launcher.Utilities;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    public static class LauncherSettingsProvider
    {
        public static string DefaultSettingsFileLocation = Path.Join(SPT.Launcher.Base.Helpers.SptPathHelper.SptRootPath, "user", "launcher", "config.json");
        
        private static Settings LoadSettings()
        {
            var settings = Json.Load<Settings>(DefaultSettingsFileLocation) ?? new Settings();
            if (settings.Server != null && string.IsNullOrEmpty(settings.Server.Url))
            {
                settings.Server.Url = "https://147.15.29.24:7073";
            }

            // === Feature 3: Auto-detect GamePath ===
            // Se o launcher está na mesma pasta que EscapeFromTarkov.exe, auto-corrigir GamePath
            try
            {
                string detectedPath = AppContext.BaseDirectory;
                string exePath = Path.Combine(detectedPath, "EscapeFromTarkov.exe");

                if (File.Exists(exePath) && !string.Equals(settings.GamePath, detectedPath, StringComparison.OrdinalIgnoreCase))
                {
                    LogManager.Instance.Info($"[Settings] GamePath auto-corrigido: '{settings.GamePath}' → '{detectedPath}'");
                    settings.GamePath = detectedPath;
                    Json.SaveWithFormatting(DefaultSettingsFileLocation, settings, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[Settings] Erro ao auto-detectar GamePath: {ex.Message}");
            }

            // === Migração de opcionais legados → dicionário ===
            settings.MigrateLegacyOptionals();

            // === Segurança (Modificada): mantendo o dev mode salvo da sessão anterior ===
            // if (settings.IsDevMode)
            // {
            //     LogManager.Instance.Info("[Settings] Dev Mode estava ativo sem senha — desativando automaticamente");
            //     settings.IsDevMode = false;
            // }

            // === Config Version: atualizar config quando o launcher evolui ===
            // Incrementar EXPECTED_CONFIG_VERSION a cada mudança estrutural no config.
            // Quando a versão salva < esperada, força re-save para limpar campos obsoletos
            // e gravar novos campos com defaults, sem perder dados do jogador.
            const int EXPECTED_CONFIG_VERSION = 5; // v5: item 030 — EnabledPerformanceItems, ModsConfigsOnboardingDone, PendingApply, SeenItemIds
            if (settings.ConfigVersion < EXPECTED_CONFIG_VERSION)
            {
                LogManager.Instance.Info($"[Settings] Config desatualizado (v{settings.ConfigVersion} → v{EXPECTED_CONFIG_VERSION}). Atualizando...");
                settings.ConfigVersion = EXPECTED_CONFIG_VERSION;
            }

            // Salvar uma vez só no final (cobre todas as migrações acima)
            Json.SaveWithFormatting(DefaultSettingsFileLocation, settings, Formatting.Indented);

            return settings;
        }

        public static Settings Instance { get; } = LoadSettings();
    }

    public class Settings : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Versão do schema do config. Incrementada quando há mudanças estruturais.
        /// LoadSettings compara com EXPECTED_CONFIG_VERSION e força re-save se menor.
        /// </summary>
        public int ConfigVersion { get; set; } = 0;

        public bool FirstRun { get; set; } = true;

        public bool SaveSettings()
        {
            if (Server != null && Server.Url != null)
            {
                Server.Url = Path.TrimEndingDirectorySeparator(Server.Url.Trim());
            }
            return Json.SaveWithFormatting(LauncherSettingsProvider.DefaultSettingsFileLocation, this, Formatting.Indented);
        }

        public void ResetDefaults()
        {
            string defaultUrl = "https://147.15.29.24:7073";
            string defaultPath = AppContext.BaseDirectory;
            
            // don't reset if running in dev mode
            if (IsDevMode)
            {
                LogManager.Instance.Info("Running in dev mode, not resetting");
                return;
            }

            if (Server != null && Server.Url != defaultUrl)
            {
                LogManager.Instance.Info($"Server URL was '{Server.Url}'");
                LogManager.Instance.Info($"Server URL was reset to default '{defaultUrl}'");
                Server.Url = defaultUrl;
            }
            else
            {
                LogManager.Instance.Info($"Server URL is already set to default '{defaultUrl}'");
            }

            if (GamePath != defaultPath)
            {
                LogManager.Instance.Info($"Game path was '{GamePath}'");
                LogManager.Instance.Info($"Game path was reset to default '{defaultPath}'");
                GamePath = defaultPath;
            }
            else
            {
                LogManager.Instance.Info($"Game path is already set to default '{defaultPath}'");
            }
            
            SaveSettings();
        }

        public string DefaultLocale { get; set; } = "Portuguese";

        private bool _isAddingServer;
        [JsonIgnore]
        public bool IsAddingServer
        {
            get => _isAddingServer;
            set => SetProperty(ref _isAddingServer, value);
        }

        private bool _allowSettings;
        [JsonIgnore]
        public bool AllowSettings
        {
            get => _allowSettings;
            set => SetProperty(ref _allowSettings, value);
        }

        private bool _gameRunning;
        [JsonIgnore]
        public bool GameRunning
        {
            get => _gameRunning;
            set
            {
                SetProperty(ref _gameRunning, value);
                RaisePropertyChanged(nameof(CanStartGame));
            }
        }

        private bool _isUpdating;
        [JsonIgnore]
        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                SetProperty(ref _isUpdating, value);
                RaisePropertyChanged(nameof(CanStartGame));
            }
        }

        [JsonIgnore]
        public bool CanStartGame => !GameRunning && !IsUpdating;

        // === Mods Opcionais — sistema dinâmico ===

        /// <summary>
        /// Dicionário persistido: groupId → enabled (true/false)
        /// Substitui EnableGore, RemoveGrass, EnableVisualEffects
        /// </summary>
        private Dictionary<string, bool> _enabledOptionals = new Dictionary<string, bool>();
        public Dictionary<string, bool> EnabledOptionals
        {
            get => _enabledOptionals;
            set => SetProperty(ref _enabledOptionals, value);
        }

        /// <summary>
        /// IDs de opcionais que mudaram e precisam ser aplicados.
        /// Transiente — não salvo no JSON.
        /// </summary>
        [JsonIgnore]
        public HashSet<string> PendingOptionalChanges { get; set; } = new HashSet<string>();

        // === Propriedades legadas — mantidas para leitura do JSON antigo e migração ===
        // Após migração, essas ficam false e são ignoradas. Serão removidas em versão futura.

        [JsonProperty("EnableGore")]
        private bool _legacyEnableGore { get; set; }

        [JsonProperty("RemoveGrass")]
        private bool _legacyRemoveGrass { get; set; }

        [JsonProperty("EnableVisualEffects")]
        private bool _legacyEnableVisualEffects { get; set; }

        [JsonIgnore]
        private bool _legacyMigrated = false;

        /// <summary>
        /// Migra propriedades legadas (EnableGore, RemoveGrass, EnableVisualEffects) para o dicionário EnabledOptionals.
        /// Chamado uma vez no LoadSettings(). Após migração, salva e limpa as props legadas.
        /// </summary>
        public void MigrateLegacyOptionals()
        {
            if (_legacyMigrated) return;
            _legacyMigrated = true;

            bool needsSave = false;

            if (_legacyEnableGore && !_enabledOptionals.ContainsKey("gore"))
            {
                _enabledOptionals["gore"] = true;
                _legacyEnableGore = false;
                needsSave = true;
                LogManager.Instance.Info("[Settings] Migrado EnableGore → EnabledOptionals[\"gore\"]");
            }

            if (_legacyRemoveGrass && !_enabledOptionals.ContainsKey("grass"))
            {
                _enabledOptionals["grass"] = true;
                _legacyRemoveGrass = false;
                needsSave = true;
                LogManager.Instance.Info("[Settings] Migrado RemoveGrass → EnabledOptionals[\"grass\"]");
            }

            if (_legacyEnableVisualEffects && !_enabledOptionals.ContainsKey("hollywood"))
            {
                _enabledOptionals["hollywood"] = true;
                _legacyEnableVisualEffects = false;
                needsSave = true;
                LogManager.Instance.Info("[Settings] Migrado EnableVisualEffects → EnabledOptionals[\"hollywood\"]");
            }

            if (needsSave)
            {
                SaveSettings();
            }
        }

        /// <summary>
        /// Verifica se um grupo opcional está ativo.
        /// </summary>
        public bool IsOptionalEnabled(string groupId)
        {
            return _enabledOptionals.TryGetValue(groupId, out bool enabled) && enabled;
        }

        /// <summary>
        /// Define o estado de um grupo opcional e salva.
        /// </summary>
        public void SetOptionalEnabled(string groupId, bool enabled)
        {
            _enabledOptionals[groupId] = enabled;
            SaveSettings();
        }

        // === Item 030: tela "Mods e Configs" — eixo de configs de performance + onboarding ===

        /// <summary>Item 030: itemId → enabled do eixo de configs de PERFORMANCE (espelha EnabledOptionals).</summary>
        private Dictionary<string, bool> _enabledPerformanceItems = new Dictionary<string, bool>();
        public Dictionary<string, bool> EnabledPerformanceItems
        {
            get => _enabledPerformanceItems;
            set => SetProperty(ref _enabledPerformanceItems, value);
        }

        /// <summary>Item 030 (D-17/CA-030.16b): fonte de verdade do onboarding — o estado do disco não decide.</summary>
        private bool _modsConfigsOnboardingDone;
        public bool ModsConfigsOnboardingDone
        {
            get => _modsConfigsOnboardingDone;
            set => SetProperty(ref _modsConfigsOnboardingDone, value);
        }

        /// <summary>
        /// Item 030 (PA-01-05/CC-20): ids alternados pelo player que ainda NÃO foram aplicados. PERSISTIDO
        /// de propósito (sem [JsonIgnore], ao contrário de PendingOptionalChanges): se o sync falhar ou o
        /// launcher fechar no meio, a intenção sobrevive e o próximo sync retenta. Um id sai daqui só quando
        /// a ação dele conclui com sucesso.
        /// </summary>
        private List<string> _pendingApply = new List<string>();
        public List<string> PendingApply
        {
            get => _pendingApply;
            set => SetProperty(ref _pendingApply, value);
        }

        /// <summary>
        /// Item 030 (CA-030.11/D-6): ids já apresentados ao player. Item do servidor que não está aqui é
        /// "novo" — ganha marcador até o player sair da tela.
        /// </summary>
        private List<string> _seenItemIds = new List<string>();
        public List<string> SeenItemIds
        {
            get => _seenItemIds;
            set => SetProperty(ref _seenItemIds, value);
        }

        /// <summary>Item 030: um item de config de performance está ligado?</summary>
        public bool IsPerformanceItemEnabled(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && _enabledPerformanceItems.TryGetValue(itemId, out bool enabled) && enabled;
        }

        /// <summary>Item 030: define o estado de um item de performance e salva.</summary>
        public void SetPerformanceItemEnabled(string itemId, bool enabled)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            _enabledPerformanceItems[itemId] = enabled;
            SaveSettings();
        }

        private LauncherAction _launcherStartGameAction;
        public LauncherAction LauncherStartGameAction
        {
            get => _launcherStartGameAction;
            set => SetProperty(ref _launcherStartGameAction, value);
        }

        private bool _rememberUsername;
        public bool RememberUsername
        {
            get => _rememberUsername;
            set => SetProperty(ref _rememberUsername, value);
        }

        private string _lastUsername = "";
        public string LastUsername
        {
            get => _lastUsername;
            set => SetProperty(ref _lastUsername, value);
        }

        private string _lastPassword = "";
        public string LastPassword
        {
            get => _lastPassword;
            set => SetProperty(ref _lastPassword, value);
        }

        private bool _useAutoLogin;
        public bool UseAutoLogin
        {
            get => _useAutoLogin;
            set => SetProperty(ref _useAutoLogin, value);
        }

        private bool _isDevMode;
        public bool IsDevMode
        {
            get => _isDevMode;
            set => SetProperty(ref _isDevMode, value);
        }

        private bool _disableUpdates;
        public bool DisableUpdates
        {
            get => _disableUpdates;
            set => SetProperty(ref _disableUpdates, value);
        }

        // Item 030 (D-12): UsePerformanceConfigs (overlay global do item 008) removido — a performance
        // agora é por item na tela "Mods e Configs", como regra de pasta config-performance.

        /// <summary>
        /// Modo homolog: quando ligado, as rotas do mod TarkovRedLine ganham o prefixo "/homolog"
        /// (ver <see cref="RequestHandler.ModRoutePrefix"/>), batendo no mod TarkovRedLine.Server.Homolog
        /// que roda no MESMO server de produção sem colidir com as rotas de produção. Rotas do SPT core
        /// (login/register/connect/customclasses) NÃO são afetadas. Uso: testar mudanças do launcher
        /// contra o ambiente real antes de subir pra produção. Default off (produção).
        /// </summary>
        private bool _homologMode;
        public bool HomologMode
        {
            get => _homologMode;
            set => SetProperty(ref _homologMode, value);
        }

        /// <summary>
        /// Ferramenta de dev: quando ligado, o launcher usa o servidor LOCAL (127.0.0.1) em vez da URL
        /// configurada, PRESERVANDO a URL anterior em <see cref="SavedServerUrl"/>. Ao desligar, restaura
        /// a URL guardada. Só faz sentido em Dev Mode (o painel dev exige Dev Mode; e o fetch do Gist é
        /// pulado quando isto está ligado).
        /// </summary>
        private bool _useLocalServer;
        public bool UseLocalServer
        {
            get => _useLocalServer;
            set => SetProperty(ref _useLocalServer, value);
        }

        /// <summary>URL do servidor guardada antes de trocar pro local (para restaurar ao desligar UseLocalServer).</summary>
        private string _savedServerUrl;
        public string SavedServerUrl
        {
            get => _savedServerUrl;
            set => SetProperty(ref _savedServerUrl, value);
        }

        private string _gamePath;
        public string GamePath
        {
            get => _gamePath;
            set => SetProperty(ref _gamePath, value);
        }

        private string[] _excludeFromCleanup = [];

        public string[] ExcludeFromCleanup
        {
            get => _excludeFromCleanup;
            set => SetProperty(ref _excludeFromCleanup, value);
        }

        public ServerSetting Server { get; set; } = new ServerSetting();

        public Settings()
        {
            if (!File.Exists(LauncherSettingsProvider.DefaultSettingsFileLocation))
            {
                LogManager.Instance.Warning("Launcher config not found");
                LogManager.Instance.Info($"Creating launcher config: {LauncherSettingsProvider.DefaultSettingsFileLocation}");
                LauncherStartGameAction = LauncherAction.MinimizeAction;
                UseAutoLogin = true;
                GamePath = AppContext.BaseDirectory;
                // ref: CR-01-04 — instalação limpa é jogador, não dev: Dev Mode default true
                // pulava a VPN/gist e engolia o erro claro de Tailscale em toda máquina nova.
                IsDevMode = false;

                Server = new ServerSetting
                {
                    Name = "SPT", 
                    Url = "https://147.15.29.24:7073"
                };
                SaveSettings();
            }
            
            LogManager.Instance.Info($"Using launcher config at: {LauncherSettingsProvider.DefaultSettingsFileLocation}");
        }
    }
}
