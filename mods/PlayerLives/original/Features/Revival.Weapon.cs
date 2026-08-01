using EFT;
using System;

namespace PlayerLives.Features
{
    // Hide / reveal the player's weapon while downed. Paired: hide on down,
    // reveal at invuln end (see Revival.Invulnerability.cs).
    internal partial class RevivalFeatures
    {
        private static void HideWeapon(Player player)
        {
            if (player == null)
                return;

            try
            {
                if (player.HandsIsEmpty) return;
                player.SetEmptyHands(delegate { });
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error hiding downed weapon: {ex.Message}");
            }
        }

        private static void ShowWeapon(Player player)
        {
            if (player == null)
                return;

            try
            {
                if (!player.HandsIsEmpty)
                    return;

                player.RevealWeapon();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"Error showing weapon: {ex.Message}");
            }
        }
    }
}
