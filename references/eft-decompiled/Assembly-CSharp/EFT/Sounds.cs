using Audio.AmbientSubsystem.Data;
using UnityEngine;

namespace EFT;

public class Sounds : ScriptableObject
{
	public SurfaceSet[] Sets;

	public SoundBank Gear;

	public SoundBank GearMedium;

	public SoundBank GearFast;

	public SoundBank GearVault;

	public SoundBank GearSprintVault;

	public SoundBank GearClimb;

	public SoundBank BackpackDrop;

	public AudioClip TinnitusSound;

	public AudioClip FractureSound;

	public AudioClip FaceShieldOn;

	public AudioClip FaceShieldOff;

	public AudioClip NightVisionOn;

	public AudioClip NightVisionOff;

	public AudioClip ThermalVisionOn;

	public AudioClip ThermalVisionOff;

	public AudioClip PropIn;

	public AudioClip PropOut;

	public AudioClip SwitchHeadlights;

	public SeasonSurfaceSetContainer SeasonSounds;

	public SurfaceSet GetSurfaceSet(BaseBallistic.ESurfaceSound surfaceSound)
	{
		SurfaceSet[] sets = Sets;
		int num = 0;
		SurfaceSet surfaceSet;
		while (true)
		{
			if (num < sets.Length)
			{
				surfaceSet = sets[num];
				if (surfaceSound == surfaceSet.Surface)
				{
					break;
				}
				num++;
				continue;
			}
			return null;
		}
		return surfaceSet;
	}

	public bool TryGetSeasonMovementSet(ESeasonStatus season, out SurfaceSet set)
	{
		return SeasonSounds.TryGetContent(season, out set);
	}
}
