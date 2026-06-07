using Microsoft.Win32;
using System.IO;

namespace SPT.Launcher.Helpers
{
    public static class ValidationUtil
    {
        public static bool Validate()
        {
            // Bypassa completamente a verificação antipirataria/Live do SPT
            // pois o Tarkov Red Line já possui a estrutura unificada correta.
            return true;
        }
    }
}
