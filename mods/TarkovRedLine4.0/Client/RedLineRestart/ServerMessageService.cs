using BepInEx.Logging;
using System;
using System.Collections;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace RedLineRestart
{
    public class ServerMessageService
    {
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private readonly ManualLogSource _logger;

        public bool ShowMessage { get; private set; }
        public string MessageType { get; private set; } = "";
        public string DisplayContent { get; private set; } = "";
        public DateTime TargetTimeUtc { get; private set; }

        public ServerMessageService(ManualLogSource logger)
        {
            _logger = logger;
        }

        public IEnumerator CheckMessage(string url)
        {
            Task<string> task;
            try
            {
                task = _client.GetStringAsync(url);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"[ServerMessage] Erro ao criar request: {e.Message}");
                yield break;
            }

            while (!task.IsCompleted) yield return null;

            if (task.Status == TaskStatus.RanToCompletion)
            {
                ProcessMessage(task.Result?.Trim() ?? string.Empty);
            }
            else
            {
                _logger.LogWarning("[ServerMessage] Falha ao consultar o Pastebin (timeout ou erro de rede). Mantendo último estado.");
            }
        }

        public void ProcessMessage(string rawText)
        {
            if (string.IsNullOrEmpty(rawText))
            {
                ShowMessage = false;
                return;
            }

            if (rawText.StartsWith("MENSAGEM:", StringComparison.OrdinalIgnoreCase))
            {
                MessageType = "MSG";
                DisplayContent = rawText.Substring("MENSAGEM:".Length).Trim();
                ShowMessage = !string.IsNullOrEmpty(DisplayContent);
            }
            else if (rawText.StartsWith("TIMER:", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    string dataClean = rawText.Substring("TIMER:".Length).Trim();
                    string[] partes = dataClean.Split('|');

                    if (partes.Length >= 2 && !string.IsNullOrWhiteSpace(partes[1]))
                    {
                        string dataBrasiliaString = partes[0].Trim();
                        string textoMensagem = partes[1].Trim();

                        DateTime dataBrasilia = DateTime.ParseExact(dataBrasiliaString, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        TargetTimeUtc = dataBrasilia.AddHours(3);

                        MessageType = "TIMER";
                        DisplayContent = textoMensagem;
                        ShowMessage = true;
                    }
                    else
                    {
                        ShowMessage = false;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"[ServerMessage] Erro ao ler data do Timer (esperado dd/MM/yyyy HH:mm:ss | texto): {e.Message}");
                    ShowMessage = false;
                }
            }
            else
            {
                ShowMessage = false;
            }
        }

        public string GetCountdownText()
        {
            TimeSpan restante = TargetTimeUtc - DateTime.UtcNow;
            if (restante.TotalSeconds < 0) restante = TimeSpan.Zero;

            string relogio = string.Format("{0:D2}:{1:D2}:{2:D2}", restante.Hours, restante.Minutes, restante.Seconds);
            if (restante.Days > 0) relogio = restante.Days + "d " + relogio;
            return relogio;
        }
    }
}
