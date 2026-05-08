using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class59<T>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	public T completed => Gparam_0;

	[DebuggerHidden]
	public Class59(T completed)
	{
		Gparam_0 = completed;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class59<T> @class = value as Class59<T>;
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
		return 2138037925 + EqualityComparer<T>.Default.GetHashCode(Gparam_0);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[1];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		return string.Format(null, "{{ completed = {0} }}", array);
	}
}
