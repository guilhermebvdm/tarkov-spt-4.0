using System;
using System.Runtime.InteropServices;
using GPUInstancer;
using UnityEngine;

public class GClass1275<T> : GInterface118
{
	public GPUInstancerPrefabPrototype prototype;

	public string bufferName;

	public ComputeBuffer variationBuffer;

	public T[] dataArray;

	public T defaultValue;

	public GClass1275(GPUInstancerPrefabPrototype prototype, string bufferName, T defaultValue = default(T))
	{
		this.prototype = prototype;
		this.bufferName = bufferName;
		this.defaultValue = defaultValue;
	}

	public void InitializeBufferAndArray(int count, bool setDefaults = true)
	{
		if (count == 0)
		{
			return;
		}
		dataArray = new T[count];
		if (setDefaults)
		{
			for (int i = 0; i < count; i++)
			{
				dataArray[i] = defaultValue;
			}
		}
		if (variationBuffer != null)
		{
			variationBuffer.Release();
		}
		variationBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(T)));
	}

	public void SetArrayAndInitializeBuffer(T[] dataArray)
	{
		if (dataArray != null && dataArray.Length != 0)
		{
			this.dataArray = dataArray;
			if (variationBuffer != null)
			{
				variationBuffer.Release();
			}
			variationBuffer = new ComputeBuffer(dataArray.Length, Marshal.SizeOf(typeof(T)));
		}
	}

	public void SetInstanceData(GPUInstancerPrefab prefabInstance)
	{
		if (prefabInstance.variationDataList != null && dataArray != null && prefabInstance.variationDataList.ContainsKey(bufferName) && dataArray.Length > prefabInstance.gpuInstancerID - 1)
		{
			dataArray[prefabInstance.gpuInstancerID - 1] = (T)prefabInstance.variationDataList[bufferName];
		}
	}

	public void SetBufferData(int managedBufferStartIndex, int computeBufferStartIndex, int count)
	{
		if (variationBuffer != null && count > 0)
		{
			variationBuffer.SetData(dataArray, managedBufferStartIndex, computeBufferStartIndex, count);
		}
	}

	public void SetVariation(MaterialPropertyBlock mpb)
	{
		if (variationBuffer != null)
		{
			mpb.SetBuffer(bufferName, variationBuffer);
		}
	}

	public void SetNewBufferSize(int newCount)
	{
		if (newCount <= 0)
		{
			return;
		}
		int num = 0;
		if (dataArray != null)
		{
			num = dataArray.Length;
			if (num < newCount)
			{
				Array.Resize(ref dataArray, newCount);
			}
		}
		else
		{
			dataArray = new T[newCount];
		}
		if (num < newCount)
		{
			if (variationBuffer != null)
			{
				variationBuffer.Release();
			}
			variationBuffer = new ComputeBuffer(newCount, Marshal.SizeOf(typeof(T)));
			Array.Resize(ref dataArray, newCount);
			for (int i = num; i < newCount; i++)
			{
				dataArray[i] = defaultValue;
			}
			variationBuffer.SetData(dataArray);
		}
	}

	public GPUInstancerPrefabPrototype GetPrototype()
	{
		return prototype;
	}

	public string GetBufferName()
	{
		return bufferName;
	}

	public void ReleaseBuffer()
	{
		if (variationBuffer != null)
		{
			variationBuffer.Release();
		}
		variationBuffer = null;
		dataArray = null;
	}
}
