using System;
using EFT;
using UnityEngine;

public class GClass201 : GClass200<GClass26>
{
	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public const float Float_1 = 5.29f;

	[NonSerialized]
	public GClass25 Gclass25_0;

	[NonSerialized]
	public bool Bool_0 = true;

	public GClass201(BotOwner bot)
		: base(bot)
	{
		Gclass25_0 = new GClass25(1f, method_5);
	}

	public void method_5()
	{
		PlantedMineAIInfo deactivatingPlace = BotOwner_0.BewarePlantedMine.DeactivatingPlace;
		if (deactivatingPlace != null)
		{
			Vector3 vector = deactivatingPlace.Pos - BotOwner_0.Position;
			Vector3 position = BotOwner_0.Position;
			Vector3 end = BotOwner_0.Position + vector * 0.8f;
			Bool_0 = Physics.Linecast(position, end, LayerMaskClass.HighPolyWithTerrainMask);
		}
	}

	public override void UpdateNodeByBrain(GClass26 data)
	{
		if (!BotOwner_0.BewarePlantedMine.CanDeactivate())
		{
			return;
		}
		PlantedMineAIInfo deactivatingPlace = BotOwner_0.BewarePlantedMine.DeactivatingPlace;
		if (deactivatingPlace == null)
		{
			return;
		}
		deactivatingPlace.SetDeactivate(BotOwner_0.Id);
		float sqrMagnitude = (deactivatingPlace.Pos - BotOwner_0.Position).sqrMagnitude;
		BotOwner_0.Sprint(val: false);
		if (sqrMagnitude < 5.29f)
		{
			Gclass25_0.Update();
			if (!Bool_0)
			{
				method_6();
				BotOwner_0.SetPose(0.1f);
				BotOwner_0.StopMove();
				BotOwner_0.Steering.LookToPoint(deactivatingPlace.Pos);
			}
		}
		else
		{
			method_7(deactivatingPlace.Pos);
		}
		method_0(deavtivatingMines: false);
	}

	public void method_6()
	{
		BotOwner_0.BewarePlantedMine.DoDeactivateCurrent();
	}

	public void method_7(Vector3 pos)
	{
		if (Float_0 < Time.time)
		{
			BotOwner_0.Mover.GoToPoint(pos, slowAtTheEnd: false, 0.5f);
			Float_0 = Time.time + 5f;
		}
	}
}
