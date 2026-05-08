using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using UnityEngine;

public class BotCreationDataClass
{
	[NonSerialized]
	public List<GClass682> List_0 = new List<GClass682>();

	public IGetProfileData _profileData;

	[NonSerialized]
	public List<Profile> List_1 = new List<Profile>();

	public int Id;

	[NonSerialized]
	public static int Int_0 = 0;

	public static int ProfilesLoadingProcess = 0;

	[NonSerialized]
	public IBotCreator IBotCreator;

	[NonSerialized]
	public GInterface22 Ginterface22_0;

	[NonSerialized]
	public HashSet<string> HashSet_0 = new HashSet<string>();

	[NonSerialized]
	public static HashSet<string> HashSet_1 = new HashSet<string>();

	[NonSerialized]
	public static List<string> List_2 = new List<string>();

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource_0 = new CancellationTokenSource();

	public bool SpawnStopped => CancellationTokenSource_0.Token.IsCancellationRequested;

	public bool KeepZoneOnSpawn => _profileData.KeepZoneOnSpawn;

	public EPlayerSide? Side => _profileData.Side;

	public BotSpawnParams SpawnParams => _profileData.SpawnParams;

	public int Count => List_1.Count;

	public int CountPositions => List_0.Count;

	public List<Profile> Profiles => List_1;

	public static BotCreationDataClass CreateWithoutProfile(IGetProfileData profileData)
	{
		return new BotCreationDataClass(profileData);
	}

	public static async Task<BotCreationDataClass> Create(IGetProfileData profileData, IBotCreator botCreator, int count, GInterface22 token)
	{
		BotCreationDataClass botCreationDataClass = new BotCreationDataClass(profileData)
		{
			Ginterface22_0 = token,
			IBotCreator = botCreator
		};
		List<Profile> list = new List<Profile>();
		if (botCreator is BotsPresets botsPresets)
		{
			await botsPresets.FillCreationDataWithProfiles(botCreationDataClass, token.GetCancelToken());
		}
		else
		{
			for (int i = 0; i < count; i++)
			{
				ProfilesLoadingProcess++;
				Profile profile = await botCreator.GenerateProfile(botCreationDataClass, token.GetCancelToken(), withDelete: true);
				ProfilesLoadingProcess--;
				if (profile == null)
				{
					continue;
				}
				if (!HashSet_1.Add(profile.Id))
				{
					Debug.LogError("generate profile with same id:" + profile.Id);
				}
				else if (profile != null)
				{
					if (botCreationDataClass.HashSet_0.Contains(profile.Id))
					{
						Debug.LogError("generate profile (AT same data) with same id:" + profile.Id);
					}
					botCreationDataClass.HashSet_0.Add(profile.Id);
					list.Add(profile);
				}
			}
			if (botCreationDataClass.SpawnStopped)
			{
				return null;
			}
			botCreationDataClass.AddProfiles(list);
			_ = botCreationDataClass.Count;
		}
		return botCreationDataClass;
	}

	public BotCreationDataClass(IGetProfileData profileData)
	{
		_profileData = profileData;
		Id = Int_0++;
		Singleton<BotEventHandler>.Instance.OnStopBotSpawn += StopSpawn;
	}

	public bool IsValidSpawnType(WildSpawnType type)
	{
		return _profileData.IsValidSpawnType(type);
	}

	public Profile ChooseProfile(List<Profile> simpleProfiles, bool withDelete)
	{
		return _profileData.ChooseProfile(simpleProfiles, withDelete);
	}

	public bool IsBossOrFollowerByTime()
	{
		return _profileData.IsBossOrFollowerByTime();
	}

	public void AddProfiles(List<Profile> profiles)
	{
		foreach (Profile profile in profiles)
		{
			AddProfile(profile);
		}
	}

	public void StopSpawn()
	{
		CancellationTokenSource_0.Cancel();
		Singleton<BotEventHandler>.Instance.OnStopBotSpawn -= StopSpawn;
	}

	public void SpawnProfiles(List<Profile> profiles)
	{
		foreach (Profile profile in profiles)
		{
			SpawnProfile(profile);
		}
	}

	public void RemoveProfile(Profile profile)
	{
		List_2.Remove(profile.Id);
		List_1.Remove(profile);
	}

	public void AddProfile(Profile profile)
	{
		List_2.Contains(profile.Id);
		List_2.Add(profile.Id);
		List_1.Add(profile);
	}

	public void SpawnProfile(Profile profile)
	{
	}

	public WaveInfoClass[] PrepareToLoadBackend(int count)
	{
		return _profileData.PrepareToLoadBackend(count);
	}

	public string GetDebugData()
	{
		return _profileData.GetDebugData();
	}

	public bool IsSpawnOnStart()
	{
		return _profileData.IsSpawnOnStart();
	}

	public bool CanAtZoneByType(BotZone botZone, ZoneLeaveControllerClass botsControllerZonesLeaveController)
	{
		return _profileData.CanAtZoneByType(botZone, botsControllerZonesLeaveController);
	}

	public string GetDebugLocalName()
	{
		return _profileData.GetDebugLocalName();
	}

	public BotCreationDataClass Separate(int spawnCount)
	{
		BotCreationDataClass botCreationDataClass = new BotCreationDataClass(_profileData);
		botCreationDataClass.Ginterface22_0 = Ginterface22_0;
		botCreationDataClass.IBotCreator = IBotCreator;
		if (List_1.Count < spawnCount)
		{
			return botCreationDataClass;
		}
		IList<Profile> list = GClass856.RandomElement(List_1, spawnCount);
		foreach (Profile item in list)
		{
			List_1.Remove(item);
			List_2.Remove(item.Id);
			botCreationDataClass.RemoveProfile(item);
		}
		botCreationDataClass.AddProfiles(list.ToList());
		return botCreationDataClass;
	}

	public void AddPosition(Vector3 spawnPointPosition, int corePointId)
	{
		List_0.Add(new GClass682(spawnPointPosition, corePointId, isUsed: false));
	}

	public GClass682 GetPosition()
	{
		foreach (GClass682 item in List_0)
		{
			if (!item.alreadyUsedToSpawn)
			{
				item.alreadyUsedToSpawn = true;
				return item;
			}
		}
		if (List_0.Count > 0)
		{
			return GClass856.RandomElement(List_0);
		}
		return null;
	}
}
