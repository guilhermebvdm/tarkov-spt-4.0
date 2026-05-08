using EFT;
using EFT.Quests;

public struct GStruct274
{
	public MongoID ConditionId;

	public EQuestStatus Status;

	public double Value;

	public bool Notify;

	public override string ToString()
	{
		return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}", "ConditionId", ConditionId, "Status", Status, "Value", Value, "Notify", Notify);
	}
}
