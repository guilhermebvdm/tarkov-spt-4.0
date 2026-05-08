using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class ProfileDataClass : IGetProfileData
{
	[Serializable]
	[CompilerGenerated]
	public class Class355
	{
		public static readonly Class355 class355_0 = new Class355();

		public static Func<DebugBotProfileChooser, bool> func_0;

		public bool method_0(DebugBotProfileChooser item)
		{
			return item != DebugBotProfileChooser.Random;
		}
	}

	[NonSerialized]
	[CompilerGenerated]
	public EPlayerSide? Nullable_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotSpawnParams BotSpawnParams_0;

	public EPlayerSide? Side
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
	}

	public bool KeepZoneOnSpawn
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
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

	public ProfileDataClass(EPlayerSide side)
	{
		Nullable_0 = side;
	}

	public bool TryGetRole(out WildSpawnType role, out BotDifficulty difficulty)
	{
		role = WildSpawnType.assault;
		difficulty = BotDifficulty.normal;
		return false;
	}

	public Profile ChooseProfile(List<Profile> profiles2Select, bool withDelete)
	{
		Profile[] array = profiles2Select.Where((Profile x) => x.Info.Side == Side).ToArray();
		if (array.Length == 0)
		{
			return null;
		}
		Profile profile = GClass856.RandomElement(array);
		if (withDelete)
		{
			profiles2Select.Remove(profile);
		}
		return profile;
	}

	public bool IsZeroWave()
	{
		return false;
	}

	public bool IsBossOrFollower()
	{
		return false;
	}

	public bool IsSpawnOnStart()
	{
		return false;
	}

	public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
	{
		return true;
	}

	public bool IsBossOrFollowerByTime()
	{
		return false;
	}

	public bool CanSpawnByHour(int timeHour)
	{
		return true;
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		WaveInfoClass waveInfoClass = new WaveInfoClass(count, WildSpawnType.assault, BotDifficulty.normal);
		return new WaveInfoClass[1] { waveInfoClass };
	}

	public bool IsValidSpawnType(WildSpawnType wildSpawnType)
	{
		return true;
	}

	public string GetDebugLocalName()
	{
		string text = GClass856.RandomElement((from DebugBotProfileChooser item in Enum.GetValues(typeof(DebugBotProfileChooser))
			where item != DebugBotProfileChooser.Random
			select item).ToArray()).ToString();
		return Side switch
		{
			EPlayerSide.Usec => "Usec" + text + "Profile", 
			EPlayerSide.Bear => "BearProfile" + text + "Profile", 
			EPlayerSide.Savage => "Savage" + text + "Profile", 
			_ => throw new Exception("Profile not found"), 
		};
	}

	public string GetDebugData()
	{
		return " Side:" + Side.ToString();
	}

	public bool ShallChooseByData()
	{
		return false;
	}

	[CompilerGenerated]
	public bool method_0(Profile x)
	{
		return x.Info.Side == Side;
	}
}
