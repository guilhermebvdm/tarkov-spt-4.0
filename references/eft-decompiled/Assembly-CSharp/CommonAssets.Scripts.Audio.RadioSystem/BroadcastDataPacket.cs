using System;

namespace CommonAssets.Scripts.Audio.RadioSystem;

[Serializable]
public struct BroadcastDataPacket
{
	public byte radioStation;

	public byte broadcastItemType;

	public int broadcastItemIndex;

	public float currentClipTime;
}
