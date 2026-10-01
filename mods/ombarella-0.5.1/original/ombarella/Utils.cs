using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace ombarella;

public static class Utils
{
	private static int alternatePlayerID = 0;

	private static bool isAlternatePlayerID = false;

	private static float _logUpdateTimer = 0f;

	private static readonly Dictionary<Type, FieldInfo> _isObservedAIFieldCache = new Dictionary<Type, FieldInfo>();

	public static ManualLogSource Logger;

	public static bool DebugViz { get; set; }

	public static Player GetMainPlayer()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (isAlternatePlayerID)
		{
			return GetPlayer(alternatePlayerID);
		}
		GameWorld instance = Singleton<GameWorld>.Instance;
		if ((Object)instance == (Object)null)
		{
			return null;
		}
		return instance.MainPlayer;
	}

	public static bool GetMainCameraCullingIndex(ref int index)
	{
		Camera main = Camera.main;
		if ((Object)(object)main != (Object)null)
		{
			index = main.cullingMask;
			return true;
		}
		return false;
	}

	public static int GetPlayerCullingMask()
	{
		return LayerMask.NameToLayer("Player");
	}

	public static List<Player> GetAllPlayers()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		GameWorld instance = Singleton<GameWorld>.Instance;
		if ((Object)instance == (Object)null)
		{
			return new List<Player>();
		}
		return instance.AllAlivePlayersList ?? new List<Player>();
	}

	public static List<Player> GetActualHumanPlayers(List<Player> players)
	{
		List<Player> list = new List<Player>();
		if (players == null)
		{
			return list;
		}
		foreach (Player player in players)
		{
			if (IsActualHumanPlayer(player))
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static List<Player> GetBotPlayers(List<Player> players)
	{
		List<Player> list = new List<Player>();
		if (players == null)
		{
			return list;
		}
		foreach (Player player in players)
		{
			if (IsLightMeterUsablePlayer(player) && IsBotPlayer(player))
			{
				list.Add(player);
			}
		}
		return list;
	}

	public static bool IsActualHumanPlayer(Player player)
	{
		if (IsLightMeterUsablePlayer(player) && !IsBotPlayer(player))
		{
			return !IsHeadlessPlayer(player);
		}
		return false;
	}

	public static bool IsBotPlayer(Player player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		if ((Object)player == (Object)null)
		{
			return false;
		}
		if (player.IsAI)
		{
			return true;
		}
		return IsFikaObservedAI(player);
	}

	public static bool IsHeadlessPlayer(Player player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		if ((Object)player == (Object)null || player.Profile == null || player.Profile.Info == null)
		{
			return false;
		}
		if (StringStartsWith(player.Profile.Info.Nickname, "headless_"))
		{
			return true;
		}
		return string.Equals(player.Profile.Info.GroupId, "HEADLESS", StringComparison.OrdinalIgnoreCase);
	}

	public static bool IsLightMeterUsablePlayer(Player player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)player == (Object)null)
		{
			return false;
		}
		if (player.HealthController == null || !player.HealthController.IsAlive)
		{
			return false;
		}
		if ((Object)(object)player.PlayerBones == (Object)null || player.PlayerBones.Head == null || player.PlayerBones.Ribcage == null)
		{
			return false;
		}
		if (IsFinite(player.Position) && IsFinite(player.PlayerBones.Head.position))
		{
			return IsFinite(player.PlayerBones.Ribcage.position);
		}
		return false;
	}

	private static bool IsFikaObservedAI(Player player)
	{
		FieldInfo isObservedAIField = GetIsObservedAIField(((object)player).GetType());
		if (isObservedAIField == null || isObservedAIField.FieldType != typeof(bool))
		{
			return false;
		}
		return (bool)isObservedAIField.GetValue(player);
	}

	private static FieldInfo GetIsObservedAIField(Type type)
	{
		if (type == null)
		{
			return null;
		}
		if (_isObservedAIFieldCache.TryGetValue(type, out var value))
		{
			return value;
		}
		Type type2 = type;
		while (type2 != null)
		{
			value = type2.GetField("IsObservedAI", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (value != null)
			{
				break;
			}
			type2 = type2.BaseType;
		}
		_isObservedAIFieldCache[type] = value;
		return value;
	}

	public static bool IsFinite(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (IsFinite(value.x) && IsFinite(value.y))
		{
			return IsFinite(value.z);
		}
		return false;
	}

	public static bool IsFinite(float value)
	{
		if (!float.IsNaN(value))
		{
			return !float.IsInfinity(value);
		}
		return false;
	}

	private static bool StringStartsWith(string value, string prefix)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public static Player GetPlayer(int playerID)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		GameWorld instance = Singleton<GameWorld>.Instance;
		if ((Object)instance == (Object)null)
		{
			return null;
		}
		Player result = null;
		if (instance.TryGetAlivePlayer(playerID, ref result))
		{
			return result;
		}
		return null;
	}

	public static void Log(string log, bool oneTimeLog)
	{
		if (!Plugin.IsDebug.Value)
		{
			return;
		}
		if (oneTimeLog)
		{
			Logger.LogInfo((object)log);
			return;
		}
		_logUpdateTimer += Time.deltaTime;
		float num = 1f / Plugin.DebugUpdateFreq.Value;
		if (_logUpdateTimer > num)
		{
			_logUpdateTimer = 0f;
			Logger.LogInfo((object)log);
		}
	}

	public static void LogError(string error)
	{
		Logger.LogError((object)error);
	}

	public static bool IsInRaid()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		if (Singleton<AbstractGame>.Instantiated && (Object)(object)Singleton<AbstractGame>.Instance != (Object)null && Singleton<AbstractGame>.Instance.InRaid)
		{
			return true;
		}
		GameWorld instance = Singleton<GameWorld>.Instance;
		if ((Object)instance != (Object)null)
		{
			return (Object)instance.MainPlayer != (Object)null;
		}
		return false;
	}

	public static void DrawDebugLine(Vector3 from, Vector3 to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Debug.DrawLine(from, to, Color.green);
	}

	public static void ForceTruePlayerID(bool setForcedPlayer, int playerID)
	{
		isAlternatePlayerID = setForcedPlayer;
		alternatePlayerID = playerID;
	}

	public static void AssignPlayerToLayer(string layerName)
	{
		GameObject gameObject = ((Component)GetMainPlayer()).gameObject;
		if (LayerMask.NameToLayer(layerName) != -1)
		{
			gameObject.layer = LayerMask.NameToLayer(layerName);
		}
		else
		{
			LogError("Layer " + layerName + " does not exist.");
		}
	}

	public static List<string> GetExistingLayers()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < 32; i++)
		{
			string text = LayerMask.LayerToName(i);
			if (!string.IsNullOrEmpty(text))
			{
				list.Add(text);
			}
		}
		return list;
	}
}
