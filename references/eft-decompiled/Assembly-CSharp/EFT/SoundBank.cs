using System;
using System.Linq;
using UnityEngine;

namespace EFT;

[Serializable]
public class SoundBank : ScriptableObject
{
	public float CustomLength;

	public float BaseVolume = 1f;

	public float BasePitch = 1f;

	public float DeltaVolume;

	public float DeltaPitch;

	public float Rolloff = 100f;

	public bool IgnoreOcclusion;

	public bool Physical;

	public bool HasEnvironment;

	public BetterAudio.AudioSourceGroupType SourceType = BetterAudio.AudioSourceGroupType.Environment;

	public bool AllowLimitedPlay;

	[SerializeField]
	private float _clipLiength;

	public EnvironmentVariety[] Environments = new EnvironmentVariety[Enum.GetNames(typeof(EnvironmentType)).Length];

	public DistanceBlendOptions BlendOptions;

	private byte[] _shuffle;

	private byte _shuffleIndex;

	public float RandomVolume
	{
		get
		{
			if (!(DeltaVolume > 0f))
			{
				return BaseVolume;
			}
			return BaseVolume + UnityEngine.Random.Range(0f - DeltaVolume, 0f);
		}
	}

	public float RandomPitch => BasePitch + UnityEngine.Random.Range(0f - DeltaPitch, DeltaPitch);

	public float ClipLength
	{
		get
		{
			if (CustomLength != 0f)
			{
				return CustomLength;
			}
			return _clipLiength;
		}
		set
		{
			_clipLiength = value;
		}
	}

	public float[] BlendValues => BlendOptions.Values;

	public void Shuffle(byte ln)
	{
		if (_shuffle == null || _shuffle.Length < ln)
		{
			_shuffle = new byte[ln];
		}
		for (byte b = 0; b < ln; b++)
		{
			_shuffle[b] = b;
		}
		for (byte b2 = 0; b2 < ln; b2++)
		{
			int num = UnityEngine.Random.Range(0, ln - 1);
			byte b3 = _shuffle[num];
			_shuffle[num] = _shuffle[b2];
			_shuffle[b2] = b3;
		}
		_shuffleIndex = ln;
	}

	public int GetRandomClipIndex(int ln)
	{
		if (ln > 1)
		{
			if (_shuffleIndex < 1)
			{
				Shuffle((byte)ln);
			}
			_shuffleIndex--;
			return _shuffle[_shuffleIndex];
		}
		return 0;
	}

	public AudioClip PickSingleClip(int environment)
	{
		EnvironmentVariety environmentVariety = Environments[HasEnvironment ? environment : 0];
		AudioClip clip = null;
		method_0(environmentVariety[0], ref clip);
		return clip;
	}

	public void PickClipsByDistance(ref AudioClip clip1, ref AudioClip clip2, ref float proportions, int environment, float distance)
	{
		EnvironmentVariety environmentVariety = Environments[HasEnvironment ? environment : 0];
		if (BlendOptions == null)
		{
			method_0(environmentVariety[0], ref clip1);
			return;
		}
		int i;
		for (i = 0; i < BlendValues.Length && !(distance < BlendValues[i]); i++)
		{
		}
		switch (i)
		{
		case 0:
			method_0(environmentVariety[0], ref clip1);
			break;
		case 1:
			method_0(environmentVariety[0], ref clip1);
			method_0(environmentVariety[1], ref clip2);
			break;
		case 2:
			method_0(environmentVariety[1], ref clip1);
			break;
		case 3:
			method_0(environmentVariety[1], ref clip1);
			method_0(environmentVariety[2], ref clip2);
			break;
		case 4:
			method_0(environmentVariety[2], ref clip1);
			break;
		}
		if (i % 2 != 0)
		{
			proportions = Mathf.InverseLerp(BlendValues[i], BlendValues[i - 1], distance);
		}
		else
		{
			proportions = 1f;
		}
	}

	public void method_0(DistanceVarity clips, ref AudioClip clip)
	{
		int length = clips.Length;
		if (length != 0)
		{
			int i = GetRandomClipIndex(length) % length;
			clip = clips[i];
		}
	}

	public float PlayOn(BetterSource source, EnvironmentType pos = EnvironmentType.Outdoor, float distance = 0f, float volume = 1f, bool forceStereo = false, bool oneShot = true)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		PickClipsByDistance(ref clip, ref clip2, ref proportions, (int)pos, distance);
		if (clip == null && clip2 == null)
		{
			return 0f;
		}
		float maxDistance = source.MaxDistance;
		if (distance > maxDistance)
		{
			return 0f;
		}
		if (source.OcclusionVolumeFactor <= 0f)
		{
			return 0f;
		}
		source.SetRolloff(Rolloff);
		source.Play(clip, clip2, proportions, volume, forceStereo, oneShot);
		return Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
	}

	public float PlayWithConstantRolloffDistance(BetterSource source, EnvironmentType pos = EnvironmentType.Outdoor, float distance = 0f, float volume = 1f, float blendParameter = 0f, bool forceStereo = false)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		PickClipsByDistance(ref clip, ref clip2, ref proportions, (int)pos, blendParameter);
		if (clip == null && clip2 == null)
		{
			return 0f;
		}
		float maxDistance = source.source1.maxDistance;
		if (distance > maxDistance)
		{
			return 0f;
		}
		volume = volume * Rolloff / maxDistance;
		source.Play(clip, clip2, proportions, volume, forceStereo);
		return Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
	}

	public float PlayWithCustomRolloffDistance(BetterSource source, EnvironmentType pos = EnvironmentType.Outdoor, float distance = 0f, float volume = 1f, float blendParameter = 0f, bool forceStereo = false, float customRolloff = 120f)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		PickClipsByDistance(ref clip, ref clip2, ref proportions, (int)pos, blendParameter);
		if (clip == null && clip2 == null)
		{
			return 0f;
		}
		float maxDistance = source.source1.maxDistance;
		if (distance > maxDistance)
		{
			return 0f;
		}
		volume = volume * customRolloff / maxDistance;
		source.Play(clip, clip2, proportions, volume, forceStereo);
		return Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
	}

	public void Play(BetterSource source, EnvironmentType pos = EnvironmentType.Outdoor, float distance = 0f, float volume = 1f, float blendParameter = 0f, bool forceStereo = false, bool oneShot = true)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		PickClipsByDistance(ref clip, ref clip2, ref proportions, (int)pos, blendParameter);
		if (!(clip == null) || !(clip2 == null))
		{
			float maxDistance = source.MaxDistance;
			if (!(distance > maxDistance))
			{
				volume = Mathf.Clamp01(volume);
				source.SetPitch(RandomPitch);
				source.Play(clip, clip2, proportions, volume, forceStereo, oneShot);
			}
		}
	}

	public float PlayOnScheduled(BetterSource source, double time, EnvironmentType pos = EnvironmentType.Outdoor, float distance = 0f, float volume = 1f)
	{
		AudioClip clip = null;
		AudioClip clip2 = null;
		float proportions = 1f;
		PickClipsByDistance(ref clip, ref clip2, ref proportions, (int)pos, distance);
		if (clip == null && clip2 == null)
		{
			return 0f;
		}
		source.SetRolloff(Rolloff);
		source.Play(clip, clip2, proportions, volume);
		source.PlayScheduled(time);
		return Mathf.Max((clip != null) ? clip.length : 0f, (clip2 != null) ? clip2.length : 0f);
	}

	public float PickClips(float distance, ref AudioClip clip1, ref AudioClip clip2, ref float proportions, EnvironmentType pos = EnvironmentType.Outdoor)
	{
		PickClipsByDistance(ref clip1, ref clip2, ref proportions, (int)pos, distance);
		return Mathf.Max((clip1 != null) ? clip1.length : 0f, (clip2 != null) ? clip2.length : 0f);
	}

	public void method_1(BetterSource source, float d)
	{
		if (source == null)
		{
			return;
		}
		AudioClip clip = source.GetClip(1);
		if (clip == null)
		{
			return;
		}
		int num;
		if (Environments[0].Clips[1].Clips.Contains(clip))
		{
			num = 1;
		}
		else
		{
			if (!Environments[0].Clips[2].Clips.Contains(clip))
			{
				return;
			}
			num = 3;
		}
		source.Balance(Mathf.InverseLerp(BlendValues[num], BlendValues[num - 1], d));
	}

	public int method_2(AudioClip audioClip)
	{
		int num = 0;
		while (true)
		{
			if (num < 2)
			{
				if (Environments[0].Clips[num].Clips.Contains(audioClip))
				{
					break;
				}
				num++;
				continue;
			}
			return -1;
		}
		return num;
	}
}
