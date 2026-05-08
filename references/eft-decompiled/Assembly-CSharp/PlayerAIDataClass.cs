using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using EFT.InventoryLogic;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerAIDataClass : IAIData
{
	public const float TILT_VALUE = 5f;

	[NonSerialized]
	public float Float_0;

	[NonSerialized]
	public float Float_1;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public MovementContext MovementContext_0;

	[NonSerialized]
	public BotOwner BotOwner_0;

	[NonSerialized]
	public TreeInteractive TreeInteractive_0;

	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public GClass586 Gclass586_0 = new GClass586();

	[NonSerialized]
	public float Float_3;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public HashSet<AIPlaceInfo> HashSet_0 = new HashSet<AIPlaceInfo>();

	[NonSerialized]
	public Vector3 Vector3_0;

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public GInterface12 Ginterface12_0;

	[NonSerialized]
	public bool Bool_1;

	[NonSerialized]
	public bool Bool_2;

	[NonSerialized]
	public float Float_6 = -9999f;

	[NonSerialized]
	public const float Float_7 = 25f;

	[NonSerialized]
	public float Float_8;

	[CompilerGenerated]
	private Action<Weapon> action_0;

	[CompilerGenerated]
	private Action<PlayerAIDataClass> action_1;

	[CompilerGenerated]
	private Action<PlayerAIDataClass> action_2;

	[CompilerGenerated]
	private Action<PlayerAIDataClass> action_3;

	[CompilerGenerated]
	private Action<AIPlaceInfo> action_4;

	[CompilerGenerated]
	private Action<AIPlaceInfo> action_5;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_3;

	[NonSerialized]
	[CompilerGenerated]
	public int Int_1;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_9;

	[NonSerialized]
	[CompilerGenerated]
	public DateTime DateTime_0;

	[NonSerialized]
	public int Int_2;

	[NonSerialized]
	public const int Int_3 = 8;

	[NonSerialized]
	public const int Int_4 = 2;

	[NonSerialized]
	public const string String_0 = "5d40407c86f774318526545a";

	[NonSerialized]
	public const string String_1 = "5d403f9186f7743cac3f229b";

	[NonSerialized]
	public const string String_2 = "5d1b376e86f774252519444e";

	[NonSerialized]
	[CompilerGenerated]
	public AIPlaceInfo AiplaceInfo_0;

	[NonSerialized]
	[CompilerGenerated]
	public AIDataRequestController AidataRequestController_0;

	[NonSerialized]
	[CompilerGenerated]
	public AIBossPlayer AibossPlayer_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_4;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_5;

	[NonSerialized]
	[CompilerGenerated]
	public Player Player_0;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_10;

	[NonSerialized]
	[CompilerGenerated]
	public float Float_11;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_6;

	[NonSerialized]
	public bool Bool_7;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_8;

	[NonSerialized]
	public GClass1679 Gclass1679_0;

	public HashSet<AIPlaceInfo> CurrentZones => HashSet_0;

	public IAIDataRoomLogic roomLogicLogic => Gclass586_0;

	public bool ShallPursuit
	{
		get
		{
			if (IsAxe && !method_7())
			{
				return true;
			}
			return IsScavAttacker;
		}
	}

	public bool IsAxe
	{
		[CompilerGenerated]
		get
		{
			return Bool_3;
		}
		[CompilerGenerated]
		set
		{
			Bool_3 = value;
		}
	}

	public bool IsInside => EnvironmentId > 0;

	public bool IsScavAttacker => Float_8 > Time.time;

	public int EnvironmentId
	{
		[CompilerGenerated]
		get
		{
			return Int_1;
		}
		[CompilerGenerated]
		set
		{
			Int_1 = value;
		}
	}

	public float LastShotTime
	{
		[CompilerGenerated]
		get
		{
			return Float_9;
		}
		[CompilerGenerated]
		set
		{
			Float_9 = value;
		}
	}

	public DateTime DrinkTimestamp
	{
		[CompilerGenerated]
		get
		{
			return DateTime_0;
		}
		[CompilerGenerated]
		set
		{
			DateTime_0 = value;
		}
	}

	public bool IsMute
	{
		get
		{
			if (PlaceInfo != null)
			{
				return PlaceInfo.IsMute;
			}
			return false;
		}
	}

	public AIPlaceInfo PlaceInfo
	{
		[CompilerGenerated]
		get
		{
			return AiplaceInfo_0;
		}
		[CompilerGenerated]
		set
		{
			AiplaceInfo_0 = value;
		}
	}

	public AIDataRequestController AskRequests
	{
		[CompilerGenerated]
		get
		{
			return AidataRequestController_0;
		}
	}

	public AIBossPlayer AIBossPlayer
	{
		[CompilerGenerated]
		get
		{
			return AibossPlayer_0;
		}
	}

	public bool GetFlare
	{
		[CompilerGenerated]
		get
		{
			return Bool_4;
		}
		[CompilerGenerated]
		set
		{
			Bool_4 = value;
		}
	}

	public bool IAmBoss => AIBossPlayer.IAmBoos;

	public bool UsingLight
	{
		[CompilerGenerated]
		get
		{
			return Bool_5;
		}
		[CompilerGenerated]
		set
		{
			Bool_5 = value;
		}
	}

	public Player Player
	{
		[CompilerGenerated]
		get
		{
			return Player_0;
		}
	}

	public float PowerOfEquipment
	{
		[CompilerGenerated]
		get
		{
			return Float_10;
		}
		[CompilerGenerated]
		set
		{
			Float_10 = value;
		}
	}

	public float LastResetPlaceInfo
	{
		[CompilerGenerated]
		get
		{
			return Float_11;
		}
		[CompilerGenerated]
		set
		{
			Float_11 = value;
		}
	}

	public bool IsDrunk => Bool_2;

	public float FlarePower
	{
		get
		{
			if (GetFlare)
			{
				if (Float_1 < Time.time)
				{
					GetFlare = false;
				}
				return LocalBotSettingsProviderClass.Core.FLARE_POWER;
			}
			if (Bool_0)
			{
				if (Float_2 < Time.time)
				{
					Bool_0 = false;
				}
				return LocalBotSettingsProviderClass.Core.MOVE_COEF;
			}
			return 1f;
		}
	}

	public bool IsAI
	{
		[CompilerGenerated]
		get
		{
			return Bool_6;
		}
		[CompilerGenerated]
		set
		{
			Bool_6 = value;
		}
	}

	public bool UseZombieSimpleAnimator => BotOwner_0.Profile.Info.Settings.UseSimpleAnimator;

	public BotOwner BotOwner
	{
		get
		{
			return BotOwner_0;
		}
		set
		{
			BotOwner_0 = value;
			IsAI = BotOwner_0 != null;
			if (IsAI)
			{
				Ginterface12_0.Dispose();
				Ginterface12_0 = new GClass588(BotOwner_0);
				IsNoOffsetShooting = BotOwner_0.Settings.FileSettings.Shoot.NO_OFFSET_SHOOTING_FROM_PLAYER;
			}
			Player.MovementContext.OnPoseChanged += method_0;
			if (Player.CharacterController.GetCollider() is CapsuleCollider ownerCollider)
			{
				Gclass1679_0.SetOwnerCollider(ownerCollider);
				Gclass1679_0.RecalculateLocalPoints();
			}
			IsNoOffsetShooting = true;
		}
	}

	public float PoseVisibilityCoef
	{
		get
		{
			if (!MovementContext_0.IsInPronePose)
			{
				return Mathf.Lerp(LocalBotSettingsProviderClass.Core.LOWER_POSE, LocalBotSettingsProviderClass.Core.MAX_POSE, MovementContext_0.PoseLevel);
			}
			return LocalBotSettingsProviderClass.Core.PRONE_POSE;
		}
	}

	public bool Boolean_0
	{
		set
		{
			Float_1 = Time.time + LocalBotSettingsProviderClass.Core.FLARE_TIME;
			GetFlare = value;
		}
	}

	public bool HaveHelmet => Bool_7;

	public bool IsInTree => TreeInteractive_0 != null;

	public float FoliageIntersectionPercent => Gclass1679_0.FoliageIntersectionPercent;

	public Transform CurrentFoliageRoot => Gclass1679_0.CurrentFoliageRoot;

	public bool IsNoOffsetShooting
	{
		[CompilerGenerated]
		get
		{
			return Bool_8;
		}
		[CompilerGenerated]
		set
		{
			Bool_8 = value;
		}
	}

	public bool PlayerLoyaltyIsAggressor => Player.Loyalty.IsAggressor;

	public bool PlayerLoyaltyBossNoAttack => Player.Loyalty.BossNoAttack;

	public bool PlayerLoyaltyCanBeFreeKilled => Player.Loyalty.CanBeFreeKilled;

	public string PlayerProfileNickname => Player.Profile.Nickname;

	public event Action<Weapon> OnStationaryTaken
	{
		[CompilerGenerated]
		add
		{
			Action<Weapon> action = action_0;
			Action<Weapon> action2;
			do
			{
				action2 = action;
				Action<Weapon> value2 = (Action<Weapon>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<Weapon> action = action_0;
			Action<Weapon> action2;
			do
			{
				action2 = action;
				Action<Weapon> value2 = (Action<Weapon>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<PlayerAIDataClass> OnBecomeScavAttacker
	{
		[CompilerGenerated]
		add
		{
			Action<PlayerAIDataClass> action = action_1;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PlayerAIDataClass> action = action_1;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_1, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<PlayerAIDataClass> OnBecomeDrunk
	{
		[CompilerGenerated]
		add
		{
			Action<PlayerAIDataClass> action = action_2;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PlayerAIDataClass> action = action_2;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<PlayerAIDataClass> OnTiltDone
	{
		[CompilerGenerated]
		add
		{
			Action<PlayerAIDataClass> action = action_3;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PlayerAIDataClass> action = action_3;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<AIPlaceInfo> OnComeToPlace
	{
		[CompilerGenerated]
		add
		{
			Action<AIPlaceInfo> action = action_4;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<AIPlaceInfo> action = action_4;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_4, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<AIPlaceInfo> OnLeavePlace
	{
		[CompilerGenerated]
		add
		{
			Action<AIPlaceInfo> action = action_5;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<AIPlaceInfo> action = action_5;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_5, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public PlayerAIDataClass(BotOwner owner, Player player)
	{
		Gclass1679_0 = new GClass1679();
		AidataRequestController_0 = new AIDataRequestController(this);
		MovementContext_0 = player.MovementContext;
		Player_0 = player;
		BotOwner = owner;
		AibossPlayer_0 = new AIBossPlayer(player);
		IsAxe = method_8(Player);
		MovementContext_0.OnTiltChanged += method_4;
		MovementContext_0.OnMotionApplied += method_2;
		player.HealthController.EffectStartedEvent += method_1;
		GInterface12 ginterface12_;
		if (!IsAI)
		{
			GInterface12 gInterface = new GClass589();
			ginterface12_ = gInterface;
		}
		else
		{
			GInterface12 gInterface = new GClass588(BotOwner);
			ginterface12_ = gInterface;
		}
		Ginterface12_0 = ginterface12_;
		LocationScene.GetAllObjects<BotZone>().ToList();
		player.MovementContext.OnTiltChanged += method_4;
		if (player.Profile.FenceInfo == null)
		{
			player.Profile.SetDefaultFenceInfo();
		}
		CalcPower();
	}

	public void method_0(int poseToInt)
	{
		Gclass1679_0.RecalculateLocalPoints();
	}

	public void method_1(IEffect obj)
	{
		if (obj is GInterface376 gInterface && (gInterface.MedItem.TemplateId.Equals("5d40407c86f774318526545a") || gInterface.MedItem.TemplateId.Equals("5d403f9186f7743cac3f229b") || gInterface.MedItem.TemplateId.Equals("5d1b376e86f774252519444e")))
		{
			Int_2++;
			DrinkTimestamp = EFTDateTimeClass.UtcNow;
			int num = ((Player.Profile.Side == EPlayerSide.Savage) ? 2 : 8);
			if (Int_2 == num)
			{
				BecomeDrunk();
			}
		}
	}

	public bool IsBossOrFollowerRequireRevenge()
	{
		if (IsAI)
		{
			if ((!BotOwner_0.SpawnProfileData.IsBossOrFollower() || BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.pmcBot || BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.exUsec) && BotOwner_0.Profile.Info.Settings.Role != WildSpawnType.assault)
			{
				return BotOwner_0.Profile.Info.Settings.Role == WildSpawnType.marksman;
			}
			return true;
		}
		return false;
	}

	public void KillEnemy(Player victim)
	{
		Int_0++;
		if (IsAxe && Int_0 > LocalBotSettingsProviderClass.Core.AXE_MAN_KILLS_END)
		{
			IsAxe = false;
		}
		Player.Loyalty.KillEnemy(victim);
	}

	public void DoMove()
	{
		Float_2 = Time.time + 0.5f;
		Bool_0 = true;
	}

	public void BecomeDrunk()
	{
		if (!Bool_2)
		{
			Bool_2 = true;
			action_2?.Invoke(this);
		}
	}

	public void SetAttackByLoyalityScav()
	{
		Float_8 = Time.time + LocalBotSettingsProviderClass.Core.SCAV_ATTACK_PERIOD;
		action_1?.Invoke(this);
	}

	public void LeavePlace([NotNull] AIPlaceInfo place)
	{
		HashSet_0.Remove(place);
		method_6();
		action_5?.Invoke(place);
	}

	public void Activate()
	{
		method_3();
	}

	public void AddPlaceInfo([NotNull] AIPlaceInfo place)
	{
		HashSet_0.Add(place);
		method_6();
		action_4?.Invoke(place);
	}

	public void CalcPower()
	{
		if (Player.HandsController == null)
		{
			return;
		}
		Weapon weapon = Player.HandsController.Item as Weapon;
		float num = 0f;
		if (weapon != null)
		{
			if (!(weapon is ShotgunItemClass) && !(weapon is RevolverItemClass))
			{
				if (!(weapon is AssaultRifleItemClass) && !(weapon is AssaultCarbineItemClass))
				{
					if (weapon is PistolItemClass)
					{
						num = LocalBotSettingsProviderClass.Core.PISTOL_POWER;
					}
					else if (weapon is SmgItemClass)
					{
						num = LocalBotSettingsProviderClass.Core.SMG_POWER;
					}
					else if (weapon is MarksmanRifleItemClass)
					{
						num = LocalBotSettingsProviderClass.Core.SNIPE_POWER;
					}
				}
				else
				{
					num = LocalBotSettingsProviderClass.Core.RIFLE_POWER;
				}
			}
			else
			{
				num = LocalBotSettingsProviderClass.Core.SHOTGUN_POWER;
			}
		}
		Slot slot = Player.InventoryController.Inventory.Equipment.GetSlot(EquipmentSlot.Headwear);
		if (slot.ContainedItem != null)
		{
			Bool_7 = GClass3380.GetItemComponentsInChildren<ArmorComponent>(slot.ContainedItem).Any();
		}
		float num2 = method_9();
		num2 *= LocalBotSettingsProviderClass.Core.ARMOR_CLASS_COEF;
		if (DebugBotData.UseDebugData && DebugBotData.Instance.NoArmorToPower)
		{
			num2 = 0f;
		}
		PowerOfEquipment = num2 + num;
	}

	public void TryPlayShootSound(Player getPlayer, Vector3 position, AISoundType soundType)
	{
		LastShotTime = Time.time;
		float power = LocalBotSettingsProviderClass.Core.GUNSHOT_SPREAD;
		if (soundType == AISoundType.silencedGun)
		{
			power = LocalBotSettingsProviderClass.Core.GUNSHOT_SPREAD_SILENCE;
		}
		bool num = Float_0 < Time.time;
		Boolean_0 = true;
		if (num)
		{
			Float_0 = Time.time + 1f;
			if (Singleton<BotEventHandler>.Instantiated)
			{
				Singleton<BotEventHandler>.Instance.PlaySound(getPlayer, position, power, soundType);
			}
		}
	}

	public void TacticalModeChange(bool siActive)
	{
		UsingLight = siActive;
	}

	public void SetStationaryWeapon(Weapon stationaryWeaponItem)
	{
		if (action_0 != null)
		{
			action_0(stationaryWeaponItem);
		}
	}

	public void EnterTree(TreeInteractive treeInteractive)
	{
		TreeInteractive_0 = treeInteractive;
	}

	public void StayInTree(TreeInteractive treeInteractive, Collider enteredCollider, Collider triggerCollider)
	{
		TreeInteractive_0 = treeInteractive;
		Gclass1679_0.StayInTreeUpdate(enteredCollider, triggerCollider, IsAI);
	}

	public void LateUpdate()
	{
		Gclass1679_0.LateUpdate();
	}

	public void DrawGizmosFoliageIntersection()
	{
		Gclass1679_0.DrawGizmosFoliageIntersection(IsAI, Player);
	}

	public void ExitTree(TreeInteractive treeInteractive)
	{
		if (TreeInteractive_0 == treeInteractive)
		{
			TreeInteractive_0 = null;
		}
	}

	public void SetAim(bool value)
	{
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.HardAim(Player, value);
		}
	}

	public void SetPosToVoxel(Vector3 pos)
	{
		Ginterface12_0.SetPos(pos);
	}

	public void DrawGizmosSelectedVoxel()
	{
		try
		{
			Ginterface12_0.TryDrawAffectionGizmos();
		}
		catch (Exception)
		{
		}
	}

	public void method_2(CollisionFlags obj, Vector3 motion)
	{
		if (!IsAI && Float_5 < Time.time)
		{
			Float_5 = Time.time + 0.5f;
			Vector3 position = Player.Position;
			if ((Vector3_0 - position).sqrMagnitude > 0.7f)
			{
				Vector3_0 = position;
				method_3();
			}
		}
	}

	public void method_3()
	{
		Vector3 position = Player.Position;
		SetPosToVoxel(position);
	}

	public void method_4(float tiltSide)
	{
		float time = Time.time;
		if (Time.time - Float_3 < 1f)
		{
			if (Math.Abs(Mathf.Abs(tiltSide) - 5f) < 0.01f)
			{
				if (Float_4 != tiltSide)
				{
					method_5();
				}
				Float_4 = tiltSide;
				Float_3 = time;
			}
		}
		else if (Math.Abs(Mathf.Abs(tiltSide) - 5f) < 0.01f)
		{
			Float_4 = tiltSide;
			Float_3 = time;
		}
	}

	public void method_5()
	{
		if (Singleton<BotEventHandler>.Instantiated)
		{
			Singleton<BotEventHandler>.Instance.QETilt(Player);
		}
	}

	public void method_6()
	{
		AIPlaceInfo aIPlaceInfo = ((HashSet_0.Count != 0) ? HashSet_0.FirstOrDefault() : null);
		if (aIPlaceInfo == null)
		{
			if (IsAI)
			{
				BotOwner.BotLight.IsInDarkPlace = false;
			}
			PlaceInfo = null;
		}
		else
		{
			PlaceInfo = aIPlaceInfo;
			if (IsAI && PlaceInfo.IsDark)
			{
				BotOwner.BotLight.IsInDarkPlace = true;
			}
			Singleton<BotEventHandler>.Instance.PlayerComeToPlace(PlaceInfo?.Id);
		}
		LastResetPlaceInfo = Time.time;
	}

	public bool method_7()
	{
		if (Float_6 + 25f > Time.time)
		{
			return Bool_1;
		}
		string groupId = Player.GroupId;
		foreach (Player allAlivePlayers in Singleton<GameWorld>.Instance.AllAlivePlayersList)
		{
			if (!string.IsNullOrEmpty(groupId) && allAlivePlayers.GroupId == groupId && allAlivePlayers != Player && !method_8(allAlivePlayers))
			{
				Float_6 = Time.time;
				Bool_1 = true;
				return true;
			}
		}
		Float_6 = Time.time;
		Bool_1 = false;
		return false;
	}

	public bool method_8(Player player)
	{
		if (player.IsAI)
		{
			return false;
		}
		Inventory inventory = player.InventoryController.Inventory;
		IEnumerable<Item> itemsInSlots = inventory.GetItemsInSlots(new EquipmentSlot[2]
		{
			EquipmentSlot.FirstPrimaryWeapon,
			EquipmentSlot.SecondPrimaryWeapon
		});
		bool flag = false;
		foreach (Item item in itemsInSlots)
		{
			if (item != null)
			{
				flag = true;
				break;
			}
		}
		float num = method_9();
		IEnumerable<Item> itemsInSlots2 = inventory.GetItemsInSlots(new EquipmentSlot[2]
		{
			EquipmentSlot.Backpack,
			EquipmentSlot.TacticalVest
		});
		bool flag2 = false;
		foreach (Item item2 in itemsInSlots2)
		{
			if (item2 != null)
			{
				flag2 = true;
				break;
			}
		}
		if (!flag && num <= 0.1f)
		{
			return !flag2;
		}
		return false;
	}

	public float method_9()
	{
		IEnumerable<ArmorComponent> putOnArmors = Player.InventoryController.Inventory.GetPutOnArmors();
		float num = 0f;
		foreach (ArmorComponent item in putOnArmors)
		{
			num += (float)item.ArmorClass;
		}
		return num;
	}

	public void Dispose()
	{
		MovementContext_0.OnTiltChanged -= method_4;
		MovementContext_0.OnMotionApplied -= method_2;
		Player.HealthController.EffectStartedEvent -= method_1;
		Player.MovementContext.OnPoseChanged -= method_0;
		AskRequests.Dispose();
		AIBossPlayer.Dispose();
	}

	public void SetEnvironment(IndoorTrigger trigger)
	{
		if (trigger == null)
		{
			EnvironmentId = 0;
		}
		else
		{
			EnvironmentId = trigger.AreaAutoId;
		}
	}
}
