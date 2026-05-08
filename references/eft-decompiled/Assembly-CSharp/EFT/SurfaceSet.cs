using System;
using EFT.Vaulting;
using UnityEngine;

namespace EFT;

[Serializable]
public class SurfaceSet
{
	public BaseBallistic.ESurfaceSound Surface;

	public SoundBank RunSoundBank;

	public SoundBank SprintSoundBank;

	public SoundBank StopSoundBank;

	public SoundBank LandingSoundBank;

	public SoundBank TurnSoundBank;

	public SoundBank DuckSoundBank;

	public SoundBank ProneSoundBank;

	public SoundBank ProneDropSoundBank;

	public SoundBank JumpSoundBank;

	[Space]
	[Header("Vaulting Sounds")]
	public VaultingSoundSet VaultSoundSet = new VaultingSoundSet();

	public VaultingSoundSet SprintVaultSoundSet = new VaultingSoundSet();

	public VaultingSoundSet ClimbSoundSet = new VaultingSoundSet();

	public bool IsFull
	{
		get
		{
			if (RunSoundBank != null && SprintSoundBank != null && StopSoundBank != null && LandingSoundBank != null && TurnSoundBank != null && DuckSoundBank != null && ProneSoundBank != null && ProneDropSoundBank != null)
			{
				return JumpSoundBank != null;
			}
			return false;
		}
	}

	public bool TryGetVaultingSoundSet(EVaultingSoundType soundType, out VaultingSoundSet vaultingSet)
	{
		vaultingSet = null;
		switch (soundType)
		{
		default:
			Debug.LogError($"Can't find bank for type: {soundType}");
			break;
		case EVaultingSoundType.None:
			return false;
		case EVaultingSoundType.Vault:
			vaultingSet = VaultSoundSet;
			break;
		case EVaultingSoundType.SprintVault:
			vaultingSet = SprintVaultSoundSet;
			break;
		case EVaultingSoundType.Climb:
			vaultingSet = ClimbSoundSet;
			break;
		}
		return vaultingSet != null;
	}
}
