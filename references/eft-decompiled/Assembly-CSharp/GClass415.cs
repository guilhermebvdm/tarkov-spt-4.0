using System;
using System.Collections.Generic;
using System.Text;

public class GClass415
{
	[NonSerialized]
	public int Int_0;

	[NonSerialized]
	public int Int_1;

	[NonSerialized]
	public bool Bool_0;

	[NonSerialized]
	public NavGraphEditorPoint NavGraphEditorPoint_0;

	[NonSerialized]
	public HashSet<int> HashSet_0 = new HashSet<int>();

	[NonSerialized]
	public Dictionary<NavGraphEditorPoint, bool> Dictionary_0 = new Dictionary<NavGraphEditorPoint, bool>();

	[NonSerialized]
	public Dictionary<NavGraphEditorPoint, float> Dictionary_1 = new Dictionary<NavGraphEditorPoint, float>();

	[NonSerialized]
	public StringBuilder StringBuilder_0 = new StringBuilder();

	public GClass415(int step, NavGraphEditorPoint currentNode)
	{
		Int_0 = step;
		Bool_0 = currentNode.IsInited;
		NavGraphEditorPoint_0 = currentNode;
		Int_1 = currentNode.ClosestPointsWaysArray.Count;
		for (int i = 0; i < Int_1; i++)
		{
			NavGraphEditorPointWay navGraphEditorPointWay = currentNode.ClosestPointsWaysArray[i];
			StringBuilder_0.Append($" {navGraphEditorPointWay.Id},");
		}
	}

	public void TestNeighbour(int neighbour)
	{
		if (!HashSet_0.Contains(neighbour))
		{
			HashSet_0.Add(neighbour);
		}
	}

	public void TestNeighbourVisited(NavGraphEditorPoint neighbour, bool isVisited)
	{
		if (!Dictionary_0.ContainsKey(neighbour))
		{
			Dictionary_0.Add(neighbour, isVisited);
		}
	}

	public void TestNeighbourHeuristic(NavGraphEditorPoint neighbour, float heuristic)
	{
		if (Dictionary_1.ContainsKey(neighbour))
		{
			Dictionary_1.Add(neighbour, heuristic);
		}
		else
		{
			Dictionary_1[neighbour] = heuristic;
		}
	}

	public string Print()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append($"step:{Int_0}   currentNode:{NavGraphEditorPoint_0.Id} pos:{NavGraphEditorPoint_0.Position} NeighbourhoodCount:{Int_1}  init:{Bool_0}  _idsList:{StringBuilder_0}");
		foreach (int item in HashSet_0)
		{
			stringBuilder.Append($" [{item}],");
		}
		foreach (KeyValuePair<NavGraphEditorPoint, bool> item2 in Dictionary_0)
		{
			stringBuilder.Append($" <{item2.Key.Id}:{item2.Key.SearchWayId}:{item2.Value}> ");
		}
		foreach (KeyValuePair<NavGraphEditorPoint, float> item3 in Dictionary_1)
		{
			stringBuilder.Append($" ({item3.Key.Id}:{item3.Value}) ");
		}
		return stringBuilder.ToString();
	}
}
