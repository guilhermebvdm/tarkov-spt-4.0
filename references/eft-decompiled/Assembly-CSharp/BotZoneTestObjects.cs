using System.Collections.Generic;
using UnityEngine;

public class BotZoneTestObjects : MonoBehaviour
{
	public AICorePoint[] TestObjects;

	public Vector3 Position => base.transform.position;

	public bool RebuildFromZone(BotZone zone)
	{
		List<AICorePoint> list = new List<AICorePoint>();
		List<Transform> list2 = method_0(zone.transform, onlyActive: false);
		list2.Remove(base.transform);
		foreach (Transform item2 in list2)
		{
			item2.transform.SetParent(base.transform, worldPositionStays: true);
		}
		foreach (Transform item3 in list2)
		{
			AICorePoint item = item3.gameObject.AddComponent<AICorePoint>();
			list.Add(item);
		}
		TestObjects = list.ToArray();
		return list2.Count > 0;
	}

	public void OnDrawGizmos()
	{
	}

	public List<Transform> method_0(Transform tr1, bool onlyActive)
	{
		return GClass855.GetChildsName(tr1, "Test", onlyActive);
	}
}
