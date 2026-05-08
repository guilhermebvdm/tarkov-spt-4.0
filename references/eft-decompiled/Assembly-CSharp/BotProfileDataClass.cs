using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class BotProfileDataClass : IGetProfileData
{
	[NonSerialized]
	public const float Float_0 = 10f;

	public const float LESS_TIME_SPAWN_SEC = 60f;

	[NonSerialized]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	public BotDifficulty BotDifficulty_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public EPlayerSide? Nullable_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public static int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public BotSpawnParams BotSpawnParams_0;

	public bool KeepZoneOnSpawn => Bool_0;

	public EPlayerSide? Side
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
	}

	public BotSpawnParams SpawnParams
	{
		[CompilerGenerated]
		get
		{
			return BotSpawnParams_0;
		}
		[CompilerGenerated]
		set
		{
			BotSpawnParams_0 = value;
		}
	}

	public BotProfileDataClass(EPlayerSide side, WildSpawnType role, BotDifficulty botDifficulty, float spawnTime, BotSpawnParams spawnParams = null, bool keepZoneOnSpawn = false)
	{
		Float_1 = spawnTime;
		Nullable_0 = side;
		WildSpawnType_0 = role;
		Int_0 = Int_1;
		Int_1++;
		Bool_0 = keepZoneOnSpawn;
		SpawnParams = spawnParams;
		BotDifficulty_0 = botDifficulty;
	}

	public bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty)
	{
		role = WildSpawnType_0;
		difficulty = BotDifficulty_0;
		return true;
	}

	public Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete)
	{
		List<Profile> list = profiles2Select.Where((Profile x) => x.Info.Side == Side && x.Info.Settings.Role == WildSpawnType_0 && x.Info.Settings.BotDifficulty == BotDifficulty_0).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		Profile profile = GClass856.RandomElement(list);
		if (withDelete)
		{
			profiles2Select.Remove(profile);
		}
		return profile;
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		WaveInfoClass waveInfoClass = new WaveInfoClass(count, WildSpawnType_0, BotDifficulty_0);
		return new WaveInfoClass[1] { waveInfoClass };
	}

	public bool IsValidSpawnType(WildSpawnType wildSpawnType)
	{
		return wildSpawnType == WildSpawnType_0;
	}

	public bool SameSide(EPlayerSide side)
	{
		return side == Side;
	}

	public string GetDebugLocalName()
	{
		string text = "";
		switch (Side)
		{
		case EPlayerSide.Usec:
			return "Usec" + (int)GClass856.Random(0f, 1f) + "Profile";
		case EPlayerSide.Bear:
			text = "Bear";
			return null;
		case EPlayerSide.Savage:
			text = "Savage";
			if (WildSpawnType_0 != WildSpawnType.assault && WildSpawnType_0 != WildSpawnType.marksman)
			{
				return text + WildSpawnType_0.ToString() + "Profile";
			}
			return text + GClass3591.GetRnd().ToString() + "Profile";
		default:
			throw new Exception("Profile not found");
		}
	}

	public string GetDebugData()
	{
		return $" Side:{Side.ToString()} Type:{WildSpawnType_0.ToString()}  BotDifficulty:{BotDifficulty_0.ToString()}  id:{Int_0}";
	}

	public bool ShallChooseByData()
	{
		if (WildSpawnType_0 != WildSpawnType.exUsec && WildSpawnType_0 != WildSpawnType.pmcBot && WildSpawnType_0 != WildSpawnType.assaultGroup && WildSpawnType_0 != WildSpawnType.arenaFighter)
		{
			return false;
		}
		return true;
	}

	public bool IsZeroWave()
	{
		return Float_1 < 0f;
	}

	public bool IsBossOrFollower()
	{
		return BotSettingsRepoClass.IsBossOrFollower(WildSpawnType_0);
	}

	public bool IsSpawnOnStart()
	{
		if (Float_1 < 60f)
		{
			return Float_1 >= 0f;
		}
		return false;
	}

	public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
	{
		return !botsControllerZonesLeaveController.IsZoneBlockFor(botZone, WildSpawnType_0);
	}

	public bool CanSpawnByHour(int timeHour)
	{
		switch (WildSpawnType_0)
		{
		default:
			return true;
		case WildSpawnType.sectantWarrior:
		case WildSpawnType.sectantPriest:
			return !method_0(timeHour);
		case WildSpawnType.assault:
			return method_0(timeHour);
		}
	}

	public bool method_0(int timeHour)
	{
		if (timeHour > 7)
		{
			return timeHour < 22;
		}
		return false;
	}

	public bool IsBossOrFollowerByTime()
	{
		if (Float_1 < 10f && Float_1 > 0f && IsBossOrFollower())
		{
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	public bool method_1(Profile x)
	{
		if (x.Info.Side == Side && x.Info.Settings.Role == WildSpawnType_0)
		{
			return x.Info.Settings.BotDifficulty == BotDifficulty_0;
		}
		return false;
	}
}
