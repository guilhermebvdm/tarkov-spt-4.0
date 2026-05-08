using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio.Data;

[Serializable]
[CreateAssetMenu(menuName = "Scriptable objects/Audio/SoundBankWithSettings", fileName = "SoundBankWithSettings")]
public class SoundBankWithSettings : ScriptableObject
{
	[SerializeField]
	private List<AudioClip> Clips = new List<AudioClip>();

	[Range(0f, 5f)]
	public int Importance;

	[Range(0f, 1f)]
	public float BaseVolume = 1f;

	[Range(0f, 1000f)]
	public float Rolloff = 100f;

	private List<AudioClip> _excludedClips = new List<AudioClip>();

	private List<AudioClip> _includedClips = new List<AudioClip>();

	public bool TryGetRandomClip(out AudioClip clip, bool ignoreExcluded = false)
	{
		clip = null;
		if (Clips.Count == 0)
		{
			return false;
		}
		clip = (ignoreExcluded ? method_0() : method_1());
		return clip != null;
	}

	public AudioClip method_0()
	{
		int index = UnityEngine.Random.Range(0, Clips.Count - 1);
		return Clips[index];
	}

	public AudioClip method_1()
	{
		if (_includedClips.Count == 0)
		{
			method_2();
		}
		int index = UnityEngine.Random.Range(0, _includedClips.Count - 1);
		AudioClip audioClip = _includedClips[index];
		_includedClips.Remove(audioClip);
		_excludedClips.Add(audioClip);
		return audioClip;
	}

	public void method_2()
	{
		_excludedClips.Clear();
		_includedClips.AddRange(Clips);
	}
}
