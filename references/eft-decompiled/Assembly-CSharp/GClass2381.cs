using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GClass2381 : IDisposable
{
	[NonSerialized]
	public ComputeBuffer ComputeBuffer_0 = new ComputeBuffer(7, 16, ComputeBufferType.Structured);

	[NonSerialized]
	public SphericalHarmonicsL2 SphericalHarmonicsL2_0;

	public SphericalHarmonicsL2 CurrentSH => SphericalHarmonicsL2_0;

	public bool SetData(SphericalHarmonicsL2 sh)
	{
		if (sh == SphericalHarmonicsL2_0)
		{
			return false;
		}
		SphericalHarmonicsL2_0 = sh;
		NativeArray<Vector4> data = new NativeArray<Vector4>(7, Allocator.Temp);
		data[6] = new Vector4(sh[0, 8], sh[1, 8], sh[2, 8], 1f);
		for (int i = 0; i < 3; i++)
		{
			data[i] = new Vector4(sh[i, 3], sh[i, 1], sh[i, 2], sh[i, 0] - sh[i, 6]);
			data[i + 3] = new Vector4(sh[i, 4], sh[i, 5], sh[i, 6] * 3f, sh[i, 7]);
		}
		ComputeBuffer_0.SetData(data);
		return true;
	}

	public void Dispose()
	{
		ComputeBuffer_0?.Dispose();
	}

	public static implicit operator ComputeBuffer(GClass2381 ambientBuffer)
	{
		return ambientBuffer.ComputeBuffer_0;
	}
}
