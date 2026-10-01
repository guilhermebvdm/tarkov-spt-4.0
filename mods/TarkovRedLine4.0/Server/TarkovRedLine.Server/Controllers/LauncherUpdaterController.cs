using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace TarkovRedLine.Server.Controllers;

[ApiController]
[Route(ModRouting.RoutePrefix + "redline/launcher")]
public class LauncherUpdaterController : ControllerBase
{
    internal static string GetUpdaterBasePath()
    {
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // Procurar a pasta Launcher-Updater subindo até 4 níveis
        for (int i = 0; i < 4; i++)
        {
            string testPath = Path.Combine(currentDir, ModRouting.UpdaterFolderName);
            if (Directory.Exists(testPath))
            {
                return Path.GetFullPath(testPath);
            }
            
            string parent = Path.GetDirectoryName(currentDir);
            if (string.IsNullOrEmpty(parent) || parent == currentDir) break;
            currentDir = parent;
        }

        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", ModRouting.UpdaterFolderName));
    }

    // Nome do exe do launcher NA PASTA Launcher-Updater (o operador coloca o exe com este nome).
    // = nome do exe instalado no cliente (TRL.Launcher.exe). O self-update do cliente grava no
    // próprio binário (Process.MainModule), então este nome é só o do arquivo servido/lido no server.
    internal const string LauncherExeFileName = "TarkovRedLine.exe";

    internal static string GetLauncherExePath()
    {
        string basePath = GetUpdaterBasePath();
        string preferred = Path.Combine(basePath, "TarkovRedLine.exe");
        if (System.IO.File.Exists(preferred)) return preferred;

        string name2 = Path.Combine(basePath, "Tarkov Red Line.exe");
        if (System.IO.File.Exists(name2)) return name2;

        string name3 = Path.Combine(basePath, "TRL.Launcher.exe");
        if (System.IO.File.Exists(name3)) return name3;

        return preferred;
    }

    private static string GetLauncherSignaturePath()
    {
        return GetLauncherExePath() + ".sig";
    }

    // Deve casar com UpdateTrust.SignatureAlgorithm no cliente (D-018.1). O .sig atual é RSA-2048
    // PKCS#1 v1.5 sobre SHA-256 (o que `openssl dgst -sha256 -sign` produz).
    private const string SignatureAlgorithm = "RSA-SHA256";

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

                // Item 018: inclui a assinatura base64 do exe, se publicada. AUSÊNCIA do .sig →
                // signature = null → cliente ABORTA o update (fail-closed, 01-spec CA-3). Nunca
                // servimos um exe "novo" sem uma assinatura que o cliente possa verificar.
                string signature = ReadSignatureBase64();

                return Ok(new
                {
                    version = version,
                    signature = signature,
                    algorithm = signature != null ? SignatureAlgorithm : null
                });
            }
            catch
            {
                return Ok(new { version = "1.0.0", signature = (string)null, algorithm = (string)null });
            }
        }

        // Se o arquivo não existir na pasta do servidor, retornamos uma versão base baixa
        // para não disparar um falso positivo de atualização no cliente
        return Ok(new { version = "1.0.0", signature = (string)null, algorithm = (string)null });
    }

    [HttpGet("download")]
    public IActionResult DownloadLauncher()
    {
        string exePath = GetLauncherExePath();
        if (System.IO.File.Exists(exePath))
        {
            return PhysicalFile(exePath, "application/octet-stream", Path.GetFileName(exePath));
        }

        return NotFound(new { error = "Launcher executable not found on server" });
    }

    // Item 018: endpoint irmão que serve o .sig cru (defensivo/diagnóstico). O cliente hoje usa o
    // campo 'signature' do /version; este endpoint existe para inspeção manual do release (Gate G-4).
    [HttpGet("signature")]
    public IActionResult GetLauncherSignature()
    {
        string sigPath = GetLauncherSignaturePath();
        if (System.IO.File.Exists(sigPath))
        {
            return PhysicalFile(sigPath, "application/octet-stream", LauncherExeFileName + ".sig");
        }

        return NotFound(new { error = "Launcher signature not found on server" });
    }

    private static string ReadSignatureBase64()
    {
        try
        {
            string sigPath = GetLauncherSignaturePath();
            if (System.IO.File.Exists(sigPath))
            {
                byte[] sig = System.IO.File.ReadAllBytes(sigPath);
                if (sig.Length > 0)
                {
                    return Convert.ToBase64String(sig);
                }
            }
        }
        catch
        {
            // Falha ao ler o .sig → trata como ausente → cliente fail-closed.
        }

        return null;
    }
}
