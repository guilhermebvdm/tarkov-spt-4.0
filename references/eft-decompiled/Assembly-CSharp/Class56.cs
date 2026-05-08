using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class56<T>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	public T id => Gparam_0;

	[DebuggerHidden]
	public Class56(T id)
	{
		Gparam_0 = id;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class56<T> @class = value as Class56<T>;
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
		return -1575367405 + EqualityComparer<T>.Default.GetHashCode(Gparam_0);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[1];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		return string.Format(null, "{{ id = {0} }}", array);
	}
}
