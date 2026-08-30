using System;
using System.IO;

namespace SPT.Launcher.Base.Helpers
{
    public static class SptPathHelper
    {
        public static string SptRootPath
        {
            get
            {
                string baseDir = AppContext.BaseDirectory;
                string sptSubDir = Path.Combine(baseDir, "SPT");
                if (!Directory.Exists(sptSubDir))
                {
                    try { Directory.CreateDirectory(sptSubDir); } catch { }
                }
                return sptSubDir;
            }
        }
    }
}
