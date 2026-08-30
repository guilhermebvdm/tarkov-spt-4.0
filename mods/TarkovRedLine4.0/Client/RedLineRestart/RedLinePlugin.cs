using BepInEx;
using Comfort.Common;
using EFT.UI;
using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedLineRestart
{
    [BepInPlugin("com.umbigopreto.redlinerestart", "RedLine Restarter", "2.4.4")]
    public class RedLinePlugin : BaseUnityPlugin
    {
        private RedLineUI _ui = new RedLineUI();
        private float _nextUpdate = 0f;
        private float _nextButtonAttempt = 0f;

        void Awake()
        {
            var configServerUrl = Config.Bind("General", "ServerUrl", "http://127.0.0.1:6969", "URL do Servidor SPT / Tarkov Red Line (ex: http://127.0.0.1:6969 ou http://100.x.y.z:6969)");
            RedLineState.ServerUrl = configServerUrl.Value.TrimEnd('/');

            Logger.LogInfo($"[RedLine] Plugin 2.4.4 iniciado. ServerUrl configurado para: {RedLineState.ServerUrl}");
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