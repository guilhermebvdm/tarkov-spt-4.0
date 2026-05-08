using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceCulling : MonoBehaviour
{
	[Serializable]
	public struct AudioSourceData
	{
		public int InstanceID;

		public AudioSource AudioSource;

		public Transform Transform;

		public float SqrCullDistance;

		public float CachedMaxDistance;

		public bool NeedRestart;

		public void UpdateData()
		{
			float maxDistance = AudioSource.maxDistance;
			if (CachedMaxDistance > maxDistance || CachedMaxDistance < maxDistance)
			{
				float maxDistance2 = AudioSource.maxDistance;
				SqrCullDistance = maxDistance2 * maxDistance2 + 25f;
				CachedMaxDistance = maxDistance2;
			}
		}
	}

	private const int int_0 = 10;

	private const float float_0 = 25f;

	private int int_1;

	private GClass1180 gclass1180_0;

	private readonly List<AudioSourceData> list_0 = new List<AudioSourceData>();

	private readonly HashSet<int> hashSet_0 = new HashSet<int>();

	private Transform transform_0;

	private Coroutine coroutine_0;

	private bool bool_0;

	public void Awake()
	{
		transform_0 = base.transform;
		gclass1180_0 = new GClass1180(256, 160, 128);
	}

	public void Start()
	{
		method_2();
		method_1();
	}

	public void SetListenerTransform(Transform listenerTransform)
	{
		transform_0 = listenerTransform;
	}

	public void Register(AudioSource audioSource, bool restartSource = false)
	{
		if (hashSet_0.Add(audioSource.GetInstanceID()))
		{
			list_0.Add(new AudioSourceData
			{
				InstanceID = audioSource.GetInstanceID(),
				AudioSource = audioSource,
				Transform = audioSource.transform,
				SqrCullDistance = audioSource.maxDistance * audioSource.maxDistance + 25f,
				CachedMaxDistance = audioSource.maxDistance,
				NeedRestart = restartSource
			});
			int_1 = Math.Min(list_0.Count, 10);
			if (!bool_0)
			{
				method_1();
			}
		}
	}

	public void Remove(AudioSource audioSource)
	{
		int instanceID = audioSource.GetInstanceID();
		int num = 0;
		while (true)
		{
			if (num < list_0.Count)
			{
				if (list_0[num].InstanceID == instanceID)
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		list_0.RemoveAt(num);
		hashSet_0.Remove(instanceID);
	}

	public void CleanRegisteredAudioSources()
	{
		list_0.Clear();
		hashSet_0.Clear();
	}

	public void StopWork()
	{
		method_2();
	}

	public void EnableRestart(int sourceInstanceID, bool restartEnabled)
	{
		for (int i = 0; i < list_0.Count; i++)
		{
			AudioSourceData value = list_0[i];
			value.NeedRestart = restartEnabled;
			list_0[i] = value;
		}
	}

	public IEnumerator method_0()
	{
		int num = 0;
		while (list_0.Count > 0)
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				AudioSourceData value = list_0[i];
				value.UpdateData();
				list_0[i] = value;
				float num2 = Vector3.SqrMagnitude(value.Transform.position - transform_0.position);
				bool flag = num2 <= value.SqrCullDistance;
				AudioSource audioSource = value.AudioSource;
				audioSource.priority = gclass1180_0.CalculatePriority(num2, value.SqrCullDistance);
				audioSource.enabled = flag;
				if (flag && audioSource.gameObject.activeInHierarchy && value.NeedRestart && !audioSource.isPlaying)
				{
					audioSource.Play();
				}
				num++;
				if (num > int_1)
				{
					num = 0;
					yield return null;
				}
			}
		}
		bool_0 = false;
	}

	public void method_1()
	{
		bool_0 = true;
		coroutine_0 = StartCoroutine(method_0());
	}

	public void method_2()
	{
		bool_0 = false;
		if (coroutine_0 != null)
		{
			StopCoroutine(coroutine_0);
			coroutine_0 = null;
		}
	}

	public void OnDestroy()
	{
		method_2();
		CleanRegisteredAudioSources();
	}
}
