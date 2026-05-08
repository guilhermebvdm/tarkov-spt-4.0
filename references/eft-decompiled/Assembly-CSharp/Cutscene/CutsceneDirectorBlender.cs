using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Timeline;

namespace Cutscene;

public class CutsceneDirectorBlender : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class751
	{
		public static readonly Class751 class751_0 = new Class751();

		public static Func<BlendingDirector, bool> func_0;

		public bool method_0(BlendingDirector d)
		{
			return d.IsPlaying;
		}
	}

	[SerializeField]
	private List<BlendingDirector> _blendingDirectors = new List<BlendingDirector>();

	[Header("Debug test loop blending")]
	[SerializeField]
	private bool _playAll;

	[CompilerGenerated]
	private BlendingDirector blendingDirector_0;

	private Dictionary<TimelineAsset, BlendingDirector> dictionary_0;

	private Queue<BlendingDirector> queue_0 = new Queue<BlendingDirector>();

	public bool IsTimelinePlaying
	{
		get
		{
			if (queue_0.Count <= 0)
			{
				return dictionary_0.Values.Any((BlendingDirector d) => d.IsPlaying);
			}
			return true;
		}
	}

	public BlendingDirector Current
	{
		[CompilerGenerated]
		get
		{
			return blendingDirector_0;
		}
		[CompilerGenerated]
		set
		{
			blendingDirector_0 = value;
		}
	}

	public void Start()
	{
		dictionary_0.Clear();
		foreach (BlendingDirector blendingDirector in _blendingDirectors)
		{
			dictionary_0.Add(blendingDirector.PlayableDirector.playableAsset as TimelineAsset, blendingDirector);
		}
	}

	public void Update()
	{
		if (_playAll)
		{
			_playAll = false;
			queue_0 = new Queue<BlendingDirector>(_blendingDirectors);
		}
		if (queue_0 != null && queue_0.Count != 0 && (Current == null || !Current.IsPlaying || (Current.IsBlendingOut && queue_0.Any())))
		{
			Current = queue_0.Dequeue();
			Current.Play();
		}
	}

	public void Play(TimelineAsset timeline, bool playImmediately = true, float transitionTime = 0.5f)
	{
		if (timeline == null || !dictionary_0.ContainsKey(timeline))
		{
			return;
		}
		BlendingDirector blendingDirector = dictionary_0[timeline];
		queue_0.Clear();
		if (Current == null)
		{
			queue_0.Enqueue(blendingDirector);
			return;
		}
		if (Current == blendingDirector && Current.IsPlaying)
		{
			Current.Seek(0f, transitionTime);
			return;
		}
		queue_0.Enqueue(blendingDirector);
		if (playImmediately && Current.IsPlaying)
		{
			Current.Stop(transitionTime);
		}
	}

	public void Play(List<TimelineAsset> timelineSequence, bool playImmediately = true, float transitionTime = 0.5f)
	{
		queue_0.Clear();
		foreach (TimelineAsset item in timelineSequence.Where((TimelineAsset t) => dictionary_0.ContainsKey(t)))
		{
			queue_0.Enqueue(dictionary_0[item]);
		}
		if (playImmediately && Current != null && !Current.IsBlendingOut)
		{
			Current.Stop(transitionTime);
		}
	}

	[CompilerGenerated]
	public bool method_0(TimelineAsset t)
	{
		return dictionary_0.ContainsKey(t);
	}
}
