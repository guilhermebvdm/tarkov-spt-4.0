using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotsPresets : GClass680
{
	[Serializable]
	[CompilerGenerated]
	public class Class354
	{
		public static readonly Class354 class354_0 = new Class354();

		public static Func<BotWaveDataClass, WaveInfoClass> func_0;

		public WaveInfoClass method_0(BotWaveDataClass spawnWave)
		{
			return new WaveInfoClass(spawnWave.BotsCount, spawnWave.WildSpawnType, spawnWave.Difficulty);
		}
	}

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public ISession ISession;

	[NonSerialized]
	public int Int_1 = 1;

	[NonSerialized]
	public List<WaveInfoClass> List_1;

	[NonSerialized]
	public List<WaveInfoClass> List_2;

	[NonSerialized]
	public GClass684 Gclass684_0;

	public override int BundlesLoading => Int_0;

	public BotsPresets(ISession backEndSession, BotWaveDataClass[] waves, BossLocationSpawn[] bossWaves, WaveInfoClass[] nonWaves = null, bool checkProfilesLoaded = false)
	{
		Bool_0 = checkProfilesLoaded;
		ISession = backEndSession;
		List_2 = ((nonWaves != null) ? new List<WaveInfoClass>(nonWaves) : new List<WaveInfoClass>());
		Gclass684_0 = new GClass684(this, method_2, method_0);
		if (waves != null)
		{
			List_2.AddRange(waves.Select((BotWaveDataClass spawnWave) => new WaveInfoClass(spawnWave.BotsCount, spawnWave.WildSpawnType, spawnWave.Difficulty)));
		}
		if (bossWaves == null)
		{
			return;
		}
		for (int num = 0; num < bossWaves.Length; num++)
		{
			foreach (WaveInfoClass usingType in bossWaves[num].GetUsingTypes())
			{
				List_2.Add(usingType);
			}
		}
	}

	public void method_0(List<Profile> profiles)
	{
		List_0.AddRange(profiles);
	}

	public async Task TryLoadBotsProfilesOnStart(IEnumerable<WaveInfoClass> external = null)
	{
		if (List_2 == null)
		{
			List_2 = new List<WaveInfoClass>();
		}
		if (external != null)
		{
			List_2.AddRange(external);
		}
		if (List_2.Count > 0)
		{
			foreach (WaveInfoClass item in List_2)
			{
				int num = LocalBotSettingsProviderClass.Core.LOCAL_BOTS_COUNT;
				if (num > 200)
				{
					num = 200;
				}
				if (item.Limit > num)
				{
					item.Limit = num;
				}
			}
			List<List<WaveInfoClass>> list = GClass680.DelemiterCountBotTypes(List_2);
			foreach (List<WaveInfoClass> item2 in list)
			{
				await method_1(item2);
			}
		}
		else
		{
			Bool_0 = true;
		}
	}

	public async Task method_1(List<WaveInfoClass> waves)
	{
		await Task.Delay(500);
		List<WaveInfoClass> waves2 = method_3(waves, out List_1);
		await method_2(waves2, EProfilesAskingStat.external);
		Bool_0 = true;
	}

	public async Task<Profile[]> method_2(List<WaveInfoClass> waves, EProfilesAskingStat stat)
	{
		if (ISession == null)
		{
			return Array.Empty<Profile>();
		}
		Profile[] array = await ISession.LoadBots(waves);
		CheckLoadingResult(array, waves);
		if (array != null)
		{
			List_0.AddRange(array);
		}
		return array;
	}

	public List<WaveInfoClass> method_3(List<WaveInfoClass> wavesProfiles, out List<WaveInfoClass> delayed)
	{
		return GClass378.OptimizeBotWaves(wavesProfiles, out delayed);
	}

	public override void FillBackupProfilesData(GClass406 resultCache)
	{
		base.FillBackupProfilesData(resultCache);
		resultCache.Struct.LoadCount = Gclass684_0.LoadsProcess;
		resultCache.Struct.TotalBackups = Gclass684_0.TotalBackups;
		if (Gclass684_0.PeriodsForBackups.Count > 0)
		{
			float num = Gclass684_0.PeriodsForBackups[Gclass684_0.PeriodsForBackups.Count - 1];
			resultCache.Struct.LastSecondsPeriod = (int)num;
		}
		else
		{
			resultCache.Struct.LastSecondsPeriod = -1;
		}
	}

	public override void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count)
	{
		Gclass684_0.AddToTargetBackup(difficulty, role, count);
	}

	public override async Task<Profile> CreateProfile(BotCreationDataClass data, CancellationToken cancellationToken, bool withDelete)
	{
		Profile profile;
		if (ISession != null && (!DebugBotData.UseDebugData || !DebugBotData.Instance.localProfiles))
		{
			profile = GetNewProfile(data, withDelete);
			if (data.SpawnStopped)
			{
				return null;
			}
			if (profile == null)
			{
				List<WaveInfoClass> list = new List<WaveInfoClass>();
				if (!data._profileData.TryGetRole(out var role, out var difficulty))
				{
					return null;
				}
				WaveInfoClass item = new WaveInfoClass(3, role, difficulty);
				list.Add(item);
				List<WaveInfoClass> delayed;
				WaveInfoClass[] source = method_3(list, out delayed).ToArray();
				List<WaveInfoClass> delayed2 = new List<WaveInfoClass>();
				List<WaveInfoClass> wavesProfiles = source.ToList();
				List<WaveInfoClass> list2 = method_3(wavesProfiles, out delayed2);
				Profile[] array = await ISession.LoadBots(list2);
				CheckLoadingResult(array, list2);
				List_0.AddRange(array);
				Gclass684_0.AddProfileForBackup(data);
				profile = data.ChooseProfile(List_0, withDelete: true);
			}
		}
		else
		{
			profile = await GClass3591.GenerateProfile(data);
		}
		if (cancellationToken.IsCancellationRequested)
		{
			return null;
		}
		Int_0++;
		await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, PoolManagerClass.AssemblyType.Local, profile.GetAllPrefabPaths(allCustomization: false).ToArray(), JobPriorityClass.General, null, PoolManagerClass.DefaultCancellationToken);
		Int_0--;
		GClass628.smethod_1(profile);
		return profile;
	}

	public void CheckLoadingResult(Profile[] result, List<WaveInfoClass> conditions)
	{
	}

	public async Task FillCreationDataWithProfiles(BotCreationDataClass data, CancellationToken cancellationToken)
	{
		Debug.LogError("BotProfileClient.CreateProfile");
		if (ISession != null && (!DebugBotData.UseDebugData || !DebugBotData.Instance.localProfiles))
		{
			Profile newProfile = GetNewProfile(data, withDelete: true);
			data.Profiles.Add(newProfile);
			if (cancellationToken.IsCancellationRequested || data.SpawnStopped)
			{
				return;
			}
			if (newProfile == null)
			{
				WaveInfoClass[] source;
				if (List_1 != null && List_1.Count > 0)
				{
					List<WaveInfoClass> delayed = new List<WaveInfoClass>();
					List<WaveInfoClass> list = method_3(List_1, out delayed);
					List_1 = delayed;
					source = list.ToArray();
				}
				else
				{
					source = data.PrepareToLoadBackend(Int_1);
				}
				List<WaveInfoClass> delayed2 = new List<WaveInfoClass>();
				List<WaveInfoClass> wavesProfiles = source.ToList();
				List<WaveInfoClass> list2 = method_3(wavesProfiles, out delayed2);
				Profile[] array = await ISession.LoadBots(list2);
				CheckLoadingResult(array, list2);
				data.AddProfiles(array.ToList());
			}
		}
		else
		{
			Profile newProfile = await GClass3591.GenerateProfile(data);
			data.Profiles.Add(newProfile);
		}
		if (cancellationToken.IsCancellationRequested || data.SpawnStopped)
		{
			return;
		}
		Int_0++;
		List<ResourceKey> list3 = new List<ResourceKey>();
		foreach (Profile profile in data.Profiles)
		{
			list3.AddRange(profile.GetAllPrefabPaths(allCustomization: false));
		}
		await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, PoolManagerClass.AssemblyType.Local, list3.ToArray(), JobPriorityClass.General, null, PoolManagerClass.DefaultCancellationToken);
		Int_0--;
	}
}
