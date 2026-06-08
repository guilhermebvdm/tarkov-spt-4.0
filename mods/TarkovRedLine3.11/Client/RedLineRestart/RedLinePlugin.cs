using BepInEx;
using Comfort.Common;
using EFT.UI;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedLineRestart
{
    [BepInPlugin("com.umbigopreto.redlinerestart", "RedLine Restarter", "2.4.3")]
    public class RedLinePlugin : BaseUnityPlugin
    {
        private RedLineUI _ui = new RedLineUI();
        private float _nextUpdate = 0f;
        private float _nextButtonAttempt = 0f;

        void Awake()
        {
            Logger.LogInfo("[RedLine] Plugin 2.4.3 iniciado. Modo seguro de UI ativado.");
            _ = FetchServerUrlAsync();
        }

        private async Task FetchServerUrlAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string pastebinContent = await client.GetStringAsync("https://pastebin.com/raw/PT4cMwLB");
                    if (!string.IsNullOrEmpty(pastebinContent))
                    {
                        // O pastebin retorna: https://xxx.xxx.xxx.xxx:7073
                        // Substituímos https por http e 7073 por 7075
                        string formattedUrl = pastebinContent.Trim()
                                .Replace("https://", "http://")
                                .Replace(":7073", ":7075");
                        
                        RedLineState.ServerUrl = formattedUrl;
                        Logger.LogInfo($"[RedLine] ServerUrl atualizado via Pastebin para: {RedLineState.ServerUrl}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError($"[RedLine] Falha ao buscar ServerUrl do Pastebin. Usando IP local fallback. Erro: {e.Message}");
            }
        }

        void Update()
        {
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
            _ui.DrawWindow(this);
        }
    }
}