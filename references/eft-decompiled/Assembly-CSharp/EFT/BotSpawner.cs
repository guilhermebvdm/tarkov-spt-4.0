using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Game.Spawning;
using JetBrains.Annotations;
using UnityEngine;

namespace EFT;

public abstract class BotSpawner : GInterface22
{
	[Serializable]
	[CompilerGenerated]
	public class Class1163
	{
		public static readonly Class1163 class1163_0 = new Class1163();

		public static Func<Player, bool> func_0;

		public bool method_0(Player x)
		{
			return x.AIData.IAmBoss;
		}
	}

	[CompilerGenerated]
	public class Class1164
	{
		public BotSpawner botSpawner_0;

		public BotCreationDataClass data;

		public Action<BotOwner> callback;

		public bool shallBeGroup;

		public Stopwatch stopWatch;

		public void method_0(BotOwner bot)
		{
			botSpawner_0.method_11(bot, data, callback, shallBeGroup, stopWatch);
		}
	}

	[CompilerGenerated]
	public class Class1165
	{
		public bool canBeSnipe;

		public bool method_0(BotZone x)
		{
			if (!canBeSnipe)
			{
				return !x.SnipeZone;
			}
			return true;
		}
	}

	[CompilerGenerated]
	public class Class1166
	{
		public string profileId;

		public bool method_0(BotOwner x)
		{
			return x.ProfileId == profileId;
		}

		public bool method_1(Player x)
		{
			return x.ProfileId == profileId;
		}
	}

	[CompilerGenerated]
	public class Class1167
	{
		public BotWaveDataClass wave;

		public bool method_0(BotZone zone)
		{
			return zone.name == wave.SpawnAreaName;
		}
	}

	[CompilerGenerated]
	public class Class1168
	{
		public string name;

		public bool method_0(BotZone x)
		{
			return x.name == name;
		}
	}

	[CompilerGenerated]
	public class Class1169
	{
		public bool canBeSnipe;

		public bool method_0(BotZone x)
		{
			if (!canBeSnipe)
			{
				return !x.SnipeZone;
			}
			return true;
		}
	}

	public readonly BossSpawnerClass BossSpawner;

	[NonSerialized]
	public BotsClass Bots;

	[NonSerialized]
	public BotZone[] AllBotZones;

	[NonSerialized]
	public BotZone[] OpenedZones;

	[NonSerialized]
	public Dictionary<PatrolPoint, BotZone> ZonesPatrols;

	[NonSerialized]
	public Dictionary<PatrolPoint, BotZone> ZonesPatrolsSnipe;

	[NonSerialized]
	public GClass1885 SpawnDelaysService;

	[NonSerialized]
	public List<Player> AllPlayers = new List<Player>(40);

	[NonSerialized]
	public bool GameEnd;

	[NonSerialized]
	public bool FreeForAll = true;

	[NonSerialized]
	public IBotCreator BotCreator;

	[NonSerialized]
	public DeadBodiesController DeadBodiesController;

	[NonSerialized]
	public ISpawnSystem SpawnSystem;

	[NonSerialized]
	public int AllBotsCount;

	[NonSerialized]
	public int FollowersBotsCount;

	[NonSerialized]
	public int BossBotsCount;

	[NonSerialized]
	public int InSpawnProcess;

	[NonSerialized]
	public HashSet<string> DeletedPlayers = new HashSet<string>();

	[NonSerialized]
	public HashSet<string> AddedPlayers = new HashSet<string>();

	[NonSerialized]
	public HashSet<WildSpawnType> BlockedRoles = new HashSet<WildSpawnType>();

	[NonSerialized]
	public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

	[field: NonSerialized]
	public int MaxBots { get; set; }

	[field: NonSerialized]
	public BotZoneGroupsDictionary Groups { get; } = new BotZoneGroupsDictionary();

	[field: NonSerialized]
	public IBotGame BotGame { get; }

	public bool IsProfilesLoaded => BotCreator.StartProfilesLoaded;

	public int AllBotsWithLoaded => InSpawnProcess + AllBotsCount;

	public int AliveAndLoadingBotsCount => BotCreator.BotsLoading + AllBotsCount;

	public int AllBotsWithDelayed => BotCreator.BotsLoading + AllBotsCount + SpawnDelaysService.WaitCount + BotCreationDataClass.ProfilesLoadingProcess;

	public int BotsDelayed => BotCreator.BotsLoading + SpawnDelaysService.WaitCount;

	public int PlayersCount => AllPlayers.Count;

	public event Action<BotOwner> OnBotCreated;

	public event Action<BotOwner> OnBotRemoved;

	public event Action<GClass1888> OnSpawnedWave;

	public BotSpawner(IBotCreator botCreator, IBotGame game, BotZone[] botZones, BotsClass bots, ISpawnSystem spawnSystem, int maxBots, bool freeForAll, Dictionary<PatrolPoint, BotZone> allZonesPatrols, string openZones)
	{
		ZonesPatrols = new Dictionary<PatrolPoint, BotZone>();
		ZonesPatrolsSnipe = new Dictionary<PatrolPoint, BotZone>();
		FreeForAll = freeForAll;
		DeadBodiesController = new DeadBodiesController(Groups);
		Bots = bots;
		BotCreator = botCreator;
		BotGame = game;
		SpawnSystem = spawnSystem;
		SpawnSystem.Validate();
		MaxBots = maxBots;
		AllBotZones = botZones;
		SpawnDelaysService = new GClass1885();
		OpenedZones = method_3(openZones, AllBotZones);
		BossSpawner = new BossSpawnerClass(spawnSystem, this, BotCreator, botZones, SpawnDelaysService);
		foreach (KeyValuePair<PatrolPoint, BotZone> allZonesPatrol in allZonesPatrols)
		{
			if (allZonesPatrol.Value.SnipeZone)
			{
				ZonesPatrolsSnipe.Add(allZonesPatrol.Key, allZonesPatrol.Value);
			}
			else
			{
				ZonesPatrols.Add(allZonesPatrol.Key, allZonesPatrol.Value);
			}
		}
	}

	public bool CanSpawnRole(IGetProfileData data)
	{
		bool flag = BlockedRoles != null && BlockedRoles.Count > 0;
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoRoleBlocks)
		{
			flag = false;
		}
		if (flag)
		{
			foreach (WildSpawnType blockedRole in BlockedRoles)
			{
				if (data.IsValidSpawnType(blockedRole))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void SetBlockedRoles(string[] resultExcludedBosses)
	{
		BlockedRoles.Clear();
		if (GClass856.IsNullOrEmpty(resultExcludedBosses))
		{
			return;
		}
		for (int i = 0; i < resultExcludedBosses.Length; i++)
		{
			if (Enum.TryParse<WildSpawnType>(resultExcludedBosses[i], out var result))
			{
				BlockedRoles.Add(result);
			}
		}
	}

	public List<Player> GetAllBossPLayers()
	{
		return AllPlayers.Where((Player x) => x.AIData.IAmBoss).ToList();
	}

	public Player GetPlayer(int index)
	{
		return AllPlayers[index];
	}

	public BotZone GetRandomBotZone(bool canBeSnipe)
	{
		BotZone botZone = GClass856.RandomElement(OpenedZones.Where((BotZone x) => canBeSnipe || !x.SnipeZone).ToList());
		if (botZone != null)
		{
			return botZone;
		}
		return GClass856.RandomElement(OpenedZones);
	}

	public ActorDataStruct GetBotDebugData(IPlayer currentPlayer, string profileId)
	{
		if (!(currentPlayer.ProfileId == profileId) && AllPlayers.Count != 0)
		{
			BotOwner botOwner = Bots.BotOwners.FirstOrDefault((BotOwner x) => x.ProfileId == profileId);
			if (botOwner != null)
			{
				return new ActorDataStruct(botOwner, 0f, currentPlayer);
			}
			Player player = AllPlayers.FirstOrDefault((Player x) => x.ProfileId == profileId);
			if (!(player == null))
			{
				return new ActorDataStruct(player.AIData, 0f, player);
			}
			return default(ActorDataStruct);
		}
		return default(ActorDataStruct);
	}

	public GStruct11 GetDebugSpawnsData()
	{
		int hour = BotGame.BotsController.ZonesLeaveController.GetHour();
		return new GStruct11(AllBotsCount, BotCreator.BotsLoading, SpawnDelaysService.WaitCount, InSpawnProcess, BotCreator.BundlesLoading, hour, SpawnDelaysService.DelaysModels);
	}

	public BotsGroup GetGroupAndSetEnemies(BotOwner bot, BotZone zone)
	{
		bool flag = BotSettingsRepoClass.IsBoss(bot.Profile.Info.Settings.Role) || BotSettingsRepoClass.IsFollower(bot.Profile.Info.Settings.Role);
		EPlayerSide side = bot.Profile.Info.Side;
		WildSpawnType role = bot.Profile.Info.Settings.Role;
		List<BotOwner> list = new List<BotOwner>();
		BotsGroup group;
		if (flag)
		{
			if (Groups.TryGetValue(zone, side, role, out group, flag) && (bot.SpawnProfileData == null || bot.SpawnProfileData.SpawnParams.ShallBeGroup == null || (!bot.Boss.IamBoss && !group.IsFull)))
			{
				method_4(bot);
				return group;
			}
			foreach (BotOwner item in method_5(bot))
			{
				list.Add(item);
			}
			method_4(bot);
			group = new BotsGroup(zone, BotGame, bot, list, DeadBodiesController, AllPlayers, forBoss: true);
			if (bot.SpawnProfileData.SpawnParams.ShallBeGroup != null)
			{
				group.TargetMembersCount = bot.SpawnProfileData.SpawnParams.ShallBeGroup.StartCount;
			}
			Groups.Add(zone, side, group, isBossOrFollower: true);
			return group;
		}
		if (Groups.TryGetValue(zone, side, role, out group, flag))
		{
			method_4(bot);
			return group;
		}
		foreach (BotOwner item2 in method_5(bot))
		{
			list.Add(item2);
		}
		method_4(bot);
		group = new BotsGroup(zone, BotGame, bot, list, DeadBodiesController, AllPlayers, forBoss: false);
		if (FreeForAll)
		{
			Groups.AddNoKey(group, zone);
		}
		else
		{
			Groups.Add(zone, side, group, isBossOrFollower: false);
		}
		return group;
	}

	public void ActivateBotsByWave(BossLocationSpawn wave)
	{
		BotSpawnParams botSpawnParams = new BotSpawnParams();
		botSpawnParams.TriggerType = wave.TriggerType;
		botSpawnParams.Id_spawn = wave.TriggerId;
		BossSpawner.Spawn(wave, botSpawnParams).HandleExceptions();
	}

	public async Task ActivateBotsWithoutWave(int count, IGetProfileData data)
	{
		TrySpawnFreeAndDelay(await BotCreationDataClass.Create(data, BotCreator, count, this), newWave: true);
	}

	public async Task ActivateBotsByWave(BotWaveDataClass wave)
	{
		if (wave.BotsCount == 0)
		{
			return;
		}
		bool flag = wave.SpawnAreaName.Length < 2 || wave.SpawnAreaName == "";
		IGetProfileData getProfileData = new BotProfileDataClass(wave.Side, wave.WildSpawnType, wave.Difficulty, wave.Time, null, wave.KeepZoneOnSpawn);
		BotCreationDataClass data = await BotCreationDataClass.Create(getProfileData, BotCreator, wave.BotsCount, this);
		getProfileData.SpawnParams = new BotSpawnParams();
		if (GClass856.IsTrue100(wave.ChanceGroup))
		{
			int gROUP_WAVE_SIZE_MAX = LocalBotSettingsProviderClass.Core.GROUP_WAVE_SIZE_MAX;
			int groupCount = Mathf.Min(GClass856.RandomInclude(2, gROUP_WAVE_SIZE_MAX), wave.BotsCount);
			getProfileData.SpawnParams.ShallBeGroup = new ShallBeGroupParams(group: true, bossGroup: true, groupCount);
		}
		if (flag)
		{
			TrySpawnFreeAndDelay(data, newWave: true);
			return;
		}
		BotZone botZone = AllBotZones.FirstOrDefault((BotZone zone) => zone.name == wave.SpawnAreaName);
		if (botZone == null)
		{
			string value = $"Can't spawn wave cause can'f find zone with name:{wave.SpawnAreaName}   _openZones:{OpenedZones}";
			if (GClass398.Instance.IsTraceEnable())
			{
				StringBuilder stringBuilder = new StringBuilder(value);
				BotZone[] allBotZones = AllBotZones;
				foreach (BotZone botZone2 in allBotZones)
				{
					stringBuilder.Append(" " + botZone2.name);
				}
			}
		}
		else
		{
			TryToSpawnInZoneAndDelay(botZone, data, wave.WithCheckMinMax, newWave: true);
		}
	}

	public void CheckOnMax(int wantSpawn, out int toDelay, out int toSpawn, bool calcOnlySimpleBots = false)
	{
		if (MaxBots != 0)
		{
			int allBotsWithLoaded = AllBotsWithLoaded;
			int num = (calcOnlySimpleBots ? (allBotsWithLoaded - FollowersBotsCount - BossBotsCount) : allBotsWithLoaded);
			int num2 = MaxBots - num;
			if (num2 <= 0)
			{
				toDelay = wantSpawn;
				toSpawn = 0;
				return;
			}
			if (num2 < wantSpawn)
			{
				toDelay = wantSpawn - num2;
				toSpawn = num2;
				return;
			}
		}
		toDelay = 0;
		toSpawn = wantSpawn;
	}

	public BotZone GetClosestZone(Vector3 position, out float dist)
	{
		return GetClosestZone(position, ZonesPatrols, out dist);
	}

	public async Task SpawnBotBTR()
	{
		BTRControllerClass instance = BTRControllerClass.Instance;
		if (instance != null && !(instance.BtrVehicle == null))
		{
			BotSpawnParams botSpawnParams = new BotSpawnParams();
			botSpawnParams.ShallBeGroup = new ShallBeGroupParams(group: true, bossGroup: true, 1);
			await SpawnBotByTypeForce(1, WildSpawnType.shooterBTR, BotDifficulty.normal, botSpawnParams);
		}
	}

	public async Task SpawnBotByTypeForce(int count, WildSpawnType botType, BotDifficulty dif, BotSpawnParams spawnParams)
	{
		BotZone randomBotZone = GetRandomBotZone(canBeSnipe: false);
		TryToSpawnInZoneInner(randomBotZone, await BotCreationDataClass.Create(new BotProfileDataClass(EPlayerSide.Savage, botType, dif, 5f, spawnParams), BotCreator, count, this), count, withCheckMinMax: false, newWave: true, null, forcedSpawn: true);
	}

	public GClass1884 TryToSpawnInZoneInner(BotZone botZone, BotCreationDataClass data, int count, bool withCheckMinMax, bool newWave, List<ISpawnPoint> pointsToSpawn = null, bool forcedSpawn = false)
	{
		if (data.SpawnStopped)
		{
			return null;
		}
		if (DebugBotData.UseDebugData && DebugBotData.Instance.spawnInstantly)
		{
			forcedSpawn = true;
		}
		if (!BotCreator.StartProfilesLoaded)
		{
			return new GClass1884(botZone, count, data, method_8);
		}
		if (DebugBotData.UseDebugData && DebugBotData.Instance.spawnInstantly)
		{
			List<ISpawnPoint> openedPositions = SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.DuplicateIfAtLeastOne);
			SpawnBotsInZoneOnPositions(openedPositions, botZone, data);
			return new GClass1884(botZone, 0, data, method_8);
		}
		if (!data.CanAtZoneByType(botZone, BotGame.BotsController.ZonesLeaveController))
		{
			return new GClass1884(botZone, count, data, method_8);
		}
		Bots.GetListByZone(botZone);
		bool flag = data.IsBossOrFollowerByTime();
		if (withCheckMinMax && !botZone.HaveFreeSpace(count) && !flag && !forcedSpawn)
		{
			return new GClass1884(botZone, count, data, method_8);
		}
		if (newWave)
		{
			this.OnSpawnedWave?.Invoke(new GClass1888(botZone, count, data));
		}
		int toDelay;
		int toSpawn;
		if (withCheckMinMax && !forcedSpawn)
		{
			CheckOnMax(count, out toDelay, out toSpawn);
		}
		else
		{
			toDelay = 0;
			toSpawn = count;
		}
		if (toDelay > 0)
		{
			return new GClass1884(botZone, toDelay, data, method_8);
		}
		if (toSpawn > 0)
		{
			if (flag)
			{
				data.IsSpawnOnStart();
			}
			count = toSpawn;
			List<ISpawnPoint> list;
			if (pointsToSpawn != null)
			{
				list = pointsToSpawn.ToList();
				if (!forcedSpawn)
				{
					bool flag2 = true;
					foreach (ISpawnPoint item in pointsToSpawn)
					{
						if (!SpawnSystem.IsValidSpawn(item, data, Time.time))
						{
							flag2 = false;
							break;
						}
					}
					if (!flag2)
					{
						return new GClass1884(botZone, pointsToSpawn.Count, data, method_8);
					}
				}
			}
			else
			{
				list = SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.DuplicateIfAtLeastOne);
				if (count > list.Count)
				{
					if (!forcedSpawn)
					{
						int count2 = count - list.Count;
						return new GClass1884(botZone, count2, data, method_8);
					}
					list = SpawnSystem.SelectAISpawnPoints(data, botZone, count, null, ActionIfNotEnoughPoints.ReturnFoundPoints);
				}
			}
			SpawnBotsInZoneOnPositions(list.ToList(), botZone, data);
		}
		return null;
	}

	public void TryToSpawnInZoneAndDelay(BotZone botZone, BotCreationDataClass data, bool withCheckMinMax, bool newWave, List<ISpawnPoint> pointsToSpawn = null, bool forcedSpawn = false)
	{
		if (!data.SpawnStopped)
		{
			GClass1884 gClass = TryToSpawnInZoneInner(botZone, data, data.Count, withCheckMinMax, newWave, pointsToSpawn, forcedSpawn);
			if (gClass != null)
			{
				SpawnDelaysService.Add(gClass);
			}
		}
	}

	public void SpawnBotsInZoneOnPositions(List<ISpawnPoint> openedPositions, BotZone botZone, BotCreationDataClass data, Action<BotOwner> callback = null)
	{
		method_7(openedPositions, botZone, data, callback, CancellationTokenSource.Token).HandleExceptions();
	}

	public async Task DebugSpawnAnyway()
	{
		try
		{
			InSpawnProcess++;
			BotZone botZone = GClass856.RandomElement(AllBotZones);
			SpawnPointMarker spawnPointMarker = GClass856.RandomElement(botZone.SpawnPointMarkers);
			BotCreationDataClass botCreationDataClass = await BotCreationDataClass.Create(new BotProfileDataClass(EPlayerSide.Savage, WildSpawnType.assault, BotDifficulty.normal, 0f), BotCreator, 1, this);
			botCreationDataClass.AddPosition(spawnPointMarker.SpawnPoint.Position, spawnPointMarker.SpawnPoint.CorePointId);
			method_10(botZone, botCreationDataClass, null, CancellationTokenSource.Token);
		}
		catch (Exception)
		{
		}
	}

	public List<BotOwner> GetAllBotsNearTarget(Vector3 t, float dist)
	{
		float num = dist * dist;
		List<BotOwner> list = new List<BotOwner>();
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			if ((botOwner.Transform.position - t).sqrMagnitude < num)
			{
				list.Add(botOwner);
			}
		}
		return list;
	}

	[CanBeNull]
	public BotOwner ClosestBotToPoint(Vector3 t)
	{
		BotOwner result = null;
		float num = float.MaxValue;
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			float sqrMagnitude = (botOwner.Transform.position - t).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = botOwner;
			}
		}
		return result;
	}

	public void Stop()
	{
		CancellationTokenSource.Cancel();
		GameEnd = true;
		SpawnDelaysService.Dispose();
		foreach (KeyValuePair<BotZone, GClass575> group in Groups)
		{
			group.Value.Dispose();
		}
		Groups.Clear();
		SpawnSystem.Dispose();
		SpawnSystem = null;
		this.OnBotCreated = null;
		this.OnBotRemoved = null;
	}

	public BotZone GetZoneByName(string name)
	{
		return AllBotZones.FirstOrDefault((BotZone x) => x.name == name);
	}

	public List<BotZone> GetPmcZones()
	{
		return BossSpawner.AvailableZonesPmc;
	}

	public IEnumerable<BotZone> SpawnZones(bool canBeSnipe)
	{
		return AllBotZones.Where((BotZone x) => canBeSnipe || !x.SnipeZone);
	}

	public void BotDied(BotOwner bot)
	{
		if (!bot.IsDead)
		{
			bot.IsDead = true;
			if (GClass2190.IsFollower(bot.Profile.Info.Settings))
			{
				FollowersBotsCount--;
			}
			else if (GClass2190.IsBoss(bot.Profile.Info.Settings))
			{
				BossBotsCount--;
			}
			AllBotsCount--;
			Bots.Remove(bot);
			if (this.OnBotRemoved != null)
			{
				this.OnBotRemoved(bot);
			}
		}
	}

	public void DeletePlayer(Player player)
	{
		DeletedPlayers.Add(player.ProfileId);
		player.AIData.OnBecomeScavAttacker -= method_13;
		AllPlayers.Remove(player);
		foreach (KeyValuePair<BotZone, GClass575> group in Groups)
		{
			foreach (BotsGroup group2 in group.Value.GetGroups(notNull: false))
			{
				group2.DeletePlayerCauseDead(player);
			}
		}
		Bots.RemovePlayer(player);
	}

	public void AddPlayer(Player player)
	{
		AddedPlayers.Add(player.ProfileId);
		if (player.HealthController.IsAlive && !AllPlayers.Contains(player))
		{
			Bots.AddPlayer(player);
			AllPlayers.Add(player);
			player.AIData.OnBecomeScavAttacker += method_13;
			method_6(player);
		}
	}

	public void SetMaxBots(int maxCount)
	{
		MaxBots = maxCount;
	}

	public void SetAllPlayersPursuitDebug()
	{
		foreach (Player allPlayer in AllPlayers)
		{
			allPlayer.AIData.SetAttackByLoyalityScav();
		}
	}

	public void DebugLogsAboultRemoveEnemies()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("deleted:");
		foreach (string deletedPlayer in DeletedPlayers)
		{
			stringBuilder.Append(deletedPlayer + ",");
		}
		stringBuilder.AppendLine("added:");
		foreach (string addedPlayer in AddedPlayers)
		{
			stringBuilder.Append(addedPlayer + ",");
		}
	}

	public CancellationToken GetCancelToken()
	{
		return CancellationTokenSource.Token;
	}

	public void FillProfilesData(GClass406 resultCache)
	{
		BotCreator.FillBackupProfilesData(resultCache);
	}

	public string FullBotsCountInfo()
	{
		return $"BotsLoading:{BotCreator.BotsLoading} allBotsCount:{AllBotsCount} WaitCount:{SpawnDelaysService.WaitCount}";
	}

	public abstract GClass1884 TrySpawnFreeInner(BotCreationDataClass data, bool newWave, Action<GClass1884> checkSpawnOnFreeAfterDelay);

	public void TrySpawnFreeAndDelay(BotCreationDataClass data, bool newWave)
	{
		GClass1884 gClass = TrySpawnFreeInner(data, newWave, method_8);
		if (gClass != null)
		{
			SpawnDelaysService.Add(gClass);
		}
	}

	public BotZone GetClosestZone(Vector3 position, Dictionary<PatrolPoint, BotZone> testingZones, out float dist)
	{
		float num = float.MaxValue;
		BotZone result = null;
		foreach (KeyValuePair<PatrolPoint, BotZone> testingZone in testingZones)
		{
			float sqrMagnitude = (testingZone.Key.Position - position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = testingZone.Value;
			}
		}
		dist = num;
		return result;
	}

	public async Task method_0(EPlayerSide side, BotZone zone, DebugBotProfileChooser profileType = DebugBotProfileChooser.Random, bool ignoreConditions = false)
	{
		if (profileType == DebugBotProfileChooser.Random)
		{
			List<DebugBotProfileChooser> list = new List<DebugBotProfileChooser>();
			foreach (DebugBotProfileChooser value in Enum.GetValues(typeof(DebugBotProfileChooser)))
			{
				list.Add(value);
			}
			list.Remove(DebugBotProfileChooser.Random);
			profileType = GClass856.RandomElement(list);
		}
		if ((BotCreatorClass)BotCreator != null)
		{
			Profile profile = await GClass3591.GenerateProfile(side, profileType);
			await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(PoolManagerClass.PoolsCategory.Raid, PoolManagerClass.AssemblyType.Local, profile.GetAllPrefabPaths(allCustomization: false).ToArray(), JobPriorityClass.General);
			TryToSpawnInZoneAndDelay(zone, await BotCreationDataClass.Create(new GClass688(profileType), BotCreator, 1, this), withCheckMinMax: true, newWave: true, null, ignoreConditions);
		}
	}

	public async Task method_1(EPlayerSide side, BotZone zone, int count, DebugBotProfileChooser profileType = DebugBotProfileChooser.Random, bool ignoreConditions = false)
	{
	}

	public async Task method_2(EPlayerSide side, BotZone zone, WildSpawnType profileType = WildSpawnType.assault, BotDifficulty botDifficulty = BotDifficulty.normal, bool forcedSpawn = false)
	{
		if (BotSettingsRepoClass.IsBossOrFollower(profileType))
		{
			BossLocationSpawn bossLocationSpawn = new BossLocationSpawn();
			bossLocationSpawn.BossZone = "";
			bossLocationSpawn.Time = 1f;
			bossLocationSpawn.Delay = 0f;
			bossLocationSpawn.TriggerId = "";
			bossLocationSpawn.TriggerName = "";
			bossLocationSpawn.BossChance = 100f;
			bossLocationSpawn.BossName = profileType.ToString();
			bossLocationSpawn.BossDifficult = BotDifficulty.normal.ToString();
			bossLocationSpawn.BossEscortAmount = 0.ToString();
			bossLocationSpawn.BossEscortDifficult = BotDifficulty.normal.ToString();
			bossLocationSpawn.BossEscortType = WildSpawnType.followerBully.ToString();
			bossLocationSpawn.ParseMainTypesTypes();
			bossLocationSpawn.ForceSpawn = forcedSpawn;
			bossLocationSpawn.IgnoreMaxBots = forcedSpawn;
			BossSpawner.Spawn(bossLocationSpawn, new BotSpawnParams()).HandleExceptions();
		}
		else
		{
			TryToSpawnInZoneAndDelay(zone, await BotCreationDataClass.Create(new BotProfileDataClass(side, profileType, botDifficulty, 0f), BotCreator, 1, this), withCheckMinMax: true, newWave: true, null, forcedSpawn);
		}
	}

	public BotZone[] method_3(string openZones, BotZone[] botZones)
	{
		if (string.IsNullOrEmpty(openZones))
		{
			return botZones.ToArray();
		}
		string[] source = openZones.Split(',');
		List<BotZone> list = new List<BotZone>();
		int num = 0;
		int num2 = 0;
		BotZone[] array = botZones;
		foreach (BotZone botZone in array)
		{
			if (source.Contains(botZone.name))
			{
				list.Add(botZone);
				if (botZone.SnipeZone)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		if (list.Count != 0)
		{
			botZones = list.ToArray();
		}
		return botZones.ToArray();
	}

	public void method_4(BotOwner bot)
	{
		foreach (GClass575 value in bot.BotsController.Groups().Values)
		{
			foreach (BotsGroup group in value.GetGroups(notNull: true))
			{
				if (group.IsPlayerEnemy(bot))
				{
					group.AddEnemy(bot, EBotEnemyCause.initial);
				}
				if (group.IsAlly(bot))
				{
					group.AddAlly(bot.GetPlayer);
				}
			}
		}
	}

	public IEnumerable<BotOwner> method_5(BotOwner owner)
	{
		if (FreeForAll)
		{
			return Bots.BotOwners;
		}
		return Bots.GetEnemies(owner);
	}

	public void method_6(IPlayer person)
	{
		foreach (GClass575 value in Groups.Values)
		{
			value.AddPlayer(person);
		}
	}

	public async Task method_7(List<ISpawnPoint> spawnPoints, BotZone botZone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
	{
		InSpawnProcess += spawnPoints.Count;
		if (data.SpawnStopped)
		{
			return;
		}
		_ = MaxBots;
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}
		foreach (ISpawnPoint spawnPoint in spawnPoints)
		{
			data.AddPosition(spawnPoint.Position, spawnPoint.CorePointId);
		}
		spawnPoints.Clear();
		method_10(botZone, data, callback, cancellationToken);
		await Task.Yield();
	}

	public void method_8(GClass1884 delayedBotsInfo)
	{
		if (delayedBotsInfo.Data.SpawnStopped)
		{
			return;
		}
		GClass1884 gClass = ((delayedBotsInfo.BotZone == null) ? TrySpawnFreeInner(delayedBotsInfo.Data, newWave: false, method_8) : TryToSpawnInZoneInner(delayedBotsInfo.BotZone, delayedBotsInfo.Data, delayedBotsInfo.Count, withCheckMinMax: true, newWave: false));
		if (!method_9(gClass, delayedBotsInfo))
		{
			SpawnDelaysService.Remove(delayedBotsInfo, GClass1885.ERemoveDelayReason.NoOneToSpawn);
			if (gClass != null)
			{
				SpawnDelaysService.Add(gClass);
			}
		}
	}

	public bool method_9(GClass1884 v1, GClass1884 v2)
	{
		if (v1 != null && v2 != null)
		{
			if (v1.BotZone != v2.BotZone)
			{
				return false;
			}
			if (v1.Count != v2.Count)
			{
				return false;
			}
			if (v1.Data != v2.Data)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void method_10(BotZone zone, BotCreationDataClass data, Action<BotOwner> callback, CancellationToken cancellationToken)
	{
		if (GameEnd)
		{
			return;
		}
		if (data.SpawnStopped)
		{
			InSpawnProcess--;
			return;
		}
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		bool shallBeGroup = data.SpawnParams != null && data.SpawnParams.ShallBeGroup != null && data.SpawnParams.ShallBeGroup.Group && data.SpawnParams.ShallBeGroup.RemainCount > 0;
		if (shallBeGroup)
		{
			data.SpawnParams.ShallBeGroup.DescreaseCount();
		}
		BotCreator.ActivateBot(data, zone, shallBeGroup, GetGroupAndSetEnemies, delegate(BotOwner bot)
		{
			method_11(bot, data, callback, shallBeGroup, stopWatch);
		}, cancellationToken);
	}

	public void method_11(BotOwner bot, BotCreationDataClass data, Action<BotOwner> callback, bool shallBeGroup, Stopwatch stopWatch)
	{
		if (data.SpawnStopped)
		{
			if (bot != null)
			{
				AllBotsCount++;
				InSpawnProcess--;
				UnityEngine.Debug.LogError("Remove from map");
				bot.LeaveData.RemoveFromMap();
			}
			else
			{
				InSpawnProcess--;
			}
			return;
		}
		bot.SpawnProfileData = data._profileData;
		Bots.Add(bot);
		if (GClass2190.IsFollower(bot.Profile.Info.Settings))
		{
			FollowersBotsCount++;
		}
		else if (GClass2190.IsBoss(bot.Profile.Info.Settings))
		{
			BossBotsCount++;
		}
		AllBotsCount++;
		this.OnBotCreated?.Invoke(bot);
		method_12(bot);
		callback?.Invoke(bot);
		InSpawnProcess--;
		stopWatch.Stop();
		if (shallBeGroup && !data.SpawnParams.ShallBeGroup.IsBossSetted)
		{
			data.SpawnParams.ShallBeGroup.IsBossSetted = true;
			bot.Boss.SetBoss(data.SpawnParams.ShallBeGroup.StartCount);
		}
	}

	public void method_12(BotOwner bot)
	{
		bot.SetDieCallback(BotDied);
		bool flag = GClass2190.IsBoss(bot.Profile.Info.Settings) || GClass2190.IsFollower(bot.Profile.Info.Settings);
		foreach (GClass575 value in Groups.Values)
		{
			bool freeForAll = FreeForAll;
			if (flag)
			{
				freeForAll = false;
			}
			value.AddBot(bot, freeForAll);
		}
	}

	public void method_13(PlayerAIDataClass aiData)
	{
		foreach (BotOwner botOwner in Bots.BotOwners)
		{
			botOwner.EnemiesController.CheckEnemyPursuit(aiData);
		}
	}

	public void AddToTargetBackup(BotDifficulty difficulty, WildSpawnType role, int count)
	{
		BotCreator.AddToTargetBackup(difficulty, role, count);
	}
}
