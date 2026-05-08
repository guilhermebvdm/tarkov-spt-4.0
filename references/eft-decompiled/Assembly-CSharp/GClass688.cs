using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;

public class GClass688 : IGetProfileData
{
	[NonSerialized]
	public DebugBotProfileChooser DebugBotProfileChooser_0;

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

	public GClass688(DebugBotProfileChooser profileChooser)
	{
		DebugBotProfileChooser_0 = profileChooser;
		SpawnParams = new BotSpawnParams
		{
			ShallBeGroup = new ShallBeGroupParams(group: true, bossGroup: true)
		};
	}

	public bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty)
	{
		throw new NotImplementedException();
	}

	public Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete)
	{
		throw new NotImplementedException();
	}

	public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
	{
		return true;
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		throw new NotImplementedException();
	}

	public bool IsValidSpawnType(WildSpawnType wildSpawnType)
	{
		return true;
	}

	public bool IsBossOrFollower()
	{
		return false;
	}

	public bool CanSpawnByHour(int timeHour)
	{
		return true;
	}

	public bool IsZeroWave()
	{
		return false;
	}

	public bool IsSpawnOnStart()
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
		return "Savage" + DebugBotProfileChooser_0.ToString() + "Profile";
	}

	public string GetDebugData()
	{
		return " Debug";
	}
}
