using System;
using System.Runtime.CompilerServices;

public class GException1 : GException0
{
	[NonSerialized]
	[CompilerGenerated]
	public int Int_0;

	public int Code
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public GException1(int code, string message)
		: base(message)
	{
		Int_0 = code;
	}

	public GException1(int code, string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public override string ToString()
	{
		return Code + ", " + base.ToString();
	}
}
