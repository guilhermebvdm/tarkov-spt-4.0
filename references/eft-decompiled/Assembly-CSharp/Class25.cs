using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class25<T, U>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public U Gparam_1;

	public T squadInviteRestriction => Gparam_0;

	public U memberCategory => Gparam_1;

	[DebuggerHidden]
	public Class25(T squadInviteRestriction, U memberCategory)
	{
		Gparam_0 = squadInviteRestriction;
		Gparam_1 = memberCategory;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class25<T, U> @class = value as Class25<T, U>;
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
		return (1358488239 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[2];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		return string.Format(null, "{{ squadInviteRestriction = {0}, memberCategory = {1} }}", array);
	}
}
