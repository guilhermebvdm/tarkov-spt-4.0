using Audio.SpatialSystem;

public abstract class GClass1118
{
	public static bool Contains(this EAudioRoomTypeMask mask, EAudioRoomTypeMask target)
	{
		return (mask & target) != 0;
	}

	public static bool IsOutdoor(this EAudioRoomTypeMask mask)
	{
		return Contains(mask, EAudioRoomTypeMask.Outdoor);
	}

	public static bool IsIndoor(this EAudioRoomTypeMask mask)
	{
		return Contains(mask, EAudioRoomTypeMask.Indoor);
	}

	public static bool Is(this EAudioRoomTypeMask mask, EAudioRoomTypeMask target)
	{
		return (mask & target) == target;
	}
}
