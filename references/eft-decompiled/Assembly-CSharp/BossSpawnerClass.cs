using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EFT;
using EFT.Game.Spawning;
using JetBrains.Annotations;
using UnityEngine;

public class BossSpawnerClass
{
	public class GClass669
	{
		public readonly BossLocationSpawn Wave;

		public readonly BotZone BotZone;

		public readonly ISpawnPoint PotentialSpawnPoint;

		public GClass669(BossLocationSpawn wave, BotZone botZone, ISpawnPoint bossPoint)
		{
			Wave = wave;
			BotZone = botZone;
			PotentialSpawnPoint = bossPoint;
		}
	}

	[Serializable]
	[CompilerGenerated]
	public class Class331
	{
		public static readonly Class331 class331_0 = new Class331();

		public static Func<BotZone, bool> func_0;

		public static Func<BotZone, bool> func_1;

		public bool method_0(BotZone x)
		{
			if (x.CanSpawnBoss)
			{
				return !x.SnipeZone;
			}
			return false;
		}

		public bool method_1(BotZone x)
		{
			return x.HasPmcBotSpawns;
		}
	}

	[CompilerGenerated]
	public class Class332
	{
		public BossSpawnerClass BossSpawnerClass;

		public GClass669 spawnProcessData;

		public BotCreationDataClass creationData;

		public BotZone botZone;

		public int followersCount;

		public BotSpawnParams spawnParams;

		public BossLocationSpawn wave;

		public EPlayerSide side;

		public List<ISpawnPoint> openedPositions;

		public void method_0(BotOwner owner)
		{
			Class333 CS_0024_003C_003E8__locals8 = new Class333();
			CS_0024_003C_003E8__locals8.class332_0 = this;
			CS_0024_003C_003E8__locals8.owner = owner;
			if (CS_0024_003C_003E8__locals8.owner.BotState == EBotState.Active)
			{
				BossSpawnerClass.List_0.Remove(spawnProcessData);
				return;
			}
			CS_0024_003C_003E8__locals8.owner.OnBotStateChange += delegate(EBotState state)
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				if (state == EBotState.Active)
				{
					CS_0024_003C_003E8__locals8.class332_0.BossSpawnerClass.List_0.Remove(CS_0024_003C_003E8__locals8.class332_0.spawnProcessData);
					CS_0024_003C_003E8__locals8.owner.OnBotStateChange -= CS_0024_003C_003E8__locals8.method_0;
				}
			};
		}

		public void method_1(BotOwner botBoss)
		{
			Class334 CS_0024_003C_003E8__locals16 = new Class334();
			CS_0024_003C_003E8__locals16.class332_0 = this;
			CS_0024_003C_003E8__locals16.botBoss = botBoss;
			if (CS_0024_003C_003E8__locals16.botBoss.BotState == EBotState.Active)
			{
				BossSpawnerClass.List_0.Remove(spawnProcessData);
				BossSpawnerClass.method_5(creationData, botZone, followersCount, spawnParams, wave, side, openedPositions, forceSpawn: false).HandleExceptions();
				return;
			}
			CS_0024_003C_003E8__locals16.botBoss.OnBotStateChange += delegate(EBotState state)
			{
				// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
				if (state == EBotState.Active)
				{
					CS_0024_003C_003E8__locals16.class332_0.BossSpawnerClass.List_0.Remove(CS_0024_003C_003E8__locals16.class332_0.spawnProcessData);
					CS_0024_003C_003E8__locals16.class332_0.BossSpawnerClass.method_5(CS_0024_003C_003E8__locals16.class332_0.creationData, CS_0024_003C_003E8__locals16.class332_0.botZone, CS_0024_003C_003E8__locals16.class332_0.followersCount, CS_0024_003C_003E8__locals16.class332_0.spawnParams, CS_0024_003C_003E8__locals16.class332_0.wave, CS_0024_003C_003E8__locals16.class332_0.side, CS_0024_003C_003E8__locals16.class332_0.openedPositions, forceSpawn: false).HandleExceptions();
					CS_0024_003C_003E8__locals16.botBoss.OnBotStateChange -= CS_0024_003C_003E8__locals16.method_0;
				}
			};
		}
	}

	[CompilerGenerated]
	public class Class333
	{
		public BotOwner owner;

		public Class332 class332_0;

		public void method_0(EBotState state)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (state == EBotState.Active)
			{
				class332_0.BossSpawnerClass.List_0.Remove(class332_0.spawnProcessData);
				owner.OnBotStateChange -= method_0;
			}
		}
	}

	[CompilerGenerated]
	public class Class334
	{
		public BotOwner botBoss;

		public Class332 class332_0;

		public void method_0(EBotState state)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (state == EBotState.Active)
			{
				class332_0.BossSpawnerClass.List_0.Remove(class332_0.spawnProcessData);
				class332_0.BossSpawnerClass.method_5(class332_0.creationData, class332_0.botZone, class332_0.followersCount, class332_0.spawnParams, class332_0.wave, class332_0.side, class332_0.openedPositions, forceSpawn: false).HandleExceptions();
				botBoss.OnBotStateChange -= method_0;
			}
		}
	}

	[CompilerGenerated]
	public class Class335
	{
		public BotProfileDataClass data;

		public int followers;

		public Action<BotOwner> callback;

		public void method_0(BotOwner ownerBoss)
		{
			BotSpawnParams spawnParams = data.SpawnParams;
			if (spawnParams != null && spawnParams.ShallBeGroup != null && !spawnParams.ShallBeGroup.IsBossSetted)
			{
				spawnParams.ShallBeGroup.IsBossSetted = true;
				ownerBoss.Boss.SetBoss(followers);
			}
			callback(ownerBoss);
		}
	}

	public readonly List<BotZone> AvailableZonesPmc;

	[NonSerialized]
	public const float Float_0 = 20f;

	[NonSerialized]
	public List<GClass669> List_0 = new List<GClass669>();

	[NonSerialized]
	public BotSpawner BotSpawner_0;

	[NonSerialized]
	public ISpawnSystem ISpawnSystem;

	[NonSerialized]
	public List<BotZone> List_1;

	[NonSerialized]
	public BotZone[] BotZone_0;

	[NonSerialized]
	public GClass1885 Gclass1885_0;

	[NonSerialized]
	public List<WildSpawnType> List_2 = new List<WildSpawnType>();

	[NonSerialized]
	public List<WildSpawnType> List_3 = new List<WildSpawnType>
	{
		WildSpawnType.sectantPriest,
		WildSpawnType.sectantWarrior,
		WildSpawnType.sectantOni,
		WildSpawnType.sectantPredvestnik,
		WildSpawnType.sectantPrizrak
	};

	[NonSerialized]
	public float Float_1 = -100f;

	[NonSerialized]
	public WildSpawnType WildSpawnType_0 = WildSpawnType.assault;

	[NonSerialized]
	public BotZone BotZone_1;

	[NonSerialized]
	public IBotCreator IBotCreator;

	public BossSpawnerClass(ISpawnSystem spawnSystem, BotSpawner spawner, IBotCreator botCreator, BotZone[] allZones, GClass1885 spawnDelaysService)
	{
		IBotCreator = botCreator;
		WildSpawnType[] array = (WildSpawnType[])Enum.GetValues(typeof(WildSpawnType));
		foreach (WildSpawnType wildSpawnType in array)
		{
			if (BotSettingsRepoClass.IsBossOrFollower(wildSpawnType) && !BotSettingsRepoClass.IsSectant(wildSpawnType))
			{
				List_2.Add(wildSpawnType);
			}
		}
		BotSpawner_0 = spawner;
		ISpawnSystem = spawnSystem;
		BotZone_0 = allZones;
		Gclass1885_0 = spawnDelaysService;
		List_1 = BotZone_0.Where((BotZone x) => x.CanSpawnBoss && !x.SnipeZone).ToList();
		AvailableZonesPmc = BotZone_0.Where((BotZone x) => x.HasPmcBotSpawns).ToList();
	}

	public void InitBossSpawnDebug()
	{
		Spawn(null, null).HandleExceptions();
	}

	public async Task Spawn([CanBeNull] BossLocationSpawn wave, BotSpawnParams spawnParams, [CanBeNull] BotCreationDataClass data = null)
	{
		if (wave == null)
		{
			return;
		}
		BotDifficulty bossDif = wave.BossDif;
		int escortCount = wave.EscortCount;
		bool flag = true;
		if (!wave.IgnoreMaxBots)
		{
			BotSpawner_0.CheckOnMax(escortCount + 1, out var toDelay, out var _);
			flag = toDelay == 0;
			if (!flag && BotSettingsRepoClass.IsSectant(wave.BossType))
			{
				List<BotZone> possibleZones = wave.GetPossibleZones(BotZone_0, List_1);
				ZoneLeaveControllerClass zonesLeaveController = BotSpawner_0.BotGame.BotsController.ZonesLeaveController;
				foreach (BotZone item in possibleZones)
				{
					if (!zonesLeaveController.IsZoneBlockFor(item, wave.BossType))
					{
						zonesLeaveController.BlockAllZonesSimpleTypes();
						zonesLeaveController.ClearForPlaces(wave.EscortCount + 1);
						break;
					}
				}
			}
		}
		BotCreationDataClass creationData = ((data == null) ? (await BotCreationDataClass.Create(new BotProfileDataClass(EPlayerSide.Savage, wave.BossType, wave.BossDif, wave.Time, spawnParams), IBotCreator, 1, BotSpawner_0)) : data);
		if (flag)
		{
			if (!method_2(wave, spawnParams, bossDif, escortCount, creationData, List_0))
			{
				method_0(wave, spawnParams, creationData);
			}
		}
		else
		{
			method_0(wave, spawnParams, creationData);
		}
	}

	public void method_0(BossLocationSpawn wave, BotSpawnParams spawnParams, BotCreationDataClass creationData)
	{
		Gclass1885_0.Add(new GClass1883(wave, spawnParams, 1, creationData, method_1));
	}

	public void method_1(GClass1883 spawnDelayModel)
	{
		Gclass1885_0.Remove(spawnDelayModel, GClass1885.ERemoveDelayReason.NoOneToSpawn);
		Spawn(spawnDelayModel.Wave, spawnDelayModel.SpawnParams, spawnDelayModel.Data).HandleExceptions();
	}

	public bool method_2(BossLocationSpawn wave, BotSpawnParams spawnParams, BotDifficulty difficulty, int followersCount, BotCreationDataClass creationData, List<GClass669> spawnProcess)
	{
		if (!BotSpawner_0.IsProfilesLoaded)
		{
			return false;
		}
		bool result = false;
		List<BotZone> markedBossZone = (BotSettingsRepoClass.IsPmcBot(wave.BossType) ? AvailableZonesPmc : List_1);
		List<BotZone> list = ((!wave.PerfectPos.HasValue) ? wave.GetPossibleZones(BotZone_0, markedBossZone) : wave.GetPossibleZones(BotZone_0, markedBossZone, wave.PerfectPos.Value));
		if (list.Count == 0)
		{
			Debug.LogError("can't find zones to spawn boss check zones on this mark _avalabaleZones:" + List_1.Count);
			return true;
		}
		if (list[0] == null)
		{
			Debug.LogError("can't find zones to spawn boss check zones on this mark _availableZones count is 0. Check backend settings at map" + List_1.Count);
			return true;
		}
		bool flag = BotSettingsRepoClass.IsSectant(wave.BossType);
		int num = 0;
		BotZone botZone;
		List<ISpawnPoint> list2;
		while (true)
		{
			if (num < list.Count)
			{
				botZone = list[num];
				if (!BotSpawner_0.BotGame.BotsController.ZonesLeaveController.IsZoneBlockFor(botZone, wave.BossType))
				{
					float num2 = Time.time - Float_1;
					bool flag2 = true;
					bool flag3 = BotSpawner_0.BotGame.Status == GameStatus.Running;
					if ((num2 < 1f || (wave.IsStartWave() && flag3)) && botZone == BotZone_1)
					{
						if (flag)
						{
							if (List_2.Contains(WildSpawnType_0))
							{
								flag2 = false;
							}
						}
						else if (BotSettingsRepoClass.IsSectant(WildSpawnType_0))
						{
							flag2 = false;
						}
					}
					BotZoneGroupsDictionary botZoneGroupsDictionary = BotSpawner_0.BotGame.BotsController.Groups();
					if (!wave.ForceSpawn && (!DebugBotData.UseDebugData || !DebugBotData.Instance.spawnInstantly) && botZoneGroupsDictionary != null && botZoneGroupsDictionary.TryGetValue(botZone, out var value))
					{
						foreach (BotsGroup group in value.GetGroups(notNull: true))
						{
							if (group != null && flag && method_4(group, List_3))
							{
								flag2 = false;
							}
						}
					}
					if (flag2)
					{
						int num3 = 1;
						int num4 = 1 + wave.EscortCount;
						ActionIfNotEnoughPoints mode = ((!wave.ForceSpawn) ? ActionIfNotEnoughPoints.ReturnNothing : ActionIfNotEnoughPoints.FillWithDiscardedPointsAndDuplicates);
						list2 = ISpawnSystem.SelectAISpawnPoints(creationData, botZone, num3, wave.PerfectPos, mode, spawnProcess);
						if (list2.Count >= num3)
						{
							if (BotSettingsRepoClass.IsSectant(wave.BossType))
							{
								BotSpawner_0.BotGame.BotsController.ZonesLeaveController.UnBlockAllZonesAsDay();
								BotSpawner_0.BotGame.BotsController.ZonesLeaveController.ClearZoneAndBlockForSimlpe(botZone);
							}
							if (list2.Count < num4 && list2.Count > 0)
							{
								int num5 = num4 - list2.Count;
								ISpawnPoint item = list2[0];
								for (int i = 0; i < num5; i++)
								{
									list2.Add(item);
								}
							}
							if (list2.Count >= num4)
							{
								break;
							}
						}
					}
				}
				num++;
				continue;
			}
			return result;
		}
		Float_1 = Time.time;
		WildSpawnType_0 = wave.BossType;
		BotZone_1 = botZone;
		result = true;
		if (creationData.SpawnStopped)
		{
			return false;
		}
		method_3(creationData, wave, spawnParams, followersCount, botZone, list2).HandleExceptions();
		return true;
	}

	public async Task method_3(BotCreationDataClass creationData, BossLocationSpawn wave, BotSpawnParams spawnParams, int followersCount, BotZone botZone, List<ISpawnPoint> openedPositions)
	{
		float time = wave.Time;
		spawnParams.ShallBeGroup = new ShallBeGroupParams(group: true, bossGroup: true, followersCount + 1);
		BotProfileDataClass data = new BotProfileDataClass(EPlayerSide.Savage, wave.BossType, wave.BossDif, time, spawnParams);
		EPlayerSide side = EPlayerSide.Savage;
		bool num = wave.IsStartWave();
		ISpawnPoint spawnPoint = openedPositions[0];
		openedPositions.Remove(spawnPoint);
		GClass669 spawnProcessData = new GClass669(wave, botZone, spawnPoint);
		List_0.Add(spawnProcessData);
		Class332 CS_0024_003C_003E8__locals8;
		if (num)
		{
			if (!BotSpawner_0.CanSpawnRole(data))
			{
				return;
			}
			await method_7(creationData, spawnPoint, botZone, followersCount, data, delegate(BotOwner owner)
			{
				Class333 CS_0024_003C_003E8__locals15 = new Class333();
				CS_0024_003C_003E8__locals15.class332_0 = CS_0024_003C_003E8__locals8;
				CS_0024_003C_003E8__locals15.owner = owner;
				if (CS_0024_003C_003E8__locals15.owner.BotState == EBotState.Active)
				{
					List_0.Remove(spawnProcessData);
				}
				else
				{
					CS_0024_003C_003E8__locals15.owner.OnBotStateChange += delegate(EBotState state)
					{
						// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
						if (state == EBotState.Active)
						{
							CS_0024_003C_003E8__locals15.class332_0.BossSpawnerClass.List_0.Remove(CS_0024_003C_003E8__locals15.class332_0.spawnProcessData);
							CS_0024_003C_003E8__locals15.owner.OnBotStateChange -= CS_0024_003C_003E8__locals15.method_0;
						}
					};
				}
			});
			await method_5(creationData, botZone, followersCount, spawnParams, wave, side, openedPositions, forceSpawn: true);
			return;
		}
		await method_7(creationData, spawnPoint, botZone, followersCount, data, delegate(BotOwner botBoss)
		{
			Class334 CS_0024_003C_003E8__locals27 = new Class334();
			CS_0024_003C_003E8__locals27.class332_0 = CS_0024_003C_003E8__locals8;
			CS_0024_003C_003E8__locals27.botBoss = botBoss;
			if (CS_0024_003C_003E8__locals27.botBoss.BotState == EBotState.Active)
			{
				List_0.Remove(spawnProcessData);
				method_5(creationData, botZone, followersCount, spawnParams, wave, side, openedPositions, forceSpawn: false).HandleExceptions();
			}
			else
			{
				CS_0024_003C_003E8__locals27.botBoss.OnBotStateChange += delegate(EBotState state)
				{
					// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
					if (state == EBotState.Active)
					{
						CS_0024_003C_003E8__locals27.class332_0.BossSpawnerClass.List_0.Remove(CS_0024_003C_003E8__locals27.class332_0.spawnProcessData);
						CS_0024_003C_003E8__locals27.class332_0.BossSpawnerClass.method_5(CS_0024_003C_003E8__locals27.class332_0.creationData, CS_0024_003C_003E8__locals27.class332_0.botZone, CS_0024_003C_003E8__locals27.class332_0.followersCount, CS_0024_003C_003E8__locals27.class332_0.spawnParams, CS_0024_003C_003E8__locals27.class332_0.wave, CS_0024_003C_003E8__locals27.class332_0.side, CS_0024_003C_003E8__locals27.class332_0.openedPositions, forceSpawn: false).HandleExceptions();
						CS_0024_003C_003E8__locals27.botBoss.OnBotStateChange -= CS_0024_003C_003E8__locals27.method_0;
					}
				};
			}
		});
	}

	public bool method_4(BotsGroup botsGroup, List<WildSpawnType> ignoreRoles)
	{
		int num = 0;
		ProfileInfoSettingsClass settings;
		while (true)
		{
			if (num < botsGroup.MembersCount)
			{
				settings = botsGroup.Member(num).Profile.Info.Settings;
				if (GClass2190.IsBossOrFollower(settings))
				{
					break;
				}
				num++;
				continue;
			}
			return false;
		}
		bool flag = false;
		foreach (WildSpawnType ignoreRole in ignoreRoles)
		{
			if (settings.Role != ignoreRole)
			{
				flag = true;
			}
		}
		if (flag)
		{
			return true;
		}
		return false;
	}

	public async Task method_5(BotCreationDataClass bossCreationData, BotZone zone, int followersCount, BotSpawnParams spawnParams, BossLocationSpawn wave, EPlayerSide side, List<ISpawnPoint> pointsToSpawn, bool forceSpawn)
	{
		List<BossLocationSpawnSubData> escors = wave.GetEscors();
		if (escors != null)
		{
			method_6(bossCreationData, zone, side, wave, escors, spawnParams, pointsToSpawn, forceSpawn).HandleExceptions();
		}
		else if (followersCount > 0)
		{
			BotCreationDataClass data = await BotCreationDataClass.Create(new BotProfileDataClass(EPlayerSide.Savage, wave.EscortType, wave.EscortDif, wave.Time, spawnParams), IBotCreator, followersCount, BotSpawner_0);
			BotSpawner_0.TryToSpawnInZoneAndDelay(zone, data, withCheckMinMax: false, newWave: true, pointsToSpawn, forceSpawn);
		}
	}

	public async Task method_6(BotCreationDataClass creationData, BotZone zone, EPlayerSide side, BossLocationSpawn wave, List<BossLocationSpawnSubData> escorts, BotSpawnParams spawnParams, List<ISpawnPoint> pointsToSpawn, bool forceSpawn)
	{
		if (wave.EscortCount > pointsToSpawn.Count)
		{
			pointsToSpawn = null;
		}
		foreach (BossLocationSpawnSubData escort in escorts)
		{
			List<ISpawnPoint> list = null;
			if (pointsToSpawn != null)
			{
				list = new List<ISpawnPoint>();
				for (int i = 0; i < escort.BossEscortAmount; i++)
				{
					if (pointsToSpawn.Count > 0)
					{
						ISpawnPoint item = pointsToSpawn.First();
						list.Add(item);
						pointsToSpawn.Remove(item);
					}
				}
				if (escort.BossEscortAmount != list.Count)
				{
					list = null;
				}
			}
			BotCreationDataClass data = await BotCreationDataClass.Create(new BotProfileDataClass(side, escort.BossEscortType, escort.EscortDifficulty, wave.Time, spawnParams), IBotCreator, escort.BossEscortAmount, BotSpawner_0);
			BotSpawner_0.TryToSpawnInZoneAndDelay(zone, data, withCheckMinMax: false, newWave: true, list, forceSpawn);
			await Task.Yield();
		}
	}

	public async Task method_7(BotCreationDataClass creationData, ISpawnPoint point, BotZone ss, int followers, BotProfileDataClass data, Action<BotOwner> callback)
	{
		List<ISpawnPoint> openedPositions = new List<ISpawnPoint> { point };
		BotSpawner_0.SpawnBotsInZoneOnPositions(openedPositions, ss, creationData, delegate(BotOwner ownerBoss)
		{
			BotSpawnParams spawnParams = data.SpawnParams;
			if (spawnParams != null && spawnParams.ShallBeGroup != null && !spawnParams.ShallBeGroup.IsBossSetted)
			{
				spawnParams.ShallBeGroup.IsBossSetted = true;
				ownerBoss.Boss.SetBoss(followers);
			}
			callback(ownerBoss);
		});
	}
}
