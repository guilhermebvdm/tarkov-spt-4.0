using BepInEx;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace RedLineShutdown
{
    [BepInPlugin("com.umbigopreto.redlineshutdown", "RedLine Shutdown Headless", "1.0.0")]
    public class RedLineShutdownPlugin : BaseUnityPlugin
    {
        private string _serverUrl = "http://8aff080e436d.sn.mynetname.net:7073";

        private void Awake()
        {
            Logger.LogInfo("[RedLineShutdown] Plugin iniciado. Modo Headless Shutdown ativado!");
            StartCoroutine(FetchServerUrlRoutine());
            StartCoroutine(CheckShutdownLoop());
        }

        private IEnumerator FetchServerUrlRoutine()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://pastebin.com/raw/PT4cMwLB"))
            {
                webRequest.timeout = 5;
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string pastebinContent = webRequest.downloadHandler.text;
                    if (!string.IsNullOrEmpty(pastebinContent))
                    {
                        string formattedUrl = pastebinContent.Trim()
                                .Replace("https://", "http://")
                                .Replace(":7073", ":7075");

                        _serverUrl = formattedUrl;
                        Logger.LogInfo($"[RedLineShutdown] ServerUrl atualizado via Pastebin para: {_serverUrl}");
                    }
                }
                else
                {
                    Logger.LogError($"[RedLineShutdown] Falha ao buscar Pastebin. Erro: {webRequest.error}");
                }
            }
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
