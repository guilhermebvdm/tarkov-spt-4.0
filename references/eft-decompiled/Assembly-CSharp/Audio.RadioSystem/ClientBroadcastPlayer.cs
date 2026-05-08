using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Audio.AudioCulling;
using CommonAssets.Scripts.Audio.RadioSystem;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.RadioSystem;

public class ClientBroadcastPlayer : MonoBehaviour, GInterface80, GInterface94
{
	private const float float_0 = 0.05f;

	[Space]
	[SerializeField]
	private ERadioStation _radioStation;

	[SerializeField]
	protected AudioSource _sourceA;

	[SerializeField]
	protected AudioSource _sourceB;

	[SerializeField]
	private AudioMixerGroup _mixerGroup;

	[SerializeField]
	private AnimationCurve _spreadCurve;

	[SerializeField]
	private AnimationCurve _rolloffCurve;

	[SerializeField]
	private float _volume = 1f;

	[SerializeField]
	private float _minDistance = 3f;

	[SerializeField]
	private float _maxDistance = 30f;

	private GClass1186 gclass1186_0;

	private Transform transform_0;

	private bool bool_0;

	[CompilerGenerated]
	private EAudibleState eaudibleState_0 = EAudibleState.Unmute;

	[CompilerGenerated]
	private float float_1;

	[CompilerGenerated]
	private Action<EAudibleState> action_0;

	public ERadioStation RadioStation => _radioStation;

	public EAudibleState CurrentAudibleState
	{
		[CompilerGenerated]
		get
		{
			return eaudibleState_0;
		}
		[CompilerGenerated]
		set
		{
			eaudibleState_0 = value;
		}
	}

	public Vector3 CurrentPosition => transform_0.position;

	public float SqrMaxDistance
	{
		[CompilerGenerated]
		get
		{
			return float_1;
		}
		[CompilerGenerated]
		set
		{
			float_1 = value;
		}
	}

	public event Action<EAudibleState> AudibleStateChanged
	{
		[CompilerGenerated]
		add
		{
			Action<EAudibleState> action = action_0;
			Action<EAudibleState> action2;
			do
			{
				action2 = action;
				Action<EAudibleState> value2 = (Action<EAudibleState>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<EAudibleState> action = action_0;
			Action<EAudibleState> action2;
			do
			{
				action2 = action;
				Action<EAudibleState> value2 = (Action<EAudibleState>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public virtual void Initialize()
	{
		if (!bool_0)
		{
			SetupSource(_sourceA);
			SetupSource(_sourceB);
			gclass1186_0 = new GClass1186(_sourceA, _sourceB);
			transform_0 = base.transform;
			SqrMaxDistance = _maxDistance * _maxDistance;
			bool_0 = true;
		}
	}

	public void OnEnable()
	{
		action_0?.Invoke(CurrentAudibleState);
	}

	public void OnDisable()
	{
		action_0?.Invoke(CurrentAudibleState);
	}

	public void SyncBroadcast(AudioClip clip, double time = 0.0, float crossfadeDuration = 1f)
	{
		if (bool_0 && CurrentAudibleState != EAudibleState.Mute && base.gameObject.activeInHierarchy && method_0(clip.GetHashCode(), (float)time))
		{
			crossfadeDuration = method_1(clip, crossfadeDuration);
			gclass1186_0.Stop();
			gclass1186_0.MixSource.Stop();
			gclass1186_0.MixSource.clip = clip;
			gclass1186_0.MixSource.time = (float)time;
			gclass1186_0.MixSource.Play();
			gclass1186_0.StartCrossfade(crossfadeDuration, 1f, gclass1186_0.CurrentSource.volume, _volume, startFromRndTime: false);
		}
	}

	public bool method_0(int newClipHashcode, float time)
	{
		AudioClip clip = gclass1186_0.CurrentSource.clip;
		if (!gclass1186_0.IsCrossfadeStarted && !(clip == null))
		{
			if (clip.GetHashCode() == newClipHashcode)
			{
				return Mathf.Abs(gclass1186_0.CurrentSource.time - time) >= 0.05f;
			}
			return true;
		}
		return true;
	}

	public void Stop()
	{
		gclass1186_0.Stop();
		_sourceA.Stop();
		_sourceB.Stop();
	}

	public void Mute(bool value)
	{
		AudioSource sourceB = _sourceB;
		bool mute = (_sourceA.mute = value);
		sourceB.mute = mute;
	}

	public void ChangeStation(ERadioStation station)
	{
		_radioStation = station;
	}

	public virtual void ChangeAudibleState(EAudibleState state)
	{
		if (CurrentAudibleState != state)
		{
			switch (state)
			{
			case EAudibleState.Mute:
				Stop();
				_sourceA.enabled = false;
				_sourceB.enabled = false;
				break;
			case EAudibleState.Unmute:
				_sourceA.enabled = true;
				_sourceB.enabled = true;
				break;
			}
			CurrentAudibleState = state;
			action_0?.Invoke(state);
		}
	}

	public virtual void SetupSource(AudioSource source)
	{
		source.playOnAwake = false;
		source.loop = false;
		source.outputAudioMixerGroup = _mixerGroup;
		source.rolloffMode = AudioRolloffMode.Custom;
		source.SetCustomCurve(AudioSourceCurveType.Spread, _spreadCurve);
		source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _rolloffCurve);
		source.minDistance = _minDistance;
		source.maxDistance = _maxDistance;
	}

	public float method_1(AudioClip clip, float targetCrossfadeDuration)
	{
		if (!(clip.length < targetCrossfadeDuration))
		{
			return targetCrossfadeDuration;
		}
		return clip.length / 3f;
	}
}
