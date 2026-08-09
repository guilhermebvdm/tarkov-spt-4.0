using EFT;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Combined.Classes;

public static class DeathAudioController
{
	public static void HandleDeathAudio(Player player, EBodyPart bodyPart)
	{
		if (player == null) return;

		switch ((int)bodyPart)
		{
			case 0: // Head / Decapitation / Explosive headshot
				// Silence vocal cords (head destroyed), play visceral blood gurgle/squirt SFX
				if (player.Speaker != null)
				{
					try { player.Speaker.Shut(); } catch {}
				}
				PlayBloodGurgleSFX(player);
				break;

			case 1: // Chest
			case 2: // Stomach
				// Agony groans / suffocating breaths
				PlayVocalPhraseIfAvailable(player, EPhraseTrigger.OnAgony);
				break;

			case 3: // Left Arm
			case 4: // Right Arm
				// Intense pain scream for arm dismemberment
				PlayVocalPhraseIfAvailable(player, EPhraseTrigger.HandBroken);
				break;

			case 5: // Left Leg
			case 6: // Right Leg
				// High intensity pain scream for leg dismemberment
				PlayVocalPhraseIfAvailable(player, EPhraseTrigger.LegBroken);
				break;
		}
	}

	private static void PlayBloodGurgleSFX(Player player)
	{
		if (VisceralEntry.Instance?.effectContainer?.bloodSFX != null && VisceralEntry.Instance.effectContainer.bloodSFX.Count > 0)
		{
			int index = Random.Range(0, VisceralEntry.Instance.effectContainer.bloodSFX.Count);
			GameObject sfxPrefab = VisceralEntry.Instance.effectContainer.bloodSFX[index];
			if (sfxPrefab != null)
			{
				Vector3 pos = player.Position;
				Object.Instantiate(sfxPrefab, pos, Quaternion.identity);
			}
		}
	}

	private static void PlayVocalPhraseIfAvailable(Player player, EPhraseTrigger trigger)
	{
		try
		{
			if (player != null && player.Speaker != null)
			{
				player.Speaker.Play(trigger, player.HealthStatus, demand: true);
			}
		}
		catch
		{
			// Safe fallback if speaker source was already released
		}
	}
}
