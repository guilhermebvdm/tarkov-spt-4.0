using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.SPT;
using SPT.Launcher.Controllers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace SPT.Launcher
{
    public static class RequestHandler
    {
        private static Request request = new Request(null, "");

        public static string GetBackendUrl()
        {
            return request.RemoteEndPoint;
        }

        public static void ChangeBackendUrl(string remoteEndPoint)
        {
            request.RemoteEndPoint = remoteEndPoint;
        }

        public static void ChangeSession(string session)
        {
            request.Session = session;
        }

        public static string RequestConnect()
        {
            return request.GetJson("/launcher/server/connect");
        }

        public static string RequestLogin(LoginRequestData data)
        {
            return request.PostJson("/launcher/profile/login", Json.Serialize(data));
        }

        public static string RequestRegister(RegisterRequestData data)
        {
            return request.PostJson("/launcher/profile/register", Json.Serialize(data));
        }

        public static string RequestRemove(LoginRequestData data)
        {
            return request.PostJson("/launcher/profile/remove", Json.Serialize(data));
        }

        public static string RequestAccount(LoginRequestData data)
        {
            return request.PostJson("/launcher/profile/get", Json.Serialize(data));
        }

        public static string RequestProfileInfo(LoginRequestData data)
        {
            return request.PostJson("/launcher/profile/info", Json.Serialize(data));
        }

        public static string RequestExistingProfiles()
        {
            return request.GetJson("/launcher/profiles");
        }

        public static string RequestChangeUsername(ChangeRequestData data)
        {
            return request.PostJson("/launcher/profile/change/username", Json.Serialize(data));
        }

        public static string RequestChangePassword(ChangeRequestData data)
        {
            return request.PostJson("/launcher/profile/change/password", Json.Serialize(data));
        }

        public static string RequestWipe(RegisterRequestData data)
        {
            return request.PostJson("/launcher/profile/change/wipe", Json.Serialize(data));
        }

        public static string SendPing()
        {
            return request.GetJson("/launcher/ping");
        }

        public static string RequestServerVersion()
        {
            return request.GetJson("/launcher/server/version");
        }

        public static string RequestCompatibleGameVersion()
        {
            return request.GetJson("/launcher/profile/compatibleTarkovVersion");
        }

        public static string RequestLoadedServerMods()
        {
            return request.GetJson("/launcher/server/loadedServerMods");
        }

        public static string RequestProfileMods()
        {
            return request.GetJson("/launcher/server/serverModsUsedByProfile");
        }

        /// <summary>
        /// Registra o HWID no servidor via HWID Manager (porta 7075)
        /// </summary>
        public static string RequestHwidRegister(HwidRegisterRequestData data)
        {
            return PostToHwidManager("/launcher/hwid/register", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Reseta a senha via HWID Manager (porta 7075)
        /// </summary>
        public static string RequestHwidResetPassword(HwidResetPasswordRequestData data)
        {
            return PostToHwidManager("/launcher/hwid/reset-password", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Busca a versão do servidor via HWID Manager (porta 7075)
        /// </summary>
        public static string RequestRedLineVersion()
        {
            return GetFromHwidManager("/launcher/hwid/version");
        }

        /// <summary>
        /// Registra o IP do TailScale do jogador no servidor (porta 7075)
        /// </summary>
        public static string RequestRegisterPlayerIp(string username, string ip)
        {
            var data = new { username = username, ip = ip };
            return PostToHwidManager("/redline/register-player-ip", JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Busca a versão dos mods do servidor (endpoint leve, sem gerar manifesto)
        /// </summary>
        public static string RequestModsVersion()
        {
            return GetFromHwidManager("/launcher/mods/version");
        }

        /// <summary>
        /// Busca o hash do manifesto (endpoint leve para skip inteligente)
        /// </summary>
        public static string RequestManifestHash()
        {
            return GetFromHwidManager("/launcher/mods/manifest-hash");
        }

        /// <summary>
        /// Busca o manifesto de mods do servidor (porta 7075)
        /// </summary>
        public static string RequestModsManifest()
        {
            return GetFromHwidManager("/launcher/mods/manifest");
        }

        /// <summary>
        /// Baixa um arquivo de mod do servidor (porta 7075)
        /// </summary>
        public static byte[] DownloadModFile(string filePath)
        {
            try
            {
                var backendUri = new Uri(request.RemoteEndPoint);
                string url = $"http://{backendUri.Host}/launcher/mods/download?file={Uri.EscapeDataString(filePath)}";

                var httpRequest = WebRequest.Create(new Uri(url));
                httpRequest.Method = "GET";
                httpRequest.Timeout = 30000;

                using (var response = httpRequest.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var memStream = new MemoryStream())
                {
                    responseStream.CopyTo(memStream);
                    return memStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[ModUpdate] Download error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Envia POST para o HWID Manager na porta 7075
        /// </summary>
        private static string PostToHwidManager(string path, string jsonData)
        {
            try
            {
                // Extrair host do endpoint principal (ex: https://127.0.0.1:6969 -> 127.0.0.1)
                var backendUri = new Uri(request.RemoteEndPoint);
                string hwidUrl = $"http://{backendUri.Host}{path}";

                var httpRequest = WebRequest.Create(new Uri(hwidUrl));
                httpRequest.Method = "POST";
                httpRequest.ContentType = "application/json";
                httpRequest.Timeout = 5000; // 5 segundos de limite máximo

                var bytes = Encoding.UTF8.GetBytes(jsonData);
                httpRequest.ContentLength = bytes.Length;

                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                using (var response = httpRequest.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse httpResponse)
            {
                using (var stream = httpResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string body = reader.ReadToEnd();
                    LogManager.Instance.Warning($"[HWID] Request failed ({httpResponse.StatusCode}): {body}");
                    return body;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[HWID] Request error: {ex.Message}");
                return "{\"status\":\"CONNECTION_ERROR\"}";
            }
        }

        /// <summary>
        /// Envia GET para o HWID Manager na porta 7075
        /// </summary>
        private static string GetFromHwidManager(string path)
        {
            try
            {
                var backendUri = new Uri(request.RemoteEndPoint);
                string hwidUrl = $"http://{backendUri.Host}{path}";

                var httpRequest = WebRequest.Create(new Uri(hwidUrl));
                httpRequest.Method = "GET";
                httpRequest.Timeout = 3000;

                using (var response = httpRequest.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Warning($"[HWID] GET request error: {ex.Message}");
                return "{\"version\":\"?\"}";
            }
        }
    }
}
