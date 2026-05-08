using System;

public class BotSpawnParams
{
	public string Id_spawn;

	[field: NonSerialized]
	public SpawnTriggerType TriggerType { get; set; }

	[field: NonSerialized]
	public ShallBeGroupParams ShallBeGroup { get; set; }

	public string DebugInfo()
	{
		return $"TriggerType:{TriggerType.ToString()}   ID: {Id_spawn}   ShallBeGroup:{ShallBeGroup}";
	}
}
