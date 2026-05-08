using System.Collections.Generic;
using UnityEngine;

public class SeasonObjectsRegistry : MonoBehaviourSingleton<SeasonObjectsRegistry>
{
	private readonly List<GameObject> list_0 = new List<GameObject>();

	private bool bool_0 = true;

	public void Add(GameObject go)
	{
		list_0.Add(go);
		if (!bool_0)
		{
			go.SetActive(value: false);
		}
	}

	public void Remove(GameObject go, bool restore = false)
	{
		list_0.Remove(go);
		if (restore && !bool_0)
		{
			go.SetActive(value: true);
		}
	}

	public void Clear(bool restore = false)
	{
		if (restore && !bool_0)
		{
			SetActiveToObjects(isActive: true);
		}
		list_0.Clear();
	}

	public void SetActiveToObjects(bool isActive)
	{
		if (bool_0 != isActive)
		{
			for (int i = 0; i < list_0.Count; i++)
			{
				list_0[i].SetActive(isActive);
			}
			bool_0 = isActive;
		}
	}
}
