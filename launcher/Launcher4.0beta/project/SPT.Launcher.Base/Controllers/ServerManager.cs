/* ServerManager.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 */


using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.SPT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPT.Launcher
{
    public static class ServerManager
    {
        public static ServerInfo SelectedServer { get; private set; } = null;

        /// <summary>
        /// Versão TRL do servidor (endpoint /redline/server/version). "—" enquanto
        /// desconhecida ou se o fetch falhar; populada após o connect bem-sucedido.
        /// </summary>
        public static string TrlServerVersion { get; private set; } = "—";

        private class TrlServerVersionResponse
        {
            public string version { get; set; }
        }

        private static void LoadTrlServerVersion()
        {
            try
            {
                string json = RequestHandler.RequestTrlServerVersion();
                var data = Json.Deserialize<TrlServerVersionResponse>(json);

                if (!string.IsNullOrWhiteSpace(data?.version))
                {
                    TrlServerVersion = data.version;
                }
            }
            catch
            {
                // mantém "—" — footer nunca deve quebrar por falha de rede
            }
        }

        /// <summary>
        /// ref: CR-02-01 (013L) — refetch para sessões em que o fetch do connect falhou
        /// transitoriamente. Síncrono (chamar fora da UI thread); no-op quando já resolvida.
        /// </summary>
        public static string RefreshTrlServerVersionIfUnknown()
        {
            if (TrlServerVersion == "—")
            {
                LoadTrlServerVersion();
            }

            return TrlServerVersion;
        }

        public static bool PingServer()
        {
            string json = "";

            try
            {
                json = RequestHandler.SendPing();

                if(json != null) return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static string GetVersion()
        {
            try
            {
                string json = RequestHandler.RequestServerVersion();

                return Json.Deserialize<string>(json);
            }
            catch
            {
                return "";
            }
        }

        public static string GetCompatibleGameVersion()
        {
            try
            {
                string json = RequestHandler.RequestCompatibleGameVersion();

                return Json.Deserialize<string>(json);
            }
            catch
            {
                return "";
            }
        }

        public static Dictionary<string, SPTServerModInfo> GetLoadedServerMods()
        {
            try
            {
                string json = RequestHandler.RequestLoadedServerMods();

                return Json.Deserialize<Dictionary<string, SPTServerModInfo>>(json);
            }
            catch
            {
                return new Dictionary<string, SPTServerModInfo>();
            }
        }

        public static SPTProfileModInfo[] GetProfileMods()
        {
            try
            {
                string json = RequestHandler.RequestProfileMods();

                return Json.Deserialize<SPTProfileModInfo[]>(json);
            }
            catch
            {
                return new SPTProfileModInfo[] { };
            }
        }

        public static bool LoadServer(string backendUrl)
        {
            string json = "";

            try
            {
                RequestHandler.ChangeBackendUrl(backendUrl);
                json = RequestHandler.RequestConnect();
                SelectedServer = Json.Deserialize<ServerInfo>(json);
            }
            catch
            {
                SelectedServer = null;
                return false;
            }

            LoadTrlServerVersion();

            return true;
        }

        public static async Task<bool> LoadDefaultServerAsync(string server)
        {
            return await Task.Run(() => LoadServer(server));
        }
    }
}
