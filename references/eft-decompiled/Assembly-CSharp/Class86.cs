using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class86<T, U, V>
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

	public T Action => Gparam_0;

	public U item => Gparam_1;

	public V to => Gparam_2;

	[DebuggerHidden]
	public Class86(T Action, U item, V to)
	{
		Gparam_0 = Action;
		Gparam_1 = item;
		Gparam_2 = to;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class86<T, U, V> @class = value as Class86<T, U, V>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0) && EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1))
			{
				return EqualityComparer<V>.Default.Equals(Gparam_2, @class.Gparam_2);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return ((-604627066 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[3];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		V val3 = Gparam_2;
		array[2] = ((val3 != null) ? val3.ToString() : null);
		return string.Format(null, "{{ Action = {0}, item = {1}, to = {2} }}", array);
	}
}
