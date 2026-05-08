using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EFT;

public class GClass629
{
	[Serializable]
	[CompilerGenerated]
	public class Class291
	{
		public static readonly Class291 class291_0 = new Class291();

		public static Func<WaveInfoClass, int> func_0;

		public int method_0(WaveInfoClass x)
		{
			return x.Limit;
		}
	}

	public int OnStart;

	public int AtProcess;

	public int OnStartProfiles;

	public int AtProcessProfiles;

	public int Backup;

	public int BackupProfiles;

	public int Used;

	public void UseProfile(Profile profile)
	{
		Used++;
	}

	public void AddProfileExteranl(List<WaveInfoClass> data, EProfilesAskingStat onStart)
	{
		int num = data.Sum((WaveInfoClass x) => x.Limit);
		switch (onStart)
		{
		case EProfilesAskingStat.onStart:
			OnStart++;
			OnStartProfiles += num;
			break;
		case EProfilesAskingStat.byBackup:
			Backup++;
			BackupProfiles += num;
			break;
		case EProfilesAskingStat.external:
			AtProcess++;
			AtProcessProfiles += num;
			break;
		}
	}

	public void DoTotalLog()
	{
	}
}
