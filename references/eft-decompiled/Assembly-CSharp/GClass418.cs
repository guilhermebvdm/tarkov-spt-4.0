using System.Collections.Generic;

public abstract class GClass418
{
	public List<ReachableClass> _allReachables = new List<ReachableClass>();

	public void AddReachable(ReachableClass toAdd)
	{
		_allReachables.Add(toAdd);
	}

	public void DebugDraw()
	{
	}

	public GClass418()
	{
	}
}
