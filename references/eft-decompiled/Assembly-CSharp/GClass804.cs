using System;

public class GClass804
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1 = float.NaN;

	public float Value => Float_1;

	public GClass804(int lookBack)
	{
		Float_0 = 2f / (float)(lookBack + 1);
	}

	public float NextValue(float value)
	{
		return Float_1 = (float.IsNaN(Float_1) ? value : ((value - Float_1) * Float_0 + Float_1));
	}
}
