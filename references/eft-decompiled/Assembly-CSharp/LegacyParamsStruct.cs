using System;
using System.Runtime.CompilerServices;

public struct LegacyParamsStruct
{
	public string Url;

	public bool UseCache;

	public object DefaultObjectInCaseOfCache;

	public string CacheId;

	public bool RunInMainThread;

	public bool ParseInBackground;

	public int TimeoutSeconds;

	public byte? Retries;

	public ERequestHandlingMode HandlingMode;

	public bool RequireResponse;

	[NonSerialized]
	[CompilerGenerated]
	public object Object_0;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_0 = 120;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_1 = 20;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_2 = 40;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_3 = 60;

	[NonSerialized]
	[CompilerGenerated]
	public static int Int_4 = 10;

	[NonSerialized]
	[CompilerGenerated]
	public static byte Byte_0 = 3;

	[NonSerialized]
	[CompilerGenerated]
	public static byte Byte_1 = 5;

	public object Params
	{
		[CompilerGenerated]
		readonly get
		{
			return Object_0;
		}
		[CompilerGenerated]
		set
		{
			Object_0 = value;
		}
	}

	public static int DefaultTimeoutSeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_0;
		}
	}

	public static byte? NoRetries => null;

	public static int FirstCycleDelaySeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public static int SecondCycleDelaySeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_2;
		}
		[CompilerGenerated]
		set
		{
			Int_2 = value;
		}
	}

	public static int NextCycleDelaySeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_3;
		}
		[CompilerGenerated]
		set
		{
			Int_3 = value;
		}
	}

	public static int AdditionalRandomDelaySeconds
	{
		[CompilerGenerated]
		get
		{
			return Int_4;
		}
		[CompilerGenerated]
		set
		{
			Int_4 = value;
		}
	}

	public static byte DefaultRetries
	{
		[CompilerGenerated]
		get
		{
			return Byte_0;
		}
		[CompilerGenerated]
		set
		{
			Byte_0 = value;
		}
	}

	public static byte MaximumRetries
	{
		[CompilerGenerated]
		get
		{
			return Byte_1;
		}
		[CompilerGenerated]
		set
		{
			Byte_1 = value;
		}
	}
}
