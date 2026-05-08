using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BitPacking;
using EFT;
using UnityEngine;

public struct GStruct21
{
	[Serializable]
	[CompilerGenerated]
	public class Class133
	{
		public static readonly Class133 class133_0 = new Class133();

		public static Func<KeyValuePair<IPlayer, BotSettingsClass>, bool> func_0;

		public bool method_0(KeyValuePair<IPlayer, BotSettingsClass> x)
		{
			return x.Value.IsHaveSeen;
		}
	}

	public static List<string> GroupsNames = new List<string>
	{
		"Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta", "Eta", "Theta", "Iota ", "Kappa",
		"Lambda", "Mu", "Nu", "Xi", "Omicron", "Pi", "Rho", "Sigma", "Tau", "Upsilon",
		"Phi", "Chi", "Psi", "Omega"
	};

	[NonSerialized]
	public static Dictionary<char, string> Dictionary_0 = new Dictionary<char, string>
	{
		{ 'а', "a" },
		{ 'б', "b" },
		{ 'в', "v" },
		{ 'г', "g" },
		{ 'д', "d" },
		{ 'е', "e" },
		{ 'ё', "yo" },
		{ 'ж', "zh" },
		{ 'з', "z" },
		{ 'и', "i" },
		{ 'й', "j" },
		{ 'к', "k" },
		{ 'л', "l" },
		{ 'м', "m" },
		{ 'н', "n" },
		{ 'о', "o" },
		{ 'п', "p" },
		{ 'р', "r" },
		{ 'с', "s" },
		{ 'т', "t" },
		{ 'у', "u" },
		{ 'ф', "f" },
		{ 'х', "h" },
		{ 'ц', "c" },
		{ 'ч', "ch" },
		{ 'ш', "sh" },
		{ 'щ', "sch" },
		{ 'ъ', "j" },
		{ 'ы', "i" },
		{ 'ь', "j" },
		{ 'э', "e" },
		{ 'ю', "yu" },
		{ 'я', "ya" },
		{ 'А', "A" },
		{ 'Б', "B" },
		{ 'В', "V" },
		{ 'Г', "G" },
		{ 'Д', "D" },
		{ 'Е', "E" },
		{ 'Ё', "Yo" },
		{ 'Ж', "Zh" },
		{ 'З', "Z" },
		{ 'И', "I" },
		{ 'Й', "J" },
		{ 'К', "K" },
		{ 'Л', "L" },
		{ 'М', "M" },
		{ 'Н', "N" },
		{ 'О', "O" },
		{ 'П', "P" },
		{ 'Р', "R" },
		{ 'С', "S" },
		{ 'Т', "T" },
		{ 'У', "U" },
		{ 'Ф', "F" },
		{ 'Х', "H" },
		{ 'Ц', "C" },
		{ 'Ч', "Ch" },
		{ 'Ш', "Sh" },
		{ 'Щ', "Sch" },
		{ 'Ъ', "J" },
		{ 'Ы', "I" },
		{ 'Ь', "J" },
		{ 'Э', "E" },
		{ 'Ю', "Yu" },
		{ 'Я', "Ya" }
	};

	public EPlayerSide Side;

	public string name;

	public int Members;

	public int Enemies;

	public int ActiveEnemies;

	public int Requests;

	public int DeadBodies;

	public int PlacesForCheckCount;

	public int DangerArea;

	public int enemiesCount;

	public float[] powers;

	public string[] ids;

	public string[] members;

	public string[] enemies;

	public string[] neutrals;

	public string[] allies;

	[NonSerialized]
	public static Dictionary<string, string> Dictionary_1 = new Dictionary<string, string>();

	public static string smethod_0(string source)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in source)
		{
			if (Dictionary_0.TryGetValue(c, out var value))
			{
				stringBuilder.Append(value);
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public GStruct21(BotsGroup groupData)
	{
		Side = groupData.Side;
		Requests = groupData.RequestsController.RequestsCount;
		DeadBodies = groupData.DeadBodiesController.BodiesCount;
		Members = groupData.MembersCount;
		Enemies = groupData.Enemies.Keys.Count;
		ActiveEnemies = groupData.CurrentEnemies.ActiveEnemies;
		DangerArea = groupData.DangerAreasCount;
		PlacesForCheckCount = groupData.PlacesForCheck.Count;
		name = groupData.Name;
		List<IPlayer> list = new List<IPlayer>();
		foreach (KeyValuePair<IPlayer, BotSettingsClass> item in groupData.Enemies.Where((KeyValuePair<IPlayer, BotSettingsClass> x) => x.Value.IsHaveSeen))
		{
			list.Add(item.Key);
		}
		enemiesCount = list.Count;
		powers = new float[enemiesCount];
		ids = new string[enemiesCount];
		for (int num = 0; num < list.Count; num++)
		{
			IPlayer player = list[num];
			ids[num] = player.Profile.Id;
			powers[num] = player.AIData.PowerOfEquipment;
		}
		members = new string[groupData.MembersCount];
		enemies = new string[groupData.Enemies.Count];
		neutrals = new string[groupData.Neutrals.Count];
		allies = new string[groupData.AllyCount];
		for (int num2 = 0; num2 < groupData.MembersCount; num2++)
		{
			members[num2] = ConvertToLatinic(groupData.Member(num2).name);
		}
		for (int num3 = 0; num3 < groupData.AllyCount; num3++)
		{
			if (groupData.Allies[num3].AIData != null && groupData.Allies[num3].AIData.IsAI)
			{
				allies[num3] = ConvertToLatinic(groupData.Allies[num3].AIData.BotOwner.name);
			}
			else
			{
				allies[num3] = ConvertToLatinic(groupData.Allies[num3].Profile.Nickname);
			}
		}
		int num4 = 0;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in groupData.Enemies)
		{
			if (enemy.Value.Player.AIData != null && enemy.Value.Player.AIData.IsAI)
			{
				enemies[num4] = ConvertToLatinic(enemy.Value.Player.AIData.BotOwner.name);
			}
			else
			{
				enemies[num4] = ConvertToLatinic(enemy.Value.Player.Profile.Nickname);
			}
			num4++;
		}
		num4 = 0;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> neutral in groupData.Neutrals)
		{
			if (neutral.Value.Player.AIData != null && neutral.Value.Player.AIData.IsAI)
			{
				neutrals[num4] = ConvertToLatinic(neutral.Value.Player.AIData.BotOwner.name);
			}
			else
			{
				neutrals[num4] = ConvertToLatinic(neutral.Value.Player.Profile.Nickname);
			}
			num4++;
		}
	}

	public static string ConvertToLatinic(string toConvert)
	{
		if (Dictionary_1.TryGetValue(toConvert, out var value))
		{
			return value;
		}
		bool flag = false;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in toConvert)
		{
			if (c >= 'a' && c <= 'z')
			{
				stringBuilder.Append(c);
				continue;
			}
			flag = true;
			if (Dictionary_0.TryGetValue(c, out var value2))
			{
				stringBuilder.Append(value2);
			}
		}
		string text = ((!flag) ? toConvert : stringBuilder.ToString());
		Dictionary_1.Add(toConvert, text);
		return text;
	}

	public string MessageInfo()
	{
		return $"{name}   M: {Members} Enemies: {Enemies}({ActiveEnemies}) Check: {PlacesForCheckCount}  B:{DeadBodies}  R:{Requests} Side:{Side.ToString()} DA:{DangerArea}";
	}

	public static void Serialize(GStruct21 data, GInterface131 stream)
	{
		stream.WriteLimitedString(data.name, ' ', 'z', BitPackingTag.DebugGroupStructName, 1600u);
		stream.WriteLimitedInt32(data.Enemies, 0, 63, BitPackingTag.DebugGroupStructA);
		stream.WriteLimitedInt32(data.ActiveEnemies, 0, 63, BitPackingTag.DebugGroupStructG);
		stream.WriteLimitedInt32(data.DangerArea, 0, 63, BitPackingTag.DebugGroupStructF);
		stream.WriteLimitedInt32(data.Members, 0, 63, BitPackingTag.DebugGroupStructB);
		stream.WriteLimitedInt32(Mathf.Clamp(data.PlacesForCheckCount, 0, 510), 0, 511, BitPackingTag.DebugGroupStructC);
		stream.WriteLimitedInt32((int)data.Side, 0, 8, BitPackingTag.DebugGroupStructD);
		stream.Write(data.enemiesCount);
		for (int i = 0; i < data.enemiesCount; i++)
		{
			stream.WriteLimitedString(data.ids[i], ' ', 'z', BitPackingTag.DebugGroupStructEnemiesCountId, 1600u);
			stream.Write((int)data.powers[i]);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int j = 0; j < data.members.Length; j++)
		{
			stringBuilder.Append(data.members[j]);
			stringBuilder.Append(" ");
		}
		stream.WriteLimitedString(stringBuilder.ToString(), ' ', 'z', BitPackingTag.DebugGroupStructGE, 1600u);
		stringBuilder.Clear();
		for (int k = 0; k < data.enemies.Length; k++)
		{
			stringBuilder.Append(data.enemies[k]);
			stringBuilder.Append(" ");
		}
		stream.WriteLimitedString(stringBuilder.ToString(), ' ', 'z', BitPackingTag.DebugGroupStructGE, 1600u);
		stringBuilder.Clear();
		for (int l = 0; l < data.neutrals.Length; l++)
		{
			stringBuilder.Append(data.neutrals[l]);
			stringBuilder.Append(" ");
		}
		stream.WriteLimitedString(stringBuilder.ToString(), ' ', 'z', BitPackingTag.DebugGroupStructGN, 1600u);
		stringBuilder.Clear();
		for (int m = 0; m < data.allies.Length; m++)
		{
			stringBuilder.Append(data.allies[m]);
			stringBuilder.Append(" ");
		}
		stream.WriteLimitedString(stringBuilder.ToString(), ' ', 'z', BitPackingTag.DebugGroupStructGA, 1600u);
	}

	public static GStruct21 Deserialize(IDataReader stream)
	{
		GStruct21 result = default(GStruct21);
		result.name = stream.ReadLimitedString(' ', 'z', 1600u);
		result.Enemies = stream.ReadLimitedInt32(0, 63);
		result.ActiveEnemies = stream.ReadLimitedInt32(0, 63);
		result.DangerArea = stream.ReadLimitedInt32(0, 63);
		result.Members = stream.ReadLimitedInt32(0, 63);
		result.PlacesForCheckCount = stream.ReadLimitedInt32(0, 511);
		result.Side = (EPlayerSide)stream.ReadLimitedInt32(0, 8);
		result.enemiesCount = stream.ReadInt32();
		result.ids = new string[result.enemiesCount];
		result.powers = new float[result.enemiesCount];
		for (int i = 0; i < result.enemiesCount; i++)
		{
			result.ids[i] = stream.ReadLimitedString(' ', 'z', 1600u);
			result.powers[i] = stream.ReadInt32();
		}
		string text = stream.ReadLimitedString(' ', 'z', 1600u);
		string text2 = stream.ReadLimitedString(' ', 'z', 1600u);
		string text3 = stream.ReadLimitedString(' ', 'z', 1600u);
		string text4 = stream.ReadLimitedString(' ', 'z', 1600u);
		result.members = text.Split(' ');
		result.enemies = text2.Split(' ');
		result.neutrals = text3.Split(' ');
		result.allies = text4.Split(' ');
		return result;
	}
}
