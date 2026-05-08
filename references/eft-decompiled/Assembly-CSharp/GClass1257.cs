using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GPUInstancer;
using UnityEngine;
using UnityEngine.Events;

public abstract class GClass1257
{
	[CompilerGenerated]
	public class Class874
	{
		public Camera camera;

		public void method_0(GPUInstancerManager m)
		{
			m.SetCamera(camera);
		}
	}

	public static void InitializeGPUInstancer(GPUInstancerManager manager, bool forceNew = true)
	{
		manager.InitializeRuntimeDataAndBuffers(forceNew);
	}

	public static void SetCamera(GPUInstancerManager manager, Camera camera)
	{
		manager.SetCamera(camera);
	}

	public static void SetCamera(Camera camera)
	{
		if (GPUInstancerManager.activeManagerList != null)
		{
			GPUInstancerManager.activeManagerList.ForEach(delegate(GPUInstancerManager m)
			{
				m.SetCamera(camera);
			});
		}
	}

	public static List<GPUInstancerManager> GetActiveManagers()
	{
		if (GPUInstancerManager.activeManagerList != null)
		{
			return GPUInstancerManager.activeManagerList.ToList();
		}
		return null;
	}

	public static void StartListeningGPUIEvent(GPUInstancerEventType eventType, UnityAction callback)
	{
		GClass1274.StartListening(eventType, callback);
	}

	public static void StopListeningGPUIEvent(GPUInstancerEventType eventType, UnityAction callback)
	{
		GClass1274.StopListening(eventType, callback);
	}

	public static void SetGlobalPositionOffset(GPUInstancerManager manager, Vector3 offsetPosition)
	{
		GClass1274.SetGlobalPositionOffset(manager, offsetPosition);
	}

	public static void RemoveInstancesInsideBounds(GPUInstancerManager manager, Bounds bounds, float offset = 0f, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		manager.RemoveInstancesInsideBounds(bounds, offset, prototypeFilter);
	}

	public static void RemoveInstancesInsideCollider(GPUInstancerManager manager, Collider collider, float offset = 0f, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		manager.RemoveInstancesInsideCollider(collider, offset, prototypeFilter);
	}

	public static ComputeBuffer GetTransformDataBuffer(GPUInstancerManager manager, GPUInstancerPrototype prototype)
	{
		return manager.GetTransformDataBuffer(prototype);
	}

	public static void SetLODBias(GPUInstancerManager manager, float newLODBias, List<GPUInstancerPrototype> prototypeFilter = null)
	{
		manager.SetLODBias(newLODBias, prototypeFilter);
	}

	public static void ChangeMaterial(GPUInstancerManager manager, GPUInstancerPrototype prototype, Material material, int lodLevel = 0, int rendererIndex = 0, int subMeshIndex = 0)
	{
		GClass1270 runtimeData = manager.GetRuntimeData(prototype, logError: true);
		if (runtimeData != null)
		{
			GClass1272 gClass = runtimeData.instanceLODs[lodLevel].renderers[rendererIndex];
			GameObject gameObject = new GameObject("ProxyGO");
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshFilter.mesh = gClass.mesh;
			gClass.materials[subMeshIndex] = GClass1262.gpuiSettings.shaderBindings.GetInstancedMaterial(material);
			meshRenderer.materials[subMeshIndex] = material;
			meshRenderer.GetPropertyBlock(gClass.mpb);
			if (gClass.shadowMPB != null)
			{
				meshRenderer.GetPropertyBlock(gClass.shadowMPB);
			}
			Object.Destroy(gameObject);
			GClass1274.SetAppendBuffers(runtimeData);
		}
	}

	public static void ChangeMesh(GPUInstancerManager manager, GPUInstancerPrototype prototype, Mesh mesh, int lodLevel = 0, int rendererIndex = 0, int subMeshIndex = 0)
	{
		GClass1270 runtimeData = manager.GetRuntimeData(prototype, logError: true);
		if (runtimeData == null)
		{
			return;
		}
		GClass1272 gClass = runtimeData.instanceLODs[lodLevel].renderers[rendererIndex];
		if (gClass.mesh.subMeshCount != mesh.subMeshCount)
		{
			Debug.LogError("ChangeMesh method can not be used with a mesh that has different amount of submeshes than the original mesh.");
			return;
		}
		if (gClass.mesh.vertexCount != mesh.vertexCount)
		{
			int argsBufferOffset = gClass.argsBufferOffset;
			for (int i = 0; i < gClass.mesh.subMeshCount; i++)
			{
				runtimeData.args[argsBufferOffset++] = gClass.mesh.GetIndexCount(i);
				runtimeData.args[argsBufferOffset++] = 0u;
				runtimeData.args[argsBufferOffset++] = gClass.mesh.GetIndexStart(i);
				runtimeData.args[argsBufferOffset++] = 0u;
				runtimeData.args[argsBufferOffset++] = 0u;
			}
			runtimeData.argsBuffer.SetData(runtimeData.args);
		}
		gClass.mesh = mesh;
	}

	public static void SetInstanceCount(GPUInstancerManager manager, GPUInstancerPrototype prototype, int instanceCount)
	{
		GClass1270 runtimeData = manager.GetRuntimeData(prototype, logError: true);
		if (runtimeData != null)
		{
			if (instanceCount > runtimeData.bufferSize)
			{
				Debug.LogError("Instance count can not be higher than the buffer size.");
			}
			else
			{
				runtimeData.instanceCount = instanceCount;
			}
		}
	}

	public static Matrix4x4[] GetInstanceDataArray(GPUInstancerManager manager, GPUInstancerPrototype prototype)
	{
		return manager.GetRuntimeData(prototype, logError: true)?.instanceDataArray;
	}

	public static List<GPUInstancerPrototype> GetPrototypeList(GPUInstancerManager manager)
	{
		return manager.prototypeList.ToList();
	}

	public static void RegisterPrefabInstanceList(GPUInstancerPrefabManager manager, IEnumerable<GPUInstancerPrefab> prefabInstanceList)
	{
		manager.RegisterPrefabInstanceList(prefabInstanceList);
	}

	public static void UnregisterPrefabInstanceList(GPUInstancerPrefabManager manager, IEnumerable<GPUInstancerPrefab> prefabInstanceList)
	{
		manager.UnregisterPrefabInstanceList(prefabInstanceList);
	}

	public static void ClearRegisteredPrefabInstances(GPUInstancerPrefabManager manager)
	{
		manager.ClearRegisteredPrefabInstances();
	}

	public static void ClearRegisteredPrefabInstances(GPUInstancerPrefabManager manager, GPUInstancerPrototype prototype)
	{
		manager.ClearRegisteredPrefabInstances(prototype);
	}

	public static void AddPrefabInstance(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance)
	{
		manager.AddPrefabInstance(prefabInstance);
	}

	public static void RemovePrefabInstance(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance, bool setRenderersEnabled = true)
	{
		manager.RemovePrefabInstance(prefabInstance, setRenderersEnabled);
	}

	public static void DisableIntancingForInstance(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance, bool setRenderersEnabled = true)
	{
		manager.DisableIntancingForInstance(prefabInstance, setRenderersEnabled);
	}

	public static void EnableInstancingForInstance(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance, bool setRenderersDisabled = true)
	{
		manager.EnableInstancingForInstance(prefabInstance, setRenderersDisabled);
	}

	public static void UpdateTransformDataForInstance(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance)
	{
		manager.UpdateTransformDataForInstance(prefabInstance);
	}

	public static void DefinePrototypeVariationBuffer<T>(GPUInstancerPrefabManager manager, GPUInstancerPrefabPrototype prototype, string bufferName)
	{
		manager.DefinePrototypeVariationBuffer<T>(prototype, bufferName);
	}

	public static void AddVariation<T>(GPUInstancerPrefab prefabInstance, string bufferName, T value)
	{
		prefabInstance.AddVariation(bufferName, value);
	}

	public static void UpdateVariation<T>(GPUInstancerPrefabManager manager, GPUInstancerPrefab prefabInstance, string bufferName, T value)
	{
		prefabInstance.AddVariation(bufferName, value);
		manager.UpdateVariationData(prefabInstance, bufferName, value);
	}

	public static void DefineAndAddVariationFromArray<T>(GPUInstancerPrefabManager manager, GPUInstancerPrefabPrototype prototype, string bufferName, T[] variationArray)
	{
		manager.DefineAndAddVariationFromArray(prototype, bufferName, variationArray);
	}

	public static void UpdateVariationFromArray<T>(GPUInstancerPrefabManager manager, GPUInstancerPrefabPrototype prototype, string bufferName, T[] variationArray, int arrayStartIndex = 0, int bufferStartIndex = 0, int count = 0)
	{
		manager.UpdateVariationsFromArray(prototype, bufferName, variationArray, arrayStartIndex, bufferStartIndex, count);
	}

	public static void InitializeWithMatrix4x4Array(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prototype, Matrix4x4[] matrix4x4Array)
	{
		GClass1274.InitializeWithMatrix4x4Array(prefabManager, prototype, matrix4x4Array);
	}

	public static void UpdateVisibilityBufferWithMatrix4x4Array(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prototype, Matrix4x4[] matrix4x4Array, int arrayStartIndex = 0, int bufferStartIndex = 0, int count = 0)
	{
		GClass1274.UpdateVisibilityBufferWithMatrix4x4Array(prefabManager, prototype, matrix4x4Array, arrayStartIndex, bufferStartIndex, count);
	}

	public static GPUInstancerPrefabPrototype DefineGameObjectAsPrefabPrototypeAtRuntime(GPUInstancerPrefabManager prefabManager, GameObject prototypeGameObject)
	{
		return prefabManager.DefineGameObjectAsPrefabPrototypeAtRuntime(prototypeGameObject);
	}

	public static GClass1270 InitializeGPUInstancer(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prototype)
	{
		prefabManager.InitializeRuntimeDataAndBuffers(forceNew: false);
		return prefabManager.InitializeRuntimeDataForPrefabPrototype(prototype);
	}

	public static void AddInstancesToPrefabPrototypeAtRuntime(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prefabPrototype, IEnumerable<GameObject> instances)
	{
		prefabManager.AddInstancesToPrefabPrototypeAtRuntime(prefabPrototype, instances);
	}

	public static void RemovePrototypeAtRuntime(GPUInstancerPrefabManager prefabManager, GPUInstancerPrefabPrototype prefabPrototype)
	{
		prefabManager.RemovePrototypeAtRuntime(prefabPrototype);
	}

	public static void SetupManagerWithTerrain(GPUInstancerTerrainManager manager, Terrain terrain)
	{
		manager.SetupManagerWithTerrain(terrain);
	}

	public static void UpdateDetailInstances(GPUInstancerDetailManager manager, bool updateMeshes = false)
	{
		GClass1274.UpdateDetailInstanceRuntimeDataList(manager.terrain, manager.runtimeDataList, manager.terrainSettings, updateMeshes, manager.detailLayer);
	}

	public static void UpdateTerrainNormalMapDetailInstance(GPUInstancerDetailManager manager)
	{
		GClass1274.UpdateTerrainNormalMapDetailInstance(manager.terrain, manager.runtimeDataList);
	}

	public static List<int[,]> GetDetailMapData(GPUInstancerDetailManager manager)
	{
		return manager.GetDetailMapData();
	}

	public static int[,] GetDetailLayer(GPUInstancerDetailManager manager, int layerIndex)
	{
		return manager.GetDetailLayer(layerIndex);
	}

	public static void SetDetailMapData(GPUInstancerDetailManager manager, List<int[,]> detailMapData)
	{
		manager.SetDetailMapData(detailMapData);
	}
}
