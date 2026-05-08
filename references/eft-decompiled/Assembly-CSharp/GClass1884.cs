using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1884 : GClass1882<GClass1884>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1160
	{
		public static readonly Class1160 class1160_0 = new Class1160();

		public static Func<Profile, WildSpawnType> func_0;

		public WildSpawnType method_0(Profile profile)
		{
			return profile.Info.Settings.Role;
		}
	}

	public BotZone BotZone;

	public GClass1884(BotZone botZone, int count, BotCreationDataClass creationData, Action<GClass1884> callback)
		: base(count, creationData, callback)
	{
		BotZone = botZone;
	}

	public override string ToString()
	{
		string arg = ((BotZone == null) ? "free" : BotZone.name);
		return $"Delay params. Count:{base.Count.ToString()} Zone:{arg} Data:{base.Data.Profiles.Select((Profile profile) => profile.Info.Settings.Role)}";
	}
}
