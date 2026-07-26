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

            // Item 030: mod opcional DESLIGADO sai do check (não baixa); seus arquivos que já existem no
            // disco são quarentenados por QuarantineDisabledOptionalMods (com as guardas do ScanExtras).
            // O filtro PERMANECE: sem ele o arquivo de mod desligado voltaria a Download e criaria loop
            // download↔quarentena a cada sync.
            var filesToCheck = manifestFiles
                .Where(f => !f.optional || _options.IsOptionalModEnabled(OptionalIdOf(f)))
                .ToList();

            // Pre-pass: destinos do config-force. O operador tende a COPIAR a cfg para config-force/ e
            // esquecer de removê-la de config/ — aí o mesmo alvo config/<rel> receberia DUAS ações
            // (um Download da entrada config/ + o ForceCopy), com o vencedor decidido pela ORDEM do
            // manifesto (arbitrária) e o baseline gravado com o hash da versão errada (o arquivo viraria
            // "customizado" para sempre e nunca mais receberia update). O FORCE VENCE, explicitamente.
            var forceTargets = BuildForceTargets(filesToCheck);

            // Item 030 (D-1): alvos config/<rel> de itens de performance LIGADOS (performance vence force
            // e config); e os alvos que TÊM versão base em config/ ou config-force/ (usado ao desligar
            // performance: se há base, o fluxo normal restaura; se não, o arquivo sai para a quarentena).
            var performanceTargets = BuildPerformanceTargets(filesToCheck);
            var baseSourcePaths = BuildBaseSourcePaths(filesToCheck);

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

                // Precedência performance > force > config (D-1). O canal de performance (branch dedicado
                // abaixo) nunca é suprimido aqui.
                if (rule == SyncFolderRule.ForceToConfig)
                {
                    // Force perde para uma config de performance LIGADA do mesmo alvo (RN-2 — registra e avisa).
                    string forceDest = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (forceDest != null && performanceTargets.Contains(SyncPathUtil.Normalize(forceDest)))
                    {
                        plan.Warnings.Add($"{file.path}: config de performance ligada sobrepõe esta config forçada (config-force ignorado)");
                        plan.InfoEntries.Add(new SyncReportEntry
                        {
                            path = forceDest,
                            action = "performance-suppressed-force",
                            detail = file.path,
                            timestamp = DateTime.UtcNow,
                        });
                        continue;
                    }
                }
                else if (rule != SyncFolderRule.PerformanceToConfig
                         && (forceTargets.Contains(normalized) || performanceTargets.Contains(normalized)))
                {
                    // Entrada normal (config/ etc.) cujo alvo é governado por um force OU por performance
                    // ligada: ignorada — quem vence é o canal de maior precedência (resultado independente
                    // da ordem do manifesto).
                    plan.Warnings.Add($"{file.path} também está em config-force/config-performance — a versão de maior precedência vence (entrada ignorada)");
                    continue;
                }

                // config-server = MIRROR-REFERENCE (biblioteca de referência). Espelha config-server/<rel>
                // na pasta config-server/ do cliente: baixa a última versão se faltar OU o hash divergir
                // (restaura sempre — a pasta é pristina, o usuário não edita, só consulta/copia pra config/).
                // NUNCA escreve em config/. NÃO deleta extras (fora de MirrorPrefixes + pulado no ScanExtras).
                // Branch DEDICADO (com continue) de propósito: não herdar a preservação de Dev Mode/baseline
                // do caminho normal — a referência sempre restaura pra versão do server.
                if (rule == SyncFolderRule.MirrorReference)
                {
                    string mirrorLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, file.path);
                    if (!File.Exists(mirrorLocal))
                    {
                        AddDownload(plan, file, rule, "referência ausente");
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
                            AddDownload(plan, file, rule, "referência desatualizada — restaura a versão do server");
                        }
                    }

                    continue;
                }

                // config-server SEED opt-in (item 017, SeedIfMissingByName — NÃO é mais o default do
                // config-server; só via folderRules explícito). Copia config-server/<rel> -> config/<rel>
                // SÓ se faltar por nome (memory-less; nunca sobrescreve/deleta o 'config' do usuário).
                if (rule == SyncFolderRule.SeedIfMissingByName)
                {
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

                // Item 030: config-performance → config. Híbrido (§1.1): EXPLÍCITO ao alternar (aplica/
                // remove mesmo divergente, com quarentena); preserve-divergent nos syncs de rotina.
                if (rule == SyncFolderRule.PerformanceToConfig)
                {
                    string perfTargetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (perfTargetRel == null)
                    {
                        continue; // sem remainder após o prefixo
                    }

                    // Guard de self-target (misconfig: prefixo sem "-performance" → alvo = fonte).
                    if (string.Equals(SyncPathUtil.Normalize(perfTargetRel), normalized, StringComparison.Ordinal))
                    {
                        plan.Warnings.Add($"performance-to-config em '{matchedPrefix}' não tem o sufixo '-performance' (alvo = fonte) — ignorado: {file.path}");
                        continue;
                    }

                    string perfTargetLocal = SyncPathUtil.ToLocalPath(_options.GameRoot, perfTargetRel);
                    string perfTargetNorm = SyncPathUtil.Normalize(perfTargetRel);
                    bool enabled = _options.IsPerformanceItemEnabled(file.performanceId ?? string.Empty);
                    bool justToggled = _options.JustToggledIds.Contains(file.performanceId ?? string.Empty);
                    bool exists = File.Exists(perfTargetLocal);

                    string perfLocalHash = exists
                        ? await Task.Run(() => SyncPathUtil.ComputeMd5(perfTargetLocal), cancellationToken)
                        : null;
                    bool perfMatchesBaseline = exists
                        && _baseline.TryGetHash(perfTargetNorm, out string perfBaseHash)
                        && string.Equals(perfBaseHash, perfLocalHash, StringComparison.OrdinalIgnoreCase);

                    if (enabled)
                    {
                        if (exists && string.Equals(perfLocalHash, file.hash, StringComparison.OrdinalIgnoreCase))
                        {
                            // já aplicado — no-op (CC7: semeia baseline com local == server).
                            plan.UpToDate.Add(new KeyValuePair<string, string>(perfTargetNorm, perfLocalHash));
                            continue;
                        }

                        // Ação explícita (D-16/CC-19) avaliada ANTES do Dev Mode: o Dev Mode protege contra
                        // reversão AUTOMÁTICA (rotina), não contra um toggle que o próprio usuário clicou.
                        if (justToggled || !exists || perfMatchesBaseline)
                        {
                            plan.Actions.Add(new SyncAction
                            {
                                RelativePath = file.path,            // fonte: config-performance/<rel>
                                SeedTargetRelative = perfTargetRel,  // destino: config/<rel>
                                MoveTargetRelative = exists
                                    ? SyncPathUtil.DeriveDisabledBackup(perfTargetRel, matchedPrefix, SyncPathUtil.DisabledOrigin.PerformanceReplaced)
                                    : null,
                                Kind = SyncActionKind.PerformanceCopy,
                                Rule = rule,
                                ServerHash = file.hash,
                                Reason = justToggled
                                    ? "performance ligada (sua config anterior foi preservada na quarentena)"
                                    : (exists ? "performance (atualizada pelo servidor)" : "performance (aplicada)"),
                            });
                            continue;
                        }

                        // Rotina + Dev Mode: preserva a build local do dev.
                        if (_options.DevMode)
                        {
                            plan.Actions.Add(new SyncAction
                            {
                                RelativePath = perfTargetRel,
                                Kind = SyncActionKind.PreserveDevMode,
                                Rule = rule,
                                ServerHash = file.hash,
                                Reason = "Dev Mode: config de performance preservada (difere do servidor)",
                            });
                            continue;
                        }

                        // Rotina: customizado desde a aplicação → preserva (CA-030.3/RN-3).
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = perfTargetRel,
                            Kind = SyncActionKind.PreserveCustomized,
                            Rule = rule,
                            ServerHash = file.hash,
                            Reason = "você customizou — sua versão foi mantida",
                        });
                        continue;
                    }

                    // ---- DESLIGADO (CA-030.2b / CA-030.5) ----
                    if (!exists)
                    {
                        continue; // nada aplicado, nada a reverter
                    }

                    if (!perfMatchesBaseline)
                    {
                        // Customizado desde a aplicação: a edição do player prevalece (RN-3/CC-3).
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = perfTargetRel,
                            Kind = SyncActionKind.PreserveCustomized,
                            Rule = rule,
                            ServerHash = file.hash,
                            Reason = "você customizou — reversão da performance foi pulada",
                        });
                        continue;
                    }

                    // Intocado. Com versão base (config/ ou config-force/) → o fluxo normal a restaura nesta
                    // mesma passada. Sem base (só existe por causa da performance) → quarentena (D-8/D-20).
                    if (!baseSourcePaths.Contains(perfTargetNorm))
                    {
                        plan.Actions.Add(new SyncAction
                        {
                            RelativePath = perfTargetRel,
                            MoveTargetRelative = SyncPathUtil.DeriveDisabledBackup(perfTargetRel, matchedPrefix, SyncPathUtil.DisabledOrigin.PerformanceRemoved),
                            Kind = SyncActionKind.MoveToDisabled,
                            Rule = rule,
                            Reason = "performance desligada (arquivo sem versão base — movido para a quarentena)",
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

            // Item 030 (§2.4): mod opcional DESLIGADO cujos arquivos existem no disco → quarentena
            // explícita (o ScanExtras não serve — manifestPaths protege optional inativo dele). Reusa
            // as MESMAS guardas do ScanExtras (coop-safe, Dev Mode, protected/ignored/excluded).
            QuarantineDisabledOptionalMods(plan, manifestFiles, cancellationToken);

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
                        || rule == SyncFolderRule.MirrorReference
                        || rule == SyncFolderRule.ForceToConfig
                        || rule == SyncFolderRule.PerformanceToConfig)
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

        /// <summary>
        /// "BepInEx/plugins/Sub/X.dll" (prefix "bepinex/plugins") → "BepInEx/plugins-disabled/Sub/X.dll".
        /// Item 030 (D-14): a origem escolhe a subpasta. O default MirrorExtra mantém o formato legado
        /// (raiz) — o único caller até aqui era o ScanExtras, e mudá-lo quebraria testes em produção sem
        /// ganho. A quarentena de mod opcional desligado passa DisabledOrigin.Optional.
        /// </summary>
        private static string BuildDisabledTarget(string relative, string matchedPrefix,
            SyncPathUtil.DisabledOrigin origin = SyncPathUtil.DisabledOrigin.MirrorExtra)
        {
            string prefixOriginalCase = relative.Substring(0, matchedPrefix.Length);
            string remainder = relative.Substring(matchedPrefix.Length).TrimStart('/');
            string segment = SyncPathUtil.OriginSegment(origin);
            return segment == null
                ? prefixOriginalCase + "-disabled/" + remainder
                : prefixOriginalCase + "-disabled/" + segment + "/" + remainder;
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

        /// <summary>
        /// Item 030: alvos config/&lt;rel&gt; de itens de performance LIGADOS. Usado para a precedência
        /// (performance vence force e config). Item desligado não entra (não suprime nada).
        /// </summary>
        private HashSet<string> BuildPerformanceTargets(IReadOnlyList<ManifestFile> files)
        {
            var targets = new HashSet<string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                string normalized = SyncPathUtil.Normalize(file.path);
                if (_resolver.Resolve(normalized, out string matchedPrefix) != SyncFolderRule.PerformanceToConfig)
                {
                    continue;
                }
                if (!_options.IsPerformanceItemEnabled(file.performanceId ?? string.Empty))
                {
                    continue; // só itens ligados vencem
                }

                string targetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                if (string.IsNullOrEmpty(targetRel)) continue;

                string normalizedTarget = SyncPathUtil.Normalize(targetRel);
                if (string.Equals(normalizedTarget, normalized, StringComparison.Ordinal)) continue; // self-target

                targets.Add(normalizedTarget);
            }

            return targets;
        }

        /// <summary>
        /// Item 030: alvos config/&lt;rel&gt; que TÊM versão base — origem em config/ (PreserveDivergent) ou
        /// config-force/ (ForceToConfig). Ao desligar performance de um alvo que está aqui, o fluxo normal
        /// restaura a base nesta mesma passada; se o alvo NÃO está aqui, ele só existia por causa da
        /// performance e sai para a quarentena.
        /// </summary>
        private HashSet<string> BuildBaseSourcePaths(IReadOnlyList<ManifestFile> files)
        {
            var bases = new HashSet<string>(StringComparer.Ordinal);

            foreach (var file in files)
            {
                string normalized = SyncPathUtil.Normalize(file.path);
                var rule = _resolver.Resolve(normalized, out string matchedPrefix);

                if (rule == SyncFolderRule.PreserveDivergent)
                {
                    bases.Add(normalized); // o próprio config/<rel> é a base
                }
                else if (rule == SyncFolderRule.ForceToConfig)
                {
                    string targetRel = SyncPathUtil.DeriveSeedTarget(file.path, matchedPrefix);
                    if (!string.IsNullOrEmpty(targetRel))
                    {
                        bases.Add(SyncPathUtil.Normalize(targetRel));
                    }
                }
            }

            return bases;
        }

        /// <summary>
        /// Item 030 (§2.4): para cada arquivo de mod opcional DESLIGADO presente no disco, emite
        /// MoveToDisabled (origem Optional). Reusa as MESMAS guardas do ScanExtras — a ação explícita não
        /// pode passar por fora delas: protected/ignored/excluded, Dev Mode (CC-14: não move build local
        /// do dev) e coop-safe (um plugin Fika oferecido como opcional e desligado quebraria o join —
        /// preserva + avisa). manifestPaths continua incluindo esses paths, então o ScanExtras não duplica.
        /// </summary>
        private void QuarantineDisabledOptionalMods(SyncPlan plan, IReadOnlyList<ManifestFile> manifestFiles, CancellationToken cancellationToken)
        {
            foreach (var file in manifestFiles)
            {
                if (!file.optional) continue;
                if (_options.IsOptionalModEnabled(OptionalIdOf(file))) continue; // ligado → fica

                cancellationToken.ThrowIfCancellationRequested();

                string relForward = (file.path ?? string.Empty).Replace('\\', '/');
                string normalized = SyncPathUtil.Normalize(relForward);
                string localPath = SyncPathUtil.ToLocalPath(_options.GameRoot, relForward);
                if (!File.Exists(localPath)) continue; // nada a mover

                if (IsIgnored(normalized)) continue;
                if (IsExcludedFromCleanup(normalized)) continue;
                if (_protectedNormalized.Contains(normalized)) continue;
                if (SyncPathUtil.ContainsDisabledSegment(normalized)) continue;

                var rule = _resolver.Resolve(normalized, out string matchedPrefix);

                if (_options.DevMode)
                {
                    plan.Warnings.Add($"Dev Mode: mod opcional desligado NÃO movido para quarentena: {file.path}");
                    continue;
                }

                if (SyncCoopSafe.IsCoopEssentialPlugin(normalized))
                {
                    plan.Warnings.Add($"coop-safe: mod opcional desligado é plugin essencial — preservado (nunca movido): {file.path}");
                    continue;
                }

                plan.Actions.Add(new SyncAction
                {
                    RelativePath = relForward,
                    MoveTargetRelative = BuildDisabledTarget(relForward, matchedPrefix, SyncPathUtil.DisabledOrigin.Optional),
                    Kind = SyncActionKind.MoveToDisabled,
                    Rule = rule,
                    Reason = "mod opcional desligado (movido para quarentena)",
                });
            }
        }

        /// <summary>
        /// Item 030: id do mod opcional dono do arquivo, com fallback pro campo legado. Enquanto o servidor
        /// não migrou (ainda emite optionalGroup), o manifesto pode vir sem optionalId — sem o fallback,
        /// todo opcional viraria "id vazio" → filtrado E quarentenado (regressão). Some na Fase 3.
        /// </summary>
        private static string OptionalIdOf(ManifestFile file) =>
            !string.IsNullOrEmpty(file.optionalId) ? file.optionalId : (file.optionalGroup ?? string.Empty);

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
