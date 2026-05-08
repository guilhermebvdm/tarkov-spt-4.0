using System;
using Diz.Binding;

public abstract class GClass1641
{
	public static GClass1645<T1, T> Combine<T1, T>(IBindable<T1> b1, Func<T1, T> projection)
	{
		return new GClass1645<T1, T>(b1, projection);
	}

	public static GClass1646<T1, T2, T> Combine<T1, T2, T>(IBindable<T1> b1, IBindable<T2> b2, Func<T1, T2, T> projection)
	{
		return new GClass1646<T1, T2, T>(b1, b2, projection);
	}

	public static GClass1647<T1, T2, T3, T> Combine<T1, T2, T3, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, Func<T1, T2, T3, T> projection)
	{
		return new GClass1647<T1, T2, T3, T>(b1, b2, b3, projection);
	}

	public static GClass1648<T1, T2, T3, T4, T> Combine<T1, T2, T3, T4, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, Func<T1, T2, T3, T4, T> projection)
	{
		return new GClass1648<T1, T2, T3, T4, T>(b1, b2, b3, b4, projection);
	}

	public static GClass1649<T1, T2, T3, T4, T5, T> Combine<T1, T2, T3, T4, T5, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, IBindable<T5> b5, Func<T1, T2, T3, T4, T5, T> projection)
	{
		return new GClass1649<T1, T2, T3, T4, T5, T>(b1, b2, b3, b4, b5, projection);
	}

	public static GClass1650<T1, T2, T3, T4, T5, T6, T> Combine<T1, T2, T3, T4, T5, T6, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, IBindable<T5> b5, IBindable<T6> b6, Func<T1, T2, T3, T4, T5, T6, T> projection)
	{
		return new GClass1650<T1, T2, T3, T4, T5, T6, T>(b1, b2, b3, b4, b5, b6, projection);
	}

	public static GClass1651<T1, T2, T3, T4, T5, T6, T7, T> Combine<T1, T2, T3, T4, T5, T6, T7, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, IBindable<T5> b5, IBindable<T6> b6, IBindable<T7> b7, Func<T1, T2, T3, T4, T5, T6, T7, T> projection)
	{
		return new GClass1651<T1, T2, T3, T4, T5, T6, T7, T>(b1, b2, b3, b4, b5, b6, b7, projection);
	}

	public static GClass1652<T1, T2, T3, T4, T5, T6, T7, T8, T> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, IBindable<T5> b5, IBindable<T6> b6, IBindable<T7> b7, IBindable<T8> b8, Func<T1, T2, T3, T4, T5, T6, T7, T8, T> projection)
	{
		return new GClass1652<T1, T2, T3, T4, T5, T6, T7, T8, T>(b1, b2, b3, b4, b5, b6, b7, b8, projection);
	}

	public static GClass1653<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> Combine<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3, IBindable<T4> b4, IBindable<T5> b5, IBindable<T6> b6, IBindable<T7> b7, IBindable<T8> b8, IBindable<T9> b9, Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T> projection)
	{
		return new GClass1653<T1, T2, T3, T4, T5, T6, T7, T8, T9, T>(b1, b2, b3, b4, b5, b6, b7, b8, b9, projection);
	}
}
