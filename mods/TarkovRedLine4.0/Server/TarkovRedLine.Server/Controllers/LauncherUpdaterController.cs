using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route("redline/launcher")]
public class LauncherUpdaterController : ControllerBase
{
    private static string GetUpdaterBasePath()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // Procurar a pasta Launcher-Updater subindo até 4 níveis
        for (int i = 0; i < 4; i++)
        {
            string testPath = Path.Combine(currentDir, "Launcher-Updater");
            if (Directory.Exists(testPath))
            {
                return Path.GetFullPath(testPath);
            }
            
            string parent = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(parent) || parent == currentDir) break;
            currentDir = parent;
        }

        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Launcher-Updater"));
    }

    private static string GetLauncherExePath()
    {
        return Path.Combine(GetUpdaterBasePath(), "Tarkov Red Line.exe");
    }

    [HttpGet("version")]
    public IActionResult GetLauncherVersion()
    {
        string exePath = GetLauncherExePath();
        if (System.IO.File.Exists(exePath))
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                // Utiliza a ProductVersion ou FileVersion do .exe
                string version = versionInfo.ProductVersion ?? versionInfo.FileVersion ?? "1.0.0";
                
                // Limpa sufixos se houver (ex: 1.4.0+commitHash)
                if (version.Contains('+'))
                {
                    version = version.Split('+')[0];
                }

                return Ok(new { version = version });
            }
            catch
            {
                return Ok(new { version = "1.0.0" });
            }
        }

        // Se o arquivo não existir na pasta do servidor, retornamos uma versão base baixa
        // para não disparar um falso positivo de atualização no cliente
        return Ok(new { version = "1.0.0" });
    }

    [HttpGet("download")]
    public IActionResult DownloadLauncher()
    {
        string exePath = GetLauncherExePath();
        if (System.IO.File.Exists(exePath))
        {
            return PhysicalFile(exePath, "application/octet-stream", "Tarkov Red Line.exe");
        }

        return NotFound(new { error = "Launcher executable not found on server" });
    }
}
