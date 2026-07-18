using System;
using System.Collections.Generic;
using System.Linq;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Resolves the <see cref="SyncFolderRule"/> for a manifest path by longest-prefix match.
    /// Server-provided rules (manifest top-level "folderRules" map) overlay the built-in fallback table.
    /// </summary>
    public sealed class SyncRuleResolver
    {
        /// <summary>
        /// Built-in fallback (assumption A3, spec-tech 007): manifest paths mirror the game root.
        /// Both the raw card names (config/, plugins/, ...) and the BepInEx-style equivalents are
        /// mapped — unmatched prefixes are harmless. The server can override everything via
        /// "folderRules" in Launcher-Updater/config.json without a launcher rebuild.
        /// ref: CR-01-03 — mirror-delete (a regra mais destrutiva) NÃO entra no fallback: o layout
        /// de config-server no mods_repo real nunca foi verificado (A2), então config-server só
        /// vira espelho-com-delete via folderRules EXPLÍCITO do server. Default seguro até P-007.2.
        /// ref: config-server = mirror-reference (biblioteca de referência). Espelha a pasta do server
        /// na 'config-server/' do cliente (baixa a última versão; NÃO deleta extras; NUNCA toca 'config/').
        /// Quem distribui defaults é o canal 'config' (preserve-divergent). Seguro no fallback: só toca a
        /// pasta 'config-server' (referência nossa, que o usuário não edita, só consulta/copia).
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> FallbackRules = new Dictionary<string, string>
        {
            ["config"] = "preserve-divergent",
            ["BepInEx/config"] = "preserve-divergent",
            ["config-server"] = "mirror-reference",
            ["BepInEx/config-server"] = "mirror-reference",
            // config-force: canal deliberado de "essa config vai pra TODO MUNDO" — sobrescreve o
            // config/<rel> do usuário sempre que divergir (ignora customização). Ver SyncFolderRule.ForceToConfig.
            ["config-force"] = "force-to-config",
            ["BepInEx/config-force"] = "force-to-config",
            ["patchers"] = "mirror-move-disabled",
            ["BepInEx/patchers"] = "mirror-move-disabled",
            ["plugins"] = "mirror-move-disabled",
            ["BepInEx/plugins"] = "mirror-move-disabled",
        };

        // Normalized prefix -> rule, ordered by prefix length descending (longest match wins).
        private readonly List<KeyValuePair<string, SyncFolderRule>> _rules;

        public SyncRuleResolver(IReadOnlyDictionary<string, string> serverFolderRules = null)
        {
            var merged = new Dictionary<string, SyncFolderRule>(StringComparer.Ordinal);

            foreach (var kvp in FallbackRules)
            {
                if (SyncFolderRuleParser.TryParse(kvp.Value, out var rule))
                {
                    merged[SyncPathUtil.Normalize(kvp.Key)] = rule;
                }
            }

            if (serverFolderRules != null)
            {
                foreach (var kvp in serverFolderRules)
                {
                    if (string.IsNullOrWhiteSpace(kvp.Key)) continue;

                    if (SyncFolderRuleParser.TryParse(kvp.Value, out var rule))
                    {
                        merged[SyncPathUtil.Normalize(kvp.Key)] = rule;
                    }
                }
            }

            _rules = merged
                .OrderByDescending(kvp => kvp.Key.Length)
                .ToList();
        }

        /// <summary>
        /// All prefixes whose rule is a mirror variant — these folders are scanned for local extras.
        /// MirrorReference is intentionally NOT here: sua pasta 'config-server' sobrescreve pra última
        /// versão mas NÃO deleta extras (é referência nossa; delete-extras fica fora de escopo).
        /// </summary>
        public IEnumerable<KeyValuePair<string, SyncFolderRule>> MirrorPrefixes =>
            _rules.Where(r => r.Value == SyncFolderRule.MirrorDelete || r.Value == SyncFolderRule.MirrorMoveDisabled);

        public SyncFolderRule Resolve(string relativePath)
        {
            return Resolve(relativePath, out _);
        }

        /// <summary>Longest-prefix match; no match → <see cref="SyncFolderRule.Default"/>.</summary>
        public SyncFolderRule Resolve(string relativePath, out string matchedPrefix)
        {
            string normalized = SyncPathUtil.Normalize(relativePath);

            foreach (var kvp in _rules)
            {
                if (SyncPathUtil.IsUnderPrefix(normalized, kvp.Key))
                {
                    matchedPrefix = kvp.Key;
                    return kvp.Value;
                }
            }

            matchedPrefix = null;
            return SyncFolderRule.Default;
        }
    }
}
