using System;
using EFT;
using UnityEngine;

public class GClass301(BotOwner bot) : GClass177<GClass26>(bot)
{
	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public Vector3 Vector3_0 = Vector3.right;

	[NonSerialized]
	public float Float_0;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (Float_0 < Time.time)
		{
			Float_0 = Time.time + 5f;
			if (Bool_0)
			{
				Vector3_0 = Vector3.right;
			}
			else
			{
				Vector3_0 = Vector3.left;
			}
			Vector3_0 = GClass855.RotateOnAngUp(Vector3_0, GClass856.Random(-20f, 20f));
			Bool_0 = !Bool_0;
		}
		GClass810.DebugArrow(BotOwner_0.Position, Vector3_0, Color.yellow, 4f);
		BotOwner_0.Steering.LookToDirection(Vector3_0, 100f);
	}
}
