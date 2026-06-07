using System;
using System.IO;
using System.Threading.Tasks;
using SPT.Launcher.Controllers;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Gerencia as configurações locais do Fika.
    /// </summary>
    public static class FikaConfigHelper
    {
        /// <summary>
        /// Atualiza Force IP e Force Bind IP no config do Fika para os padrões da Oracle Cloud.
        /// Preserva a formatação original do arquivo (encoding, line endings, linhas em branco).
        /// </summary>
        public static async Task UpdateFikaConfigAsync(string gamePath)
        {
            try
            {
                string configPath = Path.Combine(gamePath, "BepInEx", "config", "com.fika.core.cfg");
                
                if (!File.Exists(configPath))
                {
                    LogManager.Instance.Warning($"[FikaConfig] Config do Fika não encontrado: {configPath}");
                    return;
                }

                // Limpar Force IP (vazio) e setar Force Bind IP para 0.0.0.0
                string newForceIp = "";
                string newForceBindIp = "0.0.0.0";

                string[] lines = await Task.Run(() => File.ReadAllLines(configPath));
                bool changed = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    string trimmedLine = lines[i].TrimStart();
                    if (trimmedLine.StartsWith("Force IP", StringComparison.OrdinalIgnoreCase))
                    {
                        string newLine = "Force IP = " + newForceIp;
                        if (lines[i] != newLine)
                        {
                            lines[i] = newLine;
                            changed = true;
                        }
                    }
                    else if (trimmedLine.StartsWith("Force Bind IP", StringComparison.OrdinalIgnoreCase))
                    {
                        string newLine = "Force Bind IP = " + newForceBindIp;
                        if (lines[i] != newLine)
                        {
                            lines[i] = newLine;
                            changed = true;
                        }
                    }
                }

                if (changed)
                {
                    await Task.Run(() => File.WriteAllLines(configPath, lines, System.Text.Encoding.UTF8));
                    LogManager.Instance.Info($"[FikaConfig] Config do Fika atualizado para Oracle Cloud: Force IP = vazio, Force Bind IP = 0.0.0.0");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Error($"[FikaConfig] Erro ao atualizar config do Fika: {ex.Message}");
            }
        }
    }
}
