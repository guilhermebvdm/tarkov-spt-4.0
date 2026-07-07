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
        /// ref: item 017 — config-server passa a ser SEED (config-server → config, cópia só-se-ausente,
        /// nunca deleta/sobrescreve). Por ser NÃO-destrutiva (o oposto do mirror-delete), a regra de
        /// seed É segura no fallback: mesmo sem folderRules do server, o client semeia os defaults.
        /// Substitui o antigo config-server→mirror-delete (que era opt-in e nunca ativado pelo operador).
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> FallbackRules = new Dictionary<string, string>
        {
            ["config"] = "preserve-divergent",
            ["BepInEx/config"] = "preserve-divergent",
            ["config-server"] = "seed-if-missing",
            ["BepInEx/config-server"] = "seed-if-missing",
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

        /// <summary>All prefixes whose rule is a mirror variant — these folders are scanned for local extras.</summary>
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
