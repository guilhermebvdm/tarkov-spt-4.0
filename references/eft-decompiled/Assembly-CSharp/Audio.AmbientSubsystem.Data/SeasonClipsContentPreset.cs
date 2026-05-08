using UnityEngine;

namespace Audio.AmbientSubsystem.Data;

[CreateAssetMenu(menuName = "Scriptable objects/Audio/AmbientSubsystem/SeasonClipsContentPreset", fileName = "SeasonClipsContentPreset")]
public class SeasonClipsContentPreset : EnumSoundContentPreset<ESeasonStatus, AudioClipByRainIntensity>
{
}
