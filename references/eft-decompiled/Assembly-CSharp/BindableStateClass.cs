using System.Collections.Generic;
using JetBrains.Annotations;

public class BindableStateClass<T> : GClass1642<T>
{
	public override T Value
	{
		set
		{
			method_0(value);
		}
	}

	public BindableStateClass()
		: base((IEqualityComparer<T>)null)
	{
	}

	public BindableStateClass([CanBeNull] T initialValue, IEqualityComparer<T> equalityComparer = null)
		: base(equalityComparer)
	{
		Gparam_0 = initialValue;
	}
}
