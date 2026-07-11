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

            // Pre-pass: destinos do config-force. O operador tende a COPIAR a cfg para config-force/ e
            // esquecer de removê-la de config/ — aí o mesmo alvo config/<rel> receberia DUAS ações
            // (um Download da entrada config/ + o ForceCopy), com o vencedor decidido pela ORDEM do
            // manifesto (arbitrária) e o baseline gravado com o hash da versão errada (o arquivo viraria
            // "customizado" para sempre e nunca mais receberia update). O FORCE VENCE, explicitamente.
            var forceTargets = BuildForceTargets(filesToCheck);

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

                // Colisão config/ × config-force/: esta entrada do manifesto grava no MESMO destino de
                // um force. Ignorada — a versão forçada é a que vale (resultado independente da ordem).
                if (rule != SyncFolderRule.ForceToConfig && forceTargets.Contains(normalized))
                {
                    plan.Warnings.Add($"{file.path} também está em config-force — a versão FORÇADA vence (entrada do manifesto ignorada)");
                    continue;
                }

                // config-server sync (item 017 + seed-and-mirror). Handled before the missing/hash
                // logic below because the SOURCE (config-server) is a server folder the user never
                // has under 'config' — the normal path would wrongly plan a Download onto the user.
                //  SEED: copy config-server/<rel> -> config/<rel> ONLY if absent by name (memory-less;
                //        never overwrites/deletes the user-owned 'config').
                //  MIRROR (seed-and-mirror only): keep config-server/<rel> itself a replica —
                //        download the latest whenever missing or hash-divergent (overwrites user edits).
                //        Extras are NOT deleted: SeedAndMirror is NOT a MirrorPrefix and is skipped in
                //        ScanExtras (conservative, ref CR-01-03; delete stays opt-in).
                if (rule == SyncFolderRule.SeedIfMissingByName || rule == SyncFolderRule.SeedAndMirror)
                {
                    // -- SEED into config/<rel> (both rules) --
                    string targetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (targetRel != null)
                    {
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
                    }

                    // -- MIRROR config-server/<rel> itself (seed-and-mirror only): sempre a última versão --
                    if (rule == SyncFolderRule.SeedAndMirror)
                    {
                        string mirrorLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, file.path);
                        if (!File.Exists(mirrorLocal))
                        {
                            AddDownload(plan, file, rule, "mirror (config-server ausente)");
                        }
                        else
                        {
                            string mirrorHash = await Task.Run(() => SyncPathUtil.ComputeMd5(mirrorLocal), cancellationToken);
                            if (string.Equals(mirrorHash, file.hash, StringComparison.OrdinalIgnoreCase))
                            {
                                plan.UpToDate.Add(new KeyValuePair<string, string>(normalized, mirrorHash));
                            }
                            else
                            {
                                // Réplica exata: sobrescreve SEMPRE (mesmo se o usuário editou o config-server).
                                AddDownload(plan, file, rule, "mirror (config-server desatualizado)");
                            }
                        }
                    }

                    continue;
                }

                // config-force → config: FORÇA. Sobrescreve config/<rel> do usuário SEMPRE que o conteúdo
                // divergir (ou faltar) — ignora customização de propósito (é o canal "essa config vai pra
                // todo mundo"). Comparação direta: hash local do ALVO vs hash do manifesto da FONTE (sem
                // baseline). A pasta config-force NUNCA é materializada no cliente (só a fonte do download).
                if (rule == SyncFolderRule.ForceToConfig)
                {
                    string forceTargetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (forceTargetRel == null)
                    {
                        continue; // sem remainder após o prefixo — nada a forçar
                    }

                    // Guard de self-target: prefixo SEM o sufixo "-force" (misconfig do operador, ex.:
                    // folderRules com "BepInEx/config": "force-to-config"). O alvo derivado seria o PRÓPRIO
                    // arquivo → materializaria a pasta-fonte no cliente e quebraria a invariante. Pula e avisa.
                    if (string.Equals(SyncPathUtil.Normalize(forceTargetRel), normalized, StringComparison.Ordinal))
                    {
                        plan.Warnings.Add($"force-to-config em '{matchedPrefix}' não tem o sufixo '-force' (alvo = fonte) — ignorado: {file.path}");
                        continue;
                    }

                    string forceTargetLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, forceTargetRel);
                    string forceReason = null;

                    // CR-01 (TOCTOU): o destino do backup é derivado SEMPRE, mesmo quando o alvo não
                    // existe agora. Quem decide se há o que preservar é o ENGINE, no apply — o arquivo
                    // pode SURGIR entre o plano e a escrita (BepInEx regenerando o default, o jogador
                    // restaurando um backup) e, sem isso, seria sobrescrito sem backup nenhum.
                    string forceBackupRel = SyncPathUtil.DeriveDisabledBackup(forceTargetRel, matchedPrefix);

                    if (!File.Exists(forceTargetLocal))
                    {
                        forceReason = "force (ausente no config)";
                    }
                    else
                    {
                        string targetHash = await Task.Run(() => SyncPathUtil.ComputeMd5(forceTargetLocal), cancellationToken);
                        if (!string.Equals(targetHash, file.hash, StringComparison.OrdinalIgnoreCase))
                        {
                            // R5.1: Dev Mode é o escape hatch "não reverta minha edição local" — nem o
                            // force sobrescreve (o jogador comum não tem Dev Mode; o dev tunando uma
                            // config não pode perdê-la a cada sync). Só protege o que JÁ EXISTE local.
                            if (_options.DevMode)
                            {
                                plan.Actions.Add(new SyncAction
                                {
                                    RelativePath = forceTargetRel,
                                    Kind = SyncActionKind.PreserveDevMode,
                                    Rule = rule,
                                    ServerHash = file.hash,
                                    Reason = "Dev Mode: config forçada preservada (difere do servidor)",
                                });
                                plan.Warnings.Add($"Dev Mode: config FORÇADA não aplicada em {forceTargetRel} (edição local preservada)");
                                continue;
                            }

                            // NÃO-DESTRUTIVO: a config do jogador é preservada em <pasta>-disabled/ antes
                            // de ser trocada. Nada é excluído em silêncio — ele recupera de lá se quiser.
                            forceReason = "force (divergente — a sua config vai p/ config-disabled e é substituída)";
                        }
                        // hash igual → o usuário já está com a config forçada → no-op
                    }

                    if (forceReason != null)
                    {
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = file.path,             // fonte do download (config-force/<rel>)
                            SeedTargetRelative = forceTargetRel,  // destino da escrita (config/<rel>)
                            MoveTargetRelative = forceBackupRel,  // backup do que existia (config-disabled/<rel>), ou null
                            Kind = SyncActionKind.ForceCopy,
                            Rule = rule,
                            ServerHash = file.hash,
                            Reason = forceReason,
                        });
                    }

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

                    if (rule == SyncFolderRule.PreserveDivergent
                        || rule == SyncFolderRule.SeedIfMissingByName
                        || rule == SyncFolderRule.SeedAndMirror
                        || rule == SyncFolderRule.ForceToConfig)
                    {
                        // Extras in config / config-server folders are never touched (config-server
                        // overwrites to latest but doesn't delete extras — conservative, ref CR-01-03).
                        // Item 017: seeded files live under 'config' (preserve-divergent) and
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

        /// <summary>
        /// Destinos (normalizados) de todas as entradas config-force do manifesto. Usado para resolver a
        /// colisão config/ × config-force/ de forma DETERMINÍSTICA (o force vence), em vez de deixar o
        /// vencedor por conta da ordem do manifesto. Ignora o caso self-target (prefixo sem "-force"),
        /// que o próprio bloco do force descarta com aviso.
        /// </summary>
        private HashSet<string> BuildForceTargets(IReadOnlyList<ManifestFile> files)
        {
            var targets = new HashSet<string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                string normalized = SyncPathUtil.Normalize(file.path);

                if (_resolver.Resolve(normalized, out string matchedPrefix) != SyncFolderRule.ForceToConfig)
                {
                    continue;
                }

                string targetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                if (string.IsNullOrEmpty(targetRel)) continue;

                string normalizedTarget = SyncPathUtil.Normalize(targetRel);
                if (string.Equals(normalizedTarget, normalized, StringComparison.Ordinal)) continue; // self-target

                targets.Add(normalizedTarget);
            }

            return targets;
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
