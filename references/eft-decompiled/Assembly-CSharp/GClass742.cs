using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEngine;

public class GClass742 : IDisposable
{
	public struct GStruct34 : IDisposable
	{
		public GClass742 Searcher;

		public int CommandsCount;

		public GStruct33 Job;

		public JobHandle Handle;

		public void Dispose()
		{
		}
	}

	[CompilerGenerated]
	public class Class418
	{
		public int id;

		public bool method_0(GClass739 t)
		{
			return t.Id == id;
		}
	}

	[CompilerGenerated]
	public class Class419
	{
		public BoxCollider collider;

		public int id;

		public bool method_0(KeyValuePair<int, BoxCollider> kv)
		{
			return kv.Value == collider;
		}

		public bool method_1(GClass739 el)
		{
			return el.Id == id;
		}
	}

	public static int PARAMS_INDEX_COUNTER;

	[NonSerialized]
	public const int Int_0 = 2048;

	[NonSerialized]
	public const int Int_1 = 32;

	[NonSerialized]
	public const int Int_2 = 128;

	[NonSerialized]
	public HashSet<BoxCollider> HashSet_0 = new HashSet<BoxCollider>();

	[NonSerialized]
	public Dictionary<int, BoxCollider> Dictionary_0 = new Dictionary<int, BoxCollider>(32);

	[NonSerialized]
	public List<GClass739> List_0 = new List<GClass739>(32);

	[NonSerialized]
	public GStruct31[] Gstruct31_0 = new GStruct31[128];

	[NonSerialized]
	public GStruct31[] Gstruct31_1 = new GStruct31[128];

	[NonSerialized]
	public GStruct30[] Gstruct30_0;

	[NonSerialized]
	public GStruct30[] Gstruct30_1;

	[NonSerialized]
	public int Int_3;

	[NonSerialized]
	public int Int_4;

	[NonSerialized]
	public int Int_5;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public int Int_6;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	public int RegisteredTriggersCount => List_0.Count;

	public bool HaveCriticalTriggersCount
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
		[CompilerGenerated]
		set
		{
			Bool_1 = value;
		}
	}

	public GStruct31[] GStruct31_0
	{
		get
		{
			if (Int_6 != 0)
			{
				return Gstruct31_0;
			}
			return Gstruct31_1;
		}
	}

	public GStruct33.GClass741 GClass741_0 => GStruct33.JobParams[Int_4];

	public GClass742()
	{
		Int_4 = ++PARAMS_INDEX_COUNTER;
		GStruct33.JobParams[Int_4] = new GStruct33.GClass741();
		Gstruct30_0 = new GStruct30[128];
		Gstruct30_1 = new GStruct30[128];
		PhysicsCustomOverlapBoxSystem.Instance.RegisterSearcher(this);
	}

	public int? RegisterCollider(BoxCollider collider)
	{
		if (HashSet_0.Contains(collider))
		{
			return null;
		}
		int num = ++Int_5;
		Dictionary_0[num] = collider;
		HashSet_0.Add(collider);
		List_0.Add(method_0(num, collider));
		Bool_0 = true;
		if (List_0.Count >= 2048)
		{
			HaveCriticalTriggersCount = false;
		}
		return num;
	}

	public void SyncCollider(int id, BoxCollider collider)
	{
		int num = List_0.FindIndex((GClass739 t) => t.Id == id);
		if (num != -1)
		{
			GClass739 value = method_0(id, collider);
			List_0[num] = value;
			Bool_0 = true;
		}
	}

	public void RemoveCollider(BoxCollider collider)
	{
		if (HashSet_0.Contains(collider))
		{
			int id = Dictionary_0.First((KeyValuePair<int, BoxCollider> kv) => kv.Value == collider).Key;
			HashSet_0.Remove(collider);
			Dictionary_0.Remove(id);
			List_0.Remove(List_0.Find((GClass739 el) => el.Id == id));
			Bool_0 = true;
			if (List_0.Count < 2048)
			{
				HaveCriticalTriggersCount = false;
			}
		}
	}

	public void BoxOverlap(int index, Vector3 center, Vector3 halfExtents, Quaternion orientation, Collider[] result, int mask, QueryTriggerInteraction queryTriggerInteraction, EFTPhysicsClass.GClass747.GClass748 asyncData)
	{
		GStruct30 gStruct = new GStruct30(mask, queryTriggerInteraction, center, halfExtents * 2f, orientation);
		GStruct31 gStruct2 = new GStruct31
		{
			Result = result,
			AsyncData = asyncData,
			AsyncDataIndex = index
		};
		if (Int_6 == 0)
		{
			Gstruct30_0[Int_3] = gStruct;
		}
		else
		{
			Gstruct30_1[Int_3] = gStruct;
		}
		if (Int_6 == 0)
		{
			Gstruct31_0[Int_3] = gStruct2;
		}
		else
		{
			Gstruct31_1[Int_3] = gStruct2;
		}
		Int_3++;
	}

	public GStruct34 Schedule()
	{
		if (Int_3 == 0)
		{
			return default(GStruct34);
		}
		method_1();
		int num = Int_3 * List_0.Count;
		GStruct33.GClass741 gClass741_ = GClass741_0;
		if (gClass741_.Result.Length < num)
		{
			Array.Resize(ref gClass741_.Result, num);
		}
		Array.Clear(gClass741_.Result, 0, num);
		gClass741_.Commands = ((Int_6 == 0) ? Gstruct30_0 : Gstruct30_1);
		gClass741_.Count = Int_3;
		GStruct33 gStruct = new GStruct33
		{
			Index = Int_4
		};
		JobHandle handle = IJobParallelForExtensions.Schedule(gStruct, gStruct.GetExecutionLength(), 1);
		GStruct34 result = new GStruct34
		{
			CommandsCount = Int_3,
			Job = gStruct,
			Handle = handle,
			Searcher = this
		};
		method_3();
		return result;
	}

	public void WaitScheduleComplete(GStruct34 scheduled)
	{
		scheduled.Handle.Complete();
		GStruct33 job = scheduled.Job;
		GStruct33.GClass741 gClass = GStruct33.JobParams[job.Index];
		int count = gClass.Count;
		int num = gClass.Triggers.Length;
		for (int i = 0; i < count; i++)
		{
			GStruct31 gStruct = GStruct31_0[i];
			if (num == 0)
			{
				gStruct.AsyncData.OneWorldCompleteCallback(gStruct.AsyncDataIndex, 0);
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				int num2 = GClass832.Get2D(gClass.Result, count, i, j);
				if (num2 != 0)
				{
					gStruct.Result[j] = method_2(num2);
				}
				if (num2 == 0 || j + 1 == num)
				{
					gStruct.AsyncData.OneWorldCompleteCallback(gStruct.AsyncDataIndex, (num2 == 0) ? j : (j + 1));
					break;
				}
			}
		}
	}

	public void Dispose()
	{
		PhysicsCustomOverlapBoxSystem.Instance.UnregisterSearcher(this);
	}

	public void Clear()
	{
		GStruct33.JobParams[Int_4] = null;
		Gstruct30_0 = null;
		Gstruct30_1 = null;
	}

	public GClass739 method_0(int id, BoxCollider collider)
	{
		Transform transform = collider.transform;
		Vector3 vector = Vector3.Scale(transform.lossyScale, collider.center);
		Vector3 center = transform.position + collider.transform.rotation * vector;
		Vector3 size = Vector3.Scale(transform.lossyScale, collider.size);
		return new GClass739(id, collider.gameObject.layer, collider.isTrigger, center, size, collider.transform.rotation);
	}

	public void method_1()
	{
		if (Bool_0)
		{
			GClass741_0.Triggers = List_0.ToArray();
			Bool_0 = false;
		}
	}

	public BoxCollider method_2(int id)
	{
		Dictionary_0.TryGetValue(id, out var value);
		return value;
	}

	public void method_3()
	{
		Int_3 = 0;
		Int_6 = ((Int_6 == 0) ? 1 : 0);
	}
}
