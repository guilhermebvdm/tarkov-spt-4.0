using System;

public class GClass1316 : GClass1315
{
	[NonSerialized]
	public FastAnimatorControllerClass FastAnimatorControllerClass;

	public float LayerWeight;

	public override GClass1313 ParentStateMachine => FastAnimatorControllerClass;

	public GClass1316(string name, int hash, int defaultStateHash)
		: base(name, hash, defaultStateHash)
	{
		LayerWeight = 1f;
	}

	public override string ToString()
	{
		return $"{base.ToString()}, Weight: {LayerWeight}";
	}

	public override void Dispose(bool disposing)
	{
		if (disposing)
		{
			FastAnimatorControllerClass = null;
		}
		base.Dispose(disposing);
	}
}
