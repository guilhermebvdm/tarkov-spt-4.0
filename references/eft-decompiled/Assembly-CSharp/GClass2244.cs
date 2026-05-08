using EFT.NetworkPackets;
using UnityEngine;

public class GClass2244
{
	public GStruct387? SyncPositionPacket;

	public GStruct263? SwitchRenderersPacket;

	public GStruct261? ChangeSkillLevelPacket;

	public GStruct262? ChangeMasteringLevelPacket;

	public override string ToString()
	{
		return string.Format("{0}: {1}\n", "SyncPositionPacket", SyncPositionPacket) + string.Format("{0}: {1}\n", "SwitchRenderersPacket", SwitchRenderersPacket) + string.Format("{0}: {1}\n", "ChangeSkillLevelPacket", ChangeSkillLevelPacket) + string.Format("{0}: {1}", "ChangeMasteringLevelPacket", ChangeMasteringLevelPacket);
	}

	public static GClass2244 Deserialize(IDataReader reader)
	{
		return new GClass2244
		{
			SyncPositionPacket = GClass2084.DeserializeSyncPositionPacket(reader),
			SwitchRenderersPacket = GClass2084.DeserializeSwitchRenderersPacket(reader),
			ChangeSkillLevelPacket = GClass2084.DeserializeChangeSkillExperiencePacket(reader),
			ChangeMasteringLevelPacket = GClass2084.DeserializeChangeMasteringLevelPacket(reader)
		};
	}

	public void AddChangeSkillLevelPacket(byte skillId, float value, float effectiveness, float pointsEarned)
	{
		GStruct261 child = new GStruct261
		{
			SkillId = skillId,
			Value = value,
			Effectiveness = effectiveness,
			PointsEarned = pointsEarned
		};
		GClass2243.AttachToOrUpdate(ref child, ref ChangeSkillLevelPacket);
	}

	public void AddChangeMasteringLevelPacket(string groupName, float value)
	{
		GStruct262 child = new GStruct262
		{
			GroupName = groupName,
			Value = value
		};
		GClass2243.AttachToOrUpdate(ref child, ref ChangeMasteringLevelPacket);
	}

	public void AddSyncPositionPacket(byte iteration, Vector3 position, SyncPositionReason reason)
	{
		SyncPositionPacket = new GStruct387
		{
			Iteration = iteration,
			Position = position,
			Reason = reason
		};
	}

	public void AddSwitchRenderersPacket(bool value)
	{
		SwitchRenderersPacket = new GStruct263
		{
			Value = value
		};
	}
}
