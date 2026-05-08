using System;

public class GClass1491<T> : IDisposable where T : GInterface144
{
	[NonSerialized]
	public T Gparam_0;

	public GClass1491(T lockable)
	{
		Gparam_0 = lockable;
		Gparam_0.Locked.Value = true;
	}

	public void Dispose()
	{
		T gparam_ = Gparam_0;
		gparam_.Locked.Value = false;
	}
}
