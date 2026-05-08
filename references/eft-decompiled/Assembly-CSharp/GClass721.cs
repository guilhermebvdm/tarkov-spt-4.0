using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Comfort.Common;
using UnityEngine;

public class GClass721 : LoggerClass
{
	[NonSerialized]
	public const string String_0 = "Export";

	[NonSerialized]
	public string[] String_1 = new string[6] { "Material doesn't have a", "SpeedTree materials need to be regenerated.", "no culling manager registred on this scene", "GetObject failed to find Object for Library Representation", "This MeshCollider requires the mesh () to be marked as readable in order to be usable with the given transform.", "MecanimDataWasBuilt()" };

	[NonSerialized]
	public static Dictionary<LogType, StackTraceLogType> Dictionary_0 = new Dictionary<LogType, StackTraceLogType>
	{
		{
			LogType.Assert,
			StackTraceLogType.Full
		},
		{
			LogType.Error,
			StackTraceLogType.Full
		},
		{
			LogType.Exception,
			StackTraceLogType.Full
		},
		{
			LogType.Warning,
			StackTraceLogType.Full
		},
		{
			LogType.Log,
			StackTraceLogType.None
		}
	};

	[NonSerialized]
	public static Dictionary<LogType, StackTraceLogType> Dictionary_1 = new Dictionary<LogType, StackTraceLogType>();

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_2;

	public bool Boolean_0
	{
		[CompilerGenerated]
		get
		{
			return Bool_2;
		}
		[CompilerGenerated]
		set
		{
			Bool_2 = value;
		}
	}

	public static void Setup(LoggerMode mode, bool logApplicationMessages = true)
	{
		Singleton<GClass721>.Create(new GClass721(mode, logApplicationMessages));
		foreach (KeyValuePair<LogType, StackTraceLogType> item in Dictionary_0)
		{
			Dictionary_1[item.Key] = Application.GetStackTraceLogType(item.Key);
			Application.SetStackTraceLogType(item.Key, item.Value);
		}
	}

	public static void Stop()
	{
		Singleton<GClass721>.Instance.Boolean_0 = true;
	}

	public static void Continue()
	{
		Singleton<GClass721>.Instance.Boolean_0 = false;
	}

	public GClass721(LoggerMode mode, bool logApplicationMessages)
		: base("Export", mode, logApplicationMessages)
	{
	}

	public static void Release()
	{
		if (!Singleton<GClass721>.Instantiated)
		{
			return;
		}
		GClass721 instance = Singleton<GClass721>.Instance;
		Singleton<GClass721>.Release(instance);
		instance.Dispose();
		foreach (KeyValuePair<LogType, StackTraceLogType> item in Dictionary_1)
		{
			Application.SetStackTraceLogType(item.Key, item.Value);
		}
	}

	public override void LogFormat(LogType logType, string format, params object[] args)
	{
		if (Boolean_0)
		{
			return;
		}
		if (args.Length != 0)
		{
			string text = args[0].ToString();
			for (int i = 0; i < String_1.Length; i++)
			{
				string value = String_1[i];
				if (text.Contains(value))
				{
					return;
				}
			}
		}
		base.LogFormat(logType, format, args);
	}
}
