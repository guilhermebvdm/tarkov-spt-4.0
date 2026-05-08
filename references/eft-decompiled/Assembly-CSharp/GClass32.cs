using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public abstract class GClass32
{
	[NonSerialized]
	public List<GStruct9> List_0 = new List<GStruct9>();

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string>();

	[NonSerialized]
	public bool Bool_0 = true;

	[NonSerialized]
	public bool Bool_1 = true;

	[NonSerialized]
	[CompilerGenerated]
	public string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	[CompilerGenerated]
	public string String_1;

	[NonSerialized]
	[CompilerGenerated]
	public string String_2;

	[NonSerialized]
	[CompilerGenerated]
	public string String_3;

	[NonSerialized]
	[CompilerGenerated]
	public string String_4;

	[NonSerialized]
	[CompilerGenerated]
	public string String_5;

	public string Name
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
	}

	public int NodeSame
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
		[CompilerGenerated]
		set
		{
			Int_0 = value;
		}
	}

	public int ReasonSame
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public float LastPeriod
	{
		[CompilerGenerated]
		get
		{
			return Float_0;
		}
		[CompilerGenerated]
		set
		{
			Float_0 = value;
		}
	}

	public string LastNode
	{
		[CompilerGenerated]
		get
		{
			return String_1;
		}
		[CompilerGenerated]
		set
		{
			String_1 = value;
		}
	}

	public string PrevNode
	{
		[CompilerGenerated]
		get
		{
			return String_2;
		}
		[CompilerGenerated]
		set
		{
			String_2 = value;
		}
	}

	public string EndReason
	{
		[CompilerGenerated]
		get
		{
			return String_3;
		}
		[CompilerGenerated]
		set
		{
			String_3 = value;
		}
	}

	public string LastReason
	{
		[CompilerGenerated]
		get
		{
			return String_4;
		}
		[CompilerGenerated]
		set
		{
			String_4 = value;
		}
	}

	public string UsingLayer
	{
		[CompilerGenerated]
		get
		{
			return String_5;
		}
		[CompilerGenerated]
		set
		{
			String_5 = value;
		}
	}

	public GClass32(string name)
	{
		String_0 = name;
	}

	public abstract string GetCustomData();

	public bool IsProblem()
	{
		if (NodeSame <= 10)
		{
			return ReasonSame > 10;
		}
		return true;
	}

	public int NodeRepeats()
	{
		if (IsProblem())
		{
			return NodeSame;
		}
		return -1;
	}

	public abstract void Update();

	public List<GStruct9> GetAllLAyersName()
	{
		return List_0;
	}

	public virtual void OnDrawGizmos()
	{
	}

	public void AddLayer(int id, string name, int priority)
	{
		GStruct9 layer = new GStruct9
		{
			id = id,
			Name = name,
			Priority = priority
		};
		method_4(layer);
	}

	public void ActivateLayer(string layer)
	{
		HashSet_0.Add(layer);
	}

	public bool IsLayerActive(string layer)
	{
		return HashSet_0.Contains(layer);
	}

	public void Deactivate(string name)
	{
		method_0(name);
	}

	public void method_0(string layer)
	{
		HashSet_0.Remove(layer);
	}

	public void method_1(string endReason)
	{
		PrevNode = LastNode;
		EndReason = endReason;
	}

	public void method_2(string lastNode, string lastReason, float time)
	{
		if (lastNode != null && lastNode.Equals(LastNode))
		{
			NodeSame++;
			if (Bool_0)
			{
				Bool_0 = false;
			}
		}
		else
		{
			NodeSame = 0;
			Bool_0 = true;
		}
		if (lastReason != null && lastReason.Equals(LastReason))
		{
			ReasonSame++;
			if (Bool_1)
			{
				Bool_1 = false;
			}
		}
		else
		{
			Bool_1 = true;
			ReasonSame = 0;
		}
		LastPeriod = time;
		LastNode = lastNode;
		LastReason = lastReason;
	}

	public void method_3(string activeLayer)
	{
		UsingLayer = activeLayer;
	}

	public void method_4(GStruct9 layer)
	{
		int num = 0;
		while (true)
		{
			if (num < List_0.Count)
			{
				GStruct9 gStruct = List_0[num];
				if (layer.Priority > gStruct.Priority)
				{
					break;
				}
				num++;
				continue;
			}
			List_0.Add(layer);
			return;
		}
		List_0.Insert(num, layer);
	}
}
