using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AIPlaceInfoHolder : MonoBehaviour
{
	[CompilerGenerated]
	public class Class146
	{
		public string searchName;

		public bool method_0(AIPlaceInfo x)
		{
			return x.name == searchName;
		}
	}

	public List<AIPlaceInfo> Places;

	public void AddPlace(AIPlaceInfo aiPlaceInfo)
	{
		Places.Add(aiPlaceInfo);
		aiPlaceInfo.transform.SetParent(base.transform);
	}

	public AIPlaceInfo FindPlace(string searchName)
	{
		return Places.FirstOrDefault((AIPlaceInfo x) => x.name == searchName);
	}
}
