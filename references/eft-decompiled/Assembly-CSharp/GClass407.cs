using System.Collections.Generic;
using System.Linq;
using System.Text;
using EFT;

public class GClass407
{
	public BotDifficulty[] Dif;

	public WildSpawnType[] Roles;

	public int[] Counts;

	public int TotalCount;

	public int LoadCount;

	public int TotalBackups;

	public int LastSecondsPeriod;

	public void SetData(GClass624<WildSpawnType, BotDifficulty, GClass679> listRole)
	{
		List<GClass623<WildSpawnType, BotDifficulty, GClass679>> list = listRole.GetAllVals().ToList();
		Roles = new WildSpawnType[list.Count];
		Counts = new int[Roles.Length];
		Dif = new BotDifficulty[Roles.Length];
		int num = 0;
		foreach (GClass623<WildSpawnType, BotDifficulty, GClass679> item in list)
		{
			Roles[num] = item.PrimaryKey;
			Counts[num] = item.Value.Val;
			Dif[num] = item.SecondaryKey;
			num++;
		}
		TotalCount = list.Count;
	}

	public static GClass407 Deserialize(IDataReader stream)
	{
		GClass407 gClass = new GClass407();
		gClass.TotalCount = stream.ReadInt32();
		gClass.LoadCount = stream.ReadInt32();
		gClass.TotalBackups = stream.ReadInt32();
		gClass.LastSecondsPeriod = stream.ReadInt32();
		gClass.Roles = new WildSpawnType[gClass.TotalCount];
		gClass.Counts = new int[gClass.TotalCount];
		gClass.Dif = new BotDifficulty[gClass.TotalCount];
		for (int i = 0; i < gClass.TotalCount; i++)
		{
			gClass.Roles[i] = (WildSpawnType)stream.ReadInt32();
			gClass.Counts[i] = stream.ReadInt32();
			gClass.Dif[i] = (BotDifficulty)stream.ReadInt32();
		}
		return gClass;
	}

	public string MessageInfo()
	{
		if (TotalCount <= 0)
		{
			return "totalCount 0";
		}
		if (Counts != null && Counts.Length != 0)
		{
			StringBuilder stringBuilder = new StringBuilder($"Load process:{LoadCount} Total:{TotalBackups}  Last:{LastSecondsPeriod}");
			stringBuilder.AppendLine();
			for (int i = 0; i < Counts.Length; i++)
			{
				int num = Counts[i];
				WildSpawnType wildSpawnType = Roles[i];
				BotDifficulty botDifficulty = Dif[i];
				stringBuilder.AppendLine($"role:{wildSpawnType.ToString()}, {num} ,{botDifficulty.ToString()}");
			}
			return stringBuilder.ToString();
		}
		return "no cache";
	}

	public static void Serialize(GClass407 data, GInterface131 stream)
	{
		stream.Write(data.TotalCount);
		stream.Write(data.LoadCount);
		stream.Write(data.TotalBackups);
		stream.Write(data.LastSecondsPeriod);
		for (int i = 0; i < data.Roles.Length; i++)
		{
			stream.Write((int)data.Roles[i]);
			stream.Write(data.Counts[i]);
			stream.Write((int)data.Dif[i]);
		}
	}
}
