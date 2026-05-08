using System;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass256(BotOwner bot) : GClass200<GClass26>(bot)
{
	public float UpdatePointTimerDelta = 10f;

	public bool WithRestart = true;

	public Transform TargetPosition;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	public override void UpdateNodeByBrain(GClass26 data)
	{
		bool val = method_0() == DoorInteractionStatus.CanRun;
		if (TargetPosition == null)
		{
			BotEventDebug botEventDebug = UnityEngine.Object.FindObjectOfType<BotEventDebug>();
			if (botEventDebug != null)
			{
				TargetPosition = botEventDebug.transform;
			}
		}
		else
		{
			if (!WithRestart && BotOwner_0.Memory.IsInCover)
			{
				return;
			}
			Float_1 -= Time.deltaTime;
			if (Float_1 < 0f)
			{
				float duration = 5f;
				Float_1 = UpdatePointTimerDelta;
				Vector3 position;
				if (NavMesh.SamplePosition(TargetPosition.position, out var hit, 10f, -1))
				{
					position = hit.position;
				}
				else
				{
					position = TargetPosition.position;
					Debug.LogError($"Target can't find nav mesh. Original position:{TargetPosition.position}");
				}
				Debug.DrawLine(BotOwner_0.Position + Vector3.up, position + Vector3.up, Color.yellow, duration);
				Debug.DrawLine(BotOwner_0.Position + Vector3.up, BotOwner_0.Position, Color.yellow, duration);
				Debug.DrawLine(position + Vector3.up, position, Color.yellow, duration);
				BotOwner_0.GoToPoint(position, slowAtTheEnd: false, -1f, getUpWithCheck: false, mustHaveWay: false);
			}
			BotOwner_0.Sprint(val);
			if (BotOwner_0.Destination.HasValue)
			{
				Vector3 vector = BotOwner_0.Destination.Value - BotOwner_0.Transform.position;
				vector.y = ((Math.Abs(vector.y) > 1f) ? vector.y : 0f);
				Float_0 = vector.magnitude;
				if (!(Float_0 > BotOwner_0.Settings.FileSettings.Move.REACH_DIST))
				{
					method_5();
				}
			}
		}
	}

	public void method_5()
	{
		BotOwner_0.SetPose(1f);
		BotOwner_0.StopMove();
	}
}
