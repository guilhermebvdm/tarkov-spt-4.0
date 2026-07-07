using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SPT.Launcher.Models.Launcher;
using SPT.Launcher.Sync;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Real temp-directory fixture: fake game root + baseline file + in-memory downloader
    /// (the engine is exercised without HTTP, per spec-tech 007).
    /// </summary>
    public sealed class SyncTestFixture : IDisposable
    {
        public string Root { get; }

        public string BaselinePath { get; }

        public string ReportPath { get; }

        /// <summary>relative manifest path -> downloadable content.</summary>
        public Dictionary<string, byte[]> ServerContent { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> DownloadedPaths { get; } = new();

        public SyncTestFixture()
        {
            Root = Path.Combine(Path.GetTempPath(), "spt-sync-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Root);
            BaselinePath = Path.Combine(Root, "SPT", "user", "launcher", "sync-state.json");
            ReportPath = Path.Combine(Root, "SPT", "user", "launcher", "last-update.json");
        }

        public SyncDownloader Downloader => (path, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            DownloadedPaths.Add(path);

            if (!ServerContent.TryGetValue(path, out var data))
            {
                throw new InvalidOperationException($"no server content registered for '{path}'");
            }

            return Task.FromResult(data);
        };

        public string WriteLocal(string relativePath, string content)
        {
            string fullPath = Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.WriteAllText(fullPath, content);
            return fullPath;
        }

        public string LocalPath(string relativePath) =>
            Path.Combine(Root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        public bool LocalExists(string relativePath) => File.Exists(LocalPath(relativePath));

        public string ReadLocal(string relativePath) => File.ReadAllText(LocalPath(relativePath));

        public static string Md5Of(string content)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public ManifestFile Entry(string path, string content, bool optional = false, string group = null)
        {
            ServerContent[path] = System.Text.Encoding.UTF8.GetBytes(content);

            return new ManifestFile
            {
                path = path,
                hash = Md5Of(content),
                size = content.Length,
                optional = optional,
                optionalGroup = group,
            };
        }

        public SyncBaseline LoadBaseline() => SyncBaseline.Load(BaselinePath);

        /// <summary>
        /// ref: CR-01-03 — mirror-delete saiu do fallback; testes de config-server ativam a regra
        /// como o server real faria: via folderRules explícito no manifesto.
        /// </summary>
        public static SyncRuleResolver ResolverWithConfigServerMirror() =>
            new SyncRuleResolver(new Dictionary<string, string> { ["config-server"] = "mirror-delete" });

        public SyncPlannerOptions Options(bool devMode = false) => new()
        {
            GameRoot = Root,
            DevMode = devMode,
        };

        public async Task<(SyncPlan Plan, SyncResult Result, SyncBaseline Baseline)> PlanAndRunAsync(
            IReadOnlyList<ManifestFile> manifest,
            SyncPlannerOptions options = null,
            SyncRuleResolver resolver = null,
            CancellationToken ct = default)
        {
            resolver ??= new SyncRuleResolver();
            options ??= Options();
            var baseline = LoadBaseline();

            var planner = new SyncPlanner(resolver, baseline, options);
            var plan = await planner.BuildPlanAsync(manifest, null, ct);

            var engine = new SyncEngine(Root, baseline, Downloader);
            var result = await engine.ExecuteAsync(plan, ReportPath, null, ct);

            return (plan, result, baseline);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch
            {
                // best effort cleanup
            }
        }
    }
}
