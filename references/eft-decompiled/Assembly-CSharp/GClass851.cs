using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class GClass851
{
	[Serializable]
	[CompilerGenerated]
	public class Class463<T>
	{
		public static readonly Class463<T> class463_0 = new Class463<T>();

		public static Func<T, string> func_0;

		public string method_0(T x)
		{
			return x.ToString();
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class464<T>
	{
		public static readonly Class464<T> class464_0 = new Class464<T>();

		public static Func<T, string> func_0;

		public string method_0(T x)
		{
			return x.ToString();
		}
	}

	[CompilerGenerated]
	public class Class465<T>
	{
		public Func<T, object> log;

		public string method_0(T x)
		{
			return log(x).ToString();
		}
	}

	[CompilerGenerated]
	public class Class466<T>
	{
		public string elementFormat;

		public object method_0(T arg)
		{
			return string.Format(elementFormat, arg);
		}
	}

	public static T Log_DEBUG<T>(this T obj, Func<T, object> log, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, log(obj));
		return obj;
	}

	public static T Log_DEBUG<T>(this T obj, string format, Func<T, object> log, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, string.Format(format, log(obj)));
		return obj;
	}

	public static T Log_DEBUG<T>(this T obj, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, obj);
		return obj;
	}

	public static T Log_DEBUG<T>(this T obj, string format, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, string.Format(format, obj));
		return obj;
	}

	public static IEnumerable<T> LogList<T>(this IEnumerable<T> list, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, EnumerableToString(list, "\n"));
		return list;
	}

	public static IEnumerable<T> LogList<T>(this IEnumerable<T> list, string format, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, string.Format(format, EnumerableToString(list, "\n")));
		return list;
	}

	public static IEnumerable<T> LogList<T>(this IEnumerable<T> list, string format, Func<T, object> log, LogType logType = LogType.Log)
	{
		Debug.unityLogger.Log(logType, string.Format(format, EnumerableToString(list, "\n", log)));
		return list;
	}

	public static string EnumerableToString<T>(this IEnumerable<T> list, string separator)
	{
		if (list == null)
		{
			return "NULL";
		}
		return string.Join(separator, list.Select((T x) => x.ToString()).ToArray());
	}

	public static string EnumerableToString<T>(this IEnumerable<T> list, string separator, Func<T, object> log)
	{
		if (list == null)
		{
			return "NULL";
		}
		return string.Join(separator, list.Select((T x) => log(x).ToString()).ToArray());
	}

	public static string EnumerableToString<T>(this IEnumerable<T> list, string separator, string elementFormat)
	{
		return EnumerableToString(list, separator, (T arg) => string.Format(elementFormat, arg));
	}

	public static string GetAllParams<T>(this IEnumerable<T> enumerable, string separator = ", ")
	{
		return string.Join(separator, enumerable.Select((T x) => x.ToString()).ToArray());
	}
}
