using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using EFT;
using EFT.EnvironmentEffect;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

public class GClass591 : IAIData
{
	[NonSerialized]
	public GClass590 Gclass590_0 = new GClass590();

	[NonSerialized]
	public HashSet<AIPlaceInfo> HashSet_0 = new HashSet<AIPlaceInfo>();

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_0;

	[NonSerialized]
	[CompilerGenerated]
	public BotOwner BotOwner_0;

	[NonSerialized]
	[CompilerGenerated]
	public bool Bool_1;

	[NonSerialized]
	[CompilerGenerated]
	public DateTime DateTime_0;

	[CompilerGenerated]
	private Action<PlayerAIDataClass> action_0;

	[CompilerGenerated]
	private Action<PlayerAIDataClass> action_1;

	[CompilerGenerated]
	private Action<AIPlaceInfo> action_2;

	[CompilerGenerated]
	private Action<AIPlaceInfo> action_3;

	public IAIDataRoomLogic roomLogicLogic => Gclass590_0;

	public HashSet<AIPlaceInfo> CurrentZones => HashSet_0;

	public bool IsAI => false;

	public bool UseZombieSimpleAnimator
	{
		[CompilerGenerated]
		get
		{
			return Bool_0;
		}
	}

	public BotOwner BotOwner
	{
		[CompilerGenerated]
		get
		{
			return BotOwner_0;
		}
		[CompilerGenerated]
		set
		{
			BotOwner_0 = value;
		}
	}

	public AIDataRequestController AskRequests => null;

	public int EnvironmentId => -1;

	public AIPlaceInfo PlaceInfo => null;

	public bool HaveHelmet => false;

	public bool IsAxe => false;

	public bool ShallPursuit => false;

	public bool IsInTree => false;

	public float FoliageIntersectionPercent => 0f;

	public Transform CurrentFoliageRoot => null;

	public float LastShotTime => 0f;

	public float PowerOfEquipment => 0f;

	public bool IsScavAttacker => false;

	public bool IAmBoss => false;

	public bool IsNoOffsetShooting => false;

	public bool PlayerLoyaltyIsAggressor => false;

	public bool PlayerLoyaltyBossNoAttack => false;

	public bool PlayerLoyaltyCanBeFreeKilled => false;

	public string PlayerProfileNickname => "";

	public Player Player => null;

	public float FlarePower => 0f;

	public bool IsInside => false;

	public bool UsingLight => false;

	public bool GetFlare => false;

	public float PoseVisibilityCoef => 1f;

	public AIBossPlayer AIBossPlayer => null;

	public float LastResetPlaceInfo => 0f;

	public bool IsDrunk
	{
		[CompilerGenerated]
		get
		{
			return Bool_1;
		}
	}

	public DateTime DrinkTimestamp
	{
		[CompilerGenerated]
		get
		{
			return DateTime_0;
		}
	}

	public bool IsMute => false;

	public event Action<PlayerAIDataClass> OnBecomeScavAttacker
	{
		[CompilerGenerated]
		add
		{
			Action<PlayerAIDataClass> action = action_0;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<PlayerAIDataClass> action = action_0;
			Action<PlayerAIDataClass> action2;
			do
			{
				action2 = action;
				Action<PlayerAIDataClass> value2 = (Action<PlayerAIDataClass>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_0, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<PlayerAIDataClass> OnBecomeDrunk
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

	public event Action<AIPlaceInfo> OnComeToPlace
	{
		[CompilerGenerated]
		add
		{
			Action<AIPlaceInfo> action = action_2;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<AIPlaceInfo> action = action_2;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_2, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public event Action<AIPlaceInfo> OnLeavePlace
	{
		[CompilerGenerated]
		add
		{
			Action<AIPlaceInfo> action = action_3;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Combine(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
		[CompilerGenerated]
		remove
		{
			Action<AIPlaceInfo> action = action_3;
			Action<AIPlaceInfo> action2;
			do
			{
				action2 = action;
				Action<AIPlaceInfo> value2 = (Action<AIPlaceInfo>)Delegate.Remove(action2, value);
				action = Interlocked.CompareExchange(ref action_3, value2, action2);
			}
			while ((object)action != action2);
		}
	}

	public void Activate()
	{
	}

	public void KillEnemy(Player player)
	{
	}

	public void TryPlayShootSound(Player getPlayer, Vector3 position, AISoundType spredPower)
	{
	}

	public void SetStationaryWeapon(Weapon stationaryWeaponItem)
	{
	}

	public void TacticalModeChange(bool p0)
	{
	}

	public void SetEnvironment(IndoorTrigger trigger)
	{
	}

	public void StayInTree(TreeInteractive treeInteractive, Collider enteredCollider, Collider triggerCollider)
	{
	}

	public void ExitTree(TreeInteractive treeInteractive)
	{
	}

	public bool IsBossOrFollowerRequireRevenge()
	{
		return false;
	}

	public void LeavePlace(AIPlaceInfo aiPlaceInfo)
	{
	}

	public void AddPlaceInfo(AIPlaceInfo aiPlaceInfo)
	{
	}

	public void Dispose()
	{
	}

	public void BecomeDrunk()
	{
	}

	public void SetAttackByLoyalityScav()
	{
	}

	public void DrawGizmosSelectedVoxel()
	{
	}

	public void CalcPower()
	{
	}

	public void EnterTree(TreeInteractive treeInteractive)
	{
	}

	public void SetPosToVoxel(Vector3 ownerPosition)
	{
	}

	public void LateUpdate()
	{
	}

	public void DrawGizmosFoliageIntersection()
	{
	}
}
