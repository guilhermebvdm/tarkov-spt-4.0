using EFT;
using UnityEngine;

namespace SAIN.Components.Helpers;

public class SAINSoundTypeHandler
{
    public static void AISoundFileChecker(string sound, Player player)
    {
        if (player == null || player.HealthController?.IsAlive == false)
        {
            return;
        }

        SAINSoundType soundType = SAINSoundType.None;
        // ref: AUD-02-05 - Null-safety no HandsController
        var Item = player.HandsController?.Item;
        float soundDist = 20f;

        if (Item != null)
        {
            if (Item is ThrowWeapItemClass)
            {
                if (sound == "Pin")
                {
                    soundType = SAINSoundType.GrenadePin;
                    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_GrenadePinDraw;
                }
                if (sound == "Draw")
                {
                    soundType = SAINSoundType.GrenadeDraw;
                    soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_GrenadePinDraw;
                }
            }
            else if (Item is MedsItemClass)
            {
                soundType = SAINSoundType.Heal;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Healing;
                if (sound == "CapRemove" || sound == "Inject")
                {
                    soundDist *= 0.5f;
                }
            }
            else if (Item is FoodDrinkItemClass)
            {
                soundType = SAINSoundType.Food;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_EatDrink;
            }
            // ref: AUD-02-02 - Classificar como Reload apenas se o clipe de audio for de recarga/mag
            else if (!string.IsNullOrEmpty(sound) && (sound.IndexOf("Reload", System.StringComparison.OrdinalIgnoreCase) >= 0 || sound.IndexOf("Mag", System.StringComparison.OrdinalIgnoreCase) >= 0))
            {
                soundType = SAINSoundType.Reload;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_Reload;
            }
            else
            {
                soundType = SAINSoundType.GearSound;
                soundDist = SAINPlugin.LoadedPreset.GlobalSettings.Hearing.BaseSoundRange_AimingandGearRattle;
            }
        }

        BotManagerComponent.Instance?.BotHearing.PlayAISound(player.ProfileId, soundType, player.Position + Vector3.up, soundDist, 1f);
    }
}
