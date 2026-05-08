using System.Diagnostics;
using UnityEngine;

public abstract class GClass865
{
	public static void Log(string message)
	{
		UnityEngine.Debug.LogError(message);
	}

	public static void LogTrace(uint deepLimit = 6u)
	{
		try
		{
			StackTrace stackTrace = new StackTrace();
			string text = string.Empty;
			deepLimit = ((deepLimit > stackTrace.FrameCount) ? ((uint)stackTrace.FrameCount) : deepLimit);
			for (int i = 0; i < deepLimit; i++)
			{
				text = text + stackTrace.GetFrame(i).GetMethod().Name + ".";
			}
			UnityEngine.Debug.LogError(text ?? "");
		}
		catch
		{
			UnityEngine.Debug.LogError("Methods not found");
		}
	}
}
