using System;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;

public class GClass526 : GClass429
{
	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	public bool CanDropItem
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
		[CompilerGenerated]
		set
		{
			Bool_0 = value;
		}
	}

	public GClass526(BotOwner owner)
		: base(owner)
	{
	}

	public void Activate()
	{
		BotOwner_0.PatrollingData.OnPatrolPointCome += method_1;
		BotOwner_0.ItemDropper.OnItemDrop += method_0;
	}

	public void method_0(Item obj)
	{
		CanDropItem = false;
	}

	public void method_1(PatrolPointContainer obj)
	{
		CanDropItem = true;
	}

	public void Dispose()
	{
		BotOwner_0.ItemDropper.OnItemDrop -= method_0;
		BotOwner_0.PatrollingData.OnPatrolPointCome -= method_1;
	}
}
