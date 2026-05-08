using System;
using Diz.Binding;

public class GClass1645<T, U> : GClass1644<U>, IHandler
{
	[NonSerialized]
	public IBindable<T> Ibindable_0;

	[NonSerialized]
	public Func<T, U> Func_0;

	public GClass1645(IBindable<T> b1, Func<T, U> projection)
	{
		Ibindable_0 = b1;
		Func_0 = projection;
		Gparam_0 = projection(b1.Value);
	}

	public void CheckChanges()
	{
		method_0(Func_0(Ibindable_0.Value));
	}

	public override void Connect()
	{
		method_3(Ibindable_0.BindWithoutValue(this));
	}
}
