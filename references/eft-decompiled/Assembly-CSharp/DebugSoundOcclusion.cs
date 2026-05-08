using System.Collections.Generic;
using UnityEngine;

public class DebugSoundOcclusion : MonoBehaviour
{
	private List<GClass901> list_0;

	public void Awake()
	{
		list_0 = new List<GClass901>();
	}

	public void UpdateDebugInfo(GClass901 debugOcclusionRayInfo)
	{
		list_0.Add(debugOcclusionRayInfo);
	}

	public void RenderLines()
	{
		if (list_0.Count == 0)
		{
			return;
		}
		foreach (GClass901 item in list_0)
		{
			Debug.DrawLine(item.StartPoint, item.EndPoint, item.Color);
		}
		list_0.Clear();
	}
}
