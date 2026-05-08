using System;
using System.Collections.Generic;

[Serializable]
public class PathConfig
{
	public string id;

	public string enterPoint;

	public string exitPoint;

	public List<string> pathPoints;

	public bool once;

	public bool flipFlop;

	public int flipCount;

	public bool circle;

	public int circleCount;

	public bool active;

	public List<string> GetAllPathPoints()
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(enterPoint))
		{
			list.Add(enterPoint);
		}
		foreach (string pathPoint in pathPoints)
		{
			list.Add(pathPoint);
		}
		if (!string.IsNullOrEmpty(exitPoint))
		{
			list.Add(exitPoint);
		}
		return list;
	}
}
