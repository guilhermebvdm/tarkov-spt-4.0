using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class PhysicsCustomOverlapBoxSystem : MonoBehaviour
{
	[DefaultExecutionOrder(5000)]
	public class PhysicsCustomOverlapBoxSystemScheduler : MonoBehaviour
	{
		private PhysicsCustomOverlapBoxSystem physicsCustomOverlapBoxSystem_0;

		public void Awake()
		{
			physicsCustomOverlapBoxSystem_0 = GetComponent<PhysicsCustomOverlapBoxSystem>();
		}

		public void LateUpdate()
		{
			physicsCustomOverlapBoxSystem_0.method_1();
		}
	}

	[DefaultExecutionOrder(-5000)]
	public class PhysicsCustomOverlapBoxSystemHandleResult : MonoBehaviour
	{
		private PhysicsCustomOverlapBoxSystem physicsCustomOverlapBoxSystem_0;

		public void Awake()
		{
			physicsCustomOverlapBoxSystem_0 = GetComponent<PhysicsCustomOverlapBoxSystem>();
		}

		public void Update()
		{
			physicsCustomOverlapBoxSystem_0.method_0();
		}
	}

	private const int int_0 = 16;

	private static PhysicsCustomOverlapBoxSystem physicsCustomOverlapBoxSystem_0;

	private GClass742.GStruct34[] gstruct34_0;

	private List<GClass742> list_0;

	private List<GClass742> list_1;

	[Header("Debug")]
	public int SearcherCount;

	public int TotalTriggersCount;

	public static PhysicsCustomOverlapBoxSystem Instance
	{
		get
		{
			if (physicsCustomOverlapBoxSystem_0 == null)
			{
				GameObject obj = new GameObject("[PhysicsCustomOverlapBoxSystem]");
				UnityEngine.Object.DontDestroyOnLoad(obj);
				physicsCustomOverlapBoxSystem_0 = obj.AddComponent<PhysicsCustomOverlapBoxSystem>();
				obj.AddComponent<PhysicsCustomOverlapBoxSystemScheduler>();
				obj.AddComponent<PhysicsCustomOverlapBoxSystemHandleResult>();
			}
			return physicsCustomOverlapBoxSystem_0;
		}
	}

	public void Awake()
	{
		list_0 = new List<GClass742>(16);
		list_1 = new List<GClass742>(16);
		gstruct34_0 = Array.Empty<GClass742.GStruct34>();
	}

	public void RegisterSearcher(GClass742 searcher)
	{
		if (!list_0.Contains(searcher))
		{
			list_0.Add(searcher);
		}
	}

	public void UnregisterSearcher(GClass742 searcher)
	{
		if (list_0.Contains(searcher))
		{
			list_1.Add(searcher);
			list_0.Remove(searcher);
		}
	}

	public void method_0()
	{
		for (int i = 0; i < gstruct34_0.Length; i++)
		{
			if (gstruct34_0[i].CommandsCount == 0)
			{
				continue;
			}
			try
			{
				gstruct34_0[i].Searcher.WaitScheduleComplete(gstruct34_0[i]);
			}
			catch (Exception innerException)
			{
				Debug.LogException(new Exception("Failed wait scheduled jobs complete", innerException));
			}
			finally
			{
				gstruct34_0[i].Dispose();
				gstruct34_0[i] = default(GClass742.GStruct34);
			}
		}
		method_2();
	}

	public void method_1()
	{
		if (gstruct34_0.Length != list_0.Count)
		{
			Array.Resize(ref gstruct34_0, list_0.Count);
		}
		for (int i = 0; i < list_0.Count; i++)
		{
			gstruct34_0[i] = list_0[i].Schedule();
		}
		JobHandle.ScheduleBatchedJobs();
	}

	public void method_2()
	{
		if (list_1.Count == 0)
		{
			return;
		}
		foreach (GClass742 item in list_1)
		{
			item.Clear();
		}
		list_1.Clear();
		if (list_0.Count == 0)
		{
			GClass742.PARAMS_INDEX_COUNTER = 0;
		}
	}

	public void OnDestroy()
	{
		if (list_0 != null)
		{
			GClass742[] array = list_0.ToArray();
			foreach (GClass742 searcher in array)
			{
				UnregisterSearcher(searcher);
			}
			method_0();
			list_0.Clear();
			list_1.Clear();
			gstruct34_0 = null;
		}
	}
}
