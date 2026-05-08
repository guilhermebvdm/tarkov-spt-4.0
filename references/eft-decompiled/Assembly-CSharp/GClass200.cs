using System;
using EFT;

public abstract class GClass200<T> : GClass177<T> where T : GClass26
{
	public bool WithDoor = true;

	[NonSerialized]
	public BotObserveDataClass BotObserveDataClass;

	public GClass200(BotOwner bot)
		: base(bot)
	{
		BotObserveDataClass = BotOwner_0.Memory.botObserveData;
	}

	public DoorInteractionStatus method_0(bool deavtivatingMines = true)
	{
		method_1(deavtivatingMines);
		return BotOwner_0.DoorOpener.UpdateDoorInteractionStatus();
	}

	public void method_1(bool deavtivatingMines = true)
	{
		if (deavtivatingMines)
		{
			BotOwner_0.BewarePlantedMine.Update();
		}
		BotOwner_0.BotAvoidDangerPlaces.Update();
	}

	public void method_2()
	{
		BotOwner_0.Sprint(val: false);
		BotOwner_0.SetPose(1f);
		BotOwner_0.SetTargetMoveSpeed(1f);
	}

	public void method_3()
	{
		BotOwner_0.Sprint(val: false);
		BotOwner_0.SetPose(0f);
		BotOwner_0.SetTargetMoveSpeed(1f);
	}

	public void method_4(bool anyway)
	{
		BotObserveDataClass = BotOwner_0.Memory.botObserveData;
		BotObserveDataClass.Update();
		if (anyway || !BotObserveDataClass.IsActive)
		{
			BotObserveDataClass.SetVector();
		}
	}
}
