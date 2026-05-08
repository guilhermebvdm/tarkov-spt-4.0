using CommonAssets.Scripts.Audio.RadioSystem;
using UnityEngine;

public interface GInterface79
{
	bool TryGetClipAndBroadcastTime(ERadioStation station, out AudioClip clip, out double broadcastTime);
}
