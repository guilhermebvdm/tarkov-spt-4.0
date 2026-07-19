/* LocalizationProvider.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 */


using SPT.Launcher.Extensions;
using SPT.Launcher.MiniCommon;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using SPT.Launcher.Utilities;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    public static class LocalizationProvider
    {
        public static string DefaultLocaleFolderPath = Path.Join(SPT.Launcher.Base.Helpers.SptPathHelper.SptRootPath, "SPT_Data", "Launcher", "Locales");

        /// <summary>
        /// i18n (TRL): idiomas OFICIALMENTE suportados, por ietf_tag. Só estes aparecem no dropdown.
        /// Os outros 11 locales upstream (de/fr/it/es/pl/ru/tr/ja/ko/zh-hans/zh-hant) estão faltando as
        /// 23 chaves novas do TRL e o loader é tudo-ou-nada (rejeita o locale inteiro se faltar chave) —
        /// selecioná-los era um no-op silencioso. Não oferecer o que não funciona. Para promover um
        /// idioma no futuro: completar as 23 chaves nele e adicionar o ietf_tag aqui.
        /// NB: precisa vir ANTES de LocaleNameDictionary (init estático em ordem textual).
        /// </summary>
        public static readonly HashSet<string> SupportedIetfTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "en", "pt" };

        public static Dictionary<string, string> LocaleNameDictionary = GetLocaleDictionary("native_name");

        public static event EventHandler LocaleChanged = delegate { };

        public static void LoadLocalByName(string localeName)
        {
            string localeRomanName = LocaleNameDictionary.GetKeyByValue(localeName);

            if (String.IsNullOrEmpty(localeRomanName))
            {
                localeRomanName = localeName;
            }
            
            LoadLocaleFromFile(localeRomanName);
        }

        public static void LoadLocaleFromFile(string localeRomanName)
        {
            var localePath = Path.Join(DefaultLocaleFolderPath, $"{localeRomanName}.json");
            LocaleData newLocale = Json.LoadClassWithoutSaving<LocaleData>(localePath);

            if (newLocale != null)
            {
                foreach (var prop in Instance.GetType().GetProperties())
                {
                    prop.SetValue(Instance, newLocale.GetType().GetProperty(prop.Name).GetValue(newLocale));
                }

                LauncherSettingsProvider.Instance.DefaultLocale = localeRomanName;
                LauncherSettingsProvider.Instance.SaveSettings();

                LocaleChanged(null, EventArgs.Empty);

                return;
            }

            LogManager.Instance.Error($"Could not load locale: {localePath}");
        }

        public static void TryAutoSetLocale()
        {
            // get local dictionary based on ietf_tag property in locale files. like: ("English", "en")
            // "English" being the file name
            var localeTagDictionary = GetLocaleDictionary("ietf_tag");

            // get system locale. Like: "en-US"
            var tag = CultureInfo.CurrentUICulture.IetfLanguageTag;

            // get the locale file name from the dictionary based on the input tag. If it matches, or starts with the value
            var localeRomanName = localeTagDictionary.GetKeyByInput(tag);
            
            if (String.IsNullOrEmpty(localeRomanName))
            {
                localeRomanName = "Portuguese";
            }
            
            LoadLocaleFromFile(localeRomanName);
        }

        public static LocaleData GenerateDefaultLocale()
        {
            //Create default locale data and save if the default locale data file dosen't exist.
            LocaleData locale = new LocaleData();

            #region Set All Defaults
            locale.ietf_tag = "pt";
            locale.native_name = "Portuguese";
            locale.retry = "Tentar Novamente";
            locale.server_connecting = "Conectando";
            locale.server_unavailable_format_1 = "Nenhum servidor disponível em: '{0}' para conectar\nCertifique-se de iniciar o Servidor primeiro.";
            locale.no_servers_available = "Nenhum Servidor SPT encontrado. Verifique a URL do servidor nas configurações.";
            locale.settings_menu = "Configurações";
            locale.back = "Voltar";
            locale.wipe_profile = "Limpar Perfil (Wipe)";
            locale.username = "Usuário";
            locale.password = "Senha";
            locale.update = "Atualizar";
            locale.edit_account_update_error = "Ocorreu um problema ao atualizar o seu perfil.";
            locale.register = "Registrar";
            locale.go_to_register = "Criar Conta";
            locale.registration_failed = "Falha no Registro.";
            locale.registration_question_format_1 = "O perfil '{0}' não existe.\n\nGostaria de criá-lo?";
            locale.login_or_register = "Login / Registro";
            locale.go_to_login = "Fazer Login";
            locale.login_automatically = "Login Automático";
            locale.incorrect_login = "Usuário ou senha incorretos";
            locale.login_failed = "Falha no Login";
            locale.edition = "Edição";
            locale.id = "ID";
            locale.logout = "Sair";
            locale.account = "Conta";
            locale.edit_account = "Editar Conta";
            locale.start_game = "JOGAR";
            locale.installed_in_live_game_warning = "O Tarkov Red Line não deve ser instalado na pasta do jogo original. Instale em uma cópia separada.";
            locale.no_official_game_warning = "Escape From Tarkov não está instalado no seu computador. Verifique pelo launcher da BSG.";
            locale.eft_exe_not_found_warning = "EscapeFromTarkov.exe não encontrado. Verifique a pasta do jogo.";
            locale.account_exist = "A conta já existe";
            locale.url = "URL";
            locale.default_language = "Idioma Padrão";
            locale.game_path = "Caminho do Jogo";
            locale.clear_game_settings = "Limpar Configurações do Jogo";
            locale.clear_game_settings_succeeded = "Configurações do jogo limpas.";
            locale.clear_game_settings_failed = "Erro ao limpar configurações do jogo.";
            locale.load_live_settings = "Copiar Configurações do Live";
            locale.load_live_settings_succeeded = "Configurações copiadas do jogo original.";
            locale.load_live_settings_failed = "Falha ao copiar configurações do jogo original.";
            locale.remove_registry_keys = "Remover Chaves do Registro";
            locale.remove_registry_keys_succeeded = "Chaves do registro removidas.";
            locale.remove_registry_keys_failed = "Erro ao remover chaves do registro.";
            locale.clean_temp_files = "Limpar Arquivos Temporários";
            locale.clean_temp_files_succeeded = "Arquivos temporários limpos.";
            locale.clean_temp_files_failed = "Erro ao limpar arquivos temporários.";
            locale.select_folder = "Selecionar Pasta";
            locale.minimize_action = "Minimizar";
            locale.do_nothing_action = "Não fazer nada";
            locale.exit_action = "Fechar Launcher";
            locale.on_game_start = "Ao Iniciar o Jogo";
            locale.game = "Jogo";
            locale.new_password = "Nova Senha";
            locale.wipe_warning = "Alterar a edição da conta exige o Wipe do perfil. O progresso será resetado.";
            locale.cancel = "Cancelar";
            locale.need_an_account = "Ainda não tem uma conta?";
            locale.have_an_account = "Já possui uma conta?";
            locale.reapply_patch = "Reaplicar Patch";
            locale.failed_to_receive_patches = "Falha ao receber patches";
            locale.failed_core_patch = "Falha no patch Core";
            locale.failed_mod_patch = "Falha no patch Mod";
            locale.ok = "OK";
            locale.account_page_denied = "Página da conta recusada. O jogo está aberto ou não há login.";
            locale.account_updated = "Sua conta foi atualizada";
            locale.nickname = "Apelido";
            locale.side = "Facção";
            locale.level = "Nível";
            locale.patching = "Aplicando Patch";
            locale.file_mismatch_dialog_message = "Arquivos do jogo incompatíveis com a versão esperada: {0}";
            locale.yes = "Sim";
            locale.no = "Não";
            locale.open_folder = "Abrir Pasta";
            locale.select_edition = "Selecionar Edição";
            locale.profile_created = "Perfil Criado";
            locale.next_level_in = "Próximo nível em";
            locale.copied = "Copiado";
            locale.no_profile_data = "Sem dados de perfil";
            locale.profile_version_mismath = "O perfil foi criado em uma versão diferente.";
            locale.profile_removed = "Perfil removido";
            locale.profile_removal_failed = "Falha ao remover perfil";
            locale.profile_remove_question_format_1 = "Remover perfil '{0}' permanentemente?";
            locale.i_understand = "Eu Entendo";
            locale.game_version_mismatch_format_2 = "Versão do Tarkov incorreta. Encontrado: '{0}', Esperado: '{1}'";
            locale.description = "Descrição";
            locale.author = "Autor";
            locale.wipe_on_start = "Dar Wipe ao iniciar o jogo";
            locale.copy_live_settings_question = "Gostaria de copiar as configurações do jogo original?";
            locale.mod_not_in_server_warning = "Este mod está no perfil mas não foi carregado no servidor.";
            locale.active_server_mods = "Mods Ativos no Servidor";
            locale.active_server_mods_info_text = "Mods rodando no momento no servidor.";
            locale.inactive_server_mods = "Mods Inativos no Servidor";
            locale.inactive_server_mods_info_text = "Mods não carregados, mas que já foram usados no seu perfil.";
            locale.open_link_question_format_1 = "Abrir o link: \n{0} ?";
            locale.open_link = "Abrir Link";
            locale.dev_mode = "Modo Desenvolvedor";
            locale.failed_to_save_settings = "Falha ao salvar configurações";
            locale.change_server_confirm = "Trocar de servidor vai te desconectar e voltar para a tela de login. Deseja continuar?";
            locale.register_failed_name_limit = "O nome não pode exceder 15 caracteres";
            locale.copy_failed = "Falha ao copiar para a área de transferência";
            locale.copy_logs_to_clipboard = "Copiar logs";
            locale.create_password_title = "Criar Senha";
            locale.create_password_message = "Sua conta não tem senha. Crie uma para garantir a segurança.";
            locale.create_password_confirm = "Criar Senha";
            locale.create_password_success = "Senha criada com sucesso!";
            locale.reset_password = "Redefinir Senha";
            locale.reset_password_success = "Senha redefinida.";
            locale.reset_password_failed = "Falha ao redefinir a senha. Nome de usuário incorreto?";
            locale.reset_password_hwid_mismatch = "Computador não autorizado a redefinir a senha. Contate a administração.";
            locale.repeat_password = "Repetir Senha";
            locale.reset_password_no_hwid = "Nenhum HWID registrado para esta conta. Faça login primeiro.";
            locale.server_version = "Versão: ";
            locale.check_updates = "Verificar Atualizações";
            locale.update_checking = "Procurando atualizações...";
            locale.update_checking_file = "Verificando arquivos... ({0}/{1})";
            locale.update_available = "{0} arquivo(s) precisam ser atualizados";
            locale.update_up_to_date = "Tudo atualizado! ✅";
            locale.update_button = "Atualizar Launcher";
            locale.update_downloading = "Baixando: {0} ({1}/{2})";
            locale.update_completed = "Atualização concluída! {0} arquivo(s) atualizados. ✅";
            locale.update_completed_with_errors = "Atualização finalizada: {0} atualizados, {1} erro(s)";
            locale.update_error = "Falha ao atualizar: {0}";
            locale.update_files_to_delete = "{0} arquivo(s) para remover";
            locale.update_deleting = "Removendo: {0} ({1}/{2})";
            #endregion

            Directory.CreateDirectory(LocalizationProvider.DefaultLocaleFolderPath);
            LauncherSettingsProvider.Instance.DefaultLocale = "Portuguese";
            LauncherSettingsProvider.Instance.SaveSettings();
            Json.SaveWithFormatting(Path.Join(LocalizationProvider.DefaultLocaleFolderPath, "Portuguese.json"), locale, Newtonsoft.Json.Formatting.Indented);

            return locale;
        }

        public static Dictionary<string, string> GetLocaleDictionary(string property)
        {
            if (!Directory.Exists(DefaultLocaleFolderPath))
            {
                Directory.CreateDirectory(DefaultLocaleFolderPath);
            }
            
            List<FileInfo> localeFiles = new List<FileInfo>(Directory.GetFiles(DefaultLocaleFolderPath).Select(x => new FileInfo(x)).ToList());
            Dictionary<string, string> localeDictionary = new Dictionary<string, string>();

            foreach (FileInfo file in localeFiles)
            {
                // i18n (TRL): só expõe os locales suportados (completos). Filtra pelo ietf_tag do arquivo —
                // os demais não carregam de verdade (faltam as 23 chaves do TRL), então nem entram na lista.
                string ietfTag = Json.GetPropertyByName<string>(file.FullName, "ietf_tag");
                if (string.IsNullOrEmpty(ietfTag) || !SupportedIetfTags.Contains(ietfTag))
                {
                    continue;
                }

                localeDictionary.Add(file.Name.Replace(".json", ""), Json.GetPropertyByName<string>(file.FullName, property));
            }

            return localeDictionary;
        }
        public static ObservableCollection<string> GetAvailableLocales()
        {
            return new ObservableCollection<string>(LocaleNameDictionary.Values);
        }

        public static LocaleData Instance { get; private set; } = Json.LoadClassWithoutSaving<LocaleData>(Path.Join(DefaultLocaleFolderPath, $"{LauncherSettingsProvider.Instance.DefaultLocale}.json")) ?? GenerateDefaultLocale();
    }

    public class LocaleData : NotifyPropertyChangedBase
    {
        //this is going to be some pretty long boiler plate code. So I'm putting everything into regions.

        #region All Properties
        
        #region
        private string _copy_logs_to_clipboard;
        public string copy_logs_to_clipboard
        {
            get => _copy_logs_to_clipboard;
            set => SetProperty(ref _copy_logs_to_clipboard, value);
        }
        
        #endregion
        
        #region copy_failed
        private string _copy_failed;
        public string copy_failed
        {
            get => _copy_failed;
            set => SetProperty(ref _copy_failed, value);
        }
        #endregion
        
        #region register_failed_name_limit

        private string _register_failed_name_limit;

        public string register_failed_name_limit
        {
            get => _register_failed_name_limit;
            set => SetProperty(ref _register_failed_name_limit, value);
        }
        #endregion
        
        #region failed_to_save_settings

        private string _failed_to_save_settings;

        public string failed_to_save_settings
        {
            get => _failed_to_save_settings;
            set => SetProperty(ref _failed_to_save_settings, value);
        }

        private string _change_server_confirm;
        public string change_server_confirm
        {
            get => _change_server_confirm;
            set => SetProperty(ref _change_server_confirm, value);
        }

        // Painel de perfil (templates com placeholder — usados via LocalizedFormatConverter/MultiBinding).
        private string _profile_operator;
        public string profile_operator { get => _profile_operator; set => SetProperty(ref _profile_operator, value); }
        private string _profile_level;
        public string profile_level { get => _profile_level; set => SetProperty(ref _profile_level, value); }
        private string _profile_remaining_exp;
        public string profile_remaining_exp { get => _profile_remaining_exp; set => SetProperty(ref _profile_remaining_exp, value); }
        private string _profile_class;
        public string profile_class { get => _profile_class; set => SetProperty(ref _profile_class, value); }
        private string _profile_id;
        public string profile_id { get => _profile_id; set => SetProperty(ref _profile_id, value); }
        private string _profile_faction;
        public string profile_faction { get => _profile_faction; set => SetProperty(ref _profile_faction, value); }
        #endregion
        
        #region dev_mode
        private string _dev_mode;
        public string dev_mode
        {
            get => _dev_mode;
            set => SetProperty(ref _dev_mode, value);
        }
        #endregion

        #region ietf_tag

        private string _ietf_tag;

        public string ietf_tag
        {
            get => _ietf_tag;
            set => SetProperty(ref _ietf_tag, value);
        }
        #endregion
        
        #region native_name
        private string _native_name;
        public string native_name
        {
            get => _native_name;
            set => SetProperty(ref _native_name, value);
        }
        #endregion

        #region retry
        private string _retry;
        public string retry
        {
            get => _retry;
            set => SetProperty(ref _retry, value);
        }
        #endregion

        #region server_connecting
        private string _server_connecting;
        public string server_connecting
        {
            get => _server_connecting;
            set => SetProperty(ref _server_connecting, value);
        }
        #endregion

        #region server_unavailable_format_1
        private string _server_unavailable_format_1;
        public string server_unavailable_format_1
        {
            get => _server_unavailable_format_1;
            set => SetProperty(ref _server_unavailable_format_1, value);
        }
        #endregion

        #region no_servers_available
        private string _no_servers_available;
        public string no_servers_available
        {
            get => _no_servers_available;
            set => SetProperty(ref _no_servers_available, value);
        }
        #endregion

        #region settings_menu
        private string _settings_menu;
        public string settings_menu
        {
            get => _settings_menu;
            set => SetProperty(ref _settings_menu, value);
        }
        #endregion

        #region back
        private string _back;
        public string back
        {
            get => _back;
            set => SetProperty(ref _back, value);
        }
        #endregion

        #region wipe_profile
        private string _wipe_profile;
        public string wipe_profile
        {
            get => _wipe_profile;
            set => SetProperty(ref _wipe_profile, value);
        }
        #endregion

        #region username
        private string _username;
        public string username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        #endregion

        #region password
        private string _password;
        public string password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        #endregion

        #region update
        private string _update;
        public string update
        {
            get => _update;
            set => SetProperty(ref _update, value);
        }
        #endregion

        #region edit_account_update_error
        private string _edit_account_update_error;
        public string edit_account_update_error
        {
            get => _edit_account_update_error;
            set => SetProperty(ref _edit_account_update_error, value);
        }
        #endregion

        #region register
        private string _register;
        public string register
        {
            get => _register;
            set => SetProperty(ref _register, value);
        }
        #endregion

        #region go_to_register
        private string _go_to_register;
        public string go_to_register
        {
            get => _go_to_register;
            set => SetProperty(ref _go_to_register, value);
        }
        #endregion

        #region login_or_register
        private string _login_or_register;
        public string login_or_register
        {
            get => _login_or_register;
            set => SetProperty(ref _login_or_register, value);
        }
        #endregion

        #region go_to_login
        private string _go_to_login;
        public string go_to_login
        {
            get => _go_to_login;
            set => SetProperty(ref _go_to_login, value);
        }
        #endregion

        #region login_automatically
        private string _login_automatically;
        public string login_automatically
        {
            get => _login_automatically;
            set => SetProperty(ref _login_automatically, value);
        }
        #endregion

        #region incorrect_login
        private string _incorrect_login;
        public string incorrect_login
        {
            get => _incorrect_login;
            set => SetProperty(ref _incorrect_login, value);
        }
        #endregion

        #region login_failed
        private string _login_failed;
        public string login_failed
        {
            get => _login_failed;
            set => SetProperty(ref _login_failed, value);
        }
        #endregion

        #region edition
        private string _edition;
        public string edition
        {
            get => _edition;
            set => SetProperty(ref _edition, value);
        }
        #endregion

        #region id
        private string _id;
        public string id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        #endregion

        #region logout
        private string _logout;
        public string logout
        {
            get => _logout;
            set => SetProperty(ref _logout, value);
        }
        #endregion

        #region account
        private string _account;
        public string account
        {
            get => _account;
            set => SetProperty(ref _account, value);
        }
        #endregion

        #region edit_account
        private string _edit_account;
        public string edit_account
        {
            get => _edit_account;
            set => SetProperty(ref _edit_account, value);
        }
        #endregion

        #region start_game
        private string _start_game;
        public string start_game
        {
            get => _start_game;
            set => SetProperty(ref _start_game, value);
        }
        #endregion

        #region installed_in_live_game_warning
        private string _installed_in_live_game_warning;
        public string installed_in_live_game_warning
        {
            get => _installed_in_live_game_warning;
            set => SetProperty(ref _installed_in_live_game_warning, value);
        }
        #endregion

        #region no_official_game_warning
        private string _no_official_game_warning;
        public string no_official_game_warning
        {
            get => _no_official_game_warning;
            set => SetProperty(ref _no_official_game_warning, value);
        }
        #endregion

        #region eft_exe_not_found_warning
        private string _eft_exe_not_found_warning;
        public string eft_exe_not_found_warning
        {
            get => _eft_exe_not_found_warning;
            set => SetProperty(ref _eft_exe_not_found_warning, value);
        }
        #endregion

        #region account_exist
        private string _account_exist;
        public string account_exist
        {
            get => _account_exist;
            set => SetProperty(ref _account_exist, value);
        }
        #endregion

        #region url
        private string _url;
        public string url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }
        #endregion

        #region default_language
        private string _default_language;
        public string default_language
        {
            get => _default_language;
            set => SetProperty(ref _default_language, value);
        }
        #endregion

        #region game_path
        private string _game_path;
        public string game_path
        {
            get => _game_path;
            set => SetProperty(ref _game_path, value);
        }
        #endregion

        #region clear_game_settings
        private string _clear_game_settings;
        public string clear_game_settings
        {
            get => _clear_game_settings;
            set => SetProperty(ref _clear_game_settings, value);
        }
        #endregion

        #region clear_game_settings_succeeded
        private string _clear_game_settings_succeeded;
        public string clear_game_settings_succeeded
        {
            get => _clear_game_settings_succeeded;
            set => SetProperty(ref _clear_game_settings_succeeded, value);
        }
        #endregion

        #region clear_game_settings_failed
        private string _clear_game_settings_failed;
        public string clear_game_settings_failed
        {
            get => _clear_game_settings_failed;
            set => SetProperty(ref _clear_game_settings_failed, value);
        }
        #endregion

        #region load_live_settings
        private string _load_live_settings;
        public string load_live_settings
        {
            get => _load_live_settings;
            set => SetProperty(ref _load_live_settings, value);
        }
        #endregion

        #region load_live_settings_failed
        private string _load_live_settings_failed;
        public string load_live_settings_failed
        {
            get => _load_live_settings_failed;
            set => SetProperty(ref _load_live_settings_failed, value);
        }
        #endregion

        #region load_live_settings_succeeded
        private string _load_live_settings_succeeded;
        public string load_live_settings_succeeded
        {
            get => _load_live_settings_succeeded;
            set => SetProperty(ref _load_live_settings_succeeded, value);
        }
        #endregion

        #region remove_registry_keys
        private string _remove_registry_keys;
        public string remove_registry_keys
        {
            get => _remove_registry_keys;
            set => SetProperty(ref _remove_registry_keys, value);
        }
        #endregion

        #region remove_registry_keys_succeeded
        private string _remove_registry_keys_succeeded;
        public string remove_registry_keys_succeeded
        {
            get => _remove_registry_keys_succeeded;
            set => SetProperty(ref _remove_registry_keys_succeeded, value);
        }
        #endregion

        #region remove_registry_keys_failed
        private string _remove_registry_keys_failed;
        public string remove_registry_keys_failed
        {
            get => _remove_registry_keys_failed;
            set => SetProperty(ref _remove_registry_keys_failed, value);
        }
        #endregion

        #region clean_temp_files
        private string _clean_temp_files;
        public string clean_temp_files
        {
            get => _clean_temp_files;
            set => SetProperty(ref _clean_temp_files, value);
        }
        #endregion

        #region clean_temp_files_succeeded
        private string _clean_temp_files_succeeded;
        public string clean_temp_files_succeeded
        {
            get => _clean_temp_files_succeeded;
            set => SetProperty(ref _clean_temp_files_succeeded, value);
        }
        #endregion

        #region clean_temp_files_failed
        private string _clean_temp_files_failed;
        public string clean_temp_files_failed
        {
            get => _clean_temp_files_failed;
            set => SetProperty(ref _clean_temp_files_failed, value);
        }
        #endregion

        #region select_folder
        private string _select_folder;
        public string select_folder
        {
            get => _select_folder;
            set => SetProperty(ref _select_folder, value);
        }
        #endregion

        #region registration_failed
        private string _registration_failed;
        public string registration_failed
        {
            get => _registration_failed;
            set => SetProperty(ref _registration_failed, value);
        }
        #endregion

        #region registration_question_format_1
        private string _registration_question_format_1;
        public string registration_question_format_1
        {
            get => _registration_question_format_1;
            set => SetProperty(ref _registration_question_format_1, value);
        }
        #endregion

        #region minimize_action
        private string _minimize_action;
        public string minimize_action
        {
            get => _minimize_action;
            set => SetProperty(ref _minimize_action, value);
        }
        #endregion

        #region do_nothing_action
        private string _do_nothing_action;
        public string do_nothing_action
        {
            get => _do_nothing_action;
            set => SetProperty(ref _do_nothing_action, value);
        }
        #endregion

        #region exit_action
        private string _exit_action;
        public string exit_action
        {
            get => _exit_action;
            set => SetProperty(ref _exit_action, value);
        }
        #endregion

        #region on_game_start
        private string _on_game_start;
        public string on_game_start
        {
            get => _on_game_start;
            set => SetProperty(ref _on_game_start, value);
        }
        #endregion

        #region game
        private string _game;
        public string game
        {
            get => _game;
            set => SetProperty(ref _game, value);
        }
        #endregion

        #region new_password
        private string _new_password;
        public string new_password
        {
            get => _new_password;
            set => SetProperty(ref _new_password, value);
        }
        #endregion

        #region wipe_warning
        private string _wipe_warning;
        public string wipe_warning
        {
            get => _wipe_warning;
            set => SetProperty(ref _wipe_warning, value);
        }
        #endregion

        #region cancel
        private string _cancel;
        public string cancel
        {
            get => _cancel;
            set => SetProperty(ref _cancel, value);
        }
        #endregion

        #region need_an_account
        private string _need_an_account;
        public string need_an_account
        {
            get => _need_an_account;
            set => SetProperty(ref _need_an_account, value);
        }
        #endregion

        #region have_an_account
        private string _have_an_account;
        public string have_an_account
        {
            get => _have_an_account;
            set => SetProperty(ref _have_an_account, value);
        }
        #endregion

        #region reapply_patch
        private string _reapply_patch;
        public string reapply_patch
        {
            get => _reapply_patch;
            set => SetProperty(ref _reapply_patch, value);
        }
        #endregion

        #region failed_to_receive_patches
        private string _failed_to_receive_patches;
        public string failed_to_receive_patches
        {
            get => _failed_to_receive_patches;
            set => SetProperty(ref _failed_to_receive_patches, value);
        }
        #endregion

        #region failed_core_patch
        private string _failed_core_patch;
        public string failed_core_patch
        {
            get => _failed_core_patch;
            set => SetProperty(ref _failed_core_patch, value);
        }
        #endregion

        #region failed_mod_patch
        private string _failed_mod_patch;
        public string failed_mod_patch
        {
            get => _failed_mod_patch;
            set => SetProperty(ref _failed_mod_patch, value);
        }
        #endregion

        #region OK
        private string _ok;
        public string ok
        {
            get => _ok;
            set => SetProperty(ref _ok, value);
        }
        #endregion

        #region account_page_denied
        private string _account_page_denied;
        public string account_page_denied
        {
            get => _account_page_denied;
            set => SetProperty(ref _account_page_denied, value);
        }
        #endregion

        #region account_updated
        private string _account_updated;
        public string account_updated
        {
            get => _account_updated;
            set => SetProperty(ref _account_updated, value);
        }
        #endregion

        #region nickname
        private string _nickname;
        public string nickname
        {
            get => _nickname;
            set => SetProperty(ref _nickname, value);
        }
        #endregion

        #region side
        private string _side;
        public string side
        {
            get => _side;
            set => SetProperty(ref _side, value);
        }
        #endregion

        #region level
        private string _level;
        public string level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
        #endregion

        #region patching
        private string _patching;
        public string patching
        {
            get => _patching;
            set => SetProperty(ref _patching, value);
        }
        #endregion

        #region file_mismatch_dialog_message
        private string _file_mismatch_dialog_message;
        public string file_mismatch_dialog_message
        {
            get => _file_mismatch_dialog_message;
            set => SetProperty(ref _file_mismatch_dialog_message, value);
        }
        #endregion

        #region yes
        private string _yes;
        public string yes
        {
            get => _yes;
            set => SetProperty(ref _yes, value);
        }
        #endregion

        #region no
        private string _no;
        public string no
        {
            get => _no;
            set => SetProperty(ref _no, value);
        }
        #endregion

        #region profile_created
        private string _profile_created;
        public string profile_created
        {
            get => _profile_created;
            set => SetProperty(ref _profile_created, value);
        }
        #endregion

        #region open_folder
        private string _open_folder;
        public string open_folder
        {
            get => _open_folder;
            set => SetProperty(ref _open_folder, value);
        }
        #endregion

        #region select_edition
        private string _select_edition;
        public string select_edition
        {
            get => _select_edition;
            set => SetProperty(ref _select_edition, value);
        }
        #endregion

        #region copied
        private string _copied;
        public string copied
        {
            get => _copied;
            set => SetProperty(ref _copied, value);
        }
        #endregion

        #region next_level_in
        private string _next_level_in;
        public string next_level_in
        {
            get => _next_level_in;
            set => SetProperty(ref _next_level_in, value);
        }
        #endregion

        #region no_profile_data
        private string _no_profile_data;
        public string no_profile_data
        {
            get => _no_profile_data;
            set => SetProperty(ref _no_profile_data, value);
        }
        #endregion

        #region profile_version_mismatch
        private string _profile_version_mismath;
        public string profile_version_mismath
        {
            get => _profile_version_mismath;
            set => SetProperty(ref _profile_version_mismath, value);
        }
        #endregion

        #region profile_removed
        private string _profile_removed;
        public string profile_removed
        {
            get => _profile_removed;
            set => SetProperty(ref _profile_removed, value);
        }
        #endregion

        #region profile_removal_failed
        private string _profile_removal_failed;
        public string profile_removal_failed
        {
            get => _profile_removal_failed;
            set => SetProperty(ref _profile_removal_failed, value);
        }
        #endregion

        #region profile_remove_question_format_1
        private string _profile_remove_question_format_1;
        public string profile_remove_question_format_1
        {
            get => _profile_remove_question_format_1;
            set => SetProperty(ref _profile_remove_question_format_1, value);
        }
        #endregion

        #region i_understand
        private string _i_understand;
        public string i_understand
        {
            get => _i_understand;
            set => SetProperty(ref _i_understand, value);
        }
        #endregion

        #region game_version_mismatch_format_2
        private string _game_version_mismatch_format_2;
        public string game_version_mismatch_format_2
        {
            get => _game_version_mismatch_format_2;
            set => SetProperty(ref _game_version_mismatch_format_2, value);
        }
        #endregion

        #region description
        private string _description;
        public string description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        #endregion

        #region author
        private string _author;
        public string author
        {
            get => _author;
            set => SetProperty(ref _author, value);
        }

        #endregion

        #region wipe_on_start
        private string _wipe_on_start;
        public string wipe_on_start
        {
            get => _wipe_on_start;
            set => SetProperty(ref _wipe_on_start, value);
        }
        #endregion

        #region copy_live_settings_question
        private string _copy_live_settings_question;
        public string copy_live_settings_question
        {
            get => _copy_live_settings_question;
            set => SetProperty(ref _copy_live_settings_question, value);
        }
        #endregion
        
        #region mod_not_in_server_warning
        private string _mod_not_in_server_warning;
        public string mod_not_in_server_warning
        {
            get => _mod_not_in_server_warning;
            set => SetProperty(ref _mod_not_in_server_warning, value);
        }
        #endregion
        
        #region active_server_mods
        private string _active_server_mods;
        public string active_server_mods
        {
            get => _active_server_mods;
            set => SetProperty(ref _active_server_mods, value);
        }
        #endregion
        
        #region active_server_mods_info_text

        private string _active_server_mods_info_text;

        public string active_server_mods_info_text
        {
            get => _active_server_mods_info_text;
            set => SetProperty(ref _active_server_mods_info_text, value);
        }
        #endregion
        
        #region inactive_server_mods
        private string _inactive_server_mods;
        public string inactive_server_mods
        {
            get => _inactive_server_mods;
            set => SetProperty(ref _inactive_server_mods, value);
        }
        #endregion
        
        #region inactive_server_mods_info_text

        private string _inactive_server_mods_info_text;

        public string inactive_server_mods_info_text
        {
            get => _inactive_server_mods_info_text;
            set => SetProperty(ref _inactive_server_mods_info_text, value);
        }
        #endregion
        
        #region open_link_question_format_1

        private string _open_link_question_format_1;

        public string open_link_question_format_1
        {
            get => _open_link_question_format_1;
            set => SetProperty(ref _open_link_question_format_1, value);
        }
        #endregion
        
        #region open_link

        private string _open_link;

        public string open_link
        {
            get => _open_link;
            set => SetProperty(ref _open_link, value);
        }
        #endregion

        #region core_dll_file_version_mismatch
        private string _core_dll_file_version_mismatch;

        public string core_dll_file_version_mismatch
        {
            get => _core_dll_file_version_mismatch;
            set => SetProperty(ref _core_dll_file_version_mismatch, value);
        }

        #endregion

        #region create_password_title
        private string _create_password_title;
        public string create_password_title
        {
            get => _create_password_title;
            set => SetProperty(ref _create_password_title, value);
        }
        #endregion

        #region create_password_message
        private string _create_password_message;
        public string create_password_message
        {
            get => _create_password_message;
            set => SetProperty(ref _create_password_message, value);
        }
        #endregion

        #region create_password_confirm
        private string _create_password_confirm;
        public string create_password_confirm
        {
            get => _create_password_confirm;
            set => SetProperty(ref _create_password_confirm, value);
        }
        #endregion

        #region create_password_success
        private string _create_password_success;
        public string create_password_success
        {
            get => _create_password_success;
            set => SetProperty(ref _create_password_success, value);
        }
        #endregion

        #region reset_password
        private string _reset_password;
        public string reset_password
        {
            get => _reset_password;
            set => SetProperty(ref _reset_password, value);
        }
        #endregion

        #region reset_password_success
        private string _reset_password_success;
        public string reset_password_success
        {
            get => _reset_password_success;
            set => SetProperty(ref _reset_password_success, value);
        }
        #endregion

        #region reset_password_failed
        private string _reset_password_failed;
        public string reset_password_failed
        {
            get => _reset_password_failed;
            set => SetProperty(ref _reset_password_failed, value);
        }
        #endregion

        #region reset_password_hwid_mismatch
        private string _reset_password_hwid_mismatch;
        public string reset_password_hwid_mismatch
        {
            get => _reset_password_hwid_mismatch;
            set => SetProperty(ref _reset_password_hwid_mismatch, value);
        }
        #endregion

        #region repeat_password
        private string _repeat_password;
        public string repeat_password
        {
            get => _repeat_password;
            set => SetProperty(ref _repeat_password, value);
        }
        #endregion

        #region reset_password_no_hwid
        private string _reset_password_no_hwid;
        public string reset_password_no_hwid
        {
            get => _reset_password_no_hwid;
            set => SetProperty(ref _reset_password_no_hwid, value);
        }
        #endregion

        #region server_version
        private string _server_version;
        public string server_version
        {
            get => _server_version;
            set => SetProperty(ref _server_version, value);
        }
        #endregion

        #region update_strings
        private string _check_updates;
        public string check_updates { get => _check_updates; set => SetProperty(ref _check_updates, value); }

        private string _update_checking;
        public string update_checking { get => _update_checking; set => SetProperty(ref _update_checking, value); }

        private string _update_checking_file;
        public string update_checking_file { get => _update_checking_file; set => SetProperty(ref _update_checking_file, value); }

        private string _update_available;
        public string update_available { get => _update_available; set => SetProperty(ref _update_available, value); }

        private string _update_up_to_date;
        public string update_up_to_date { get => _update_up_to_date; set => SetProperty(ref _update_up_to_date, value); }

        private string _update_button;
        public string update_button { get => _update_button; set => SetProperty(ref _update_button, value); }

        private string _update_downloading;
        public string update_downloading { get => _update_downloading; set => SetProperty(ref _update_downloading, value); }

        private string _update_completed;
        public string update_completed { get => _update_completed; set => SetProperty(ref _update_completed, value); }

        private string _update_completed_with_errors;
        public string update_completed_with_errors { get => _update_completed_with_errors; set => SetProperty(ref _update_completed_with_errors, value); }

        private string _update_error;
        public string update_error { get => _update_error; set => SetProperty(ref _update_error, value); }

        private string _update_files_to_delete;
        public string update_files_to_delete { get => _update_files_to_delete; set => SetProperty(ref _update_files_to_delete, value); }

        private string _update_deleting;
        public string update_deleting { get => _update_deleting; set => SetProperty(ref _update_deleting, value); }
        #endregion

        #region trl_i18n_batch
        private string _nav_launcher;
        public string nav_launcher { get => _nav_launcher; set => SetProperty(ref _nav_launcher, value); }

        private string _settings;
        public string settings { get => _settings; set => SetProperty(ref _settings, value); }

        private string _nav_support_kofi;
        public string nav_support_kofi { get => _nav_support_kofi; set => SetProperty(ref _nav_support_kofi, value); }

        private string _under_construction;
        public string under_construction { get => _under_construction; set => SetProperty(ref _under_construction, value); }

        private string _nav_mod_list;
        public string nav_mod_list { get => _nav_mod_list; set => SetProperty(ref _nav_mod_list, value); }

        private string _save_and_go_back;
        public string save_and_go_back { get => _save_and_go_back; set => SetProperty(ref _save_and_go_back, value); }

        private string _settings_section_cleanup_data;
        public string settings_section_cleanup_data { get => _settings_section_cleanup_data; set => SetProperty(ref _settings_section_cleanup_data, value); }

        private string _clear_game_settings_tooltip;
        public string clear_game_settings_tooltip { get => _clear_game_settings_tooltip; set => SetProperty(ref _clear_game_settings_tooltip, value); }

        private string _clean_temp_files_tooltip;
        public string clean_temp_files_tooltip { get => _clean_temp_files_tooltip; set => SetProperty(ref _clean_temp_files_tooltip, value); }

        private string _copy_logs_tooltip;
        public string copy_logs_tooltip { get => _copy_logs_tooltip; set => SetProperty(ref _copy_logs_tooltip, value); }

        private string _settings_section_basic_options;
        public string settings_section_basic_options { get => _settings_section_basic_options; set => SetProperty(ref _settings_section_basic_options, value); }

        private string _enabled;
        public string enabled { get => _enabled; set => SetProperty(ref _enabled, value); }

        private string _disabled;
        public string disabled { get => _disabled; set => SetProperty(ref _disabled, value); }

        private string _use_performance_configs;
        public string use_performance_configs { get => _use_performance_configs; set => SetProperty(ref _use_performance_configs, value); }

        private string _use_performance_configs_description;
        public string use_performance_configs_description { get => _use_performance_configs_description; set => SetProperty(ref _use_performance_configs_description, value); }

        private string _admin_password_watermark;
        public string admin_password_watermark { get => _admin_password_watermark; set => SetProperty(ref _admin_password_watermark, value); }

        private string _toggle_password_visibility;
        public string toggle_password_visibility { get => _toggle_password_visibility; set => SetProperty(ref _toggle_password_visibility, value); }

        private string _toggle_dev_mode_tooltip;
        public string toggle_dev_mode_tooltip { get => _toggle_dev_mode_tooltip; set => SetProperty(ref _toggle_dev_mode_tooltip, value); }

        private string _dev_mode_description;
        public string dev_mode_description { get => _dev_mode_description; set => SetProperty(ref _dev_mode_description, value); }

        private string _settings_section_dev_tools;
        public string settings_section_dev_tools { get => _settings_section_dev_tools; set => SetProperty(ref _settings_section_dev_tools, value); }

        private string _server_url_label;
        public string server_url_label { get => _server_url_label; set => SetProperty(ref _server_url_label, value); }

        private string _server_url_watermark;
        public string server_url_watermark { get => _server_url_watermark; set => SetProperty(ref _server_url_watermark, value); }

        private string _use_local_server_checkbox;
        public string use_local_server_checkbox { get => _use_local_server_checkbox; set => SetProperty(ref _use_local_server_checkbox, value); }

        private string _use_local_server_tooltip;
        public string use_local_server_tooltip { get => _use_local_server_tooltip; set => SetProperty(ref _use_local_server_tooltip, value); }

        private string _use_local_server_description;
        public string use_local_server_description { get => _use_local_server_description; set => SetProperty(ref _use_local_server_description, value); }

        private string _block_launcher_update_checkbox;
        public string block_launcher_update_checkbox { get => _block_launcher_update_checkbox; set => SetProperty(ref _block_launcher_update_checkbox, value); }

        private string _block_launcher_update_tooltip;
        public string block_launcher_update_tooltip { get => _block_launcher_update_tooltip; set => SetProperty(ref _block_launcher_update_tooltip, value); }

        private string _block_launcher_update_description;
        public string block_launcher_update_description { get => _block_launcher_update_description; set => SetProperty(ref _block_launcher_update_description, value); }

        private string _homolog_mode_checkbox;
        public string homolog_mode_checkbox { get => _homolog_mode_checkbox; set => SetProperty(ref _homolog_mode_checkbox, value); }

        private string _homolog_mode_tooltip;
        public string homolog_mode_tooltip { get => _homolog_mode_tooltip; set => SetProperty(ref _homolog_mode_tooltip, value); }

        private string _homolog_mode_description;
        public string homolog_mode_description { get => _homolog_mode_description; set => SetProperty(ref _homolog_mode_description, value); }

        private string _dev_mode_status_active;
        public string dev_mode_status_active { get => _dev_mode_status_active; set => SetProperty(ref _dev_mode_status_active, value); }

        private string _dev_mode_status_inactive;
        public string dev_mode_status_inactive { get => _dev_mode_status_inactive; set => SetProperty(ref _dev_mode_status_inactive, value); }

        private string _dev_mode_enabled_notification;
        public string dev_mode_enabled_notification { get => _dev_mode_enabled_notification; set => SetProperty(ref _dev_mode_enabled_notification, value); }

        private string _wrong_password_notification;
        public string wrong_password_notification { get => _wrong_password_notification; set => SetProperty(ref _wrong_password_notification, value); }

        private string _dev_mode_disabled_notification;
        public string dev_mode_disabled_notification { get => _dev_mode_disabled_notification; set => SetProperty(ref _dev_mode_disabled_notification, value); }

        private string _select_spt_folder;
        public string select_spt_folder { get => _select_spt_folder; set => SetProperty(ref _select_spt_folder, value); }

        private string _nav_settings;
        public string nav_settings { get => _nav_settings; set => SetProperty(ref _nav_settings, value); }

        private string _nav_support_coffee;
        public string nav_support_coffee { get => _nav_support_coffee; set => SetProperty(ref _nav_support_coffee, value); }

        private string _nav_under_construction_tooltip;
        public string nav_under_construction_tooltip { get => _nav_under_construction_tooltip; set => SetProperty(ref _nav_under_construction_tooltip, value); }

        private string _nav_mods_list;
        public string nav_mods_list { get => _nav_mods_list; set => SetProperty(ref _nav_mods_list, value); }

        private string _sidebar_logout;
        public string sidebar_logout { get => _sidebar_logout; set => SetProperty(ref _sidebar_logout, value); }

        private string _version_panel_title;
        public string version_panel_title { get => _version_panel_title; set => SetProperty(ref _version_panel_title, value); }

        private string _version_server_label;
        public string version_server_label { get => _version_server_label; set => SetProperty(ref _version_server_label, value); }

        private string _version_launcher_label;
        public string version_launcher_label { get => _version_launcher_label; set => SetProperty(ref _version_launcher_label, value); }

        private string _optional_mods_panel_title;
        public string optional_mods_panel_title { get => _optional_mods_panel_title; set => SetProperty(ref _optional_mods_panel_title, value); }

        private string _account_panel_title;
        public string account_panel_title { get => _account_panel_title; set => SetProperty(ref _account_panel_title, value); }

        private string _account_change_edition_button;
        public string account_change_edition_button { get => _account_change_edition_button; set => SetProperty(ref _account_change_edition_button, value); }

        private string _account_wipe_button;
        public string account_wipe_button { get => _account_wipe_button; set => SetProperty(ref _account_wipe_button, value); }

        private string _account_wipe_tooltip;
        public string account_wipe_tooltip { get => _account_wipe_tooltip; set => SetProperty(ref _account_wipe_tooltip, value); }

        private string _account_delete_button;
        public string account_delete_button { get => _account_delete_button; set => SetProperty(ref _account_delete_button, value); }

        private string _account_delete_tooltip;
        public string account_delete_tooltip { get => _account_delete_tooltip; set => SetProperty(ref _account_delete_tooltip, value); }

        private string _account_wipe_delete_explainer;
        public string account_wipe_delete_explainer { get => _account_wipe_delete_explainer; set => SetProperty(ref _account_wipe_delete_explainer, value); }

        private string _carousel_change_background_tooltip;
        public string carousel_change_background_tooltip { get => _carousel_change_background_tooltip; set => SetProperty(ref _carousel_change_background_tooltip, value); }

        private string _update_cancel_button;
        public string update_cancel_button { get => _update_cancel_button; set => SetProperty(ref _update_cancel_button, value); }

        private string _update_cancel_tooltip;
        public string update_cancel_tooltip { get => _update_cancel_tooltip; set => SetProperty(ref _update_cancel_tooltip, value); }

        private string _last_update_report_tooltip;
        public string last_update_report_tooltip { get => _last_update_report_tooltip; set => SetProperty(ref _last_update_report_tooltip, value); }

        private string _profile_level_label;
        public string profile_level_label { get => _profile_level_label; set => SetProperty(ref _profile_level_label, value); }

        private string _profile_remaining_xp_label;
        public string profile_remaining_xp_label { get => _profile_remaining_xp_label; set => SetProperty(ref _profile_remaining_xp_label, value); }

        private string _profile_operator_label;
        public string profile_operator_label { get => _profile_operator_label; set => SetProperty(ref _profile_operator_label, value); }

        private string _profile_class_label;
        public string profile_class_label { get => _profile_class_label; set => SetProperty(ref _profile_class_label, value); }

        private string _profile_id_label;
        public string profile_id_label { get => _profile_id_label; set => SetProperty(ref _profile_id_label, value); }

        private string _profile_id_copy_tooltip;
        public string profile_id_copy_tooltip { get => _profile_id_copy_tooltip; set => SetProperty(ref _profile_id_copy_tooltip, value); }

        private string _profile_side_label;
        public string profile_side_label { get => _profile_side_label; set => SetProperty(ref _profile_side_label, value); }

        private string _verify_files_button;
        public string verify_files_button { get => _verify_files_button; set => SetProperty(ref _verify_files_button, value); }

        private string _play_button;
        public string play_button { get => _play_button; set => SetProperty(ref _play_button, value); }

        private string _optional_apply_error;
        public string optional_apply_error { get => _optional_apply_error; set => SetProperty(ref _optional_apply_error, value); }

        private string _optional_enable_failed_partial;
        public string optional_enable_failed_partial { get => _optional_enable_failed_partial; set => SetProperty(ref _optional_enable_failed_partial, value); }

        private string _optional_enable_failed_group_unavailable;
        public string optional_enable_failed_group_unavailable { get => _optional_enable_failed_group_unavailable; set => SetProperty(ref _optional_enable_failed_group_unavailable, value); }

        private string _optional_enabled_with_errors;
        public string optional_enabled_with_errors { get => _optional_enabled_with_errors; set => SetProperty(ref _optional_enabled_with_errors, value); }

        private string _optional_disabled_with_errors;
        public string optional_disabled_with_errors { get => _optional_disabled_with_errors; set => SetProperty(ref _optional_disabled_with_errors, value); }

        private string _update_generating_list;
        public string update_generating_list { get => _update_generating_list; set => SetProperty(ref _update_generating_list, value); }

        private string _update_preparing_retry_countdown;
        public string update_preparing_retry_countdown { get => _update_preparing_retry_countdown; set => SetProperty(ref _update_preparing_retry_countdown, value); }

        private string _update_cancelled_partial_state;
        public string update_cancelled_partial_state { get => _update_cancelled_partial_state; set => SetProperty(ref _update_cancelled_partial_state, value); }

        private string _update_completed_success;
        public string update_completed_success { get => _update_completed_success; set => SetProperty(ref _update_completed_success, value); }

        private string _update_up_to_date_preserved_suffix;
        public string update_up_to_date_preserved_suffix { get => _update_up_to_date_preserved_suffix; set => SetProperty(ref _update_up_to_date_preserved_suffix, value); }

        private string _update_check_cancelled_by_user;
        public string update_check_cancelled_by_user { get => _update_check_cancelled_by_user; set => SetProperty(ref _update_check_cancelled_by_user, value); }

        private string _sync_cancel_confirm_question;
        public string sync_cancel_confirm_question { get => _sync_cancel_confirm_question; set => SetProperty(ref _sync_cancel_confirm_question, value); }

        private string _sync_cancel_confirm_yes;
        public string sync_cancel_confirm_yes { get => _sync_cancel_confirm_yes; set => SetProperty(ref _sync_cancel_confirm_yes, value); }

        private string _sync_cancel_confirm_no;
        public string sync_cancel_confirm_no { get => _sync_cancel_confirm_no; set => SetProperty(ref _sync_cancel_confirm_no, value); }

        private string _last_update_files_updated;
        public string last_update_files_updated { get => _last_update_files_updated; set => SetProperty(ref _last_update_files_updated, value); }

        private string _action_generic_error;
        public string action_generic_error { get => _action_generic_error; set => SetProperty(ref _action_generic_error, value); }

        private string _wipe_remove_old_profile_error;
        public string wipe_remove_old_profile_error { get => _wipe_remove_old_profile_error; set => SetProperty(ref _wipe_remove_old_profile_error, value); }

        private string _wipe_create_new_profile_error;
        public string wipe_create_new_profile_error { get => _wipe_create_new_profile_error; set => SetProperty(ref _wipe_create_new_profile_error, value); }

        private string _wipe_success;
        public string wipe_success { get => _wipe_success; set => SetProperty(ref _wipe_success, value); }

        private string _wipe_confirm_question;
        public string wipe_confirm_question { get => _wipe_confirm_question; set => SetProperty(ref _wipe_confirm_question, value); }

        private string _account_deleted_success;
        public string account_deleted_success { get => _account_deleted_success; set => SetProperty(ref _account_deleted_success, value); }

        private string _account_delete_no_connection;
        public string account_delete_no_connection { get => _account_delete_no_connection; set => SetProperty(ref _account_delete_no_connection, value); }

        private string _account_delete_failed;
        public string account_delete_failed { get => _account_delete_failed; set => SetProperty(ref _account_delete_failed, value); }

        private string _class_effect_coming_soon;
        public string class_effect_coming_soon { get => _class_effect_coming_soon; set => SetProperty(ref _class_effect_coming_soon, value); }

        private string _class_selection_title;
        public string class_selection_title { get => _class_selection_title; set => SetProperty(ref _class_selection_title, value); }

        private string _class_selection_loading;
        public string class_selection_loading { get => _class_selection_loading; set => SetProperty(ref _class_selection_loading, value); }

        private string _class_selection_choose_button;
        public string class_selection_choose_button { get => _class_selection_choose_button; set => SetProperty(ref _class_selection_choose_button, value); }

        private string _common_back_button;
        public string common_back_button { get => _common_back_button; set => SetProperty(ref _common_back_button, value); }

        private string _class_selection_meta_label;
        public string class_selection_meta_label { get => _class_selection_meta_label; set => SetProperty(ref _class_selection_meta_label, value); }

        private string _class_selection_description_label;
        public string class_selection_description_label { get => _class_selection_description_label; set => SetProperty(ref _class_selection_description_label, value); }

        private string _class_selection_buffs_debuffs_label;
        public string class_selection_buffs_debuffs_label { get => _class_selection_buffs_debuffs_label; set => SetProperty(ref _class_selection_buffs_debuffs_label, value); }

        private string _class_selection_buffs_label;
        public string class_selection_buffs_label { get => _class_selection_buffs_label; set => SetProperty(ref _class_selection_buffs_label, value); }

        private string _class_selection_debuffs_label;
        public string class_selection_debuffs_label { get => _class_selection_debuffs_label; set => SetProperty(ref _class_selection_debuffs_label, value); }

        private string _class_selection_xp_multipliers_label;
        public string class_selection_xp_multipliers_label { get => _class_selection_xp_multipliers_label; set => SetProperty(ref _class_selection_xp_multipliers_label, value); }

        private string _register_username_collision_error;
        public string register_username_collision_error { get => _register_username_collision_error; set => SetProperty(ref _register_username_collision_error, value); }

        private string _register_username_verify_failed;
        public string register_username_verify_failed { get => _register_username_verify_failed; set => SetProperty(ref _register_username_verify_failed, value); }

        private string _notification_password_title;
        public string notification_password_title { get => _notification_password_title; set => SetProperty(ref _notification_password_title, value); }

        private string _register_password_save_failed;
        public string register_password_save_failed { get => _register_password_save_failed; set => SetProperty(ref _register_password_save_failed, value); }

        private string _register_create_account_error;
        public string register_create_account_error { get => _register_create_account_error; set => SetProperty(ref _register_create_account_error, value); }

        private string _class_selection_none_available;
        public string class_selection_none_available { get => _class_selection_none_available; set => SetProperty(ref _class_selection_none_available, value); }

        private string _connect_settings_button;
        public string connect_settings_button { get => _connect_settings_button; set => SetProperty(ref _connect_settings_button, value); }

        private string _login_welcome_heading;
        public string login_welcome_heading { get => _login_welcome_heading; set => SetProperty(ref _login_welcome_heading, value); }

        private string _login_username_label;
        public string login_username_label { get => _login_username_label; set => SetProperty(ref _login_username_label, value); }

        private string _login_username_watermark;
        public string login_username_watermark { get => _login_username_watermark; set => SetProperty(ref _login_username_watermark, value); }

        private string _login_password_label;
        public string login_password_label { get => _login_password_label; set => SetProperty(ref _login_password_label, value); }

        private string _login_password_watermark;
        public string login_password_watermark { get => _login_password_watermark; set => SetProperty(ref _login_password_watermark, value); }

        private string _toggle_password_visibility_tooltip;
        public string toggle_password_visibility_tooltip { get => _toggle_password_visibility_tooltip; set => SetProperty(ref _toggle_password_visibility_tooltip, value); }

        private string _login_forgot_password_prompt;
        public string login_forgot_password_prompt { get => _login_forgot_password_prompt; set => SetProperty(ref _login_forgot_password_prompt, value); }

        private string _login_reset_password_button;
        public string login_reset_password_button { get => _login_reset_password_button; set => SetProperty(ref _login_reset_password_button, value); }

        private string _login_remember_autologin_checkbox;
        public string login_remember_autologin_checkbox { get => _login_remember_autologin_checkbox; set => SetProperty(ref _login_remember_autologin_checkbox, value); }

        private string _login_submit_button;
        public string login_submit_button { get => _login_submit_button; set => SetProperty(ref _login_submit_button, value); }

        private string _login_no_account_prompt;
        public string login_no_account_prompt { get => _login_no_account_prompt; set => SetProperty(ref _login_no_account_prompt, value); }

        private string _login_register_button;
        public string login_register_button { get => _login_register_button; set => SetProperty(ref _login_register_button, value); }

        private string _register_heading;
        public string register_heading { get => _register_heading; set => SetProperty(ref _register_heading, value); }

        private string _register_username_label;
        public string register_username_label { get => _register_username_label; set => SetProperty(ref _register_username_label, value); }

        private string _register_username_watermark;
        public string register_username_watermark { get => _register_username_watermark; set => SetProperty(ref _register_username_watermark, value); }

        private string _register_password_label;
        public string register_password_label { get => _register_password_label; set => SetProperty(ref _register_password_label, value); }

        private string _register_password_watermark;
        public string register_password_watermark { get => _register_password_watermark; set => SetProperty(ref _register_password_watermark, value); }

        private string _register_confirm_password_label;
        public string register_confirm_password_label { get => _register_confirm_password_label; set => SetProperty(ref _register_confirm_password_label, value); }

        private string _register_confirm_password_watermark;
        public string register_confirm_password_watermark { get => _register_confirm_password_watermark; set => SetProperty(ref _register_confirm_password_watermark, value); }

        private string _register_submit_button;
        public string register_submit_button { get => _register_submit_button; set => SetProperty(ref _register_submit_button, value); }

        private string _register_have_account_prompt;
        public string register_have_account_prompt { get => _register_have_account_prompt; set => SetProperty(ref _register_have_account_prompt, value); }

        private string _register_back_to_login_button;
        public string register_back_to_login_button { get => _register_back_to_login_button; set => SetProperty(ref _register_back_to_login_button, value); }

        private string _connect_fetching_server_url;
        public string connect_fetching_server_url { get => _connect_fetching_server_url; set => SetProperty(ref _connect_fetching_server_url, value); }

        private string _connect_p2p_connecting;
        public string connect_p2p_connecting { get => _connect_p2p_connecting; set => SetProperty(ref _connect_p2p_connecting, value); }

        private string _connect_updating_network_config;
        public string connect_updating_network_config { get => _connect_updating_network_config; set => SetProperty(ref _connect_updating_network_config, value); }

        private string _connect_p2p_authkey_rejected;
        public string connect_p2p_authkey_rejected { get => _connect_p2p_authkey_rejected; set => SetProperty(ref _connect_p2p_authkey_rejected, value); }

        private string _connect_p2p_auth_failed;
        public string connect_p2p_auth_failed { get => _connect_p2p_auth_failed; set => SetProperty(ref _connect_p2p_auth_failed, value); }

        private string _connect_checking_launcher_updates;
        public string connect_checking_launcher_updates { get => _connect_checking_launcher_updates; set => SetProperty(ref _connect_checking_launcher_updates, value); }

        private string _connect_downloading_launcher_update;
        public string connect_downloading_launcher_update { get => _connect_downloading_launcher_update; set => SetProperty(ref _connect_downloading_launcher_update, value); }

        private string _connect_updating_launcher_restart;
        public string connect_updating_launcher_restart { get => _connect_updating_launcher_restart; set => SetProperty(ref _connect_updating_launcher_restart, value); }

        private string _connect_launcher_update_verification_failed;
        public string connect_launcher_update_verification_failed { get => _connect_launcher_update_verification_failed; set => SetProperty(ref _connect_launcher_update_verification_failed, value); }

        private string _connect_reconnecting;
        public string connect_reconnecting { get => _connect_reconnecting; set => SetProperty(ref _connect_reconnecting, value); }

        private string _connect_fatal_error;
        public string connect_fatal_error { get => _connect_fatal_error; set => SetProperty(ref _connect_fatal_error, value); }

        private string _autologin_connection_error;
        public string autologin_connection_error { get => _autologin_connection_error; set => SetProperty(ref _autologin_connection_error, value); }

        private string _register_invalid_username;
        public string register_invalid_username { get => _register_invalid_username; set => SetProperty(ref _register_invalid_username, value); }

        private string _register_passwords_mismatch;
        public string register_passwords_mismatch { get => _register_passwords_mismatch; set => SetProperty(ref _register_passwords_mismatch, value); }

        private string _change_edition_dialog_title;
        public string change_edition_dialog_title { get => _change_edition_dialog_title; set => SetProperty(ref _change_edition_dialog_title, value); }

        private string _change_edition_coop_warning;
        public string change_edition_coop_warning { get => _change_edition_coop_warning; set => SetProperty(ref _change_edition_coop_warning, value); }

        private string _confirmation_dialog_title;
        public string confirmation_dialog_title { get => _confirmation_dialog_title; set => SetProperty(ref _confirmation_dialog_title, value); }

        private string _delete_account_dialog_title;
        public string delete_account_dialog_title { get => _delete_account_dialog_title; set => SetProperty(ref _delete_account_dialog_title, value); }

        private string _delete_account_permanent_warning;
        public string delete_account_permanent_warning { get => _delete_account_permanent_warning; set => SetProperty(ref _delete_account_permanent_warning, value); }

        private string _delete_account_vs_wipe_explanation;
        public string delete_account_vs_wipe_explanation { get => _delete_account_vs_wipe_explanation; set => SetProperty(ref _delete_account_vs_wipe_explanation, value); }

        private string _delete_account_coop_warning;
        public string delete_account_coop_warning { get => _delete_account_coop_warning; set => SetProperty(ref _delete_account_coop_warning, value); }

        private string _delete_account_type_name_label;
        public string delete_account_type_name_label { get => _delete_account_type_name_label; set => SetProperty(ref _delete_account_type_name_label, value); }

        private string _cancel_button;
        public string cancel_button { get => _cancel_button; set => SetProperty(ref _cancel_button, value); }

        private string _delete_account_confirm_button;
        public string delete_account_confirm_button { get => _delete_account_confirm_button; set => SetProperty(ref _delete_account_confirm_button, value); }

        private string _register_dialog_title;
        public string register_dialog_title { get => _register_dialog_title; set => SetProperty(ref _register_dialog_title, value); }

        private string _warning_dialog_title;
        public string warning_dialog_title { get => _warning_dialog_title; set => SetProperty(ref _warning_dialog_title, value); }

        private string _mod_update_title;
        public string mod_update_title { get => _mod_update_title; set => SetProperty(ref _mod_update_title, value); }

        private string _back_button;
        public string back_button { get => _back_button; set => SetProperty(ref _back_button, value); }

        private string _mod_update_cancel_tooltip;
        public string mod_update_cancel_tooltip { get => _mod_update_cancel_tooltip; set => SetProperty(ref _mod_update_cancel_tooltip, value); }

        private string _mod_update_open_report_folder;
        public string mod_update_open_report_folder { get => _mod_update_open_report_folder; set => SetProperty(ref _mod_update_open_report_folder, value); }

        private string _mod_update_open_report_tooltip;
        public string mod_update_open_report_tooltip { get => _mod_update_open_report_tooltip; set => SetProperty(ref _mod_update_open_report_tooltip, value); }

        private string _mod_update_check_again;
        public string mod_update_check_again { get => _mod_update_check_again; set => SetProperty(ref _mod_update_check_again, value); }

        private string _mod_update_update_button;
        public string mod_update_update_button { get => _mod_update_update_button; set => SetProperty(ref _mod_update_update_button, value); }

        private string _mod_update_check_canceled;
        public string mod_update_check_canceled { get => _mod_update_check_canceled; set => SetProperty(ref _mod_update_check_canceled, value); }

        private string _mod_update_canceled_partial;
        public string mod_update_canceled_partial { get => _mod_update_canceled_partial; set => SetProperty(ref _mod_update_canceled_partial, value); }

        private string _mod_update_cancel_confirm_question;
        public string mod_update_cancel_confirm_question { get => _mod_update_cancel_confirm_question; set => SetProperty(ref _mod_update_cancel_confirm_question, value); }

        private string _mod_update_cancel_confirm_yes;
        public string mod_update_cancel_confirm_yes { get => _mod_update_cancel_confirm_yes; set => SetProperty(ref _mod_update_cancel_confirm_yes, value); }

        private string _mod_update_cancel_confirm_deny;
        public string mod_update_cancel_confirm_deny { get => _mod_update_cancel_confirm_deny; set => SetProperty(ref _mod_update_cancel_confirm_deny, value); }

        private string _plan_summary_to_update;
        public string plan_summary_to_update { get => _plan_summary_to_update; set => SetProperty(ref _plan_summary_to_update, value); }

        private string _plan_summary_preserved;
        public string plan_summary_preserved { get => _plan_summary_preserved; set => SetProperty(ref _plan_summary_preserved, value); }

        private string _plan_summary_to_seed;
        public string plan_summary_to_seed { get => _plan_summary_to_seed; set => SetProperty(ref _plan_summary_to_seed, value); }

        private string _plan_summary_forced_configs;
        public string plan_summary_forced_configs { get => _plan_summary_forced_configs; set => SetProperty(ref _plan_summary_forced_configs, value); }

        private string _plan_summary_to_move;
        public string plan_summary_to_move { get => _plan_summary_to_move; set => SetProperty(ref _plan_summary_to_move, value); }

        private string _plan_summary_to_remove;
        public string plan_summary_to_remove { get => _plan_summary_to_remove; set => SetProperty(ref _plan_summary_to_remove, value); }

        private string _plan_summary_dev_mode_warnings;
        public string plan_summary_dev_mode_warnings { get => _plan_summary_dev_mode_warnings; set => SetProperty(ref _plan_summary_dev_mode_warnings, value); }

        private string _password_placeholder;
        public string password_placeholder { get => _password_placeholder; set => SetProperty(ref _password_placeholder, value); }

        private string _titlebar_settings_tooltip;
        public string titlebar_settings_tooltip { get => _titlebar_settings_tooltip; set => SetProperty(ref _titlebar_settings_tooltip, value); }

        private string _version_footer_launcher_label;
        public string version_footer_launcher_label { get => _version_footer_launcher_label; set => SetProperty(ref _version_footer_launcher_label, value); }

        private string _version_footer_server_label;
        public string version_footer_server_label { get => _version_footer_server_label; set => SetProperty(ref _version_footer_server_label, value); }

        private string _window_title;
        public string window_title { get => _window_title; set => SetProperty(ref _window_title, value); }

        #endregion

        #endregion
    }
}
