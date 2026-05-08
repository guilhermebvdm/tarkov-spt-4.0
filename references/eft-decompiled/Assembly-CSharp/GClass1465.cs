using UnityEngine;

public abstract class GClass1465
{
	public delegate void GDelegate45(string message);

	public static bool logged;

	public static void Log(string message, GDelegate45 logger, bool logInEditMode = false)
	{
		if ((logInEditMode || Application.isPlaying) && !logged)
		{
			logger?.Invoke(message);
			logged = true;
		}
	}

	public static void Log(string message, Transform context, bool logInEditMode = false)
	{
		if ((logInEditMode || Application.isPlaying) && !logged)
		{
			Debug.LogWarning(message, context);
			logged = true;
		}
	}
}
