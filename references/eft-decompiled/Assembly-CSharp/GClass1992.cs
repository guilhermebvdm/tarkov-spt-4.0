using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1992 : GClass1990
{
	public override GStruct152<BaseInventoryOperationClass> ToQuestOperation(Player player)
	{
		if (!player.IsInBufferZone)
		{
			return new GClass1522("Cant finish quest. Player not in buffer zone");
		}
		QuestClass questClass = player.AbstractQuestControllerClass.Quests.FirstOrDefault((QuestClass q) => q.Id.Equals(QuestId));
		if (questClass == null)
		{
			return new GClass1522($"Accept quest cant find quest with id {QuestId}");
		}
		return new GClass3521(base.OperationId, player.InventoryController, player.AbstractQuestControllerClass, questClass);
	}

	[CompilerGenerated]
	public bool method_0(QuestClass q)
	{
		return q.Id.Equals(QuestId);
	}
}
