using BepInEx;
using Comfort.Common;
using EFT.UI;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedLineRestart
{
    [BepInPlugin("com.umbigopreto.redlinerestart", "RedLine Restarter", "2.5.0")]
    public class RedLinePlugin : BaseUnityPlugin
    {
        private const string GistUrl = "https://gist.githubusercontent.com/rockettechnology-dev/1fe6a8a243ea568c07e46a84744dff41/raw/gistfile1.txt";
        private RedLineUI _ui = new RedLineUI();
        private float _nextUpdate = 0f;
        private float _nextButtonAttempt = 0f;

        private ServerMessageService _serverMessage;
        private bool _serverMessageEnabled;
        private string _serverMessageUrl;
        private float _serverMessageCheckInterval;
        private float _nextMessageCheck = 0f;

        void Awake()
        {
            var configServerUrl = Config.Bind("General", "ServerUrl", "http://100.106.152.7:6969", "URL do Servidor SPT / Tarkov Red Line (ex: http://100.106.152.7:6969 ou http://127.0.0.1:6969)");
            RedLineState.ServerUrl = configServerUrl.Value.TrimEnd('/');

            var configMsgEnabled = Config.Bind("ServerMessage", "Enabled", true, "Ativa o banner de mensagem/timer administrativo lido do Pastebin.");
            var configMsgUrl = Config.Bind("ServerMessage", "Url", "https://pastebin.com/raw/q1CwdvV4", "URL raw do Pastebin com a mensagem/timer atual (formato MENSAGEM: ... ou TIMER: dd/MM/yyyy HH:mm:ss | ...).");
            var configMsgInterval = Config.Bind("ServerMessage", "CheckInterval", 60f, "Intervalo em segundos entre consultas ao Pastebin.");

            _serverMessageEnabled = configMsgEnabled.Value;
            _serverMessageUrl = configMsgUrl.Value;
            _serverMessageCheckInterval = Mathf.Max(5f, configMsgInterval.Value);
            _serverMessage = new ServerMessageService(Logger);

            Logger.LogInfo($"[RedLine] Plugin 2.5.0 iniciado. Buscando URL atualizada via Gist...");
            _ = FetchGistUrlAsync();
        }

        private async Task FetchGistUrlAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient { Timeout = System.TimeSpan.FromSeconds(5) })
                {
                    string content = await client.GetStringAsync(GistUrl);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        string formatted = content.Trim().Replace("https://", "http://").TrimEnd('/');
                        RedLineState.ServerUrl = formatted;
                        Logger.LogInfo($"[RedLine] ServerUrl atualizado via Gist para: {RedLineState.ServerUrl}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logger.LogWarning($"[RedLine] Falha ao consultar Gist ({ex.Message}). Usando ServerUrl configurado: {RedLineState.ServerUrl}");
            }
        }

        void Update()
        {
            if (_serverMessageEnabled && Time.time > _nextMessageCheck)
            {
                _nextMessageCheck = Time.time + _serverMessageCheckInterval;
                StartCoroutine(_serverMessage.CheckMessage(_serverMessageUrl));
            }

            // Se o jogo não inicializou o sistema de áudio ou UI, não faz nada (Evita Crash)
            if (!Singleton<GUISounds>.Instantiated) return;

            // Tenta criar o botão apenas a cada 3 segundos se ele não existir
            // Isso impede que o mod sobrecarregue o jogo durante o Load Screen
            if (!_ui.IsButtonReady && Time.time > _nextButtonAttempt)
            {
                _nextButtonAttempt = Time.time + 3.0f;
                _ui.TryCreateButton();
            }

            // Se o botão existe, atualizamos a lógica
            if (_ui.IsButtonReady)
            {
                _ui.UpdateButtonVisuals();

                // Check no servidor a cada 1.5s
                if (Time.time > _nextUpdate)
                {
                    _nextUpdate = Time.time + 1.5f;
                    StartCoroutine(RedLineAPI.CheckVoteStatus());
                }

                // Atalhos de teclado (Só funcionam se votação ativa e não vetada)
                if (RedLineState.InProgress && !RedLineState.IsVetoed && !RedLineState.VotedInThisSession)
                {
                    if (Input.GetKeyDown(KeyCode.Y)) StartCoroutine(RedLineAPI.SendVote(true));
                    if (Input.GetKeyDown(KeyCode.N)) StartCoroutine(RedLineAPI.SendVote(false));
                }
            }
        }

        void OnGUI()
        {
            _ui.DrawServerMessageBanner(_serverMessage);
            _ui.DrawWindow(this);
        }
    }
}