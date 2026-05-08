using System;

namespace Audio.SpatialSystem;

[Serializable]
public class AudioTriggerArea : ServerRoomColliderArea
{
	public override string Description { get; } = "AudioTriggerArea";
}
