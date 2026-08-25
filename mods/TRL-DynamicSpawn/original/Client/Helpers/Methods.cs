using Comfort.Common;
using System.Linq;
using EFT;
using EFT.Communications;
using SPT.Reflection.Utils;

namespace TRLDynamicSpawn.Helpers
{
    public class Methods
    {
        public static void DisplayMessage(string message, EFT.Communications.ENotificationIconType iconType = EFT.Communications.ENotificationIconType.Default)
        {
            NotificationManagerClass.DisplayMessageNotification(
                message, 
                EFT.Communications.ENotificationDurationType.Default, 
                EFT.Communications.ENotificationIconType.Default
            );
        }

        public static async void RefreshLocationInfo()
        {
            if (PatchConstants.BackEndSession != null)
                await PatchConstants.BackEndSession.GetLevelSettings();
            // await PatchConstants.BackEndSession.GetWeatherAndTime();
        }

        public static AddSpawnRequest GetPlayersCoordinatesAndLevel()
        {
            var position = Singleton<GameWorld>.Instance.MainPlayer.Position;
            var location = Singleton<GameWorld>.Instance.MainPlayer.Location;

            return new AddSpawnRequest
            {
                map = location,
                position = new Ixyz
                {
                    x = position.x,
                    y = position.y,
                    z = position.z,
                },
            };
        }

        public static BotZone GetRandomZone(BotSpawner botSpawner, bool allowSnipeZone = false)
        {
            if (botSpawner == null) return null;
            var zones = LocationScene.GetAllObjects<BotZone>();
            if (zones != null)
            {
                var filtered = zones.Where(z => z != null && (allowSnipeZone || !SpawnPointHelper.IsSniperZone(z))).ToArray();
                if (filtered.Length > 0)
                {
                    return filtered[UnityEngine.Random.Range(0, filtered.Length)];
                }
            }
            return null;
        }
    }
}
