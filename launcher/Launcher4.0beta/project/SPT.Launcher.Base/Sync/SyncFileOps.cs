using System.IO;

namespace SPT.Launcher.Sync
{
    /// <summary>
    /// Pure atomic file-write primitive, single source of truth shared by the sync engine and the
    /// legacy FS paths (item 019). Writes to "&lt;dest&gt;.sync-tmp" in the same directory (same
    /// volume) then atomically moves over the destination with rollback (delete temp) on failure.
    /// Does NOT guard the path — the caller must resolve it under the game root first
    /// (see <see cref="SyncPathUtil.ResolveUnderRoot"/>).
    /// </summary>
    public static class SyncFileOps
    {
        /// <summary>Suffix of the same-volume temp file used during an atomic write.</summary>
        public const string TempSuffix = ".sync-tmp";

        /// <summary>E3: write to "&lt;dest&gt;.sync-tmp" in the same directory, then atomic move.</summary>
        public static void WriteAtomic(string absolutePath, byte[] data)
        {
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = absolutePath + TempSuffix;

            try
            {
                File.WriteAllBytes(tempPath, data);
                File.Move(tempPath, absolutePath, overwrite: true);
            }
            catch
            {
                try
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
                catch
                {
                    // best effort — never mask the original exception
                }

                throw;
            }
        }
    }
}
