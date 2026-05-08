using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using UnityEngine;
using UnityEngine.AI;

public class GClass483 : GClass429
{
	[NonSerialized]
	public float Float_0 = 60f;

	[NonSerialized]
	public float Float_1 = 8f;

	[NonSerialized]
	public float Float_2;

	[NonSerialized]
	public float Float_3 = 40f;

	[NonSerialized]
	public float Float_4;

	[NonSerialized]
	public Vector3 Vector3_0 = new Vector3(1f, 0f, 0f);

	[NonSerialized]
	public List<Vector3> List_0 = new List<Vector3>();

	[NonSerialized]
	public float Float_5;

	[NonSerialized]
	public EShootPositionType EshootPositionType_0;

	[NonSerialized]
	public Vector3 Vector3_1;

	[NonSerialized]
	[CompilerGenerated]
	public Vector3? Nullable_0;

	[NonSerialized]
	[CompilerGenerated]
	public ShootPointClass ShootPointClass;

	public EShootPositionType ShootPositionType => EshootPositionType_0;

	public Vector3? LastGoodPoint
	{
		[CompilerGenerated]
		get
		{
			return Nullable_0;
		}
		[CompilerGenerated]
		set
		{
			Nullable_0 = value;
		}
	}

	public ShootPointClass LastGoodPointShoot
	{
		[CompilerGenerated]
		get
		{
			return ShootPointClass;
		}
		[CompilerGenerated]
		set
		{
			ShootPointClass = value;
		}
	}

	public GClass483(BotOwner owner, int dist, int minDist, float periodCheck)
		: base(owner)
	{
		Float_5 = periodCheck;
		Float_0 = dist;
		Float_3 = minDist;
		Float_1 = Mathf.Clamp(Float_0 * 0.2f, 2f, 18f);
		Set(EShootPositionType.stand);
		method_0();
	}

	public bool ManualUpdateSearch(ShootPointClass shootTarget, float enemyCheck, out Vector3 point)
	{
		if (Float_2 > Time.time)
		{
			point = Vector3.zero;
			return false;
		}
		Float_2 = Time.time + Float_5;
		if (method_1(shootTarget, enemyCheck, out point))
		{
			LastGoodPoint = point;
			LastGoodPointShoot = shootTarget;
			return true;
		}
		return false;
	}

	public void Drop()
	{
		LastGoodPointShoot = null;
		LastGoodPoint = null;
	}

	public void Set(EShootPositionType positionType)
	{
		EshootPositionType_0 = positionType;
		switch (EshootPositionType_0)
		{
		case EShootPositionType.stand:
			Vector3_1 = BotOwner_0.ShootData.WeaponRootOffset;
			break;
		case EShootPositionType.sit:
			Vector3_1 = BotOwner_0.ShootData.WeaponRootOffset * 0.5f;
			break;
		case EShootPositionType.lay:
			Vector3_1 = BotOwner_0.ShootData.WeaponRootOffset * 0.1f;
			break;
		}
	}

	public bool IsShootToThis(ShootPointClass pointToTest)
	{
		if (LastGoodPoint.HasValue && (LastGoodPointShoot.Point - pointToTest.Point).sqrMagnitude > 9f)
		{
			return false;
		}
		return true;
	}

	public void method_0()
	{
		float num = Mathf.Acos(Float_1 / Float_0) * 57.29578f;
		Float_4 = 180f - 2f * num;
		Vector3_0 *= Float_0;
		Vector3 vector = new Vector3(0f, 0f, 1f);
		int num2 = 6;
		float angDegree = 60f;
		for (int i = 0; i < num2; i++)
		{
			vector = GClass855.RotateOnAngUp(vector, angDegree);
			List_0.Add(vector * Float_1 * 0.7f);
		}
		int num3 = 4;
		angDegree = 90f;
		vector = new Vector3(0f, 0f, 1f);
		for (int j = 0; j < num3; j++)
		{
			vector = GClass855.RotateOnAngUp(vector, angDegree);
			List_0.Add(vector * Float_1 * 0.4f);
		}
	}

	public bool method_1(ShootPointClass shootTarget, float enemyCheck, out Vector3 point)
	{
		Vector3 point2 = shootTarget.Point;
		Vector3_0 = GClass855.RotateOnAngUp(Vector3_0, Float_4);
		Vector3 vector = point2 + Vector3_0;
		if (enemyCheck > 0f)
		{
			float num = enemyCheck * enemyCheck;
			foreach (KeyValuePair<IPlayer, EnemyInfo> enemyInfo in BotOwner_0.EnemiesController.EnemyInfos)
			{
				if (!((enemyInfo.Value.CurrPosition - vector).sqrMagnitude >= num))
				{
					point = Vector3.zero;
					GClass810.DebugCircle(vector, Vector3.up, Color.red, Float_1, 1f);
					return false;
				}
			}
		}
		GClass810.DebugCircle(vector, Vector3.up, Color.magenta, Float_1, 1f);
		foreach (Vector3 item in List_0)
		{
			Vector3 pointToTest = vector + item;
			if (method_2(pointToTest, point2, out var navMeshPoint))
			{
				point = navMeshPoint;
				return true;
			}
		}
		point = Vector3.zero;
		return false;
	}

	public bool method_2(Vector3 pointToTest, Vector3 shootPoint, out Vector3 navMeshPoint)
	{
		ShootPointClass shootToPoint = new ShootPointClass(shootPoint);
		if (NavMesh.SamplePosition(pointToTest, out var hit, Float_1 * 0.5f, -1))
		{
			navMeshPoint = hit.position;
			Vector3 vector = navMeshPoint + Vector3_1;
			bool num = GClass369.CanShootToTarget(shootToPoint, vector, BotOwner_0.LookSensor.Mask);
			float magnitude = (shootPoint - navMeshPoint).magnitude;
			GClass810.DebugPoint(pointToTest, Color.white, 0.8f, 1f);
			Debug.DrawLine(vector, pointToTest, Color.white, 1f);
			if (!num)
			{
				GClass810.DebugPoint(vector, Color.red, 0.5f, 1f);
				return false;
			}
			GClass810.DebugPoint(vector, Color.magenta, 0.5f, 1f);
			if (magnitude > Float_3)
			{
				NavMeshPath navMeshPath = new NavMeshPath();
				if (NavMesh.CalculatePath(BotOwner_0.Position, navMeshPoint, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					Debug.DrawLine(vector, shootPoint, Color.green, 1f);
					return true;
				}
				Debug.DrawLine(vector, shootPoint, Color.yellow, 1f);
				navMeshPoint = Vector3.zero;
				return false;
			}
			Debug.DrawLine(vector, shootPoint, Color.blue, 1f);
		}
		else
		{
			GClass810.DebugPoint(pointToTest, Color.red, 0.5f, 1f);
		}
		navMeshPoint = Vector3.zero;
		return false;
	}
}
