using GPUInstancer;
using UnityEngine;

public interface GInterface118
{
	void InitializeBufferAndArray(int count, bool setDefaults = true);

	void SetInstanceData(GPUInstancerPrefab prefabInstance);

	void SetBufferData(int managedBufferStartIndex, int computeBufferStartIndex, int count);

	void SetVariation(MaterialPropertyBlock mpb);

	void SetNewBufferSize(int newCount);

	GPUInstancerPrefabPrototype GetPrototype();

	string GetBufferName();

	void ReleaseBuffer();
}
