using System;
using System.IO;
using System.Text;
using SPT.Launcher.Sync;
using Xunit;

namespace SPT.Launcher.Tests.Sync
{
    /// <summary>
    /// Item 019 — coverage for the shared atomic-write primitive <see cref="SyncFileOps.WriteAtomic"/>
    /// (CA-5). Same primitive the engine and the optional-mods download now share.
    /// </summary>
    public sealed class SyncFileOpsTests : IDisposable
    {
        private readonly string _root;

        public SyncFileOpsTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "spt-fileops-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [Fact]
        public void Creates_missing_directory_and_writes_content() // CA-5
        {
            string dest = Path.Combine(_root, "BepInEx", "plugins", "new.dll");
            byte[] data = Encoding.UTF8.GetBytes("brand-new");

            SyncFileOps.WriteAtomic(dest, data);

            Assert.True(File.Exists(dest));
            Assert.Equal("brand-new", File.ReadAllText(dest));
            Assert.Empty(Directory.GetFiles(_root, "*" + SyncFileOps.TempSuffix, SearchOption.AllDirectories));
        }

        [Fact]
        public void Overwrites_existing_file_and_leaves_no_temp() // CA-5
        {
            string dest = Path.Combine(_root, "config.json");
            File.WriteAllText(dest, "old-content");

            SyncFileOps.WriteAtomic(dest, Encoding.UTF8.GetBytes("new-content"));

            Assert.Equal("new-content", File.ReadAllText(dest));
            // No partial/truncated state and no .sync-tmp residue after success.
            Assert.Empty(Directory.GetFiles(_root, "*" + SyncFileOps.TempSuffix, SearchOption.AllDirectories));
        }

        [Fact]
        public void Rollback_on_move_failure_leaves_no_temp_and_keeps_original()
        {
            // Force File.Move(temp, dest) to fail by making `dest` an existing DIRECTORY:
            // a file cannot overwrite a directory, so Move throws and rollback must delete the temp.
            string dest = Path.Combine(_root, "collision");
            Directory.CreateDirectory(dest);

            Assert.ThrowsAny<Exception>(
                () => SyncFileOps.WriteAtomic(dest, Encoding.UTF8.GetBytes("payload")));

            // The directory (the "original") is untouched and the temp file was cleaned up.
            Assert.True(Directory.Exists(dest));
            Assert.Empty(Directory.GetFiles(_root, "*" + SyncFileOps.TempSuffix, SearchOption.AllDirectories));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch
            {
                // best effort cleanup
            }
        }
    }
}
