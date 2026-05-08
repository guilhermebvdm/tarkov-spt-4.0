using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NLog;

public class GClass651
{
	public class GClass711 : LoggerClass
	{
		public GClass711()
			: base(LogManager.GetLogger("backend"), LoggerMode.Add)
		{
		}
	}

	public enum EConnectionType
	{
		Default,
		HttpConnection
	}

	public const string SESSION_COOKIE_KEY = "PHPSESSID=";

	public const string FAILED_TO_RECEIVE_DATA = "Failed to receive data";

	public static readonly GClass711 Logger = new GClass711();

	public static long NextRequestIndex;

	[NonSerialized]
	public static Class320 Class320_0 = new Class320();

	[NonSerialized]
	public static Class319 Class319_0 = new Class319();

	[NonSerialized]
	public static EConnectionType EconnectionType_0 = EConnectionType.Default;

	[NonSerialized]
	public static Dictionary<EConnectionType, Interface2> Dictionary_0 = new Dictionary<EConnectionType, Interface2>
	{
		{
			EConnectionType.Default,
			Class320_0
		},
		{
			EConnectionType.HttpConnection,
			Class319_0
		}
	};

	[NonSerialized]
	[CompilerGenerated]
	public static string String_0;

	[NonSerialized]
	[CompilerGenerated]
	public static string String_1;

	public static TimeSpan EmptyResponseRepeatDelay => TimeSpan.FromSeconds(10.0);

	public static int EmptyResponseLimit => 5;

	public static string PhpSessionId
	{
		[CompilerGenerated]
		get
		{
			return String_0;
		}
		[CompilerGenerated]
		set
		{
			String_0 = value;
		}
	}

	public static string AppVersion
	{
		[CompilerGenerated]
		get
		{
			return String_1;
		}
		[CompilerGenerated]
		set
		{
			String_1 = value;
		}
	}

	public static Interface2 Interface2_0 => Dictionary_0[EConnectionType.Default];

	public static void SetSenderType(EConnectionType type)
	{
		EconnectionType_0 = type;
	}
}
