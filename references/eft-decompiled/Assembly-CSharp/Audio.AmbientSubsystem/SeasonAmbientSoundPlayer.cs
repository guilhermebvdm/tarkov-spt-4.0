using System;
using EFT;
using UnityEngine;

namespace Audio.AmbientSubsystem;

public class SeasonAmbientSoundPlayer : BaseRandomAmbientSoundPlayer, GInterface98
{
	[Serializable]
	public class SeasonSoundBanks : SerializableEnumDictionary<ESeasonStatus, SoundBank>
	{
	}

	[Serializable]
	public class SeasonRandomTimeRange : SerializableEnumDictionary<ESeasonStatus, Vector2>
	{
	}

	[HideInInspector]
	[SerializeField]
	private SeasonSoundBanks _seasonAmbientBanks;

	[HideInInspector]
	[SerializeField]
	private SeasonRandomTimeRange _seasonRandomTimeRange;

	private SoundBank soundBank_0;

	private Action action_3;

	private bool bool_3;

	private ESeasonStatus eseasonStatus_0;

	public override SoundBank CurrentBank
	{
		get
		{
			return soundBank_0;
		}
		set
		{
			soundBank_0 = value;
		}
	}

	public override float GetRandomDelay()
	{
		if (_seasonRandomTimeRange.TryGetValue(eseasonStatus_0, out var value))
		{
			return UnityEngine.Random.Range(value.x, value.y);
		}
		return base.GetRandomDelay();
	}

	public bool method_5(ESeasonStatus seasonStatus, out SoundBank bank)
	{
		bank = null;
		if (_seasonAmbientBanks.TryGetValue(seasonStatus, out bank))
		{
			soundBank_0 = bank;
			return true;
		}
		return false;
	}

	public override void OnDestroy()
	{
		action_3?.Invoke();
		action_3 = null;
		base.OnDestroy();
	}

	public void SetSeasonStatus(ESeasonStatus targetSeasonStatus)
	{
		if (!method_5(targetSeasonStatus, out var bank))
		{
			Debug.LogWarning("sound bank is null on GO: " + base.gameObject.name);
			return;
		}
		ChangeSoundBank(bank);
		eseasonStatus_0 = targetSeasonStatus;
		bool_3 = true;
	}

	public override bool CanPlay()
	{
		if (bool_3)
		{
			return base.CanPlay();
		}
		return false;
	}
}
