using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.Interactive.SecretExfiltrations;
using UnityEngine;

public class ExfiltrationControllerClass
{
	[Serializable]
	[CompilerGenerated]
	public class Class987
	{
		public static readonly Class987 class987_0 = new Class987();

		public static Func<ExfiltrationPoint, bool> func_0;

		public static Func<ExfiltrationPoint, bool> func_1;

		public static Func<ExfiltrationPoint, bool> func_2;

		public static Func<ExfiltrationPoint, bool> func_3;

		public static Func<ExfiltrationPoint, bool> func_4;

		public static Func<ExfiltrationPoint, bool> func_5;

		public static Func<SecretExfiltrationPoint, bool> func_6;

		public static Func<SecretExfiltrationPoint, bool> func_7;

		public bool method_0(ExfiltrationPoint x)
		{
			if (!(x is ScavExfiltrationPoint) && !(x is SecretExfiltrationPoint))
			{
				return true;
			}
			return x is SharedExfiltrationPoint;
		}

		public bool method_1(ExfiltrationPoint x)
		{
			return x is ScavExfiltrationPoint;
		}

		public bool method_2(ExfiltrationPoint x)
		{
			return x is SecretExfiltrationPoint;
		}

		public bool method_3(ExfiltrationPoint point)
		{
			return point.Status != EExfiltrationStatus.NotPresent;
		}

		public bool method_4(ExfiltrationPoint x)
		{
			return string.IsNullOrEmpty(x.Settings.Name);
		}

		public bool method_5(ExfiltrationPoint x)
		{
			return x.Settings.Chance > 0f;
		}

		public bool method_6(SecretExfiltrationPoint x)
		{
			return x.EligibleForPmc;
		}

		public bool method_7(SecretExfiltrationPoint x)
		{
			return x.EligibleForScav;
		}
	}

	[CompilerGenerated]
	public class Class988
	{
		public string exitName;

		public bool method_0(LocationExitClass x)
		{
			return exitName == x.Name;
		}
	}

	[CompilerGenerated]
	public class Class989
	{
		public string exitName;

		public bool method_0(GClass1432 x)
		{
			return exitName == x.Name;
		}
	}

	[CompilerGenerated]
	public class Class990
	{
		public string entryPointName;

		public bool method_0(ExfiltrationPoint x)
		{
			if (x != null)
			{
				return x.EligibleEntryPoints.Contains(entryPointName);
			}
			return false;
		}
	}

	[CompilerGenerated]
	public class Class991
	{
		public string name;

		public bool method_0(ExfiltrationPoint x)
		{
			return x.Settings.Name == name;
		}
	}

	[CompilerGenerated]
	public class Class992
	{
		public string name;

		public bool method_0(ExfiltrationPoint x)
		{
			return x.Settings.Name == name;
		}

		public bool method_1(ScavExfiltrationPoint x)
		{
			return x.Settings.Name == name;
		}

		public bool method_2(SecretExfiltrationPoint x)
		{
			return x.Settings.Name == name;
		}
	}

	[CompilerGenerated]
	public class Class993
	{
		public Vector3 position;

		public float method_0(ScavExfiltrationPoint x)
		{
			return Vector3.Distance(position, x.transform.position);
		}
	}

	[CompilerGenerated]
	private Action<ExfiltrationPoint> action_0;

	public readonly GClass728 Logger = new GClass728();

	[NonSerialized]
	[CompilerGenerated]
	public GClass3721 Gclass3721_0;

	[NonSerialized]
	[CompilerGenerated]
	public ExfiltrationPoint[] ExfiltrationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public ScavExfiltrationPoint[] ScavExfiltrationPoint_0;

	[NonSerialized]
	[CompilerGenerated]
	public SecretExfiltrationPoint[] SecretExfiltrationPoint_0;

	public readonly HashSet<int> BannedPlayers = new HashSet<int>();

	[NonSerialized]
	public List<ScavExfiltrationPoint> List_0;

	[NonSerialized]
	public List<ScavExfiltrationPoint> List_1;

	public static ExfiltrationControllerClass Instance => Singleton<GameWorld>.Instance.ExfiltrationController;

	public GClass3721 SecretExfilitranionController
	{
		[CompilerGenerated]
		get
		{
			return Gclass3721_0;
		}
		[CompilerGenerated]
		set
		{
			Gclass3721_0 = value;
		}
	}

	public ExfiltrationPoint[] ExfiltrationPoints
	{
		[CompilerGenerated]
		get
		{
			return ExfiltrationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			ExfiltrationPoint_0 = value;
		}
	}

	public ScavExfiltrationPoint[] ScavExfiltrationPoints
	{
		[CompilerGenerated]
		get
		{
			return ScavExfiltrationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			ScavExfiltrationPoint_0 = value;
		}
	}

	public SecretExfiltrationPoint[] SecretExfiltrationPoints
	{
		[CompilerGenerated]
		get
		{
			return SecretExfiltrationPoint_0;
		}
		[CompilerGenerated]
		set
		{
			SecretExfiltrationPoint_0 = value;
		}
	}

	public event Action<ExfiltrationPoint> StatusChanged
	{
		[CompilerGenerated]
		add
		{
			Action<ExfiltrationPoint> action = action_0;
			Action<ExfiltrationPoint> action2;
			do
			{
				action2 = action;
				Action<ExfiltrationPoint> value2 = (Action<ExfiltrationPoint>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<ExfiltrationPoint> action = action_0;
			Action<ExfiltrationPoint> action2;
			do
			{
				action2 = action;
				Action<ExfiltrationPoint> value2 = (Action<ExfiltrationPoint>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void InitAllExfiltrationPoints(MongoID locationId, LocationExitClass[] settings, GClass1432[] secretExitsSettings, bool justLoadSettings = false, string disabledScavExits = "", bool giveAuthority = true)
	{
		ExfiltrationPoint[] array = LocationScene.GetAllObjects<ExfiltrationPoint>().ToArray();
		ExfiltrationPoints = array.Where((ExfiltrationPoint x) => (!(x is ScavExfiltrationPoint) && !(x is SecretExfiltrationPoint)) || x is SharedExfiltrationPoint).ToArray();
		ScavExfiltrationPoints = array.Where((ExfiltrationPoint x) => x is ScavExfiltrationPoint).Cast<ScavExfiltrationPoint>().ToArray();
		SecretExfiltrationPoints = array.Where((ExfiltrationPoint x) => x is SecretExfiltrationPoint).Cast<SecretExfiltrationPoint>().ToArray();
		List_0 = new List<ScavExfiltrationPoint>(ScavExfiltrationPoints.Length);
		List_1 = new List<ScavExfiltrationPoint>();
		ScavExfiltrationPoint[] scavExfiltrationPoints = ScavExfiltrationPoints;
		foreach (ScavExfiltrationPoint scavExfiltrationPoint in scavExfiltrationPoints)
		{
			if (scavExfiltrationPoint is SharedExfiltrationPoint { IsMandatoryForScavs: not false })
			{
				List_1.Add(scavExfiltrationPoint);
			}
			else
			{
				List_0.Add(scavExfiltrationPoint);
			}
		}
		UnityEngine.Random.InitState(EFTDateTimeClass.Now.Millisecond);
		ExfiltrationPoint[] exfiltrationPoints = ExfiltrationPoints;
		foreach (ExfiltrationPoint exfiltrationPoint in exfiltrationPoints)
		{
			string exitName = exfiltrationPoint.Settings.Name;
			LocationExitClass locationExitClass = settings.FirstOrDefault((LocationExitClass x) => exitName == x.Name);
			int num2 = Array.IndexOf(array, exfiltrationPoint) + 1;
			MongoID exfilId = locationId.Add(num2 + 1);
			if (locationExitClass != null)
			{
				exfiltrationPoint.LoadSettings(exfilId, locationExitClass, giveAuthority);
				if (!justLoadSettings && !method_0(exfiltrationPoint))
				{
					exfiltrationPoint.SetStatusLogged(EExfiltrationStatus.NotPresent, "ExfiltrationController.InitAllExfiltrationPoints-2");
				}
			}
			else
			{
				exfiltrationPoint.SetStatusLogged(EExfiltrationStatus.NotPresent, "ExfiltrationController.InitAllExfiltrationPoints-3");
			}
		}
		bool flag = secretExitsSettings == null;
		SecretExfiltrationPoint[] secretExfiltrationPoints = SecretExfiltrationPoints;
		foreach (SecretExfiltrationPoint secretExfiltrationPoint in secretExfiltrationPoints)
		{
			if (flag)
			{
				secretExfiltrationPoint.SetStatusLogged(EExfiltrationStatus.NotPresent, "ExfiltrationController.InitAllExfiltrationPoints-4");
				continue;
			}
			string exitName2 = secretExfiltrationPoint.Settings.Name;
			GClass1432 gClass = secretExitsSettings.FirstOrDefault((GClass1432 x) => exitName2 == x.Name);
			int num3 = Array.IndexOf(array, secretExfiltrationPoint) + 1;
			MongoID exfilId2 = locationId.Add(num3 + 1);
			if (gClass != null)
			{
				secretExfiltrationPoint.LoadSettings(exfilId2, gClass, giveAuthority);
			}
			else
			{
				secretExfiltrationPoint.SetStatusLogged(EExfiltrationStatus.NotPresent, "ExfiltrationController.InitAllExfiltrationPoints-5");
			}
		}
		SecretExfilitranionController = new GClass3722(this);
	}

	public void DisableExitsInteraction()
	{
		ExfiltrationPoint[] exfiltrationPoints = ExfiltrationPoints;
		for (int i = 0; i < exfiltrationPoints.Length; i++)
		{
			exfiltrationPoints[i].DisableInteraction();
		}
	}

	public void EnableExitsInteraction()
	{
		ExfiltrationPoint[] exfiltrationPoints = ExfiltrationPoints;
		for (int i = 0; i < exfiltrationPoints.Length; i++)
		{
			exfiltrationPoints[i].EnableInteraction();
		}
	}

	public bool IsMyPlayerBanned()
	{
		if (GamePlayerOwner.MyPlayer != null)
		{
			return BannedPlayers.Contains(GamePlayerOwner.MyPlayer.Id);
		}
		return false;
	}

	public void EventDisableAllExitsExceptOne(string exitNameToSave)
	{
		if (GClass856.IsNullOrEmpty(exitNameToSave))
		{
			Debug.LogError("Can't find proper exit point");
			return;
		}
		foreach (ExfiltrationPoint item in ExfiltrationPoints.Where((ExfiltrationPoint point) => point.Status != EExfiltrationStatus.NotPresent))
		{
			if (item.Settings.Name.Equals(exitNameToSave))
			{
				float exitTimeMultiplier = Singleton<BackendConfigSettingsClass>.Instance.EventSettings.ExitTimeMultiplier;
				item.Settings.ExfiltrationTime *= exitTimeMultiplier;
			}
			else
			{
				item.Status = EExfiltrationStatus.NotPresent;
			}
		}
	}

	public void CancelExtractionForPlayer(Player player)
	{
		ExfiltrationPoint[] exfiltrationPoints = ExfiltrationPoints;
		foreach (ExfiltrationPoint exfiltrationPoint in exfiltrationPoints)
		{
			exfiltrationPoint.OnCancelExtraction?.Invoke(exfiltrationPoint, player);
		}
	}

	public ExfiltrationPoint[] EligiblePoints(Profile profile)
	{
		return EligiblePoints(string.IsNullOrEmpty(profile.Info.EntryPoint) ? string.Empty : profile.Info.EntryPoint.ToLower());
	}

	public ExfiltrationPoint[] EligiblePoints(string entryPointName)
	{
		if (string.IsNullOrEmpty(entryPointName))
		{
			LogDebug("<color=red>Attention! Entry point name is null or empty. Enabling ALL EPs...</color>");
		}
		ExfiltrationPoint[] source = ((!string.IsNullOrEmpty(entryPointName)) ? ExfiltrationPoints.Where((ExfiltrationPoint x) => x != null && x.EligibleEntryPoints.Contains(entryPointName)).ToArray() : new ExfiltrationPoint[0]);
		foreach (ExfiltrationPoint item in source.Where((ExfiltrationPoint x) => string.IsNullOrEmpty(x.Settings.Name)))
		{
			item.Settings.Name = Guid.NewGuid().ToString().Substring(0, 10);
		}
		return source.Where((ExfiltrationPoint x) => x.Settings.Chance > 0f).ToArray();
	}

	public SecretExfiltrationPoint[] SecretEligiblePoints()
	{
		return SecretExfiltrationPoints.Where((SecretExfiltrationPoint x) => x.EligibleForPmc).ToArray();
	}

	public bool method_0(ExfiltrationPoint trigger)
	{
		return UnityEngine.Random.Range(0f, 100f) <= trigger.Settings.Chance;
	}

	public void WriteStates(EFTWriterClass writer)
	{
		ExfiltrationPoint[] array = ExfiltrationPoints.Concat(SecretExfiltrationPoints).ToArray();
		GClass1290.WriteShort(writer, (short)array.Length);
		foreach (ExfiltrationPoint exfiltrationPoint in array)
		{
			GClass1290.WriteString(writer, exfiltrationPoint.Settings.Name);
			writer.WriteByte((byte)exfiltrationPoint.Status);
			GClass1290.WriteInt(writer, exfiltrationPoint.Settings.StartTime);
			if (exfiltrationPoint.Status == EExfiltrationStatus.Countdown)
			{
				GClass1290.WriteShort(writer, (short)exfiltrationPoint.ExfiltrationStartTime);
			}
			GClass1290.WriteShort(writer, (short)exfiltrationPoint.QueuedPlayers.Count);
			foreach (string queuedPlayer in exfiltrationPoint.QueuedPlayers)
			{
				GClass1290.WriteString(writer, queuedPlayer);
			}
			if (exfiltrationPoint is SecretExfiltrationPoint secretExfiltrationPoint)
			{
				GClass1290.WriteBool(writer, value: true);
				GClass1290.WriteBool(writer, secretExfiltrationPoint.ItemTransferred);
			}
			else
			{
				GClass1290.WriteBool(writer, value: false);
			}
		}
	}

	public void ReadStates(EFTReaderClass reader)
	{
		short num = GClass1285.ReadShort(reader);
		ExfiltrationPoint[] source = ExfiltrationPoints.Concat(SecretExfiltrationPoints).ToArray();
		for (int i = 0; i < num; i++)
		{
			string name = GClass1285.ReadString(reader);
			int num2 = -1;
			EExfiltrationStatus eExfiltrationStatus = (EExfiltrationStatus)reader.ReadByte();
			int startTime = GClass1285.ReadInt(reader);
			if (eExfiltrationStatus == EExfiltrationStatus.Countdown)
			{
				num2 = GClass1285.ReadShort(reader);
			}
			short num3 = GClass1285.ReadShort(reader);
			List<string> list = new List<string>();
			for (int j = 0; j < num3; j++)
			{
				list.Add(GClass1285.ReadString(reader));
			}
			bool flag;
			bool itemTransferred = (flag = GClass1285.ReadBool(reader)) && GClass1285.ReadBool(reader);
			ExfiltrationPoint exfiltrationPoint = source.FirstOrDefault((ExfiltrationPoint x) => x.Settings.Name == name);
			if (exfiltrationPoint != null && !flag)
			{
				exfiltrationPoint.Status = eExfiltrationStatus;
				exfiltrationPoint.Settings.StartTime = startTime;
				if (num2 > 0)
				{
					exfiltrationPoint.ExfiltrationStartTime = num2;
				}
				foreach (string item in list)
				{
					exfiltrationPoint.OnItemTransferred(item);
				}
			}
			else if (flag && exfiltrationPoint is SecretExfiltrationPoint secretExfiltrationPoint)
			{
				if (secretExfiltrationPoint.Status != EExfiltrationStatus.Hidden)
				{
					exfiltrationPoint.Status = eExfiltrationStatus;
				}
				exfiltrationPoint.Settings.StartTime = startTime;
				if (num2 > 0)
				{
					exfiltrationPoint.ExfiltrationStartTime = num2;
				}
				foreach (string item2 in list)
				{
					exfiltrationPoint.OnItemTransferred(item2);
				}
				secretExfiltrationPoint.ItemTransferred = itemTransferred;
			}
			else
			{
				LogError("Exfiltration point {0} is missing", name);
			}
		}
	}

	public void UpdatePoint(string name, EExfiltrationStatus command, List<string> queuedPlayers, bool itemTransferred)
	{
		ExfiltrationPoint exfiltrationPoint = ExfiltrationPoints.FirstOrDefault((ExfiltrationPoint x) => x.Settings.Name == name);
		if (exfiltrationPoint == null)
		{
			ScavExfiltrationPoint scavExfiltrationPoint = ScavExfiltrationPoints.FirstOrDefault((ScavExfiltrationPoint x) => x.Settings.Name == name);
			if (scavExfiltrationPoint != null)
			{
				scavExfiltrationPoint.Status = command;
				return;
			}
			SecretExfiltrationPoint secretExfiltrationPoint = SecretExfiltrationPoints.FirstOrDefault((SecretExfiltrationPoint x) => x.Settings.Name == name);
			if (secretExfiltrationPoint != null)
			{
				if (secretExfiltrationPoint.Status != EExfiltrationStatus.Hidden)
				{
					secretExfiltrationPoint.Status = command;
				}
				foreach (string queuedPlayer in queuedPlayers)
				{
					secretExfiltrationPoint.OnItemTransferred(queuedPlayer);
				}
				secretExfiltrationPoint.ItemTransferred = itemTransferred;
				if (command == EExfiltrationStatus.Countdown && secretExfiltrationPoint.ExfiltrationStartTime <= 0f)
				{
					secretExfiltrationPoint.ExfiltrationStartTime = GClass1893.PastTimeSeconds(Singleton<AbstractGame>.Instance.GameTimer);
				}
			}
			else
			{
				LogDebug("{0} ep does not exist", name);
			}
			return;
		}
		exfiltrationPoint.Status = command;
		foreach (string queuedPlayer2 in queuedPlayers)
		{
			exfiltrationPoint.OnItemTransferred(queuedPlayer2);
		}
		if (command == EExfiltrationStatus.Countdown && exfiltrationPoint.ExfiltrationStartTime <= 0f)
		{
			exfiltrationPoint.ExfiltrationStartTime = GClass1893.PastTimeSeconds(Singleton<AbstractGame>.Instance.GameTimer);
		}
	}

	public void Dispose()
	{
		action_0 = null;
		SecretExfilitranionController?.Dispose();
	}

	public void ScavExfiltrationClaim(Vector3 position, string profileId, int count)
	{
		RemoveProfileIdFromPoints(profileId);
		foreach (ScavExfiltrationPoint item in List_1)
		{
			AssignScavIdToPoint(item, profileId);
		}
		List<ScavExfiltrationPoint> list = List_0.OrderByDescending((ScavExfiltrationPoint x) => Vector3.Distance(position, x.transform.position)).ToList();
		LogDebug("ScavExfiltrationClaim ({0}): {1} exits. Available: {2}", profileId, count, list.Count);
		for (int num = 0; num < count; num++)
		{
			int maxExclusive = (int)((float)list.Count / 2f + 0.9f);
			ScavExfiltrationPoint scavExfiltrationPoint = list[UnityEngine.Random.Range(0, maxExclusive)];
			AssignScavIdToPoint(scavExfiltrationPoint, profileId);
			list.Remove(scavExfiltrationPoint);
			if (list.Count < 1)
			{
				break;
			}
		}
	}

	public int GetScavExfiltrationMask(string profileId)
	{
		int num = 0;
		for (int i = 0; i < ScavExfiltrationPoints.Length; i++)
		{
			if (ScavExfiltrationPoints[i].EligibleIds.Contains(profileId))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public void RemoveProfileIdFromPoints(string profileId)
	{
		ScavExfiltrationPoint[] scavExfiltrationPoints = ScavExfiltrationPoints;
		foreach (ScavExfiltrationPoint scavExfiltrationPoint in scavExfiltrationPoints)
		{
			if (scavExfiltrationPoint.EligibleIds.Remove(profileId) && scavExfiltrationPoint.EligibleIds.Count < 1 && !(scavExfiltrationPoint is SharedExfiltrationPoint))
			{
				scavExfiltrationPoint.SetStatusLogged(EExfiltrationStatus.NotPresent, "RemoveProfileIdFromPoints");
			}
		}
	}

	public ExfiltrationPoint[] ScavExfiltrationClaim(int mask, string profileId)
	{
		RemoveProfileIdFromPoints(profileId);
		List<ExfiltrationPoint> list = new List<ExfiltrationPoint>();
		LogDebug("ScavExfiltrationClaim {2}: {0} mask, exits: {1}", Convert.ToString(mask, 2), ScavExfiltrationPoints.Length, profileId);
		for (int i = 0; i < 31; i++)
		{
			if ((mask & (1 << i)) != 0)
			{
				ScavExfiltrationPoint scavExfiltrationPoint = ScavExfiltrationPoints[i];
				scavExfiltrationPoint.EligibleIds.Add(profileId);
				if (scavExfiltrationPoint.Status != EExfiltrationStatus.RegularMode && !(scavExfiltrationPoint is SharedExfiltrationPoint))
				{
					scavExfiltrationPoint.SetStatusLogged(EExfiltrationStatus.RegularMode, "ExfiltrationController.ScavExfiltrationClaim()");
				}
				list.Add(scavExfiltrationPoint);
			}
		}
		return list.ToArray();
	}

	public SecretExfiltrationPoint[] GetScavSecretExits()
	{
		return SecretExfiltrationPoints.Where((SecretExfiltrationPoint x) => x.EligibleForScav).ToArray();
	}

	public void AssignScavIdToPoint(ScavExfiltrationPoint point, string profileId)
	{
		if (point.Status != EExfiltrationStatus.RegularMode && !(point is SharedExfiltrationPoint))
		{
			point.SetStatusLogged(EExfiltrationStatus.RegularMode, "ExfiltrationController.AssignScavIdToPoint()");
		}
		if (!point.EligibleIds.Contains(profileId))
		{
			point.EligibleIds.Add(profileId);
		}
	}

	public void LogDebug(string message, params object[] args)
	{
	}

	public void LogInfo(string message, params object[] args)
	{
	}

	public void LogError(string message, params object[] args)
	{
	}

	public void InitSecretExfils(Player player)
	{
		SecretExfiltrationPoint[] secretExfiltrationPoints = SecretExfiltrationPoints;
		for (int i = 0; i < secretExfiltrationPoints.Length; i++)
		{
			secretExfiltrationPoints[i].InitSecretExfilPoint(player);
		}
	}
}
