using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public class GClass689 : IGetProfileData
{
	[NonSerialized]
	public WildSpawnType WildSpawnType_0;

	[NonSerialized]
	public BotDifficulty BotDifficulty_0;

	[NonSerialized]
	public Profile Profile_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotSpawnParams BotSpawnParams_0;

	public bool KeepZoneOnSpawn
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public EPlayerSide? Side => null;

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

	public GClass689(Profile profile)
	{
		profile.Id = MongoID.Generate();
		Profile_0 = profile;
	}

	public bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty)
	{
		role = WildSpawnType_0;
		difficulty = BotDifficulty_0;
		return true;
	}

	public Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete)
	{
		return Profile_0;
	}

	public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
	{
		return true;
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		return new WaveInfoClass[1]
		{
			new WaveInfoClass(count, Profile_0.Info.Settings.Role, Profile_0.Info.Settings.BotDifficulty)
		};
	}

	public bool CanSpawnByHour(int timeHour)
	{
		return true;
	}

	public bool IsSpawnOnStart()
	{
		return false;
	}

	public bool IsValidSpawnType(WildSpawnType wildSpawnType)
	{
		return true;
	}

	public bool IsZeroWave()
	{
		return false;
	}

	public bool IsBossOrFollower()
	{
		return false;
	}

	public bool IsBossOrFollowerByTime()
	{
		return false;
	}

	public bool ShallChooseByData()
	{
		return false;
	}

	public string GetDebugLocalName()
	{
		return "Savage" + Profile_0?.ToString() + "Profile";
	}

	public string GetDebugData()
	{
		return " Debug";
	}
}
