using System;
using NLog;
using UnityEngine;

public abstract class GClass861
{
	public class Class390 : LoggerClass
	{
		public Class390(LoggerMode mode)
			: base(LogManager.GetLogger("assets"), mode)
		{
		}
	}

	[NonSerialized]
	public static Class390 Class390_0 = new Class390(LoggerMode.Add);

	public static UnityEngine.Object Load(string path)
	{
		Class390_0.LogInfo("Resources.Load Object: {0}", path);
		return Resources.Load(path);
	}

	public static T Load<T>(string path) where T : UnityEngine.Object
	{
		Class390_0.LogInfo("Resources.Load<{0}> : {1}", typeof(T), path);
		return Resources.Load<T>(path);
	}
}
