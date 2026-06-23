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

            return true;
        }

        public static async Task<bool> LoadDefaultServerAsync(string server)
        {
            return await Task.Run(() => LoadServer(server));
        }
    }
}
