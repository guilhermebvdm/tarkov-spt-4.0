using System;
using UnityEngine;

public abstract class ISyncAble : MonoBehaviour
{
	[SerializeField]
	protected uint _random;

	private static int int_0;

	private uint uint_0;

	public void RegenerateRandom()
	{
		_random = method_0();
	}

	public void Awake()
	{
		uint_0 = (uint)GClass1298.GetStableHashCode(GClass6.GetFullPath(base.transform));
	}

	public void OnValidate()
	{
		if (_random.Equals(0u))
		{
			_random = method_0();
		}
	}

	public uint GetScenePathHash()
	{
		if (uint_0.Equals(0u))
		{
			uint_0 = (uint)GClass1298.GetStableHashCode(GClass6.GetFullPath(base.transform));
		}
		return uint_0;
	}

	public uint method_0()
	{
		int_0++;
		System.Random random = new System.Random(DateTime.Now.Millisecond + int_0);
		byte[] array = new byte[4];
		random.NextBytes(array);
		return BitConverter.ToUInt32(array, 0);
	}

	public ulong GetSceneId()
	{
		return (ulong)GetScenePathHash() | (ulong)_random;
	}

	public abstract void Serialize(GInterface131 writerStream);

	public abstract void Deserialize(IDataReader readerStream);

	public ISyncAble()
	{
	}
}
