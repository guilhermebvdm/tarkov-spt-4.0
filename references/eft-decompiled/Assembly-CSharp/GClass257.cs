using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass257(BotOwner bot) : GClass200<GClass26>(bot)
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	public Vector3 Vector3_2 = Vector3.positiveInfinity;

	[NonSerialized]
	public bool Bool_0;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_6();
		method_5();
		method_0();
		BotOwner_0.GetPlayer.Physical.Stamina.Current = 10000f;
		BotOwner_0.Sprint(val: true);
		if (!(Float_0 < Time.time))
		{
			return;
		}
		Float_0 = Time.time + 1f;
		if (BotOwner_0.HasPathAndNotComplete)
		{
			Vector3 vector = BotOwner_0.Position - BotOwner_0.Mover.TargetPoint.Value;
			if (Mathf.Abs(vector.y) < 0.5f)
			{
				vector.y = 0f;
			}
			if (vector.magnitude < 1f)
			{
				BotOwner_0.StopMove();
			}
		}
		else
		{
			float x = GClass856.Random(Vector3_0.x, Vector3_1.x);
			float y = GClass856.Random(Vector3_0.y, Vector3_1.y);
			float z = GClass856.Random(Vector3_0.z, Vector3_1.z);
			if (NavMesh.SamplePosition(new Vector3(x, y, z), out var hit, 100f, -1) && !BotRun.CalcPathToMoveZigZagAndGo(hit.position, BotOwner_0, EZigZAgType.allSteps))
			{
				BotOwner_0.StopMove();
			}
		}
	}

	public void method_5()
	{
		if (!Bool_0)
		{
			AICoversData aICoversData = AICoversData.CreateOrFind(undestandAtGame: true);
			Vector3_1 = aICoversData.MaxVoxelesValues;
			Vector3_0 = aICoversData.MinVoxelesValues;
			Bool_0 = true;
		}
	}

	public void method_6()
	{
		if (Float_1 > Time.time)
		{
			return;
		}
		Float_1 = Time.time + 5f;
		if (BotOwner_0.HasPathAndNotComplete)
		{
			if ((BotOwner_0.Position - Vector3_2).sqrMagnitude < 0.1f)
			{
				Debug.LogError($"Looks like bot:{BotOwner_0.Id} STUCK at position:{Vector3_2}");
			}
			Vector3_2 = BotOwner_0.Position;
		}
	}
}
