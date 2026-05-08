using System;
using Diz.Binding;

public class GClass1651<T, U, V, W, X, Y, Z, T7> : GClass1644<T7>, IHandler
{
	[NonSerialized]
	public IBindable<T> Ibindable_0;

	[NonSerialized]
	public IBindable<U> Ibindable_1;

	[NonSerialized]
	public IBindable<V> Ibindable_2;

	[NonSerialized]
	public IBindable<W> Ibindable_3;

	[NonSerialized]
	public IBindable<X> Ibindable_4;

	[NonSerialized]
	public IBindable<Y> Ibindable_5;

	[NonSerialized]
	public IBindable<Z> Ibindable_6;

	[NonSerialized]
	public Func<T, U, V, W, X, Y, Z, T7> Func_0;

	public GClass1651(IBindable<T> b1, IBindable<U> b2, IBindable<V> b3, IBindable<W> b4, IBindable<X> b5, IBindable<Y> b6, IBindable<Z> b7, Func<T, U, V, W, X, Y, Z, T7> projection)
	{
		Ibindable_0 = b1;
		Ibindable_1 = b2;
		Ibindable_2 = b3;
		Ibindable_3 = b4;
		Ibindable_4 = b5;
		Ibindable_5 = b6;
		Ibindable_6 = b7;
		Func_0 = projection;
		Gparam_0 = projection(b1.Value, b2.Value, b3.Value, b4.Value, b5.Value, b6.Value, b7.Value);
	}

	public void CheckChanges()
	{
		method_0(Func_0(Ibindable_0.Value, Ibindable_1.Value, Ibindable_2.Value, Ibindable_3.Value, Ibindable_4.Value, Ibindable_5.Value, Ibindable_6.Value));
	}

	public override void Connect()
	{
		method_3(Ibindable_0.BindWithoutValue(this), Ibindable_1.BindWithoutValue(this), Ibindable_2.BindWithoutValue(this), Ibindable_3.BindWithoutValue(this), Ibindable_4.BindWithoutValue(this), Ibindable_5.BindWithoutValue(this), Ibindable_6.BindWithoutValue(this));
	}
}
