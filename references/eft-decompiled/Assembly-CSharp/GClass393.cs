using System;
using UnityEngine;

public class GClass393 : IAICorePointLink
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public AICorePoint AicorePoint_0;

	[NonSerialized]
	public GroupPoint GroupPoint_0;

	public int OwnerId => Int_0;

	public AICorePoint CorePointInGame => AicorePoint_0;

	public Vector3 Position => GroupPoint_0.Position;

	public bool IsSpotted
	{
		get
		{
			if (Bool_1)
			{
				return true;
			}
			if (Bool_0)
			{
				if (Float_0 < Time.time)
				{
					Bool_0 = false;
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public GClass393(AICorePoint corePoint, GroupPoint parent)
	{
		GroupPoint_0 = parent;
		AicorePoint_0 = corePoint;
	}

	public bool IsFreeById(int botId)
	{
		if (OwnerId <= 0)
		{
			return true;
		}
		return OwnerId == botId;
	}

	public void Spotted(float period)
	{
		if (Bool_0)
		{
			float a = Time.time + period;
			Float_0 = Mathf.Max(a, Float_0);
		}
		else
		{
			Bool_0 = true;
			Float_0 = Time.time + period;
		}
	}

	public void Block()
	{
		Bool_1 = true;
	}

	public void Unblock()
	{
		Bool_1 = false;
	}

	public void SetOwner(int id)
	{
		Int_0 = id;
	}
}
