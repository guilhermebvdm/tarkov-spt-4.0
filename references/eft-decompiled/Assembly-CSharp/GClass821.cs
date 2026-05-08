using System;
using System.Runtime.CompilerServices;

public class GClass821
{
	[NonSerialized]
	public GClass822 Gclass822_0;

	[NonSerialized]
	public GClass822 Gclass822_1;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	public int Capacity
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

	public float Speed
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

	public GClass821(int capacity)
	{
		Capacity = capacity;
		Gclass822_0 = new GClass822(Capacity);
		Gclass822_1 = new GClass822(Capacity);
	}

	public void Add(float distance, float time)
	{
		Gclass822_0.Add(distance);
		Gclass822_1.Add(time);
		Speed = Gclass822_0.Sum / Gclass822_1.Sum;
	}

	public void Clear()
	{
		Gclass822_0 = new GClass822(Capacity);
		Gclass822_1 = new GClass822(Capacity);
		Speed = 0f;
	}
}
