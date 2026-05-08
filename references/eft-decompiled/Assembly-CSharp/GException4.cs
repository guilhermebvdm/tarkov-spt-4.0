using System;
using System.Net;

public class GException4 : GException3
{
	public HttpStatusCode StatusCode => (HttpStatusCode)base.Code;

	public GException4(int code, string message)
		: base(code, message)
	{
	}

	public GException4(int code, string message, Exception innerException)
		: base(code, message, innerException)
	{
	}
}
