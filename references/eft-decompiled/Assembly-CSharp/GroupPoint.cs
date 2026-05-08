using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

[Serializable]
public class GroupPoint : GInterface3, IPositionPoint, IAICorePointLink
{
	public const float GROUP_POINT_NAV_SAMPLE_DIST = 1f;

	public const int BASE_HIDE_VAL = 51;

	public const float CHECK_CAN_HIDE_STAY = 1.272f;

	public const float LIGHT_WALL_ANG = 57f;

	[SerializeField]
	public Vector3 FirePosition;

	[SerializeField]
	public int Index;

	[SerializeField]
	public PointWithNeighborType PointWithNeighborType;

	[SerializeField]
	public Vector3 _position;

	public bool AlwaysGood;

	public CoverLevel CoverLevel;

	public int Id;

	public CoverPointDefenceInfo DefenceInfo;

	public Vector3? AltPosition;

	public Vector3 WallDirection;

	public CoverType CoverType;

	public ECoverPointSpecial Special;

	public EnvironmentType EnvironmentType;

	public int IdEnvironment;

	public int ConnectionGroup = -1;

	public bool CanLookLeft;

	public bool CanLookRight;

	public bool BordersLightHave;

	public Vector3 LeftBorderLight;

	public Vector3 RightBorderLight;

	public BotTiltType TiltType;

	public bool IsGoodInsideBuilding;

	public List<GroupPointWay> NeighbourhoodsWays = new List<GroupPointWay>();

	public List<int> NeighbourhoodsWaysIds = new List<int>();

	public int ParentNavPoint;

	public int CorePointId;

	[NonSerialized]
	public Dictionary<int, CustomNavigationPoint> Childs = new Dictionary<int, CustomNavigationPoint>();

	[NonSerialized]
	public GClass393 InGame;

	public int PlaceId;

	[NonSerialized]
	public AICorePoint CorePoint;

	public Vector3 Position => _position;

	public float DefenceLevel => DefenceInfo.DefenceLevel;

	[field: NonSerialized]
	public float MagnitudeForSort { get; set; }

	[field: NonSerialized]
	public int CounterId { get; set; } = -1;

	public bool IsSpotted => InGame.IsSpotted;

	public int HideLevel => 51;

	public int OwnerInGame => InGame.OwnerId;

	public AICorePoint CorePointInGame => CorePoint;

	public GroupPoint(int id, NavPoint point, Vector3 position, Vector3? altPos, AICorePoint corePoint, CoverLevel coverLevel = CoverLevel.Sit, bool alwaysGood = false, Vector3 wallDirection = default(Vector3), Vector3 firePosition = default(Vector3), PointWithNeighborType pointWithNeighborType = PointWithNeighborType.cover)
	{
		DefenceInfo = new CoverPointDefenceInfo(0);
		CorePointId = corePoint.Id;
		ConnectionGroup = corePoint.ConnectionGroupId;
		Id = id;
		ParentNavPoint = point?.Id ?? (-1);
		_position = position;
		AltPosition = altPos;
		WallDirection = wallDirection;
		if (firePosition == Vector3.zero)
		{
			firePosition = position;
		}
		FirePosition = firePosition + Vector3.up * 1.272f;
		CoverLevel = coverLevel;
		PointWithNeighborType = pointWithNeighborType;
		AlwaysGood = alwaysGood;
	}

	public void InitLightBorders()
	{
		if (WallDirection.sqrMagnitude > 0f)
		{
			LeftBorderLight = GClass855.RotateOnAngUp(WallDirection, 57f);
			RightBorderLight = GClass855.RotateOnAngUp(WallDirection, -57f);
			BordersLightHave = true;
			LeftBorderLight = GClass855.NormalizeFastSelf(LeftBorderLight);
			RightBorderLight = GClass855.NormalizeFastSelf(RightBorderLight);
		}
		else
		{
			BordersLightHave = false;
		}
	}

	public bool IsSameMeaning(GInterface3 p1, float sDist, out GInterface3 worst)
	{
		worst = null;
		if (p1 is GroupPoint groupPoint)
		{
			if (groupPoint.PointWithNeighborType != PointWithNeighborType.ambush && PointWithNeighborType != PointWithNeighborType.ambush)
			{
				return false;
			}
			float num = Vector3.Dot(groupPoint.WallDirection, WallDirection);
			if (num > 0f && sDist < 1.6899998f)
			{
				if (groupPoint.PointWithNeighborType == PointWithNeighborType.ambush)
				{
					worst = groupPoint;
				}
				else
				{
					worst = this;
				}
				return true;
			}
			if (num < 0f && sDist < 0.36f)
			{
				if (groupPoint.PointWithNeighborType == PointWithNeighborType.ambush)
				{
					worst = groupPoint;
				}
				else
				{
					worst = this;
				}
				return true;
			}
		}
		return false;
	}

	public CustomNavigationPoint CreateCustomNavigationPoint(int botOwnerId)
	{
		CustomNavigationPoint customNavigationPoint = new CustomNavigationPoint(this);
		Childs.Add(botOwnerId, customNavigationPoint);
		return customNavigationPoint;
	}

	public void InitForGame(AICorePointHolder corePointHolder)
	{
		method_0();
		InitLightBorders();
		method_2();
		CorePoint = corePointHolder.GetCorePoint(CorePointId);
		InGame = new GClass393(CorePoint, this);
		Childs = new Dictionary<int, CustomNavigationPoint>();
	}

	public void CalcDefenceLevel()
	{
		DefenceInfo = new CoverPointDefenceInfo(Position);
	}

	public void DrawGizmos(bool withEdges, bool withCore, float upRay = -1f)
	{
		bool flag = false;
		if (AlwaysGood)
		{
			Gizmos.color = Color.magenta;
		}
		else
		{
			switch (PointWithNeighborType)
			{
			case PointWithNeighborType.cover:
				flag = true;
				Gizmos.color = Color.red;
				break;
			case PointWithNeighborType.ambush:
				Gizmos.color = Color.blue;
				break;
			case PointWithNeighborType.both:
				flag = true;
				Gizmos.color = Color.green;
				break;
			}
		}
		float num = 1f;
		Gizmos.DrawRay(Position + Vector3.up * num, WallDirection * 0.2f);
		if (upRay > 0f)
		{
			Gizmos.DrawRay(Position, Vector3.up * upRay);
		}
		Vector3 vector = Vector3.up * num * 0.5f;
		Gizmos.DrawCube(Position + vector, new Vector3(0.1f, num, 0.1f));
		if (flag)
		{
			Gizmos.DrawCube(FirePosition, new Vector3(0.1f, 0.1f, 0.1f));
			Gizmos.DrawLine(Position + Vector3.up * num, FirePosition);
			Gizmos.DrawRay(FirePosition, WallDirection * 0.3f);
		}
		if (withEdges)
		{
			Gizmos.color = Color.yellow;
			foreach (GroupPointWay neighbourhoodsWay in NeighbourhoodsWays)
			{
				if (neighbourhoodsWay.Target != null)
				{
					Gizmos.DrawLine(neighbourhoodsWay.Target.Position, Position + new Vector3(0f, 0.05f, 0f));
				}
			}
		}
		Gizmos.color = Color.blue;
		if (withCore && CorePoint != null)
		{
			Gizmos.DrawLine(CorePoint.Position, Position);
		}
	}

	public void SetPosition(Vector3 position)
	{
		_position = position;
	}

	public CustomNavigationPoint GetById(int botId)
	{
		return Childs[botId];
	}

	public void AddConnectedGroupPoint(GroupPointWay way)
	{
		NeighbourhoodsWays.Add(way);
		NeighbourhoodsWaysIds.Add(way.Id);
	}

	public bool IsFreeById(int botId)
	{
		return InGame.IsFreeById(botId);
	}

	public void Spotted(float period)
	{
		InGame.Spotted(period);
	}

	public void Block()
	{
		InGame.Block();
	}

	public void Unblock()
	{
		InGame.Unblock();
	}

	public void RemoveWay(GroupPointWay target)
	{
		NeighbourhoodsWays.Remove(target);
		NeighbourhoodsWaysIds.Remove(target.Id);
	}

	public void SetOwner(int owner)
	{
		InGame.SetOwner(owner);
	}

	public void method_0()
	{
		Vector3 vector = GClass855.Rotate90(WallDirection, GClass855.SideTurn.left);
		Vector3 vector2 = GClass855.Rotate90(WallDirection, GClass855.SideTurn.right);
		Vector3 vector3 = Vector3.up * 0.8f;
		CanLookLeft = GClass369.TestDir(Position + vector3, vector, LocalBotSettingsProviderClass.Core.HOLD_MIN_LIGHT_DIST, out var outPos);
		CanLookRight = GClass369.TestDir(Position + vector3, vector2, LocalBotSettingsProviderClass.Core.HOLD_MIN_LIGHT_DIST, out var outPos2);
		if (CoverLevel == CoverLevel.Stay && PointWithNeighborType != PointWithNeighborType.ambush)
		{
			Vector3 rhs = FirePosition - Position;
			if (!CanLookLeft && Vector3.Dot(vector, rhs) > 0f)
			{
				CanLookLeft = true;
			}
			if (!CanLookRight && Vector3.Dot(vector2, rhs) > 0f)
			{
				CanLookRight = true;
			}
		}
		if (!CanLookLeft)
		{
			CanLookLeft = method_1(outPos);
		}
		if (!CanLookRight)
		{
			CanLookRight = method_1(outPos2);
		}
	}

	public bool method_1(Vector3? sidePos)
	{
		float num = 0.5f;
		float dist = 1.2f;
		if (sidePos.HasValue)
		{
			Vector3 value = sidePos.Value;
			value.y = Position.y;
			Vector3 v = Position - value;
			int num2 = (int)(v.magnitude / num);
			Vector3 sTAY_HEIGHT = BotOwner.STAY_HEIGHT;
			for (int i = 0; i < num2; i++)
			{
				float num3 = (float)i * num;
				Vector3 vector = GClass855.NormalizeFastSelf(v);
				Vector3 vector2 = value + vector * num3;
				if (GClass369.TestDir(sTAY_HEIGHT + vector2, WallDirection, dist))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void method_2()
	{
		if (CoverLevel == CoverLevel.Stay)
		{
			Vector3 n = Position - FirePosition;
			n.y = 0f;
			Vector3 wallDirection = WallDirection;
			wallDirection.y = 0f;
			Vector3 vector = GClass856.Rotate90(n, 1);
			Vector3 vector2 = GClass856.Rotate90(n, -1);
			float num = Vector3.Angle(vector, wallDirection);
			float num2 = Vector3.Angle(vector2, wallDirection);
			TiltType = ((!(num2 > num)) ? BotTiltType.right : BotTiltType.left);
		}
	}
}
