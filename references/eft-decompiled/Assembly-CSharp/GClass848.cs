using System;

public class GClass848<T>
{
	[NonSerialized]
	public Func<T> Func_0;

	public virtual T Value => Func_0();

	public GClass848(Func<T> updateFunc)
	{
		Func_0 = updateFunc;
	}

	public static implicit operator T(GClass848<T> compute)
	{
		return compute.Value;
	}

	public virtual void SetDirty()
	{
	}

	public void Dispose()
	{
		Func_0 = null;
	}
}
