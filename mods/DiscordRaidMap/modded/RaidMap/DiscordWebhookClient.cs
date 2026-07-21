using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DiscordRaidMap.Discord
{
    internal sealed class DiscordWebhookClient
    {
        private const int TimeoutSeconds = 15;

        private readonly string _webhookUrl;
        private readonly HttpClient _http = new();
        private string _messageId;

        public DiscordWebhookClient(string webhookUrl)
        {
            _webhookUrl = webhookUrl.Trim().TrimEnd('/');
        }

        public async Task UpsertMessageAsync(byte[] image, string fileName, string contentType)
        {
            if (string.IsNullOrWhiteSpace(_messageId))
            {
                _messageId = await CreateMessageAsync(image, fileName, contentType);
                return;
            }

            await EditMessageAsync(image, fileName, contentType);
        }

        public async Task DeleteMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(_messageId))
            {
                return;
            }

            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
                using var req = new HttpRequestMessage(HttpMethod.Delete, $"{_webhookUrl}/messages/{_messageId}");
                using var resp = await _http.SendAsync(req, cts.Token);
                _messageId = null;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"Failed to delete Discord raid map message: {ex.Message}");
            }
        }

        private async Task<string> CreateMessageAsync(byte[] image, string fileName, string contentType)
        {
            var url = $"{_webhookUrl}?wait=true";
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            var form = CreateMultipartPayload(image, fileName, contentType);
            HttpResponseMessage resp = null;

            try
            {
                resp = await _http.PostAsync(url, form, cts.Token);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();
                var message = JsonConvert.DeserializeObject<WebhookMessage>(json);
                return message?.Id;
            }
            finally
            {
                resp?.Dispose();
                DisposeQuietly(form);
            }
        }

        private async Task EditMessageAsync(byte[] image, string fileName, string contentType)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            var form = CreateMultipartPayload(image, fileName, contentType);
            HttpRequestMessage req = null;
            HttpResponseMessage resp = null;

            try
            {
                req = new HttpRequestMessage(new HttpMethod("PATCH"), $"{_webhookUrl}/messages/{_messageId}")
                {
                    Content = form
                };

                resp = await _http.SendAsync(req, cts.Token);
                resp.EnsureSuccessStatusCode();
            }
            finally
            {
                resp?.Dispose();
                req?.Dispose();
                DisposeQuietly(form);
            }
        }

        private static MultipartFormDataContent CreateMultipartPayload(byte[] image, string fileName, string contentType)
        {
            var payload = new
            {
                username = Settings.MessageName.Value,
                flags = 4096,
                content = "",
                attachments = new[]
                {
                    new { id = 0, filename = fileName }
                }
            };

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"), "payload_json");
            var imageContent = new ByteArrayContent(image);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(imageContent, "files[0]", fileName);
            return form;
        }

        private static void DisposeQuietly(IDisposable disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch
            {
            }
        }

        private sealed class WebhookMessage
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }


    }
}
