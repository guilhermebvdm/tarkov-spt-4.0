using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class AmbientSoundPlayer : BaseRandomAmbientSoundPlayer
{
	[SerializeField]
	private SoundBank _ambientBank;

	public override SoundBank CurrentBank
	{
		get
		{
			return _ambientBank;
		}
		set
		{
			_ambientBank = value;
		}
	}
}
