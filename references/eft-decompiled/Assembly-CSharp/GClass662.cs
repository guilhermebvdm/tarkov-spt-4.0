using System;

public abstract class GClass662<T> where T : class, new()
{
	[NonSerialized]
	public static T Gparam_0;

	public static T Instance => Gparam_0 ?? (Gparam_0 = new T());

	public GClass662()
	{
	}
}
