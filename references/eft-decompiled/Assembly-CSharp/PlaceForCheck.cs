using System;
using System.Collections.Generic;
using EFT;
using UnityEngine;

public class PlaceForCheck
{
	public Vector3 BasePoint;

	public BotOwner CheckingPlayer;

	public bool Reacheble = true;

	[NonSerialized]
	public bool IsCome_1;

	[NonSerialized]
	public List<Vector3> Points2look = new List<Vector3>(3);

	[NonSerialized]
	public List<int> IsPointChecked = new List<int>();

	public int _environmentId;

	[field: NonSerialized]
	public CustomNavigationPoint Point { get; }

	[field: NonSerialized]
	public PlaceForCheckType Type { get; set; }

	[field: NonSerialized]
	public float CreatedTime { get; set; }

	[field: NonSerialized]
	public float DieTime { get; set; }

	public bool IsDanger => Type == PlaceForCheckType.danger;

	public Vector3 Position
	{
		get
		{
			if (Point != null)
			{
				return Point.Position;
			}
			return BasePoint;
		}
	}

	public bool IsCome
	{
		get
		{
			return IsCome_1;
		}
		set
		{
			IsCome_1 = value;
			if (IsCome_1 && IsDanger)
			{
				method_2();
			}
		}
	}

	public int EnvironmentId => _environmentId;

	public PlaceForCheck(CustomNavigationPoint Point, Vector3 basePoint, PlaceForCheckType type)
	{
		this.Point = Point;
		BasePoint = basePoint;
		method_1(type);
		method_0();
	}

	public PlaceForCheck(Vector3 Point, PlaceForCheckType type)
	{
		BasePoint = Point;
		method_1(type);
		method_0();
	}

	public void method_0()
	{
		if ((bool)EnvironmentManagerBase.Instance)
		{
			_environmentId = EnvironmentManagerBase.Instance.TryFindEnvironmentIdByPos(BasePoint);
		}
	}

	public bool IsThisPointClose(Vector3 testPoint)
	{
		return (testPoint - BasePoint).magnitude < LocalBotSettingsProviderClass.Core.CLOSE_POINTS;
	}

	public void PointLookComplete(int index)
	{
		IsPointChecked.Add(index);
	}

	public int GetPointToLook(out Vector3? point)
	{
		int num = 0;
		while (true)
		{
			if (num < Points2look.Count)
			{
				if (!IsPointChecked.Contains(num))
				{
					break;
				}
				num++;
				continue;
			}
			point = null;
			return num;
		}
		point = Points2look[num];
		return num;
	}

	public bool IsOld()
	{
		if (DieTime < Time.time)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return "Pos:" + Position.ToString() + " Type:" + Type.ToString() + "  IsCome:" + IsCome_1;
	}

	public void method_1(PlaceForCheckType type)
	{
		Type = type;
		CreatedTime = Time.time;
		float num = (IsDanger ? LocalBotSettingsProviderClass.Core.DANGER_POINT_LIFE_TIME_SEC : LocalBotSettingsProviderClass.Core.SIMPLE_POINT_LIFE_TIME_SEC);
		DieTime = Time.time + num;
	}

	public void method_2()
	{
		Vector3 normalized = new Vector3(UnityEngine.Random.value, 0f, UnityEngine.Random.value).normalized;
		for (int i = 0; i < LocalBotSettingsProviderClass.Core.COUNT_TURNS; i++)
		{
			Vector3 vector = Quaternion.AngleAxis(360f / (float)LocalBotSettingsProviderClass.Core.COUNT_TURNS * (float)i, Vector3.up) * normalized + Position;
			if (!GClass369.HaveWall(Position, vector))
			{
				Points2look.Add(vector);
			}
		}
		IsPointChecked.Clear();
	}
}
