using System;
using System.Collections.Generic;
using System.Text;
using EFT;
using UnityEngine;

public abstract class GClass369
{
	public const int DIRS_COUNT = 8;

	public static List<Vector3> BaseDirections;

	public static List<Vector3> BaseDirections8;

	public static readonly List<Vector3> AllBaseSides;

	[NonSerialized]
	public static bool Bool_0;

	[NonSerialized]
	public static Dictionary<Vector3, Vector3[]> Dictionary_0;

	public static void ExceptWith<T>(this List<T> thisList, List<T> other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (other.Count == 0)
		{
			return;
		}
		if (other == thisList)
		{
			thisList.Clear();
			return;
		}
		foreach (T item in other)
		{
			thisList.Remove(item);
		}
	}

	public static string ParentsLog(GameObject go)
	{
		Transform transform = go.transform;
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (flag && num < 10)
		{
			num++;
			if (flag = transform.parent != null)
			{
				transform = transform.transform.parent;
				stringBuilder.Append(" -> " + transform.name);
			}
		}
		return stringBuilder.ToString();
	}

	public static bool IsDisabledAtLocalGame(this WildSpawnType t)
	{
		if (BotSettingsRepoClass.IsSectant(t))
		{
			return true;
		}
		if ((uint)(t - 29) > 1u && (uint)(t - 51) > 1u)
		{
			return false;
		}
		return true;
	}

	public static string WideLog(this Vector3 v)
	{
		return v.x.ToString("0.000") + " " + v.y.ToString("0.000") + " " + v.z.ToString("0.000");
	}

	public static bool CanShootToTarget(ShootPointClass shootToPoint, CustomNavigationPoint point, LayerMask mask, bool doubleSide = false)
	{
		return point.CanIShootToEnemy = CanShootToTarget(shootToPoint, point.FirePosition, mask, doubleSide);
	}

	public static bool CanShootToTarget(ShootPointClass shootToPoint, GroupPointSearchData point, LayerMask mask, bool doubleSide = false)
	{
		return point.CanIShootToEnemy = CanShootToTarget(shootToPoint, point.Point.FirePosition, mask, doubleSide);
	}

	public static void DebugDrawWay(Vector3[] way, Vector3 offset, Color c, float duration = 10f)
	{
	}

	public static void DebugDrawWay<T>(T[] way, Vector3 offset, Color c, float duration = 10f) where T : IPositionPoint
	{
	}

	public static bool IsPointInsideDangerZone(BotOwner bot, Vector3 point)
	{
		BotZoneDangerAreas zoneDangerAreas = bot.BotsGroup.BotZone.ZoneDangerAreas;
		if (zoneDangerAreas.ActiveAreas.Count == 0)
		{
			return false;
		}
		foreach (AIDangerArea activeArea in zoneDangerAreas.ActiveAreas)
		{
			if (activeArea.IsPointInside(point))
			{
				return true;
			}
		}
		return false;
	}

	public static bool CanShootToTarget(ShootPointClass shootToPoint, Vector3 firePos, LayerMask mask, bool doubleSide = false)
	{
		if (shootToPoint == null)
		{
			return false;
		}
		bool result = false;
		Vector3 vector = shootToPoint.Point - firePos;
		Ray ray = new Ray(firePos, vector);
		float magnitude = vector.magnitude;
		if (!Physics.Raycast(ray, out var hitInfo, magnitude * shootToPoint.DistCoef, mask))
		{
			if (doubleSide)
			{
				if (!Physics.Raycast(new Ray(shootToPoint.Point, -vector), out hitInfo, magnitude, mask))
				{
					result = true;
				}
			}
			else
			{
				result = true;
			}
		}
		return result;
	}

	public static bool CanShoot(BotOwner bot, EnemyInfo enemyInfo)
	{
		return CanShoot(bot.Fireport.position, enemyInfo);
	}

	public static bool CanShoot(Vector3 from, EnemyInfo enemyInfo)
	{
		from += Vector3.up;
		Vector3 direction = enemyInfo.GetBodyPartPosition() - from;
		float magnitude = direction.magnitude;
		if (Physics.Raycast(from, direction, out var _, magnitude, LayerMaskClass.HighPolyWithTerrainMask))
		{
			return false;
		}
		return true;
	}

	public static int GetNearEntity(int count, Func<int, Vector3> getSegment)
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = getSegment(i);
			float num2 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			if (!(num2 > num))
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int GetNearEntity(Vector3 susPoint, IPositionPoint[] points)
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 vector = susPoint - points[i].Position;
			float num2 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			if (!(num2 > num))
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int GetNearEntity<T>(Vector3 susPoint, List<T> points) where T : IPositionPoint
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 vector = susPoint - points[i].Position;
			float num2 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			if (!(num2 > num))
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static int GetNearEntity(Vector3 susPoint, List<Vector3> points)
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < points.Count; i++)
		{
			Vector3 vector = susPoint - points[i];
			float num2 = vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
			if (!(num2 > num))
			{
				num = num2;
				result = i;
			}
		}
		return result;
	}

	public static bool PointInOABB(Vector3 point, BoxCollider box)
	{
		return PointInOABB(point, box.transform, box.size, box.center);
	}

	public static bool PointInOABB(Vector3 point, Transform boxPos, Vector3 boxSize, Vector3 centerOffset)
	{
		point = boxPos.transform.InverseTransformPoint(point) - centerOffset;
		float num = boxSize.x * 0.5f;
		float num2 = boxSize.y * 0.5f;
		float num3 = boxSize.z * 0.5f;
		if (point.x < num && point.x > 0f - num && point.y < num2 && point.y > 0f - num2 && point.z < num3 && point.z > 0f - num3)
		{
			return true;
		}
		return false;
	}

	public static bool PointInOABB(Vector3 point, Vector3 boxPos, Vector3 boxSize)
	{
		float num = boxSize.x * 0.5f;
		float num2 = boxSize.y * 0.5f;
		float num3 = boxSize.z * 0.5f;
		if (point.x < boxPos.x + num && point.x > boxPos.x - num && point.y < boxPos.y + num2 && point.y > boxPos.y - num2 && point.z < boxPos.z + num3 && point.z > boxPos.z - num3)
		{
			return true;
		}
		return false;
	}

	public static bool IsPointInside(Vector3 point, Vector3 s, Vector3 e)
	{
		if (point.x < e.x && point.x > s.x && point.y < e.y && point.y > s.y && point.z < e.z && point.z > s.z)
		{
			return true;
		}
		return false;
	}

	public static List<GClass367> CovnertToSave(List<CustomNavigationPoint> points)
	{
		List<GClass367> list = new List<GClass367>();
		foreach (CustomNavigationPoint point in points)
		{
			GClass367 item = new GClass367(point.Position, point.CoveringWeight, point.Id);
			list.Add(item);
		}
		return list;
	}

	public static void DrawGroups(List<CustomNavigationPoint> nearGroups, Vector3 CenterPos, CustomNavigationPoint betterPoint)
	{
	}

	public static void DrawGroups(List<GClass367> nearGroups, Vector3 CenterPos, GClass367 betterPoint, HashSet<GClass367> cantHide = null)
	{
	}

	public static bool HaveWall(Vector3 from, Vector3 to, float dist = 4f)
	{
		Vector3 direction = to - from;
		if (Physics.Raycast(new Ray(from, direction), out var _, dist, LayerMaskClass.HighPolyWithTerrainMask))
		{
			return true;
		}
		return false;
	}

	public static void Init()
	{
		if (Bool_0)
		{
			return;
		}
		int[] array = new int[8];
		int num = 45;
		int num2 = 4;
		int num3 = 1;
		for (int i = 0; i < num2; i++)
		{
			if (i == 0)
			{
				int num4 = 0;
				int num5 = 180;
				array[0] = 0;
				array[7] = 180;
			}
			else
			{
				int num4 = i * num;
				int num5 = 360 - num4;
				array[num3] = num4;
				array[num3 + 1] = num5;
				num3 += 2;
			}
		}
		smethod_0(Vector3.forward, array);
		smethod_0(Vector3.left, array);
		smethod_0(Vector3.right, array);
		smethod_0(Vector3.back, array);
		Bool_0 = true;
	}

	public static Vector3 GetProjectionPoint(Vector3 p, Vector3 p1, Vector3 p2)
	{
		float num = p1.z - p2.z;
		if (num == 0f)
		{
			return new Vector3(p.x, p1.y, p1.z);
		}
		float num2 = p2.x - p1.x;
		if (num2 == 0f)
		{
			return new Vector3(p1.x, p1.y, p.z);
		}
		float num3 = p1.x * p2.z - p2.x * p1.z;
		float num4 = num2 * p.x - num * p.z;
		float num5 = (0f - (num2 * num3 + num * num4)) / (num2 * num2 + num * num);
		return new Vector3((0f - (num3 + num2 * num5)) / num, p1.y, num5);
	}

	public static Vector3 Test4Sides(Vector3 dir, Vector3 headPos)
	{
		dir.y = 0f;
		Vector3[] array = smethod_1(dir);
		if (array == null)
		{
			Debug.LogError("can' find posible dirs");
			return dir;
		}
		int num = 0;
		Vector3 vector;
		while (true)
		{
			if (num < array.Length)
			{
				vector = array[num];
				if (TestDir(headPos, vector, LocalBotSettingsProviderClass.Core.PATROL_MIN_LIGHT_DIST))
				{
					break;
				}
				num++;
				continue;
			}
			return Vector3.one;
		}
		return vector;
	}

	public static float MaxRayDist8HorizontalSides(Vector3 startPos, out RaycastHit rayMax, float maxDist = 200f)
	{
		float num = float.MinValue;
		rayMax = default(RaycastHit);
		int num2 = 0;
		while (true)
		{
			if (num2 < AllBaseSides.Count)
			{
				Vector3 direction = AllBaseSides[num2];
				if (!Physics.Raycast(new Ray(startPos, direction), out var hitInfo, maxDist, LayerMaskClass.HighPolyWithTerrainMask))
				{
					break;
				}
				if (hitInfo.distance > num)
				{
					rayMax = hitInfo;
					num = hitInfo.distance;
				}
				num2++;
				continue;
			}
			return num;
		}
		return float.MaxValue;
	}

	public static bool TestDir(Vector3 headPos, Vector3 dir, float dist, out Vector3? outPos)
	{
		outPos = null;
		RaycastHit hitInfo;
		bool num = Physics.Raycast(new Ray(headPos, dir), out hitInfo, dist, LayerMaskClass.HighPolyWithTerrainMask);
		bool result = !num;
		if (num)
		{
			outPos = hitInfo.point;
		}
		return result;
	}

	public static bool TestDir(Vector3 headPos, Vector3 dir, float dist)
	{
		Vector3? outPos;
		return TestDir(headPos, dir, dist, out outPos);
	}

	public static Vector3? GetCrossPoint(GClass365 p1, GClass365 p2)
	{
		return GetCrossPoint(p1.a, p1.b, p2.a, p2.b);
	}

	public static Vector3? GetCrossPoint(Vector3 a1, Vector3 b1, Vector3 a2, Vector3 b2)
	{
		GClass366 gClass = smethod_2(new GClass366(a1), new GClass366(b1), new GClass366(a2), new GClass366(b2));
		if (gClass != null)
		{
			return new Vector3(gClass.x, a1.y, gClass.y);
		}
		return null;
	}

	public static Vector3 LerpPointWithControl(Vector3 start, Vector3 trg, Vector3 control, float time)
	{
		float num = 1f - time;
		float num2 = num * num;
		float num3 = time * time;
		float x = num2 * start.x + 2f * time * num * control.x + num3 * trg.x;
		float y = num2 * start.y + 2f * time * num * control.y + num3 * trg.y;
		float z = num2 * start.z + 2f * time * num * control.z + num3 * trg.z;
		return new Vector3(x, y, z);
	}

	public static GClass612[] DebugBotScatterings()
	{
		return new List<GClass612>
		{
			new GClass612("smg", 0.3f, 0.15f, 0.04f),
			new GClass612("pistol", 0.3f, 0.15f, 0.04f),
			new GClass612("sniperRifle", 0.3f, 0.15f, 0.04f),
			new GClass612("shotgun", 0.3f, 0.15f, 0.04f),
			new GClass612("assaultRifle", 0.3f, 0.15f, 0.04f),
			new GClass612("assaultCarbine", 0.3f, 0.15f, 0.04f)
		}.ToArray();
	}

	public static void smethod_0(Vector3 startDir, int[] indexOfDirs)
	{
		Vector3[] array = new Vector3[8];
		for (int i = 0; i < 8; i++)
		{
			int num = indexOfDirs[i];
			Vector3 vector = GClass855.RotateOnAngUp(startDir, GClass856.GreateRandom(num, 0.1f));
			array[i] = vector;
		}
		Dictionary_0.Add(startDir, array);
	}

	public static Vector3[] smethod_1(Vector3 dir)
	{
		float num = float.MaxValue;
		Vector3[] result = null;
		foreach (KeyValuePair<Vector3, Vector3[]> item in Dictionary_0)
		{
			float num2 = Vector3.Angle(dir, item.Key);
			if (num2 < num)
			{
				num = num2;
				result = item.Value;
			}
		}
		return result;
	}

	public static GClass366 smethod_2(GClass366 p1, GClass366 p2, GClass366 p3, GClass366 p4)
	{
		float num = (p1.x - p2.x) * (p4.y - p3.y) - (p1.y - p2.y) * (p4.x - p3.x);
		float num2 = (p1.x - p3.x) * (p4.y - p3.y) - (p1.y - p3.y) * (p4.x - p3.x);
		float num3 = (p1.x - p2.x) * (p1.y - p3.y) - (p1.y - p2.y) * (p1.x - p3.x);
		if (num == 0f)
		{
			return null;
		}
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 >= 0f && num4 <= 1f && num5 >= 0f && num5 <= 1f)
		{
			float dx = p1.x + num4 * (p2.x - p1.x);
			float dy = p1.y + num4 * (p2.y - p1.y);
			return new GClass366(dx, dy);
		}
		return null;
	}

	public static float SDistHorizontal(Vector3 v)
	{
		return v.x * v.x + v.z * v.z;
	}

	static GClass369()
	{
		BaseDirections = new List<Vector3>();
		BaseDirections8 = new List<Vector3>();
		AllBaseSides = new List<Vector3>(8)
		{
			new Vector3(0f, 0f, 1f),
			new Vector3(1f, 0f, 0f),
			new Vector3(0f, 0f, -1f),
			new Vector3(-1f, 0f, 0f),
			new Vector3(1f, 0f, 1f).normalized,
			new Vector3(1f, 0f, -1f).normalized,
			new Vector3(-1f, 0f, 1f).normalized,
			new Vector3(-1f, 0f, -1f).normalized
		};
		Dictionary_0 = new Dictionary<Vector3, Vector3[]>();
		BaseDirections.Add(new Vector3(1f, 0f, 0f));
		BaseDirections.Add(new Vector3(0f, 0f, -1f));
		BaseDirections.Add(new Vector3(0f, 0f, 1f));
		BaseDirections.Add(new Vector3(-1f, 0f, 0f));
		BaseDirections8.Add(new Vector3(1f, 0f, 0f));
		BaseDirections8.Add(new Vector3(0f, 0f, -1f));
		BaseDirections8.Add(new Vector3(0f, 0f, 1f));
		BaseDirections8.Add(new Vector3(-1f, 0f, 0f));
		BaseDirections8.Add(GClass855.NormalizeFastSelf(new Vector3(1f, 0f, 1f)));
		BaseDirections8.Add(GClass855.NormalizeFastSelf(new Vector3(1f, 0f, -1f)));
		BaseDirections8.Add(GClass855.NormalizeFastSelf(new Vector3(-1f, 0f, 1f)));
		BaseDirections8.Add(GClass855.NormalizeFastSelf(new Vector3(-1f, 0f, -1f)));
	}

	public static void DebugDrawArc(Vector3 p1, Vector3 p2, float height, float duration, Color color)
	{
		Vector3 vector = Vector3.up * height;
		Debug.DrawLine(p1, p1 + vector, color, duration);
		Debug.DrawLine(p2, p2 + vector, color, duration);
		Debug.DrawLine(p1 + vector, p2 + vector, color, duration);
	}

	public static bool IsPointOnWay(Vector3[] path, Vector3 p, float distToBeClose, int startIndex, int maxPointToCheck = 5)
	{
		int num = Mathf.Min(path.Length, maxPointToCheck + startIndex);
		int num2 = 1 + startIndex;
		while (true)
		{
			if (num2 < num)
			{
				Vector3 p2 = path[num2 - 1];
				Vector3 p3 = path[num2];
				if ((GetProjectionPoint(p, p2, p3) - p).magnitude < distToBeClose)
				{
					break;
				}
				num2++;
				continue;
			}
			return false;
		}
		return true;
	}
}
