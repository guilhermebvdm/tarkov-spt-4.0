using System;

[AttributeUsage(AttributeTargets.Field)]
public class GAttribute4 : Attribute
{
	[NonSerialized]
	public string String_0;

	public string Comment => String_0;

	public GAttribute4(string commentText)
	{
		String_0 = commentText;
	}
}
