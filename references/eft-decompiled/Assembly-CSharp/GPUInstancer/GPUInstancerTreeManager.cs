using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer;

[ExecuteInEditMode]
public class GPUInstancerTreeManager : GPUInstancerTerrainManager
{
	private static ComputeShader computeShader_0;

	public bool initializeWithCoroutine = true;

	public override void Awake()
	{
		base.Awake();
		if (computeShader_0 == null)
		{
			computeShader_0 = Resources.Load<ComputeShader>(GClass1262.TREE_INSTANTIATION_RESOURCE_PATH);
		}
	}

	public override void ClearInstancingData()
	{
		base.ClearInstancingData();
		if (base.terrain != null && base.terrain.treeDistance == 0f)
		{
			base.terrain.treeDistance = terrainSettings.maxTreeDistance;
		}
	}

	public override void GeneratePrototypes(bool forceNew = false)
	{
		base.GeneratePrototypes(forceNew);
		if (terrainSettings != null && base.terrain != null && base.terrain.terrainData != null)
		{
			GClass1274.SetTreeInstancePrototypes(base.gameObject, prototypeList, base.terrain.terrainData.treePrototypes, terrainSettings, forceNew);
		}
	}

	public override void InitializeRuntimeDataAndBuffers(bool forceNew = true)
	{
		base.InitializeRuntimeDataAndBuffers(forceNew);
		if ((forceNew || !isInitialized) && !(terrainSettings == null))
		{
			if (prototypeList != null && prototypeList.Count > 0)
			{
				GClass1274.AddTreeInstanceRuntimeDataToList(runtimeDataList, prototypeList, terrainSettings);
			}
			StartCoroutine(ReplaceUnityTrees());
			isInitialized = true;
		}
	}

	public override void DeletePrototype(GPUInstancerPrototype prototype, bool removeSO = true)
	{
		if (terrainSettings != null && base.terrain != null && base.terrain.terrainData != null)
		{
			int num = prototypeList.IndexOf(prototype);
			List<TreePrototype> list = new List<TreePrototype>(base.terrain.terrainData.treePrototypes);
			List<TreeInstance> list2 = new List<TreeInstance>();
			for (int i = 0; i < base.terrain.terrainData.treeInstances.Length; i++)
			{
				TreeInstance item = base.terrain.terrainData.treeInstances[i];
				if (item.prototypeIndex < num)
				{
					list2.Add(item);
				}
				else if (item.prototypeIndex > num)
				{
					item.prototypeIndex--;
					list2.Add(item);
				}
			}
			if (list.Count > num)
			{
				list.RemoveAt(num);
			}
			base.terrain.terrainData.treeInstances = list2.ToArray();
			base.terrain.terrainData.treePrototypes = list.ToArray();
			base.terrain.terrainData.RefreshPrototypes();
			if (removeSO)
			{
				base.DeletePrototype(prototype, removeSO);
			}
			GeneratePrototypes();
			if (!removeSO)
			{
				base.DeletePrototype(prototype, removeSO);
			}
		}
		else
		{
			base.DeletePrototype(prototype, removeSO);
		}
	}

	public IEnumerator ReplaceUnityTrees()
	{
		TreeInstance[] treeInstances = base.terrain.terrainData.treeInstances;
		int num = treeInstances.Length;
		if (num > 0)
		{
			_ = Vector3.zero;
			Vector4[] array = new Vector4[prototypeList.Count];
			int num2 = 0;
			foreach (GPUInstancerTreePrototype prototype in prototypeList)
			{
				array[num2] = (prototype.isApplyPrefabScale ? prototype.prefabObject.transform.localScale : Vector3.one);
				num2++;
			}
			base.terrain.treeDistance = 0f;
			Vector4[] array2 = new Vector4[num * 2];
			int[] array3 = new int[base.terrain.terrainData.treePrototypes.Length];
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				TreeInstance treeInstance = treeInstances[i];
				Vector3 position = treeInstance.position;
				array2[num3].x = treeInstance.prototypeIndex;
				array2[num3].y = position.x;
				array2[num3].z = position.y;
				array2[num3].w = position.z;
				num3++;
				array2[num3].x = treeInstance.rotation;
				array2[num3].y = treeInstance.widthScale;
				array2[num3].z = treeInstance.heightScale;
				num3++;
				array3[treeInstance.prototypeIndex]++;
			}
			if (initializeWithCoroutine)
			{
				yield return null;
			}
			ComputeBuffer computeBuffer = new ComputeBuffer(array2.Length, GClass1262.STRIDE_SIZE_FLOAT4);
			computeBuffer.SetData(array2);
			ComputeBuffer computeBuffer2 = new ComputeBuffer(array.Length, GClass1262.STRIDE_SIZE_FLOAT4);
			computeBuffer2.SetData(array);
			ComputeBuffer computeBuffer3 = new ComputeBuffer(1, GClass1262.STRIDE_SIZE_INT);
			uint[] data = new uint[1];
			for (int j = 0; j < runtimeDataList.Count; j++)
			{
				if (array3[j] != 0)
				{
					GClass1270 gClass = runtimeDataList[j];
					computeBuffer3.SetData(data);
					gClass.transformationMatrixVisibilityBuffer = new ComputeBuffer(array3[j], GClass1262.STRIDE_SIZE_MATRIX4X4);
					computeShader_0.SetBuffer(0, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, gClass.transformationMatrixVisibilityBuffer);
					computeShader_0.SetBuffer(0, GClass1262.GClass1268.TREE_DATA, computeBuffer);
					computeShader_0.SetBuffer(0, GClass1262.GClass1268.TREE_SCALES, computeBuffer2);
					computeShader_0.SetBuffer(0, GClass1262.GClass1267.COUNTER_BUFFER, computeBuffer3);
					computeShader_0.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, num);
					computeShader_0.SetVector(GClass1262.GClass1267.TERRAIN_SIZE_DATA, base.terrain.terrainData.size);
					computeShader_0.SetVector(GClass1262.GClass1268.TERRAIN_POSITION, base.terrain.GetPosition());
					computeShader_0.SetBool(GClass1262.GClass1268.IS_APPLY_ROTATION, ((GPUInstancerTreePrototype)gClass.prototype).isApplyRotation);
					computeShader_0.SetBool(GClass1262.GClass1268.IS_APPLY_TERRAIN_HEIGHT, ((GPUInstancerTreePrototype)gClass.prototype).isApplyTerrainHeight);
					computeShader_0.SetInt(GClass1262.GClass1268.PROTOTYPE_INDEX, j);
					computeShader_0.Dispatch(0, Mathf.CeilToInt((float)num / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
					gClass.bufferSize = array3[j];
					gClass.instanceCount = array3[j];
					GClass1274.InitializeGPUBuffer(gClass);
					if (initializeWithCoroutine)
					{
						yield return null;
					}
				}
			}
			computeBuffer.Release();
			computeBuffer2.Release();
			computeBuffer3.Release();
		}
		isInitial = true;
		GClass1274.TriggerEvent(GPUInstancerEventType.TreeInitializationFinished);
	}
}
