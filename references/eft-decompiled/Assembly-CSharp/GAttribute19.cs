using System;

public class GAttribute19 : Attribute
{
	[NonSerialized]
	public Type Type_0;

	public GAttribute19(Type convertedType)
	{
		Type_0 = convertedType;
	}

	public GInterface122 GetConverter()
	{
		if (!typeof(GInterface122).IsAssignableFrom(Type_0))
		{
			throw new Exception($"{Type_0} supposed to implement {typeof(GInterface122)}");
		}
		return Activator.CreateInstance(Type_0) as GInterface122;
	}
}
