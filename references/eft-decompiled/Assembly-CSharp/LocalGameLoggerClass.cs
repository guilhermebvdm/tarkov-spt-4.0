using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EFT;
using EFT.Game.Spawning;
using NLog;
using Newtonsoft.Json;
using UnityEngine;

public class LocalGameLoggerClass : LoggerClass
{
	public delegate LocalGameLoggerClass Delegate7(IDictionary<string, Player> players, IDictionary<string, Player> bots);

	public enum ActionId
	{
		PlayerSpawned,
		PlayerSpawnedWithTeleport,
		BotSpawned,
		BotSpawnedWithTeleport
	}

	public class GClass1886
	{
		public string ProfileId;

		public int PlayerId;

		public ESpawnCategory Category;

		public EPlayerSide Side;

		public Vector3 Position;

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GroupId;

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Vector3? DiscardedPosition;

		public GClass1887[] Players;

		public GClass1887[] Bots;
	}

	public class GClass1887
	{
		public string ProfileId;

		public ESpawnCategory Category;

		public EPlayerSide Side;

		public Vector3 Position;

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string GroupId;
	}

	[Serializable]
	[CompilerGenerated]
	public class Class1162
	{
		public static readonly Class1162 class1162_0 = new Class1162();

		public static Func<KeyValuePair<string, Player>, GClass1887> func_0;

		public static Func<KeyValuePair<string, Player>, bool> func_1;

		public static Func<KeyValuePair<string, Player>, GClass1887> func_2;

		public static Func<KeyValuePair<string, Player>, GClass1887> func_3;

		public static Func<KeyValuePair<string, Player>, bool> func_4;

		public static Func<KeyValuePair<string, Player>, GClass1887> func_5;

		public GClass1887 method_0(KeyValuePair<string, Player> pair)
		{
			return new GClass1887
			{
				ProfileId = pair.Key,
				Category = ESpawnCategory.Player,
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			};
		}

		public bool method_1(KeyValuePair<string, Player> pair)
		{
			if (pair.Value.HealthController != null)
			{
				return pair.Value.HealthController.IsAlive;
			}
			return false;
		}

		public GClass1887 method_2(KeyValuePair<string, Player> pair)
		{
			return new GClass1887
			{
				ProfileId = pair.Key,
				Category = (GClass2190.IsBoss(pair.Value.Profile.Info.Settings) ? ESpawnCategory.Boss : ESpawnCategory.Bot),
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			};
		}

		public GClass1887 method_3(KeyValuePair<string, Player> pair)
		{
			return new GClass1887
			{
				ProfileId = pair.Key,
				Category = ESpawnCategory.Player,
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			};
		}

		public bool method_4(KeyValuePair<string, Player> pair)
		{
			if (pair.Value.HealthController != null)
			{
				return pair.Value.HealthController.IsAlive;
			}
			return false;
		}

		public GClass1887 method_5(KeyValuePair<string, Player> pair)
		{
			return new GClass1887
			{
				ProfileId = pair.Key,
				Category = (GClass2190.IsBoss(pair.Value.Profile.Info.Settings) ? ESpawnCategory.Boss : ESpawnCategory.Bot),
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			};
		}
	}

	[NonSerialized]
	public const string String_0 = "spawns";

	[NonSerialized]
	public IDictionary<string, Player> Idictionary_0;

	[NonSerialized]
	public IDictionary<string, Player> Idictionary_1;

	[NonSerialized]
	public StringBuilder StringBuilder_1 = new StringBuilder();

	[NonSerialized]
	public StringBuilder StringBuilder_2 = new StringBuilder();

	public LocalGameLoggerClass(LoggerMode mode, IDictionary<string, Player> players, IDictionary<string, Player> bots)
		: base("spawns", mode)
	{
		Idictionary_0 = players;
		Idictionary_1 = bots;
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("SERVER")]
	public void method_3(Player player)
	{
		LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "spawns", ActionId.PlayerSpawned.ToString());
		method_8(logEventInfo.Properties, player, ESpawnCategory.Player);
		Log(logEventInfo);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("SERVER")]
	public void method_4(Player player, Vector3 discardedPosition)
	{
		LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "spawns", ActionId.PlayerSpawnedWithTeleport.ToString());
		method_8(logEventInfo.Properties, player, ESpawnCategory.Player, discardedPosition);
		Log(logEventInfo);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("SERVER")]
	public void method_5(Player player, bool isBoss)
	{
		LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "spawns", ActionId.BotSpawned.ToString());
		method_8(logEventInfo.Properties, player, isBoss ? ESpawnCategory.Boss : ESpawnCategory.Bot);
		Log(logEventInfo);
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("SERVER")]
	public void method_6(Player player, bool isBoss, Vector3 discardedPosition)
	{
		LogEventInfo logEventInfo = new LogEventInfo(LogLevel.Info, "spawns", ActionId.BotSpawnedWithTeleport.ToString());
		method_8(logEventInfo.Properties, player, isBoss ? ESpawnCategory.Boss : ESpawnCategory.Bot, discardedPosition);
		Log(logEventInfo);
	}

	public void method_7(IDictionary<object, object> properties, Player player, ESpawnCategory category, Vector3? discardedPosition = null)
	{
		string value = JsonParserClass.ToJson(new GClass1886
		{
			ProfileId = player.ProfileId,
			Category = category,
			Side = player.Side,
			GroupId = player.Profile.Info.GroupId,
			Position = player.Position,
			DiscardedPosition = discardedPosition,
			Players = Idictionary_0.Select((KeyValuePair<string, Player> pair) => new GClass1887
			{
				ProfileId = pair.Key,
				Category = ESpawnCategory.Player,
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			}).ToArray(),
			Bots = (from pair in Idictionary_1
				where pair.Value.HealthController != null && pair.Value.HealthController.IsAlive
				select new GClass1887
				{
					ProfileId = pair.Key,
					Category = (GClass2190.IsBoss(pair.Value.Profile.Info.Settings) ? ESpawnCategory.Boss : ESpawnCategory.Bot),
					Side = pair.Value.Side,
					GroupId = pair.Value.Profile.Info.GroupId,
					Position = pair.Value.Position
				}).ToArray()
		});
		properties.Add("Json", value);
	}

	public void method_8(IDictionary<object, object> properties, Player player, ESpawnCategory category, Vector3? discardedPosition = null)
	{
		GClass1886 gClass = new GClass1886
		{
			ProfileId = player.ProfileId,
			Category = category,
			Side = player.Side,
			GroupId = player.Profile.Info.GroupId,
			Position = player.Position,
			DiscardedPosition = discardedPosition,
			Players = Idictionary_0.Select((KeyValuePair<string, Player> pair) => new GClass1887
			{
				ProfileId = pair.Key,
				Category = ESpawnCategory.Player,
				Side = pair.Value.Side,
				GroupId = pair.Value.Profile.Info.GroupId,
				Position = pair.Value.Position
			}).ToArray(),
			Bots = (from pair in Idictionary_1
				where pair.Value.HealthController != null && pair.Value.HealthController.IsAlive
				select new GClass1887
				{
					ProfileId = pair.Key,
					Category = (GClass2190.IsBoss(pair.Value.Profile.Info.Settings) ? ESpawnCategory.Boss : ESpawnCategory.Bot),
					Side = pair.Value.Side,
					GroupId = pair.Value.Profile.Info.GroupId,
					Position = pair.Value.Position
				}).ToArray()
		};
		StringBuilder_1.Clear();
		GClass1887[] players = gClass.Players;
		foreach (GClass1887 gClass2 in players)
		{
			StringBuilder_1.Append($"[ProfileId: {gClass2.ProfileId}, Category: {gClass2.Category}, Side: {gClass2.Side}, GroupId: {gClass2.GroupId}, Position: {gClass2.Position}], ");
		}
		StringBuilder_2.Clear();
		players = gClass.Bots;
		foreach (GClass1887 gClass3 in players)
		{
			StringBuilder_2.Append($"[ProfileId: {gClass3.ProfileId}, Category: {gClass3.Category}, Side: {gClass3.Side}, GroupId: {gClass3.GroupId}, Position: {gClass3.Position}], ");
		}
		string value = $"ProfileId: {gClass.ProfileId}, Category: {category}, Side: {gClass.Side}, GroupId:{gClass.GroupId}, Position: {gClass.Position}, DiscardedPosition: {gClass.DiscardedPosition},Players: {StringBuilder_1}, Bots: {StringBuilder_2}";
		properties.Add("SpawnData", value);
	}
}
