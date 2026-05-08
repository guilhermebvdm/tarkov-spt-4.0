using System.Collections.Generic;
using Comfort.Common;
using UnityEngine;

public class BotEventDebug : MonoBehaviour
{
	public float Power = 10f;

	public float ArtilleryDuration = 10f;

	public bool CanBeUsed = true;

	public GroupPoint Selected;

	private Vector3 vector3_0 = new Vector3(0f, 0.5f);

	private Vector3 vector3_1 = new Vector3(0f, 0.3f);

	public void PlaySound()
	{
		Singleton<BotEventHandler>.Instance?.PlaySound(null, base.transform.position, Power, AISoundType.gun);
	}

	public void ArtilleryStart()
	{
		Singleton<BotEventHandler>.Instance?.ArtilleryStart(base.transform.position, Power, ArtilleryDuration);
	}

	public void OnDrawGizmosSelected()
	{
		Gizmos.DrawWireSphere(base.transform.position, Power);
	}

	public void DoTest()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform item in base.transform)
		{
			list.Add(item);
		}
		if (list.Count >= 2)
		{
			Transform transform = list[0];
			Transform transform2 = list[1];
			Debug.Log("Magnitude is:" + (transform.position - transform2.position).magnitude);
		}
	}

	public void FindClosestsPoint()
	{
		AICoversData aICoversData = AICoversData.CreateOrFind();
		if (aICoversData.RestoreData())
		{
			Selected = aICoversData.GetClosest(base.transform.position);
		}
	}

	public void DrawRecursieveConnects(int maxDeep)
	{
		if (Selected == null)
		{
			Debug.LogError("seelcted null");
			return;
		}
		HashSet<GroupPoint> hashSet = new HashSet<GroupPoint>();
		hashSet.Add(Selected);
		method_0(Selected, hashSet, 0, maxDeep);
	}

	public void method_0(GroupPoint selected, HashSet<GroupPoint> drawed, int deep, int maxDeep)
	{
		if (deep >= maxDeep)
		{
			return;
		}
		foreach (GroupPointWay neighbourhoodsWay in selected.NeighbourhoodsWays)
		{
			GroupPoint target = neighbourhoodsWay.Target;
			if (!drawed.Contains(target))
			{
				drawed.Add(target);
				Debug.DrawLine(selected.Position + vector3_0, target.Position + vector3_1, Color.green, 4f);
				method_0(target, drawed, deep + 1, maxDeep);
			}
		}
	}
}
