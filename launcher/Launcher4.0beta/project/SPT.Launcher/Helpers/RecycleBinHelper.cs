using System;
using System.IO;

namespace SPT.Launcher.Helpers
{
    /// <summary>
    /// Item 019: shared recoverable-deletion strategy, extracted from
    /// ProfileViewModel.DeleteToRecycleBin so the sync engine, the deleteFiles loop and
    /// OptionalModsHelper all delete the same way. Sends the file to the recycle bin
    /// (recoverable) and only falls back to a permanent <see cref="File.Delete(string)"/>
    /// when the recycle bin is unavailable (non-Windows).
    /// </summary>
    public static class RecycleBinHelper
    {
        /// <summary>Sends <paramref name="path"/> to the recycle bin, with a permanent-delete fallback off-Windows.</summary>
        public static void Delete(string path)
        {
            try
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path,
                    Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                    Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            }
            catch (PlatformNotSupportedException)
            {
                File.Delete(path);
            }
        }
    }
}
