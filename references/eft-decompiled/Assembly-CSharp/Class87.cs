using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class87<T, U>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public U Gparam_1;

	public T keycard => Gparam_0;

	public U closureKeycard => Gparam_1;

	[DebuggerHidden]
	public Class87(T keycard, U closureKeycard)
	{
		Gparam_0 = keycard;
		Gparam_1 = closureKeycard;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class87<T, U> @class = value as Class87<T, U>;
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
		return (2110227163 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[2];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		return string.Format(null, "{{ keycard = {0}, closureKeycard = {1} }}", array);
	}
}
