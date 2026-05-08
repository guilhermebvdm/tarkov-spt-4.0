using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class EventRandomPlayer : BaseRandomAmbientSoundPlayer, GInterface104
{
	[SerializeField]
	private EventSoundBankChanger.EventSoundBanks _soundBanks;

	private bool bool_3;

	[CompilerGenerated]
	private SoundBank soundBank_0;

	public override SoundBank CurrentBank
	{
		[CompilerGenerated]
		get
		{
			return soundBank_0;
		}
		[CompilerGenerated]
		set
		{
			soundBank_0 = value;
		}
	}

	public override void OnPlay(float dt)
	{
		if (bool_3)
		{
			base.OnPlay(dt);
		}
	}

	public void ChangeSoundContent(EEventType eventType)
	{
		if (_soundBanks.TryGetValue(eventType, out var value))
		{
			CurrentBank = value;
			bool_3 = true;
		}
		else
		{
			Debug.Log($"No proposed bank for event: {eventType}");
		}
	}
}
