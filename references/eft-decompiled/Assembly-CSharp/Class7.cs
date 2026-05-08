using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class7<T>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	public T containerId => Gparam_0;

	[DebuggerHidden]
	public Class7(T containerId)
	{
		Gparam_0 = containerId;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class7<T> @class = value as Class7<T>;
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
		return -1313119596 + EqualityComparer<T>.Default.GetHashCode(Gparam_0);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[1];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		return string.Format(null, "{{ containerId = {0} }}", array);
	}
}
