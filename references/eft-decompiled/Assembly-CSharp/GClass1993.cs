using System.Linq;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using EFT.Quests;

public class GClass1993 : GClass1990
{
	[CompilerGenerated]
	public class Class1456
	{
		public GClass1993 gclass1993_0;

		public Player player;

		public bool method_0(QuestClass q)
		{
			return q.Id.Equals(gclass1993_0.QuestId);
		}

		public Item method_1(MongoID id)
		{
			return player.InventoryController.FindItem<Item>(id);
		}
	}

	public MongoID[] ItemIds;

	public MongoID ConditionId;

	public override GStruct152<BaseInventoryOperationClass> ToQuestOperation(Player player)
	{
		if (!player.IsInBufferZone)
		{
			return new GClass1522("Cant handover item. Player not in buffer zone");
		}
		QuestClass questClass = player.AbstractQuestControllerClass.Quests.FirstOrDefault((QuestClass q) => q.Id.Equals(QuestId));
		if (questClass == null)
		{
			return new GClass1522($"Handover cant find quest with id {QuestId}");
		}
		Condition condition = questClass.GetCondition(ConditionId);
		ConditionItem condition2 = condition as ConditionItem;
		if (condition == null)
		{
			return new GClass1522($"Handover cant find condition {ConditionId}");
		}
		Item[] items;
		try
		{
			items = ItemIds.Select((MongoID id) => player.InventoryController.FindItem<Item>(id)).ToArray();
		}
		catch (GException16)
		{
			return new GClass1522("Quest handover cant find items");
		}
		return new GClass3522(base.OperationId, questClass, items, condition2, player.InventoryController, player.AbstractQuestControllerClass);
	}
}
