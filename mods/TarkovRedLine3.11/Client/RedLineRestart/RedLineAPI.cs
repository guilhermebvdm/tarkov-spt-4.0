using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RedLineRestart
{
    public static class RedLineAPI
    {
        private static readonly HttpClient _client = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        private static bool _isSendingRequest = false;
        private static int _consecutiveFailures = 0;

        public static IEnumerator CheckVoteStatus()
        {
            Task<string> task = null;
            try
            {
                task = _client.GetStringAsync(RedLineState.ServerUrl + "/redline/vote/status");
            }
            catch (Exception e)
            {
                Debug.LogError($"[RedLine] Erro ao criar request de status: {e.Message}");
                _consecutiveFailures++;
                yield break;
            }

            while (!task.IsCompleted) yield return null;

            if (task.Status == TaskStatus.RanToCompletion)
            {
                _consecutiveFailures = 0;
                try
                {
                    string json = task.Result;
                    bool wasInProgress = RedLineState.InProgress;

                    RedLineState.InProgress = json.Contains("\"inProgress\":true");
                    RedLineState.IsVetoed = json.Contains("\"isVetoed\":true");
                    RedLineState.TimeLeft = GetJsonInt(json, "timeLeft");
                    RedLineState.CooldownLeft = GetJsonInt(json, "cooldownLeft");
                    RedLineState.YesVotes = GetJsonInt(json, "yesVotes");

                    // LOGICA DE AUTO-ABERTURA:
                    // Se não estava em progresso, e agora está => Abre a janela para todos!
                    if (!wasInProgress && RedLineState.InProgress)
                    {
                        RedLineState.ShowVoteWindow = true;
                    }

                    // Se acabou, fecha e reseta flags
                    if (wasInProgress && !RedLineState.InProgress)
                    {
                        RedLineState.ShowVoteWindow = false;
                        RedLineState.VotedInThisSession = false;
                        RedLineState.AmITheInitiator = false;
                    }
                }
                catch { }
            }
            else
            {
                // Request falhou (timeout, rede, etc) — incrementa contador de falhas
                _consecutiveFailures++;

                // Após 5 falhas consecutivas, reseta o estado local para não travar a UI
                if (_consecutiveFailures >= 5)
                {
                    RedLineState.CooldownLeft = 0;
                    RedLineState.InProgress = false;
                    RedLineState.ShowVoteWindow = false;
                    RedLineState.VotedInThisSession = false;
                    RedLineState.AmITheInitiator = false;
                    _consecutiveFailures = 0;
                    Debug.LogWarning("[RedLine] 5 falhas consecutivas de status. Estado local resetado.");
                }
            }
        }

        public static IEnumerator SendVote(bool vote)
        {
            if (_isSendingRequest) yield break;
            _isSendingRequest = true;
            try
            {
                RedLineState.VotedInThisSession = true;

                string json = "{\"clientId\": \"" + RedLineState.ClientId + "\", \"vote\": " + (vote ? "true" : "false") + "}";
                yield return PostRequest("/redline/vote/cast", json);
            }
            finally
            {
                _isSendingRequest = false;
            }
        }

        public static IEnumerator CancelVote()
        {
            if (_isSendingRequest) yield break;
            _isSendingRequest = true;
            try
            {
                string json = "{\"clientId\": \"" + RedLineState.ClientId + "\"}";
                var task = PostRequestTask("/redline/vote/cancel", json);
                while (!task.IsCompleted) yield return null;

                if (task.Status == TaskStatus.RanToCompletion && task.Result)
                {
                    // Libera para votar de novo (seja Sim ou Não)
                    RedLineState.VotedInThisSession = false;
                }
            }
            finally
            {
                _isSendingRequest = false;
            }
        }

        public static IEnumerator TerminateSession()
        {
            if (_isSendingRequest) yield break;
            _isSendingRequest = true;
            try
            {
                yield return PostRequest("/redline/vote/terminate", "{}");
            }
            finally
            {
                _isSendingRequest = false;
            }
        }

        private static IEnumerator PostRequest(string endpoint, string json)
        {
            var task = PostRequestTask(endpoint, json);
            while (!task.IsCompleted) yield return null;
        }

        private static async Task<bool> PostRequestTask(string endpoint, string json)
        {
            try
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(RedLineState.ServerUrl + endpoint, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                Debug.LogError($"[RedLine] Erro de POST: {e.Message}");
                return false;
            }
        }

        private static int GetJsonInt(string json, string key)
        {
            try
            {
                string search = $"\"{key}\":";
                int start = json.IndexOf(search);
                if (start == -1) return 0;
                start += search.Length;
                int end = json.IndexOf(",", start);
                if (end == -1) end = json.IndexOf("}", start);
                return int.Parse(json.Substring(start, end - start));
            }
            catch { return 0; }
        }
    }
}