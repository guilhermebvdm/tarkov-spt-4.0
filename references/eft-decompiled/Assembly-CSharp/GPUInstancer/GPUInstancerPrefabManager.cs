using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerPrefabManager : GPUInstancerManager
{
	[Serializable]
	[CompilerGenerated]
	public class Class892
	{
		public static readonly Class892 class892_0 = new Class892();

		public static Predicate<GPUInstancerPrefab> predicate_0;

		public bool method_0(GPUInstancerPrefab pi)
		{
			return pi == null;
		}
	}

	[CompilerGenerated]
	public class Class893
	{
		public GClass1270 runtimeData;

		public void method_0()
		{
			GClass1274.InitializeGPUBuffer(runtimeData);
		}

		public void method_1()
		{
			runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
		}
	}

	[CompilerGenerated]
	public class Class894
	{
		public GClass1270 runtimeData;

		public int instanceCount;

		public int bufferSize;

		public void method_0()
		{
			runtimeData.instanceCount = instanceCount;
			runtimeData.bufferSize = bufferSize;
			GClass1274.InitializeGPUBuffer(runtimeData);
		}
	}

	[CompilerGenerated]
	public class Class895<T>
	{
		public GPUInstancerPrefabPrototype prototype;

		public string bufferName;

		public bool method_0(GInterface118 v)
		{
			if (v.GetPrototype() == prototype)
			{
				return v.GetBufferName() == bufferName;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class896<T>
	{
		public GPUInstancerPrefab prefabInstance;

		public string bufferName;

		public bool method_0(GInterface118 v)
		{
			if (v.GetPrototype() == prefabInstance.prefabPrototype)
			{
				return v.GetBufferName() == bufferName;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class897<T>
	{
		public GPUInstancerPrefabPrototype prototype;

		public string bufferName;

		public bool method_0(GInterface118 v)
		{
			if (v is GClass1275<T> && ((GClass1275<T>)v).prototype == prototype)
			{
				return ((GClass1275<T>)v).bufferName == bufferName;
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class898
	{
		public GPUInstancerPrefabPrototype prefabPrototype;

		public bool method_0(RegisteredPrefabsData d)
		{
			return d.prefabPrototype == prefabPrototype;
		}
	}

	[CompilerGenerated]
	public class Class899
	{
		public GPUInstancerPrefabPrototype prefabPrototype;

		public bool method_0(RegisteredPrefabsData d)
		{
			return d.prefabPrototype == prefabPrototype;
		}
	}

	[CompilerGenerated]
	public class Class900
	{
		public GPUInstancerPrefab prefabInstance;

		public bool method_0(RegisteredPrefabsData rpd)
		{
			return rpd.prefabPrototype == prefabInstance.prefabPrototype;
		}
	}

	[SerializeField]
	public List<RegisteredPrefabsData> registeredPrefabs = new List<RegisteredPrefabsData>();

	[SerializeField]
	public List<GameObject> prefabList;

	public bool enableMROnManagerDisable = true;

	public bool enableMROnRemoveInstance = true;

	protected List<GPUInstancerModificationCollider> _modificationColliders;

	protected Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>> _registeredPrefabsRuntimeData;

	protected List<GInterface118> _variationDataList;

	protected volatile bool _addRemoveInProgress;

	public bool IsActive = true;

	private static readonly string string_0 = "_DryColorBuffer";

	private static readonly string string_1 = "_HealthyColorBuffer";

	private ComputeBuffer computeBuffer_0;

	private ComputeBuffer computeBuffer_1;

	public override bool isCulled => !IsActive;

	public override void Awake()
	{
		base.Awake();
		if (prefabList == null)
		{
			prefabList = new List<GameObject>();
		}
	}

	public override void Reset()
	{
		base.Reset();
		RegisterPrefabsInScene();
	}

	public override void Update()
	{
		base.Update();
		if (runtimeDataList == null)
		{
			return;
		}
		foreach (GClass1270 runtimeData in runtimeDataList)
		{
			if (runtimeData.prototype.autoUpdateTransformData)
			{
				foreach (GPUInstancerPrefab item in _registeredPrefabsRuntimeData[runtimeData.prototype])
				{
					Transform instanceTransform = item.GetInstanceTransform();
					if (instanceTransform.hasChanged && item.state == PrefabInstancingState.Instanced)
					{
						instanceTransform.hasChanged = false;
						runtimeData.instanceDataArray[item.gpuInstancerID - 1] = instanceTransform.localToWorldMatrix;
						runtimeData.transformDataModified = true;
					}
				}
			}
			if (runtimeData.transformDataModified)
			{
				runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
				runtimeData.transformDataModified = false;
			}
		}
	}

	public override void ClearInstancingData()
	{
		base.ClearInstancingData();
		if (Application.isPlaying && _registeredPrefabsRuntimeData != null && enableMROnManagerDisable)
		{
			foreach (GPUInstancerPrefabPrototype key in _registeredPrefabsRuntimeData.Keys)
			{
				if (key.meshRenderersDisabled)
				{
					continue;
				}
				foreach (GPUInstancerPrefab item in _registeredPrefabsRuntimeData[key])
				{
					if ((bool)item)
					{
						SetRenderersEnabled(item, enabled: true);
					}
				}
			}
		}
		if (_variationDataList == null)
		{
			return;
		}
		foreach (GInterface118 variationData in _variationDataList)
		{
			variationData.ReleaseBuffer();
		}
	}

	public void GeneratePrototypesForWinter()
	{
		base.GeneratePrototypes(forceNew: true);
	}

	public override void GeneratePrototypes(bool forceNew = false)
	{
		base.GeneratePrototypes();
		GClass1274.SetPrefabInstancePrototypes(base.gameObject, prototypeList, prefabList, forceNew);
	}

	public override void InitializeRuntimeDataAndBuffers(bool forceNew = true)
	{
		base.InitializeRuntimeDataAndBuffers(forceNew);
		if (!forceNew && isInitialized)
		{
			return;
		}
		if (_registeredPrefabsRuntimeData == null)
		{
			_registeredPrefabsRuntimeData = new Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>>();
		}
		if (registeredPrefabs != null && registeredPrefabs.Count > 0)
		{
			foreach (RegisteredPrefabsData registeredPrefab in registeredPrefabs)
			{
				if (!_registeredPrefabsRuntimeData.ContainsKey(registeredPrefab.prefabPrototype))
				{
					_registeredPrefabsRuntimeData.Add(registeredPrefab.prefabPrototype, registeredPrefab.registeredPrefabs);
				}
				else
				{
					_registeredPrefabsRuntimeData[registeredPrefab.prefabPrototype].AddRange(registeredPrefab.registeredPrefabs);
				}
			}
			registeredPrefabs.Clear();
		}
		if (_registeredPrefabsRuntimeData.Count != prototypeList.Count)
		{
			foreach (GPUInstancerPrototype prototype in prototypeList)
			{
				if (!_registeredPrefabsRuntimeData.ContainsKey(prototype))
				{
					_registeredPrefabsRuntimeData.Add(prototype, new List<GPUInstancerPrefab>());
				}
			}
		}
		method_0();
		GClass1274.InitializeGPUBuffers(runtimeDataList);
		isInitial = true;
		isInitialized = true;
	}

	public override void DeletePrototype(GPUInstancerPrototype prototype, bool removeSO = true)
	{
		base.DeletePrototype(prototype, removeSO);
		prefabList.Remove(prototype.prefabObject);
		if (removeSO)
		{
			UnityEngine.Object.DestroyImmediate(prototype.prefabObject.GetComponent<GPUInstancerPrefab>(), allowDestroyingAssets: true);
			if (prototype.prefabObject.GetComponent<GPUInstancerPrefabRuntimeHandler>() != null)
			{
				UnityEngine.Object.DestroyImmediate(prototype.prefabObject.GetComponent<GPUInstancerPrefabRuntimeHandler>(), allowDestroyingAssets: true);
			}
		}
		GeneratePrototypes();
	}

	public void method_0(int additionalBufferSize = 0)
	{
		if (runtimeDataList == null)
		{
			runtimeDataList = new List<GClass1270>();
		}
		else
		{
			GClass1274.ClearInstanceData(runtimeDataList);
		}
		if (runtimeDataDictionary == null)
		{
			runtimeDataDictionary = new Dictionary<GPUInstancerPrototype, GClass1270>();
		}
		foreach (GPUInstancerPrefabPrototype prototype in prototypeList)
		{
			InitializeRuntimeDataForPrefabPrototype(prototype, additionalBufferSize);
		}
	}

	public virtual GClass1270 InitializeRuntimeDataForPrefabPrototype(GPUInstancerPrefabPrototype p, int additionalBufferSize = 0)
	{
		if (GClass1262.gpuiSettings.isLWRP || GClass1262.gpuiSettings.isHDRP)
		{
			p.useOriginalShaderForShadow = true;
		}
		GClass1270 gClass = GetRuntimeData(p);
		if (gClass == null)
		{
			gClass = new GClass1270(p);
			if (!gClass.CreateRenderersFromGameObject(p))
			{
				return null;
			}
			runtimeDataList.Add(gClass);
			runtimeDataDictionary.Add(p, gClass);
			if (p.isShadowCasting)
			{
				gClass.hasShadowCasterBuffer = true;
				if (!p.useOriginalShaderForShadow)
				{
					gClass.shadowCasterMaterial = new Material(Shader.Find(GClass1262.SHADER_GPUI_SHADOWS_ONLY));
				}
			}
			GClass1274.AddBillboardToRuntimeData(gClass);
			if (p.treeType == GPUInstancerTreeType.SpeedTree || p.treeType == GPUInstancerTreeType.SpeedTree8 || p.treeType == GPUInstancerTreeType.TreeCreatorTree)
			{
				GPUInstancerManager.AddTreeProxy(p, gClass);
			}
		}
		int num = 0;
		List<GPUInstancerPrefab> value = null;
		if (p.isTransformsSerialized)
		{
			StringReader stringReader = new StringReader(p.serializedTransformData.text);
			List<Matrix4x4> list = new List<Matrix4x4>();
			while (true)
			{
				string text = stringReader.ReadLine();
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				list.Add(GClass1274.Matrix4x4FromString(text));
			}
			gClass.instanceDataArray = list.ToArray();
			gClass.bufferSize = gClass.instanceDataArray.Length + ((p.enableRuntimeModifications && p.addRemoveInstancesAtRuntime) ? p.extraBufferSize : 0) + additionalBufferSize;
			num = gClass.instanceDataArray.Length;
		}
		else if (_registeredPrefabsRuntimeData.TryGetValue(p, out value))
		{
			gClass.instanceDataArray = new Matrix4x4[value.Count + ((p.enableRuntimeModifications && p.addRemoveInstancesAtRuntime) ? p.extraBufferSize : 0) + additionalBufferSize];
			gClass.bufferSize = gClass.instanceDataArray.Length;
			foreach (GPUInstancerPrefab item in value)
			{
				if (!item)
				{
					continue;
				}
				Matrix4x4 matrix4x = item.GetInstanceTransform().localToWorldMatrix;
				item.GetInstanceTransform().hasChanged = false;
				item.state = PrefabInstancingState.Instanced;
				bool flag = true;
				if (item.prefabPrototype.enableRuntimeModifications && _modificationColliders != null && _modificationColliders.Count > 0)
				{
					bool flag2 = false;
					foreach (GPUInstancerModificationCollider modificationCollider in _modificationColliders)
					{
						if (modificationCollider.IsInsideCollider(item))
						{
							flag2 = true;
							modificationCollider.AddEnteredInstance(item);
							matrix4x = GClass1262.zeroMatrix;
							item.state = PrefabInstancingState.Disabled;
							flag = false;
							break;
						}
					}
					if (!flag2 && item.prefabPrototype.startWithRigidBody && item.GetComponent<Rigidbody>() != null)
					{
						flag2 = true;
						_modificationColliders[0].AddEnteredInstance(item);
						matrix4x = GClass1262.zeroMatrix;
						item.state = PrefabInstancingState.Disabled;
						flag = false;
					}
				}
				if (flag && !item.prefabPrototype.meshRenderersDisabled)
				{
					SetRenderersEnabled(item, enabled: false);
				}
				gClass.instanceDataArray[num] = matrix4x;
				num = (item.gpuInstancerID = num + 1);
			}
		}
		gClass.instanceCount = num;
		if (_variationDataList != null)
		{
			foreach (GInterface118 variationData in _variationDataList)
			{
				if (!(variationData.GetPrototype() == p))
				{
					continue;
				}
				variationData.InitializeBufferAndArray(gClass.bufferSize);
				if (value != null)
				{
					foreach (GPUInstancerPrefab item2 in value)
					{
						variationData.SetInstanceData(item2);
					}
				}
				variationData.SetBufferData(0, 0, gClass.bufferSize);
				for (int i = 0; i < gClass.instanceLODs.Count; i++)
				{
					for (int j = 0; j < gClass.instanceLODs[i].renderers.Count; j++)
					{
						variationData.SetVariation(gClass.instanceLODs[i].renderers[j].mpb);
					}
				}
			}
		}
		return gClass;
	}

	public virtual void SetRenderersEnabled(GPUInstancerPrefab prefabInstance, bool enabled)
	{
		if (!prefabInstance || !prefabInstance.prefabPrototype || !prefabInstance.prefabPrototype.prefabObject)
		{
			return;
		}
		MeshRenderer[] componentsInChildren = prefabInstance.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (GClass1274.IsInLayer(layerMask, componentsInChildren[i].gameObject.layer))
				{
					componentsInChildren[i].enabled = enabled;
				}
			}
		}
		BillboardRenderer[] componentsInChildren2 = prefabInstance.GetComponentsInChildren<BillboardRenderer>(includeInactive: true);
		if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
		{
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				if (GClass1274.IsInLayer(layerMask, componentsInChildren2[j].gameObject.layer))
				{
					componentsInChildren2[j].enabled = enabled;
				}
			}
		}
		LODGroup component = prefabInstance.GetComponent<LODGroup>();
		if (component != null)
		{
			component.enabled = enabled;
		}
		Rigidbody component2 = prefabInstance.GetComponent<Rigidbody>();
		if (enabled)
		{
			if (component2 == null)
			{
				GPUInstancerPrefabPrototype.RigidbodyData rigidbodyData = prefabInstance.prefabPrototype.rigidbodyData;
				if (rigidbodyData != null && prefabInstance.prefabPrototype.hasRigidBody)
				{
					component2 = prefabInstance.gameObject.AddComponent<Rigidbody>();
					component2.useGravity = rigidbodyData.useGravity;
					component2.angularDrag = rigidbodyData.angularDrag;
					component2.mass = rigidbodyData.mass;
					component2.constraints = rigidbodyData.constraints;
					component2.detectCollisions = true;
					component2.drag = rigidbodyData.drag;
					component2.isKinematic = rigidbodyData.isKinematic;
					component2.interpolation = rigidbodyData.interpolation;
				}
			}
		}
		else if (component2 != null && !prefabInstance.prefabPrototype.autoUpdateTransformData)
		{
			UnityEngine.Object.Destroy(component2);
		}
	}

	public void DisableIntancingForInstance(GPUInstancerPrefab prefabInstance, bool setRenderersEnabled = true)
	{
		if (!prefabInstance)
		{
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prefabInstance.prefabPrototype, logError: true);
		if (runtimeData != null && prefabInstance.gpuInstancerID > 0 && prefabInstance.gpuInstancerID <= runtimeData.instanceDataArray.Length)
		{
			prefabInstance.state = PrefabInstancingState.Disabled;
			runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = GClass1262.zeroMatrix;
			GClass1274.SetDataPartial(runtimeData.transformationMatrixVisibilityBuffer, runtimeData.instanceDataArray, prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
			if (setRenderersEnabled)
			{
				SetRenderersEnabled(prefabInstance, enabled: true);
			}
		}
		else
		{
			Debug.LogWarning("Can not disable instancing for instance with id: " + prefabInstance.gpuInstancerID);
		}
	}

	public void EnableInstancingForInstance(GPUInstancerPrefab prefabInstance, bool setRenderersDisabled = true)
	{
		if (!prefabInstance)
		{
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prefabInstance.prefabPrototype, logError: true);
		if (runtimeData != null && prefabInstance.gpuInstancerID > 0 && prefabInstance.gpuInstancerID <= runtimeData.instanceDataArray.Length)
		{
			prefabInstance.state = PrefabInstancingState.Instanced;
			runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = prefabInstance.GetInstanceTransform().localToWorldMatrix;
			GClass1274.SetDataPartial(runtimeData.transformationMatrixVisibilityBuffer, runtimeData.instanceDataArray, prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
			if (setRenderersDisabled)
			{
				SetRenderersEnabled(prefabInstance, enabled: false);
			}
		}
		else
		{
			Debug.LogWarning("Can not enable instancing for instance with id: " + prefabInstance.gpuInstancerID);
		}
	}

	public void UpdateTransformDataForInstance(GPUInstancerPrefab prefabInstance)
	{
		if ((bool)prefabInstance)
		{
			GClass1270 runtimeData = GetRuntimeData(prefabInstance.prefabPrototype, logError: true);
			if (runtimeData != null && prefabInstance.gpuInstancerID > 0 && prefabInstance.gpuInstancerID <= runtimeData.instanceDataArray.Length)
			{
				runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = prefabInstance.GetInstanceTransform().localToWorldMatrix;
				runtimeData.transformDataModified = true;
			}
			else
			{
				Debug.LogWarning("Can not update transform for instance with id: " + prefabInstance.gpuInstancerID);
			}
		}
	}

	public void AddPrefabInstance(GPUInstancerPrefab prefabInstance, bool automaticallyIncreaseBufferSize = false)
	{
		if (!prefabInstance || prefabInstance.state == PrefabInstancingState.Instanced || runtimeDataList == null)
		{
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prefabInstance.prefabPrototype, logError: true);
		if (runtimeData == null)
		{
			return;
		}
		if (runtimeData.instanceDataArray.Length == runtimeData.instanceCount)
		{
			if (!automaticallyIncreaseBufferSize)
			{
				Debug.LogWarning("Can not add instance. Buffer is full.");
				return;
			}
			runtimeData.bufferSize += 1024;
			Matrix4x4[] instanceDataArray = runtimeData.instanceDataArray;
			runtimeData.instanceDataArray = new Matrix4x4[runtimeData.bufferSize];
			Array.Copy(instanceDataArray, runtimeData.instanceDataArray, instanceDataArray.Length);
			runtimeData.instanceDataArray[runtimeData.instanceCount] = prefabInstance.GetInstanceTransform().localToWorldMatrix;
			runtimeData.instanceCount++;
			prefabInstance.gpuInstancerID = runtimeData.instanceCount;
			_registeredPrefabsRuntimeData[prefabInstance.prefabPrototype].Add(prefabInstance);
			if (!prefabInstance.prefabPrototype.meshRenderersDisabled)
			{
				SetRenderersEnabled(prefabInstance, enabled: false);
			}
			prefabInstance.GetInstanceTransform().hasChanged = false;
			prefabInstance.state = PrefabInstancingState.Instanced;
			GClass1274.InitializeGPUBuffer(runtimeData);
			prefabInstance.SetupPrefabInstance(runtimeData, forceNew: true);
			if (_variationDataList == null)
			{
				return;
			}
			{
				foreach (GInterface118 variationData in _variationDataList)
				{
					if (!(variationData.GetPrototype() == prefabInstance.prefabPrototype))
					{
						continue;
					}
					variationData.SetNewBufferSize(runtimeData.bufferSize);
					variationData.SetInstanceData(prefabInstance);
					variationData.SetBufferData(prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
					for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
					{
						for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
						{
							variationData.SetVariation(runtimeData.instanceLODs[i].renderers[j].mpb);
						}
					}
				}
				return;
			}
		}
		prefabInstance.state = PrefabInstancingState.Instanced;
		runtimeData.instanceDataArray[runtimeData.instanceCount] = prefabInstance.GetInstanceTransform().localToWorldMatrix;
		runtimeData.instanceCount++;
		prefabInstance.gpuInstancerID = runtimeData.instanceCount;
		GClass1274.SetDataPartial(runtimeData.transformationMatrixVisibilityBuffer, runtimeData.instanceDataArray, prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
		if (!prefabInstance.prefabPrototype.meshRenderersDisabled)
		{
			SetRenderersEnabled(prefabInstance, enabled: false);
		}
		if (!_registeredPrefabsRuntimeData.ContainsKey(prefabInstance.prefabPrototype))
		{
			_registeredPrefabsRuntimeData.Add(prefabInstance.prefabPrototype, new List<GPUInstancerPrefab>());
		}
		_registeredPrefabsRuntimeData[prefabInstance.prefabPrototype].Add(prefabInstance);
		prefabInstance.GetInstanceTransform().hasChanged = false;
		if (_variationDataList != null)
		{
			foreach (GInterface118 variationData2 in _variationDataList)
			{
				if (variationData2.GetPrototype() == prefabInstance.prefabPrototype)
				{
					variationData2.SetInstanceData(prefabInstance);
					variationData2.SetBufferData(prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
				}
			}
		}
		prefabInstance.SetupPrefabInstance(runtimeData, forceNew: true);
	}

	public void SetColorBuffers(List<Color> dryColor, List<Color> healthyColor)
	{
		computeBuffer_0 = new ComputeBuffer(dryColor.Count, Marshal.SizeOf(typeof(Color)));
		computeBuffer_1 = new ComputeBuffer(healthyColor.Count, Marshal.SizeOf(typeof(Color)));
		computeBuffer_0.SetData(dryColor);
		computeBuffer_1.SetData(healthyColor);
	}

	public void AddPrefabInstances(IEnumerable<GPUInstancerPrefab> prefabInstances, bool isThreading = false)
	{
		while (isThreading && _addRemoveInProgress)
		{
			Thread.Sleep(100);
		}
		_addRemoveInProgress = true;
		List<GPUInstancerPrefab>[] array = new List<GPUInstancerPrefab>[prototypeList.Count];
		Dictionary<GPUInstancerPrototype, int> dictionary = new Dictionary<GPUInstancerPrototype, int>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<GPUInstancerPrefab>();
			dictionary.Add(prototypeList[i], i);
		}
		foreach (GPUInstancerPrefab prefabInstance in prefabInstances)
		{
			array[dictionary[prefabInstance.prefabPrototype]].Add(prefabInstance);
		}
		for (int j = 0; j < array.Length; j++)
		{
			AddPrefabInstances((GPUInstancerPrefabPrototype)prototypeList[j], array[j], isThreading);
		}
		if (isThreading)
		{
			threadQueue.Enqueue(delegate
			{
				_addRemoveInProgress = false;
			});
		}
		else
		{
			_addRemoveInProgress = false;
		}
	}

	public void AddPrefabInstances(GPUInstancerPrefabPrototype prototype, IEnumerable<GPUInstancerPrefab> prefabInstances, bool isThreading = false)
	{
		if (prefabInstances == null || prefabInstances.Count() == 0)
		{
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prototype, logError: true);
		if (runtimeData == null)
		{
			return;
		}
		int num = prefabInstances.Count();
		if (runtimeData.instanceCount + num > runtimeData.bufferSize)
		{
			runtimeData.bufferSize = runtimeData.instanceCount + num;
			Matrix4x4[] instanceDataArray = runtimeData.instanceDataArray;
			runtimeData.instanceDataArray = new Matrix4x4[runtimeData.bufferSize];
			Array.Copy(instanceDataArray, runtimeData.instanceDataArray, instanceDataArray.Length);
			for (int i = 0; i < num; i++)
			{
				GPUInstancerPrefab gPUInstancerPrefab = prefabInstances.ElementAt(i);
				runtimeData.instanceDataArray[runtimeData.instanceCount + i] = gPUInstancerPrefab.GetLocalToWorldMatrix();
				gPUInstancerPrefab.gpuInstancerID = runtimeData.instanceCount + i + 1;
				if (!prototype.meshRenderersDisabled)
				{
					SetRenderersEnabled(gPUInstancerPrefab, enabled: false);
				}
				gPUInstancerPrefab.state = PrefabInstancingState.Instanced;
			}
			_registeredPrefabsRuntimeData[prototype].AddRange(prefabInstances);
			runtimeData.instanceCount = runtimeData.bufferSize;
			if (isThreading)
			{
				threadQueue.Enqueue(delegate
				{
					GClass1274.InitializeGPUBuffer(runtimeData);
				});
			}
			else
			{
				GClass1274.InitializeGPUBuffer(runtimeData);
			}
			return;
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			GPUInstancerPrefab gPUInstancerPrefab = prefabInstances.ElementAt(num2);
			runtimeData.instanceDataArray[runtimeData.instanceCount + num2] = gPUInstancerPrefab.GetLocalToWorldMatrix();
			gPUInstancerPrefab.gpuInstancerID = runtimeData.instanceCount + num2 + 1;
			if (!prototype.meshRenderersDisabled)
			{
				SetRenderersEnabled(gPUInstancerPrefab, enabled: false);
			}
			gPUInstancerPrefab.state = PrefabInstancingState.Instanced;
		}
		_registeredPrefabsRuntimeData[prototype].AddRange(prefabInstances);
		if (isThreading)
		{
			threadQueue.Enqueue(delegate
			{
				runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
			});
		}
		else
		{
			runtimeData.transformationMatrixVisibilityBuffer.SetData(runtimeData.instanceDataArray);
		}
		runtimeData.instanceCount += num;
	}

	public void UpdateInstanceDataArray(GClass1270 runtimeData, List<GPUInstancerPrefab> prefabList, bool isThreading = false)
	{
		if (runtimeData.instanceDataArray.Length != prefabList.Count)
		{
			runtimeData.instanceDataArray = new Matrix4x4[prefabList.Count];
		}
		for (int num = 0; num < prefabList.Count; num = (prefabList[num].gpuInstancerID = num + 1))
		{
			runtimeData.instanceDataArray[num] = prefabList[num].GetLocalToWorldMatrix();
		}
		int instanceCount = prefabList.Count;
		int bufferSize = instanceCount + ((GPUInstancerPrefabPrototype)runtimeData.prototype).extraBufferSize;
		if (isThreading)
		{
			threadQueue.Enqueue(delegate
			{
				runtimeData.instanceCount = instanceCount;
				runtimeData.bufferSize = bufferSize;
				GClass1274.InitializeGPUBuffer(runtimeData);
			});
		}
		else
		{
			runtimeData.instanceCount = instanceCount;
			runtimeData.bufferSize = bufferSize;
			GClass1274.InitializeGPUBuffer(runtimeData);
		}
	}

	public void RemovePrefabInstance(GPUInstancerPrefab prefabInstance, bool setRenderersEnabled = true)
	{
		if (!prefabInstance || prefabInstance.state == PrefabInstancingState.None)
		{
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prefabInstance.prefabPrototype);
		if (runtimeData == null)
		{
			return;
		}
		if (prefabInstance.gpuInstancerID > runtimeData.instanceDataArray.Length)
		{
			Debug.LogWarning("Instance can not be removed.");
			return;
		}
		List<GPUInstancerPrefab> list = _registeredPrefabsRuntimeData[prefabInstance.prefabPrototype];
		if (prefabInstance.gpuInstancerID == runtimeData.instanceCount)
		{
			prefabInstance.state = PrefabInstancingState.None;
			runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = GClass1262.zeroMatrix;
			runtimeData.instanceCount--;
			list.RemoveAt(prefabInstance.gpuInstancerID - 1);
			if (setRenderersEnabled && enableMROnRemoveInstance && !prefabInstance.prefabPrototype.meshRenderersDisabled)
			{
				SetRenderersEnabled(prefabInstance, enabled: true);
			}
			return;
		}
		GPUInstancerPrefab gPUInstancerPrefab = null;
		for (int num = list.Count - 1; num >= 0; num--)
		{
			GPUInstancerPrefab gPUInstancerPrefab2 = list[num];
			if (gPUInstancerPrefab2 == null)
			{
				list.RemoveAt(num);
				num++;
			}
			else if (gPUInstancerPrefab2.gpuInstancerID == runtimeData.instanceCount)
			{
				gPUInstancerPrefab = gPUInstancerPrefab2;
				break;
			}
		}
		if (!gPUInstancerPrefab)
		{
			list.RemoveAll((GPUInstancerPrefab pi) => pi == null);
			Debug.LogWarning("Prefab instance was destoyed before being removed from instance list in GPUI Prefab Manager!");
			return;
		}
		prefabInstance.state = PrefabInstancingState.None;
		runtimeData.instanceDataArray[prefabInstance.gpuInstancerID - 1] = runtimeData.instanceDataArray[gPUInstancerPrefab.gpuInstancerID - 1];
		runtimeData.instanceDataArray[gPUInstancerPrefab.gpuInstancerID - 1] = GClass1262.zeroMatrix;
		runtimeData.instanceCount--;
		GClass1274.SetDataPartial(runtimeData.transformationMatrixVisibilityBuffer, runtimeData.instanceDataArray, prefabInstance.gpuInstancerID - 1, prefabInstance.gpuInstancerID - 1, 1);
		list.RemoveAt(gPUInstancerPrefab.gpuInstancerID - 1);
		gPUInstancerPrefab.gpuInstancerID = prefabInstance.gpuInstancerID;
		list[gPUInstancerPrefab.gpuInstancerID - 1] = gPUInstancerPrefab;
		if (setRenderersEnabled && enableMROnRemoveInstance && !prefabInstance.prefabPrototype.meshRenderersDisabled)
		{
			SetRenderersEnabled(prefabInstance, enabled: true);
		}
		if (_variationDataList != null)
		{
			foreach (GInterface118 variationData in _variationDataList)
			{
				if (variationData.GetPrototype() == gPUInstancerPrefab.prefabPrototype)
				{
					variationData.SetInstanceData(gPUInstancerPrefab);
					variationData.SetBufferData(gPUInstancerPrefab.gpuInstancerID - 1, gPUInstancerPrefab.gpuInstancerID - 1, 1);
				}
			}
		}
		gPUInstancerPrefab.SetupPrefabInstance(runtimeData);
	}

	public void RemovePrefabInstances(IEnumerable<GPUInstancerPrefab> prefabInstances, bool isThreading = false)
	{
		while (isThreading && _addRemoveInProgress)
		{
			Thread.Sleep(100);
		}
		_addRemoveInProgress = true;
		List<GPUInstancerPrefab>[] array = new List<GPUInstancerPrefab>[prototypeList.Count];
		Dictionary<GPUInstancerPrototype, int> dictionary = new Dictionary<GPUInstancerPrototype, int>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<GPUInstancerPrefab>();
			dictionary.Add(prototypeList[i], i);
		}
		foreach (GPUInstancerPrefab prefabInstance in prefabInstances)
		{
			array[dictionary[prefabInstance.prefabPrototype]].Add(prefabInstance);
		}
		for (int j = 0; j < array.Length; j++)
		{
			RemovePrefabInstances((GPUInstancerPrefabPrototype)prototypeList[j], array[j], isThreading);
		}
		if (isThreading)
		{
			threadQueue.Enqueue(delegate
			{
				_addRemoveInProgress = false;
			});
		}
		else
		{
			_addRemoveInProgress = false;
		}
	}

	public void RemovePrefabInstances(GPUInstancerPrefabPrototype prototype, IEnumerable<GPUInstancerPrefab> prefabInstances, bool isThreading = false)
	{
		if (prefabInstances == null || prefabInstances.Count() == 0)
		{
			return;
		}
		int count = prefabInstances.Count();
		GClass1270 runtimeData = GetRuntimeData(prototype, logError: true);
		if (runtimeData == null)
		{
			return;
		}
		List<GPUInstancerPrefab> list = _registeredPrefabsRuntimeData[prototype];
		list.RemoveRange(prefabInstances.ElementAt(0).gpuInstancerID - 1, count);
		foreach (GPUInstancerPrefab prefabInstance in prefabInstances)
		{
			if (enableMROnRemoveInstance && !prototype.meshRenderersDisabled)
			{
				SetRenderersEnabled(prefabInstance, enabled: true);
			}
			prefabInstance.state = PrefabInstancingState.None;
			prefabInstance.gpuInstancerID = 0;
		}
		UpdateInstanceDataArray(runtimeData, list, isThreading);
	}

	public void RegisterPrefabsInScene()
	{
		registeredPrefabs.Clear();
		foreach (GPUInstancerPrefabPrototype prototype in prototypeList)
		{
			registeredPrefabs.Add(new RegisteredPrefabsData(prototype));
		}
		GPUInstancerPrefab[] array = UnityEngine.Object.FindObjectsOfType<GPUInstancerPrefab>();
		foreach (GPUInstancerPrefab prefabInstance in array)
		{
			AddRegisteredPrefab(prefabInstance);
		}
	}

	public void RegisterPrefabInstanceList(IEnumerable<GPUInstancerPrefab> prefabInstanceList)
	{
		if (_registeredPrefabsRuntimeData == null)
		{
			_registeredPrefabsRuntimeData = new Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>>();
		}
		if (_registeredPrefabsRuntimeData.Keys.Count != prototypeList.Count)
		{
			foreach (GPUInstancerPrototype prototype in prototypeList)
			{
				if (!_registeredPrefabsRuntimeData.ContainsKey(prototype))
				{
					_registeredPrefabsRuntimeData.Add(prototype, new List<GPUInstancerPrefab>());
				}
			}
		}
		foreach (GPUInstancerPrefab prefabInstance in prefabInstanceList)
		{
			_registeredPrefabsRuntimeData[prefabInstance.prefabPrototype].Add(prefabInstance);
		}
	}

	public void UnregisterPrefabInstanceList(IEnumerable<GPUInstancerPrefab> prefabInstanceList)
	{
		if (_registeredPrefabsRuntimeData == null)
		{
			_registeredPrefabsRuntimeData = new Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>>();
		}
		if (_registeredPrefabsRuntimeData.Keys.Count != prototypeList.Count)
		{
			foreach (GPUInstancerPrototype prototype in prototypeList)
			{
				if (!_registeredPrefabsRuntimeData.ContainsKey(prototype))
				{
					_registeredPrefabsRuntimeData.Add(prototype, new List<GPUInstancerPrefab>());
				}
			}
		}
		foreach (GPUInstancerPrefab prefabInstance in prefabInstanceList)
		{
			_registeredPrefabsRuntimeData[prefabInstance.prefabPrototype].Remove(prefabInstance);
		}
	}

	public void ClearRegisteredPrefabInstances()
	{
		foreach (GPUInstancerPrototype key in _registeredPrefabsRuntimeData.Keys)
		{
			_registeredPrefabsRuntimeData[key].Clear();
		}
	}

	public void ClearRegisteredPrefabInstances(GPUInstancerPrototype p)
	{
		if (_registeredPrefabsRuntimeData.ContainsKey(p))
		{
			_registeredPrefabsRuntimeData[p].Clear();
		}
	}

	public GClass1275<T> DefinePrototypeVariationBuffer<T>(GPUInstancerPrefabPrototype prototype, string bufferName)
	{
		if (_variationDataList == null)
		{
			_variationDataList = new List<GInterface118>();
		}
		if (GClass1274.matrixHandlingType != GPUIMatrixHandlingType.Default)
		{
			Debug.LogError("GPUI can not define material variations in this platform and/or with this rendering settings.");
			return null;
		}
		GClass1275<T> gClass = (GClass1275<T>)_variationDataList.Find((GInterface118 v) => v.GetPrototype() == prototype && v.GetBufferName() == bufferName);
		if (gClass == null)
		{
			gClass = new GClass1275<T>(prototype, bufferName);
			_variationDataList.Add(gClass);
			if (isInitialized)
			{
				GClass1270 runtimeData = GetRuntimeData(prototype);
				gClass.InitializeBufferAndArray(runtimeData.bufferSize);
				if (_registeredPrefabsRuntimeData != null && _registeredPrefabsRuntimeData.ContainsKey(prototype))
				{
					foreach (GPUInstancerPrefab item in _registeredPrefabsRuntimeData[prototype])
					{
						gClass.SetInstanceData(item);
					}
				}
				gClass.SetBufferData(0, 0, runtimeData.bufferSize);
				for (int num = 0; num < runtimeData.instanceLODs.Count; num++)
				{
					for (int num2 = 0; num2 < runtimeData.instanceLODs[num].renderers.Count; num2++)
					{
						gClass.SetVariation(runtimeData.instanceLODs[num].renderers[num2].mpb);
					}
				}
			}
		}
		return gClass;
	}

	public void UpdateVariationData<T>(GPUInstancerPrefab prefabInstance, string bufferName, T value)
	{
		if (!prefabInstance || !prefabInstance.prefabPrototype)
		{
			return;
		}
		GClass1275<T> gClass = (GClass1275<T>)_variationDataList.Find((GInterface118 v) => v.GetPrototype() == prefabInstance.prefabPrototype && v.GetBufferName() == bufferName);
		if (gClass != null && gClass.dataArray != null)
		{
			int num = prefabInstance.gpuInstancerID - 1;
			if (num >= 0 && num < gClass.dataArray.Length)
			{
				gClass.dataArray[num] = value;
				gClass.variationBuffer.SetData(gClass.dataArray, num, num, 1);
			}
		}
	}

	public GClass1275<T> DefineAndAddVariationFromArray<T>(GPUInstancerPrefabPrototype prototype, string bufferName, T[] variationArray)
	{
		GClass1275<T> gClass = DefinePrototypeVariationBuffer<T>(prototype, bufferName);
		GClass1270 runtimeData = GetRuntimeData(prototype);
		if (runtimeData != null && gClass != null)
		{
			gClass.SetArrayAndInitializeBuffer(variationArray);
			gClass.SetBufferData(0, 0, runtimeData.bufferSize);
			for (int i = 0; i < runtimeData.instanceLODs.Count; i++)
			{
				for (int j = 0; j < runtimeData.instanceLODs[i].renderers.Count; j++)
				{
					MaterialPropertyBlock mpb = runtimeData.instanceLODs[i].renderers[j].mpb;
					gClass.SetVariation(mpb);
					mpb.SetBuffer(string_0, computeBuffer_0);
					mpb.SetBuffer(string_1, computeBuffer_1);
				}
			}
		}
		return gClass;
	}

	public GClass1275<T> UpdateVariationsFromArray<T>(GPUInstancerPrefabPrototype prototype, string bufferName, T[] variationArray, int arrayStartIndex = 0, int bufferStartIndex = 0, int count = 0)
	{
		GClass1275<T> gClass = (GClass1275<T>)_variationDataList.Find((GInterface118 v) => v is GClass1275<T> && ((GClass1275<T>)v).prototype == prototype && ((GClass1275<T>)v).bufferName == bufferName);
		if (gClass != null)
		{
			GClass1270 runtimeData = GetRuntimeData(prototype);
			if (runtimeData != null)
			{
				gClass.dataArray = variationArray;
				if (count > 0)
				{
					gClass.SetBufferData(arrayStartIndex, bufferStartIndex, count);
				}
				else
				{
					gClass.SetBufferData(0, 0, runtimeData.bufferSize);
				}
				for (int num = 0; num < runtimeData.instanceLODs.Count; num++)
				{
					for (int num2 = 0; num2 < runtimeData.instanceLODs[num].renderers.Count; num2++)
					{
						gClass.SetVariation(runtimeData.instanceLODs[num].renderers[num2].mpb);
					}
				}
			}
		}
		return gClass;
	}

	public GPUInstancerPrefabPrototype DefineGameObjectAsPrefabPrototypeAtRuntime(GameObject prototypeGameObject)
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("DefineGameObjectAsPrefabPrototypeAtRuntime method is designed to use at runtime. Prototype generation canceled.");
			return null;
		}
		if (prefabList == null)
		{
			prefabList = new List<GameObject>();
		}
		GPUInstancerPrefabPrototype gPUInstancerPrefabPrototype = GClass1274.GeneratePrefabPrototype(prototypeGameObject, forceNew: false);
		if (!prototypeList.Contains(gPUInstancerPrefabPrototype))
		{
			prototypeList.Add(gPUInstancerPrefabPrototype);
		}
		if (!prefabList.Contains(prototypeGameObject))
		{
			prefabList.Add(prototypeGameObject);
		}
		if (gPUInstancerPrefabPrototype.minCullingDistance < minCullingDistance)
		{
			gPUInstancerPrefabPrototype.minCullingDistance = minCullingDistance;
		}
		return gPUInstancerPrefabPrototype;
	}

	public void AddInstancesToPrefabPrototypeAtRuntime(GPUInstancerPrefabPrototype prefabPrototype, IEnumerable<GameObject> instances)
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("AddInstancesToPrefabPrototypeAtRuntime method is designed to use at runtime. Adding instances canceled.");
			return;
		}
		if (base.isActiveAndEnabled)
		{
			if (!_registeredPrefabsRuntimeData.TryGetValue(prefabPrototype, out var value))
			{
				value = new List<GPUInstancerPrefab>();
				_registeredPrefabsRuntimeData.Add(prefabPrototype, value);
			}
			foreach (GameObject instance in instances)
			{
				GPUInstancerPrefab gPUInstancerPrefab = instance.GetComponent<GPUInstancerPrefab>();
				if (gPUInstancerPrefab == null)
				{
					gPUInstancerPrefab = instance.AddComponent<GPUInstancerPrefab>();
					gPUInstancerPrefab.prefabPrototype = prefabPrototype;
				}
				if (gPUInstancerPrefab != null && !value.Contains(gPUInstancerPrefab))
				{
					value.Add(gPUInstancerPrefab);
				}
			}
			GClass1270 runtimeData = InitializeRuntimeDataForPrefabPrototype(prefabPrototype);
			GClass1274.ReleaseInstanceBuffers(runtimeData);
			GClass1274.InitializeGPUBuffer(runtimeData);
			return;
		}
		if (registeredPrefabs == null)
		{
			registeredPrefabs = new List<RegisteredPrefabsData>();
		}
		RegisteredPrefabsData registeredPrefabsData = registeredPrefabs.Find((RegisteredPrefabsData d) => d.prefabPrototype == prefabPrototype);
		if (registeredPrefabsData == null)
		{
			registeredPrefabsData = new RegisteredPrefabsData(prefabPrototype);
			registeredPrefabs.Add(registeredPrefabsData);
		}
		foreach (GameObject instance2 in instances)
		{
			GPUInstancerPrefab gPUInstancerPrefab2 = instance2.GetComponent<GPUInstancerPrefab>();
			if (gPUInstancerPrefab2 == null)
			{
				gPUInstancerPrefab2 = instance2.AddComponent<GPUInstancerPrefab>();
				gPUInstancerPrefab2.prefabPrototype = prefabPrototype;
			}
			if (gPUInstancerPrefab2 != null && !registeredPrefabsData.registeredPrefabs.Contains(gPUInstancerPrefab2))
			{
				registeredPrefabsData.registeredPrefabs.Add(gPUInstancerPrefab2);
			}
		}
	}

	public void RemovePrototypeAtRuntime(GPUInstancerPrefabPrototype prefabPrototype)
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("RemovePrototypeAtRuntime method is designed to use at runtime. Prototype removal canceled.");
			return;
		}
		GClass1270 runtimeData = GetRuntimeData(prefabPrototype);
		if (runtimeData != null)
		{
			GClass1274.ReleaseInstanceBuffers(runtimeData);
			if (runtimeDataList != null)
			{
				runtimeDataList.Remove(runtimeData);
			}
			if (runtimeDataDictionary != null)
			{
				runtimeDataDictionary.Remove(runtimeData.prototype);
			}
		}
		if (base.isActiveAndEnabled)
		{
			_registeredPrefabsRuntimeData.Remove(prefabPrototype);
		}
		else if (registeredPrefabs != null)
		{
			RegisteredPrefabsData registeredPrefabsData = registeredPrefabs.Find((RegisteredPrefabsData d) => d.prefabPrototype == prefabPrototype);
			if (registeredPrefabsData != null)
			{
				registeredPrefabs.Remove(registeredPrefabsData);
			}
		}
		if (prototypeList.Contains(prefabPrototype))
		{
			prototypeList.Remove(prefabPrototype);
		}
		if ((bool)prefabPrototype.prefabObject && prefabList.Contains(prefabPrototype.prefabObject))
		{
			prefabList.Remove(prefabPrototype.prefabObject);
		}
	}

	public void AddRegisteredPrefab(GPUInstancerPrefab prefabInstance)
	{
		registeredPrefabs.Find((RegisteredPrefabsData rpd) => rpd.prefabPrototype == prefabInstance.prefabPrototype)?.registeredPrefabs.Add(prefabInstance);
	}

	public void AddRuntimeRegisteredPrefab(GPUInstancerPrefab prefabInstance)
	{
		List<GPUInstancerPrefab> list;
		if (_registeredPrefabsRuntimeData.ContainsKey(prefabInstance.prefabPrototype))
		{
			list = _registeredPrefabsRuntimeData[prefabInstance.prefabPrototype];
		}
		else
		{
			list = new List<GPUInstancerPrefab>();
			_registeredPrefabsRuntimeData.Add(prefabInstance.prefabPrototype, list);
		}
		if (!list.Contains(prefabInstance))
		{
			list.Add(prefabInstance);
		}
	}

	public void AddModificationCollider(GPUInstancerModificationCollider modificationCollider)
	{
		if (_modificationColliders == null)
		{
			_modificationColliders = new List<GPUInstancerModificationCollider>();
		}
		_modificationColliders.Add(modificationCollider);
	}

	public int GetEnabledPrefabCount()
	{
		int num = 0;
		if (_modificationColliders != null)
		{
			for (int i = 0; i < _modificationColliders.Count; i++)
			{
				num += _modificationColliders[i].GetEnteredInstanceCount();
			}
		}
		return num;
	}

	public Dictionary<GPUInstancerPrototype, List<GPUInstancerPrefab>> GetRegisteredPrefabsRuntimeData()
	{
		return _registeredPrefabsRuntimeData;
	}

	[CompilerGenerated]
	public void method_1()
	{
		_addRemoveInProgress = false;
	}

	[CompilerGenerated]
	public void method_2()
	{
		_addRemoveInProgress = false;
	}
}
