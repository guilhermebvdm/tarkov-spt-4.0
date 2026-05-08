using System;
using System.Runtime.CompilerServices;

public class GClass822
{
	[NonSerialized]
	[CompilerGenerated]
	public float Float_0;

	[NonSerialized]
	public float[] Float_1;

	[NonSerialized]
	public int Int_0;

	public float Sum
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

	public GClass822(int capacity)
	{
		Float_1 = new float[capacity];
	}

	public void Add(float value)
	{
		Sum = Sum - Float_1[Int_0] + value;
		Float_1[Int_0] = value;
		Int_0 = (Int_0 + 1) % Float_1.Length;
	}
}
