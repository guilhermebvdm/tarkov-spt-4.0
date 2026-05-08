using System;
using System.Collections.Generic;
using System.Text;

public class GClass416
{
	[NonSerialized]
	public Dictionary<int, GClass415> Dictionary_0 = new Dictionary<int, GClass415>();

	[NonSerialized]
	public GClass415 Gclass415_0;

	[NonSerialized]
	public int Int_0;

	public GClass416(int searchId)
	{
		Int_0 = searchId;
	}

	public string Print()
	{
		StringBuilder stringBuilder = new StringBuilder($"searchId:{Int_0} ");
		foreach (KeyValuePair<int, GClass415> item in Dictionary_0)
		{
			stringBuilder.AppendLine(item.Value.Print());
		}
		return stringBuilder.ToString();
	}

	public void Finish()
	{
	}

	public void AddStep(int step, NavGraphEditorPoint currentNode)
	{
		GClass415 gClass = new GClass415(step, currentNode);
		Dictionary_0.Add(step, gClass);
		Gclass415_0 = gClass;
	}

	public void TestNeighbour(int neighbour)
	{
		Gclass415_0.TestNeighbour(neighbour);
	}

	public void TestNeighbourVisited(NavGraphEditorPoint neighbour, bool isVisited)
	{
		Gclass415_0.TestNeighbourVisited(neighbour, isVisited);
	}

	public void TestNeighbourHeuristic(NavGraphEditorPoint neighbour, float heuristic)
	{
		Gclass415_0.TestNeighbourHeuristic(neighbour, heuristic);
	}
}
