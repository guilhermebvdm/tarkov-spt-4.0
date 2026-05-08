using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class BotAvoidDangerPlaces : GClass429
{
	[NonSerialized]
	public float NextCheck;

	public void Update()
	{
		if (NextCheck > Time.time || BotOwner_0.Settings.FileSettings.Mind.IGNORE_TRAP)
		{
			return;
		}
		List<IPositionPoint> nearestPlacesToAvoid = BotOwner_0.VoxelesPersonalData.CurVoxel.GetNearestPlacesToAvoid();
		if (nearestPlacesToAvoid == null || nearestPlacesToAvoid.Count == 0)
		{
			return;
		}
		if (!BotOwner_0.HasPathAndNotComplete)
		{
			NextCheck = Time.time + 0.5f;
		}
		else
		{
			if (BotOwner_0.Settings.FileSettings.Mind.IGNORE_DANGER_PLACES)
			{
				return;
			}
			float num = float.MaxValue;
			float num2 = float.MinValue;
			IPositionPoint positionPoint = null;
			IPositionPoint positionPoint2 = null;
			List<Vector3> list = new List<Vector3>();
			foreach (IPositionPoint item in nearestPlacesToAvoid)
			{
				Vector3 lhs = item.Position - BotOwner_0.Position;
				if (!(Vector3.Dot(lhs, BotOwner_0.LookDirection) < 0f) && !(Mathf.Abs(lhs.y) > 1f))
				{
					float sqrMagnitude = lhs.sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						positionPoint = item;
					}
					if (sqrMagnitude > num2)
					{
						num2 = sqrMagnitude;
						positionPoint2 = item;
					}
					list.Add(item.Position);
				}
			}
			if (positionPoint != null)
			{
				Vector3 vector = (positionPoint.Position + positionPoint2.Position) * 0.5f;
				GClass369.DebugDrawArc(BotOwner_0.Position, vector, 2f, 4f, Color.cyan);
				if (BotOwner_0.Mover.TryRelacePathAround(vector, list, out var _))
				{
					NextCheck = Time.time + 4f;
				}
				else
				{
					NextCheck = Time.time + 2f;
				}
			}
		}
	}

	public BotAvoidDangerPlaces(BotOwner owner)
		: base(owner)
	{
	}
}
