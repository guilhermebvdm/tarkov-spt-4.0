using System;

public class GException2 : GException1
{
	public GException2(int code, string message)
		: base(code, message)
	{
	}

	public GException2(int code, string message, Exception innerException)
		: base(code, message, innerException)
	{
	}
}
