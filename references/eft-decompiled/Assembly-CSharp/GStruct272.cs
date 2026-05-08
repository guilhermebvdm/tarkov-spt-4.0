using EFT;
using EFT.Quests;

public struct GStruct272(int questId, EQuestStatus status, MongoID conditionId, double value, bool notify)
{
	public int QuestId = questId;

	public EQuestStatus Status = status;

	public MongoID ConditionId = conditionId;

	public double Value = value;

	public bool Notify = notify;
}
