using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class0<T, U, V, W, X, Y, Z>
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

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public Z Gparam_6;

	public T location => Gparam_0;

	public U savage => Gparam_1;

	public V dt => Gparam_2;

	public W keyId => Gparam_3;

	public X raidMode => Gparam_4;

	public Y spawnPlace => Gparam_5;

	public Z transitionType => Gparam_6;

	[DebuggerHidden]
	public Class0(T location, U savage, V dt, W keyId, X raidMode, Y spawnPlace, Z transitionType)
	{
		Gparam_0 = location;
		Gparam_1 = savage;
		Gparam_2 = dt;
		Gparam_3 = keyId;
		Gparam_4 = raidMode;
		Gparam_5 = spawnPlace;
		Gparam_6 = transitionType;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class0<T, U, V, W, X, Y, Z> @class = value as Class0<T, U, V, W, X, Y, Z>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0) && EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1) && EqualityComparer<V>.Default.Equals(Gparam_2, @class.Gparam_2) && EqualityComparer<W>.Default.Equals(Gparam_3, @class.Gparam_3) && EqualityComparer<X>.Default.Equals(Gparam_4, @class.Gparam_4) && EqualityComparer<Y>.Default.Equals(Gparam_5, @class.Gparam_5))
			{
				return EqualityComparer<Z>.Default.Equals(Gparam_6, @class.Gparam_6);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return ((((((1364121452 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1)) * -1521134295 + EqualityComparer<V>.Default.GetHashCode(Gparam_2)) * -1521134295 + EqualityComparer<W>.Default.GetHashCode(Gparam_3)) * -1521134295 + EqualityComparer<X>.Default.GetHashCode(Gparam_4)) * -1521134295 + EqualityComparer<Y>.Default.GetHashCode(Gparam_5)) * -1521134295 + EqualityComparer<Z>.Default.GetHashCode(Gparam_6);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[7];
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
		Z val7 = Gparam_6;
		array[6] = ((val7 != null) ? val7.ToString() : null);
		return string.Format(null, "{{ location = {0}, savage = {1}, dt = {2}, keyId = {3}, raidMode = {4}, spawnPlace = {5}, transitionType = {6} }}", array);
	}
}
