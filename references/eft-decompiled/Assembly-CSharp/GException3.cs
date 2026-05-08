using System;

public class GException3 : GException1
{
	public GException3(int code, string message)
		: base(code, message)
	{
	}

	public GException3(int code, string message, Exception innerException)
		: base(code, message, innerException)
	{
	}
}
