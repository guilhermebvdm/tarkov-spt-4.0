using System;
using System.Collections.Generic;

public abstract class GClass1315 : GClass1313
{
	[NonSerialized]
	public List<GClass1315> List_0;

	[NonSerialized]
	public List<CurrentStateAbstractClass> List_1;

	[NonSerialized]
	public int Int_0;

	public int StatesCount => List_1.Count;

	public int StatesMachinesCount => List_0.Count;

	public abstract GClass1313 ParentStateMachine { get; }

	public int DefaultStateHash => Int_0;

	public GClass1315(string name, int hash, int defaultStateHash)
		: base(name, hash)
	{
		Int_0 = defaultStateHash;
		List_1 = new List<CurrentStateAbstractClass>();
		List_0 = new List<GClass1315>();
	}

	public override void ResetCache()
	{
		for (int i = 0; i < List_0.Count; i++)
		{
			List_0[i].ResetCache();
		}
		for (int j = 0; j < List_1.Count; j++)
		{
			List_1[j].ResetCache();
		}
		base.ResetCache();
	}

	public CurrentStateAbstractClass GetState(int index)
	{
		return List_1[index];
	}

	public GClass1315 GetStateMachine(int index)
	{
		return List_0[index];
	}

	public IEnumerable<GClass1315> GetStateMachinesEnumerator()
	{
		return List_0;
	}

	public IEnumerable<CurrentStateAbstractClass> GetStatesEnumerator()
	{
		return List_1;
	}

	public CurrentStateAbstractClass FindDefaultState()
	{
		foreach (GClass1315 item in List_0)
		{
			CurrentStateAbstractClass currentStateAbstractClass = method_0(item);
			if (currentStateAbstractClass != null)
			{
				return currentStateAbstractClass;
			}
		}
		foreach (CurrentStateAbstractClass item2 in List_1)
		{
			if (item2.Hash == DefaultStateHash)
			{
				return item2;
			}
		}
		return null;
	}

	public CurrentStateAbstractClass method_0(GClass1315 stateMachine)
	{
		int num = 0;
		while (true)
		{
			if (num < stateMachine.List_1.Count)
			{
				if (stateMachine.List_1[num].Hash == DefaultStateHash)
				{
					break;
				}
				num++;
				continue;
			}
			foreach (GClass1315 item in stateMachine.List_0)
			{
				CurrentStateAbstractClass currentStateAbstractClass = method_0(item);
				if (currentStateAbstractClass != null)
				{
					return currentStateAbstractClass;
				}
			}
			return null;
		}
		return stateMachine.List_1[num];
	}

	public override void Dispose(bool disposing)
	{
		foreach (GClass1315 item in List_0)
		{
			item?.Dispose();
		}
		List_0.Clear();
		foreach (CurrentStateAbstractClass item2 in List_1)
		{
			item2?.Dispose();
		}
		List_1.Clear();
		base.Dispose(disposing);
	}
}
