using System;
using System.Text;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass700
{
	[NonSerialized]
	public string String_0;

	[NonSerialized]
	public string String_1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_0;

	public GClass700(ISpawnPoint rnd, float sqrDist)
	{
		String_0 = rnd.Id;
		String_1 = rnd.Name;
		Vector3_0 = rnd.Position;
		Float_0 = sqrDist;
	}

	public StringBuilder DebugInfo()
	{
		StringBuilder stringBuilder = new StringBuilder("_id:" + String_0);
		stringBuilder.Append("_name:" + String_1 + " ");
		stringBuilder.Append($"_pos:{Vector3_0} ");
		stringBuilder.Append($"_sqrDist:{Float_0} ");
		return stringBuilder;
	}
}
