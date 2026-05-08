using System.Collections.Generic;
using UnityEngine;

public class SpawnResultsLogParserContainer : MonoBehaviour
{
	public List<SpawnResultDebug> _elements = new List<SpawnResultDebug>();

	public void Clear()
	{
		foreach (SpawnResultDebug element in _elements)
		{
			Object.DestroyImmediate(element.gameObject);
		}
		_elements.Clear();
	}

	public void AddData(LocalGameLoggerClass.GClass1886 data)
	{
		SpawnResultDebug spawnResultDebug = new GameObject($"ProfileId:{data.ProfileId} PlayerId:{data.PlayerId}").AddComponent<SpawnResultDebug>();
		_elements.Add(spawnResultDebug);
		spawnResultDebug.Init(data);
		spawnResultDebug.transform.SetParent(base.transform);
	}
}
