using System;
using System.Collections.Generic;
using System.IO;
using EFT;
using UnityEngine;

public class GClass666
{
	[NonSerialized]
	public static Dictionary<string, StreamWriter> Dictionary_0 = new Dictionary<string, StreamWriter>(10);

	public static void AILog(string log, UnityEngine.Object context = null)
	{
		if (LoggingSettings.Instance.AILogging)
		{
			if (context == null)
			{
				Debug.Log(log);
			}
			else
			{
				Debug.Log(log, context);
			}
		}
	}

	public static void WriteLog(BotOwner owner, string log)
	{
		if (!GClass842.DisabledForNow && LoggingSettings.Instance.AILogging && !(owner == null) && Application.isEditor)
		{
			string path = Application.streamingAssetsPath + owner.name;
			if (!File.Exists(path))
			{
				StreamWriter streamWriter = new StreamWriter(path);
				streamWriter.AutoFlush = true;
				Dictionary_0.Add(owner.name, streamWriter);
			}
			if (Dictionary_0.TryGetValue(owner.name, out var value))
			{
				log += "\n";
				value.Write(log);
			}
		}
	}
}
