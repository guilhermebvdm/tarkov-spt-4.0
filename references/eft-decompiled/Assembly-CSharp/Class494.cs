using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Class494
{
	public static void smethod_0(ref ComputeBuffer computeBuffer, int count, int stride, ComputeBufferType type = ComputeBufferType.Default)
	{
		if (computeBuffer == null || computeBuffer.count != count || computeBuffer.stride != stride)
		{
			computeBuffer?.Release();
			computeBuffer = new ComputeBuffer(count, stride, type);
		}
	}

	public static void smethod_1([CanBeNull] ref ComputeBuffer computeBuffer)
	{
		if (computeBuffer != null)
		{
			computeBuffer.Release();
			computeBuffer = null;
		}
	}

	public static void smethod_2([CanBeNull] ref CommandBuffer commandBuffer)
	{
		if (commandBuffer != null)
		{
			commandBuffer.Release();
			commandBuffer = null;
		}
	}
}
