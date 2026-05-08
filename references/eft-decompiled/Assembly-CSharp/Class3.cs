using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class3<T>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	public T sessionMode => Gparam_0;

	[DebuggerHidden]
	public Class3(T sessionMode)
	{
		Gparam_0 = sessionMode;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class3<T> @class = value as Class3<T>;
		if (this != @class)
		{
			if (@class != null)
			{
				return EqualityComparer<T>.Default.Equals(Gparam_0, @class.Gparam_0);
			}
			return false;
		}
		return true;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return -731416897 + EqualityComparer<T>.Default.GetHashCode(Gparam_0);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[1];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		return string.Format(null, "{{ sessionMode = {0} }}", array);
	}
}
