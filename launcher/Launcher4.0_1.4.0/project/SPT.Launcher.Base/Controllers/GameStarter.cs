/* GameStarter.cs
 * License: NCSA Open Source License
 * 
 * Copyright: SPT
 * AUTHORS:
 * waffle.lord
 * reider123
 */


using SPT.Launcher.Helpers;
using SPT.Launcher.MiniCommon;
using SPT.Launcher.Models.Launcher;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;
using SPT.Launcher.Interfaces;
using System.Runtime.InteropServices;
using SPT.Launcher.Models.SPT;
using Newtonsoft.Json.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SPT.Launcher
{
    public class GameStarter
    {
        private readonly IGameStarterFrontend _frontend;
        private readonly bool _showOnly;
        private readonly string _originalGamePath;
        private readonly string[] _excludeFromCleanup;
        private const string registryInstall = @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\EscapeFromTarkov";

        private const string registrySettings = @"Software\Battlestate Games\EscapeFromTarkov";

        public GameStarter(IGameStarterFrontend frontend, string gamePath = null, string originalGamePath = null,
            bool showOnly = false, string[] excludeFromCleanup = null)
        {
            _frontend = frontend;
            _showOnly = showOnly;
            _originalGamePath = originalGamePath ??= DetectOriginalGamePath();
            _excludeFromCleanup = excludeFromCleanup ?? LauncherSettingsProvider.Instance.ExcludeFromCleanup;
        }

        private static string DetectOriginalGamePath()
        {
            // We can't detect the installed path on non-Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            var installLocation = Registry.LocalMachine.OpenSubKey(registryInstall, false)
                ?.GetValue("InstallLocation");
            var info = (installLocation is string key) ? new DirectoryInfo(key) : null;
            return info?.FullName;
        }

        public async Task<GameStarterResult> LaunchGame(ServerInfo server, AccountInfo account, string gamePath)
        {
            LogManager.Instance.Info(">>> Launching Game");
            LogManager.Instance.Info($">>> Account: {account.username}");
            LogManager.Instance.Info($">>> Server : {server.backendUrl}");
            // setup directories
            if (IsInstalledInLive(gamePath))
            {
                LogManager.Instance.Error("[LaunchGame] PERIGO: Mod instalado na pasta do jogo oficial! Launch cancelado para evitar banimento.");
                return GameStarterResult.FromError(-1);
            }

            // Confirm core.dll version matches version server is running
            if (IsCoreDllVersionMismatched(gamePath))
            {
                LogManager.Instance.Error("[LaunchGame] Core dll mismatch :: FAILED");
                return GameStarterResult.FromError(-8);
            }
            
            LogManager.Instance.Info("[LaunchGame] Installed in Live :: NO");
            
            LogManager.Instance.Info("[LaunchGame] Setup Game Files ...");
            SetupGameFiles(gamePath);

            if (!ValidationUtil.Validate())
            {
                LogManager.Instance.Error("[LaunchGame] Game Validation   :: FAILED");
                return GameStarterResult.FromError(-2);
            }
            
            LogManager.Instance.Info("[LaunchGame] Game Validation   :: OK");

            if (account.wipe)
            {
                LogManager.Instance.Info("[LaunchGame] Wipe profile requested");
                RemoveProfileRegistryKeys(account.id, gamePath);
                CleanTempFiles(gamePath);
            }

            // check game path
            var clientExecutable = Path.Join(gamePath, "EscapeFromTarkov.exe");

            if (!File.Exists(clientExecutable))
            {
                LogManager.Instance.Error("[LaunchGame] Valid Game Path   :: FAILED");
                LogManager.Instance.Error($"Could not find {clientExecutable}");
                return GameStarterResult.FromError(-6);
            }
            
            LogManager.Instance.Info("[LaunchGame] Valid Game Path   :: OK");

            // apply patches
            ProgressReportingPatchRunner patchRunner = new ProgressReportingPatchRunner(gamePath);

            try
            {
                await _frontend.CompletePatchTask(patchRunner.PatchFiles());
            }
            catch (TaskCanceledException)
            {
                LogManager.Instance.Error("[LaunchGame] Applying Patch    :: FAILED");
                return GameStarterResult.FromError(-4);
            }
            
            LogManager.Instance.Info("[LaunchGame] Applying Patch    :: OK");
            
            //start game
            var args =
                $"-force-gfx-jobs native -token={account.id} -config={Json.SerializeSingleQuotes(new ClientConfig(server.backendUrl))}";

            if (_showOnly)
            {
                Console.WriteLine($"{clientExecutable} {args}");
                LogManager.Instance.Info("[LaunchGame] NOOP :: show only");
            }
            else
            {
                var clientProcess = new ProcessStartInfo(clientExecutable)
                {
                    Arguments = args,
                    UseShellExecute = false,
                    WorkingDirectory = gamePath,
                };

                // Workaround for .NET 9 SingleFile DLL hijacking mitigations (SetDefaultDllDirectories)
                clientProcess.EnvironmentVariables["PATH"] = gamePath + ";" + Environment.GetEnvironmentVariable("PATH");

                try
                {
                    Process.Start(clientProcess);
                    LogManager.Instance.Info("[LaunchGame] Game process started");
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Exception(ex);
                    return GameStarterResult.FromError(-7);
                }
            }

            return GameStarterResult.FromSuccess();
        }

        bool IsInstalledInLive(string currentPath)
        {
            if (string.IsNullOrWhiteSpace(_originalGamePath))
                return false;

            try
            {
                var livePathNormalized = Path.GetFullPath(_originalGamePath).TrimEnd('\\', '/');
                var currentPathNormalized = Path.GetFullPath(currentPath).TrimEnd('\\', '/');

                if (string.Equals(livePathNormalized, currentPathNormalized, StringComparison.OrdinalIgnoreCase) || 
                    currentPathNormalized.StartsWith(livePathNormalized, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
            }

            return false;
        }

        static bool IsCoreDllVersionMismatched(string gamePath)
        {
            try
            {
                var serverVersion = new SPTVersion(ServerManager.GetVersion());

                var coreDllVersionInfo = FileVersionInfo.GetVersionInfo(Path.Join(gamePath, @"\BepinEx\plugins\spt", "spt-core.dll"));
                var dllVersion = new SPTVersion(coreDllVersionInfo.FileVersion);

                LogManager.Instance.Info($"[LaunchGame] spt-core.dll version: {dllVersion}");

                // Edge case, running on locally built modules dlls, ignore check and return ok
                if (dllVersion.Major == 1) return false;

                // check 'X'.x.x
                if (serverVersion.Major != dllVersion.Major) return true;

                // check x.'X'.x
                if (serverVersion.Minor != dllVersion.Minor) return true;

                // check x.x.'X'
                if (serverVersion.Build != dllVersion.Build) return true;

                return false; // Versions match, hooray
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
            }

            return true;
        }

        void SetupGameFiles(string gamePath)
        {
            var files = new []
            {
                GetFileForCleanup("BattlEye", gamePath),
                GetFileForCleanup("Logs", gamePath),
                GetFileForCleanup("ConsistencyInfo", gamePath),
                GetFileForCleanup("EscapeFromTarkov_BE.exe", gamePath),
                GetFileForCleanup("Uninstall.exe", gamePath),
                GetFileForCleanup("UnityCrashHandler64.exe", gamePath),
                GetFileForCleanup("WinPixEventRuntime.dll", gamePath),
                Path.Combine(gamePath, @"EscapeFromTarkov_Data\Plugins\x86_64\hwecho.dll")
            };

            foreach (var file in files)
            {
                try
                {
                    if (file == null)
                    {
                        continue;
                    }

                    if (Directory.Exists(file))
                    {
                        RemoveFilesRecurse(new DirectoryInfo(file));
                    }

                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Instance.Error($"[SetupGameFiles] Falha ao tentar limpar o arquivo/diretorio: {file} - {ex.Message}");
                }
            }
        }

        private string GetFileForCleanup(string fileName, string gamePath)
        {
            if (_excludeFromCleanup.Contains(fileName))
            {
                LogManager.Instance.Info($"Excluded {fileName} from file cleanup");
                return null;
            }
            
            return Path.Combine(gamePath, fileName);
        }

        /// <summary>
        /// Remove the SPT JSON-based registry keys associated with the given profile ID
        /// </summary>
		public void RemoveProfileRegistryKeys(string profileId, string gamePath)
        {
            var registryFile = new FileInfo(Path.Combine(gamePath, "SPT", "user", "sptRegistry", "registry.json"));

            if (!registryFile.Exists)
            {
                return;
            }

            JObject registryData = JObject.Parse(File.ReadAllText(registryFile.FullName));

            // Find any property that has a key containing the profileId, and remove it
            var propsToRemove = registryData.Properties().Where(prop => prop.Name.Contains(profileId, StringComparison.CurrentCultureIgnoreCase)).ToList();
            propsToRemove.ForEach(prop => prop.Remove());

            File.WriteAllText(registryFile.FullName, registryData.ToString());
        }

        /// <summary>
        /// Clean the temp folder
        /// </summary>
        /// <returns>returns true if the temp folder was cleaned succefully or doesn't exist. returns false if something went wrong.</returns>
		public bool CleanTempFiles(string gamePath)
        {
            var rootdir = new DirectoryInfo(Path.Combine(gamePath, "SPT", "user", "sptappdata"));

            if (!rootdir.Exists)
            {
                return true;
            }

            return RemoveFilesRecurse(rootdir);
        }

        bool RemoveFilesRecurse(DirectoryInfo basedir)
        {
            LogManager.Instance.Info($"Recursive Removal: {basedir}");
            
            if (!basedir.Exists)
            {
                return true;
            }

            try
            {
                // remove subdirectories
                foreach (var dir in basedir.EnumerateDirectories())
                {
                    RemoveFilesRecurse(dir);
                }

                // remove files
                var files = basedir.GetFiles();

                foreach (var file in files)
                {
                    file.IsReadOnly = false;
                    file.Delete();
                }

                // remove directory
                basedir.Delete();
            }
            catch (Exception ex)
            {
                LogManager.Instance.Exception(ex);
                return false;
            }

            return true;
        }
    }
}
