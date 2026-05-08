using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class72<T, U, V, W>
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

	public T Action => Gparam_0;

	public U areaType => Gparam_1;

	public V items => Gparam_2;

	public W timestamp => Gparam_3;

	[DebuggerHidden]
	public Class72(T Action, U areaType, V items, W timestamp)
	{
		Gparam_0 = Action;
		Gparam_1 = areaType;
		Gparam_2 = items;
		Gparam_3 = timestamp;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class72<T, U, V, W> @class = value as Class72<T, U, V, W>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0) && EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1) && EqualityComparer<V>.Default.Equals(Gparam_2, @class.Gparam_2))
			{
				return EqualityComparer<W>.Default.Equals(Gparam_3, @class.Gparam_3);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return (((-942164463 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2)) * -1521134295 + EqualityComparer<W>.Default.GetHashCode(Gparam_3);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[4];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		V val3 = Gparam_2;
		array[2] = ((val3 != null) ? val3.ToString() : null);
		W val4 = Gparam_3;
		array[3] = ((val4 != null) ? val4.ToString() : null);
		return string.Format(null, "{{ Action = {0}, areaType = {1}, items = {2}, timestamp = {3} }}", array);
	}
}
