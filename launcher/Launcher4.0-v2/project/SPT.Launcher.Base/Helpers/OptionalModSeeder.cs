using System.Collections.Generic;
using System.IO;
using System.Linq;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 033: calcula o estado INICIAL dos mods opcionais no login, ANTES do sync — para que um mod que
    /// o jogador já usa não seja movido para a quarentena por nascer desligado (D-6 do 030). Só age sobre
    /// ids ainda NÃO decididos; respeita escolhas anteriores (CA-033.3). Configs opcionais nunca são
    /// semeadas (CA-033.5). Recovery: olha SÓ BepInEx/plugins, não a quarentena (CC-13).
    ///
    /// Função PURA: <see cref="ComputeSeed"/> retorna o dicionário a semear; quem persiste é o caller (via
    /// <c>LauncherSettingsProvider.SeedOptionalDefaults</c>). Categorias vêm por parâmetro (o catálogo vive
    /// no projeto do app), mantendo esta classe testável no assembly Base.
    /// </summary>
    public static class OptionalModSeeder
    {
        // Categorias que nascem ligadas para um cliente LIMPO (sem nenhum plugin). "dev" fica de fora (CA-033.2).
        private static readonly HashSet<string> SeedOnCategories =
            new HashSet<string>(System.StringComparer.OrdinalIgnoreCase) { "opcionais", "pesados", "performance" };

        /// <summary>
        /// Estado inicial dos mods opcionais NÃO-decididos. Jogador COM plugins (existe .dll em
        /// BepInEx/plugins) → liga o que está instalado no disco; SEM nenhum plugin → liga as categorias
        /// Optional/Heavy/Performance (dev off). <paramref name="alreadyDecided"/> = ids já em
        /// EnabledOptionals; <paramref name="categoryById"/> = modId → categoryId (do catálogo).
        /// </summary>
        public static IReadOnlyDictionary<string, bool> ComputeSeed(
            IReadOnlyList<ManifestFile> manifestFiles,
            string gameRoot,
            ISet<string> alreadyDecided,
            IReadOnlyDictionary<string, string> categoryById)
        {
            var seeded = new Dictionary<string, bool>();
            if (manifestFiles == null || string.IsNullOrEmpty(gameRoot)) return seeded;

            // id do mod opcional → os arquivos (paths relativos) que pertencem a ele no manifesto.
            var pathsByMod = manifestFiles
                .Where(f => f.optional && !string.IsNullOrEmpty(f.optionalId) && !string.IsNullOrEmpty(f.path))
                .GroupBy(f => f.optionalId)
                .ToDictionary(g => g.Key, g => g.Select(f => f.path).ToList());

            if (pathsByMod.Count == 0) return seeded;

            bool hasPlugins = HasAnyPlugin(gameRoot);

            foreach (var kvp in pathsByMod)
            {
                string id = kvp.Key;
                if (alreadyDecided != null && alreadyDecided.Contains(id)) continue; // já decidido — respeita

                bool enable;
                if (hasPlugins)
                {
                    // "instalado" = qualquer arquivo do mod presente no disco (CC-1/CC-1b).
                    enable = kvp.Value.Any(p => IsInstalled(gameRoot, p));
                }
                else
                {
                    string cat = categoryById != null && categoryById.TryGetValue(id, out var c) ? c : null;
                    enable = cat != null && SeedOnCategories.Contains(cat);
                }
                seeded[id] = enable;
            }

            return seeded;
        }

        /// <summary>Existe algum .dll sob BepInEx/plugins? NÃO olha plugins-disabled (CC-1c/CC-13).</summary>
        public static bool HasAnyPlugin(string gameRoot)
        {
            string dir = Path.Combine(gameRoot, "BepInEx", "plugins");
            if (!Directory.Exists(dir)) return false;
            try { return Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories).Any(); }
            catch { return false; }
        }

        /// <summary>
        /// "Instalado": o path do manifesto é um ARQUIVO concreto (o servidor emite os arquivos da pasta,
        /// não o prefixo), então File.Exists resolve tanto mod-.dll quanto mod-pasta (qualquer arquivo dela).
        /// </summary>
        private static bool IsInstalled(string gameRoot, string relPath)
        {
            string full = Path.Combine(gameRoot, relPath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(full);
        }
    }
}
