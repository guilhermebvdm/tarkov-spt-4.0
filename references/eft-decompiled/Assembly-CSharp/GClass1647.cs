using System;
using Diz.Binding;

public class GClass1647<T, U, V, W> : GClass1644<W>, IHandler
{
	[NonSerialized]
	public IBindable<T> Ibindable_0;

	[NonSerialized]
	public IBindable<U> Ibindable_1;

	[NonSerialized]
	public IBindable<V> Ibindable_2;

	[NonSerialized]
	public Func<T, U, V, W> Func_0;

	public GClass1647(IBindable<T> b1, IBindable<U> b2, IBindable<V> b3, Func<T, U, V, W> projection)
	{
		Ibindable_0 = b1;
		Ibindable_1 = b2;
		Ibindable_2 = b3;
		Func_0 = projection;
		Gparam_0 = projection(b1.Value, b2.Value, b3.Value);
	}

	public void CheckChanges()
	{
		method_0(Func_0(Ibindable_0.Value, Ibindable_1.Value, Ibindable_2.Value));
	}

	public override void Connect()
	{
		method_3(Ibindable_0.BindWithoutValue(this), Ibindable_1.BindWithoutValue(this), Ibindable_2.BindWithoutValue(this));
	}
}
