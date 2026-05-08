using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Cutscene;

[RequireComponent(typeof(PlayableDirector))]
public class BlendingDirector : MonoBehaviour
{
	public enum BlendType
	{
		None,
		In,
		Out,
		Seek
	}

	[CompilerGenerated]
	public class Class745
	{
		public Action onBlendInFinished;

		public BlendingDirector blendingDirector_0;

		public void method_0()
		{
			onBlendInFinished?.Invoke();
			blendingDirector_0.action_0 = null;
			blendingDirector_0.CurrentBlend = BlendType.None;
		}
	}

	[CompilerGenerated]
	public class Class746
	{
		public Action onFinished;

		public BlendingDirector blendingDirector_0;

		public void method_0()
		{
			onFinished?.Invoke();
			blendingDirector_0.CurrentBlend = BlendType.None;
		}
	}

	[CompilerGenerated]
	public class Class747
	{
		public Action onFinished;

		public BlendingDirector blendingDirector_0;

		public void method_0()
		{
			onFinished?.Invoke();
			blendingDirector_0.CurrentBlend = BlendType.None;
		}
	}

	[SerializeField]
	private PlayableDirector _playableDirector;

	[CompilerGenerated]
	private BlendType blendType_0;

	private AnimationPlayableOutput animationPlayableOutput_0;

	private Playable playable_0;

	private Playable playable_1;

	private AnimationMixerPlayable animationMixerPlayable_0;

	private int int_0;

	private int int_1;

	private float float_0;

	private float float_1;

	private TrackAsset trackAsset_0;

	private double double_0 = -1.0;

	private Action action_0;

	private float float_2;

	private AnimationPlayableOutput animationPlayableOutput_1;

	private int int_2;

	private Stopwatch stopwatch_0 = new Stopwatch();

	private static bool bool_0;

	private Coroutine coroutine_0;

	private Coroutine coroutine_1;

	private Coroutine coroutine_2;

	private string string_0;

	public PlayableDirector PlayableDirector => _playableDirector;

	public bool IsPlaying => _playableDirector.state == PlayState.Playing;

	public BlendType CurrentBlend
	{
		[CompilerGenerated]
		get
		{
			return blendType_0;
		}
		[CompilerGenerated]
		set
		{
			blendType_0 = value;
		}
	}

	public bool IsBlending => CurrentBlend != BlendType.None;

	public bool IsBlendingOut => CurrentBlend == BlendType.Out;

	public bool IsBlendingIn => CurrentBlend == BlendType.In;

	public double TimeRemaining => _playableDirector.time - _playableDirector.duration;

	public string Name => _playableDirector.name;

	public void Awake()
	{
		string_0 = "OverriddenDirectorOutput" + GetInstanceID();
	}

	public void Update()
	{
		if (IsPlaying && CurrentBlend == BlendType.None && double_0 >= 0.0 && _playableDirector.time >= double_0)
		{
			CurrentBlend = BlendType.Out;
			stopwatch_0.Restart();
			if (coroutine_0 != null)
			{
				StopCoroutine(coroutine_0);
			}
			coroutine_0 = StartCoroutine(method_2(_playableDirector, animationPlayableOutput_0, float_2, animationPlayableOutput_0.GetWeight(), delegate
			{
				stopwatch_0.Stop();
				action_0?.Invoke();
				action_0 = null;
				CurrentBlend = BlendType.None;
			}));
		}
	}

	public void OnDestroy()
	{
		method_0();
	}

	public void OnDisable()
	{
		method_0();
	}

	public void method_0()
	{
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
		if (coroutine_1 != null)
		{
			StopCoroutine(coroutine_1);
			coroutine_1 = null;
		}
		if (coroutine_2 != null)
		{
			StopCoroutine(coroutine_2);
			coroutine_2 = null;
		}
	}

	public void method_1()
	{
		_playableDirector.Evaluate();
		if (_playableDirector.playableGraph.IsValid())
		{
			int_2 = 0;
			trackAsset_0 = (_playableDirector.playableAsset as TimelineAsset)?.GetOutputTrack(int_2);
			animationPlayableOutput_1 = (AnimationPlayableOutput)_playableDirector.playableGraph.GetOutputByType<AnimationPlayableOutput>(int_2);
			playable_0 = animationPlayableOutput_1.GetSourcePlayable();
			playable_1 = _playableDirector.playableAsset.CreatePlayable(_playableDirector.playableGraph, _playableDirector.gameObject);
			animationMixerPlayable_0 = AnimationMixerPlayable.Create(_playableDirector.playableGraph, 2);
			int_0 = animationMixerPlayable_0.AddInput(playable_1, 0);
			int_1 = animationMixerPlayable_0.AddInput(playable_0, 0, 1f);
			if (animationPlayableOutput_1.IsOutputValid() && animationPlayableOutput_1.GetTarget() != null)
			{
				animationPlayableOutput_0 = AnimationPlayableOutput.Create(_playableDirector.playableGraph, string_0, animationPlayableOutput_1.GetTarget());
				animationPlayableOutput_0.SetSourcePlayable(animationMixerPlayable_0);
				animationPlayableOutput_0.SetSourceOutputPort(animationPlayableOutput_1.GetSourceOutputPort());
				animationPlayableOutput_0.SetWeight(1f);
				animationPlayableOutput_1.SetTarget(null);
			}
		}
	}

	public void Play(float blendInDuration = 0.5f, float blendOutDuration = 0.5f, float startTime = -1f, float endTime = -1f, Action onBlendInFinished = null, Action onFinished = null)
	{
		if (CurrentBlend != BlendType.None)
		{
			return;
		}
		CurrentBlend = BlendType.In;
		if (!animationPlayableOutput_0.IsOutputValid())
		{
			_playableDirector.RebuildGraph();
			method_1();
		}
		if (!animationPlayableOutput_0.IsOutputValid())
		{
			return;
		}
		action_0 = onFinished;
		float_2 = blendOutDuration;
		double_0 = ((endTime < 0f) ? (trackAsset_0.start + trackAsset_0.duration - (double)blendOutDuration) : ((double)Math.Max(0f, endTime - blendInDuration))) - (double)Time.deltaTime;
		if (blendInDuration == 0f)
		{
			_playableDirector.Play();
			return;
		}
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
		}
		coroutine_0 = StartCoroutine(method_3(_playableDirector, animationPlayableOutput_0, blendInDuration, startTime, delegate
		{
			onBlendInFinished?.Invoke();
			action_0 = null;
			CurrentBlend = BlendType.None;
		}));
	}

	public void Stop(float blendDuration = 0.5f, Action onFinished = null)
	{
		if (blendDuration <= 0f)
		{
			_playableDirector.Pause();
		}
		if (CurrentBlend != BlendType.None && CurrentBlend != BlendType.In)
		{
			return;
		}
		if (CurrentBlend == BlendType.In)
		{
			bool_0 = true;
		}
		CurrentBlend = BlendType.Out;
		if (!animationPlayableOutput_0.IsOutputValid())
		{
			_playableDirector.RebuildGraph();
			method_1();
		}
		if (animationPlayableOutput_0.IsOutputValid())
		{
			if (coroutine_1 != null)
			{
				StopCoroutine(coroutine_1);
			}
			coroutine_1 = StartCoroutine(method_2(_playableDirector, animationPlayableOutput_0, blendDuration, animationPlayableOutput_0.GetWeight(), delegate
			{
				onFinished?.Invoke();
				CurrentBlend = BlendType.None;
			}));
		}
	}

	public void Seek(float toTime, float blendDuration, Action onFinished = null)
	{
		if (CurrentBlend != BlendType.None)
		{
			return;
		}
		CurrentBlend = BlendType.Seek;
		if (animationPlayableOutput_0.IsOutputValid())
		{
			if (coroutine_2 != null)
			{
				StopCoroutine(coroutine_2);
			}
			coroutine_2 = StartCoroutine(method_4(blendDuration, toTime, delegate
			{
				onFinished?.Invoke();
				CurrentBlend = BlendType.None;
			}));
		}
	}

	public IEnumerator method_2(PlayableDirector director, AnimationPlayableOutput output, float blendTime, float fromWeight = 1f, Action onFinished = null)
	{
		for (float num = blendTime - blendTime * fromWeight; !(num >= blendTime); num += Time.deltaTime)
		{
			float value = 1f - Mathf.Clamp01(num / blendTime);
			if (!output.IsOutputValid())
			{
				break;
			}
			output.SetWeight(value);
			yield return null;
		}
		if (output.IsOutputValid())
		{
			output.SetWeight(0f);
		}
		onFinished?.Invoke();
		if (director.isActiveAndEnabled)
		{
			director.Pause();
		}
		coroutine_1 = null;
	}

	public IEnumerator method_3(PlayableDirector director, AnimationPlayableOutput output, float blendTime, float startTime = -1f, Action onFinished = null)
	{
		director.time = ((startTime > 0f) ? startTime : 0f);
		director.Play();
		output.SetWeight(0f);
		bool_0 = false;
		for (float num = 0f; !(num >= blendTime); num += Time.deltaTime)
		{
			if (bool_0)
			{
				bool_0 = false;
				break;
			}
			float value = Mathf.Clamp01(num / blendTime);
			output.SetWeight(value);
			yield return null;
		}
		output.SetWeight(1f);
		onFinished?.Invoke();
		coroutine_0 = null;
	}

	public IEnumerator method_4(float blendTime, float startTime = -1f, Action onFinished = null)
	{
		float num = 0f;
		playable_1.SetTime(_playableDirector.time);
		playable_1.Play();
		animationPlayableOutput_0.SetWeight(1f);
		animationMixerPlayable_0.SetInputWeight(int_0, 1f);
		animationMixerPlayable_0.SetInputWeight(int_1, 0f);
		_playableDirector.time = startTime;
		_playableDirector.Play();
		_playableDirector.Evaluate();
		for (; num < blendTime; num += Time.deltaTime)
		{
			float num2 = Mathf.Clamp01(num / blendTime);
			float_0 = 1f - num2;
			float_1 = num2;
			animationMixerPlayable_0.SetInputWeight(int_0, float_0);
			animationMixerPlayable_0.SetInputWeight(int_1, float_1);
			yield return null;
		}
		animationMixerPlayable_0.SetInputWeight(int_0, 0f);
		animationMixerPlayable_0.SetInputWeight(int_1, 1f);
		playable_1.Pause();
		onFinished?.Invoke();
		coroutine_2 = null;
	}

	[CompilerGenerated]
	public void method_5()
	{
		stopwatch_0.Stop();
		action_0?.Invoke();
		action_0 = null;
		CurrentBlend = BlendType.None;
	}
}
