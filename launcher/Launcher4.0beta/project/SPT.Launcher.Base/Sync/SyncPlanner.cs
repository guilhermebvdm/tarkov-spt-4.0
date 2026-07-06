using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Builds a <see cref="SyncPlan"/> from the server manifest + local disk + baseline.
    /// Read-only: hashes files and enumerates folders, never writes.
    /// Rules R1–R5 and protections R2.3 are documented in 007-sincronizacao-arquivos-01-spec.md.
    /// </summary>
    public sealed class SyncPlanner
    {
        private readonly SyncRuleResolver _resolver;
        private readonly SyncBaseline _baseline;
        private readonly SyncPlannerOptions _options;
        private readonly List<string> _ignoredNormalized;
        private readonly List<string> _excludeNormalized;
        private readonly HashSet<string> _protectedNormalized;

        public SyncPlanner(SyncRuleResolver resolver, SyncBaseline baseline, SyncPlannerOptions options)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrEmpty(options.GameRoot))
            {
                throw new ArgumentException("GameRoot is required", nameof(options));
            }

            _ignoredNormalized = (options.IgnoredFiles ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(SyncPathUtil.Normalize)
                .ToList();

            _excludeNormalized = (options.ExcludeFromCleanup ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(SyncPathUtil.Normalize)
                .ToList();

            _protectedNormalized = new HashSet<string>(
                (options.ProtectedPaths ?? Array.Empty<string>()).Select(SyncPathUtil.Normalize),
                StringComparer.Ordinal);
        }

        public async Task<SyncPlan> BuildPlanAsync(
            IReadOnlyList<ManifestFile> manifestFiles,
            IProgress<SyncProgress> progress = null,
            CancellationToken cancellationToken = default)
        {
            var plan = new SyncPlan();
            manifestFiles = manifestFiles ?? Array.Empty<ManifestFile>();

            // Full manifest path set (mandatory + optional, active or not) — protection CC3:
            // files of disabled optional groups are never treated as extras.
            var manifestPaths = new HashSet<string>(
                manifestFiles.Select(f => SyncPathUtil.Normalize(f.path)),
                StringComparer.Ordinal);

            var filesToCheck = manifestFiles
                .Where(f => !f.optional || _options.IsOptionalGroupEnabled(f.optionalGroup ?? string.Empty))
                .ToList();

            int checkedCount = 0;

            foreach (var file in filesToCheck)
            {
                cancellationToken.ThrowIfCancellationRequested();

                checkedCount++;
                progress?.Report(new SyncProgress("checking", file.path, checkedCount, filesToCheck.Count));

                string normalized = SyncPathUtil.Normalize(file.path);

                // ref: CR-01-02 — ignoredFiles NÃO filtra o manifesto (semântica legada: protege
                // extras contra deleção no ScanExtras, nunca bloqueia update). Filtrar aqui pulava
                // silenciosamente os updates do SPT core (ignoredFiles default = "BepInEx/plugins/spt").

                var rule = _resolver.Resolve(normalized, out string matchedPrefix);

                // Item 017: seed rule — copy SERVER config-server/<rel> to USER config/<rel> only
                // when the target is ABSENT BY NAME. Handled before the missing/hash logic below
                // because the SOURCE (config-server) is a server-only folder the user never has on
                // disk — the normal path would wrongly plan a Download of config-server onto the user.
                // No hash, no baseline: the seed is memory-less (a file the user deleted reappears).
                if (rule == SyncFolderRule.SeedIfMissingByName)
                {
                    string targetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (targetRel == null)
                    {
                        continue; // no file remainder after the prefix — nothing to seed
                    }

                    string targetLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, targetRel);
                    if (!File.Exists(targetLocal))
                    {
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = file.path,       // download source (config-server/<rel>)
                            SeedTargetRelative = targetRel, // write destination (config/<rel>)
                            Kind = SyncActionKind.SeedCopy,
                            Rule = rule,
                            ServerHash = file.hash,
                            Reason = "seed (target missing by name)",
                        });
                    }
                    // target present (any content, any hash) -> no-op: seed never overwrites.
                    continue;
                }

                string localPath = SyncPathUtil.ToLocalPath(_options.GameRoot, file.path);

                if (!File.Exists(localPath))
                {
                    AddDownload(plan, file, rule, "missing");
                    continue;
                }

                string localHash = await Task.Run(() => SyncPathUtil.ComputeMd5(localPath), cancellationToken);

                if (string.Equals(localHash, file.hash, StringComparison.OrdinalIgnoreCase))
                {
                    // CC7: safe baseline seeding — local == server by definition.
                    plan.UpToDate.Add(new KeyValuePair<string, string>(normalized, localHash));
                    continue;
                }

                bool hasBaseline = _baseline.TryGetHash(normalized, out string baselineHash);
                bool matchesBaseline = hasBaseline && string.Equals(localHash, baselineHash, StringComparison.OrdinalIgnoreCase);

                if (rule == SyncFolderRule.PreserveDivergent)
                {
                    if (matchesBaseline)
                    {
                        // R1.3: untouched since last sync, server evolved → update.
                        AddDownload(plan, file, rule, "outdated (equals baseline)");
                    }
                    else
                    {
                        // R1.4 (customized) / R1.5 (first run without baseline — conservative).
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = file.path,
                            Kind = SyncActionKind.PreserveCustomized,
                            Rule = rule,
                            ServerHash = file.hash,
                            Reason = hasBaseline ? "customized (differs from baseline)" : "no baseline (first run, treated as customized)",
                        });
                    }
                }
                else if (_options.DevMode && !matchesBaseline)
                {
                    // R5.1: never revert local dev builds while Dev Mode is on.
                    plan.Actions.Add(new SyncAction
                    {
                        RelativePath = file.path,
                        Kind = SyncActionKind.PreserveDevMode,
                        Rule = rule,
                        ServerHash = file.hash,
                        Reason = "Dev Mode: local hash differs from server and baseline",
                    });
                    plan.Warnings.Add($"Dev Mode: preservado {file.path} (difere do servidor e do baseline)");
                }
                else
                {
                    AddDownload(plan, file, rule, "outdated");
                }
            }

            ScanExtras(plan, manifestPaths, cancellationToken);

            return plan;
        }

        /// <summary>
        /// Scans mirror-rule folders and Default managedPaths for local files absent from the
        /// manifest. Per-file rule resolution (longest prefix) decides delete vs move; the
        /// "handled" set prevents double-processing when roots nest.
        /// </summary>
        private void ScanExtras(SyncPlan plan, HashSet<string> manifestPaths, CancellationToken cancellationToken)
        {
            var scanRoots = new List<string>();

            foreach (var mirror in _resolver.MirrorPrefixes)
            {
                scanRoots.Add(mirror.Key);
            }

            foreach (var managedPath in _options.ManagedPaths ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(managedPath))
                {
                    scanRoots.Add(SyncPathUtil.Normalize(managedPath));
                }
            }

            var handled = new HashSet<string>(StringComparer.Ordinal);

            foreach (var rootPrefix in scanRoots.Distinct())
            {
                cancellationToken.ThrowIfCancellationRequested();

                string rootDir = SyncPathUtil.ToLocalPath(_options.GameRoot, rootPrefix);
                if (!Directory.Exists(rootDir)) continue;

                // Use the on-disk casing so extras (and the -disabled target folders derived
                // from them) keep the user-visible casing instead of the normalized lower-case.
                rootDir = SyncPathUtil.ToLocalPath(_options.GameRoot, ResolveOnDiskCasing(rootPrefix));

                foreach (var localFile in Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relative = Path.GetRelativePath(_options.GameRoot, localFile).Replace('\\', '/');
                    string normalized = SyncPathUtil.Normalize(relative);

                    if (!handled.Add(normalized)) continue;
                    if (manifestPaths.Contains(normalized)) continue;
                    if (IsIgnored(normalized)) continue;
                    if (IsExcludedFromCleanup(normalized)) continue;
                    if (_protectedNormalized.Contains(normalized)) continue;
                    if (SyncPathUtil.ContainsDisabledSegment(normalized)) continue; // R3.4

                    var rule = _resolver.Resolve(normalized, out string matchedPrefix);

                    if (rule == SyncFolderRule.PreserveDivergent || rule == SyncFolderRule.SeedIfMissingByName)
                    {
                        // Extras in config / config-server folders are never touched (neither is a
                        // mirror). Item 017: seeded files live under 'config' (preserve-divergent) and
                        // are never manifest entries, so they must survive any managedPaths overlap.
                        continue;
                    }

                    if (_options.DevMode)
                    {
                        // R5.2: extras under Dev Mode are likely local dev builds — preserve + warn.
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = relative,
                            Kind = SyncActionKind.PreserveDevMode,
                            Rule = rule,
                            Reason = "Dev Mode: extra local preservado",
                        });
                        plan.Warnings.Add($"Dev Mode: extra preservado {relative}");
                        continue;
                    }

                    // Item 023 (Frente A / RN-1..RN-3): coop-safe allowlist. A coop-essential client
                    // plugin (Fika family) present locally but absent from the manifest would be
                    // quarantined to <prefix>-disabled/ below, silently breaking every non-host
                    // client's ability to join the host's raid. Preserve it and WARN (never silent).
                    // Placed AFTER manifestPaths (:223, RN-2 — Fika in the manifest downloads/updates
                    // normally) and AFTER the Dev Mode block (:239 — Dev Mode already preserves it as
                    // an extra, no double action / no coop-safe warning, CA-A5); gated to
                    // MirrorMoveDisabled so non-Fika extras keep being cleaned (CA-A3).
                    if (rule == SyncFolderRule.MirrorMoveDisabled
                        && SyncCoopSafe.IsCoopEssentialPlugin(normalized))
                    {
                        plan.Warnings.Add($"coop-safe: preservado plugin essencial fora do manifesto: {relative}");
                        continue; // nunca vira MoveToDisabled
                    }

                    if (rule == SyncFolderRule.MirrorMoveDisabled)
                    {
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = relative,
                            Kind = SyncActionKind.MoveToDisabled,
                            Rule = rule,
                            MoveTargetRelative = BuildDisabledTarget(relative, matchedPrefix),
                            Reason = "removed from server mirror",
                        });
                    }
                    else
                    {
                        // MirrorDelete (R2.2) or Default extra inside managedPaths (R4.2).
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = relative,
                            Kind = SyncActionKind.DeleteExtra,
                            Rule = rule,
                            Reason = rule == SyncFolderRule.MirrorDelete ? "removed from server mirror" : "extra in managed path",
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Resolves the on-disk casing of a normalized prefix, segment by segment
        /// (Windows enumeration is case-insensitive; unmatched segments keep the normalized form).
        /// </summary>
        private string ResolveOnDiskCasing(string normalizedPrefix)
        {
            string current = _options.GameRoot;
            var resolved = new List<string>();

            foreach (var segment in normalizedPrefix.Split('/'))
            {
                string actual = segment;

                try
                {
                    if (Directory.Exists(current))
                    {
                        var matches = Directory.GetFileSystemEntries(current, segment);
                        if (matches.Length > 0)
                        {
                            actual = Path.GetFileName(matches[0]);
                        }
                    }
                }
                catch
                {
                    // best effort — fall back to the normalized segment
                }

                resolved.Add(actual);
                current = Path.Combine(current, actual);
            }

            return string.Join("/", resolved);
        }

        /// <summary>"BepInEx/plugins/Sub/X.dll" (prefix "bepinex/plugins") → "BepInEx/plugins-disabled/Sub/X.dll".</summary>
        private static string BuildDisabledTarget(string relative, string matchedPrefix)
        {
            string prefixOriginalCase = relative.Substring(0, matchedPrefix.Length);
            string remainder = relative.Substring(matchedPrefix.Length).TrimStart('/');
            return prefixOriginalCase + "-disabled/" + remainder;
        }

        private void AddDownload(SyncPlan plan, ManifestFile file, SyncFolderRule rule, string reason)
        {
            plan.Actions.Add(new SyncAction
            {
                RelativePath = file.path,
                Kind = SyncActionKind.Download,
                Rule = rule,
                ServerHash = file.hash,
                Reason = reason,
            });
        }

        private bool IsIgnored(string normalizedPath)
        {
            foreach (var ignored in _ignoredNormalized)
            {
                // Legacy semantics: substring match (see ProfileViewModel/ModUpdateViewModel).
                if (normalizedPath.Contains(ignored)) return true;
            }

            return false;
        }

        private bool IsExcludedFromCleanup(string normalizedPath)
        {
            foreach (var excluded in _excludeNormalized)
            {
                if (SyncPathUtil.IsUnderPrefix(normalizedPath, excluded)) return true;
            }

            return false;
        }
    }
}
