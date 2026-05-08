using System;
using UnityEngine;

public class GClass550 : BotCurrentPathAbstractClass
{
	public new string DebugInfo;

	[NonSerialized]
	public GClass554 Gclass554_0;

	[NonSerialized]
	public Action<GInterface9> Action_0;

	[NonSerialized]
	public bool Bool_0;

	public override GInterface9 TargetPoint => Gclass554_0;

	public GClass550(Vector3[] path, Action<GInterface9> endPathCallback)
		: base(path)
	{
		Gclass554_0 = new GClass554(Vector3_0[path.Length - 1]);
		Action_0 = endPathCallback;
	}

	public override void DrawSelectedGizmosWay(float upCoef, Color color)
	{
		Vector3 vector = Vector3.up * upCoef;
		for (int i = 0; i < Vector3_0.Length; i++)
		{
			Vector3 vector2 = Vector3_0[i];
			if (i + 1 < Vector3_0.Length)
			{
				Gizmos.color = color;
				Vector3 vector3 = Vector3_0[i + 1];
				Gizmos.DrawLine(vector2 + vector, vector3 + vector);
			}
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(vector2 + vector, new Vector3(0.1f, 1f, 0.1f));
		}
	}

	public override void DrawDebugWay(float upCoef)
	{
		Vector3 vector = Vector3.up * upCoef;
		for (int i = 0; i < Vector3_0.Length; i++)
		{
			Vector3 vector2 = Vector3_0[i];
			if (i + 1 < Vector3_0.Length)
			{
				Vector3 vector3 = Vector3_0[i + 1];
				Debug.DrawLine(vector2 + vector, vector3 + vector, Color.magenta, 5f);
			}
		}
	}

	public override void Complete()
	{
		if (!Bool_0)
		{
			Bool_0 = true;
			Action_0(Gclass554_0);
		}
	}

	public override bool IsSame(Vector3 target, Vector3 ownerPosition)
	{
		return Gclass554_0.IsSame(new GClass554(target));
	}

	public override bool IsSame(IAICorePointLink target, Vector3 ownerPosition)
	{
		return Gclass554_0.IsSame(new GClass553(target));
	}
}
