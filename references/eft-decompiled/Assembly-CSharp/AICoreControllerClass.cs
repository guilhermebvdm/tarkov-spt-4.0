using System;
using System.Collections.Generic;

public class AICoreControllerClass
{
	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public HashSet<GClass32> HashSet_0 = new HashSet<GClass32>();

	[NonSerialized]
	public HashSet<GClass32> HashSet_1 = new HashSet<GClass32>();

	[NonSerialized]
	public HashSet<GClass32> HashSet_2 = new HashSet<GClass32>();

	public void AddNodeController(GClass32 aiCoreAgentM)
	{
		HashSet_0.Add(aiCoreAgentM);
	}

	public void RemoveNodeController(GClass32 nodesController)
	{
		HashSet_1.Add(nodesController);
	}

	public HashSet<GClass32> ListAgents()
	{
		return HashSet_0;
	}

	public void Update()
	{
		if (!Bool_0)
		{
			return;
		}
		if (HashSet_1.Count > 0)
		{
			foreach (GClass32 item in HashSet_1)
			{
				HashSet_0.Remove(item);
			}
		}
		foreach (GClass32 item2 in HashSet_0)
		{
			try
			{
				item2.Update();
			}
			catch (Exception)
			{
				if (!HashSet_2.Contains(item2) && Int_0 < 10)
				{
					HashSet_2.Add(item2);
					Int_0++;
				}
			}
		}
	}

	public void Stop()
	{
		Bool_0 = false;
	}

	public void Activate()
	{
		Bool_0 = true;
	}

	public void method_0()
	{
		HashSet_0.Clear();
		HashSet_2.Clear();
	}

	public void OnDrawGizmos()
	{
		foreach (GClass32 item in HashSet_0)
		{
			item.OnDrawGizmos();
		}
	}
}
