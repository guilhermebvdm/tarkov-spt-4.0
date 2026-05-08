using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class22<T, U, V, W, X>
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

	public T location => Gparam_0;

	public U savage => Gparam_1;

	public V dt => Gparam_2;

	public W servers => Gparam_3;

	public X keyId => Gparam_4;

	[DebuggerHidden]
	public Class22(T location, U savage, V dt, W servers, X keyId)
	{
		Gparam_0 = location;
		Gparam_1 = savage;
		Gparam_2 = dt;
		Gparam_3 = servers;
		Gparam_4 = keyId;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class22<T, U, V, W, X> @class = value as Class22<T, U, V, W, X>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0) && EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1) && EqualityComparer<V>.Default.Equals(Gparam_2, @class.Gparam_2) && EqualityComparer<W>.Default.Equals(Gparam_3, @class.Gparam_3))
			{
				return EqualityComparer<X>.Default.Equals(Gparam_4, @class.Gparam_4);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return ((((1280036834 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2)) * -1521134295 + EqualityComparer<W>.Default.GetHashCode(Gparam_3)) * -1521134295 + EqualityComparer<X>.Default.GetHashCode(Gparam_4);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[5];
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
		return string.Format(null, "{{ location = {0}, savage = {1}, dt = {2}, servers = {3}, keyId = {4} }}", array);
	}
}
