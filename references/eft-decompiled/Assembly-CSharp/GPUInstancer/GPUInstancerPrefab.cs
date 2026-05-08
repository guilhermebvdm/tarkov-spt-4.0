using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerPrefab : MonoBehaviour
{
	[HideInInspector]
	public GPUInstancerPrefabPrototype prefabPrototype;

	[HideInInspector]
	public int gpuInstancerID;

	[HideInInspector]
	public PrefabInstancingState state;

	public Dictionary<string, object> variationDataList;

	private bool bool_0;

	private Transform transform_0;

	private bool bool_1;

	private Matrix4x4 matrix4x4_0;

	public void AddVariation<T>(string bufferName, T value)
	{
		if (variationDataList == null)
		{
			variationDataList = new Dictionary<string, object>();
		}
		if (variationDataList.ContainsKey(bufferName))
		{
			variationDataList[bufferName] = value;
		}
		else
		{
			variationDataList.Add(bufferName, value);
		}
	}

	public Transform GetInstanceTransform(bool forceNew = false)
	{
		if (!bool_0 || forceNew)
		{
			transform_0 = base.transform;
			transform_0.hasChanged = false;
			bool_0 = true;
		}
		return transform_0;
	}

	public Matrix4x4 GetLocalToWorldMatrix(bool forceNew = false)
	{
		if (!bool_1 || forceNew)
		{
			matrix4x4_0 = GetInstanceTransform(forceNew).localToWorldMatrix;
			bool_1 = true;
		}
		return matrix4x4_0;
	}

	public virtual void SetupPrefabInstance(GClass1270 runtimeData, bool forceNew = false)
	{
	}
}
