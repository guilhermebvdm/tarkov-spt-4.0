using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace GPUInstancer;

public class GPUInstancerPrefabListRuntimeHandler : MonoBehaviour
{
	[Serializable]
	[CompilerGenerated]
	public class Class891
	{
		public static readonly Class891 class891_0 = new Class891();

		public static Func<GPUInstancerPrototype, bool> func_0;

		public bool method_0(GPUInstancerPrototype p)
		{
			return ((GPUInstancerPrefabPrototype)p).meshRenderersDisabled;
		}
	}

	public GPUInstancerPrefabManager prefabManager;

	private IEnumerable<GPUInstancerPrefab> ienumerable_0;

	private bool bool_0;

	public bool runInThreads = true;

	public void OnEnable()
	{
		if (prefabManager == null)
		{
			return;
		}
		if (!prefabManager.prototypeList.All((GPUInstancerPrototype p) => ((GPUInstancerPrefabPrototype)p).meshRenderersDisabled))
		{
			Debug.LogWarning("GPUInstancerPrefabListRuntimeHandler can not run in Threads while Mesh Renderers are enabled on the prefabs. Disabling threading...");
			runInThreads = false;
		}
		ienumerable_0 = base.gameObject.GetComponentsInChildren<GPUInstancerPrefab>(includeInactive: true);
		if (ienumerable_0 == null || ienumerable_0.Count() <= 0)
		{
			return;
		}
		bool_0 = true;
		if (runInThreads)
		{
			foreach (GPUInstancerPrefab item in ienumerable_0)
			{
				item.GetLocalToWorldMatrix(forceNew: true);
			}
			Thread thread = new Thread(AddPrefabInstancesAsync);
			thread.IsBackground = true;
			prefabManager.threadStartQueue.Enqueue(new GPUInstancerManager.GClass1261
			{
				thread = thread,
				parameter = ienumerable_0
			});
		}
		else
		{
			AddPrefabInstancesAsync(ienumerable_0);
		}
	}

	public void OnDisable()
	{
		bool_0 = false;
		if (prefabManager == null)
		{
			return;
		}
		if (ienumerable_0 != null && ienumerable_0.Count() > 0)
		{
			if (runInThreads)
			{
				Thread thread = new Thread(RemovePrefabInstancesAsync);
				thread.IsBackground = true;
				prefabManager.threadStartQueue.Enqueue(new GPUInstancerManager.GClass1261
				{
					thread = thread,
					parameter = ienumerable_0
				});
			}
			else
			{
				RemovePrefabInstancesAsync(ienumerable_0);
			}
		}
		ienumerable_0 = null;
	}

	public void AddPrefabInstancesAsync(object param)
	{
		try
		{
			prefabManager.AddPrefabInstances((IEnumerable<GPUInstancerPrefab>)param, runInThreads);
		}
		catch (Exception ex)
		{
			if (runInThreads)
			{
				prefabManager.threadException = ex;
				prefabManager.threadQueue.Enqueue(prefabManager.LogThreadException);
			}
			else
			{
				Debug.LogException(ex);
			}
		}
	}

	public void RemovePrefabInstancesAsync(object param)
	{
		try
		{
			prefabManager.RemovePrefabInstances((IEnumerable<GPUInstancerPrefab>)param, runInThreads);
		}
		catch (Exception ex)
		{
			if (runInThreads)
			{
				prefabManager.threadException = ex;
				prefabManager.threadQueue.Enqueue(prefabManager.LogThreadException);
			}
			else
			{
				Debug.LogException(ex);
			}
		}
	}

	public void SetManager(GPUInstancerPrefabManager prefabManager)
	{
		if (bool_0)
		{
			OnDisable();
		}
		this.prefabManager = prefabManager;
		if (base.isActiveAndEnabled)
		{
			OnEnable();
		}
	}
}
