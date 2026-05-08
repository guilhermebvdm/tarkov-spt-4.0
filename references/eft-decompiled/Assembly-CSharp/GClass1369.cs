using UnityEngine;

public abstract class GClass1369
{
	public const uint PACKET_MAX_SIZE = 1600u;

	public static void CheckMaxSize(int size, uint maxSize = 1600u)
	{
		if (size <= 0 || size > maxSize)
		{
			Debug.LogErrorFormat("serialized data size:{0} maxSize:{1}", size, maxSize);
			throw InvalidMaxSize(size, maxSize);
		}
	}

	public static GException10 InvalidMaxSize(int size, uint maxSize)
	{
		return new GException10(size, maxSize);
	}

	public static GException10 InvalidMaxSize(int size, uint maxSize, GInterface127 bitReader)
	{
		return new GException10(size, maxSize, bitReader);
	}
}
