using System;

public class GException0 : Exception
{
	public GException0(string message)
		: base(message)
	{
	}

	public GException0(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
