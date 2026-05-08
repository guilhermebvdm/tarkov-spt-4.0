using System.IO;
using UnityEngine;

public abstract class GClass930
{
	public static readonly string Path = System.IO.Path.Combine(Application.temporaryCachePath, "Icon Cache");

	public static void ClearIconCache()
	{
		if (Directory.Exists(Path))
		{
			Directory.Delete(Path, recursive: true);
			Debug.Log("Icon cache cleared successfully");
		}
		else
		{
			Debug.Log("Cache has been already cleared");
		}
	}
}
