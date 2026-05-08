using System.Collections.Generic;
using EFT;
using UnityEngine;

public class DebugCoverSystemTester : MonoBehaviour
{
	public float MaxShootDist = 100f;

	public WildSpawnType WildSpawnType = WildSpawnType.assault;

	public List<Transform> CarePos;

	public Transform ShotPos;

	public bool DrawLast;

	public ESearchCoverEngine SearchToDraw;

	public bool UseLineCastToCover;

	public bool RefreshWayBlockerTester;

	public int StepConstrain;

	public float RadiusConstrain;

	public CoverShootType ShootType = CoverShootType.hide;

	private GClass404 gclass404_0;

	private readonly Dictionary<ESearchCoverEngine, NavGraphFindDebug> dictionary_0 = new Dictionary<ESearchCoverEngine, NavGraphFindDebug>();

	public HashSet<Vector3> CarePositions()
	{
		HashSet<Vector3> hashSet = new HashSet<Vector3>();
		foreach (Transform carePo in CarePos)
		{
			hashSet.Add(carePo.position);
		}
		return hashSet;
	}

	public EnemyInfo GetGoalEnemy()
	{
		return null;
	}

	public void DoFind()
	{
		Debug.LogError("TODO");
	}

	public void PrintLastInfo()
	{
		if (dictionary_0.TryGetValue(SearchToDraw, out var value))
		{
			value.PrintLogs();
		}
	}

	public void method_0(CustomNavigationPoint point, NavGraphFindDebug debug, ESearchCoverEngine coverengine)
	{
		if (dictionary_0.ContainsKey(coverengine))
		{
			dictionary_0[coverengine] = debug;
		}
		else
		{
			dictionary_0.Add(coverengine, debug);
		}
	}

	public void method_1()
	{
		NavGraphVoxelAffectorTester[] array = Object.FindObjectsOfType<NavGraphVoxelAffectorTester>();
		foreach (NavGraphVoxelAffectorTester obj in array)
		{
			obj.Refresh();
			obj.UpdateTest();
		}
	}

	public void method_2()
	{
		if (DrawLast)
		{
		}
	}

	public void OnDrawGizmos()
	{
		method_2();
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(base.transform.position, 0.4f);
	}
}
