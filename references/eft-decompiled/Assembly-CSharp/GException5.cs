using System;

public class GException5 : GException3
{
	public GException5(string message)
		: base(9001, message)
	{
	}

	public GException5(string message, Exception innerException)
		: base(9001, message, innerException)
	{
	}
}
