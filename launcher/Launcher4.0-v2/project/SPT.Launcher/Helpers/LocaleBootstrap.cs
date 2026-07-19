using System;
using System.IO;
using System.Reflection;
using SPT.Launcher.Base.Helpers;   // SptPathHelper
using SPT.Launcher.Controllers;    // LogManager

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// i18n (TRL): garante que os locales OFICIAIS completos (en/pt) estejam na pasta de runtime,
    /// escrevendo-os a partir dos recursos EMBUTIDOS no exe quando faltarem ou divergirem.
    ///
    /// Corrige o bug em que a pasta de locales usada em runtime (SPT_Data/Launcher/Locales) vinha do
    /// SPT server (vanilla) SEM as chaves novas do TRL. O loader é tudo-ou-nada (rejeita o arquivo
    /// inteiro se faltar 1 chave) → o English (incompleto) nunca carregava e a troca de idioma travava,
    /// caindo no pt hardcoded do GenerateDefaultLocale.
    ///
    /// Roda no <see cref="Program.Main"/> ANTES de qualquer acesso ao LocalizationProvider — senão o
    /// Instance estático inicializaria do arquivo incompleto. Não referencia LocalizationProvider de
    /// propósito (não disparar o init estático dele aqui).
    /// </summary>
    public static class LocaleBootstrap
    {
        // arquivo no runtime -> LogicalName do recurso embutido (definido no .csproj).
        private static readonly (string File, string Resource)[] Supported =
        {
            ("English.json", "locale.English.json"),
            ("Portuguese.json", "locale.Portuguese.json"),
        };

        public static void EnsureInstalled()
        {
            try
            {
                string folder = Path.Join(SptPathHelper.SptRootPath, "SPT_Data", "Launcher", "Locales");
                Directory.CreateDirectory(folder);

                var asm = Assembly.GetExecutingAssembly();
                foreach (var (file, resource) in Supported)
                {
                    byte[] embedded = ReadResource(asm, resource);
                    if (embedded == null)
                    {
                        LogManager.Instance.Warning($"[Locale] Recurso embutido ausente: {resource}");
                        continue;
                    }

                    string target = Path.Join(folder, file);

                    // Só escreve se faltar OU divergir do embutido — evita churn de mtime e respeita um
                    // arquivo que já esteja idêntico ao oficial.
                    if (File.Exists(target) && File.ReadAllBytes(target).AsSpan().SequenceEqual(embedded))
                    {
                        continue;
                    }

                    File.WriteAllBytes(target, embedded);
                    LogManager.Instance.Info($"[Locale] Locale oficial (re)instalado no runtime: {file}");
                }
            }
            catch (Exception ex)
            {
                // Nunca deixa o boot cair por causa disto — no pior caso, o idioma segue no fallback antigo.
                try { LogManager.Instance.Error($"[Locale] Falha ao instalar locales embutidos: {ex.Message}"); }
                catch { /* sem canal de log confiável */ }
            }
        }

        private static byte[] ReadResource(Assembly asm, string logicalName)
        {
            using Stream stream = asm.GetManifestResourceStream(logicalName);
            if (stream == null) return null;

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
