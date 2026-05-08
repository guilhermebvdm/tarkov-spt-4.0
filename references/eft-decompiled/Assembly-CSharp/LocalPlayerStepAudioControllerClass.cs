using System;
using System.Collections.Generic;
using Audio.SpatialSystem;
using Comfort.Common;
using CommonAssets.Scripts.Audio;
using EFT;
using EFT.EnvironmentEffect;
using UnityEngine;

public class LocalPlayerStepAudioControllerClass : GInterface95, IDisposable
{
	[NonSerialized]
	public const float Float_0 = 0f;

	[NonSerialized]
	public const float Float_1 = 1f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3 = 1f;

	[NonSerialized]
	public Dictionary<EAudioMovementState, SoundBank> Dictionary_0 = new Dictionary<EAudioMovementState, SoundBank>();

	[NonSerialized]
	public BetterSource BetterSource_0;

	[NonSerialized]
	public BaseBallistic.ESurfaceSound EsurfaceSound_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public IPlayer Iplayer_0;

	[NonSerialized]
	public bool Bool_1;

	public LocalPlayerStepAudioControllerClass(SurfaceSet surfaceSet, IPlayer player, float minStepVolume = 0.1f, bool useOcclusion = false, ESeasonStatus season = ESeasonStatus.Summer)
	{
		method_1(surfaceSet, player, useOcclusion);
		Iplayer_0 = player;
		Float_2 = minStepVolume;
		if (Singleton<GClass1706>.Instantiated)
		{
			Float_3 = Singleton<GClass1706>.Instance.AudioSettings.EnvironmentSettings.GetSeasonSettings(season).StepsVolumeMultiplier;
		}
		EnvironmentManager.Instance.OnPlayerEnvironmentChanged += method_0;
	}

	public void method_0(string profileID, EnvironmentType env)
	{
		if (!(Iplayer_0.ProfileId != profileID))
		{
			if (env == EnvironmentType.Indoor)
			{
				method_5();
			}
			else
			{
				InitializeSource(Iplayer_0, !Iplayer_0.IsYourPlayer);
			}
		}
	}

	public void method_1(SurfaceSet surfaceSet, IPlayer player, bool useOcclusion)
	{
		method_2(surfaceSet);
		InitializeSource(player, useOcclusion);
	}

	public void method_2(SurfaceSet surfaceSet)
	{
		if (surfaceSet == null)
		{
			Debug.LogError("Surface set not initialized");
			return;
		}
		Dictionary_0 = new Dictionary<EAudioMovementState, SoundBank>
		{
			{
				EAudioMovementState.Run,
				surfaceSet.RunSoundBank
			},
			{
				EAudioMovementState.Sprint,
				surfaceSet.SprintSoundBank
			},
			{
				EAudioMovementState.Stop,
				surfaceSet.StopSoundBank
			},
			{
				EAudioMovementState.Land,
				surfaceSet.LandingSoundBank
			},
			{
				EAudioMovementState.Turn,
				surfaceSet.TurnSoundBank
			},
			{
				EAudioMovementState.Duck,
				surfaceSet.DuckSoundBank
			},
			{
				EAudioMovementState.Prone,
				surfaceSet.ProneSoundBank
			},
			{
				EAudioMovementState.Drop,
				surfaceSet.ProneDropSoundBank
			},
			{
				EAudioMovementState.Jump,
				surfaceSet.JumpSoundBank
			}
		};
	}

	public virtual void InitializeSource(IPlayer player, bool useOcclusion)
	{
		BetterAudio instance = MonoBehaviourSingleton<BetterAudio>.Instance;
		BetterSource_0 = instance.GetSource(BetterAudio.AudioSourceGroupType.Character);
		BetterSource_0.StartTrackingPosition(player.Transform.Original);
		BetterSource_0.LocalPosition = new Vector3(0f, 0.1f, 0f);
		BetterSource_0.EnabledEQ(!player.IsYourPlayer);
		BetterSource_0.SetMixerGroup(useOcclusion ? instance.ObservedPlayerMovementMixer : instance.ClientPlayerMovementMixer);
		if (useOcclusion && MonoBehaviourSingleton<SpatialAudioSystem>.Instantiated)
		{
			MonoBehaviourSingleton<SpatialAudioSystem>.Instance.ProcessSourceOcclusion(player, BetterSource_0);
		}
		Bool_1 = true;
	}

	public void UpdateSoundRolloff(float rolloff)
	{
		if (BetterSource_0 != null)
		{
			BetterSource_0.SetRolloff(rolloff);
		}
	}

	public void UpdateSurface(BaseBallistic.ESurfaceSound surface)
	{
		EsurfaceSound_0 = surface;
	}

	public void UpdateUnderRoofStatus(bool isUnderRoof)
	{
		Bool_0 = isUnderRoof;
	}

	public virtual void Play(EAudioMovementState movementState, EnvironmentType environment, float distance = 0f, float baseStepVolume = 1f, float blendParameter = 0f, bool stereo = false)
	{
		if (environment == EnvironmentType.Indoor || movementState == EAudioMovementState.None)
		{
			return;
		}
		if (!method_4(movementState, out var bank))
		{
			Debug.LogError($"Can't find bank for movement state: {movementState}");
		}
		else if (!Bool_0)
		{
			if (!Bool_1)
			{
				InitializeSource(Iplayer_0, !Iplayer_0.IsYourPlayer);
			}
			float volume = CalculateFinalVolume(baseStepVolume, bank);
			bank.Play(BetterSource_0, EnvironmentType.Outdoor, distance, volume, blendParameter, stereo);
		}
	}

	public virtual float CalculateFinalVolume(float baseStepVolume, SoundBank bank)
	{
		float num = method_3();
		float num2 = (Iplayer_0.IsYourPlayer ? 1f : bank.BaseVolume);
		return Mathf.Clamp(baseStepVolume * num * num2, Float_2, 1f) * Float_3;
	}

	public float method_3()
	{
		float result = 0f;
		if (!Singleton<BackendConfigSettingsClass>.Instantiated)
		{
			return result;
		}
		return Singleton<GClass1706>.Instance.AudioSettings.EnvironmentSettings.GetMultBySurface(EsurfaceSound_0);
	}

	public bool method_4(EAudioMovementState movementState, out SoundBank bank)
	{
		return Dictionary_0.TryGetValue(movementState, out bank);
	}

	public virtual void Dispose()
	{
		if (EnvironmentManager.Instance != null)
		{
			EnvironmentManager.Instance.OnPlayerEnvironmentChanged -= method_0;
		}
		method_5();
	}

	public void method_5()
	{
		if (BetterSource_0 != null)
		{
			BetterSource_0.Release();
			BetterSource_0 = null;
		}
		Bool_1 = false;
	}
}
