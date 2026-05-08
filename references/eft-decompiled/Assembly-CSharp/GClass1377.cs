using UnityEngine;

public abstract class GClass1377
{
	public static void Serialize(this ISerializer stream, ref Vector3 value, GClass1378 settings)
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
