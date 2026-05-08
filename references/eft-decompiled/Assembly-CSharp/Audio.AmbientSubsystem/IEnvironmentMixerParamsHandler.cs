using Audio.SpatialSystem;
using UnityEngine.Audio;

namespace Audio.AmbientSubsystem;

public interface IEnvironmentMixerParamsHandler
{
	void Apply(AudioMixer mixer, EAudioRoomTypeMask roomType);
}
