using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Comfort.Common;
using EFT;
using EFT.Counters;
using EFT.Game.Spawning;
using EFT.HealthSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class BotsGroup
{
	public enum BotCurrentTactic
	{
		Attack,
		Ambush,
		Protect
	}

	[Serializable]
	[CompilerGenerated]
	public class Class272
	{
		public static readonly Class272 class272_0 = new Class272();

		public static Func<KeyValuePair<IPlayer, BotSettingsClass>, bool> func_0;

		public static Func<KeyValuePair<IPlayer, BotSettingsClass>, float> func_1;

		public static Func<EnemyInfo, float> func_2;

		public static Predicate<PlaceForCheck> predicate_0;

		public static Func<KeyValuePair<IPlayer, BotSettingsClass>, bool> func_3;

		public static Func<KeyValuePair<IPlayer, BotSettingsClass>, BotSettingsClass> func_4;

		public static Func<BotOwner, float> func_5;

		public bool method_0(KeyValuePair<IPlayer, BotSettingsClass> player)
		{
			return player.Value.IsHaveSeen;
		}

		public float method_1(KeyValuePair<IPlayer, BotSettingsClass> player)
		{
			return player.Key.AIData.PowerOfEquipment;
		}

		public float method_2(EnemyInfo info)
		{
			return (info.EnemyLastPosition - info.Owner.Transform.position).sqrMagnitude;
		}

		public bool method_3(PlaceForCheck placeForCheck)
		{
			return placeForCheck.IsOld();
		}

		public bool method_4(KeyValuePair<IPlayer, BotSettingsClass> x)
		{
			if (!x.Value.IsLastPositionChecked)
			{
				return x.Key.HealthController.IsAlive;
			}
			return false;
		}

		public BotSettingsClass method_5(KeyValuePair<IPlayer, BotSettingsClass> x)
		{
			return x.Value;
		}

		public float method_6(BotOwner x)
		{
			return x.Tactic.AggressionCoef;
		}
	}

	[CompilerGenerated]
	public class Class273
	{
		public BotsGroup botsGroup_0;

		public GClass578 place;

		public void method_0()
		{
			botsGroup_0.GrenadeSmokePlaces.Remove(place);
		}
	}

	[CompilerGenerated]
	public class Class274
	{
		public BotsGroup botsGroup_0;

		public IPlayer enemy;

		public void method_0(PlayerAIDataClass data)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			if (enemy.HealthController.IsAlive)
			{
				botsGroup_0.RemoveEnemy(enemy, EBotEnemyCause.drunk);
				enemy.AIData.OnBecomeDrunk -= method_0;
			}
		}
	}

	[CompilerGenerated]
	public class Class275
	{
		public PlaceForCheck place;

		public bool method_0(PlaceForCheck check)
		{
			return place.IsThisPointClose(check.BasePoint);
		}
	}

	public readonly IBotGame BotGame;

	public Dictionary<IPlayer, BotSettingsClass> Enemies = new Dictionary<IPlayer, BotSettingsClass>(15);

	public Dictionary<IPlayer, BotSettingsClass> Neutrals = new Dictionary<IPlayer, BotSettingsClass>(10);

	public EPlayerSide Side;

	public BotCurrentEnemiesClass CurrentEnemies;

	[NonSerialized]
	public List<BotOwner> Members = new List<BotOwner>();

	[NonSerialized]
	public List<GClass578> GrenadeSmokePlaces = new List<GClass578>();

	[NonSerialized]
	public float NextGetGoalTime = 2f;

	[NonSerialized]
	public float NextCheckDestination = 2f;

	[NonSerialized]
	public GClass573 GroupDangerAreas;

	[NonSerialized]
	public WildSpawnType DefWildSpawnType;

	[NonSerialized]
	public BotGlobalsMindSettings InitialBotMindSettings_1;

	[NonSerialized]
	public List<string> EnemyPlayerGroups = new List<string>();

	[NonSerialized]
	public bool EnemyByGroupsPmcPlayers;

	[NonSerialized]
	public bool EnemyByGroupsSavagePlayers;

	[NonSerialized]
	public BotOwner InitialBot;

	[NonSerialized]
	public bool IsFirstMemberAdded;

	[NonSerialized]
	public List<IPlayer> RecheckPersonsAfterInit = new List<IPlayer>();

	[NonSerialized]
	public static int GroupsIds = 1;

	[field: NonSerialized]
	public bool Locked { get; set; }

	public BotGlobalsMindSettings InitialBotMindSettings => InitialBotMindSettings_1;

	[field: NonSerialized]
	public WildSpawnType InitialBotType { get; }

	[field: NonSerialized]
	public BotDifficulty InitialBotDifficulty { get; }

	[field: NonSerialized]
	public BotSettingsComponents InitialFileSettings { get; }

	[field: NonSerialized]
	public List<IPlayer> Allies { get; } = new List<IPlayer>();

	[field: NonSerialized]
	public BossGroup BossGroup { get; set; }

	public int DangerAreasCount => GroupDangerAreas.DangerZonesCount;

	public int AssaultGroupsCount => GroupDangerAreas.AssaultGroupsCount;

	[field: NonSerialized]
	public float EnemyLastSeenTimeSence { get; set; }

	[field: NonSerialized]
	public float EnemyLastSeenTimeReal { get; set; }

	[field: NonSerialized]
	public BotsGroupMarkOfUnknown GroupMarkOfUnknown { get; }

	[field: NonSerialized]
	public BotsGroupGrenade GroupGrenade { get; }

	[field: NonSerialized]
	public BotsGroupLaying GroupLaying { get; }

	[field: NonSerialized]
	public BotGroupTalk GroupTalk { get; }

	[field: NonSerialized]
	public Vector3 EnemyLastSeenPositionReal { get; set; }

	[field: NonSerialized]
	public Vector3 EnemyLastSeenPositionSence { get; set; }

	[field: NonSerialized]
	public LastSoundsController LastSoundsController { get; }

	[field: NonSerialized]
	public CoverPointMaster CoverPointMaster { get; }

	[field: NonSerialized]
	public BotZone BotZone { get; }

	[field: NonSerialized]
	public DeadBodiesController DeadBodiesController { get; }

	[field: NonSerialized]
	public BotGroupWarnData BotGroupWarnData { get; set; }

	[field: NonSerialized]
	public string Name { get; }

	[field: NonSerialized]
	public int Id { get; }

	[field: NonSerialized]
	public bool IsLastPositionOld { get; set; }

	[field: NonSerialized]
	public BotGroupRequestController RequestsController { get; }

	[field: NonSerialized]
	public bool ForcedAggressiveForNewPlayers { get; set; }

	public float GroupPower
	{
		get
		{
			float num = 0f;
			foreach (BotOwner member in Members)
			{
				num += member.AIData.PowerOfEquipment;
			}
			return num;
		}
	}

	public float Single_0 => Enemies.Where((KeyValuePair<IPlayer, BotSettingsClass> player) => player.Value.IsHaveSeen).Sum((KeyValuePair<IPlayer, BotSettingsClass> player) => player.Key.AIData.PowerOfEquipment);

	public int MembersCount => Members.Count;

	public int AllyCount => Allies.Count;

	[field: NonSerialized]
	public List<PlaceForCheck> PlacesForCheck { get; } = new List<PlaceForCheck>(70);

	[field: NonSerialized]
	public bool AnyBodyShootImmediately { get; set; }

	[field: NonSerialized]
	public int TargetMembersCount { get; set; }

	[field: NonSerialized]
	public bool IsFull { get; set; }

	public event Action<IPlayer, EBotEnemyCause> OnEnemyAdd;

	public event Action<IPlayer> OnEnemyRemove;

	public event Action<IPlayer> OnAddNeutral;

	public event GDelegate6 OnReportEnemy;

	public event Action<BotOwner> OnBossSetted;

	public event Action<BotOwner> OnMemberRemove;

	public event Action<BotOwner> OnMemberAdd;

	public BotsGroup(BotZone zone, IBotGame botGame, BotOwner initialBot, List<BotOwner> enemies, DeadBodiesController deadBodiesController, List<Player> allPlayers, bool forBoss)
	{
		GroupTalk = new BotGroupTalk();
		GroupGrenade = new BotsGroupGrenade();
		GroupLaying = new BotsGroupLaying();
		CurrentEnemies = new BotCurrentEnemiesClass(this);
		LastSoundsController = new LastSoundsController(this);
		RequestsController = new BotGroupRequestController(this);
		GroupMarkOfUnknown = new BotsGroupMarkOfUnknown(this);
		DeadBodiesController = deadBodiesController;
		BotGroupWarnData = new BotGroupWarnData(this);
		InitialBot = initialBot;
		Id = GroupsIds++;
		Side = initialBot.Profile.Info.Side;
		InitialBotType = initialBot.Profile.Info.Settings.Role;
		InitialBotDifficulty = initialBot.Profile.Info.Settings.BotDifficulty;
		InitialFileSettings = initialBot.Settings.FileSettings;
		BotGame = botGame;
		BotZone = zone;
		InitialBotMindSettings_1 = initialBot.Settings.FileSettings.Mind;
		DefWildSpawnType = initialBot.Profile.Info.Settings.Role;
		EnemyByGroupsPmcPlayers = initialBot.Settings.FileSettings.Mind.ENEMY_BY_GROUPS_PMC_PLAYERS;
		EnemyByGroupsSavagePlayers = initialBot.Settings.FileSettings.Mind.ENEMY_BY_GROUPS_SAVAGE_PLAYERS;
		foreach (Player allPlayer in allPlayers)
		{
			if (IsPlayerEnemy(allPlayer))
			{
				AddEnemy(allPlayer, EBotEnemyCause.initial);
			}
			else
			{
				AddNeutral(allPlayer);
			}
		}
		IPlayer[] array = Enemies.Keys.ToArray();
		foreach (IPlayer player in array)
		{
			AddEnemyGroupIfAllowed(player.GroupId, player.Side);
		}
		foreach (BotOwner enemy in enemies)
		{
			AddEnemy(enemy, EBotEnemyCause.initCauseEnemy);
		}
		Name = string.Format("{1} [{0}]", forBoss ? "Boss" : "Simple", zone.gameObject.name);
		CoverPointMaster = CoverPointMaster.Create(zone);
		StaticManager.Instance.StaticUpdate += method_3;
		method_16();
		GroupDangerAreas = new GClass573(zone.ZoneDangerAreas, this);
	}

	public void Lock()
	{
		Locked = true;
	}

	public bool IsPlayerEnemy(IPlayer player)
	{
		bool flag = false;
		if (player.AIData.IsAI)
		{
			WildSpawnType role = player.AIData.BotOwner.Profile.Info.Settings.Role;
			if (IsSuitable(role))
			{
				return false;
			}
			if (InitialBot.Settings.GetFriendlyBotTypes().Contains(role) || InitialBot.Settings.GetWarnBotTypes().Contains(role))
			{
				return false;
			}
			if (InitialBot.Settings.GetEnemyBotTypes().Contains(role))
			{
				flag = true;
			}
		}
		bool flag2 = false;
		if (!player.IsAI || (player.IsAI && player.AIData.BotOwner.BotsGroup != this))
		{
			flag2 = InitialBot.Settings.IsPlayerEnemy(player);
		}
		switch (player.Side)
		{
		case EPlayerSide.Usec:
			flag = flag || flag2 || method_1(player);
			break;
		case EPlayerSide.Bear:
			flag = flag || flag2;
			break;
		case EPlayerSide.Savage:
			flag = flag || player.Loyalty.HostileScavs;
			if (flag2)
			{
				flag = !BotSettingsRepoClass.IsBoss(InitialBotType) || player.Loyalty == null || flag || !player.Loyalty.BossNoAttack || InitialBotMindSettings_1.BOSS_IGNORE_LOYALTY;
			}
			break;
		}
		if (!player.IsAI && InitialBotMindSettings_1.CHECK_MARK_OF_UNKNOWS)
		{
			EPlayerSide side = player.Side;
			if ((uint)(side - 1) <= 1u && GroupMarkOfUnknown.HasMarkOfUnknown(player))
			{
				flag = false;
			}
		}
		return flag;
	}

	public bool IsAlly(IPlayer player)
	{
		return Allies.Contains(player);
	}

	public void SetAggressiveToAllNewPlayers(bool state)
	{
		ForcedAggressiveForNewPlayers = state;
	}

	public bool HaveMemberWithRole(WildSpawnType type)
	{
		foreach (BotOwner member in Members)
		{
			if (member.IsRole(type))
			{
				return true;
			}
		}
		return false;
	}

	public void AddEnemyGroupIfAllowed(string playerGroupId, EPlayerSide playerSide)
	{
		if (((!EnemyByGroupsPmcPlayers || playerSide == EPlayerSide.Savage) && (!EnemyByGroupsSavagePlayers || playerSide != EPlayerSide.Savage)) || string.IsNullOrEmpty(playerGroupId) || EnemyPlayerGroups.Contains(playerGroupId))
		{
			return;
		}
		EnemyPlayerGroups.Add(playerGroupId);
		KeyValuePair<IPlayer, BotSettingsClass>[] array = Neutrals.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<IPlayer, BotSettingsClass> keyValuePair = array[i];
			if (keyValuePair.Key.GroupId == playerGroupId)
			{
				AddEnemy(keyValuePair.Key, EBotEnemyCause.addCauseGroup);
			}
		}
	}

	public BotOwner Member(int i)
	{
		return Members[i];
	}

	public bool Contains(BotOwner member)
	{
		return Members.Contains(member);
	}

	public void CalcGoalForBot(BotOwner bot)
	{
		bot.GoalCulculator.CalcGoalForBot();
	}

	[CanBeNull]
	public PlaceForCheck YoungestFastPlace(BotOwner owner, float distRestriction, float timeOld)
	{
		float num = distRestriction * distRestriction;
		if (PlacesForCheck.Count <= 0)
		{
			return null;
		}
		PlaceForCheck placeForCheck = PlacesForCheck[PlacesForCheck.Count - 1];
		if ((owner.Transform.position - placeForCheck.BasePoint).sqrMagnitude < num && Time.time - placeForCheck.CreatedTime < timeOld)
		{
			return placeForCheck;
		}
		return null;
	}

	[CanBeNull]
	public PlaceForCheck YoungestPlace(BotOwner owner, float distRestriction, bool priorityDanger)
	{
		float num = distRestriction * distRestriction;
		PlaceForCheck result = null;
		float num2 = float.MaxValue;
		for (int num3 = PlacesForCheck.Count - 1; num3 >= 0; num3--)
		{
			PlaceForCheck placeForCheck = PlacesForCheck[num3];
			if ((owner.Transform.position - placeForCheck.BasePoint).sqrMagnitude < num)
			{
				float num4 = Time.time - placeForCheck.CreatedTime;
				if (priorityDanger && placeForCheck.IsDanger && num4 < owner.Settings.FileSettings.Look.GOAL_TO_FULL_DISSAPEAR)
				{
					num4 *= 0.8f;
				}
				if (num4 < num2)
				{
					num2 = num4;
					result = placeForCheck;
				}
			}
		}
		return result;
	}

	public void AddAlly(Player ally)
	{
		if (!Allies.Contains(ally))
		{
			Allies.Add(ally);
		}
	}

	public void RemoveAlly(BotOwner ally)
	{
		Members.Remove(ally);
		this.OnMemberRemove?.Invoke(ally);
		foreach (BotOwner member in Members)
		{
			member.Tactic.AggressionChange(member.Settings.FileSettings.Mind.FRIEND_DEAD_AGR_LOW);
		}
		if (!ally.HealthController.IsAlive)
		{
			DeadBodiesController.AddBody(ally.GetPlayer);
		}
		if (Members.Count == 0)
		{
			PlacesForCheck.Clear();
			List<IPlayer> list = new List<IPlayer>();
			foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in Enemies)
			{
				enemy.Value.Clear();
				if (enemy.Key.Side == Side)
				{
					list.Add(enemy.Key);
				}
			}
			foreach (IPlayer item in list)
			{
				Enemies.Remove(item);
				this.OnEnemyRemove?.Invoke(item);
				foreach (BotOwner member2 in Members)
				{
					member2.Memory.DeleteInfoAboutEnemy(item);
				}
			}
		}
		method_16();
	}

	public void ReportAboutEnemy([NotNull] IPlayer enemy, EEnemyPartVisibleType isVisibleOnlyBySence, BotOwner reporter)
	{
		SetEnemyPos(enemy, enemy.Transform.position, enemy.WeaponRoot.position, isVisibleOnlyBySence);
		this.OnReportEnemy?.Invoke(enemy, enemy.Transform.position, enemy.WeaponRoot.position, isVisibleOnlyBySence, reporter);
	}

	public bool IsInSmoke(Vector3 p)
	{
		if (GrenadeSmokePlaces == null)
		{
			return false;
		}
		if (GrenadeSmokePlaces.Count == 0)
		{
			return false;
		}
		foreach (GClass578 grenadeSmokePlace in GrenadeSmokePlaces)
		{
			if (grenadeSmokePlace.IsInRadius(p))
			{
				return true;
			}
		}
		return false;
	}

	public void AddSmokePlace(Vector3 pos, float duration, float radius, Vector3 throwPos)
	{
		if (Members.Count > 0 && !((ClosestMemberToPoint(pos).Position - pos).sqrMagnitude > 22500f))
		{
			GClass578 place = new GClass578(pos, duration, radius);
			GrenadeSmokePlaces.Add(place);
			StaticManager.Instance.TimerManager.MakeTimer(TimeSpan.FromSeconds(duration)).OnTimer += delegate
			{
				GrenadeSmokePlaces.Remove(place);
			};
			for (int num = 0; num < Members.Count; num++)
			{
				BotOwner botOwner = Member(num);
				botOwner.SmokeGrenade.AddSmokeGrenadeData(place);
				AddPointToSearch(throwPos, 160f, botOwner, baseReacheble: false);
			}
		}
	}

	[CanBeNull]
	public PlaceForCheck AddPointToSearch(Vector3 suspectedPoint, float power, BotOwner owner, bool baseReacheble = true, bool canUseCovers = true)
	{
		bool flag = power > LocalBotSettingsProviderClass.Core.DANGER_POWER;
		PlaceForCheck placeForCheck = null;
		try
		{
			placeForCheck = method_13(owner, suspectedPoint, flag ? PlaceForCheckType.danger : PlaceForCheckType.simple, canUseCovers);
		}
		catch (Exception)
		{
			return null;
		}
		if (placeForCheck == null)
		{
			return null;
		}
		if (IsInSmoke(suspectedPoint))
		{
			baseReacheble = false;
		}
		placeForCheck.Reacheble = baseReacheble;
		if (baseReacheble)
		{
			if (method_7(placeForCheck))
			{
				method_11(placeForCheck);
			}
		}
		else
		{
			method_11(placeForCheck);
		}
		return placeForCheck;
	}

	public void RemoveInfo(IPlayer person)
	{
		method_14(person.Transform.position);
	}

	public bool AddEnemy(IPlayer person, EBotEnemyCause cause)
	{
		Class274 CS_0024_003C_003E8__locals8 = new Class274();
		CS_0024_003C_003E8__locals8.botsGroup_0 = this;
		if (!IsFirstMemberAdded)
		{
			RecheckPersonsAfterInit.Add(person);
			return false;
		}
		foreach (BotOwner member in Members)
		{
			if (member.GetPlayer.Id == person.Id)
			{
				return false;
			}
		}
		if (InitialBot.Settings.FileSettings.Mind.USE_ADD_TO_ENEMY_VALIDATION && !InitialBot.Settings.FileSettings.Mind.VALID_REASONS_TO_ADD_ENEMY.Contains(cause))
		{
			return false;
		}
		if (cause != EBotEnemyCause.checkAddTODO && cause != EBotEnemyCause.byKill && InitialBot.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY && !person.IsAI && person.AIData.IsDrunk)
		{
			return false;
		}
		if (!Enemies.TryGetValue(person, out var value))
		{
			if (person.IsAI && HaveMemberWithRole(WildSpawnType.gifter))
			{
				return false;
			}
			bool flag = true;
			WildSpawnType defWildSpawnType = DefWildSpawnType;
			if ((uint)defWildSpawnType > 1u && defWildSpawnType != WildSpawnType.assaultGroup && (uint)(defWildSpawnType - 29) <= 1u && Members.Count > 0)
			{
				foreach (BotOwner member2 in Members)
				{
					if (member2.IsRole(WildSpawnType.bossZryachiy) && member2.Boss.BossLogic is ZyriachyBossLogicClass zyriachyBossLogicClass && !zyriachyBossLogicClass.IsEnemyNow(person))
					{
						flag = false;
						break;
					}
				}
			}
			if (!flag)
			{
				AddNeutral(person);
				return false;
			}
			person.AIData.CalcPower();
			Player alivePlayerByProfileID = Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(person.ProfileId);
			if (alivePlayerByProfileID == null)
			{
				return false;
			}
			value = new BotSettingsClass(alivePlayerByProfileID, this, cause);
			if (InitialBot.Settings.FileSettings.Mind.REACT_ADD_DRUNK_ENEMY)
			{
				CS_0024_003C_003E8__locals8.enemy = person;
				CS_0024_003C_003E8__locals8.enemy.AIData.OnBecomeDrunk += delegate
				{
					// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
					if (CS_0024_003C_003E8__locals8.enemy.HealthController.IsAlive)
					{
						CS_0024_003C_003E8__locals8.botsGroup_0.RemoveEnemy(CS_0024_003C_003E8__locals8.enemy, EBotEnemyCause.drunk);
						CS_0024_003C_003E8__locals8.enemy.AIData.OnBecomeDrunk -= CS_0024_003C_003E8__locals8.method_0;
					}
				};
			}
			Enemies.Add(person, value);
			this.OnEnemyAdd?.Invoke(person, cause);
			RequestsController.RemoveAllRequestByRequester(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(person.ProfileId));
			Neutrals.Remove(person);
			Allies.Remove(person);
		}
		BotGame.BotsController.Bots.GetConnector().Remove(person);
		foreach (BotOwner member3 in Members)
		{
			member3.Memory.AddEnemy(person, value, onActivation: false);
		}
		method_16();
		return true;
	}

	public void method_0(IPlayer person, BotSettingsClass value)
	{
		Enemies.Add(person, value);
		person.OnIPlayerDeadOrUnspawn += delegate(IPlayer obj)
		{
			// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
			RemoveEnemy(obj, EBotEnemyCause.death);
			obj.OnIPlayerDeadOrUnspawn -= method_18;
		};
	}

	public void PlaceChecked(PlaceForCheck currentPlaceForCheck)
	{
		method_14(currentPlaceForCheck.Position, currentPlaceForCheck);
	}

	public void RemoveEnemy(IPlayer player, EBotEnemyCause cause = EBotEnemyCause.initial)
	{
		Enemies.Remove(player);
		this.OnEnemyRemove?.Invoke(player);
		Neutrals.Remove(player);
		IHealthController healthController = player.HealthController;
		if (healthController != null && !healthController.IsAlive)
		{
			DeadBodiesController.AddBody(player);
		}
		foreach (BotOwner member in Members)
		{
			member.Memory.DeleteInfoAboutEnemy(player);
		}
	}

	public void DeletePlayerCauseDead(IPlayer player)
	{
		RemoveEnemy(player, EBotEnemyCause.death);
		method_16();
	}

	public void PointChecked(PlaceForCheck place)
	{
		method_15(place);
		PlacesForCheck.Remove(place);
	}

	public void LoseVision(IPlayer target)
	{
		if (Enemies.TryGetValue(target, out var value))
		{
			value.IsLastPositionChecked = false;
			value.EnemyLastPosition = target.Transform.position;
			CurrentEnemies.LoseVision();
		}
	}

	public void GetVision()
	{
		method_9();
	}

	public string MessageInfoWide()
	{
		string text = "";
		foreach (BotOwner member in Members)
		{
			text = text + member.Memory.MessageInfo(withN: false) + "; \n";
		}
		return text;
	}

	public string MessageInfo()
	{
		return $"Initial:{InitialBotType} name:{Name} M:{Members.Count}  E:{Enemies.Count}   Side:{Side} ";
	}

	public bool CheckAndAddEnemy(IPlayer player, bool ignoreAI = false)
	{
		if (!player.HealthController.IsAlive)
		{
			return false;
		}
		if (player.AIData.IsAI && InitialBot.Settings.GetAlwaysFriendlyBotTypes().Contains(player.AIData.BotOwner.Profile.Info.Settings.Role))
		{
			return false;
		}
		if (!player.AIData.IsAI && InitialBot.Settings.IsPlayerAlwaysFriends(player))
		{
			return false;
		}
		if (player.AIData.IsAI)
		{
			if (InitialBot.Id == player.AIData.BotOwner.Id)
			{
				return false;
			}
			if (!ignoreAI)
			{
				return false;
			}
		}
		if (Enemies.ContainsKey(player))
		{
			return false;
		}
		return AddEnemy(player, EBotEnemyCause.checkAddTODO);
	}

	public void AddNeutral(IPlayer player)
	{
		if (!Neutrals.ContainsKey(player))
		{
			Neutrals.Add(player, new BotSettingsClass(Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(player.ProfileId), this));
			this.OnAddNeutral?.Invoke(player);
		}
	}

	public bool IsEnemy(IPlayer player)
	{
		if (player == null)
		{
			return false;
		}
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in Enemies)
		{
			if (enemy.Value.Player != null && enemy.Value.Player.Id == player.Id)
			{
				return true;
			}
		}
		return false;
	}

	public void SetLastVisionEnemyTimeSence(float enemyLastSeenTime, Vector3 pos)
	{
		EnemyLastSeenTimeSence = enemyLastSeenTime;
		EnemyLastSeenPositionSence = pos;
	}

	public void SetLastVisionEnemyTimeReal(float enemyLastSeenTime, Vector3 pos)
	{
		EnemyLastSeenTimeReal = enemyLastSeenTime;
		EnemyLastSeenPositionReal = pos;
	}

	public void BossAppear(BotOwner botBoss)
	{
		if (BossGroup == null)
		{
			BossGroup = new BossGroup(botBoss);
			this.OnBossSetted?.Invoke(botBoss);
		}
	}

	public bool ContainsEnemy(BotOwner bot)
	{
		return Enemies.ContainsKey(bot);
	}

	public bool IsSuitable(WildSpawnType tested)
	{
		return BotSettingsRepoClass.IsSuitable(InitialBot.Profile.Info.Settings.Role, tested);
	}

	public void SetEnemyPos(IPlayer enemy, Vector3 enemyPos, Vector3 weaponRootLast, EEnemyPartVisibleType isVisibleOnlyBySense)
	{
		if (Enemies.TryGetValue(enemy, out var value))
		{
			value.EnemyWeaponRootLastPos = weaponRootLast;
			value.EnemyLastPosition = enemyPos;
			if (isVisibleOnlyBySense == EEnemyPartVisibleType.Visible)
			{
				CurrentEnemies.GetVision();
				value.EnemyLastVisiblePosition = enemyPos;
			}
		}
	}

	public BotOwner ClosestMemberToPoint(Vector3 pos)
	{
		if (MembersCount == 0)
		{
			return null;
		}
		BotOwner result = null;
		float num = float.MaxValue;
		for (int i = 0; i < MembersCount; i++)
		{
			BotOwner botOwner = Members[i];
			float sqrMagnitude = (botOwner.Position - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = botOwner;
			}
		}
		return result;
	}

	public bool method_1(IPlayer player)
	{
		if (player.Profile.Info.Side != EPlayerSide.Usec)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(player.GroupId) && BotSettingsRepoClass.IsExUsec(InitialBot.Profile.Info.Settings.Role))
		{
			List<Player> allAlivePlayersList = Singleton<GameWorld>.Instance.AllAlivePlayersList;
			for (int i = 0; i < allAlivePlayersList.Count; i++)
			{
				Player player2 = allAlivePlayersList[i];
				if (player2.GroupId == player.GroupId && player2.Side != EPlayerSide.Usec)
				{
					return true;
				}
			}
		}
		return player.Profile.EftStats.OverallCounters.GetInt(CounterTag.UsecRaidRemainKills) > 0;
	}

	public void method_2(BotOwner member)
	{
		Members.Add(member);
		if (TargetMembersCount > 0 && !IsFull)
		{
			IsFull = Members.Count >= TargetMembersCount;
		}
		if (!IsFirstMemberAdded)
		{
			IsFirstMemberAdded = true;
			foreach (IPlayer item in RecheckPersonsAfterInit)
			{
				AddEnemy(item, EBotEnemyCause.AddNewMember);
			}
		}
		this.OnMemberAdd?.Invoke(member);
	}

	public void AddMember(BotOwner ally, bool onActivation)
	{
		if (Members.Contains(ally) || ally == null || Allies.Contains(ally) || ally.BotState != EBotState.Active)
		{
			return;
		}
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in Enemies)
		{
			ally.Memory.AddEnemy(enemy.Key, enemy.Value, onActivation);
		}
		if (ally.BotsGroup.BotZone == BotZone)
		{
			method_2(ally);
		}
		method_16();
	}

	public void method_3()
	{
		GroupMarkOfUnknown.ManualUpdate();
		GroupDangerAreas.ManualUpdate();
		method_8();
	}

	public void method_4()
	{
		float num = 0f;
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in Enemies)
		{
			if (enemy.Value.IsHaveSeen && enemy.Value.EnemyLastSeenTimeSense > num)
			{
				num = enemy.Value.EnemyLastSeenTimeSense;
			}
		}
		for (int i = 0; i < PlacesForCheck.Count; i++)
		{
			PlaceForCheck placeForCheck = PlacesForCheck[i];
			if (placeForCheck.CreatedTime > num)
			{
				num = placeForCheck.CreatedTime;
			}
		}
		IsLastPositionOld = method_5(num);
	}

	public bool method_5(float newestPosition)
	{
		float num = Time.time - newestPosition;
		if (num < 0f)
		{
			return true;
		}
		return num > LocalBotSettingsProviderClass.Core.LAST_SEEN_POS_LIFETIME;
	}

	public bool method_6(Vector3 pos, PlaceForCheck place2check)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		if (BotGame.BotsController.Bots.Count > 20)
		{
			if (NextCheckDestination < Time.time)
			{
				NextCheckDestination = Time.time + 1f;
				NavMesh.CalculatePath(pos, place2check.Position, -1, navMeshPath);
			}
		}
		else
		{
			NavMesh.CalculatePath(pos, place2check.Position, -1, navMeshPath);
		}
		if (place2check.Reacheble && navMeshPath.status == NavMeshPathStatus.PathComplete)
		{
			place2check.Reacheble = true;
			return true;
		}
		place2check.Reacheble = false;
		if (Mathf.Abs(pos.y - place2check.BasePoint.y) > LocalBotSettingsProviderClass.Core.VERTICAL_DIST_TO_IGNORE_SOUND)
		{
			return false;
		}
		return true;
	}

	public bool method_7(PlaceForCheck place2check)
	{
		Vector3 pos = default(Vector3);
		ISpawnPoint spawnPoint = BotZone.SpawnPoints.FirstOrDefault();
		if (spawnPoint != null)
		{
			pos = spawnPoint.Position;
		}
		return method_6(pos, place2check);
	}

	public void method_8()
	{
		if (NextGetGoalTime < Time.time)
		{
			method_9();
		}
	}

	public void method_9()
	{
		NextGetGoalTime = LocalBotSettingsProviderClass.Core.UPDATE_GOAL_TIMER_SEC + Time.time;
		method_10();
		method_12();
		method_4();
	}

	public void method_10()
	{
		Dictionary<IPlayer, List<BotOwner>> dictionary = new Dictionary<IPlayer, List<BotOwner>>();
		Dictionary<IPlayer, List<EnemyInfo>> dictionary2 = new Dictionary<IPlayer, List<EnemyInfo>>();
		for (int i = 0; i < Members.Count; i++)
		{
			BotOwner botOwner = Members[i];
			if (botOwner.Memory.GoalEnemy != null)
			{
				IPlayer person = botOwner.Memory.GoalEnemy.Person;
				if (!dictionary.TryGetValue(person, out var value))
				{
					value = new List<BotOwner>();
					dictionary.Add(person, value);
				}
				if (!dictionary2.TryGetValue(person, out var value2))
				{
					value2 = new List<EnemyInfo>();
					dictionary2.Add(person, value2);
				}
				value.Add(botOwner);
				value2.Add(botOwner.Memory.GoalEnemy);
			}
		}
		int num = Mathf.Clamp((int)((float)Members.Count * LocalBotSettingsProviderClass.Core.PERCENT_PERSON_SEARCH) + 1, LocalBotSettingsProviderClass.Core.MIN_MAX_PERSON_SEARCH, 100);
		bool flag = true;
		foreach (KeyValuePair<IPlayer, List<BotOwner>> item in dictionary)
		{
			EnemyInfo[] array = dictionary2[item.Key].OrderBy((EnemyInfo info) => (info.EnemyLastPosition - info.Owner.Transform.position).sqrMagnitude).ToArray();
			int num2 = 0;
			for (int num3 = 0; num3 < array.Length; num3++)
			{
				EnemyInfo enemyInfo = array[num3];
				bool flag2 = num2 < num;
				if (flag)
				{
					flag2 = flag2 && enemyInfo.Owner.Tactic.IsCurTactic(BotCurrentTactic.Attack);
				}
				if (flag2)
				{
					num2++;
				}
				enemyInfo.CanISearch = flag2;
				if (flag2)
				{
					enemyInfo.SearchIndex = num3;
				}
				else
				{
					enemyInfo.SearchIndex = -1;
				}
			}
		}
	}

	public void method_11(PlaceForCheck placeForCheck)
	{
		PlacesForCheck.Add(placeForCheck);
		if (PlacesForCheck.Count != 1)
		{
			return;
		}
		foreach (BotOwner member in Members)
		{
			CalcGoalForBot(member);
		}
	}

	public void method_12()
	{
		PlacesForCheck.RemoveAll((PlaceForCheck placeForCheck) => placeForCheck.IsOld());
	}

	public PlaceForCheck method_13(BotOwner owner, Vector3 suspectedPoint, PlaceForCheckType type, bool canUseCovrPoints = true)
	{
		PlaceForCheck placeForCheck = null;
		if (canUseCovrPoints)
		{
			CustomNavigationPoint closestPoint = owner.Covers.GetClosestPoint(suspectedPoint);
			if (closestPoint == null)
			{
				placeForCheck = new PlaceForCheck(suspectedPoint, type);
				placeForCheck.Reacheble = false;
				return placeForCheck;
			}
			Vector3 direction = closestPoint.Position - suspectedPoint;
			float magnitude = direction.magnitude;
			bool flag;
			if (flag = magnitude < LocalBotSettingsProviderClass.Core.COVER_DIST_CLOSE)
			{
				flag = !Physics.Raycast(new Ray(suspectedPoint, direction), magnitude);
			}
			if (flag)
			{
				placeForCheck = new PlaceForCheck(closestPoint, suspectedPoint, type);
			}
		}
		if (placeForCheck == null)
		{
			placeForCheck = new PlaceForCheck(suspectedPoint, type);
		}
		return placeForCheck;
	}

	public List<PlaceForCheck> method_14(Vector3 pos, PlaceForCheck pfc = null)
	{
		List<PlaceForCheck> list = new List<PlaceForCheck>();
		if (pfc != null)
		{
			list.Add(pfc);
		}
		foreach (BotOwner member in Members)
		{
			member.Memory.RemovePlaceForCheck(list);
		}
		return list;
	}

	public List<BotSettingsClass> GetLastSeenPositions()
	{
		return (from x in Enemies
			where !x.Value.IsLastPositionChecked && x.Key.HealthController.IsAlive
			select x.Value).ToList();
	}

	public void method_15(PlaceForCheck place)
	{
		PlacesForCheck.RemoveAll((PlaceForCheck check) => place.IsThisPointClose(check.BasePoint));
		foreach (KeyValuePair<IPlayer, BotSettingsClass> enemy in Enemies)
		{
			if (!enemy.Value.IsLastPositionChecked && place.IsThisPointClose(enemy.Value.EnemyLastPosition))
			{
				enemy.Value.IsLastPositionChecked = true;
			}
		}
	}

	public void method_16()
	{
		if (LocalBotSettingsProviderClass.Core.MAIN_TACTIC_ONLY_ATTACK)
		{
			foreach (BotOwner member in Members)
			{
				method_17(member, BotCurrentTactic.Attack);
			}
			return;
		}
		float num = Members.Sum((BotOwner x) => x.Tactic.AggressionCoef) / (float)Members.Count;
		BotCurrentTactic globalCurrentTactic = ((GroupPower * num < Single_0) ? BotCurrentTactic.Ambush : BotCurrentTactic.Attack);
		foreach (BotOwner member2 in Members)
		{
			method_17(member2, globalCurrentTactic);
		}
	}

	public void method_17(BotOwner botOwner, BotCurrentTactic globalCurrentTactic)
	{
		if (!botOwner.BotFollower.HaveBoss)
		{
			botOwner.Tactic.SetTactic(globalCurrentTactic);
		}
	}

	public void Dispose()
	{
		CurrentEnemies.Dispose();
		foreach (BotOwner member in Members)
		{
			member.Dispose();
		}
		BossGroup?.Dispose();
		GroupMarkOfUnknown.Dispose();
		CoverPointMaster.Dispose();
		PlacesForCheck.Clear();
		Enemies.Clear();
		Allies.Clear();
		Neutrals.Clear();
		Members.Clear();
		GrenadeSmokePlaces.Clear();
		StaticManager.Instance.StaticUpdate -= method_3;
	}

	public bool ShallRevengeFor(IPlayer target)
	{
		if (target.Loyalty.CanBeFreeKilled)
		{
			return false;
		}
		if (!target.IsAI)
		{
			if (target.Profile.Side == EPlayerSide.Savage)
			{
				return InitialBotMindSettings_1.REVENGE_FOR_SAVAGE_PLAYERS;
			}
			return false;
		}
		return InitialBotMindSettings_1.REVENGE_BOT_TYPES.Contains(target.AIData.BotOwner.Profile.Info.Settings.Role);
	}

	public bool HaveBoss(out BotOwner botBoss)
	{
		if (BossGroup == null)
		{
			botBoss = null;
			return false;
		}
		botBoss = BossGroup.Boss;
		return botBoss != null;
	}

	public bool HaveFollowTarget(IPlayer aggressor)
	{
		foreach (BotOwner member in Members)
		{
			if (member.PlayerFollowData.IsFollower(aggressor))
			{
				return true;
			}
		}
		return false;
	}

	public BotOwner GetRandomMember()
	{
		return GClass856.RandomElement(Members);
	}

	[CompilerGenerated]
	public void method_18(IPlayer obj)
	{
		// Found self-referencing delegate construction. Abort transformation to avoid stack overflow.
		RemoveEnemy(obj, EBotEnemyCause.death);
		obj.OnIPlayerDeadOrUnspawn -= method_18;
	}
}
