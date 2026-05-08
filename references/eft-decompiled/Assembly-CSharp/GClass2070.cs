using System;

public class GClass2070<T> where T : GInterface119
{
	[NonSerialized]
	public const float Float_0 = 0.25f;

	[NonSerialized]
	public T[] Gparam_0;

	[NonSerialized]
	public int Int_0;

	public GClass2070()
	{
		Gparam_0 = new T[2];
	}

	public void Add(T message)
	{
		Gparam_0[1] = Gparam_0[0];
		Gparam_0[0] = message;
		Int_0++;
	}

	public bool TryExtrapolate(float localTime, Func<T, T, float, T> interpolateFunc, out T snapshot)
	{
		snapshot = default(T);
		if (Int_0 < Gparam_0.Length)
		{
			return false;
		}
		double num = (double)localTime - Gparam_0[0].localTime;
		if (num > 0.25)
		{
			return false;
		}
		float num2 = (float)(num / (Gparam_0[0].remoteTime - Gparam_0[1].remoteTime)) * 0.9f;
		snapshot = interpolateFunc(Gparam_0[1], Gparam_0[0], 1f + num2);
		return true;
	}
}
