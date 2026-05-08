using EFT;
using UnityEngine;

public struct BotDataStruct
{
	public bool Inited;

	public string Name;

	public EBotState Status;

	public int MainTactic;

	public string StrategyName;

	public int Ammo;

	public string LookDirType;

	public string Target;

	public float VisibleDist;

	public float GreenDist;

	public int HourServer;

	public bool InCover;

	public byte GreenType;

	public int CoverIndex;

	public bool UnderFire;

	public bool TriangleLoad;

	public bool IsInSpawnWeapon;

	public bool Reloading;

	public bool HaveAxeEnemy;

	public bool IsProblem;

	public int Request;

	public int BotMoverCurrentState;

	public Vector3 DoorOpenerCurrentDoorPosition;

	public bool Shooting;

	public bool HaveGrenade;

	public bool IsFlashed;

	public bool IsLay;

	public bool IsInPronePos;

	public bool IsParametesChanged;

	public bool IsGoalEnemyVisible;

	public bool CanLookThoughtGrass;

	public bool CanShoot;

	public bool InBuildingTarget;

	public bool InBuildingMe;

	public float PoseLevel;

	public float BaseVisibilityChangeSpeed;

	public float AccuratySpeed;

	public float ScatteringPerMeter;

	public float RuntimeVisionEffectK;

	public float RuntimeVisionEffectKCur;

	public float ScatteringPerMeterCur;

	public bool ShallRunIfNoAmmo;

	public string LayerName;

	public int NodeRepeats;

	public string NodeName;

	public string PrevNodeName;

	public string Reason;

	public string PrevNodeExitReason;

	public string CustomData;

	public int BotRole;

	public int BotDifficulty;

	public bool HasBossToFollow;

	public BotStandByType StandBy;

	public bool UseMeds;

	public bool Damaged;

	public bool HaveMeds;

	public bool HaveStimulator;

	public bool UseStimulator;

	public bool HaveSurgical;

	public bool UseSurgical;

	public int HitsOnMe;

	public int ShootsOnMe;

	public int Id;

	public bool PatrolPointStatus;

	public bool PatrolWayStatus;

	public string FightType;

	public bool CanShootByState;

	public EPlayerState PlayerState;

	public Vector3 TargetPoint;

	public Vector3 PositionOnWay;

	public Vector3[] WayPoints;

	public int WayPointsLength;

	public bool HaveUnderbarrelLauncher;

	public bool UnderbarrelLauncherActive;

	public bool UnderbarrelLauncherHasAmmoInChamber;

	public string GroupId;
}
