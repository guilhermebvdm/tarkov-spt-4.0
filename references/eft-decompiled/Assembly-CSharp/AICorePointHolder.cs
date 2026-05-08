using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class AICorePointHolder : MonoBehaviour
{
	[CompilerGenerated]
	public class Class136
	{
		public List<AICorePoint> all;

		public Vector3 pos;

		public Vector3 method_0(int i)
		{
			return all[i].Position - pos;
		}
	}

	[CompilerGenerated]
	public class Class137
	{
		public List<AICorePoint> all;

		public Vector3 pos;

		public Vector3 method_0(int i)
		{
			return all[i].Position - pos;
		}
	}

	[CompilerGenerated]
	public class Class138
	{
		public int corePointId;

		public bool method_0(AICorePoint x)
		{
			return x.Id == corePointId;
		}
	}

	[CompilerGenerated]
	public class Class139
	{
		public int corePointId;

		public bool method_0(AICorePoint x)
		{
			return x.Id == corePointId;
		}
	}

	public float DrawHeight = 10f;

	private static AICorePointHolder aicorePointHolder_0;

	public List<AICorePoint> CorePoints;

	public bool IsAlwaysDraw;

	public static List<AICorePoint> GetAllTestObjects(bool canUseCache = false)
	{
		if (aicorePointHolder_0 != null && canUseCache)
		{
			return aicorePointHolder_0.CorePoints;
		}
		aicorePointHolder_0 = Object.FindObjectOfType<AICorePointHolder>();
		return aicorePointHolder_0.CorePoints;
	}

	public static AICorePoint GetClosest(Vector3 pos)
	{
		List<AICorePoint> all = GetAllTestObjects();
		int nearEntity = GClass369.GetNearEntity(all.Count, (int i) => all[i].Position - pos);
		return all[nearEntity];
	}

	public static AICorePoint GetAnyPointToConnect(Vector3 pos)
	{
		List<AICorePoint> all = GetAllTestObjects(canUseCache: true);
		int nearEntity = GClass369.GetNearEntity(all.Count, (int i) => all[i].Position - pos);
		AICorePoint aICorePoint = all[nearEntity];
		if (smethod_0(pos, aICorePoint.Position))
		{
			return aICorePoint;
		}
		foreach (AICorePoint item in all)
		{
			if (smethod_0(pos, item.Position))
			{
				return item;
			}
		}
		return null;
	}

	public static bool smethod_0(Vector3 f, Vector3 t)
	{
		NavMeshPath navMeshPath = new NavMeshPath();
		NavMeshPath navMeshPath2 = new NavMeshPath();
		if (NavMesh.CalculatePath(f, t, -1, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete && NavMesh.CalculatePath(t, f, -1, navMeshPath2) && navMeshPath2.status == NavMeshPathStatus.PathComplete)
		{
			return true;
		}
		return false;
	}

	public AICorePoint GetCorePoint(int corePointId)
	{
		AICorePoint aICorePoint = CorePoints.FirstOrDefault((AICorePoint x) => x.Id == corePointId);
		_ = aICorePoint == null;
		return aICorePoint;
	}

	public static AICorePoint GetCorePointStatic(int corePointId, bool canUseCache)
	{
		AICorePoint aICorePoint = GetAllTestObjects(canUseCache).FirstOrDefault((AICorePoint x) => x.Id == corePointId);
		if (aICorePoint == null)
		{
			Debug.LogError($"!blocker can't find core point id :{corePointId}");
		}
		return aICorePoint;
	}

	public void AddCorePoint(AICorePoint corePoint)
	{
		corePoint.transform.SetParent(base.transform);
		CorePoints.Add(corePoint);
	}

	public void OnDrawGizmos()
	{
		if (IsAlwaysDraw)
		{
			method_0();
		}
	}

	public void OnDrawGizmosSelected()
	{
		if (!IsAlwaysDraw)
		{
			method_0();
		}
	}

	public void method_0()
	{
		foreach (AICorePoint corePoint in CorePoints)
		{
			if (!(corePoint == null))
			{
				corePoint.Restore(CorePoints);
				corePoint.GizmosDrawWays(DrawHeight);
			}
		}
	}

	public void Refresh()
	{
		aicorePointHolder_0 = null;
	}
}
