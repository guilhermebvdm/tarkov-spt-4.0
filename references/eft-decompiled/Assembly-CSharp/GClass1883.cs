using System;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass1883 : GClass1882<GClass1883>
{
	[Serializable]
	[CompilerGenerated]
	public class Class1159
	{
		public static readonly Class1159 class1159_0 = new Class1159();

		public static Func<Profile, WildSpawnType> func_0;

		public WildSpawnType method_0(Profile profile)
		{
			return profile.Info.Settings.Role;
		}
	}

	public readonly BossLocationSpawn Wave;

	public readonly BotSpawnParams SpawnParams;

	public GClass1883(BossLocationSpawn wave, BotSpawnParams spawnParams, int count, BotCreationDataClass creationData, Action<GClass1883> callback)
		: base(count, creationData, callback)
	{
		Wave = wave;
		SpawnParams = spawnParams;
	}

	public override string ToString()
	{
		return $"Delay params. Count:{base.Count.ToString()} Zone:{Wave.BornZone} Data:{base.Data.Profiles.Select((Profile profile) => profile.Info.Settings.Role)}";
	}
}
