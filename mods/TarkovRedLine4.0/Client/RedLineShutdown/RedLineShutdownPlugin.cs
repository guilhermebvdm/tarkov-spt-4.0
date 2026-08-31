using BepInEx;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RedLineShutdown
{
    [BepInPlugin("com.umbigopreto.redlineshutdown", "RedLine Shutdown Headless", "1.0.2")]
    public class RedLineShutdownPlugin : BaseUnityPlugin
    {
        private const string GistUrl = "https://gist.githubusercontent.com/rockettechnology-dev/1fe6a8a243ea568c07e46a84744dff41/raw/gistfile1.txt";
        private string _serverUrl = "http://100.106.152.7:6969";

        private void Awake()
        {
            var configServerUrl = Config.Bind("General", "ServerUrl", "http://100.106.152.7:6969", "URL do Servidor SPT / Tarkov Red Line (ex: http://100.106.152.7:6969)");
            _serverUrl = configServerUrl.Value.TrimEnd('/');

            Logger.LogInfo($"[RedLineShutdown] Plugin 1.0.2 iniciado. Buscando URL atualizada via Gist...");
            StartCoroutine(FetchGistUrlAndStart());
        }

        private IEnumerator FetchGistUrlAndStart()
        {
            using (UnityWebRequest req = UnityWebRequest.Get(GistUrl))
            {
                req.timeout = 5;
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string content = req.downloadHandler.text?.Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        string formatted = content.Replace("https://", "http://").TrimEnd('/');
                        _serverUrl = formatted;
                        Logger.LogInfo($"[RedLineShutdown] ServerUrl atualizado via Gist para: {_serverUrl}");
                    }
                }
                else
                {
                    Logger.LogWarning($"[RedLineShutdown] Falha ao consultar Gist ({req.error}). Usando ServerUrl configurado: {_serverUrl}");
                }
            }

            StartCoroutine(CheckShutdownLoop());
        }

        private IEnumerator CheckShutdownLoop()
        {
            // Aguarda 5 segundos antes de começar a checar
            yield return new WaitForSecondsRealtime(5f);

            int checkCount = 0;

            while (true)
            {
                // Checa a cada 10 segundos
                yield return new WaitForSecondsRealtime(10f);

                checkCount++;
                string url = $"{_serverUrl}/redline/vote/status";

                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    webRequest.timeout = 5;
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                    {
                        string json = webRequest.downloadHandler.text;

                        // LOG VERBOSE: mostra o JSON a cada check para diagnóstico
                        Logger.LogInfo($"[RedLineShutdown] [CHECK #{checkCount}] Resposta do servidor: {json}");

                        if (json.Contains("\"triggerRestart\":true") || json.Contains("\"triggerRestart\": true"))
                        {
                            Logger.LogWarning("[RedLineShutdown] !!! FLAG DE RESTART DETECTADA !!! Confirmando ao servidor e encerrando...");
                            // Primeiro confirma ao servidor que recebeu o comando (handshake ACK)
                            yield return StartCoroutine(AckRestartRoutine());
                            // Depois encerra o processo
                            Application.Quit();
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                    }
                    else
                    {
                        Logger.LogError($"[RedLineShutdown] [CHECK #{checkCount}] ERRO de conexão: {webRequest.error} (HTTP {webRequest.responseCode}) | URL: {url}");
                    }
                }
            }
        }

        /// <summary>
        /// Envia um POST ao servidor confirmando que o sinal de restart foi recebido.
        /// O servidor usa essa confirmação para zerar triggerRestart com precisão.
        /// </summary>
        private IEnumerator AckRestartRoutine()
        {
            string ackUrl = $"{_serverUrl}/redline/headless/ack-restart";
            byte[] bodyRaw = Encoding.UTF8.GetBytes("{}");

            using (UnityWebRequest webRequest = new UnityWebRequest(ackUrl, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 5;
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Logger.LogInfo("[RedLineShutdown] ACK enviado ao servidor com sucesso. Encerrando...");
                }
                else
                {
                    // Mesmo se falhar o ACK, o processo será encerrado de qualquer forma.
                    // O servidor tem um fallback de 60s para limpar a flag automaticamente.
                    Logger.LogWarning($"[RedLineShutdown] Falha ao enviar ACK ({webRequest.error}), encerrando mesmo assim.");
                }
            }
        }
    }
}
