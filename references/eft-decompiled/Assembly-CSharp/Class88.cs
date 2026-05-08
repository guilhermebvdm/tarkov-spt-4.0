using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class88<T, U>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public U Gparam_1;

	public T Prop_0 => Gparam_0;

	public U keyName => Gparam_1;

	[DebuggerHidden]
	public Class88(T gparam_2, U keyName)
	{
		Gparam_0 = gparam_2;
		Gparam_1 = keyName;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class88<T, U> @class = value as Class88<T, U>;
		if (this != @class)
		{
			if (@class != null && EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0))
			{
				return EqualityComparer<U>.Default.Equals(Gparam_1, @class.Gparam_1);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return (-1873897747 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[2];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		return string.Format(null, "{{ <>h__TransparentIdentifier0 = {0}, keyName = {1} }}", array);
	}
}
