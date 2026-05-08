using System;
using UnityEngine;

namespace Audio.ConfiguredAudioPlayer;

[Serializable]
public class AudioClipConfig : IEquatable<AudioClipConfig>
{
	public AudioClip clip;

	public float volume = 1f;

	public float pitch = 1f;

	public bool loop;

	public float fadeInTime;

	public float fadeOutTime;

	public bool Equals(AudioClipConfig other)
	{
		if (other == null)
		{
			return false;
		}
		if (clip == other.clip && Mathf.Approximately(volume, other.volume) && Mathf.Approximately(pitch, other.pitch) && loop == other.loop && Mathf.Approximately(fadeInTime, other.fadeInTime))
		{
			return Mathf.Approximately(fadeOutTime, other.fadeOutTime);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as AudioClipConfig);
	}

	public override int GetHashCode()
	{
		return (((((391 + ((clip != null) ? clip.GetHashCode() : 0)) * 23 + volume.GetHashCode()) * 23 + pitch.GetHashCode()) * 23 + loop.GetHashCode()) * 23 + fadeInTime.GetHashCode()) * 23 + fadeOutTime.GetHashCode();
	}

	public static bool operator ==(AudioClipConfig a, AudioClipConfig b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a != null && (object)b != null)
		{
			return a.Equals(b);
		}
		return false;
	}

	public static bool operator !=(AudioClipConfig a, AudioClipConfig b)
	{
		return !(a == b);
	}
}
