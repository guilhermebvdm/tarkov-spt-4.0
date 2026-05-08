using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

[Serializable]
public class CustomNavigationPoint : IPositionPoint, IAICorePointLink
{
	public const float MAX_DEFENCE_LEVEL_SIDE = 8f;

	public const int MAX_HIDE_VAL = 100;

	public bool CanIShootToEnemy;

	public bool lastCanShoot;

	[NonSerialized]
	public float StartBaseWeight = 1f;

	[NonSerialized]
	public float CoveringWeight_1;

	[NonSerialized]
	public float DecreasedWeightCoef = 1f;

	[NonSerialized]
	public float NextCheckCanShootTime;

	[NonSerialized]
	public GroupPoint GroupPoint_1;

	public GroupPoint GroupPoint => GroupPoint_1;

	public PointWithNeighborType StrategyType => GroupPoint_1.PointWithNeighborType;

	public int Id => GroupPoint_1.Id;

	public Vector3 AltPosition => GroupPoint_1.AltPosition.Value;

	public bool HaveAltPosition => GroupPoint_1.AltPosition.HasValue;

	public Vector3 BasePosition => GroupPoint_1.Position;

	public Vector3 ToWallVector => GroupPoint_1.WallDirection;

	public Vector3 FirePosition => GroupPoint_1.FirePosition;

	public BotTiltType TiltType => GroupPoint_1.TiltType;

	public CoverLevel CoverLevel => GroupPoint_1.CoverLevel;

	public CoverPointDefenceInfo DefenceInfo => GroupPoint_1.DefenceInfo;

	public bool BordersLightHave => GroupPoint_1.BordersLightHave;

	public Vector3 LeftBorderLight => GroupPoint_1.LeftBorderLight;

	public Vector3 RightBorderLight => GroupPoint_1.RightBorderLight;

	public bool AlwaysGood => GroupPoint_1.AlwaysGood;

	public bool CanLookLeft => GroupPoint_1.CanLookLeft;

	public bool CanLookRight => GroupPoint_1.CanLookRight;

	public int HideLevel => GroupPoint_1.HideLevel;

	public int PlaceId => GroupPoint_1.PlaceId;

	public int IdPoint => Id;

	public Vector3 Position => BasePosition;

	public EnvironmentType EnvironmentType => GroupPoint_1.EnvironmentType;

	public bool IsGoodInsideBuilding => GroupPoint_1.IsGoodInsideBuilding;

	public float CoveringWeight
	{
		get
		{
			return CoveringWeight_1;
		}
		set
		{
			CoveringWeight_1 = value;
		}
	}

	public bool IsSpotted => GroupPoint_1.IsSpotted;

	public float BaseWeight
	{
		get
		{
			return StartBaseWeight;
		}
		set
		{
			if (value <= 1f)
			{
				StartBaseWeight = 1f;
			}
			else
			{
				StartBaseWeight = value;
			}
		}
	}

	public ECoverPointSpecial Special => GroupPoint_1.Special;

	public int EnvironmentId => GroupPoint_1.IdEnvironment;

	public CoverType CoverType => GroupPoint_1.CoverType;

	public int ParentGroupPointId => GroupPoint_1.Id;

	public int Owner => GroupPoint_1.OwnerInGame;

	public AICorePoint CorePointInGame => GroupPoint_1.CorePointInGame;

	public CustomNavigationPoint(GroupPoint groupPoint)
	{
		GroupPoint_1 = groupPoint;
	}

	public void SetWeight(float v, bool withBaseWeight = true)
	{
		if (withBaseWeight)
		{
			v *= BaseWeight;
		}
		CoveringWeight = v * DecreasedWeightCoef;
	}

	public void SetClose()
	{
	}

	public void SetLong()
	{
	}

	public void Block()
	{
		GroupPoint_1.Block();
	}

	public void Unblock()
	{
		GroupPoint_1.Unblock();
	}

	public void Spotted(float period)
	{
		GroupPoint_1.Spotted(period);
	}

	public bool IsDangerPositionFarEnough(IEnumerable<Vector3> positionsIMustCare, float minSDistToEnemy)
	{
		return GClass394.IsDangerPositionFarEnough(Position, positionsIMustCare, minSDistToEnemy);
	}

	public bool CanIHide(IEnumerable<Vector3> positionsIMustCare, float minSDistToEnemy, bool useRaycast, bool useAng = true)
	{
		return GClass394.CanIHide(Position, ToWallVector, positionsIMustCare, minSDistToEnemy, useRaycast, useAng);
	}

	public bool CanIHideFromPos(float minSDistToEnemy, bool useRaycast, bool useAng, Vector3 pos)
	{
		return GClass394.CanIHideFromPos(Position, ToWallVector, minSDistToEnemy, useRaycast, useAng, pos);
	}

	public bool CanShootToTargetCast(BotOwner shooter, float deltaLastTimeVision)
	{
		EnemyInfo goalEnemy = shooter.Memory.GoalEnemy;
		if (goalEnemy.CanShoot && goalEnemy.IsVisible)
		{
			return true;
		}
		ShootPointClass shootPointClass = shooter.CurrentEnemyTargetPosition(sensPosition: false);
		if (shootPointClass == null)
		{
			return false;
		}
		bool flag = lastCanShoot;
		if (NextCheckCanShootTime < Time.time)
		{
			if (Time.time - goalEnemy.TimeLastSeen > deltaLastTimeVision)
			{
				flag = false;
			}
			else
			{
				float num = 2f;
				NextCheckCanShootTime = Time.time + num;
				flag = GClass369.CanShootToTarget(shootPointClass, this, shooter.LookSensor.Mask);
			}
		}
		lastCanShoot = flag;
		return lastCanShoot;
	}

	public void SetDecreasedWeight(bool val)
	{
		DecreasedWeightCoef = (val ? LocalBotSettingsProviderClass.Core.MIDDLE_POINT_COEF : 1f);
	}

	public void OnDrawGizmosAsAmbush(Vector3? cameraPos = null, float sDist = 0f, bool drawSides = true)
	{
		if (!cameraPos.HasValue || !((Position - cameraPos.Value).sqrMagnitude > sDist))
		{
			method_0();
			Vector3 up = Vector3.up;
			Gizmos.color = new Color(0.1f, 0.2f, 0.7f);
			Gizmos.DrawLine(Position, Position + up);
			if (HaveAltPosition)
			{
				Gizmos.DrawLine(AltPosition, AltPosition + up);
			}
			method_1();
			Gizmos.color = new Color(0.7f, 0.2f, 0.2f);
			Gizmos.DrawLine(Position + Vector3.right / 6f, Position + up + Vector3.right / 6f);
			Gizmos.DrawLine(Position + Vector3.left / 6f, Position + up + Vector3.left / 6f);
			Gizmos.DrawLine(Position + Vector3.back / 6f, Position + up + Vector3.back / 6f);
			Gizmos.DrawLine(Position + Vector3.forward / 6f, Position + up + Vector3.forward / 6f);
			Gizmos.color = new Color(0.5f, 1f, 0.2f);
			Vector3 vector = Position + up;
			Vector3 to = Position + ToWallVector.normalized * 0.8f + up;
			Gizmos.DrawLine(vector, to);
			Color color = new Color(0.9f, 0.5f, 0.1f);
			switch (CoverLevel)
			{
			case CoverLevel.Lay:
				color = new Color(0.1f, 0.9f, 0.3f);
				break;
			case CoverLevel.Sit:
				color = new Color(0.5f, 0.1f, 0.9f);
				break;
			}
			Gizmos.color = color;
			Gizmos.DrawSphere(Position + up, 1f / 6f);
		}
	}

	public void OnDrawGizmosFullAsCover(Vector3? cameraPos = null, float sDist = 0f, bool drawSides = true)
	{
		if (cameraPos.HasValue && sDist > -1f && (Position - cameraPos.Value).sqrMagnitude > sDist)
		{
			return;
		}
		method_1();
		if (StrategyType == PointWithNeighborType.both)
		{
			Gizmos.color = new Color(1f, 0.4f, 0f);
			Gizmos.DrawWireCube(Position + Vector3.up * 0.5f, Vector3.one / 3f);
		}
		method_0();
		Vector3 up = Vector3.up;
		if (HaveAltPosition)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(AltPosition, AltPosition + up);
		}
		if (AlwaysGood)
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(Position, Position + up);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(Position + up, Position + up + Vector3.right / 2f);
			Gizmos.DrawLine(Position + up, Position + up + Vector3.left / 2f);
			Gizmos.DrawLine(Position + up, Position + up + Vector3.back / 2f);
			Gizmos.DrawLine(Position + up, Position + up + Vector3.forward / 2f);
		}
		else
		{
			if (!(ToWallVector != Vector3.zero))
			{
				return;
			}
			float num = 0f;
			switch (CoverLevel)
			{
			case CoverLevel.Stay:
				num = 1.7f;
				break;
			case CoverLevel.Sit:
				num = 1f;
				break;
			case CoverLevel.Lay:
				num = 0.5f;
				break;
			}
			up = Vector3.up * num;
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(Position, Position + up);
			Gizmos.color = Color.red;
			Vector3 vector = Position + up;
			Vector3 vector2 = Position + ToWallVector.normalized * 0.8f + up;
			Gizmos.DrawLine(vector, vector2);
			Vector3 b = vector2 - vector;
			Vector3 normalized = GClass855.RotateOnAngUp(b, 13f).normalized;
			Vector3 normalized2 = GClass855.RotateOnAngUp(b, -13f).normalized;
			Gizmos.DrawLine(vector, vector + normalized);
			Gizmos.DrawLine(vector, vector + normalized2);
			if (!(FirePosition != Vector3.zero))
			{
				return;
			}
			Vector3 firePosition = FirePosition;
			switch (CoverLevel)
			{
			case CoverLevel.Stay:
			{
				Vector3 position = Position;
				Gizmos.color = Color.yellow;
				position.y = firePosition.y;
				if (firePosition.x != 0f || firePosition.z != 0f)
				{
					Gizmos.DrawLine(firePosition, position);
					Gizmos.color = Color.green;
					Gizmos.DrawLine(firePosition, firePosition + ToWallVector.normalized * 0.8f);
				}
				break;
			}
			case CoverLevel.Sit:
			{
				Vector3 vector4 = Position + up;
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(vector4, vector4 + ToWallVector.normalized * 0.8f);
				break;
			}
			case CoverLevel.Lay:
			{
				Gizmos.color = Color.cyan;
				Vector3 vector3 = Position + up;
				Gizmos.DrawLine(vector3, vector3 + ToWallVector.normalized * 0.8f);
				break;
			}
			}
		}
	}

	public bool IsFreeById(int ownerId)
	{
		return GroupPoint_1.IsFreeById(ownerId);
	}

	public void SetOwner(BotOwner owner)
	{
		GroupPoint_1.SetOwner(owner.Id);
	}

	public void SetFree()
	{
		GroupPoint_1.SetOwner(-1);
	}

	public bool IsGoodForGrenade(GClass581 grenadeDangerPoint, BotOwner owner)
	{
		if (grenadeDangerPoint != null && !(owner == null) && owner.BotState == EBotState.Active)
		{
			if (!IsFreeById(owner.Id))
			{
				return false;
			}
			Vector3 vector = Vector3.Project(ToWallVector.normalized, (grenadeDangerPoint.DangerPoint - owner.Position).normalized);
			Vector3 direction = grenadeDangerPoint.DangerPoint - owner.Position;
			if (vector.magnitude <= 0.15f)
			{
				return false;
			}
			RaycastHit hitInfo;
			return Physics.Raycast(new Ray(owner.Position + BotOwner.STAY_HEIGHT, direction), out hitInfo, direction.magnitude * 0.8f, LayerMaskClass.HighPolyWithTerrainMask);
		}
		return true;
	}

	public void method_0()
	{
		float t = (float)HideLevel / 100f;
		Gizmos.color = Color.Lerp(Color.red, Color.green, t);
		float num = 0.3f;
		Gizmos.DrawCube(Position + Vector3.up * 0.5f, new Vector3(num, num, num));
	}

	public void method_1()
	{
		if (IsGoodInsideBuilding)
		{
			Gizmos.color = new Color(0.1f, 1f, 0f);
			Gizmos.DrawWireCube(Position + Vector3.up * 0.5f, new Vector3(0.3f, 1f, 0.2f));
		}
	}
}
