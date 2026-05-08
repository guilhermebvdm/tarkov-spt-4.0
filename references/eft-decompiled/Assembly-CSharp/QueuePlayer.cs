using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class QueuePlayer : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class567
	{
		public static readonly Class567 class567_0 = new Class567();

		public static Action<AudioSource> action_0;

		public static Predicate<AudioSource> predicate_0;

		public static Action<AudioSource> action_1;

		public void method_0(AudioSource x)
		{
			x.playOnAwake = false;
		}

		public bool method_1(AudioSource x)
		{
			return !x.isPlaying;
		}

		public void method_2(AudioSource x)
		{
			x.gameObject.SetActive(value: false);
		}
	}

	private List<AudioSource> list_0;

	private AudioSource audioSource_0;

	public GClass1628<AudioClip> Queue = new GClass1628<AudioClip>();

	private Coroutine coroutine_0;

	public float QueueRefreshTime = 1f;

	public void Awake()
	{
		list_0 = new List<AudioSource>(base.gameObject.GetComponentsInChildren<AudioSource>());
		list_0.ForEach(delegate(AudioSource x)
		{
			x.playOnAwake = false;
		});
		if (list_0.Count > 0)
		{
			audioSource_0 = list_0[0];
			Queue.ItemAdded += method_0;
			Queue.ItemRemoved += method_0;
			Queue.AllItemsRemoved += delegate
			{
				method_0(null);
			};
		}
		else
		{
			base.enabled = false;
		}
	}

	public void method_0(AudioClip _)
	{
		if (coroutine_0 == null && Queue.Count > 0)
		{
			coroutine_0 = StartCoroutine(method_1());
		}
	}

	public IEnumerator method_1()
	{
		while (Queue.Count > 0)
		{
			if (list_0.TrueForAll((AudioSource x) => !x.isPlaying))
			{
				list_0.ForEach(delegate(AudioSource x)
				{
					x.gameObject.SetActive(value: false);
				});
				yield return null;
				AudioClip clip = Queue[0];
				Queue.RemoveAt(0);
				method_2(clip);
			}
			yield return new WaitForSeconds(QueueRefreshTime);
		}
		coroutine_0 = null;
	}

	public void Play(AudioClip clip)
	{
		Queue.Add(clip);
	}

	public void method_2(AudioClip clip)
	{
		foreach (AudioSource item in list_0)
		{
			item.gameObject.SetActive(value: true);
			item.clip = clip;
			item.Play();
		}
	}

	[CompilerGenerated]
	public void method_3()
	{
		method_0(null);
	}
}
