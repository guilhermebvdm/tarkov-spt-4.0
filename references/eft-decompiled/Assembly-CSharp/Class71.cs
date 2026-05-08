using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
public class Class71<T, U>
{
	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Gparam_0;

	[NonSerialized]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public U Gparam_1;

	public T Action => Gparam_0;

	public U timestamp => Gparam_1;

	[DebuggerHidden]
	public Class71(T Action, U timestamp)
	{
		Gparam_0 = Action;
		Gparam_1 = timestamp;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		Class71<T, U> @class = value as Class71<T, U>;
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
		return (645378412 + EqualityComparer<T>.Default.GetHashCode(Gparam_0)) * -1521134295 + EqualityComparer<U>.Default.GetHashCode(Gparam_1);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		object[] array = new object[2];
		T val = Gparam_0;
		array[0] = ((val != null) ? val.ToString() : null);
		U val2 = Gparam_1;
		array[1] = ((val2 != null) ? val2.ToString() : null);
		return string.Format(null, "{{ Action = {0}, timestamp = {1} }}", array);
	}
}
