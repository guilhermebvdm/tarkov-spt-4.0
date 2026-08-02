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

        /// <summary>
        /// Prefixo aplicado APENAS às rotas do mod TarkovRedLine (não às do SPT core).
        /// Quando "Modo homolog" está ligado, vira "/homolog" para bater no mod
        /// TarkovRedLine.Server.Homolog (que roda no mesmo server, com rotas prefixadas).
        /// "" = produção (rotas normais). Lido ao vivo do setting — toggle sem restart.
        /// </summary>
        public static string ModRoutePrefix =>
            SPT.Launcher.Helpers.LauncherSettingsProvider.Instance.HomologMode ? "/homolog" : "";

        /// <summary>Prefixa uma rota do mod com <see cref="ModRoutePrefix"/> (no-op em produção).</summary>
        private static string M(string modPath) => ModRoutePrefix + modPath;

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
            return request.PostJson(M("/redline/profile/get"), Json.Serialize(data), compress: false, decompressResponse: false);
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
            return request.PostJson(M("/redline/password/change"), Json.Serialize(data), compress: false, decompressResponse: false);
        }

        /// <summary>
        /// Item 020 (A4/BR-020.3) — APAGA a chave do username no cofre TRL (redline_passwords.json),
        /// nunca grava senha vazia. Idempotente: username inexistente responde "OK". Usado após um
        /// remove/wipe bem-sucedido para não deixar entrada órfã reciclável.
        /// </summary>
        public static string RequestDeleteVaultEntry(ChangeRequestData data)
        {
            return request.PostJson(M("/redline/password/delete"), Json.Serialize(data), compress: false, decompressResponse: false);
        }

        public static string RequestWipe(RegisterRequestData data)
        {
            return request.PostJson("/launcher/profile/change/wipe", Json.Serialize(data));
        }

        /// <summary>
        /// Lista de classes do CustomClasses (item 058) — contrato SP0.
        /// Resposta vem zlib por default; GetJson já descomprime.
        /// </summary>
        public static string RequestClassList()
        {
            return request.GetJson("/customclasses/classes");
        }

        public static string SendPing()
        {
            return request.GetJson("/launcher/ping");
        }

        public static string RequestServerVersion()
        {
            return request.GetJson("/launcher/server/version");
        }

        /// <summary>
        /// Busca a versão TRL do servidor (endpoint redline, resposta JSON pura — sem zlib)
        /// </summary>
        public static string RequestTrlServerVersion()
        {
            return request.GetJson(M("/redline/server/version"), decompressResponse: false);
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
            return PostToHwidManager(M("/launcher/hwid/register"), JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Reseta a senha via HWID Manager (porta 7075)
        /// </summary>
        public static string RequestHwidResetPassword(HwidResetPasswordRequestData data)
        {
            return PostToHwidManager(M("/launcher/hwid/reset-password"), JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Busca a versão do servidor via HWID Manager (porta 7075)
        /// </summary>
        public static string RequestRedLineVersion()
        {
            return GetFromHwidManager(M("/launcher/hwid/version"));
        }

        /// <summary>
        /// Registra o IP do TailScale do jogador no servidor (porta 7075)
        /// </summary>
        public static string RequestRegisterPlayerIp(string username, string ip)
        {
            var data = new { username = username, ip = ip };
            return PostToHwidManager(M("/redline/register-player-ip"), JsonConvert.SerializeObject(data));
        }

        /// <summary>
        /// Busca a versão dos mods do servidor (endpoint leve, sem gerar manifesto)
        /// </summary>
        public static string RequestModsVersion()
        {
            return GetFromHwidManager(M("/launcher/mods/version"));
        }

        /// <summary>
        /// Busca o hash do manifesto (endpoint leve para skip inteligente)
        /// </summary>
        public static string RequestManifestHash()
        {
            return GetFromHwidManager(M("/launcher/mods/manifest-hash"));
        }

        /// <summary>
        /// Busca o manifesto de mods do servidor (porta 7075)
        /// </summary>
        public static string RequestModsManifest()
        {
            return GetFromHwidManager(M("/launcher/mods/manifest"));
        }

        /// <summary>
        /// Baixa um arquivo de mod do servidor (porta 7075).
        /// Item 021: <paramref name="timeoutMs"/> parametrizado — o download de grupos opcionais
        /// (arquivos grandes) passa um limite maior que os 30 s default do sync (R-1).
        /// </summary>
        public static byte[] DownloadModFile(string filePath, int timeoutMs = 30000, Action<long> onProgress = null)
        {
            return DownloadBinary($"{request.RemoteEndPoint}{M("/launcher/mods/download")}?file={Uri.EscapeDataString(filePath)}", timeoutMs, onProgress);
        }

        // Item 030 (S-7): DownloadOptionalFile / RequestOptionalsManifest / DownloadPerformanceFile /
        // RequestOptionalsList removidos — as rotas correspondentes saíram do servidor (modelo antigo de
        // opcionais + overlay de performance aposentados, D-13). O canal config-performance baixa do
        // /download comum, como o resto do manifesto.

        // Item 032: onProgress != null → lê em blocos e reporta os bytes ACUMULADOS durante a
        // transferência (medição intra-arquivo). null = caminho antigo (CopyTo), byte[] idêntico (CC-4).
        private static byte[] DownloadBinary(string url, int timeoutMs = 30000, Action<long> onProgress = null)
        {
            try
            {
                var httpRequest = WebRequest.Create(new Uri(url));
                httpRequest.Method = "GET";
                httpRequest.Timeout = timeoutMs;

                using (var response = httpRequest.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var memStream = new MemoryStream())
                {
                    if (onProgress == null)
                    {
                        responseStream.CopyTo(memStream);
                    }
                    else
                    {
                        var buffer = new byte[81920];
                        long total = 0;
                        int read;
                        while ((read = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            memStream.Write(buffer, 0, read);
                            total += read;
                            onProgress(total);
                        }
                    }
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
        /// Item 021: GET de texto puro (sem zlib) pelo backend real, rethrow em erro para o chamador
        /// tratar como falha visível — distinto de <see cref="GetFromHwidManager"/>, que engole o erro
        /// devolvendo um JSON placeholder (bom para versão/ping, ruim para um manifesto).
        /// </summary>
        private static string GetString(string url, int timeoutMs)
        {
            var httpRequest = WebRequest.Create(new Uri(url));
            httpRequest.Method = "GET";
            httpRequest.Timeout = timeoutMs;

            using (var response = httpRequest.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var reader = new StreamReader(responseStream))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Envia POST para o HWID Manager na porta 7075
        /// </summary>
        private static string PostToHwidManager(string path, string jsonData)
        {
            try
            {
                string hwidUrl = $"{request.RemoteEndPoint}{path}";

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
                string hwidUrl = $"{request.RemoteEndPoint}{path}";

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
