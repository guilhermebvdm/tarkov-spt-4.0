using EFT;
using System;
using System.Reflection;

namespace PlayerLives.Helpers
{
    internal static class FikaReviveDetector
    {
        private const string FikaPlayerTypeName = "Fika.Core.Main.Players.FikaPlayer";
        private const string FikaHealthControllerTypeName = "Fika.Core.Main.ClientClasses.ClientHealthController";

        public static bool IsFikaReviveEnabled(Player player)
        {
            if (player == null)
            {
                return false;
            }

            try
            {
                bool isFikaPlayer = player.GetType().FullName == FikaPlayerTypeName;
                object healthController = player.ActiveHealthController;
                Type healthControllerType = healthController?.GetType();
                bool isFikaHealthController = healthControllerType?.FullName == FikaHealthControllerTypeName;

                if (!isFikaPlayer && !isFikaHealthController)
                {
                    return false;
                }

                PropertyInfo reviveEnabled = healthControllerType?.GetProperty("ReviveEnabled", BindingFlags.Instance | BindingFlags.Public);
                return reviveEnabled?.PropertyType == typeof(bool) && (bool)reviveEnabled.GetValue(healthController);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"Could not check Fika revive status: {ex.Message}");
                return false;
            }
        }
    }
}
