using UnityEngine;

public abstract class GClass1376
{
	public static void Serialize(this ISerializer stream, ref Vector2 value, GClass1375 settings)
	{
		if (stream is IDataReader bitReaderStream)
		{
			settings.Read(bitReaderStream, out value);
		}
		else
		{
			settings.Write((GInterface131)stream, value);
		}
	}
}
