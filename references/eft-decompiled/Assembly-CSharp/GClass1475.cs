using System;
using System.Diagnostics;
using FlyingWormConsole3.LiteNetLib;
using UnityEngine;

public abstract class GClass1475
{
	public static GInterface142 Logger = null;

	[NonSerialized]
	public static object Object_0 = new object();

	public static void smethod_0(NetLogLevel logLevel, string str, params object[] args)
	{
		lock (Object_0)
		{
			if (Logger == null)
			{
				UnityEngine.Debug.Log(string.Format(str, args));
			}
			else
			{
				Logger.WriteNet(logLevel, str, args);
			}
		}
	}

	[Conditional("DEBUG_MESSAGES")]
	public static void smethod_1(string str, params object[] args)
	{
		smethod_0(NetLogLevel.Trace, str, args);
	}

	[Conditional("DEBUG_MESSAGES")]
	public static void smethod_2(NetLogLevel level, string str, params object[] args)
	{
		smethod_0(level, str, args);
	}

	[Conditional("DEBUG_MESSAGES")]
	[Conditional("DEBUG")]
	public static void smethod_3(string str, params object[] args)
	{
		smethod_0(NetLogLevel.Trace, str, args);
	}

	[Conditional("DEBUG_MESSAGES")]
	[Conditional("DEBUG")]
	public static void smethod_4(NetLogLevel level, string str, params object[] args)
	{
		smethod_0(level, str, args);
	}

	public static void smethod_5(string str, params object[] args)
	{
		smethod_0(NetLogLevel.Error, str, args);
	}
}
