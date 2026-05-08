using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class2<T, U, V, W, X, Y>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public U Gparam_1;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public V Gparam_2;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public W Gparam_3;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public X Gparam_4;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public Y Gparam_5;

	public T email => Gparam_0;

	public U pass => Gparam_1;

	public V version => Gparam_2;

	public W device_id => Gparam_3;

	public X develop => Gparam_4;

	public Y sec => Gparam_5;

	[DebuggerHidden]
	public Class2(T email, U pass, V version, W device_id, X develop, Y sec)
	{
		Gparam_0 = email;
		Gparam_1 = pass;
		Gparam_2 = version;
		Gparam_3 = device_id;
		Gparam_4 = develop;
		Gparam_5 = sec;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class2<T, U, V, W, X, Y> @class = value as Class2<T, U, V, W, X, Y>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0) && EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1) && EqualityComparer<V>.Default.Equals(Gparam_2, @class.Gparam_2) && EqualityComparer<W>.Default.Equals(Gparam_3, @class.Gparam_3) && EqualityComparer<X>.Default.Equals(Gparam_4, @class.Gparam_4))
			{
				return EqualityComparer<Y>.Default.Equals(Gparam_5, @class.Gparam_5);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return (((((1193385397 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2)) * -1521134295 + EqualityComparer<W>.Default.GetHashCode(Gparam_3)) * -1521134295 + EqualityComparer<X>.Default.GetHashCode(Gparam_4)) * -1521134295 + EqualityComparer<Y>.Default.GetHashCode(Gparam_5);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[6];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		V val3 = Gparam_2;
		array[2] = ((val3 != null) ? val3.ToString() : null);
		W val4 = Gparam_3;
		array[3] = ((val4 != null) ? val4.ToString() : null);
		X val5 = Gparam_4;
		array[4] = ((val5 != null) ? val5.ToString() : null);
		Y val6 = Gparam_5;
		array[5] = ((val6 != null) ? val6.ToString() : null);
		return string.Format(null, "{{ email = {0}, pass = {1}, version = {2}, device_id = {3}, develop = {4}, sec = {5} }}", array);
	}
}
