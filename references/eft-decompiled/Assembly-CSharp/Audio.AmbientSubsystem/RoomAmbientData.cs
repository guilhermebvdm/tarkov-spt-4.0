using System;
using Audio.AmbientSubsystem.Data;
using UnityEngine;

namespace Audio.AmbientSubsystem;

[Serializable]
public class RoomAmbientData
{
	[Header("Room Tone Settings")]
	public float FadeInSeconds = 0.25f;

	public float FadeOutSeconds = 0.5f;

	[Range(0f, 1f)]
	public float RoomToneVolume = 1f;

	public AudioClip RoomTone;

	[Header("Room Outdoor Settings")]
	[Range(0f, 1f)]
	public float OutdoorAmbientVolume = 0.1f;

	[Range(20f, 22000f)]
	public float OutdoorAmbientHighCutFreq = 0.5f;

	[Header("Precipitation Settings")]
	public SeasonClipsContentPreset SeasonSoundPreset;

	public bool UseVolumeRange;

	[Range(0f, 1f)]
	public float PrecipitationVolume = 1f;

	[Tooltip("From Floor To Ceiling (RANGE 0-1)")]
	public Vector2 PrecipitationVolumeRange = new Vector2(1f, 1f);

	[Header("Reverb Settings")]
	[GAttribute17]
	public string ReverbPreset = string.Empty;

	[Range(0f, 1f)]
	public float ReverbSendVolume = 1f;

	public int RoomToneClipHash
	{
		get
		{
			if (!(RoomTone == null))
			{
				return RoomTone.GetHashCode();
			}
			return -1;
		}
	}

	public int GetPrecipitationClip(ESeasonStatus season, RainController.ERainIntensity rainIntensity, out AudioClip clip)
	{
		clip = null;
		if (!(SeasonSoundPreset == null) && SeasonSoundPreset.TryGetContent(season, out var content))
		{
			if (!content.TryGetValue(rainIntensity, out clip))
			{
				return -1;
			}
			return clip.GetHashCode();
		}
		return -1;
	}
}
