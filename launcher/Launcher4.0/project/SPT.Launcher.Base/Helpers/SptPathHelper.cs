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
                
                // Se existe uma subpasta chamada "SPT" e dentro dela tem a pasta "SPT_Data",
                // assumimos que os arquivos do SPT estão lá dentro.
                if (Directory.Exists(sptSubDir) && Directory.Exists(Path.Combine(sptSubDir, "SPT_Data")))
                {
                    return sptSubDir;
                }
                
                return baseDir;
            }
        }
    }
}
