using UnityEngine;

namespace EFT;

public interface IAudioClipContainer
{
	AudioClip GetClip();

	float GetVolume();

	int GetMaxDistance();
}
