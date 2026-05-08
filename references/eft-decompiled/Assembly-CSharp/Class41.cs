using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class41<T>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	public T profileId => Gparam_0;

	[DebuggerHidden]
	public Class41(T profileId)
	{
		Gparam_0 = profileId;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class41<T> @class = value as Class41<T>;
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
		return -657504754 + EqualityComparer<T>.Default.GetHashCode(Gparam_0);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[1];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		return string.Format(null, "{{ profileId = {0} }}", array);
	}
}
