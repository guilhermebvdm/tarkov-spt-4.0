using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.Game.Spawning;
using UnityEngine;

public class GClass255 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public List<Vector3> List_0;

	[NonSerialized]
	public Vector3? Nullable_0;

	public GClass255(BotOwner bot)
		: base(bot)
	{
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		method_0();
		BotOwner_0.GetPlayer.Physical.Stamina.Current = 10000f;
		BotOwner_0.Sprint(val: true);
		if (Float_2 < Time.time)
		{
			Float_2 = Time.time + 13f;
			if (List_0 == null)
			{
				List_0 = new List<Vector3>();
				foreach (GroupPoint item in GClass856.RandomElement(BotOwner_0.BotsGroup.BotGame.BotsController.CoversData.Points.Where((GroupPoint x) => x.ConnectionGroup == BotOwner_0.StartCorePoint.ConnectionGroupId).ToList(), 30))
				{
					List_0.Add(item.Position);
				}
				foreach (SpawnPointMarker spawnPointMarker in BotOwner_0.BotsGroup.BotZone.SpawnPointMarkers)
				{
					List_0.Add(spawnPointMarker.Position);
				}
			}
			Nullable_0 = GClass856.RandomElement(List_0);
		}
		method_5();
	}

	public void method_5()
	{
		if (!(Float_0 < Time.time))
		{
			return;
		}
		Float_0 = Time.time + 3f;
		if (Nullable_0.HasValue)
		{
			BotOwner_0.GoToPoint(Nullable_0.Value);
			if (BotOwner_0.HasPathAndNotComplete && BotOwner_0.Mover.TargetPoint.HasValue && (BotOwner_0.Mover.TargetPoint.Value - Nullable_0.Value).magnitude > 0.5f)
			{
				Debug.LogError($"Final node of calculated path is too far from target  _lastTarget:{Nullable_0.Value}   TargetPoint:{BotOwner_0.Mover.TargetPoint.Value}");
			}
		}
	}

	public void method_6()
	{
		if (Float_1 < Time.time)
		{
			Float_1 = Time.time + 1f;
			if (BotOwner_0.HasPathAndNotComplete && BotOwner_0.Mover.TargetPoint.HasValue)
			{
				BotOwner_0.Mover.Teleport(BotOwner_0.Mover.TargetPoint.Value);
				Nullable_0 = null;
			}
		}
	}

	[CompilerGenerated]
	public bool method_7(GroupPoint x)
	{
		return x.ConnectionGroup == BotOwner_0.StartCorePoint.ConnectionGroupId;
	}
}
