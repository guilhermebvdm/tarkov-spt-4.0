using System;

public class GClass570
{
	[NonSerialized]
	public float[] Float_0 = new float[4] { 45f, 135f, -45f, -135f };

	[NonSerialized]
	public int Int_0;

	public float GetOffset()
	{
		Int_0++;
		if (Int_0 >= 4)
		{
			Int_0 = 0;
		}
		return Float_0[Int_0];
	}
}
