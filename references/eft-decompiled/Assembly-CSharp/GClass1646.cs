using System;
using Diz.Binding;

public class GClass1646<T, U, V> : GClass1644<V>, IHandler
{
	[NonSerialized]
	public IBindable<T> Ibindable_0;

	[NonSerialized]
	public IBindable<U> Ibindable_1;

	[NonSerialized]
	public Func<T, U, V> Func_0;

	public GClass1646(IBindable<T> b1, IBindable<U> b2, Func<T, U, V> projection)
	{
		Ibindable_0 = b1;
		Ibindable_1 = b2;
		Func_0 = projection;
		Gparam_0 = projection(b1.Value, b2.Value);
	}

	public void CheckChanges()
	{
		method_0(Func_0(Ibindable_0.Value, Ibindable_1.Value));
	}

	public override void Connect()
	{
		method_3(Ibindable_0.BindWithoutValue(this), Ibindable_1.BindWithoutValue(this));
	}
}
