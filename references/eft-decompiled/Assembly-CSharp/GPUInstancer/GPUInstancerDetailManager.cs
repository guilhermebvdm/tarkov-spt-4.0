using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerDetailManager : GPUInstancerTerrainManager
{
	public int detailLayer;

	public bool runInThreads = true;

	public bool doRefresh = true;

	private static ComputeShader computeShader_0;

	private ComputeBuffer computeBuffer_0;

	private bool bool_0;

	private ComputeBuffer computeBuffer_1;

	private int[] int_2 = new int[1];

	public float[,] threadHeightMapData;

	public List<int[,]> threadDetailMapData;

	public int threadHeightResolution;

	private float float_0;

	public override void Awake()
	{
		if (computeShader_0 == null)
		{
			computeShader_0 = (ComputeShader)Resources.Load(GClass1262.GRASS_INSTANTIATION_RESOURCE_PATH);
		}
		base.terrain.freeUnusedRenderingResources = false;
		base.Awake();
		CameraClass.Instance.OnCameraChanged -= method_0;
		CameraClass.Instance.OnCameraChanged += method_0;
		method_0();
	}

	public void method_0()
	{
		method_5(method_1());
	}

	public IEnumerator method_1()
	{
		yield return null;
		yield return null;
		yield return null;
		yield return null;
		GClass1257.UpdateTerrainNormalMapDetailInstance(this);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		computeBuffer_0?.Release();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		CameraClass.Instance.OnCameraChanged -= method_0;
	}

	public override void ClearInstancingData()
	{
		base.ClearInstancingData();
		if (base.terrain != null && base.terrain.detailObjectDistance <= 0f)
		{
			base.terrain.detailObjectDistance = terrainSettings.maxDetailDistanceLegacy;
		}
		computeBuffer_1?.Release();
		computeBuffer_1 = null;
	}

	public override void GeneratePrototypes(bool forceNew = false)
	{
		base.GeneratePrototypes(forceNew);
		if (terrainSettings != null && base.terrain != null && base.terrain.terrainData != null)
		{
			GClass1274.SetDetailInstancePrototypes(this, prototypeList, base.terrain.terrainData.detailPrototypes, 2, terrainSettings, forceNew, base.terrain);
		}
	}

	public override void InitializeRuntimeDataAndBuffers(bool forceNew = true)
	{
		base.InitializeRuntimeDataAndBuffers(forceNew);
		if ((!forceNew && isInitialized) || terrainSettings == null || base.terrain == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(terrainSettings.warningText))
		{
			Debug.LogError("A GPU Instancer Detail Manager currently has errors. Please refer to the error description in the Detail manager.");
			return;
		}
		replacingInstances = false;
		initalizingInstances = true;
		base.terrain.detailObjectDistance = 0f;
		InitializeSpatialPartitioning();
		List<GPUInstancerPrototype> list = prototypeList;
		if (list != null && list.Count > 0)
		{
			GClass1274.AddDetailInstanceRuntimeDataToList(base.terrain, runtimeDataList, prototypeList, terrainSettings, detailLayer);
		}
		isInitialized = true;
	}

	public override void UpdateSpatialPartitioningCells(GPUInstancerCameraData renderingCameraData)
	{
		base.UpdateSpatialPartitioningCells(renderingCameraData);
		if (!(terrainSettings == null) && spData != null && !initalizingInstances && !replacingInstances && spData.IsActiveCellUpdateRequired(renderingCameraData.mainCamera.transform.position))
		{
			replacingInstances = true;
			method_5(GClass1262.DETAIL_STORE_INSTANCE_DATA ? method_6() : method_7());
		}
	}

	public override void DeletePrototype(GPUInstancerPrototype prototype, bool removeSO = true)
	{
		if (terrainSettings != null && base.terrain != null && base.terrain.terrainData != null)
		{
			int num = prototypeList.IndexOf(prototype);
			DetailPrototype[] detailPrototypes = base.terrain.terrainData.detailPrototypes;
			List<DetailPrototype> list = new List<DetailPrototype>();
			List<int[,]> list2 = new List<int[,]>();
			for (int i = 0; i < detailPrototypes.Length; i++)
			{
				if (i != num)
				{
					list.Add(detailPrototypes[i]);
					list2.Add(base.terrain.terrainData.GetDetailLayer(0, 0, base.terrain.terrainData.detailResolution, base.terrain.terrainData.detailResolution, i));
				}
				base.terrain.terrainData.SetDetailLayer(0, 0, i, new int[base.terrain.terrainData.detailResolution, base.terrain.terrainData.detailResolution]);
			}
			base.terrain.terrainData.detailPrototypes = list.ToArray();
			for (int j = 0; j < list2.Count; j++)
			{
				base.terrain.terrainData.SetDetailLayer(0, 0, j, list2[j]);
			}
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

	public override void RemoveInstancesInsideBounds(Bounds bounds, float offset, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		base.RemoveInstancesInsideBounds(bounds, offset, prototypeFilter);
		if (spData == null || initalizingInstances)
		{
			return;
		}
		int num = base.terrain.terrainData.detailResolution / spData.cellRowAndCollumnCountPerTerrain;
		float num2 = base.terrain.terrainData.size.x / (float)spData.cellRowAndCollumnCountPerTerrain / (float)num;
		float num3 = base.terrain.terrainData.size.z / (float)spData.cellRowAndCollumnCountPerTerrain / (float)num;
		int num4 = Mathf.CeilToInt((bounds.extents.x * 2f + offset) / num2);
		int num5 = Mathf.CeilToInt((bounds.extents.z * 2f + offset) / num3);
		foreach (GClass1259 cell in spData.GetCellList())
		{
			if (!cell.cellInnerBounds.Intersects(bounds))
			{
				continue;
			}
			if (cell.isActive && cell.detailInstanceBuffers != null)
			{
				foreach (int key in cell.detailInstanceBuffers.Keys)
				{
					if (prototypeFilter == null || prototypeFilter.Contains(prototypeList[key]))
					{
						GClass1274.RemoveInstancesInsideBounds(cell.detailInstanceBuffers[key], bounds.center, bounds.extents, offset);
					}
				}
			}
			int num6 = Mathf.FloorToInt((bounds.center.x - bounds.extents.x - cell.instanceStartPosition.x - offset) / num2);
			int num7 = Mathf.FloorToInt((bounds.center.z - bounds.extents.z - cell.instanceStartPosition.z - offset) / num3);
			for (int i = 0; i < cell.detailMapData.Count; i++)
			{
				if (prototypeFilter != null && !prototypeFilter.Contains(prototypeList[i]))
				{
					continue;
				}
				for (int j = num7; j < num5 + num7; j++)
				{
					if (j < 0 || j >= num)
					{
						continue;
					}
					for (int k = num6; k < num4 + num6; k++)
					{
						if (k >= 0 && k < num)
						{
							cell.detailMapData[i][k + j * num] = 0;
						}
					}
				}
			}
		}
	}

	public override void RemoveInstancesInsideCollider(Collider collider, float offset, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		base.RemoveInstancesInsideCollider(collider, offset, prototypeFilter);
		if (spData == null || initalizingInstances)
		{
			return;
		}
		Bounds bounds = collider.bounds;
		int num = base.terrain.terrainData.detailResolution / spData.cellRowAndCollumnCountPerTerrain;
		float num2 = base.terrain.terrainData.size.x / (float)spData.cellRowAndCollumnCountPerTerrain / (float)num;
		float num3 = base.terrain.terrainData.size.z / (float)spData.cellRowAndCollumnCountPerTerrain / (float)num;
		int num4 = Mathf.CeilToInt((bounds.extents.x * 2f + offset) / num2);
		int num5 = Mathf.CeilToInt((bounds.extents.z * 2f + offset) / num3);
		Vector3 zero = Vector3.zero;
		_ = Vector3.zero;
		foreach (GClass1259 cell in spData.GetCellList())
		{
			if (!cell.cellInnerBounds.Intersects(bounds))
			{
				continue;
			}
			if (cell.isActive && cell.detailInstanceBuffers != null)
			{
				foreach (int key in cell.detailInstanceBuffers.Keys)
				{
					if (prototypeFilter == null || prototypeFilter.Contains(prototypeList[key]))
					{
						if (collider is BoxCollider)
						{
							GClass1274.RemoveInstancesInsideBoxCollider(cell.detailInstanceBuffers[key], (BoxCollider)collider, offset);
						}
						else if (collider is SphereCollider)
						{
							GClass1274.RemoveInstancesInsideSphereCollider(cell.detailInstanceBuffers[key], (SphereCollider)collider, offset);
						}
						else if (collider is CapsuleCollider)
						{
							GClass1274.RemoveInstancesInsideCapsuleCollider(cell.detailInstanceBuffers[key], (CapsuleCollider)collider, offset);
						}
						else
						{
							GClass1274.RemoveInstancesInsideBounds(cell.detailInstanceBuffers[key], collider.bounds.center, collider.bounds.extents, offset);
						}
					}
				}
			}
			int num6 = Mathf.FloorToInt((bounds.center.x - bounds.extents.x - cell.instanceStartPosition.x - offset) / num2);
			int num7 = Mathf.FloorToInt((bounds.center.z - bounds.extents.z - cell.instanceStartPosition.z - offset) / num3);
			for (int i = num7; i < num5 + num7; i++)
			{
				if (i < 0 || i >= num)
				{
					continue;
				}
				for (int j = num6; j < num4 + num6; j++)
				{
					if (j < 0 || j >= num)
					{
						continue;
					}
					zero.x = cell.instanceStartPosition.x + (float)j * num2;
					zero.z = cell.instanceStartPosition.z + (float)i * num3;
					zero.y = base.terrain.SampleHeight(zero);
					if (Vector3.Distance(collider.ClosestPoint(zero), zero) > num2 + offset)
					{
						continue;
					}
					for (int k = 0; k < cell.detailMapData.Count; k++)
					{
						if (prototypeFilter == null || prototypeFilter.Contains(prototypeList[k]))
						{
							cell.detailMapData[k][j + i * num] = 0;
						}
					}
				}
			}
		}
	}

	public override void SetGlobalPositionOffset(Vector3 offsetPosition)
	{
		base.SetGlobalPositionOffset(offsetPosition);
		if (spData == null)
		{
			return;
		}
		foreach (GClass1259 cell in spData.GetCellList())
		{
			if (cell == null)
			{
				continue;
			}
			cell.instanceStartPosition += offsetPosition;
			cell.cellBounds.center += offsetPosition;
			if (cell.detailInstanceBuffers != null)
			{
				foreach (ComputeBuffer value in cell.detailInstanceBuffers.Values)
				{
					if (value != null)
					{
						GClass1262.computeRuntimeModification.SetBuffer(GClass1262.computeBufferTransformOffsetId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, value);
						GClass1262.computeRuntimeModification.SetInt(GClass1262.GClass1264.BUFFER_PARAMETER_BUFFER_SIZE, value.count);
						GClass1262.computeRuntimeModification.SetVector(GClass1262.GClass1269.BUFFER_PARAMETER_POSITION_OFFSET, offsetPosition);
						GClass1262.computeRuntimeModification.Dispatch(GClass1262.computeBufferTransformOffsetId, Mathf.CeilToInt((float)value.count / GClass1262.COMPUTE_SHADER_THREAD_COUNT), 1, 1);
					}
				}
			}
			if (!GClass1262.DETAIL_STORE_INSTANCE_DATA || cell.detailInstanceList == null)
			{
				continue;
			}
			foreach (Matrix4x4[] value2 in cell.detailInstanceList.Values)
			{
				for (int i = 0; i < value2.Length; i++)
				{
					value2[i].SetColumn(3, value2[i].GetColumn(3) + new Vector4(offsetPosition.x, offsetPosition.y, offsetPosition.z, 0f));
				}
			}
		}
	}

	public static int smethod_3(int value, int max, int failValue)
	{
		if (value >= max)
		{
			return failValue;
		}
		return value;
	}

	public static Matrix4x4[] GetInstanceDataForDetailPrototype(GPUInstancerDetailPrototype detailPrototype, int[] detailMap, float[] heightMapData, int detailMapSize, int heightMapSize, int detailResolution, int heightResolution, Vector3 startPosition, Vector3 terrainSize, int instanceCount)
	{
		Matrix4x4[] array = new Matrix4x4[instanceCount];
		if (instanceCount == 0)
		{
			return array;
		}
		System.Random prng = new System.Random();
		float num = ((float)heightResolution - 1f) / (float)detailResolution;
		int max = heightMapSize * heightMapSize;
		float num2 = terrainSize.x / (float)detailResolution;
		float num3 = terrainSize.z / (float)detailResolution;
		float num4 = (float)heightResolution / (terrainSize.x / terrainSize.y);
		Vector3 zero = Vector3.zero;
		Quaternion identity = Quaternion.identity;
		Vector3 zero2 = Vector3.zero;
		Vector3 vector = new Vector3(0f, 0f, 1f);
		Vector3 vector2 = new Vector3(1f, 0f, 0f);
		Vector3 zero3 = Vector3.zero;
		int num5 = 0;
		Vector3 pos = default(Vector3);
		for (int i = 0; i < detailMapSize; i++)
		{
			for (int j = 0; j < detailMapSize; j++)
			{
				for (int k = 0; k < detailMap[i * detailMapSize + j]; k++)
				{
					pos.x = (float)j + GClass1274.Range(prng, 0f, 0.99f);
					pos.y = 0f;
					pos.z = (float)i + GClass1274.Range(prng, 0f, 0.99f);
					float num6 = pos.x * num;
					float num7 = pos.z * num;
					int num8 = Mathf.FloorToInt(num6) + Mathf.FloorToInt(num7) * heightMapSize;
					float num9 = heightMapData[num8];
					float num10 = heightMapData[smethod_3(num8 + heightMapSize, max, num8)];
					float num11 = heightMapData[num8 + 1];
					float rightTopH = heightMapData[smethod_3(num8 + heightMapSize + 1, max, num8)];
					pos.x *= num2;
					pos.y = GClass1274.SampleTerrainHeight(num6 - Mathf.Floor(num6), num7 - Mathf.Floor(num7), num9, num10, num11, rightTopH) * terrainSize.y;
					pos.z *= num3;
					pos += startPosition;
					zero2.y = num9 * num4;
					vector.y = num10 * num4;
					zero2.y = num11 * num4;
					zero3 = Vector3.Cross(vector - vector2, vector2 - zero2).normalized;
					identity.SetFromToRotation(Vector3.up, zero3);
					identity *= Quaternion.AngleAxis(GClass1274.Range(prng, 0f, 360f), Vector3.up);
					float num12 = GClass1274.Range(prng, 0f, 1f);
					float num13 = detailPrototype.detailScale.x + (detailPrototype.detailScale.y - detailPrototype.detailScale.x) * num12;
					float y = detailPrototype.detailScale.z + (detailPrototype.detailScale.w - detailPrototype.detailScale.z) * num12;
					zero.x = num13;
					zero.y = y;
					zero.z = num13;
					array[num5].SetTRS(pos, identity, zero);
					num5++;
				}
			}
		}
		return array;
	}

	public Matrix4x4[] method_2(GPUInstancerDetailPrototype detailPrototype, int[] detailMap, float[] heightMapData, int heightMapSize, int heightResolution, Vector3 startPosition, Vector3 terrainSize, int instanceCount, ComputeShader grassInstantiationComputeShader, GPUInstancerTerrainSettings terrainSettings)
	{
		Matrix4x4[] array = new Matrix4x4[instanceCount];
		if (instanceCount == 0)
		{
			return array;
		}
		int num = detailPrototype.detailResolution / spData.cellRowAndCollumnCountPerTerrain;
		int grassInstantiationComputeKernelId = grassInstantiationComputeShader.FindKernel(GClass1262.GRASS_INSTANTIATION_KERNEL);
		ComputeBuffer computeBuffer = new ComputeBuffer(heightMapData.Length, GClass1262.STRIDE_SIZE_INT);
		computeBuffer.SetData(heightMapData);
		ComputeBuffer computeBuffer2 = new ComputeBuffer(instanceCount, GClass1262.STRIDE_SIZE_MATRIX4X4);
		computeBuffer_1.SetData(int_2);
		ComputeBuffer computeBuffer3 = new ComputeBuffer(Mathf.CeilToInt(num * num), GClass1262.STRIDE_SIZE_INT);
		computeBuffer3.SetData(detailMap);
		method_4(grassInstantiationComputeShader, grassInstantiationComputeKernelId, computeBuffer2, computeBuffer3, computeBuffer, new Vector4(num, num, heightMapSize, heightMapSize), startPosition, terrainSize, detailPrototype.detailResolution, heightResolution, detailPrototype.detailScale, terrainSettings.GetHealthyDryNoiseTexture(detailPrototype), detailPrototype.noiseSpread, detailPrototype.GetInstanceID(), detailPrototype.detailDensity, detailPrototype.detailGrowDirection);
		computeBuffer3.Release();
		computeBuffer2.GetData(array);
		computeBuffer2.Release();
		computeBuffer.Release();
		return array;
	}

	public ComputeBuffer method_3(GPUInstancerDetailPrototype detailPrototype, int heightMapSize, int heightResolution, Vector3 startPosition, Vector3 terrainSize, int instanceCount, ComputeShader grassInstantiationComputeShader, GPUInstancerTerrainSettings terrainSettings, ComputeBuffer heightMapBuffer, ComputeBuffer detailMapBuffer)
	{
		if (instanceCount == 0)
		{
			return null;
		}
		int num = detailPrototype.detailResolution / spData.cellRowAndCollumnCountPerTerrain;
		int grassInstantiationComputeKernelId = grassInstantiationComputeShader.FindKernel(GClass1262.GRASS_INSTANTIATION_KERNEL);
		ComputeBuffer computeBuffer = new ComputeBuffer(instanceCount, GClass1262.STRIDE_SIZE_MATRIX4X4);
		computeBuffer_1.SetData(int_2);
		int hashCode = detailPrototype.name.GetHashCode();
		method_4(grassInstantiationComputeShader, grassInstantiationComputeKernelId, computeBuffer, detailMapBuffer, heightMapBuffer, new Vector4(num, num, heightMapSize, heightMapSize), startPosition, terrainSize, detailPrototype.detailResolution, heightResolution, detailPrototype.detailScale, terrainSettings.GetHealthyDryNoiseTexture(detailPrototype), detailPrototype.noiseSpread, hashCode, detailPrototype.detailDensity, detailPrototype.detailGrowDirection);
		return computeBuffer;
	}

	public void method_4(ComputeShader grassComputeShader, int grassInstantiationComputeKernelId, ComputeBuffer visibilityBuffer, ComputeBuffer detailMapBuffer, ComputeBuffer heightMapBuffer, Vector4 detailAndHeightMapSize, Vector3 startPosition, Vector3 terrainSize, float detailResolution, int heightResolution, Vector4 detailScale, Texture healthyDryNoiseTexture, float noiseSpread, int instanceID, float detailDensity, float detailTerrainNormal)
	{
		grassComputeShader.SetBuffer(grassInstantiationComputeKernelId, GClass1262.GClass1264.INSTANCE_DATA_BUFFER, visibilityBuffer);
		grassComputeShader.SetBuffer(grassInstantiationComputeKernelId, GClass1262.GClass1267.DETAIL_MAP_DATA_BUFFER, detailMapBuffer);
		grassComputeShader.SetBuffer(grassInstantiationComputeKernelId, GClass1262.GClass1267.HEIGHT_MAP_DATA_BUFFER, heightMapBuffer);
		grassComputeShader.SetBuffer(grassInstantiationComputeKernelId, GClass1262.GClass1267.COUNTER_BUFFER, computeBuffer_1);
		grassComputeShader.SetFloat(GClass1262.GClass1267.TERRAIN_DETAIL_RESOLUTION_DATA, detailResolution);
		grassComputeShader.SetInt(GClass1262.GClass1267.TERRAIN_HEIGHT_RESOLUTION_DATA, heightResolution);
		grassComputeShader.SetVector(GClass1262.GClass1267.GRASS_START_POSITION_DATA, startPosition);
		grassComputeShader.SetVector(GClass1262.GClass1267.TERRAIN_SIZE_DATA, terrainSize);
		grassComputeShader.SetVector(GClass1262.GClass1267.DETAIL_SCALE_DATA, detailScale);
		grassComputeShader.SetVector(GClass1262.GClass1267.DETAIL_AND_HEIGHT_MAP_SIZE_DATA, detailAndHeightMapSize);
		if (healthyDryNoiseTexture != null)
		{
			grassComputeShader.SetTexture(grassInstantiationComputeKernelId, GClass1262.GClass1267.HEALTHY_DRY_NOISE_TEXTURE, healthyDryNoiseTexture);
			grassComputeShader.SetFloat(GClass1262.GClass1267.NOISE_SPREAD, noiseSpread);
		}
		grassComputeShader.SetFloat(GClass1262.GClass1267.DETAIL_UNIQUE_VALUE, (float)instanceID / 1000f);
		grassComputeShader.SetFloat(GClass1262.GClass1267.DETAIL_DENSITY, detailDensity);
		grassComputeShader.SetFloat(GClass1262.GClass1267.DETAIL_TERRAIN_NORMAL, detailTerrainNormal);
		grassComputeShader.Dispatch(grassInstantiationComputeKernelId, Mathf.CeilToInt(detailAndHeightMapSize.x / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D), 1, Mathf.CeilToInt(detailAndHeightMapSize.y / GClass1262.COMPUTE_SHADER_THREAD_COUNT_2D));
	}

	public void method_5(IEnumerator routine)
	{
		if (Application.isPlaying)
		{
			StartCoroutine(routine);
		}
		else
		{
			while (routine.MoveNext())
			{
			}
		}
	}

	public override void InitializeSpatialPartitioning()
	{
		base.InitializeSpatialPartitioning();
		GClass1274.ReleaseSPBuffers(spData);
		spData = new GClass1273<GClass1258>();
		GClass1274.CalculateSpatialPartitioningValuesFromTerrain(spData, base.terrain, terrainSettings.maxDetailDistance, (!terrainSettings.autoSPCellSize) ? terrainSettings.preferedSPCellSize : 0);
		method_8();
	}

	public IEnumerator method_6()
	{
		if (initalizingInstances)
		{
			yield break;
		}
		List<GClass1270> list = new List<GClass1270>(runtimeDataList);
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			GClass1270 gClass = list[i];
			if (spData.activeCellList != null && spData.activeCellList.Count > 0)
			{
				int num3 = 0;
				foreach (GClass1259 activeCell in spData.activeCellList)
				{
					if (activeCell != null && activeCell.detailInstanceList != null)
					{
						num3 += activeCell.detailInstanceList[i].Length;
					}
				}
				gClass.bufferSize = num3;
				gClass.instanceCount = num3;
				if (num3 == 0)
				{
					gClass.transformationMatrixVisibilityBuffer?.Release();
					gClass.transformationMatrixVisibilityBuffer = null;
					continue;
				}
				computeBuffer_0 = new ComputeBuffer(num3, GClass1262.STRIDE_SIZE_MATRIX4X4);
				int num4 = 0;
				for (int j = 0; j < spData.activeCellList.Count; j++)
				{
					GClass1259 gClass3 = (GClass1259)spData.activeCellList[j];
					for (int k = 0; (float)k < Mathf.Ceil((float)gClass3.detailInstanceList[i].Length / (float)GClass1262.BUFFER_COROUTINE_STEP_NUMBER); k++)
					{
						int num5 = k * GClass1262.BUFFER_COROUTINE_STEP_NUMBER;
						int num6 = GClass1262.BUFFER_COROUTINE_STEP_NUMBER;
						if (num5 + num6 > gClass3.detailInstanceList[i].Length)
						{
							num6 = gClass3.detailInstanceList[i].Length - num5;
						}
						GClass1274.SetDataPartial(computeBuffer_0, gClass3.detailInstanceList[i], num5, num4, num6);
						num4 += num6;
						num += num6;
						if (num6 + num5 < gClass3.detailInstanceList[i].Length - 1 && num - num2 > GClass1262.BUFFER_COROUTINE_STEP_NUMBER)
						{
							num2 = num;
							yield return null;
						}
					}
					if (initalizingInstances)
					{
						break;
					}
				}
				if (initalizingInstances)
				{
					break;
				}
				gClass.transformationMatrixVisibilityBuffer?.Release();
				gClass.transformationMatrixVisibilityBuffer = computeBuffer_0;
			}
			if (initalizingInstances)
			{
				break;
			}
			GClass1274.InitializeGPUBuffer(gClass);
			num2 = num;
			yield return null;
		}
		if (initalizingInstances)
		{
			computeBuffer_0?.Release();
			GClass1274.ReleaseInstanceBuffers(list);
			GClass1274.ClearInstanceData(list);
		}
		computeBuffer_0 = null;
		replacingInstances = false;
		if (!initalizingInstances)
		{
			if (bool_0)
			{
				GClass1274.TriggerEvent(GPUInstancerEventType.DetailInitializationFinished);
			}
			bool_0 = false;
			isInitial = true;
		}
	}

	public IEnumerator method_7()
	{
		if (initalizingInstances)
		{
			yield break;
		}
		List<GClass1270> list = new List<GClass1270>(runtimeDataList);
		int heightMapSize = (base.terrain.terrainData.heightmapResolution - 1) / spData.cellRowAndCollumnCountPerTerrain + 1;
		int heightmapResolution = base.terrain.terrainData.heightmapResolution;
		Vector3 size = base.terrain.terrainData.size;
		ComputeBuffer computeBuffer = null;
		ComputeBuffer computeBuffer2 = null;
		GClass1259 gClass = null;
		if (spData.activeCellList != null && spData.activeCellList.Count > 0)
		{
			for (int i = 0; i < spData.activeCellList.Count; i++)
			{
				GClass1259 gClass2 = (GClass1259)spData.activeCellList[i];
				if (gClass2?.totalDetailCounts == null)
				{
					continue;
				}
				if (gClass2.detailInstanceBuffers == null)
				{
					gClass2.detailInstanceBuffers = new Dictionary<int, ComputeBuffer>();
				}
				for (int j = 0; j < gClass2.totalDetailCounts.Count; j++)
				{
					if (gClass2.totalDetailCounts[j] <= 0 || (gClass2.detailInstanceBuffers.ContainsKey(j) && gClass2.detailInstanceBuffers[j] != null))
					{
						continue;
					}
					if (gClass != gClass2)
					{
						if (computeBuffer == null)
						{
							computeBuffer = new ComputeBuffer(gClass2.heightMapData.Length, GClass1262.STRIDE_SIZE_INT);
						}
						computeBuffer.SetData(gClass2.heightMapData);
						gClass = gClass2;
					}
					GPUInstancerDetailPrototype gPUInstancerDetailPrototype = (GPUInstancerDetailPrototype)prototypeList[j];
					int num = gPUInstancerDetailPrototype.detailResolution / spData.cellRowAndCollumnCountPerTerrain;
					computeBuffer2?.Release();
					computeBuffer2 = new ComputeBuffer(num * num, GClass1262.STRIDE_SIZE_INT);
					computeBuffer2.SetData(gClass2.detailMapData[j]);
					gClass2.detailInstanceBuffers[j] = method_3(gPUInstancerDetailPrototype, heightMapSize, heightmapResolution, gClass2.instanceStartPosition, size, gClass2.totalDetailCounts[j], computeShader_0, terrainSettings, computeBuffer, computeBuffer2);
				}
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			GClass1270 gClass3 = list[k];
			if (spData.activeCellList != null && spData.activeCellList.Count > 0)
			{
				int num2 = 0;
				for (int l = 0; l < spData.activeCellList.Count; l++)
				{
					GClass1259 gClass4 = (GClass1259)spData.activeCellList[l];
					if (gClass4?.totalDetailCounts != null)
					{
						num2 += gClass4.totalDetailCounts[k];
					}
				}
				gClass3.bufferSize = num2;
				gClass3.instanceCount = num2;
				if (num2 == 0)
				{
					gClass3.transformationMatrixVisibilityBuffer?.Release();
					gClass3.transformationMatrixVisibilityBuffer = null;
					continue;
				}
				computeBuffer_0 = new ComputeBuffer(num2, GClass1262.STRIDE_SIZE_MATRIX4X4);
				int num3 = 0;
				for (int m = 0; m < spData.activeCellList.Count; m++)
				{
					GClass1259 gClass5 = (GClass1259)spData.activeCellList[m];
					if (gClass5.detailInstanceBuffers.ContainsKey(k) && gClass5.detailInstanceBuffers[k] != null)
					{
						GClass1274.CopyComputeBuffer(computeBuffer_0, num3, gClass5.detailInstanceBuffers[k].count, gClass5.detailInstanceBuffers[k]);
						num3 += gClass5.detailInstanceBuffers[k].count;
					}
				}
				gClass3.transformationMatrixVisibilityBuffer?.Release();
				gClass3.transformationMatrixVisibilityBuffer = computeBuffer_0;
			}
			GClass1274.InitializeGPUBuffer(gClass3);
		}
		computeBuffer?.Release();
		computeBuffer2?.Release();
		computeBuffer_0 = null;
		replacingInstances = false;
		if (bool_0)
		{
			GClass1274.TriggerEvent(GPUInstancerEventType.DetailInitializationFinished);
		}
		bool_0 = false;
	}

	public void method_8()
	{
		if (computeBuffer_1 == null)
		{
			computeBuffer_1 = new ComputeBuffer(1, GClass1262.STRIDE_SIZE_INT);
		}
		method_5(FillCellsDetailData(base.terrain));
	}

	public void FillCellsDetailDataCallBack()
	{
		ClearCompletedThreads();
		if (threadHeightMapData != null && (!runInThreads || activeThreads.Count <= 0))
		{
			threadHeightMapData = null;
			threadDetailMapData = null;
			if (GClass1262.DETAIL_STORE_INSTANCE_DATA)
			{
				int heightMapSize = (base.terrain.terrainData.heightmapResolution - 1) / spData.cellRowAndCollumnCountPerTerrain + 1;
				int heightmapResolution = base.terrain.terrainData.heightmapResolution;
				Vector3 size = base.terrain.terrainData.size;
				method_5(method_10(spData, prototypeList, heightMapSize, heightmapResolution, size, terrainSettings, method_9));
			}
			else
			{
				method_9();
			}
		}
	}

	public void method_9()
	{
		initalizingInstances = false;
		foreach (GClass1258 activeCell in spData.activeCellList)
		{
			activeCell.isActive = false;
		}
		spData.activeCellList.Clear();
		bool_0 = true;
	}

	public IEnumerator method_10(GClass1273<GClass1258> spData, List<GPUInstancerPrototype> prototypeList, int heightMapSize, int heightmapResolution, Vector3 terrainSize, GPUInstancerTerrainSettings terrainSettings, Action callback)
	{
		int num = 0;
		foreach (GClass1259 cell in spData.GetCellList())
		{
			if (cell.detailMapData == null)
			{
				continue;
			}
			cell.detailInstanceList = new Dictionary<int, Matrix4x4[]>();
			for (int i = 0; i < prototypeList.Count; i++)
			{
				num += cell.totalDetailCounts[i];
				cell.detailInstanceList[i] = method_2((GPUInstancerDetailPrototype)prototypeList[i], cell.detailMapData[i], cell.heightMapData, heightMapSize, heightmapResolution, cell.instanceStartPosition, terrainSize, cell.totalDetailCounts[i], computeShader_0, terrainSettings);
				if (num >= GClass1262.BUFFER_COROUTINE_STEP_NUMBER)
				{
					num = 0;
					yield return null;
				}
			}
		}
		callback();
	}

	public int[,] method_11(int layer, float density, int resizeCount)
	{
		int detailResolution = base.terrain.terrainData.detailResolution;
		int[,] array = base.terrain.terrainData.GetDetailLayer(0, 0, detailResolution, detailResolution, layer);
		if (density == 1f)
		{
			return array;
		}
		float density2 = Mathf.Pow(density, 1f / (float)resizeCount);
		for (int i = 0; i < resizeCount; i++)
		{
			array = method_12(array, density2);
		}
		return array;
	}

	public int[,] method_12(int[,] map, float density)
	{
		int length = map.GetLength(0);
		int num = Mathf.RoundToInt((float)length * density);
		int[,] array = new int[num, num];
		float[,] array2 = new float[num, num];
		float_0 = 0f;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				float num2 = 1f * (float)i / (float)num;
				float num3 = 1f * (float)j / (float)num;
				float num4 = num2 * (float)length;
				float num5 = num3 * (float)length;
				float num6 = 1f * (float)(i + 1) / (float)num;
				float num7 = 1f * (float)(j + 1) / (float)num;
				float num8 = num6 * (float)length;
				float num9 = num7 * (float)length;
				int num10 = 8;
				float num11 = (num8 - num4) / 8f;
				float num12 = (num9 - num5) / 8f;
				for (int k = 0; k < num10; k++)
				{
					for (int l = 0; l < num10; l++)
					{
						int num13 = Mathf.Clamp(Mathf.FloorToInt(num4 + num11 * (float)k), 0, length - 1);
						int num14 = Mathf.Clamp(Mathf.FloorToInt(num5 + num12 * (float)l), 0, length - 1);
						float_0 += map[num13, num14];
					}
				}
				array2[i, j] = float_0 / (float)(num10 * num10);
				float_0 = 0f;
				array[i, j] = Mathf.CeilToInt(array2[i, j]);
			}
		}
		return array;
	}

	public IEnumerator FillCellsDetailData(Terrain terrain)
	{
		threadHeightResolution = terrain.terrainData.heightmapResolution;
		if (threadHeightMapData == null)
		{
			threadHeightMapData = terrain.terrainData.GetHeights(0, 0, threadHeightResolution, threadHeightResolution);
		}
		if (threadDetailMapData == null)
		{
			threadDetailMapData = new List<int[,]>();
			for (int i = 0; i < prototypeList.Count; i++)
			{
				GPUInstancerDetailPrototype gPUInstancerDetailPrototype = prototypeList[i] as GPUInstancerDetailPrototype;
				if (!(gPUInstancerDetailPrototype == null))
				{
					int[,] array = method_13(gPUInstancerDetailPrototype.cachedDensityMapForInstance);
					if (array == null || array.GetLength(0) == 0)
					{
						Debug.LogWarning(gPUInstancerDetailPrototype.name + " cachedDensityMap not found; Realtime generating...");
						float density = Mathf.Floor(Mathf.Sqrt(gPUInstancerDetailPrototype.detailDensity) * 16f) * 0.0625f;
						int resizeDensityCount = terrainSettings.resizeDensityCount;
						array = method_11(i, density, resizeDensityCount);
					}
					threadDetailMapData.Add(array);
					gPUInstancerDetailPrototype.detailResolution = array.GetLength(0);
					if (runInThreads && i % 3 == 0)
					{
						yield return null;
					}
				}
			}
		}
		if (runInThreads)
		{
			ParameterizedThreadStart start = FillCellsDetailDataThread;
			Thread thread = new Thread(start);
			thread.IsBackground = true;
			Vector4 zero = Vector4.zero;
			zero.z = Mathf.CeilToInt((float)spData.cellRowAndCollumnCountPerTerrain / 2f);
			zero.w = spData.cellRowAndCollumnCountPerTerrain;
			threadStartQueue.Enqueue(new GClass1261
			{
				thread = thread,
				parameter = zero
			});
			if (spData.cellRowAndCollumnCountPerTerrain > 1)
			{
				thread = new Thread(start);
				thread.IsBackground = true;
				zero.x = zero.z;
				zero.z = spData.cellRowAndCollumnCountPerTerrain;
				threadStartQueue.Enqueue(new GClass1261
				{
					thread = thread,
					parameter = zero
				});
			}
		}
		else
		{
			Vector4 coord = new Vector4(0f, 0f, spData.cellRowAndCollumnCountPerTerrain, spData.cellRowAndCollumnCountPerTerrain);
			method_5(FillCellsDetailDataCoroutine(coord));
		}
	}

	public void FillCellsDetailDataThread(object parameter)
	{
		try
		{
			Vector4 obj = (Vector4)parameter;
			int num = (int)obj.x;
			int num2 = (int)obj.y;
			int num3 = (int)obj.z;
			int num4 = (int)obj.w;
			int num5 = (threadHeightResolution - 1) / spData.cellRowAndCollumnCountPerTerrain + 1;
			GClass1258 cell = null;
			for (int i = num2; i < num4; i++)
			{
				for (int j = num; j < num3; j++)
				{
					int hash = GClass1258.CalculateHash(j, 0, i);
					spData.GetCell(hash, out cell);
					if (cell != null)
					{
						GClass1259 gClass = (GClass1259)cell;
						gClass.heightMapData = GClass1274.MirrorAndFlatten(threadHeightMapData, gClass.coordX * (num5 - 1), gClass.coordZ * (num5 - 1), num5, num5);
						gClass.detailMapData = new List<int[]>();
						gClass.totalDetailCounts = new List<int>();
						for (int k = 0; k < threadDetailMapData.Count; k++)
						{
							int num6 = threadDetailMapData[k].GetLength(0) / spData.cellRowAndCollumnCountPerTerrain;
							int[] array = GClass1274.MirrorAndFlatten(threadDetailMapData[k], gClass.coordX * num6, gClass.coordZ * num6, num6, num6);
							gClass.detailMapData.Add(array);
							int item = array.Sum();
							gClass.totalDetailCounts.Add(item);
						}
						continue;
					}
					throw new Exception("Can not find cell!");
				}
			}
			threadQueue.Enqueue(FillCellsDetailDataCallBack);
		}
		catch (Exception ex)
		{
			threadException = ex;
			threadQueue.Enqueue(base.LogThreadException);
		}
	}

	public IEnumerator FillCellsDetailDataCoroutine(Vector4 coord)
	{
		int num = (int)coord.x;
		int num2 = (int)coord.y;
		int num3 = (int)coord.z;
		int num4 = (int)coord.w;
		int num5 = (threadHeightResolution - 1) / spData.cellRowAndCollumnCountPerTerrain + 1;
		GClass1258 cell = null;
		for (int i = num2; i < num4; i++)
		{
			for (int j = num; j < num3; j++)
			{
				int hash = GClass1258.CalculateHash(j, 0, i);
				spData.GetCell(hash, out cell);
				if (cell == null)
				{
					Debug.LogError("Can not find cell!");
					continue;
				}
				GClass1259 gClass = (GClass1259)cell;
				gClass.heightMapData = GClass1274.MirrorAndFlatten(threadHeightMapData, gClass.coordX * (num5 - 1), gClass.coordZ * (num5 - 1), num5, num5);
				gClass.detailMapData = new List<int[]>();
				gClass.totalDetailCounts = new List<int>();
				for (int k = 0; k < threadDetailMapData.Count; k++)
				{
					int num6 = threadDetailMapData[k].GetLength(0) / spData.cellRowAndCollumnCountPerTerrain;
					int[] array = GClass1274.MirrorAndFlatten(threadDetailMapData[k], gClass.coordX * num6, gClass.coordZ * num6, num6, num6);
					gClass.detailMapData.Add(array);
					int item = array.Sum();
					gClass.totalDetailCounts.Add(item);
				}
				yield return null;
			}
		}
		FillCellsDetailDataCallBack();
	}

	public void SetDetailMapData(List<int[,]> detailMapData)
	{
		threadDetailMapData = detailMapData;
	}

	public int[,] GetDetailLayer(int layer)
	{
		if (isInitialized && !(base.terrain == null) && spData != null)
		{
			int detailResolution = base.terrain.terrainData.detailResolution;
			int num = detailResolution / spData.cellRowAndCollumnCountPerTerrain;
			int[,] array = new int[detailResolution, detailResolution];
			for (int i = 0; i < spData.cellRowAndCollumnCountPerTerrain; i++)
			{
				for (int j = 0; j < spData.cellRowAndCollumnCountPerTerrain; j++)
				{
					if (!spData.GetCell(GClass1258.CalculateHash(i, 0, j), out var cell))
					{
						continue;
					}
					GClass1259 gClass = (GClass1259)cell;
					if (gClass.detailMapData == null)
					{
						continue;
					}
					for (int k = 0; k < num; k++)
					{
						for (int l = 0; l < num; l++)
						{
							array[l + j * num, k + i * num] = gClass.detailMapData[layer][k + l * num];
						}
					}
				}
			}
			return array;
		}
		return null;
	}

	public List<int[,]> GetDetailMapData()
	{
		if (isInitialized && !(base.terrain == null) && spData != null)
		{
			List<int[,]> list = new List<int[,]>();
			for (int i = 0; i < prototypeList.Count; i++)
			{
				list.Add(GetDetailLayer(i));
			}
			return list;
		}
		return null;
	}

	public int[,] method_13(int[] array)
	{
		if (array != null && array.Length != 0)
		{
			int num = Mathf.FloorToInt(Mathf.Sqrt(array.Length));
			int[,] array2 = new int[num, num];
			for (int i = 0; i < num; i++)
			{
				for (int j = 0; j < num; j++)
				{
					array2[i, j] = array[i * num + j];
				}
			}
			return array2;
		}
		return new int[0, 0];
	}

	public int[] method_14(int[,] array)
	{
		int length = array.GetLength(0);
		int[] array2 = new int[length * length];
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length; j++)
			{
				array2[i * length + j] = array[i, j];
			}
		}
		return array2;
	}
}
