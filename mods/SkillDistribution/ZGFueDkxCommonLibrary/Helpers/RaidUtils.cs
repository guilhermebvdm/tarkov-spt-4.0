using Comfort.Common;
using EFT;

namespace ZGFueDkx.ZGCLib.helpers
{
    internal static class RaidUtils
    {
        public static bool IsInRaid()
        {
            return Singleton<AbstractGame>.Instance?.InRaid is true;
        }
    }
}
