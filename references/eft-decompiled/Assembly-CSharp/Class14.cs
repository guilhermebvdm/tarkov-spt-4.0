using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class14<T, U, V>
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

	public U tid => Gparam_1;

	public V repairItems => Gparam_2;

	[DebuggerHidden]
	public Class14(T Action, U tid, V repairItems)
	{
		Gparam_0 = Action;
		Gparam_1 = tid;
		Gparam_2 = repairItems;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class14<T, U, V> @class = value as Class14<T, U, V>;
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
		return ((-1988389134 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2);
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
		return string.Format(null, "{{ Action = {0}, tid = {1}, repairItems = {2} }}", array);
	}
}
