using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 021 — coverage for the pure, injectable optional-download core
    /// (<see cref="OptionalGroupApplier.Apply"/>) and the result DTO (<see cref="OptionalOpResult"/>).
    /// Proves: all-ok, a single failure counted+continued+not-rethrown, hash-skip (downloader NOT
    /// called), path traversal rejected as a failure, and the success/failure decision of the DTO
    /// (CA-021.4/5/8/10, RN-2/3). Uses a temp root (WriteAtomic/File.Exists touch disk) and a fake
    /// downloader (no network/exe).
    /// </summary>
    public sealed class OptionalGroupApplierTests : IDisposable
    {
        private readonly string _root;

        public OptionalGroupApplierTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "spt-optional-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        private static byte[] Bytes(string s) => Encoding.UTF8.GetBytes(s);

        private static OptionalGroupApplier.Entry Entry(string path, string hash = null)
            => new OptionalGroupApplier.Entry(path, path, hash);

        [Fact]
        public void All_files_download_and_write_ok()
        {
            var files = new[]
            {
                Entry("BepInEx/plugins/a.dll"),
                Entry("BepInEx/plugins/sub/b.dll"),
            };
            var calls = new List<string>();

            var result = OptionalGroupApplier.Apply(
                _root, files,
                key => { calls.Add(key); return Bytes("payload:" + key); });

            Assert.True(result.AllOk);
            Assert.Equal(2, result.Ok);
            Assert.Equal(0, result.Failed);
            Assert.Equal(2, result.Total);
            Assert.Equal(2, calls.Count);
            Assert.True(File.Exists(Path.Combine(_root, "BepInEx", "plugins", "a.dll")));
            Assert.True(File.Exists(Path.Combine(_root, "BepInEx", "plugins", "sub", "b.dll")));
            // No temp residue after atomic writes.
            Assert.Empty(Directory.GetFiles(_root, "*" + SyncFileOps.TempSuffix, SearchOption.AllDirectories));
        }

        [Fact]
        public void One_download_that_throws_is_counted_and_the_rest_continue_without_rethrow()
        {
            var files = new[]
            {
                Entry("ok1.dll"),
                Entry("boom.dll"),
                Entry("ok2.dll"),
            };
            var errors = new List<string>();

            var result = OptionalGroupApplier.Apply(
                _root, files,
                key =>
                {
                    if (key == "boom.dll") throw new IOException("network down");
                    return Bytes(key);
                },
                onError: (path, ex) => errors.Add(path));

            Assert.False(result.AllOk);
            Assert.Equal(2, result.Ok);          // the two good files still landed
            Assert.Equal(1, result.Failed);
            Assert.Equal(3, result.Total);
            Assert.Contains("boom.dll", result.FailedPaths);
            Assert.Equal(new[] { "boom.dll" }, errors);
            // The two good files exist; the failed one does not.
            Assert.True(File.Exists(Path.Combine(_root, "ok1.dll")));
            Assert.True(File.Exists(Path.Combine(_root, "ok2.dll")));
            Assert.False(File.Exists(Path.Combine(_root, "boom.dll")));
        }

        [Fact]
        public void File_present_with_matching_hash_is_skipped_and_not_downloaded() // RN-3
        {
            const string content = "already-here";
            string dest = Path.Combine(_root, "present.dll");
            File.WriteAllBytes(dest, Bytes(content));
            string realHash = SyncPathUtil.ComputeMd5(dest);

            var calls = new List<string>();
            var result = OptionalGroupApplier.Apply(
                _root,
                new[] { Entry("present.dll", realHash) },
                key => { calls.Add(key); return Bytes("SHOULD-NOT-RUN"); });

            Assert.True(result.AllOk);
            Assert.Equal(1, result.Skipped);
            Assert.Equal(0, result.Ok);
            Assert.Equal(0, result.Failed);
            Assert.Empty(calls); // downloader never invoked for a hash match
            Assert.Equal(content, File.ReadAllText(dest)); // untouched
        }

        [Fact]
        public void File_present_with_different_hash_is_redownloaded()
        {
            string dest = Path.Combine(_root, "stale.dll");
            File.WriteAllBytes(dest, Bytes("old"));

            var result = OptionalGroupApplier.Apply(
                _root,
                new[] { Entry("stale.dll", "00000000000000000000000000000000") },
                key => Bytes("fresh"));

            Assert.True(result.AllOk);
            Assert.Equal(1, result.Ok);
            Assert.Equal(0, result.Skipped);
            Assert.Equal("fresh", File.ReadAllText(dest));
        }

        [Theory]
        [InlineData("../../evil.dll")]
        [InlineData("../sibling/x.dll")]
        public void Path_that_escapes_the_root_is_rejected_as_a_failure_not_written(string relPath) // CA-021.8
        {
            bool downloaderCalled = false;

            var result = OptionalGroupApplier.Apply(
                _root,
                new[] { Entry(relPath) },
                key => { downloaderCalled = true; return Bytes("x"); });

            Assert.False(result.AllOk);
            Assert.Equal(1, result.Failed);
            Assert.Contains(relPath, result.FailedPaths);
            Assert.False(downloaderCalled); // guard rejects BEFORE any download/write
        }

        [Fact]
        public void Empty_group_is_all_ok_with_nothing_done()
        {
            var result = OptionalGroupApplier.Apply(_root, Array.Empty<OptionalGroupApplier.Entry>(), key => Bytes("x"));

            Assert.True(result.AllOk);
            Assert.False(result.TotalFailure);
            Assert.Equal(0, result.Total);
        }

        [Fact]
        public void Result_total_failure_when_every_file_fails() // D-021.A precondition
        {
            var result = OptionalGroupApplier.Apply(
                _root,
                new[] { Entry("a.dll"), Entry("b.dll") },
                key => throw new IOException("down"));

            Assert.Equal(2, result.Failed);
            Assert.False(result.AnyApplied);
            Assert.True(result.TotalFailure);   // → toggle reverts to false
            Assert.False(result.AllOk);
        }

        [Fact]
        public void Result_partial_failure_is_not_total_failure()
        {
            var result = OptionalGroupApplier.Apply(
                _root,
                new[] { Entry("ok.dll"), Entry("bad.dll") },
                key => key == "bad.dll" ? throw new IOException("down") : Bytes(key));

            Assert.Equal(1, result.Ok);
            Assert.Equal(1, result.Failed);
            Assert.True(result.AnyApplied);
            Assert.False(result.TotalFailure); // partial → stays enabled, error shown (RN-2)
            Assert.False(result.AllOk);
        }

        [Fact]
        public void GroupResolved_false_is_a_total_failure() // CC-5
        {
            var result = new OptionalOpResult { GroupResolved = false };

            Assert.False(result.AllOk);
            Assert.True(result.TotalFailure);
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, recursive: true); }
            catch { /* best effort */ }
        }
    }
}
