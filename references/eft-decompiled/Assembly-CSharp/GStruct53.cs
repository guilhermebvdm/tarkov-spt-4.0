using System.Runtime.InteropServices;
using EFT;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GStruct53 : ISerializer<Vector3?>
{
	public Vector3? Deserialize()
	{
		return default(GStruct52).Deserialize();
	}

	public object Serialize(Vector3? t)
	{
		if (!t.HasValue)
		{
			return null;
		}
		return default(GStruct52).Serialize(t.Value);
	}
}
