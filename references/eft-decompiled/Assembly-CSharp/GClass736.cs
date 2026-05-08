using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class GClass736
{
	public class GClass737
	{
		[NonSerialized]
		public GClass738 Gclass738_0;

		[NonSerialized]
		[CompilerGenerated]
		public RaycastCommand RaycastCommand_0;

		[NonSerialized]
		[CompilerGenerated]
		public RaycastHit RaycastHit_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_0;

		[NonSerialized]
		[CompilerGenerated]
		public bool Bool_1;

		public RaycastCommand Command
		{
			[CompilerGenerated]
			get
			{
				return RaycastCommand_0;
			}
			[CompilerGenerated]
			set
			{
				RaycastCommand_0 = value;
			}
		}

		public RaycastHit Result
		{
			[CompilerGenerated]
			get
			{
				return RaycastHit_0;
			}
			[CompilerGenerated]
			set
			{
				RaycastHit_0 = value;
			}
		}

		public bool Cocked
		{
			[CompilerGenerated]
			get
			{
				return Bool_0;
			}
			[CompilerGenerated]
			set
			{
				Bool_0 = value;
			}
		}

		public bool ResultIsActual
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

		public GClass737(GClass738 root)
		{
			Gclass738_0 = root;
		}

		public void SetNewCommand(RaycastCommand newCommand)
		{
			ResultIsActual = false;
			Cocked = true;
			Command = newCommand;
		}

		public void SetResult(RaycastHit result)
		{
			ResultIsActual = true;
			Cocked = false;
			Result = result;
		}

		public void CheckCompleteBatch()
		{
			Gclass738_0.CheckCompleteBatch();
		}

		public void Dispose()
		{
			Gclass738_0.UnregisterContainer(this);
		}
	}

	public class GClass738
	{
		[NonSerialized]
		public string String_0;

		[NonSerialized]
		public List<GClass737> List_0 = new List<GClass737>();

		[NonSerialized]
		public JobHandle JobHandle_0;

		[NonSerialized]
		public NativeArray<RaycastCommand> NativeArray_0;

		[NonSerialized]
		public NativeArray<RaycastHit> NativeArray_1;

		[NonSerialized]
		public bool Bool_0;

		public GClass738(string batchid)
		{
			String_0 = batchid;
		}

		public void RegisterContainer(GClass737 container)
		{
			if (Bool_0)
			{
				throw new Exception("Trying to register new container while executing batch!");
			}
			List_0.Add(container);
		}

		public void UnregisterContainer(GClass737 container)
		{
			if (Bool_0)
			{
				throw new Exception("Trying to unregister new container while executing batch!");
			}
			List_0.Remove(container);
		}

		public void ExecuteBatch()
		{
			if (Bool_0 && !JobHandle_0.IsCompleted)
			{
				JobHandle_0.Complete();
			}
			if (NativeArray_0.IsCreated)
			{
				NativeArray_0.Dispose();
				NativeArray_1.Dispose();
			}
			int num = 0;
			for (int i = 0; i < List_0.Count; i++)
			{
				if (List_0[i].Cocked)
				{
					num++;
				}
			}
			if (num <= 0)
			{
				return;
			}
			Bool_0 = true;
			NativeArray_0 = new NativeArray<RaycastCommand>(num, Allocator.TempJob);
			NativeArray_1 = new NativeArray<RaycastHit>(num, Allocator.TempJob);
			int j = 0;
			int num2 = 0;
			for (; j < List_0.Count; j++)
			{
				GClass737 gClass = List_0[j];
				if (gClass.Cocked)
				{
					NativeArray_0[num2] = gClass.Command;
					num2++;
				}
			}
			JobHandle_0 = RaycastCommand.ScheduleBatch(NativeArray_0, NativeArray_1, 4, JobHandle_0);
		}

		public void CheckCompleteBatch()
		{
			if (!Bool_0)
			{
				return;
			}
			JobHandle_0.Complete();
			Bool_0 = false;
			int i = 0;
			int num = 0;
			for (; i < List_0.Count; i++)
			{
				GClass737 gClass = List_0[i];
				if (gClass.Cocked)
				{
					gClass.SetResult(NativeArray_1[num]);
					num++;
				}
			}
			NativeArray_0.Dispose();
			NativeArray_1.Dispose();
		}

		public void Dispose()
		{
			NativeArray_0.Dispose();
			NativeArray_1.Dispose();
		}
	}

	[NonSerialized]
	public Dictionary<string, GClass738> Dictionary_0 = new Dictionary<string, GClass738>();

	[NonSerialized]
	public static GClass736 Gclass736_0;

	public static GClass736 Instance
	{
		get
		{
			if (Gclass736_0 == null)
			{
				Gclass736_0 = new GClass736();
			}
			return Gclass736_0;
		}
	}

	public GClass737 CreateContainerForBatch(string batchId)
	{
		if (!Dictionary_0.TryGetValue(batchId, out var value))
		{
			value = new GClass738(batchId);
			Dictionary_0[batchId] = value;
		}
		GClass737 gClass = new GClass737(value);
		value.RegisterContainer(gClass);
		return gClass;
	}

	public void ExecuteBatch(string batchId)
	{
		if (Dictionary_0.TryGetValue(batchId, out var value))
		{
			value.ExecuteBatch();
		}
	}

	public void ManuallyCompleteBatch(string batchId)
	{
		if (Dictionary_0.TryGetValue(batchId, out var value))
		{
			value.CheckCompleteBatch();
		}
	}
}
