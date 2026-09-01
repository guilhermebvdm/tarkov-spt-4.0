using System;
using EFT;
using EFT.Game.Spawning;

namespace TRLDynamicSpawn.Helpers
{
    public static class SpawnPointHelper
    {
        public static bool IsSniperRole(WildSpawnType role)
        {
            if (role == WildSpawnType.marksman || role == WildSpawnType.bossBoarSniper) return true;
            string roleStr = role.ToString().ToLower();
            return roleStr.Contains("marksman") || roleStr.Contains("sniper") || roleStr.Contains("snipe");
        }

        public static bool IsSniperZone(BotZone zone)
        {
            if (zone == null) return false;
            if (zone.SnipeZone) return true;
            string name = zone.NameZone ?? "";
            return name.IndexOf("snip", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsSniperSpawnPoint(ISpawnPoint sp, BotZone zone = null)
        {
            if (sp == null) return false;

            if (sp is CustomSpawnPoint custom && custom.IsSnipeZone) return true;

            try
            {
                if (sp.IsSnipeZone) return true;
            }
            catch { }

            try
            {
                string catStr = sp.Categories.ToString();
                if (catStr.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0 || catStr.IndexOf("sniper", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            catch { }

            if (!string.IsNullOrEmpty(sp.Name) && (sp.Name.IndexOf("snip", StringComparison.OrdinalIgnoreCase) >= 0 || sp.Name.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            if (!string.IsNullOrEmpty(sp.Infiltration) && (sp.Infiltration.IndexOf("snip", StringComparison.OrdinalIgnoreCase) >= 0 || sp.Infiltration.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            if (!string.IsNullOrEmpty(sp.Id) && (sp.Id.IndexOf("snip", StringComparison.OrdinalIgnoreCase) >= 0 || sp.Id.IndexOf("marksman", StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            if (zone != null && IsSniperZone(zone)) return true;

            return false;
        }
    }
}
